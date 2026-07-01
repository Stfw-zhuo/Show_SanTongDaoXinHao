using System;
using System.Collections.Generic;
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

        // 线程安全缓存（从后台线程读取，避免跨线程访问控件）
        private volatile bool autoRefresh = true;
        private volatile UnpackMode unpackMode = UnpackMode.CH395Q;

        private Signal? sig1;
        private Signal? sig2;
        private Signal? sig3;

        private List<double> plotCH1 = new();
        private List<double> plotCH2 = new();
        private List<double> plotCH3 = new();

        private const int MAX_PLOT_POINTS = 10000;

        private enum UnpackMode { CH395Q, W5500 }

        public UserControl_CollectPage()
        {
            InitializeComponent();

            // 协议
            comboBox_protocol.Items.AddRange(new object[] { "UDP", "TCP_Client" });
            comboBox_protocol.SelectedIndex = 0;
            comboBox_protocol.SelectedIndexChanged += comboBox_protocol_SelectedIndexChanged;

            // 解包模式
            comboBox_mode.Items.AddRange(new object[] { "CH395Q (旧)", "W5500" });
            comboBox_mode.SelectedIndex = 1;  // 默认 W5500 模式
            unpackMode = UnpackMode.W5500;
            comboBox_mode.SelectedIndexChanged += (s, e) =>
                unpackMode = comboBox_mode.SelectedIndex == 1 ? UnpackMode.W5500 : UnpackMode.CH395Q;
            checkBox_autoRefresh.CheckedChanged += (s, e) =>
                autoRefresh = checkBox_autoRefresh.Checked;

            // 初始化图表
            InitializePlot();

            PopulateLocalIPs();
            UpdateStatus("● 空闲");
        }

        //---------------------------------------------------
        // ScottPlot 初始化
        //---------------------------------------------------
        private void InitializePlot()
        {
            formsPlot1.Plot.Clear();
            formsPlot1.Plot.Title("三通道实时波形");
            formsPlot1.Plot.XLabel("Sample");
            formsPlot1.Plot.YLabel("ADC");
            formsPlot1.Refresh();
        }

        //---------------------------------------------------
        // 实时刷新图表 (线程安全)
        //---------------------------------------------------
        private void RefreshPlot()
        {
            if (!this.IsHandleCreated) return;

            if (formsPlot1.InvokeRequired)
            {
                formsPlot1.Invoke(new Action(RefreshPlot));
                return;
            }

            formsPlot1.Plot.Clear();

            int cnt = plotCH1.Count;
            if (cnt == 0)
            {
                formsPlot1.Refresh();
                return;
            }

            double[] y1 = plotCH1.ToArray();
            double[] y2 = (plotCH2.Count == cnt) ? plotCH2.ToArray() : new double[cnt];
            double[] y3 = (plotCH3.Count == cnt) ? plotCH3.ToArray() : new double[cnt];

            sig1 = formsPlot1.Plot.Add.Signal(y1);
            sig2 = formsPlot1.Plot.Add.Signal(y2);
            sig3 = formsPlot1.Plot.Add.Signal(y3);

            sig1.LegendText = "CH1";
            sig2.LegendText = "CH2";
            sig3.LegendText = "CH3";

            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Axes.AutoScale();
            formsPlot1.Refresh();
        }

        //---------------------------------------------------
        // 追加数据点并更新图表
        //---------------------------------------------------
        private void AppendPlotData(double ch1, double ch2, double ch3)
        {
            if (plotCH1.Count >= MAX_PLOT_POINTS)
            {
                plotCH1.RemoveRange(0, plotCH1.Count / 2);
                plotCH2.RemoveRange(0, plotCH2.Count / 2);
                plotCH3.RemoveRange(0, plotCH3.Count / 2);
            }

            plotCH1.Add(ch1);
            plotCH2.Add(ch2);
            plotCH3.Add(ch3);
        }

        private int plotDirtyCount = 0;
        private const int PLOT_REFRESH_INTERVAL = 200;

        private void MarkPlotDirty()
        {
            plotDirtyCount++;
            if (plotDirtyCount >= PLOT_REFRESH_INTERVAL && autoRefresh)
            {
                plotDirtyCount = 0;
                RefreshPlot();
            }
        }

        private void ForceRefreshPlot()
        {
            plotDirtyCount = 0;
            RefreshPlot();
        }

        //---------------------------------------------------
        // 枚举本机所有 IPv4 地址
        //---------------------------------------------------
        private void PopulateLocalIPs()
        {
            comboBox_localIP.Items.Clear();

            List<string> ipList = new List<string> { "0.0.0.0 (所有网卡)" };

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        ipList.Add($"{ip.Address}  ({ni.Name})");
                }
            }

            comboBox_localIP.Items.AddRange(ipList.ToArray());
            comboBox_localIP.SelectedIndex = 0;
        }

        //---------------------------------------------------
        // 协议切换
        //---------------------------------------------------
        private void comboBox_protocol_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool isTcp = comboBox_protocol.SelectedItem?.ToString() == "TCP_Client";
            label_remoteIP.Visible = isTcp;
            textBox_remoteIP.Visible = isTcp;
        }

        private IPAddress GetLocalIPAddress()
        {
            string selected = comboBox_localIP.SelectedItem?.ToString() ?? "0.0.0.0";
            int spaceIdx = selected.IndexOf(' ');
            string ipStr = spaceIdx > 0 ? selected.Substring(0, spaceIdx) : selected;
            return IPAddress.Parse(ipStr);
        }

        private UnpackMode GetUnpackMode()
        {
            return unpackMode;
        }

        //---------------------------------------------------
        // 状态显示
        //---------------------------------------------------
        private void UpdateStatus(string text)
        {
            if (label_status.InvokeRequired)
                label_status.Invoke(new Action<string>(UpdateStatus), text);
            else
                label_status.Text = text;
        }

        private void SetStatusColor(Color color)
        {
            if (label_status.InvokeRequired)
                label_status.Invoke(new Action<Color>(SetStatusColor), color);
            else
                label_status.ForeColor = color;
        }

        private void SetStatusIdle()
        {
            UpdateStatus("● 空闲");
            SetStatusColor(Color.Gray);
        }
        private int lastPacketLen = 0;
        private int lastSampleAdded = 0;
        private string? lastException = null;
        private int dumpedCount = 0;

        private void SetStatusReceiving()
        {
            string exMsg = lastException != null ? $" E:{lastException}" : "";
            UpdateStatus($"● 采集中 — {packetCount}包({lastPacketLen}B, +{lastSampleAdded}点){exMsg} / D:{plotCH1.Count}");
            SetStatusColor(Color.Lime);
        }
        private void SetStatusStopped()
        {
            UpdateStatus($"● 已停止 — {packetCount}包 / {plotCH1.Count}点");
            SetStatusColor(Color.OrangeRed);
        }
        private void SetStatusError(string msg)
        {
            UpdateStatus($"● 错误: {msg}");
            SetStatusColor(Color.Red);
        }

        //---------------------------------------------------
        // 连接 / 断开
        //---------------------------------------------------
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
                try
                {
                    Connect(protocol);
                    isConnected = true;
                    button_connect.Text = "断开";
                    UpdateStatus("● 已连接");
                    SetStatusColor(Color.Cyan);
                }
                catch (Exception ex) { SetStatusError(ex.Message); }
            }
        }

        private void Connect(string protocol)
        {
            string portText = textBox_port.Text.Trim();
            if (!int.TryParse(portText, out int port)) throw new Exception("端口号格式不正确");

            IPAddress localIP = GetLocalIPAddress();

            if (protocol == "TCP_Client")
            {
                string remoteIP = textBox_remoteIP.Text.Trim();
                tcpClient = new TcpClient(new IPEndPoint(localIP, 0));
                tcpClient.Connect(remoteIP, port);
                tcpStream = tcpClient.GetStream();
            }
            else
            {
                udpClient = new UdpClient();
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.Client.Bind(new IPEndPoint(localIP, port));
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

        //---------------------------------------------------
        // 开始采集
        //---------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            if (isReceiving) { MessageBox.Show("已经正在采集！"); return; }
            if (!isConnected) { MessageBox.Show("请先点击\"连接\"。"); return; }

            DataManager.ClearData();
            plotCH1.Clear(); plotCH2.Clear(); plotCH3.Clear();
            packetCount = 0; plotDirtyCount = 0;
            InitializePlot();

            isReceiving = true;
            string protocol = comboBox_protocol.SelectedItem?.ToString() ?? "UDP";
            Task.Run(() =>
            {
                if (protocol == "TCP_Client") ReceiveData_TcpClient();
                else ReceiveData_Udp();
            });

            SetStatusReceiving();
        }

        //---------------------------------------------------
        // 停止采集并保存
        //---------------------------------------------------
        private void button2_Click(object sender, EventArgs e)
        {
            if (!isReceiving) { MessageBox.Show("当前没有采集。"); return; }
            isReceiving = false;

            // 等待约 200ms 让接收线程自然结束
            System.Threading.Thread.Sleep(200);

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "保存采集数据",
                Filter = "文本文件 (*.txt)|*.txt",
                FileName = $"Tri-MCD_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DataManager.SaveToTxt(dialog.FileName);
                MessageBox.Show("保存完成！");
            }

            ForceRefreshPlot();
            SetStatusStopped();
        }

        //---------------------------------------------------
        // 读取TXT
        //---------------------------------------------------
        private void button3_Click(object sender, EventArgs e)
        {
            if (isReceiving) { MessageBox.Show("请先停止采集。"); return; }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "读取采集数据",
                Filter = "文本文件 (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DataManager.LoadFromTxt(dialog.FileName);

                var ch1 = DataManager.GetChannel1();
                var ch2 = DataManager.GetChannel2();
                var ch3 = DataManager.GetChannel3();

                plotCH1.Clear(); plotCH2.Clear(); plotCH3.Clear();
                for (int i = 0; i < ch1.Count; i++)
                {
                    plotCH1.Add(ch1[i]);
                    plotCH2.Add(ch2[i]);
                    plotCH3.Add(ch3[i]);
                }
                ForceRefreshPlot();
                MessageBox.Show("读取完成！");
            }
        }

        //---------------------------------------------------
        // 处理接收到的数据包
        //---------------------------------------------------
        private void DumpPacket(byte[] packet)
        {
            try
            {
                string hex = BitConverter.ToString(packet, 0, Math.Min(128, packet.Length));
                string msg = $"PACKET#{packetCount} Len={packet.Length} [{hex}]";
                System.Diagnostics.Debug.WriteLine(msg);
                // 保存到用户桌面方便查看
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string file = System.IO.Path.Combine(desktop,
                    $"pkt_dump_{DateTime.Now:yyyyMMdd_HHmmss}_{packetCount}.bin");
                System.IO.File.WriteAllBytes(file, packet);
            }
            catch { /* 忽略dump异常 */ }
        }

        private void ProcessPacket(byte[] packet)
        {
            try
            {
                UnpackMode mode = GetUnpackMode();

                if (mode == UnpackMode.CH395Q)
                {
                    if (packet.Length >= 6)
                    {
                        // DataManager.AppendPacket 会验证 1440 字节，不符合则跳过
                        DataManager.AppendPacket(packet);

                        int sampleCount = packet.Length / 6;
                        int added = 0;
                        for (int i = 0; i < sampleCount; i++)
                        {
                            int pos = i * 6;
                            ushort ch1 = BitConverter.ToUInt16(packet, pos);
                            ushort ch2 = BitConverter.ToUInt16(packet, pos + 2);
                            ushort ch3 = BitConverter.ToUInt16(packet, pos + 4);
                            AppendPlotData(ch1, ch2, ch3);
                            DataManager.AppendData(ch1, ch2, ch3);
                            added++;
                        }
                        // 诊断：每次收到包都更新状态显示最新数据量
                        lastSampleAdded = added;
                    }
                }
                else
                {
                    if (packet.Length >= 12 && packet.Length <= 2000)
                    {
                        bool frameDone = DataManager.AppendW5500Packet(packet);
                        if (frameDone)
                        {
                            var ch1 = DataManager.GetChannel1();
                            var ch2 = DataManager.GetChannel2();
                            var ch3 = DataManager.GetChannel3();
                            for (int i = Math.Max(0, ch1.Count - 1024); i < ch1.Count; i++)
                            {
                                AppendPlotData(ch1[i], ch2[i], ch3[i]);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lastException = ex.Message;
            }
        }

        //---------------------------------------------------
        // UDP 接收
        //---------------------------------------------------
        private void ReceiveData_Udp()
        {
            try
            {
                while (isReceiving)
                {
                    IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                    byte[] packet = udpClient!.Receive(ref remote);
                    lastPacketLen = packet.Length;
                    packetCount++;

                    if (dumpedCount < 3)
                    {
                        DumpPacket(packet);
                        dumpedCount++;
                    }

                    ProcessPacket(packet);

                    if (packetCount % 50 == 0)
                        SetStatusReceiving();
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                if (isReceiving) SetStatusError($"{ex.GetType().Name}: {ex.Message}");
            }
            finally
            {
                isReceiving = false;
                SetStatusStopped();
            }
        }

        //---------------------------------------------------
        // TCP Client 接收
        //---------------------------------------------------
        private void ReceiveData_TcpClient()
        {
            try
            {
                byte[] buffer = new byte[1440];
                while (isReceiving)
                {
                    int offset = 0;
                    while (offset < 1440 && isReceiving)
                    {
                        int read = tcpStream!.Read(buffer, offset, 1440 - offset);
                        if (read <= 0) { isReceiving = false; return; }
                        offset += read;
                    }
                    if (!isReceiving) return;

                    lastPacketLen = buffer.Length;
                    packetCount++;
                    ProcessPacket(buffer);

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