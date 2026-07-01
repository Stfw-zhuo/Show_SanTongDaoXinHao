using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using ScottPlot.Plottables;

namespace Show_SanTongDaoXinHao
{
    public partial class UserControl_CollectPage : UserControl
    {
        private UdpClient? udpClient;
        private TcpClient? tcpClient;
        private NetworkStream? tcpStream;

        private bool isConnected = false;
        private bool isReceiving = false;
        private int packetCount = 0;

        private volatile bool autoRefresh = true;
        private volatile UnpackMode unpackMode = UnpackMode.W5500;

        private Signal? sig1, sig2, sig3;
        private List<double> plotCH1 = new(), plotCH2 = new(), plotCH3 = new();
        private const int MAX_PLOT_POINTS = 10000;

        private FileStream? rawStream;
        private BinaryWriter? rawWriter;
        private string? rawFilePath;
        private string? saveDirectory;
        private string? saveBaseName;

        private enum UnpackMode { CH395Q, W5500 }

        public UserControl_CollectPage()
        {
            InitializeComponent();
            comboBox_protocol.Items.AddRange(new object[] { "UDP", "TCP_Client" });
            comboBox_protocol.SelectedIndex = 0;
            comboBox_protocol.SelectedIndexChanged += comboBox_protocol_SelectedIndexChanged;
            comboBox_mode.Items.AddRange(new object[] { "CH395Q (旧)", "W5500" });
            comboBox_mode.SelectedIndex = 1;
            comboBox_mode.SelectedIndexChanged += (s, e) =>
                unpackMode = comboBox_mode.SelectedIndex == 1 ? UnpackMode.W5500 : UnpackMode.CH395Q;
            checkBox_autoRefresh.CheckedChanged += (s, e) => autoRefresh = checkBox_autoRefresh.Checked;
            InitializePlot();
            PopulateLocalIPs();
            UpdateStatus("● 空闲");
        }

        private void InitializePlot()
        {
            formsPlot1.Plot.Clear();
            formsPlot1.Plot.Title("三通道波形");
            formsPlot1.Plot.XLabel("Sample");
            formsPlot1.Plot.YLabel("ADC");
            formsPlot1.Refresh();
        }

        private void RefreshPlot()
        {
            if (!this.IsHandleCreated) return;
            if (formsPlot1.InvokeRequired) { formsPlot1.Invoke(new Action(RefreshPlot)); return; }
            formsPlot1.Plot.Clear();
            int cnt = plotCH1.Count;
            if (cnt == 0) { formsPlot1.Refresh(); return; }
            double[] y1 = plotCH1.ToArray();
            double[] y2 = plotCH2.Count == cnt ? plotCH2.ToArray() : new double[cnt];
            double[] y3 = plotCH3.Count == cnt ? plotCH3.ToArray() : new double[cnt];
            sig1 = formsPlot1.Plot.Add.Signal(y1);
            sig2 = formsPlot1.Plot.Add.Signal(y2);
            sig3 = formsPlot1.Plot.Add.Signal(y3);
            sig1.LegendText = "CH1"; sig2.LegendText = "CH2"; sig3.LegendText = "CH3";
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Axes.AutoScale();
            formsPlot1.Refresh();
        }

        private void AppendPlotData(double ch1, double ch2, double ch3)
        {
            if (plotCH1.Count >= MAX_PLOT_POINTS) {
                plotCH1.RemoveRange(0, plotCH1.Count/2);
                plotCH2.RemoveRange(0, plotCH2.Count/2);
                plotCH3.RemoveRange(0, plotCH3.Count/2);
            }
            plotCH1.Add(ch1);
            plotCH2.Add(ch2);
            plotCH3.Add(ch3);
        }

        private int plotDirtyCount = 0;
        private const int PLOT_REFRESH_INTERVAL = 200;
        private void MarkPlotDirty() { if (++plotDirtyCount >= PLOT_REFRESH_INTERVAL && autoRefresh) { plotDirtyCount = 0; RefreshPlot(); } }
        private void ForceRefreshPlot() { plotDirtyCount = 0; RefreshPlot(); }

        private void PopulateLocalIPs()
        {
            comboBox_localIP.Items.Clear();
            var ipList = new List<string> { "0.0.0.0 (所有网卡)" };
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        ipList.Add($"{ip.Address}  ({ni.Name})");
            }
            comboBox_localIP.Items.AddRange(ipList.ToArray());
            comboBox_localIP.SelectedIndex = 0;
        }

        private void comboBox_protocol_SelectedIndexChanged(object? s, EventArgs e)
        {
            bool tcp = comboBox_protocol.SelectedItem?.ToString() == "TCP_Client";
            label_remoteIP.Visible = tcp;
            textBox_remoteIP.Visible = tcp;
        }

