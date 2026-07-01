using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Show_SanTongDaoXinHao
{
    public partial class UserControl_AnalysisPage : UserControl
    {
        private readonly System.Windows.Forms.Timer playTimer = new();
        private bool isPlaying = false;
        private int currentIndex = 0;
        private const int STEP = 1000;

        private List<double> ch1 = new();
        private List<double> ch2 = new();
        private List<double> ch3 = new();

        private Signal? sig1, sig2, sig3;

        public UserControl_AnalysisPage()
        {
            InitializeComponent();

            InitializePlot();
            ClearLabels();
            playTimer.Interval = 20;
            playTimer.Tick += PlayTimer_Tick;
            button_FFT.Enabled = false;
        }

        private void InitializePlot()
        {
            formsPlot1.Plot.Clear();
            formsPlot1.Plot.Title("三通道采样波形");
            formsPlot1.Plot.XLabel("Sample");
            formsPlot1.Plot.YLabel("Voltage (V)");
            formsPlot1.Plot.Axes.SetLimitsX(0, 10000);
            formsPlot1.Plot.Axes.SetLimitsY(-3.5, 3.5);
            formsPlot1.Refresh();
        }

        private void ClearLabels()
        {
            label_CH1_Mean.Text = label_CH1_Max.Text = label_CH1_Min.Text =
            label_CH1_PP.Text = label_CH1_RMS.Text = label_CH1_f.Text = "";
            label_CH2_Mean.Text = label_CH2_Max.Text = label_CH2_Min.Text =
            label_CH2_PP.Text = label_CH2_RMS.Text = label_CH2_f.Text = "";
            label_CH3_Mean.Text = label_CH3_Max.Text = label_CH3_Min.Text =
            label_CH3_PP.Text = label_CH3_RMS.Text = label_CH3_f.Text = "";
        }

        private bool PrepareData()
        {
            ch1 = DataManager.GetChannel1().Select(x => (double)x * 3.0 / 4095.0).ToList();
            ch2 = DataManager.GetChannel2().Select(x => (double)x * 3.0 / 4095.0).ToList();
            ch3 = DataManager.GetChannel3().Select(x => (double)x * 3.0 / 4095.0).ToList();
            if (ch1.Count == 0) { MessageBox.Show("没有可供分析的数据。"); return false; }
            currentIndex = 0;
            return true;
        }

        private void DrawCurrentWave()
        {
            formsPlot1.Plot.Clear();
            int count = Math.Min(currentIndex, ch1.Count);
            if (count <= 0) { formsPlot1.Refresh(); return; }
            double[] y1 = ch1.Take(count).ToArray();
            double[] y2 = ch2.Take(count).ToArray();
            double[] y3 = ch3.Take(count).ToArray();
            sig1 = formsPlot1.Plot.Add.Signal(y1);
            sig2 = formsPlot1.Plot.Add.Signal(y2);
            sig3 = formsPlot1.Plot.Add.Signal(y3);
            sig1.LegendText = "CH1"; sig2.LegendText = "CH2"; sig3.LegendText = "CH3";
            formsPlot1.Plot.ShowLegend();
            formsPlot1.Plot.Axes.AutoScaleX();
            formsPlot1.Plot.Axes.SetLimitsY(-3.5, 3.5);
            formsPlot1.Refresh();
        }

        private void PlayTimer_Tick(object? sender, EventArgs e)
        {
            currentIndex += STEP;
            if (currentIndex >= ch1.Count)
            {
                currentIndex = ch1.Count;
                DrawCurrentWave(); UpdateStatistics();
                playTimer.Stop(); isPlaying = false; button_FFT.Enabled = true;
                return;
            }
            DrawCurrentWave(); UpdateStatistics();
        }

        private void StartPlay()
        {
            if (!PrepareData()) return;
            button_FFT.Enabled = false; isPlaying = true; playTimer.Start();
        }

        private void StopPlay()
        {
            playTimer.Stop(); isPlaying = false; button_FFT.Enabled = true;
        }

        private void UpdateStatistics()
        {
            UpdateOneChannel(ch1, label_CH1_Mean, label_CH1_Max, label_CH1_Min, label_CH1_PP, label_CH1_RMS, label_CH1_f);
            UpdateOneChannel(ch2, label_CH2_Mean, label_CH2_Max, label_CH2_Min, label_CH2_PP, label_CH2_RMS, label_CH2_f);
            UpdateOneChannel(ch3, label_CH3_Mean, label_CH3_Max, label_CH3_Min, label_CH3_PP, label_CH3_RMS, label_CH3_f);
        }

        private void UpdateOneChannel(List<double> data, Label mean, Label max, Label min, Label pp, Label rms, Label freq)
        {
            if (currentIndex <= 0) return;
            var part = data.Take(currentIndex).ToArray();
            double Mean = part.Average();
            double Max = part.Max(); double Min = part.Min();
            double PP = Max - Min;
            double RMS = Math.Sqrt(part.Select(x => x * x).Average());
            mean.Text = Mean.ToString("F6");
            max.Text = Max.ToString("F6");
            min.Text = Min.ToString("F6");
            pp.Text = PP.ToString("F6");
            rms.Text = RMS.ToString("F6");
            freq.Text = "--";
        }

        private void button_draw_Click(object sender, EventArgs e) { if (!isPlaying) StartPlay(); }
        private void button_stop_Click(object sender, EventArgs e) { if (isPlaying) StopPlay(); }
        private void button_clear_Click(object sender, EventArgs e)
        {
            StopPlay(); currentIndex = 0;
            formsPlot1.Plot.Clear(); formsPlot1.Refresh(); ClearLabels(); button_FFT.Enabled = false;
        }
        private void button_FFT_Click(object sender, EventArgs e) { MessageBox.Show("FFT分析将在下一版本实现。"); }
        private void button_refresh_Click(object sender, EventArgs e)
        {
            if (isPlaying) StopPlay();
            if (!PrepareData()) return;
            currentIndex = ch1.Count;
            DrawCurrentWave(); UpdateStatistics();
            button_FFT.Enabled = true;
        }
        private void button_FFT_Click_1(object sender, EventArgs e)
        {
            if (isPlaying) { MessageBox.Show("请先暂停绘图后再进行FFT分析！"); return; }
            if (DataManager.GetDataCount() == 0) { MessageBox.Show("没有可分析的数据！"); return; }
            Form2 fftForm = new Form2();
            fftForm.ShowDialog();
            label_CH1_f.Text = fftForm.CH1Frequency.ToString("F2") + " Hz";
            label_CH2_f.Text = fftForm.CH2Frequency.ToString("F2") + " Hz";
            label_CH3_f.Text = fftForm.CH3Frequency.ToString("F2") + " Hz";
        }

        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void label23_Click(object sender, EventArgs e) { }
        private void label23_Click_1(object sender, EventArgs e) { }
        private void label24_Click(object sender, EventArgs e) { }
    }
}