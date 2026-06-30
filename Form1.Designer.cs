namespace Show_SanTongDaoXinHao
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel1 = new Panel();
            button_shouye = new Button();
            button_caiji = new Button();
            button_fenxi = new Button();
            button_bangzhu = new Button();
            panelMain = new Panel();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.BackColor = Color.FromArgb(24, 30, 54);
            flowLayoutPanel1.Controls.Add(panel1);
            flowLayoutPanel1.Controls.Add(button_shouye);
            flowLayoutPanel1.Controls.Add(button_caiji);
            flowLayoutPanel1.Controls.Add(button_fenxi);
            flowLayoutPanel1.Controls.Add(button_bangzhu);
            flowLayoutPanel1.Dock = DockStyle.Left;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Margin = new Padding(4);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(353, 1280);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(4, 4);
            panel1.Margin = new Padding(4);
            panel1.Name = "panel1";
            panel1.Size = new Size(349, 312);
            panel1.TabIndex = 0;
            // 
            // button_shouye
            // 
            button_shouye.Dock = DockStyle.Top;
            button_shouye.FlatAppearance.BorderSize = 0;
            button_shouye.FlatStyle = FlatStyle.Flat;
            button_shouye.ForeColor = Color.FromArgb(0, 126, 249);
            button_shouye.Location = new Point(4, 324);
            button_shouye.Margin = new Padding(4);
            button_shouye.Name = "button_shouye";
            button_shouye.Size = new Size(349, 116);
            button_shouye.TabIndex = 1;
            button_shouye.Text = "首页";
            button_shouye.UseVisualStyleBackColor = true;
            button_shouye.Click += button_shouye_Click;
            // 
            // button_caiji
            // 
            button_caiji.Dock = DockStyle.Top;
            button_caiji.FlatAppearance.BorderSize = 0;
            button_caiji.FlatStyle = FlatStyle.Flat;
            button_caiji.ForeColor = Color.FromArgb(0, 126, 249);
            button_caiji.Location = new Point(4, 448);
            button_caiji.Margin = new Padding(4);
            button_caiji.Name = "button_caiji";
            button_caiji.Size = new Size(349, 116);
            button_caiji.TabIndex = 2;
            button_caiji.Text = "采集";
            button_caiji.UseVisualStyleBackColor = true;
            button_caiji.Click += button_caiji_Click;
            // 
            // button_fenxi
            // 
            button_fenxi.Dock = DockStyle.Top;
            button_fenxi.FlatAppearance.BorderSize = 0;
            button_fenxi.FlatStyle = FlatStyle.Flat;
            button_fenxi.ForeColor = Color.FromArgb(0, 126, 249);
            button_fenxi.Location = new Point(4, 572);
            button_fenxi.Margin = new Padding(4);
            button_fenxi.Name = "button_fenxi";
            button_fenxi.Size = new Size(349, 116);
            button_fenxi.TabIndex = 3;
            button_fenxi.Text = "分析";
            button_fenxi.UseVisualStyleBackColor = true;
            button_fenxi.Click += button_fenxi_Click;
            // 
            // button_bangzhu
            // 
            button_bangzhu.Dock = DockStyle.Top;
            button_bangzhu.FlatAppearance.BorderSize = 0;
            button_bangzhu.FlatStyle = FlatStyle.Flat;
            button_bangzhu.ForeColor = Color.FromArgb(0, 126, 249);
            button_bangzhu.Location = new Point(4, 696);
            button_bangzhu.Margin = new Padding(4);
            button_bangzhu.Name = "button_bangzhu";
            button_bangzhu.Size = new Size(349, 116);
            button_bangzhu.TabIndex = 4;
            button_bangzhu.Text = "帮助";
            button_bangzhu.UseVisualStyleBackColor = true;
            button_bangzhu.Click += button_bangzhu_Click;
            // 
            // panelMain
            // 
            panelMain.Dock = DockStyle.Fill;
            panelMain.Location = new Point(353, 0);
            panelMain.Name = "panelMain";
            panelMain.Size = new Size(1613, 1280);
            panelMain.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(20F, 42F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(46, 51, 73);
            ClientSize = new Size(1966, 1280);
            Controls.Add(panelMain);
            Controls.Add(flowLayoutPanel1);
            Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold, GraphicsUnit.Point, 134);
            FormBorderStyle = FormBorderStyle.None;
            Margin = new Padding(4);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private FlowLayoutPanel flowLayoutPanel1;
        private Panel panel1;
        private Button button_shouye;
        private Button button_caiji;
        private Button button_fenxi;
        private Button button_bangzhu;
        private Panel panelMain;
    }
}
