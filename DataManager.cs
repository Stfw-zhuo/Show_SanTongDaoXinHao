using System;
using System.Collections.Generic;
using System.IO;

namespace Show_SanTongDaoXinHao
{
    public static class DataManager
    {
        //=========================================
        // 三个通道的数据
        //=========================================

        private static readonly List<ushort> channel1 = new();
        private static readonly List<ushort> channel2 = new();
        private static readonly List<ushort> channel3 = new();

        //=========================================
        // 清空数据
        //=========================================

        public static void ClearData()
        {
            lock (channel1)
            {
                channel1.Clear();
                channel2.Clear();
                channel3.Clear();
            }
        }

        //=========================================
        // 添加一组三通道数据
        //=========================================

        public static void AppendData(
            ushort ch1,
            ushort ch2,
            ushort ch3)
        {
            lock (channel1)
            {
                channel1.Add(ch1);
                channel2.Add(ch2);
                channel3.Add(ch3);
            }
        }

        //=========================================
        // STM32 UDP数据包
        // 一个包固定1440Byte
        //=========================================

        public static void AppendPacket(byte[] packet)
        {
            if (packet == null)
                return;

            if (packet.Length != 1440)
                return;

            lock (channel1)
            {
                for (int i = 0; i < 720; i += 3)
                {
                    ushort ch1 = BitConverter.ToUInt16(packet, i * 2);

                    ushort ch2 = BitConverter.ToUInt16(packet, (i + 1) * 2);

                    ushort ch3 = BitConverter.ToUInt16(packet, (i + 2) * 2);

                    channel1.Add(ch1);
                    channel2.Add(ch2);
                    channel3.Add(ch3);
                }
            }
        }

        //=========================================
        // 保存TXT
        // 一行：
        // ch1,ch2,ch3
        //=========================================

        public static void SaveToTxt(string fileName)
        {
            lock (channel1)
            {
                using StreamWriter sw = new StreamWriter(fileName);

                for (int i = 0; i < channel1.Count; i++)
                {
                    sw.WriteLine(
                        $"{channel1[i]},{channel2[i]},{channel3[i]}");
                }
            }
        }

        //=========================================
        // 读取TXT
        //=========================================

        public static void LoadFromTxt(string fileName)
        {
            ClearData();

            string[] lines = File.ReadAllLines(fileName);

            lock (channel1)
            {
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    string[] s = line.Split(',');

                    if (s.Length != 3)
                        continue;

                    channel1.Add(Convert.ToUInt16(s[0]));
                    channel2.Add(Convert.ToUInt16(s[1]));
                    channel3.Add(Convert.ToUInt16(s[2]));
                }
            }
        }

        //=========================================
        // AnalysisPage读取
        //=========================================

        public static IReadOnlyList<ushort> GetChannel1()
        {
            lock (channel1)
            {
                return channel1.ToList();
            }
        }

        public static IReadOnlyList<ushort> GetChannel2()
        {
            lock (channel1)
            {
                return channel2.ToList();
            }
        }

        public static IReadOnlyList<ushort> GetChannel3()
        {
            lock (channel1)
            {
                return channel3.ToList();
            }
        }

        //=========================================
        // 当前点数
        //=========================================

        public static int GetDataCount()
        {
            lock (channel1)
            {
                return channel1.Count;
            }
        }
    }
}