        private IPAddress GetLocalIPAddress()
        {
            string sel = comboBox_localIP.SelectedItem?.ToString() ?? "0.0.0.0";
            int idx = sel.IndexOf(' ');
            return IPAddress.Parse(idx > 0 ? sel.Substring(0, idx) : sel);
        }

        private void UpdateStatus(string text)
        {
            if (label_status.InvokeRequired) label_status.Invoke(new Action<string>(UpdateStatus), text);
            else label_status.Text = text;
        }

        private void SetStatusColor(Color c)
        {
            if (label_status.InvokeRequired) label_status.Invoke(new Action<Color>(SetStatusColor), c);
            else label_status.ForeColor = c;
        }

        private void SetStatusIdle() { UpdateStatus("● 空闲"); SetStatusColor(Color.Gray); }
        private void SetStatusReceiving() { UpdateStatus($"● 采集中 — {packetCount}包"); SetStatusColor(Color.Lime); }
        private void SetStatusStopped() { UpdateStatus($"● 已停止 — {packetCount}包"); SetStatusColor(Color.OrangeRed); }
        private void SetStatusError(string msg) { UpdateStatus($"● 错误: {msg}"); SetStatusColor(Color.Red); }

        private void SetRawFileLabel(string? path)
        {
            if (InvokeRequired) { Invoke(new Action<string?>(SetRawFileLabel), path); return; }
            label_rawFile.Text = path != null ? $"原始包：{path}" : "原始包文件：无";
            button_decode.Enabled = path != null;
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            if (isConnected)
            {
                if (isReceiving) { MessageBox.Show("请先停止采集再断开。"); return; }
                Disconnect();
            }
            else
            {
                string protocol = comboBox_protocol.SelectedItem?.ToString() ?? "UDP";
                try { Connect(protocol); isConnected = true; button_connect.Text = "断开"; UpdateStatus("● 已连接"); SetStatusColor(Color.Cyan); }
                catch (Exception ex) { SetStatusError(ex.Message); }
            }
        }

        private void Connect(string protocol)
        {
            if (!int.TryParse(textBox_port.Text.Trim(), out int port)) throw new Exception("端口号格式不正确");
            IPAddress lip = GetLocalIPAddress();
            if (protocol == "TCP_Client")
            {
                tcpClient = new TcpClient(new IPEndPoint(lip, 0));
                tcpClient.Connect(textBox_remoteIP.Text.Trim(), port);
                tcpStream = tcpClient.GetStream();
            }
            else
            {
                udpClient = new UdpClient();
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.Bind(new IPEndPoint(lip, port));
            }
        }

        private void Disconnect()
        {
            try { udpClient?.Client.Shutdown(SocketShutdown.Both); udpClient?.Close(); udpClient?.Dispose(); } catch { }
            try { tcpStream?.Close(); } catch { }
            try { tcpClient?.Close(); } catch { }
            udpClient = null; tcpStream = null; tcpClient = null;
            isConnected = false;
            button_connect.Text = "连接";
            SetStatusIdle();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isReceiving) { MessageBox.Show("已经正在采集！"); return; }
            if (!isConnected) { MessageBox.Show("请先点击\"连接\"。"); return; }

            string baseName = $"Tri-MCD_{DateTime.Now:yyyyMMdd_HHmmss}";
            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "选择保存位置",
                Filter = "原始包文件 (*.bin)|*.bin",
                FileName = baseName + ".bin",
                InitialDirectory = saveDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (dlg.ShowDialog() != DialogResult.OK) { SetStatusIdle(); return; }

            saveDirectory = Path.GetDirectoryName(dlg.FileName) ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            saveBaseName = baseName;
            DataManager.ClearData();
            plotCH1.Clear(); plotCH2.Clear(); plotCH3.Clear();
            packetCount = 0; plotDirtyCount = 0;
            InitializePlot();

            rawFilePath = Path.Combine(saveDirectory, baseName + ".bin");
            rawStream = new FileStream(rawFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            rawWriter = new BinaryWriter(rawStream);
            SetRawFileLabel(rawFilePath);
            isReceiving = true;

            string protocol = comboBox_protocol.SelectedItem?.ToString() ?? "UDP";
            Task.Run(() => { if (protocol == "TCP_Client") ReceiveData_TcpClient(); else ReceiveData_Udp(); });
            SetStatusReceiving();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!isReceiving) { MessageBox.Show("当前没有采集。"); return; }
            isReceiving = false;
            try { udpClient?.Client.Close(); } catch { }
            try { tcpClient?.Close(); } catch { }
            System.Threading.Thread.Sleep(200);
            try { rawWriter?.Close(); } catch { }
            try { rawStream?.Close(); } catch { }
            rawWriter = null; rawStream = null;

