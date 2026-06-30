namespace Show_SanTongDaoXinHao
{
    partial class Form2
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            formsPlot2 = new ScottPlot.WinForms.FormsPlot();
            formsPlot3 = new ScottPlot.WinForms.FormsPlot();
            button_on = new Button();
            button_off = new Button();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            SuspendLayout();
            // 
            // formsPlot1
            // 
            formsPlot1.Location = new Point(29, 12);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(777, 282);
            formsPlot1.TabIndex = 0;
            // 
            // formsPlot2
            // 
            formsPlot2.Location = new Point(29, 329);
            formsPlot2.Name = "formsPlot2";
            formsPlot2.Size = new Size(777, 282);
            formsPlot2.TabIndex = 1;
            // 
            // formsPlot3
            // 
            formsPlot3.Location = new Point(29, 666);
            formsPlot3.Name = "formsPlot3";
            formsPlot3.Size = new Size(777, 282);
            formsPlot3.TabIndex = 2;
            // 
            // button_on
            // 
            button_on.Location = new Point(193, 1016);
            button_on.Name = "button_on";
            button_on.Size = new Size(112, 34);
            button_on.TabIndex = 3;
            button_on.Text = "开始FFT";
            button_on.UseVisualStyleBackColor = true;
            button_on.Click += button_on_Click;
            // 
            // button_off
            // 
            button_off.Location = new Point(627, 1012);
            button_off.Name = "button_off";
            button_off.Size = new Size(112, 34);
            button_off.TabIndex = 4;
            button_off.Text = "关闭界面";
            button_off.UseVisualStyleBackColor = true;
            button_off.Click += button_off_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(894, 116);
            label1.Name = "label1";
            label1.Size = new Size(94, 36);
            label1.TabIndex = 5;
            label1.Text = "label1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(894, 426);
            label2.Name = "label2";
            label2.Size = new Size(94, 36);
            label2.TabIndex = 6;
            label2.Text = "label2";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(894, 774);
            label3.Name = "label3";
            label3.Size = new Size(94, 36);
            label3.TabIndex = 7;
            label3.Text = "label3";
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(17F, 36F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1096, 1133);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button_off);
            Controls.Add(button_on);
            Controls.Add(formsPlot3);
            Controls.Add(formsPlot2);
            Controls.Add(formsPlot1);
            Name = "Form2";
            Text = "Form2";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ScottPlot.WinForms.FormsPlot formsPlot1;
        private ScottPlot.WinForms.FormsPlot formsPlot2;
        private ScottPlot.WinForms.FormsPlot formsPlot3;
        private Button button_on;
        private Button button_off;
        private Label label1;
        private Label label2;
        private Label label3;
    }
}