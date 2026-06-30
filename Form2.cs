using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace Show_SanTongDaoXinHao
{
    public partial class Form2 : Form
    {
        //====================================================
        // 采样率（以后如果STM32修改，只改这里即可）
        //====================================================

        public double CH1Frequency { get; private set; }

        public double CH2Frequency { get; private set; }

        public double CH3Frequency { get; private set; }

        private const double SAMPLE_RATE = 200000.0;

        //====================================================
        // 三通道数据
        //====================================================

        private List<double> ch1 = new();
        private List<double> ch2 = new();
        private List<double> ch3 = new();

        //====================================================
        // 主频（返回AnalysisPage）
        //====================================================

        public double MainFreq1 { get; private set; }

        public double MainFreq2 { get; private set; }

        public double MainFreq3 { get; private set; }

        //====================================================
        // 构造函数
        //====================================================

        public Form2()
        {
            InitializeComponent();

            InitializePlots();
        }

        //====================================================
        // 初始化三个Plot
        //====================================================

        private void InitializePlots()
        {
            InitPlot(formsPlot1, "CH1 FFT");

            InitPlot(formsPlot2, "CH2 FFT");

            InitPlot(formsPlot3, "CH3 FFT");
        }

        private void InitPlot(ScottPlot.WinForms.FormsPlot fp, string title)
        {
            fp.Plot.Clear();

            fp.Plot.Title(title);

            fp.Plot.XLabel("Frequency (Hz)");

            fp.Plot.YLabel("Magnitude");

            fp.Refresh();
        }

        //====================================================
        // 从DataManager读取数据
        //====================================================

        private void LoadData()
        {
            ch1 = DataManager
                .GetChannel1()
                .Select(x => (double)x)
                .ToList();

            ch2 = DataManager
                .GetChannel2()
                .Select(x => (double)x)
                .ToList();

            ch3 = DataManager
                .GetChannel3()
                .Select(x => (double)x)
                .ToList();
        }

        //====================================================
        // 开始FFT
        //====================================================

        private void button_on_Click(object sender, EventArgs e)
        {
            var ch1 = DataManager.GetChannel1()
                                 .Select(x => (double)x)
                                 .ToArray();

            var ch2 = DataManager.GetChannel2()
                                 .Select(x => (double)x)
                                 .ToArray();

            var ch3 = DataManager.GetChannel3()
                                 .Select(x => (double)x)
                                 .ToArray();

            if (ch1.Length == 0)
            {
                MessageBox.Show("没有可分析的数据！");
                return;
            }

            DrawFFT(formsPlot1, ch1, label1, out double f1);

            DrawFFT(formsPlot2, ch2, label2, out double f2);

            DrawFFT(formsPlot3, ch3, label3, out double f3);

            CH1Frequency = f1;
            CH2Frequency = f2;
            CH3Frequency = f3;

            MessageBox.Show("FFT已结束");
        }

        //====================================================
        // 关闭窗口
        //====================================================

        private void button_off_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            Close();
        }

        //====================================================
        // 单通道FFT流程
        //====================================================

        private void AnalyzeOneChannel(
            List<double> signal,
            ScottPlot.WinForms.FormsPlot plot,
            System.Windows.Forms.Label freqLabel,
            out double mainFreq)
        {
            double[] freq;

            double[] mag;

            CalculateFFT(
                signal,
                out freq,
                out mag);

            mainFreq = FindPeakFrequency(
                freq,
                mag);

            DrawSpectrum(
                plot,
                freq,
                mag,
                mainFreq);

            freqLabel.Text =
                $"主频：{mainFreq:F2} Hz";
        }
        //====================================================
        // FFT计算
        //====================================================

        private void CalculateFFT(
            List<double> signal,
            out double[] frequency,
            out double[] magnitude)
        {
            int n = NextPowerOfTwo(signal.Count);

            Complex[] data = new Complex[n];

            for (int i = 0; i < signal.Count; i++)
            {
                data[i] = new Complex(signal[i], 0);
            }

            for (int i = signal.Count; i < n; i++)
            {
                data[i] = Complex.Zero;
            }

            Fourier.Forward(data, FourierOptions.Matlab);

            int half = n / 2;

            frequency = new double[half];

            magnitude = new double[half];

            for (int i = 0; i < half; i++)
            {
                frequency[i] =
                    i * SAMPLE_RATE / n;

                magnitude[i] =
                    data[i].Magnitude;
            }
        }

        //====================================================
        // 找主峰（忽略直流）
        //====================================================

        private double FindPeakFrequency(
            double[] frequency,
            double[] magnitude)
        {
            int index = 1;

            double max = magnitude[1];

            for (int i = 2; i < magnitude.Length; i++)
            {
                if (magnitude[i] > max)
                {
                    max = magnitude[i];
                    index = i;
                }
            }

            return frequency[index];
        }

        //====================================================
        // 绘制频谱
        //====================================================

        private void DrawSpectrum(
            ScottPlot.WinForms.FormsPlot plot,
            double[] frequency,
            double[] magnitude,
            double peakFrequency)
        {
            plot.Plot.Clear();

            // FFT曲线
            plot.Plot.Add.Scatter(
                frequency,
                magnitude);

            // 找峰值位置
            int peakIndex = 0;

            double minError = double.MaxValue;

            for (int i = 0; i < frequency.Length; i++)
            {
                double err =
                    Math.Abs(frequency[i] - peakFrequency);

                if (err < minError)
                {
                    minError = err;
                    peakIndex = i;
                }
            }

            // 红点标记峰值
            plot.Plot.Add.Scatter(
                new double[]
                {
                    frequency[peakIndex]
                },
                new double[]
                {
                    magnitude[peakIndex]
                });

            // 峰值文字
            plot.Plot.Add.Text(
                $"{peakFrequency:F1} Hz",
                frequency[peakIndex],
                magnitude[peakIndex]);

            plot.Plot.Axes.AutoScale();

            plot.Refresh();
        }

        //====================================================
        // 求大于等于n的最小2次幂
        //====================================================

        private int NextPowerOfTwo(int n)
        {
            int p = 1;

            while (p < n)
            {
                p <<= 1;
            }

            return p;
        }
        private (double[] freq, double[] amp) CalculateFFT(double[] signal)
        {
            int n = signal.Length;

            Complex[] buffer = new Complex[n];

            for (int i = 0; i < n; i++)
                buffer[i] = new Complex(signal[i], 0);

            Fourier.Forward(buffer, FourierOptions.Matlab);

            int half = n / 2;

            double[] freq = new double[half];
            double[] amp = new double[half];

            for (int i = 0; i < half; i++)
            {
                freq[i] = i * SAMPLE_RATE / n;
                amp[i] = buffer[i].Magnitude;
            }

            return (freq, amp);
        }

        private void DrawFFT(
    ScottPlot.WinForms.FormsPlot plot,
    double[] signal,
    System.Windows.Forms.Label label,
    out double peakFreq)
        {
            var result = CalculateFFT(signal);

            double[] freq = result.freq;
            double[] amp = result.amp;

            plot.Plot.Clear();

            plot.Plot.Add.Scatter(freq, amp);

            int index = 1;

            double max = amp[1];

            for (int i = 2; i < amp.Length; i++)
            {
                if (amp[i] > max)
                {
                    max = amp[i];
                    index = i;
                }
            }

            peakFreq = freq[index];

            plot.Plot.Add.Marker(
                peakFreq,
                max);

            plot.Plot.Add.Text(
                $"{peakFreq:F1} Hz",
                peakFreq,
                max);

            plot.Plot.Axes.AutoScale();

            plot.Refresh();

            label.Text = $"主频：{peakFreq:F1} Hz";
        }

    }
}