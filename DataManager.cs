using System;
using System.Collections.Generic;
using System.IO;

namespace Show_SanTongDaoXinHao
{
    public static class DataManager
    {
        // 三个通道的数据（差分值，可为负）
        private static readonly List<short> channel1 = new();
        private static readonly List<short> channel2 = new();
        private static readonly List<short> channel3 = new();

        // W5500 帧缓冲区
        private const int FRAME_SAMPLE_COUNT = 1024;

        private struct W5500Sample
        {
            public short CH1;
            public short CH2;
            public short CH3;
            public bool Valid;

            public void Set(short ch1, short ch2, short ch3)
            {
                CH1 = ch1; CH2 = ch2; CH3 = ch3; Valid = true;
            }
        }

        private static readonly W5500Sample[][][] frameBuf = new W5500Sample[2][][]
        {
            new W5500Sample[3][], new W5500Sample[3][]
        };

        private static readonly bool[][][] segReceived = new bool[2][][]
        {
            new bool[3][], new bool[3][]
        };

        private static DateTime lastPacketTime = DateTime.MinValue;
        private static readonly TimeSpan FLUSH_TIMEOUT = TimeSpan.FromMilliseconds(500);

        static DataManager()
        {
            for (int h = 0; h < 2; h++)
                for (int s = 0; s < 3; s++)
                {
                    frameBuf[h][s] = new W5500Sample[FRAME_SAMPLE_COUNT];
                    segReceived[h][s] = new bool[FRAME_SAMPLE_COUNT];
                }
        }

        //========== 大端转有符号 int16 ==========
        private static short ReadBigEndianInt16(byte[] data, int offset)
        {
            return (short)((data[offset] << 8) | data[offset + 1]);
        }

        //========== 清空 ==========
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
                for (int s = 0; s < 3; s++)
                    for (int i = 0; i < FRAME_SAMPLE_COUNT; i++)
                    {
                        frameBuf[h][s][i].Valid = false;
                        segReceived[h][s][i] = false;
                    }
            lastPacketTime = DateTime.MinValue;
        }

        public static void FlushAllPending()
        {
            lock (channel1)
            {
                FlushHalfFrame(0);
                FlushHalfFrame(1);
                lastPacketTime = DateTime.MinValue;
            }
        }

        private static void FlushHalfFrame(int half)
        {
            int start = half * 512;
            bool hasAny = false;
            for (int s = 0; s < 3; s++)
                for (int i = 0; i < FRAME_SAMPLE_COUNT; i++)
                    if (segReceived[half][s][i]) { hasAny = true; break; }
            if (!hasAny) return;

            for (int si = start; si < start + 512; si++)
            {
                int lo = si - start;
                int segIdx = lo / 170;
                if (lo >= 340) segIdx = 2;
                int sub = lo - segIdx * 170;
                ref W5500Sample s = ref frameBuf[half][segIdx][sub];
                channel1.Add(s.Valid ? s.CH1 : (short)0);
                channel2.Add(s.Valid ? s.CH2 : (short)0);
                channel3.Add(s.Valid ? s.CH3 : (short)0);
            }
        }

        //========== 逐点添加 ==========
        public static void AppendData(short ch1, short ch2, short ch3)
        {
            lock (channel1)
            {
                channel1.Add(ch1);
                channel2.Add(ch2);
                channel3.Add(ch3);
            }
        }

        //========== 旧协议（不做大端转换，保持兼容）==========
        public static void AppendPacket(byte[] packet)
        {
            if (packet == null || packet.Length != 1440) return;
            lock (channel1)
            {
                for (int i = 0; i < 720; i += 3)
                {
                    channel1.Add((short)BitConverter.ToUInt16(packet, i * 2));
                    channel2.Add((short)BitConverter.ToUInt16(packet, (i + 1) * 2));
                    channel3.Add((short)BitConverter.ToUInt16(packet, (i + 2) * 2));
                }
            }
        }

