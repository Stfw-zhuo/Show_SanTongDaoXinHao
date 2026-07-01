namespace Show_SanTongDaoXinHao
{
    partial class UserControl_CollectPage
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
            button1 = new Button();
            button2 = new Button();
            button3 = new Button();
            button_connect = new Button();
            label_title = new Label();
            label_protocol = new Label();
            comboBox_protocol = new ComboBox();
            label_localIP = new Label();
            comboBox_localIP = new ComboBox();
            label_port = new Label();
            textBox_port = new TextBox();
            label_remoteIP = new Label();
            textBox_remoteIP = new TextBox();
            label_status = new Label();
            label_mode = new Label();
            comboBox_mode = new ComboBox();
            checkBox_autoRefresh = new CheckBox();
            formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            SuspendLayout();
            // 
            // label_title
            // 
            label_title.AutoSize = true;
            label_title.Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold);
            label_title.ForeColor = Color.FromArgb(0, 126, 249);
            label_title.Location = new Point(16, 14);
            label_title.Name = "label_title";
            label_title.Size = new Size(136, 30);
            label_title.TabIndex = 3;
            label_title.Text = "数据接收设置";
            // 
            // label_protocol
            // 
            label_protocol.AutoSize = true;
            label_protocol.Font = new Font("Microsoft YaHei UI", 12F);
            label_protocol.ForeColor = Color.White;
            label_protocol.Location = new Point(16, 56);
            label_protocol.Name = "label_protocol";
            label_protocol.Size = new Size(90, 21);
            label_protocol.TabIndex = 4;
            label_protocol.Text = "协议选择：";
            // 
            // comboBox_protocol
            // 
            comboBox_protocol.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_protocol.Font = new Font("Microsoft YaHei UI", 12F);
            comboBox_protocol.Location = new Point(110, 54);
            comboBox_protocol.Name = "comboBox_protocol";
            comboBox_protocol.Size = new Size(120, 29);
            comboBox_protocol.TabIndex = 5;
            // 
            // label_localIP
            // 
            label_localIP.AutoSize = true;
            label_localIP.Font = new Font("Microsoft YaHei UI", 12F);
            label_localIP.ForeColor = Color.White;
            label_localIP.Location = new Point(16, 92);
            label_localIP.Name = "label_localIP";
            label_localIP.Size = new Size(90, 21);
            label_localIP.TabIndex = 6;
            label_localIP.Text = "本机网卡：";
            // 
            // comboBox_localIP
            // 
            comboBox_localIP.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_localIP.Font = new Font("Microsoft YaHei UI", 12F);
            comboBox_localIP.Location = new Point(110, 90);
            comboBox_localIP.Name = "comboBox_localIP";
            comboBox_localIP.Size = new Size(220, 29);
            comboBox_localIP.TabIndex = 7;
            // 
            // label_port
            // 
            label_port.AutoSize = true;
            label_port.Font = new Font("Microsoft YaHei UI", 12F);
            label_port.ForeColor = Color.White;
            label_port.Location = new Point(16, 128);
            label_port.Name = "label_port";
            label_port.Size = new Size(74, 21);
            label_port.TabIndex = 8;
            label_port.Text = "端口号：";
            // 
            // textBox_port
            // 
            textBox_port.Font = new Font("Microsoft YaHei UI", 12F);
            textBox_port.Location = new Point(110, 126);
            textBox_port.Name = "textBox_port";
            textBox_port.Size = new Size(80, 29);
            textBox_port.TabIndex = 9;
            textBox_port.Text = "8080";
            // 
            // button_connect
            // 
            button_connect.Font = new Font("Microsoft YaHei UI", 12F, FontStyle.Bold);
            button_connect.Location = new Point(200, 124);
            button_connect.Name = "button_connect";
            button_connect.Size = new Size(90, 32);
            button_connect.TabIndex = 11;
            button_connect.Text = "连接";
            button_connect.UseVisualStyleBackColor = true;
            button_connect.Click += button_connect_Click;
            // 
            // label_remoteIP
            // 
            label_remoteIP.AutoSize = true;
            label_remoteIP.Font = new Font("Microsoft YaHei UI", 12F);
            label_remoteIP.ForeColor = Color.White;
            label_remoteIP.Location = new Point(16, 164);
            label_remoteIP.Name = "label_remoteIP";
            label_remoteIP.Size = new Size(74, 21);
            label_remoteIP.TabIndex = 12;
            label_remoteIP.Text = "远程IP：";
            label_remoteIP.Visible = false;
            // 
            // textBox_remoteIP
            // 
            textBox_remoteIP.Font = new Font("Microsoft YaHei UI", 12F);
            textBox_remoteIP.Location = new Point(110, 162);
            textBox_remoteIP.Name = "textBox_remoteIP";
            textBox_remoteIP.Size = new Size(180, 29);
            textBox_remoteIP.TabIndex = 13;
            textBox_remoteIP.Text = "192.168.1.110";
            textBox_remoteIP.Visible = false;
            // 
            // label_mode
            // 
            label_mode.AutoSize = true;
            label_mode.Font = new Font("Microsoft YaHei UI", 12F);
            label_mode.ForeColor = Color.White;
            label_mode.Location = new Point(16, 196);
            label_mode.Name = "label_mode";
            label_mode.Size = new Size(90, 21);
            label_mode.TabIndex = 14;
            label_mode.Text = "解包模式：";
            // 
            // comboBox_mode
            // 
            comboBox_mode.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox_mode.Font = new Font("Microsoft YaHei UI", 12F);
            comboBox_mode.Location = new Point(110, 194);
            comboBox_mode.Name = "comboBox_mode";
            comboBox_mode.Size = new Size(140, 29);
            comboBox_mode.TabIndex = 15;
            // 
            // checkBox_autoRefresh
            // 
            checkBox_autoRefresh.AutoSize = true;
            checkBox_autoRefresh.Checked = true;
            checkBox_autoRefresh.CheckState = CheckState.Checked;
            checkBox_autoRefresh.Font = new Font("Microsoft YaHei UI", 12F);
            checkBox_autoRefresh.ForeColor = Color.White;
            checkBox_autoRefresh.Location = new Point(270, 196);
            checkBox_autoRefresh.Name = "checkBox_autoRefresh";
            checkBox_autoRefresh.Size = new Size(110, 25);
            checkBox_autoRefresh.TabIndex = 16;
            checkBox_autoRefresh.Text = "实时绘图";
            checkBox_autoRefresh.UseVisualStyleBackColor = true;
            // 
            // label_status
            // 
            label_status.AutoSize = true;
            label_status.Font = new Font("Microsoft YaHei UI", 13F, FontStyle.Bold);
            label_status.ForeColor = Color.Gray;
            label_status.Location = new Point(16, 236);
            label_status.Name = "label_status";
            label_status.Size = new Size(82, 25);
            label_status.TabIndex = 10;
            label_status.Text = "● 空闲";
            // 
            // button1
            // 
            button1.Font = new Font("Microsoft YaHei UI", 12F);
            button1.Location = new Point(16, 270);
            button1.Name = "button1";
            button1.Size = new Size(100, 36);
            button1.TabIndex = 0;
            button1.Text = "开始采集";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Font = new Font("Microsoft YaHei UI", 12F);
            button2.Location = new Point(126, 270);
            button2.Name = "button2";
            button2.Size = new Size(140, 36);
            button2.TabIndex = 1;
            button2.Text = "停止采集并保存";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // button3
            // 
            button3.Font = new Font("Microsoft YaHei UI", 12F);
            button3.Location = new Point(276, 270);
            button3.Name = "button3";
            button3.Size = new Size(100, 36);
            button3.TabIndex = 2;
            button3.Text = "读取TXT";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // formsPlot1
            // 
            formsPlot1.Location = new Point(16, 320);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(700, 340);
            formsPlot1.TabIndex = 17;
            // 
            // UserControl_CollectPage
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(46, 51, 73);
            Controls.Add(formsPlot1);
            Controls.Add(checkBox_autoRefresh);
            Controls.Add(comboBox_mode);
            Controls.Add(label_mode);
            Controls.Add(label_remoteIP);
            Controls.Add(textBox_remoteIP);
            Controls.Add(label_status);
            Controls.Add(button_connect);
            Controls.Add(button3);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(textBox_port);
            Controls.Add(label_port);
            Controls.Add(comboBox_localIP);
            Controls.Add(label_localIP);
            Controls.Add(comboBox_protocol);
            Controls.Add(label_protocol);
            Controls.Add(label_title);
            Name = "UserControl_CollectPage";
            Size = new Size(740, 680);
            ResumeLayout(false);
            PerformLayout();
        }

        private Button button1;
        private Button button2;
        private Button button3;
        private Button button_connect;
        private Label label_title;
        private Label label_protocol;
        private ComboBox comboBox_protocol;
        private Label label_localIP;
        private ComboBox comboBox_localIP;
        private Label label_port;
        private TextBox textBox_port;
        private Label label_remoteIP;
        private TextBox textBox_remoteIP;
        private Label label_status;
        private Label label_mode;
        private ComboBox comboBox_mode;
        private CheckBox checkBox_autoRefresh;
        private ScottPlot.WinForms.FormsPlot formsPlot1;
    }
}