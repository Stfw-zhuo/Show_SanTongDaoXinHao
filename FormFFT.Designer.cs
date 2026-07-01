namespace Show_SanTongDaoXinHao
{
    partial class FormFFT
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            formsPlot2 = new ScottPlot.WinForms.FormsPlot();
            formsPlot3 = new ScottPlot.WinForms.FormsPlot();
            progressBar1 = new ProgressBar();
            label_progress = new Label();
            SuspendLayout();
            // 
            // formsPlot1
            // 
            formsPlot1.Location = new Point(12, 12);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(750, 180);
            formsPlot1.TabIndex = 0;
            // 
            // formsPlot2
            // 
            formsPlot2.Location = new Point(12, 200);
            formsPlot2.Name = "formsPlot2";
            formsPlot2.Size = new Size(750, 180);
            formsPlot2.TabIndex = 1;
            // 
            // formsPlot3
            // 
            formsPlot3.Location = new Point(12, 388);
            formsPlot3.Name = "formsPlot3";
            formsPlot3.Size = new Size(750, 180);
            formsPlot3.TabIndex = 2;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(12, 578);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(620, 20);
            progressBar1.TabIndex = 3;
            // 
            // label_progress
            // 
            label_progress.AutoSize = true;
            label_progress.Font = new Font("Microsoft YaHei UI", 9F);
            label_progress.ForeColor = Color.LightGray;
            label_progress.Location = new Point(640, 580);
            label_progress.Name = "label_progress";
            label_progress.Size = new Size(60, 17);
            label_progress.TabIndex = 4;
            label_progress.Text = "0/3";
            // 
            // FormFFT
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(26, 26, 46);
            ClientSize = new Size(774, 610);
            Controls.Add(label_progress);
            Controls.Add(progressBar1);
            Controls.Add(formsPlot3);
            Controls.Add(formsPlot2);
            Controls.Add(formsPlot1);
            Name = "FormFFT";
            Text = "FFT 分析";
            ResumeLayout(false);
            PerformLayout();
        }

        private ScottPlot.WinForms.FormsPlot formsPlot1;
        private ScottPlot.WinForms.FormsPlot formsPlot2;
        private ScottPlot.WinForms.FormsPlot formsPlot3;
        private ProgressBar progressBar1;
        private Label label_progress;
    }
}