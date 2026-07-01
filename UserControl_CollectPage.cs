using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private bool isConnected, isReceiving;
        private int packetCount;
        private long totalBytes = 0;
        private long lastCheckBytes = 0;
        private DateTime lastCheckTime = DateTime.Now;
        private double currentSpeed = 0;
        private volatile bool autoRefresh = true;
        private volatile UnpackMode unpackMode = UnpackMode.W5500;
        private Signal? sig1, sig2, sig3;
        private List<double> plotCH1 = new(), plotCH2 = new(), plotCH3 = new();
        private const int MAX_PLOT_POINTS = 10000;
        private FileStream? rawStream;
        private BinaryWriter? rawWriter;
        private string? rawFilePath, saveDirectory, saveBaseName;
        private FormFFT? fftWindow;
        private ProgressBar? fftProgressBar;
        private int lastAnalysisCount, lastFFTCount;
        private double[] lastFFTFreqs = new double[3];
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
            CreateAnalysisLabels();
            CreateTrackBars();
            InitializePlot();
            PopulateLocalIPs();
            UpdateStatus("● 空闲");
        }

        // 分析标签（右侧）
        private Label label_analysis = null!;
        private void CreateAnalysisLabels()
        {
            label_analysis = new Label
            {
                AutoSize = false,
                Size = new Size(400, 200),
                Font = new Font("Consolas", 9F),
                ForeColor = Color.Cyan,
                BackColor = Color.FromArgb(32, 32, 48),
                Location = new Point(740, 340),
                Text = "等待解码分析..."
            };
            Controls.Add(label_analysis);
        }

        // 示波器滑条
        private TrackBar trackBarY = null!;
        private TrackBar trackBarX = null!;
        private Label labelYScale = null!;
        private Label labelXScale = null!;

        private void CreateTrackBars()
        {
            labelYScale = new Label
            {
                Text = "Y: ±1.0V", AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = Color.White,
                Location = new Point(16, 690)
            };
            trackBarY = new TrackBar
            {
                Minimum = 5, Maximum = 50, Value = 10, TickFrequency = 5,
                Width = 200, Height = 30,
                Location = new Point(16, 710),
                BackColor = Color.FromArgb(46, 51, 73)
            };
            trackBarY.ValueChanged += (s, e) =>
            {
                double volts = trackBarY.Value * 0.1;
                labelYScale.Text = $"Y: ±{volts:F1}V";
                ApplyTrackBars();
            };

            labelXScale = new Label
            {
                Text = "X: 200 pts", AutoSize = true,
                Font = new Font("Microsoft YaHei UI", 9F),
                ForeColor = Color.White,
                Location = new Point(240, 690)
            };
            trackBarX = new TrackBar
            {
                Minimum = 5, Maximum = 500, Value = 20, TickFrequency = 25,
                Width = 400, Height = 30,
                Location = new Point(240, 710),
                BackColor = Color.FromArgb(46, 51, 73)
            };
            trackBarX.ValueChanged += (s, e) =>
            {
                int pts = trackBarX.Value * 10;
                labelXScale.Text = $"X: {pts} pts";
                ApplyTrackBars();
            };

            Controls.Add(labelYScale);
            Controls.Add(trackBarY);
            Controls.Add(labelXScale);
            Controls.Add(trackBarX);
        }

        private void ApplyTrackBars()
        {
            double yRange = trackBarY.Value * 0.1;
            int xRange = trackBarX.Value * 10;
            formsPlot1.Plot.Axes.SetLimitsX(-xRange * 0.05, xRange * 1.05);
            formsPlot1.Plot.Axes.SetLimitsY(-yRange, yRange);
            formsPlot1.Refresh();
        }

        private void InitializePlot()
        {
            formsPlot1.Plot.Clear();
            formsPlot1.Plot.Title("三通道波形");
            formsPlot1.Plot.XLabel("Sample");
            formsPlot1.Plot.YLabel("Voltage (V)");
            ApplyTrackBars();
        }

        private void RefreshPlot()
        {
            if (!this.IsHandleCreated) return;

            // 异步更新：不阻塞接收线程
            if (formsPlot1.InvokeRequired)
            {
                formsPlot1.BeginInvoke(new Action(RefreshPlot));
                return;
            }

            int cnt = plotCH1.Count;
            if (cnt == 0) { ApplyTrackBars(); return; }

            double[] y1 = plotCH1.ToArray();
            double[] y2 = plotCH2.Count == cnt ? plotCH2.ToArray() : new double[cnt];
            double[] y3 = plotCH3.Count == cnt ? plotCH3.ToArray() : new double[cnt];

            // 更新：Clear + 重建（ScottPlot5 Signal 不支持直接赋值 Data）
            formsPlot1.Plot.Clear();
            sig1 = formsPlot1.Plot.Add.Signal(y1);
            sig2 = formsPlot1.Plot.Add.Signal(y2);
            sig3 = formsPlot1.Plot.Add.Signal(y3);
            sig1.LegendText = "CH1"; sig2.LegendText = "CH2"; sig3.LegendText = "CH3";
            formsPlot1.Plot.ShowLegend();

            ApplyTrackBars();
        }

        private void AppendPlotData(double ch1, double ch2, double ch3)
        {
            if (plotCH1.Count >= MAX_PLOT_POINTS) {
                plotCH1.RemoveRange(0, plotCH1.Count/2);
                plotCH2.RemoveRange(0, plotCH2.Count/2);
                plotCH3.RemoveRange(0, plotCH3.Count/2);
            }
            plotCH1.Add(ch1 * 3.0 / 4095.0);
            plotCH2.Add(ch2 * 3.0 / 4095.0);
            plotCH3.Add(ch3 * 3.0 / 4095.0);
        }

        private int plotDirtyCount;
        private const int PLOT_REFRESH_INTERVAL = 500;
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
        private void SetStatusReceiving()
        {
            var now = DateTime.Now;
            var dt = (now - lastCheckTime).TotalSeconds;
            if (dt >= 1.0)
            {
                currentSpeed = (totalBytes - lastCheckBytes) / dt;
                lastCheckBytes = totalBytes;
                lastCheckTime = now;
            }
            string speed = currentSpeed > 1e6 ? $"{currentSpeed / 1e6:F2} MB/s"
                         : currentSpeed > 1e3 ? $"{currentSpeed / 1e3:F2} KB/s"
                         : $"{currentSpeed:F0} B/s";
            UpdateStatus($"● 采集中 — {packetCount}包 | {speed}");
            SetStatusColor(Color.Lime);
        }

        private void SetStatusStopped()
        {
            string total = totalBytes > 1e6 ? $"{totalBytes / 1e6:F2} MB"
                         : totalBytes > 1e3 ? $"{totalBytes / 1e3:F2} KB"
                         : $"{totalBytes:F0} B";
            UpdateStatus($"● 已停止 — {packetCount}包 | 总流量 {total}");
            SetStatusColor(Color.OrangeRed);
        }
        private void SetStatusError(string msg) { UpdateStatus($"● 错误: {msg}"); SetStatusColor(Color.Red); }

        private void SetRawFileLabel(string? path)
        {
            if (InvokeRequired) { Invoke(new Action<string?>(SetRawFileLabel), path); return; }
            label_rawFile.Text = path != null ? $"原始包：{path}" : "原始包文件：无";
            button_decode.Enabled = path != null;
        }

        private void button_connect_Click(object sender, EventArgs e)
        {
            if (isConnected) { if (isReceiving) { MessageBox.Show("请先停止采集再断开。"); return; } Disconnect(); }
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

            string protocol = comboBox_protocol.SelectedItem?.ToString() ?? "UDP";

            if (!isConnected)
            {
                Disconnect();
                try { Connect(protocol); isConnected = true; button_connect.Text = "断开"; UpdateStatus("● 已连接"); SetStatusColor(Color.Cyan); }
                catch (Exception ex) { SetStatusError(ex.Message); return; }
            }

            string baseName = $"Tri-MCD_{DateTime.Now:yyyyMMdd_HHmmss}";
            string logDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".log");
            Directory.CreateDirectory(logDir);
            saveDirectory = logDir;
            saveBaseName = baseName;

            DataManager.ClearData();
            plotCH1.Clear(); plotCH2.Clear(); plotCH3.Clear();
            totalBytes = 0; lastCheckBytes = 0; lastCheckTime = DateTime.Now; currentSpeed = 0;
            packetCount = 0; plotDirtyCount = 0;
            InitializePlot();

            rawFilePath = Path.Combine(saveDirectory, baseName + ".bin");
            rawStream = new FileStream(rawFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            rawWriter = new BinaryWriter(rawStream);
            SetRawFileLabel(rawFilePath);
            isReceiving = true;

            Task.Run(() => { if (protocol == "TCP_Client") ReceiveData_TcpClient(); else ReceiveData_Udp(); });
            SetStatusReceiving();
        }

        private int DecodeBinFile(string binPath, bool updatePlot)
        {
            if (!File.Exists(binPath)) return 0;
            byte[] all = File.ReadAllBytes(binPath);
            int pos = 0, total = 0;
            int firstLen = BitConverter.ToInt32(all, 0);
            if (firstLen == 1026 || firstLen == 1038 || firstLen == 1440) pos = 4;

            bool isW5500 = false;
            if (pos + 6 <= all.Length)
            {
                byte flags = all[pos + 2];
                byte half = (byte)((flags >> 6) & 0x03);
                byte seg  = (byte)((flags >> 4) & 0x03);
                isW5500 = (half <= 1) && (seg <= 2) && (all[pos + 3] >= 140 && all[pos + 3] <= 180);
            }

            if (isW5500)
            {
                while (pos + 6 <= all.Length)
                {
                    byte flags = all[pos + 2];
                    byte half = (byte)((flags >> 6) & 0x03);
                    byte seg  = (byte)((flags >> 4) & 0x03);
                    int sc = all[pos + 3];
                    if (half > 1 || seg > 2) { pos++; continue; }
                    if (sc < 10 || sc > 200) sc = Math.Min(200, (all.Length - pos - 6) / 6);
                    int plen = 6 + sc * 6;
                    if (pos + plen > all.Length) break;
                    byte[] pkt = new byte[plen];
                    Array.Copy(all, pos, pkt, 0, plen);
                    pos += plen;
                    DataManager.AppendW5500PacketDirect(pkt);
                    total++;
                }
            }
            else if (unpackMode == UnpackMode.CH395Q && all.Length - pos >= 1440)
            {
                while (pos + 1440 <= all.Length)
                {
                    byte[] pkt = new byte[1440];
                    Array.Copy(all, pos, pkt, 0, 1440);
                    pos += 1440;
                    DataManager.AppendPacket(pkt);
                    total++;
                }
            }
            else
            {
                while (pos + 6 <= all.Length)
                {
                    short ch1 = (short)BitConverter.ToUInt16(all, pos);
                    short ch2 = (short)BitConverter.ToUInt16(all, pos + 2);
                    short ch3 = (short)BitConverter.ToUInt16(all, pos + 4);
                    pos += 6;
                    DataManager.AppendData(ch1, ch2, ch3);
                    total++;
                }
            }

            if (updatePlot)
            {
                var a1 = DataManager.GetChannel1(); var a2 = DataManager.GetChannel2(); var a3 = DataManager.GetChannel3();
                plotCH1.Clear(); plotCH2.Clear(); plotCH3.Clear();
                for (int i = 0; i < a1.Count; i++) AppendPlotData(a1[i], a2[i], a3[i]);
                ForceRefreshPlot();
            }
            return total;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!isReceiving) { MessageBox.Show("当前没有采集。"); return; }
            isReceiving = false;

            // 温柔关闭连接并唤醒接收线程
            try { udpClient?.Client.Shutdown(SocketShutdown.Both); udpClient?.Client.Close(); } catch { }
            try { tcpClient?.Client.Close(); } catch { }
            System.Threading.Thread.Sleep(200);
            try { rawWriter?.Close(); } catch { }
            try { rawStream?.Close(); } catch { }

            // 断开连接
            try { udpClient?.Dispose(); } catch { }
            try { tcpClient?.Dispose(); } catch { }
            udpClient = null; tcpClient = null; tcpStream = null;
            rawWriter = null; rawStream = null;
            isConnected = false;
            button_connect.Text = "连接";
            SetStatusColor(Color.Gray);
            UpdateStatus("● 已停止");
            MessageBox.Show($"原始包已保存：\n\n  {rawFilePath}\n\n可点击「解码并绘图」解析。", "保存完成");
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

            int total = DecodeBinFile(rawFilePath, true);
            if (total > 0)
            {
                var c1 = DataManager.GetChannel1(); var c2 = DataManager.GetChannel2(); var c3 = DataManager.GetChannel3();
                int cnt = c1.Count;
                if (cnt > 0)
                {
                    double[] v1 = c1.Select(x => (double)x * 3.0 / 4095.0).ToArray();
                    double[] v2 = c2.Select(x => (double)x * 3.0 / 4095.0).ToArray();
                    double[] v3 = c3.Select(x => (double)x * 3.0 / 4095.0).ToArray();

                    try { fftWindow?.Close(); } catch { }

                    // 显示进度条
                    fftProgressBar ??= new ProgressBar
                    {
                        Location = new Point(740, 550),
                        Size = new Size(400, 25),
                        Minimum = 0, Maximum = 3, Value = 0,
                        Visible = true
                    };
                    Controls.Add(fftProgressBar);
                    fftProgressBar.BringToFront();
                    fftProgressBar.Visible = true;

                    UpdateStatus("● FFT 计算中 — 0/3");
                    var c1Arr = c1.ToArray();
                    var c2Arr = c2.ToArray();
                    var c3Arr = c3.ToArray();

                    var fft = new FormFFT(c1Arr, c2Arr, c3Arr, progress =>
                    {
                        if (fftProgressBar != null)
                        {
                            if (progress == "1/3") fftProgressBar.Value = 1;
                            else if (progress == "2/3") fftProgressBar.Value = 2;
                            else if (progress == "3/3") fftProgressBar.Value = 3;
                        }
                        UpdateStatus($"● FFT 计算中 — {progress}");
                        Application.DoEvents(); // 强制 UI 刷新，避免进度条卡顿
                    });

                    fftProgressBar!.Visible = false;
                    Controls.Remove(fftProgressBar);
                    fft.Show();
                    fftWindow = fft;
                    double f1 = fft.Freqs[0];
                    double f2 = fft.Freqs[1];
                    double f3 = fft.Freqs[2];

                    string stats = $"CH1 | Mean={v1.Average():F4} Max={v1.Max():F4} Min={v1.Min():F4} PP={(v1.Max()-v1.Min()):F4} RMS={Math.Sqrt(v1.Select(x=>x*x).Average()):F4} Freq={f1:F1}Hz\n"
                                 + $"CH2 | Mean={v2.Average():F4} Max={v2.Max():F4} Min={v2.Min():F4} PP={(v2.Max()-v2.Min()):F4} RMS={Math.Sqrt(v2.Select(x=>x*x).Average()):F4} Freq={f2:F1}Hz\n"
                                 + $"CH3 | Mean={v3.Average():F4} Max={v3.Max():F4} Min={v3.Min():F4} PP={(v3.Max()-v3.Min()):F4} RMS={Math.Sqrt(v3.Select(x=>x*x).Average()):F4} Freq={f3:F1}Hz";

                    label_analysis.Text = stats;
                    MessageBox.Show($"解码完成：{total} 包，{cnt} 点\n\n{stats}", "分析结果");
                    UpdateStatus($"● 分析完成 — {cnt} 点");

                    string modeName = unpackMode == UnpackMode.W5500 ? "W5500" : "CH395Q";
                    string txtPath = Path.Combine(saveDirectory ?? "", $"{saveBaseName ?? "data"}_{modeName}.txt");
                    DataManager.SaveToTxt(txtPath);
                }
            }
            else MessageBox.Show("解码失败，未识别到有效数据包。", "解码失败");
            SetStatusColor(Color.Lime);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (isReceiving) { MessageBox.Show("请先停止采集。"); return; }
            OpenFileDialog d = new OpenFileDialog
            {
                Title = "读取采集数据",
                Filter = "数据文件 (*.bin;*.txt)|*.bin;*.txt|原始包 (*.bin)|*.bin|文本文件 (*.txt)|*.txt"
            };
            if (d.ShowDialog() == DialogResult.OK)
            {
                DataManager.ClearData();
                plotCH1.Clear(); plotCH2.Clear(); plotCH3.Clear();
                plotDirtyCount = 0;
                InitializePlot();
                string ext = Path.GetExtension(d.FileName).ToLower();

                if (ext == ".txt")
                {
                    DataManager.LoadFromTxt(d.FileName);
                    var c1 = DataManager.GetChannel1(); var c2 = DataManager.GetChannel2(); var c3 = DataManager.GetChannel3();
                    for (int i = 0; i < c1.Count; i++) AppendPlotData(c1[i], c2[i], c3[i]);
                    ForceRefreshPlot();
                    MessageBox.Show($"读取完成：{c1.Count} 点", "导入TXT");
                }
                else
                {
                    int total = DecodeBinFile(d.FileName, true);
                    MessageBox.Show(total > 0 ? $"解码完成：{total} 包，{DataManager.GetDataCount()} 点" : "解码失败，未识别到有效数据包。", "导入BIN");
                }
                saveDirectory = Path.GetDirectoryName(d.FileName);
                saveBaseName = Path.GetFileNameWithoutExtension(d.FileName);
                rawFilePath = d.FileName;
                SetRawFileLabel(rawFilePath);
            }
        }

        private double EstimateFrequency(double[] data)
        {
            if (data.Length < 10) return 0;
            double mean = data.Average();
            int zeroC = 0;
            for (int i = 1; i < data.Length; i++)
                if (data[i - 1] < mean && data[i] >= mean) zeroC++;
            return zeroC * 200e3 / 1024.0 / 2.0;
        }

        private void ReceiveData_Udp()
        {
            try
            {
                while (isReceiving)
                {
                    IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                    byte[] packet = udpClient!.Receive(ref remote);
                    totalBytes += packet.Length;
                    packetCount++;
                    rawWriter?.Write(packet);
                    ProcessPacketRealTime(packet);
                    if (packetCount % 500 == 0) { SetStatusReceiving(); TryUpdateAnalysis(); }
                    else if (packetCount % 200 == 0) SetStatusReceiving();
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
                    totalBytes += read;
                    packetCount++;
                    rawWriter?.Write(pkt);
                    ProcessPacketRealTime(pkt);
                    if (packetCount % 500 == 0) { SetStatusReceiving(); TryUpdateAnalysis(); }
                    else if (packetCount % 200 == 0) SetStatusReceiving();
                }
            }
            catch (ObjectDisposedException) { }
            catch (System.IO.IOException) { }
            catch (Exception ex) { if (isReceiving) SetStatusError(ex.Message); }
            finally { isReceiving = false; SetStatusStopped(); }
        }

        private void ProcessPacketRealTime(byte[] packet)
        {
            try
            {
                if (packet.Length < 6) return;
                if (unpackMode == UnpackMode.W5500 && packet.Length >= 12)
                {
                    byte flags = packet[2];
                    byte half = (byte)((flags >> 6) & 0x03);
                    byte seg  = (byte)((flags >> 4) & 0x03);
                    int sc = packet[3];
                    if (half > 1 || seg > 2) return;
                    if (sc < 10 || sc > 200) sc = Math.Min(200, (packet.Length - 6) / 6);

                    // W5500: 直接从 byte[] 解析，不读回 DataManager（避免 Lock+Copy）
                    for (int i = 0; i < sc; i++)
                    {
                        int pos = 6 + i * 6;
                        short ch1 = (short)((packet[pos] << 8) | packet[pos + 1]);
                        short ch2 = (short)((packet[pos + 2] << 8) | packet[pos + 3]);
                        short ch3 = (short)((packet[pos + 4] << 8) | packet[pos + 5]);
                        AppendPlotData(ch1, ch2, ch3);
                        DataManager.AppendData(ch1, ch2, ch3);
                    }
                }
                else if (unpackMode == UnpackMode.CH395Q && packet.Length == 1440)
                {
                    DataManager.AppendPacket(packet);  // 旧协议仍走旧方法

                    // CH395Q: 直接从 byte[] 解析，不读回 DataManager
                    int sampleCount = 240;
                    for (int i = 0; i < sampleCount; i++)
                    {
                        int pos = i * 6;
                        short ch1 = (short)BitConverter.ToUInt16(packet, pos);
                        short ch2 = (short)BitConverter.ToUInt16(packet, pos + 2);
                        short ch3 = (short)BitConverter.ToUInt16(packet, pos + 4);
                        AppendPlotData(ch1 - 2048, ch2 - 2048, ch3 - 2048);
                    }
                }
                else
                {
                    int sampleCount = packet.Length / 6;
                    for (int i = 0; i < sampleCount; i++)
                    {
                        int pos = i * 6;
                        short ch1 = (short)BitConverter.ToUInt16(packet, pos);
                        short ch2 = (short)BitConverter.ToUInt16(packet, pos + 2);
                        short ch3 = (short)BitConverter.ToUInt16(packet, pos + 4);
                        AppendPlotData(ch1, ch2, ch3);
                        DataManager.AppendData(ch1, ch2, ch3);
                    }
                }
                MarkPlotDirty();
            }
            catch { }
        }

        private void TryUpdateAnalysis()
        {
            int cnt = plotCH1.Count;
            if (cnt < 512 || cnt - lastAnalysisCount < 500) return;
            lastAnalysisCount = cnt;

            // 使用 plotCH 数据（固定上限 5000 点，极轻量）
            double[] v1 = plotCH1.ToArray();
            double[] v2 = plotCH2.ToArray();
            double[] v3 = plotCH3.ToArray();

            double ff1 = lastFFTFreqs[0], ff2 = lastFFTFreqs[1], ff3 = lastFFTFreqs[2];
            if (ff1 == 0) ff1 = EstimateFrequency(v1);
            if (ff2 == 0) ff2 = EstimateFrequency(v2);
            if (ff3 == 0) ff3 = EstimateFrequency(v3);

            string stats = $"CH1 | Mean={v1.Average():F4} Max={v1.Max():F4} Min={v1.Min():F4} PP={(v1.Max()-v1.Min()):F4} RMS={Math.Sqrt(v1.Select(x=>x*x).Average()):F4} Freq={ff1:F1}Hz\n"
                         + $"CH2 | Mean={v2.Average():F4} Max={v2.Max():F4} Min={v2.Min():F4} PP={(v2.Max()-v2.Min()):F4} RMS={Math.Sqrt(v2.Select(x=>x*x).Average()):F4} Freq={ff2:F1}Hz\n"
                         + $"CH3 | Mean={v3.Average():F4} Max={v3.Max():F4} Min={v3.Min():F4} PP={(v3.Max()-v3.Min()):F4} RMS={Math.Sqrt(v3.Select(x=>x*x).Average()):F4} Freq={ff3:F1}Hz";
            label_analysis.Invoke(new Action(() => label_analysis.Text = stats));

            if (cnt - lastFFTCount >= 4096)
            {
                lastFFTCount = cnt;
                var s1 = v1.ToArray(); var s2 = v2.ToArray(); var s3 = v3.ToArray();
                Task.Run(() =>
                {
                    var r1 = ComputeFFT(s1); var r2 = ComputeFFT(s2); var r3 = ComputeFFT(s3);
                    lastFFTFreqs[0] = r1; lastFFTFreqs[1] = r2; lastFFTFreqs[2] = r3;
                    label_analysis.Invoke(new Action(() =>
                    {
                        var lines = label_analysis.Text.Split('\n');
                        if (lines.Length >= 3)
                            label_analysis.Text = $"CH1 | Mean={v1.Average():F4} Max={v1.Max():F4} Min={v1.Min():F4} PP={(v1.Max()-v1.Min()):F4} RMS={Math.Sqrt(v1.Select(x=>x*x).Average()):F4} Freq={r1:F1}Hz\n"
                                                + $"CH2 | Mean={v2.Average():F4} Max={v2.Max():F4} Min={v2.Min():F4} PP={(v2.Max()-v2.Min()):F4} RMS={Math.Sqrt(v2.Select(x=>x*x).Average()):F4} Freq={r2:F1}Hz\n"
                                                + $"CH3 | Mean={v3.Average():F4} Max={v3.Max():F4} Min={v3.Min():F4} PP={(v3.Max()-v3.Min()):F4} RMS={Math.Sqrt(v3.Select(x=>x*x).Average()):F4} Freq={r3:F1}Hz";
                    }));
                });
            }
        }

        /// <summary>
        /// 后台 FFT 计算（电压数组，已去直流）
        /// </summary>
        private double ComputeFFT(double[] v)
        {
            if (v.Length < 32) return 0;
            int n = v.Length;
            // 去直流分量（减均值）
            double mean = v.Average();
            System.Numerics.Complex[] cpx = v.Select(x => new System.Numerics.Complex(x - mean, 0)).ToArray();
            MathNet.Numerics.IntegralTransforms.Fourier.Forward(cpx, MathNet.Numerics.IntegralTransforms.FourierOptions.Default);
            double[] mag = cpx.Take(n / 2).Select(x => x.Magnitude / n).ToArray();
            int maxIdx = 1;
            for (int i = 2; i < mag.Length; i++)
                if (mag[i] > mag[maxIdx]) maxIdx = i;
            return maxIdx * 200e3 / n;
        }
    }
}