namespace Show_SanTongDaoXinHao
{
    partial class UserControl_AnalysisPage
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            button_draw = new Button();
            button_stop = new Button();
            button_clear = new Button();
            button_FFT = new Button();
            groupBox1 = new GroupBox();
            label_CH1_f = new Label();
            label11 = new Label();
            label_CH1_RMS = new Label();
            label9 = new Label();
            label_CH1_PP = new Label();
            label7 = new Label();
            label_CH1_Min = new Label();
            label5 = new Label();
            label_CH1_Max = new Label();
            label3 = new Label();
            label_CH1_Mean = new Label();
            label1 = new Label();
            groupBox2 = new GroupBox();
            label_CH2_f = new Label();
            label23 = new Label();
            label_CH2_RMS = new Label();
            label21 = new Label();
            label_CH2_PP = new Label();
            label19 = new Label();
            label_CH2_Min = new Label();
            label17 = new Label();
            label_CH2_Max = new Label();
            label15 = new Label();
            label_CH2_Mean = new Label();
            label13 = new Label();
            groupBox3 = new GroupBox();
            label_CH3_f = new Label();
            label35 = new Label();
            label_CH3_RMS = new Label();
            label33 = new Label();
            label_CH3_PP = new Label();
            label31 = new Label();
            label_CH3_Min = new Label();
            label29 = new Label();
            label_CH3_Max = new Label();
            label27 = new Label();
            label_CH3_Mean = new Label();
            label25 = new Label();
            formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // button_draw
            // 
            button_draw.Location = new Point(985, 169);
            button_draw.Name = "button_draw";
            button_draw.Size = new Size(112, 56);
            button_draw.TabIndex = 0;
            button_draw.Text = "绘图";
            button_draw.UseVisualStyleBackColor = true;
            button_draw.Click += button_draw_Click;
            // 
            // button_stop
            // 
            button_stop.Location = new Point(1134, 169);
            button_stop.Name = "button_stop";
            button_stop.Size = new Size(112, 49);
            button_stop.TabIndex = 1;
            button_stop.Text = "停止";
            button_stop.UseVisualStyleBackColor = true;
            button_stop.Click += button_stop_Click;
            // 
            // button_clear
            // 
            button_clear.Location = new Point(985, 295);
            button_clear.Name = "button_clear";
            button_clear.Size = new Size(112, 58);
            button_clear.TabIndex = 2;
            button_clear.Text = "清除";
            button_clear.UseVisualStyleBackColor = true;
            button_clear.Click += button_clear_Click;
            // 
            // button_FFT
            // 
            button_FFT.Location = new Point(1134, 292);
            button_FFT.Name = "button_FFT";
            button_FFT.Size = new Size(134, 65);
            button_FFT.TabIndex = 3;
            button_FFT.Text = "FFT分析";
            button_FFT.UseVisualStyleBackColor = true;
            button_FFT.Click += button_FFT_Click_1;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label_CH1_f);
            groupBox1.Controls.Add(label11);
            groupBox1.Controls.Add(label_CH1_RMS);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(label_CH1_PP);
            groupBox1.Controls.Add(label7);
            groupBox1.Controls.Add(label_CH1_Min);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label_CH1_Max);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(label_CH1_Mean);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(49, 605);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(400, 412);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "通道1";
            // 
            // label_CH1_f
            // 
            label_CH1_f.AutoSize = true;
            label_CH1_f.Location = new Point(268, 349);
            label_CH1_f.Name = "label_CH1_f";
            label_CH1_f.Size = new Size(110, 36);
            label_CH1_f.TabIndex = 11;
            label_CH1_f.Text = "label12";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(69, 365);
            label11.Name = "label11";
            label11.Size = new Size(110, 36);
            label11.TabIndex = 10;
            label11.Text = "label11";
            // 
            // label_CH1_RMS
            // 
            label_CH1_RMS.AutoSize = true;
            label_CH1_RMS.Location = new Point(284, 292);
            label_CH1_RMS.Name = "label_CH1_RMS";
            label_CH1_RMS.Size = new Size(110, 36);
            label_CH1_RMS.TabIndex = 9;
            label_CH1_RMS.Text = "label10";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(67, 293);
            label9.Name = "label9";
            label9.Size = new Size(94, 36);
            label9.TabIndex = 8;
            label9.Text = "label9";
            // 
            // label_CH1_PP
            // 
            label_CH1_PP.AutoSize = true;
            label_CH1_PP.Location = new Point(277, 223);
            label_CH1_PP.Name = "label_CH1_PP";
            label_CH1_PP.Size = new Size(94, 36);
            label_CH1_PP.TabIndex = 7;
            label_CH1_PP.Text = "label8";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(40, 223);
            label7.Name = "label7";
            label7.Size = new Size(94, 36);
            label7.TabIndex = 6;
            label7.Text = "label7";
            // 
            // label_CH1_Min
            // 
            label_CH1_Min.AutoSize = true;
            label_CH1_Min.Location = new Point(274, 162);
            label_CH1_Min.Name = "label_CH1_Min";
            label_CH1_Min.Size = new Size(94, 36);
            label_CH1_Min.TabIndex = 5;
            label_CH1_Min.Text = "label6";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(55, 165);
            label5.Name = "label5";
            label5.Size = new Size(94, 36);
            label5.TabIndex = 4;
            label5.Text = "label5";
            // 
            // label_CH1_Max
            // 
            label_CH1_Max.AutoSize = true;
            label_CH1_Max.Location = new Point(257, 110);
            label_CH1_Max.Name = "label_CH1_Max";
            label_CH1_Max.Size = new Size(94, 36);
            label_CH1_Max.TabIndex = 3;
            label_CH1_Max.Text = "label4";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(35, 110);
            label3.Name = "label3";
            label3.Size = new Size(94, 36);
            label3.TabIndex = 2;
            label3.Text = "label3";
            // 
            // label_CH1_Mean
            // 
            label_CH1_Mean.AutoSize = true;
            label_CH1_Mean.Location = new Point(257, 49);
            label_CH1_Mean.Name = "label_CH1_Mean";
            label_CH1_Mean.Size = new Size(94, 36);
            label_CH1_Mean.TabIndex = 1;
            label_CH1_Mean.Text = "label2";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(28, 49);
            label1.Name = "label1";
            label1.Size = new Size(94, 36);
            label1.TabIndex = 0;
            label1.Text = "label1";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(label_CH2_f);
            groupBox2.Controls.Add(label23);
            groupBox2.Controls.Add(label_CH2_RMS);
            groupBox2.Controls.Add(label21);
            groupBox2.Controls.Add(label_CH2_PP);
            groupBox2.Controls.Add(label19);
            groupBox2.Controls.Add(label_CH2_Min);
            groupBox2.Controls.Add(label17);
            groupBox2.Controls.Add(label_CH2_Max);
            groupBox2.Controls.Add(label15);
            groupBox2.Controls.Add(label_CH2_Mean);
            groupBox2.Controls.Add(label13);
            groupBox2.Location = new Point(485, 605);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(400, 412);
            groupBox2.TabIndex = 7;
            groupBox2.TabStop = false;
            groupBox2.Text = "通道2";
            // 
            // label_CH2_f
            // 
            label_CH2_f.AutoSize = true;
            label_CH2_f.Location = new Point(261, 340);
            label_CH2_f.Name = "label_CH2_f";
            label_CH2_f.Size = new Size(110, 36);
            label_CH2_f.TabIndex = 0;
            label_CH2_f.Text = "label24";
            label_CH2_f.Click += label24_Click;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(78, 349);
            label23.Name = "label23";
            label23.Size = new Size(110, 36);
            label23.TabIndex = 10;
            label23.Text = "label23";
            label23.Click += label23_Click_1;
            // 
            // label_CH2_RMS
            // 
            label_CH2_RMS.AutoSize = true;
            label_CH2_RMS.Location = new Point(271, 294);
            label_CH2_RMS.Name = "label_CH2_RMS";
            label_CH2_RMS.Size = new Size(110, 36);
            label_CH2_RMS.TabIndex = 9;
            label_CH2_RMS.Text = "label22";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(70, 298);
            label21.Name = "label21";
            label21.Size = new Size(110, 36);
            label21.TabIndex = 8;
            label21.Text = "label21";
            // 
            // label_CH2_PP
            // 
            label_CH2_PP.AutoSize = true;
            label_CH2_PP.Location = new Point(268, 230);
            label_CH2_PP.Name = "label_CH2_PP";
            label_CH2_PP.Size = new Size(110, 36);
            label_CH2_PP.TabIndex = 7;
            label_CH2_PP.Text = "label20";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(95, 241);
            label19.Name = "label19";
            label19.Size = new Size(110, 36);
            label19.TabIndex = 6;
            label19.Text = "label19";
            // 
            // label_CH2_Min
            // 
            label_CH2_Min.AutoSize = true;
            label_CH2_Min.Location = new Point(261, 177);
            label_CH2_Min.Name = "label_CH2_Min";
            label_CH2_Min.Size = new Size(110, 36);
            label_CH2_Min.TabIndex = 5;
            label_CH2_Min.Text = "label18";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(72, 178);
            label17.Name = "label17";
            label17.Size = new Size(110, 36);
            label17.TabIndex = 4;
            label17.Text = "label17";
            // 
            // label_CH2_Max
            // 
            label_CH2_Max.AutoSize = true;
            label_CH2_Max.Location = new Point(241, 117);
            label_CH2_Max.Name = "label_CH2_Max";
            label_CH2_Max.Size = new Size(110, 36);
            label_CH2_Max.TabIndex = 3;
            label_CH2_Max.Text = "label16";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(78, 116);
            label15.Name = "label15";
            label15.Size = new Size(110, 36);
            label15.TabIndex = 2;
            label15.Text = "label15";
            // 
            // label_CH2_Mean
            // 
            label_CH2_Mean.AutoSize = true;
            label_CH2_Mean.Location = new Point(241, 59);
            label_CH2_Mean.Name = "label_CH2_Mean";
            label_CH2_Mean.Size = new Size(110, 36);
            label_CH2_Mean.TabIndex = 1;
            label_CH2_Mean.Text = "label14";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(58, 52);
            label13.Name = "label13";
            label13.Size = new Size(110, 36);
            label13.TabIndex = 0;
            label13.Text = "label13";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(label_CH3_f);
            groupBox3.Controls.Add(label35);
            groupBox3.Controls.Add(label_CH3_RMS);
            groupBox3.Controls.Add(label33);
            groupBox3.Controls.Add(label_CH3_PP);
            groupBox3.Controls.Add(label31);
            groupBox3.Controls.Add(label_CH3_Min);
            groupBox3.Controls.Add(label29);
            groupBox3.Controls.Add(label_CH3_Max);
            groupBox3.Controls.Add(label27);
            groupBox3.Controls.Add(label_CH3_Mean);
            groupBox3.Controls.Add(label25);
            groupBox3.Location = new Point(921, 605);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(400, 412);
            groupBox3.TabIndex = 8;
            groupBox3.TabStop = false;
            groupBox3.Text = "通道3";
            // 
            // label_CH3_f
            // 
            label_CH3_f.AutoSize = true;
            label_CH3_f.Location = new Point(257, 349);
            label_CH3_f.Name = "label_CH3_f";
            label_CH3_f.Size = new Size(110, 36);
            label_CH3_f.TabIndex = 11;
            label_CH3_f.Text = "label36";
            // 
            // label35
            // 
            label35.AutoSize = true;
            label35.Location = new Point(66, 349);
            label35.Name = "label35";
            label35.Size = new Size(110, 36);
            label35.TabIndex = 10;
            label35.Text = "label35";
            // 
            // label_CH3_RMS
            // 
            label_CH3_RMS.AutoSize = true;
            label_CH3_RMS.Location = new Point(263, 274);
            label_CH3_RMS.Name = "label_CH3_RMS";
            label_CH3_RMS.Size = new Size(110, 36);
            label_CH3_RMS.TabIndex = 9;
            label_CH3_RMS.Text = "label34";
            // 
            // label33
            // 
            label33.AutoSize = true;
            label33.Location = new Point(72, 290);
            label33.Name = "label33";
            label33.Size = new Size(110, 36);
            label33.TabIndex = 8;
            label33.Text = "label33";
            // 
            // label_CH3_PP
            // 
            label_CH3_PP.AutoSize = true;
            label_CH3_PP.Location = new Point(246, 218);
            label_CH3_PP.Name = "label_CH3_PP";
            label_CH3_PP.Size = new Size(110, 36);
            label_CH3_PP.TabIndex = 7;
            label_CH3_PP.Text = "label32";
            // 
            // label31
            // 
            label31.AutoSize = true;
            label31.Location = new Point(38, 218);
            label31.Name = "label31";
            label31.Size = new Size(110, 36);
            label31.TabIndex = 6;
            label31.Text = "label31";
            // 
            // label_CH3_Min
            // 
            label_CH3_Min.AutoSize = true;
            label_CH3_Min.Location = new Point(257, 161);
            label_CH3_Min.Name = "label_CH3_Min";
            label_CH3_Min.Size = new Size(110, 36);
            label_CH3_Min.TabIndex = 5;
            label_CH3_Min.Text = "label30";
            // 
            // label29
            // 
            label29.AutoSize = true;
            label29.Location = new Point(70, 166);
            label29.Name = "label29";
            label29.Size = new Size(110, 36);
            label29.TabIndex = 4;
            label29.Text = "label29";
            // 
            // label_CH3_Max
            // 
            label_CH3_Max.AutoSize = true;
            label_CH3_Max.Location = new Point(233, 116);
            label_CH3_Max.Name = "label_CH3_Max";
            label_CH3_Max.Size = new Size(110, 36);
            label_CH3_Max.TabIndex = 3;
            label_CH3_Max.Text = "label28";
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Location = new Point(50, 117);
            label27.Name = "label27";
            label27.Size = new Size(110, 36);
            label27.TabIndex = 2;
            label27.Text = "label27";
            // 
            // label_CH3_Mean
            // 
            label_CH3_Mean.AutoSize = true;
            label_CH3_Mean.Location = new Point(215, 63);
            label_CH3_Mean.Name = "label_CH3_Mean";
            label_CH3_Mean.Size = new Size(110, 36);
            label_CH3_Mean.TabIndex = 1;
            label_CH3_Mean.Text = "label26";
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(45, 59);
            label25.Name = "label25";
            label25.Size = new Size(110, 36);
            label25.TabIndex = 0;
            label25.Text = "label25";
            // 
            // formsPlot1
            // 
            formsPlot1.Location = new Point(49, 38);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(901, 509);
            formsPlot1.TabIndex = 9;
            // 
            // UserControl_AnalysisPage
            // 
            AutoScaleDimensions = new SizeF(17F, 36F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            Controls.Add(formsPlot1);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(button_FFT);
            Controls.Add(button_clear);
            Controls.Add(button_stop);
            Controls.Add(button_draw);
            Name = "UserControl_AnalysisPage";
            Size = new Size(1454, 1062);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button button_draw;
        private Button button_stop;
        private Button button_clear;
        private Button button_FFT;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private ScottPlot.WinForms.FormsPlot formsPlot1;
        private Label label_CH1_Mean;
        private Label label1;
        private Label label_CH1_f;
        private Label label11;
        private Label label_CH1_RMS;
        private Label label9;
        private Label label_CH1_PP;
        private Label label7;
        private Label label_CH1_Min;
        private Label label5;
        private Label label_CH1_Max;
        private Label label3;
        private Label label23;
        private Label label_CH2_RMS;
        private Label label21;
        private Label label_CH2_PP;
        private Label label19;
        private Label label_CH2_Min;
        private Label label17;
        private Label label_CH2_Max;
        private Label label15;
        private Label label_CH2_Mean;
        private Label label13;
        private Label label_CH2_f;
        private Label label_CH3_f;
        private Label label35;
        private Label label_CH3_RMS;
        private Label label33;
        private Label label_CH3_PP;
        private Label label31;
        private Label label_CH3_Min;
        private Label label29;
        private Label label_CH3_Max;
        private Label label27;
        private Label label_CH3_Mean;
        private Label label25;
    }
}