            string txtPath = Path.Combine(saveDirectory ?? "", (saveBaseName ?? "data") + ".txt");
            DataManager.SaveToTxt(txtPath);
            MessageBox.Show($"已保存：\n\n  原始包：{rawFilePath}\n  解析 TXT：{txtPath}", "保存完成");
            SetStatusStopped();
        }

        private void button_decode_Click(object sender, EventArgs e)
        {
            if (isReceiving) { MessageBox.Show("请先停止采集。"); return; }
            if (string.IsNullOrEmpty(rawFilePath) || !File.Exists(rawFilePath))
            { MessageBox.Show("没有可解码的原始包文件。"); return; }

            DataManager.ClearData();
            plotCH1.Clear(); plotCH2.Clear(); plotCH3.Clear();
            plotDirtyCount = 0;
            InitializePlot();

            byte[] all = File.ReadAllBytes(rawFilePath);
            int pos = 0, total = 0;
            UnpackMode mode = (UnpackMode)Environment.TickCount; mode = unpackMode;

            if (mode == UnpackMode.CH395Q)
            {
                while (pos + 1440 <= all.Length)
                {
                    byte[] pkt = new byte[1440];
                    Array.Copy(all, pos, pkt, 0, 1440);
                    pos += 1440;
                    DataManager.AppendPacket(pkt);
                    var c1 = DataManager.GetChannel1(); var c2 = DataManager.GetChannel2(); var c3 = DataManager.GetChannel3();
                    for (int i = Math.Max(0, c1.Count - 240); i < c1.Count; i++) AppendPlotData(c1[i], c2[i], c3[i]);
                    total++;
                }
            }
            else
            {
                while (pos + 6 <= all.Length)
                {
                    int sc = all[pos + 3];
                    if (sc < 10 || sc > 200) sc = Math.Min(200, (all.Length - pos - 6) / 6);
                    int plen = 6 + sc * 6;
                    if (pos + plen > all.Length) break;
                    byte[] pkt = new byte[plen];
                    Array.Copy(all, pos, pkt, 0, plen);
                    pos += plen;
                    DataManager.AppendW5500Packet(pkt);
                    total++;
                }
            }

            DataManager.FlushAllPending();
            var a1 = DataManager.GetChannel1(); var a2 = DataManager.GetChannel2(); var a3 = DataManager.GetChannel3();
            plotCH1.Clear(); plotCH2.Clear(); plotCH3.Clear();
            for (int i = 0; i < a1.Count; i++) AppendPlotData(a1[i], a2[i], a3[i]);
            ForceRefreshPlot();
            SetStatusColor(Color.Lime);
            MessageBox.Show($"解码完成：{total} 包，{a1.Count} 点。");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (isReceiving) { MessageBox.Show("请先停止采集。"); return; }
            OpenFileDialog d = new OpenFileDialog { Title = "读取采集数据", Filter = "文本文件 (*.txt)|*.txt" };
            if (d.ShowDialog() == DialogResult.OK)
            {
                DataManager.LoadFromTxt(d.FileName);
                var c1 = DataManager.GetChannel1(); var c2 = DataManager.GetChannel2(); var c3 = DataManager.GetChannel3();
                plotCH1.Clear(); plotCH2.Clear(); plotCH3.Clear();
                for (int i = 0; i < c1.Count; i++) AppendPlotData(c1[i], c2[i], c3[i]);
                ForceRefreshPlot();
                MessageBox.Show("读取完成！");
            }
        }

        private void ReceiveData_Udp()
        {
            try
            {
                while (isReceiving)
                {
                    IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                    byte[] packet = udpClient!.Receive(ref remote);
                    packetCount++;
                    rawWriter?.Write(packet);
                    if (packetCount % 50 == 0) SetStatusReceiving();
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex) { if (isReceiving) SetStatusError($"{ex.GetType().Name}: {ex.Message}"); }
            finally { isReceiving = false; SetStatusStopped(); }
        }

        private void ReceiveData_TcpClient()
        {
            try
            {
                byte[] buf = new byte[2000];
                while (isReceiving)
                {
                    int read = tcpStream!.Read(buf, 0, buf.Length);
                    if (read <= 0) { isReceiving = false; return; }
                    byte[] pkt = new byte[read];
                    Array.Copy(buf, pkt, read);
                    packetCount++;
                    rawWriter?.Write(pkt);
                    if (packetCount % 50 == 0) SetStatusReceiving();
                }
            }
            catch (ObjectDisposedException) { }
            catch (System.IO.IOException) { }
            catch (Exception ex) { if (isReceiving) SetStatusError(ex.Message); }
            finally { isReceiving = false; SetStatusStopped(); }
        }
    }
}