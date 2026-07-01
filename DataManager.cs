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
        // W5500 帧缓冲区 (half0[1024] + half1[1024])
        //=========================================

        private const int FRAME_SAMPLE_COUNT = 1024;

        private struct W5500Sample
        {
            public ushort CH1;
            public ushort CH2;
            public ushort CH3;
            public bool Valid;

            public void Set(ushort ch1, ushort ch2, ushort ch3)
            {
                CH1 = ch1; CH2 = ch2; CH3 = ch3; Valid = true;
            }
        }

        private static readonly W5500Sample[][][] frameBuf = new W5500Sample[2][][]
        {
            new W5500Sample[3][],  // half0: seg0/seg1/seg2
            new W5500Sample[3][]   // half1: seg0/seg1/seg2
        };

        // 每个段接收标记
        private static readonly bool[][][] segReceived = new bool[2][][]
        {
            new bool[3][], new bool[3][]
        };

        static DataManager()
        {
            for (int h = 0; h < 2; h++)
            {
                for (int s = 0; s < 3; s++)
                {
                    frameBuf[h][s] = new W5500Sample[FRAME_SAMPLE_COUNT];
                    segReceived[h][s] = new bool[FRAME_SAMPLE_COUNT];
                }
            }
        }

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

            ClearW5500Frame();
        }

        private static void ClearW5500Frame()
        {
            for (int h = 0; h < 2; h++)
            {
                for (int s = 0; s < 3; s++)
                {
                    for (int i = 0; i < FRAME_SAMPLE_COUNT; i++)
                    {
                        frameBuf[h][s][i].Valid = false;
                        segReceived[h][s][i] = false;
                    }
                }
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

        //=========================================
        // W5500 UDP 包解析
        // 协议: [SEQ:2][FLAG:1][SEG:1][OFFSET:2] + [CH1:2][CH2:2][CH3:2] × N
        //=========================================

        public static bool AppendW5500Packet(byte[] packet)
        {
            if (packet == null)
                return false;

            if (packet.Length < 12)
                return false;

            // 实测格式: [SEQ:2][(SEG<<4)|FLAG:1][SEG_SAMPLES:1][OFFSET:2] + data
            //  byte2 = (seg_id << 4) | half_sel
            //   0x00=half0 seg0, 0x10=half0 seg1, 0x20=half0 seg2
            //   0x01=half1 seg0, 0x11=half1 seg1, 0x21=half1 seg2
            byte packed = packet[2];
            byte flag = (byte)(packed & 0x0F);
            byte seg  = (byte)(packed >> 4);
            byte segSampleCount = packet[3];   // 段内样点数 (170/172)
            ushort seq    = BitConverter.ToUInt16(packet, 0);
            ushort offset = BitConverter.ToUInt16(packet, 4);

            if (flag > 1 || seg > 2)
                return false;
            if (segSampleCount == 0)
                return false;

            // sampleCount 优先用包头里的 segSampleCount，其次用包长反算
            int sampleCount = segSampleCount;
            int calcCount = (packet.Length - 6) / 6;
            if (sampleCount > calcCount || sampleCount < 10)
                sampleCount = calcCount;

            if (offset + sampleCount > FRAME_SAMPLE_COUNT)
                return false;

            lock (channel1)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    int pos = 6 + i * 6;
                    ushort ch1 = BitConverter.ToUInt16(packet, pos);
                    ushort ch2 = BitConverter.ToUInt16(packet, pos + 2);
                    ushort ch3 = BitConverter.ToUInt16(packet, pos + 4);

                    int idx = offset + i;
                    frameBuf[flag][seg][idx].Set(ch1, ch2, ch3);
                    segReceived[flag][seg][idx] = true;
                }

                // 一帧完整 (half1 seg2 到达)
                if (flag == 1 && seg == 2)
                {
                    FlushCompleteFrame();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 一帧完整后，按 half0(0-511)→half1(512-1023) 顺序写入 channel
        /// </summary>
        private static void FlushCompleteFrame()
        {
            for (int h = 0; h < 2; h++)
            {
                int startSample = h * 512;     // half起始样点
                int endSample   = startSample + 512;

                for (int si = startSample; si < endSample; si++)
                {
                    // 定位到对应 seg
                    int localOffset = si - startSample;
                    int segIdx = localOffset / 170;  // 0/1
                    if (localOffset >= 340) segIdx = 2;

                    int subIdx = si - startSample - segIdx * 170;

                    // 容错：即使标记无效也追加
                    ref W5500Sample s = ref frameBuf[h][segIdx][subIdx];

                    channel1.Add(s.Valid ? s.CH1 : (ushort)0);
                    channel2.Add(s.Valid ? s.CH2 : (ushort)0);
                    channel3.Add(s.Valid ? s.CH3 : (ushort)0);
                }
            }

            ClearW5500Frame();
        }
    }
}