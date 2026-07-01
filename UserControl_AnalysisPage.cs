using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Show_SanTongDaoXinHao
{
    public partial class UserControl_AnalysisPage : UserControl
    {
        //=================================================
        // 播放控制
        //=================================================

        private readonly System.Windows.Forms.Timer playTimer = new();

        private bool isPlaying = false;

        private int currentIndex = 0;

        private const int STEP = 1000;

        //=================================================
        // 数据快照
        //=================================================

        private List<double> ch1 = new();

        private List<double> ch2 = new();

        private List<double> ch3 = new();

        //=================================================
        // ScottPlot曲线
        //=================================================

        private Signal sig1;

        private Signal sig2;

        private Signal sig3;

        //=================================================
        // 构造函数
        //=================================================

        public UserControl_AnalysisPage()
        {
            InitializeComponent();

            InitializePlot();

            ClearLabels();

            playTimer.Interval = 20;

            playTimer.Tick += PlayTimer_Tick;

            button_FFT.Enabled = false;
        }

        //=================================================
        // ScottPlot初始化
        //=================================================

        private void InitializePlot()
        {
            formsPlot1.Plot.Clear();

            formsPlot1.Plot.Title("三通道采样波形");

            formsPlot1.Plot.XLabel("Sample");

            formsPlot1.Plot.YLabel("ADC");

            formsPlot1.Refresh();
        }

        //=================================================
        // 清空测量值
        //=================================================

        private void ClearLabels()
        {
            label_CH1_Mean.Text = "";
            label_CH1_Max.Text = "";
            label_CH1_Min.Text = "";
            label_CH1_PP.Text = "";
            label_CH1_RMS.Text = "";
            label_CH1_f.Text = "";

            label_CH2_Mean.Text = "";
            label_CH2_Max.Text = "";
            label_CH2_Min.Text = "";
            label_CH2_PP.Text = "";
            label_CH2_RMS.Text = "";
            label_CH2_f.Text = "";

            label_CH3_Mean.Text = "";
            label_CH3_Max.Text = "";
            label_CH3_Min.Text = "";
            label_CH3_PP.Text = "";
            label_CH3_RMS.Text = "";
            label_CH3_f.Text = "";
        }

        //=================================================
        // 从DataManager复制数据
        //=================================================

        private bool PrepareData()
        {
            ch1 = DataManager.GetChannel1()
                             .Select(x => (double)x)
                             .ToList();

            ch2 = DataManager.GetChannel2()
                             .Select(x => (double)x)
                             .ToList();

            ch3 = DataManager.GetChannel3()
                             .Select(x => (double)x)
                             .ToList();

            if (ch1.Count == 0)
            {
                MessageBox.Show("没有可供分析的数据。");

                return false;
            }

            currentIndex = 0;

            return true;
        }

        //=================================================
        // 绘制当前波形
        //=================================================

        private void DrawCurrentWave()
        {
            formsPlot1.Plot.Clear();

            int count = Math.Min(currentIndex, ch1.Count);

            if (count <= 0)
            {
                formsPlot1.Refresh();
                return;
            }

            double[] y1 = ch1.Take(count).ToArray();
            double[] y2 = ch2.Take(count).ToArray();
            double[] y3 = ch3.Take(count).ToArray();

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

        //=================================================
        // Timer播放
        //=================================================

        private void PlayTimer_Tick(object? sender, EventArgs e)
        {
            currentIndex += STEP;

            if (currentIndex >= ch1.Count)
            {
                currentIndex = ch1.Count;

                DrawCurrentWave();

                UpdateStatistics();

                playTimer.Stop();

                isPlaying = false;

                button_FFT.Enabled = true;

                MessageBox.Show("所有数据已解析。");

                return;
            }

            DrawCurrentWave();

            UpdateStatistics();
        }

        //=================================================
        // 开始播放
        //=================================================

        private void StartPlay()
        {
            if (!PrepareData())
                return;

            button_FFT.Enabled = false;

            isPlaying = true;

            playTimer.Start();
        }

        //=================================================
        // 停止播放
        //=================================================

        private void StopPlay()
        {
            playTimer.Stop();

            isPlaying = false;

            button_FFT.Enabled = true;
        }

        //=================================================
        // 以下内容在第二部分继续
        //=================================================
        //========================================================
        // 更新三个GroupBox中的测量值
        //========================================================
        private void UpdateStatistics()
        {
            UpdateOneChannel(
                ch1,
                label_CH1_Mean,
                label_CH1_Max,
                label_CH1_Min,
                label_CH1_PP,
                label_CH1_RMS,
                label_CH1_f);

            UpdateOneChannel(
                ch2,
                label_CH2_Mean,
                label_CH2_Max,
                label_CH2_Min,
                label_CH2_PP,
                label_CH2_RMS,
                label_CH2_f);

            UpdateOneChannel(
                ch3,
                label_CH3_Mean,
                label_CH3_Max,
                label_CH3_Min,
                label_CH3_PP,
                label_CH3_RMS,
                label_CH3_f);
        }

        //========================================================
        // 单通道统计
        //========================================================
        private void UpdateOneChannel(
            List<double> data,
            System.Windows.Forms.Label mean,
            System.Windows.Forms.Label max,
            System.Windows.Forms.Label min,
            System.Windows.Forms.Label pp,
            System.Windows.Forms.Label rms,
            System.Windows.Forms.Label freq)
        {
            if (currentIndex <= 0)
                return;

            var part = data.Take(currentIndex).ToArray();

            double Mean = part.Average();

            double Max = part.Max();

            double Min = part.Min();

            double PP = Max - Min;

            double RMS = Math.Sqrt(part.Select(x => x * x).Average());

            mean.Text = Mean.ToString("F2");

            max.Text = Max.ToString("F0");

            min.Text = Min.ToString("F0");

            pp.Text = PP.ToString("F2");

            rms.Text = RMS.ToString("F2");

            // FFT完成之前暂时不计算频率
            freq.Text = "--";
        }

        //========================================================
        // 开始绘图
        //========================================================
        private void button_draw_Click(object sender, EventArgs e)
        {
            if (isPlaying)
                return;

            StartPlay();
        }

        //========================================================
        // 暂停
        //========================================================
        private void button_stop_Click(object sender, EventArgs e)
        {
            if (!isPlaying)
                return;

            StopPlay();
        }

        //========================================================
        // 清除波形
        //========================================================
        private void button_clear_Click(object sender, EventArgs e)
        {
            StopPlay();

            currentIndex = 0;

            formsPlot1.Plot.Clear();

            formsPlot1.Refresh();

            ClearLabels();

            button_FFT.Enabled = false;
        }

        //========================================================
        // FFT（以后实现）
        //========================================================
        private void button_FFT_Click(object sender, EventArgs e)
        {
            MessageBox.Show("FFT分析将在下一版本实现。");
        }

        //========================================================
        // 实时刷新数据
        //========================================================
        private void button_refresh_Click(object sender, EventArgs e)
        {
            if (isPlaying)
                StopPlay();

            if (!PrepareData())
                return;

            currentIndex = ch1.Count;
            DrawCurrentWave();
            UpdateStatistics();
            button_FFT.Enabled = true;
        }

        //========================================================
        // Designer自动生成的空事件
        // 保留即可
        //========================================================

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void label23_Click(object sender, EventArgs e)
        {
        }

        private void label23_Click_1(object sender, EventArgs e)
        {
        }

        private void label24_Click(object sender, EventArgs e)
        {
        }

        private void button_FFT_Click_1(object sender, EventArgs e)
        {
            // 只有暂停状态才能打开FFT
            if (isPlaying)
            {
                MessageBox.Show("请先暂停绘图后再进行FFT分析！");
                return;
            }

            // 没有数据
            if (DataManager.GetDataCount() == 0)
            {
                MessageBox.Show("没有可分析的数据！");
                return;
            }

            Form2 fftForm = new Form2();

            fftForm.ShowDialog();

            // FFT结束后，把主频显示回来
            label_CH1_f.Text = fftForm.CH1Frequency.ToString("F2") + " Hz";
            label_CH2_f.Text = fftForm.CH2Frequency.ToString("F2") + " Hz";
            label_CH3_f.Text = fftForm.CH3Frequency.ToString("F2") + " Hz";
        }
    }
}

