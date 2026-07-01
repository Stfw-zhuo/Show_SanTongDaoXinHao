using MathNet.Numerics.IntegralTransforms;
using ScottPlot.WinForms;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Show_SanTongDaoXinHao
{
    public partial class FormFFT : Form
    {
        public double[] Freqs = new double[3];
        private readonly Action<string>? onProgress;

        public FormFFT(short[] ch1, short[] ch2, short[] ch3, Action<string>? progressCallback = null)
        {
            InitializeComponent();
            onProgress = progressCallback;

            progressBar1.Maximum = 3;
            progressBar1.Value = 0;
            label_progress.Text = "0/3";
            onProgress?.Invoke("0/3");

            ShowFFT(ch1, 0, formsPlot1);
            ShowFFT(ch2, 1, formsPlot2);
            ShowFFT(ch3, 2, formsPlot3);

            Text = $"FFT 分析 - CH1:{Freqs[0]:F1}Hz  CH2:{Freqs[1]:F1}Hz  CH3:{Freqs[2]:F1}Hz";
            progressBar1.Value = 3;
            label_progress.Text = "3/3";
            onProgress?.Invoke("3/3");
        }

        private void ShowFFT(short[] data, int idx, FormsPlot plot)
        {
            int N = data.Length;
            if (N < 16) return;

            // 去直流分量
            double mean = data.Select(x => (double)x).Average();
            System.Numerics.Complex[] cpx = data.Select(x => new System.Numerics.Complex((double)x - mean, 0)).ToArray();
            Fourier.Forward(cpx, FourierOptions.Default);

            // 正频率部分
            double[] mag = cpx.Take(N / 2).Select(x => x.Magnitude / N).ToArray();
            double[] freq = Enumerable.Range(0, N / 2).Select(i => i * 200e3 / N).ToArray();

            // 跳过DC找主频
            int maxIdx = 1;
            double maxMag = 0;
            for (int i = 2; i < mag.Length; i++)
            {
                if (mag[i] > maxMag)
                {
                    maxMag = mag[i];
                    maxIdx = i;
                }
            }
            Freqs[idx] = maxIdx > 0 ? freq[maxIdx] : 0;

            plot.Plot.Clear();

            // 跳过 DC 分量绘制
            double[] magNoDC = mag.Skip(1).ToArray();
            double[] freqNoDC = freq.Skip(1).ToArray();

            var sig = plot.Plot.Add.Scatter(freqNoDC, magNoDC);
            sig.LegendText = $"CH{idx + 1} 峰值={Freqs[idx]:F1}Hz";
            sig.LineWidth = 1;
            sig.MarkerSize = 0;          // 不显示点
            sig.Color = new ScottPlot.Color(0, 200, 255, 200);

            plot.Plot.Title($"通道{idx + 1} FFT");
            plot.Plot.XLabel("频率 (Hz)");
            plot.Plot.YLabel("幅度");
            plot.Plot.Axes.AutoScale();
            plot.Plot.ShowLegend();

            // Y 轴自适应（基于非 DC 分量）
            double maxPlot = magNoDC.Max();
            if (maxPlot > 0)
                plot.Plot.Axes.SetLimitsY(0, maxPlot * 1.1);

            plot.Refresh();

            // 更新进度条
            string progress = $"{idx + 1}/3";
            if (InvokeRequired)
                Invoke(new Action(() => { progressBar1.Value = idx + 1; label_progress.Text = progress; }));
            else
            { progressBar1.Value = idx + 1; label_progress.Text = progress; }
            onProgress?.Invoke(progress);
        }
    }
}