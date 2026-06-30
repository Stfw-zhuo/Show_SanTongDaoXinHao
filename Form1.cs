namespace Show_SanTongDaoXinHao
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            OpenPage(new UserControl_HomePage());
        }

        private void OpenPage(UserControl page)
        {
            panelMain.Controls.Clear();

            page.Dock = DockStyle.Fill;

            panelMain.Controls.Add(page);
        }

        private void button_shouye_Click(object sender, EventArgs e)
        {
            OpenPage(new UserControl_HomePage());
        }

        private void button_caiji_Click(object sender, EventArgs e)
        {
            OpenPage(new UserControl_CollectPage());
        }

        private void button_fenxi_Click(object sender, EventArgs e)
        {
            OpenPage(new UserControl_AnalysisPage());
        }

        private void button_bangzhu_Click(object sender, EventArgs e)
        {
            OpenPage(new UserControl_HelpPage());
        }
    }
}