        //========== TXT 保存/读取 ==========
        public static void SaveToTxt(string fileName)
        {
            lock (channel1)
            {
                using StreamWriter sw = new(fileName);
                for (int i = 0; i < channel1.Count; i++)
                    sw.WriteLine($"{channel1[i]},{channel2[i]},{channel3[i]}");
            }
        }

        public static void LoadFromTxt(string fileName)
        {
            ClearData();
            string[] lines = File.ReadAllLines(fileName);
            lock (channel1)
            {
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] s = line.Split(',');
                    if (s.Length != 3) continue;
                    channel1.Add(short.Parse(s[0]));
                    channel2.Add(short.Parse(s[1]));
                    channel3.Add(short.Parse(s[2]));
                }
            }
        }

        //========== 读取通道数据 ==========
        public static IReadOnlyList<short> GetChannel1() { lock (channel1) return channel1.ToList(); }
        public static IReadOnlyList<short> GetChannel2() { lock (channel1) return channel2.ToList(); }
        public static IReadOnlyList<short> GetChannel3() { lock (channel1) return channel3.ToList(); }
        public static int GetDataCount() { lock (channel1) return channel1.Count; }

        //========== W5500 协议解析 (大端 int16) ==========
        public static bool AppendW5500Packet(byte[] packet)
        {
            if (packet == null || packet.Length < 12) return false;

            // 大端解析包头
            // V2.3: seq(2B BE) | half(1B) | seg(1B) | offset(2B BE)
            ushort seqBig        = (ushort)((packet[0] << 8) | packet[1]);
            byte   half          = packet[2];
            byte   seg           = packet[3];
            ushort offsetBig     = (ushort)((packet[4] << 8) | packet[5]);

            if (half > 1 || seg > 2) return false;

            int sampleCount = (packet.Length - 6) / 6;
            if (sampleCount <= 0) return false;
            if (offsetBig + sampleCount > FRAME_SAMPLE_COUNT) return false;

            lock (channel1)
            {
                for (int i = 0; i < sampleCount; i++)
                {
                    int pos = 6 + i * 6;
                    short ch1 = ReadBigEndianInt16(packet, pos);
                    short ch2 = ReadBigEndianInt16(packet, pos + 2);
                    short ch3 = ReadBigEndianInt16(packet, pos + 4);

                    int idx = offsetBig + i;
                    frameBuf[half][seg][idx].Set(ch1, ch2, ch3);
                    segReceived[half][seg][idx] = true;
                }

                lastPacketTime = DateTime.UtcNow;

                // 超时兜底：half0 完整但 half1 迟迟不来
                if (half == 0 && seg == 2)
                {
                    bool h1 = false;
                    for (int s = 0; s < 3; s++)
                        for (int i = 0; i < FRAME_SAMPLE_COUNT; i++)
                            if (segReceived[1][s][i]) { h1 = true; break; }
                    if (!h1) lastPacketTime = DateTime.UtcNow;
                }
                if (half == 0 && seg == 2 && DateTime.UtcNow - lastPacketTime > FLUSH_TIMEOUT)
                {
                    bool h1e = true;
                    for (int s = 0; s < 3; s++)
                        for (int i = 0; i < FRAME_SAMPLE_COUNT; i++)
                            if (segReceived[1][s][i]) { h1e = false; break; }
                    if (h1e) { FlushHalfFrame(0); ClearW5500Frame(); return true; }
                }

                // 完整帧
                if (half == 1 && seg == 2) { FlushCompleteFrame(); return true; }
            }

            return false;
        }

        private static void FlushCompleteFrame()
        {
            for (int h = 0; h < 2; h++)
            {
                int start = h * 512;
                for (int si = start; si < start + 512; si++)
                {
                    int lo = si - start;
                    int segIdx = lo / 170;
                    if (lo >= 340) segIdx = 2;
                    int sub = lo - segIdx * 170;
                    ref W5500Sample s = ref frameBuf[h][segIdx][sub];
                    channel1.Add(s.Valid ? s.CH1 : (short)0);
                    channel2.Add(s.Valid ? s.CH2 : (short)0);
                    channel3.Add(s.Valid ? s.CH3 : (short)0);
                }
            }
            ClearW5500Frame();
        }
    }
}