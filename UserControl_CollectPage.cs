using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Show_SanTongDaoXinHao
{
    public partial class UserControl_CollectPage : UserControl
    {
        private UdpClient? udpClient;

        private bool isReceiving = false;

        public UserControl_CollectPage()
        {
            InitializeComponent();
        }

        //---------------------------------------------------
        // 开始采集
        //---------------------------------------------------
        private void button1_Click(object sender, EventArgs e)
        {
            if (isReceiving)
            {
                MessageBox.Show("已经正在采集！");
                return;
            }

            DataManager.ClearData();

            isReceiving = true;

            Task.Run(() => ReceiveData());

            MessageBox.Show("开始采集。");
        }

        //---------------------------------------------------
        // 停止采集并保存
        //---------------------------------------------------
        private void button2_Click(object sender, EventArgs e)
        {
            if (!isReceiving)
            {
                MessageBox.Show("当前没有采集。");
                return;
            }

            isReceiving = false;

            try
            {
                udpClient?.Close();
            }
            catch
            {
            }

            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "保存采集数据";

            dialog.Filter = "文本文件 (*.txt)|*.txt";

            dialog.FileName = "Data.txt";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DataManager.SaveToTxt(dialog.FileName);

                MessageBox.Show("保存完成！");
            }
        }

        //---------------------------------------------------
        // 读取TXT
        //---------------------------------------------------
        private void button3_Click(object sender, EventArgs e)
        {
            if (isReceiving)
            {
                MessageBox.Show("请先停止采集。");
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "读取采集数据";

            dialog.Filter = "文本文件 (*.txt)|*.txt";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DataManager.LoadFromTxt(dialog.FileName);

                MessageBox.Show("读取完成！");
            }
        }

        //---------------------------------------------------
        // UDP接收线程
        //---------------------------------------------------
        private void ReceiveData()
        {
            try
            {
                IPEndPoint localPoint =
                    new IPEndPoint(
                        IPAddress.Parse("192.168.1.110"),
                        8080);

                udpClient = new UdpClient(localPoint);

                while (isReceiving)
                {
                    IPEndPoint remote =
                        new IPEndPoint(IPAddress.Any, 0);

                    byte[] packet =
                        udpClient.Receive(ref remote);

                    if (packet.Length != 1440)
                    {
                        continue;
                    }

                    DataManager.AppendPacket(packet);
                }
            }
            catch (ObjectDisposedException)
            {
                // 正常关闭UDP时会进入这里
            }
            catch (Exception ex)
            {
                if (isReceiving)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            finally
            {
                try
                {
                    udpClient?.Close();
                }
                catch
                {
                }

                udpClient = null;

                isReceiving = false;
            }
        }
    }
}