﻿using System;
using System.Text;

namespace WillPlusManager
{
    public class WS2 {
        private byte[] script;
        private byte[] CuttedScript;
        private bool Initialized = false;
        private bool filter;
        public WS2(byte[] Script, bool Filter) {
            script = Decrypt(ref Script);
            Initialized = true;
            filter = Filter;
        }

        public WS2String[] Import() {
            CuttedScript = new byte[script.Length];
            script.CopyTo(CuttedScript, 0);
            WS2String[] Rst = new WS2String[0];
            byte[] TxtEntry = StringParse("char\x0");
            byte[] CharName = StringParse("%LC");
            WS2String Actor = null;
            bool AskMerged = true;
            for (int i = 0; i < CuttedScript.Length; i++) {
                if (Equals(CuttedScript, CharName, i)) {
                    i += CharName.Length;
                    Actor = GetString(i);
                    AskMerged = false;
                }
                if (Equals(CuttedScript, TxtEntry, i)) {
                    i += TxtEntry.Length;
                    WS2String str = GetString(i);
                    if (!AskMerged) {
                        str.Actor = Actor;
                        str.HaveActor = true;
                        AskMerged = true;
                    }
                    WS2String[] tmp = new WS2String[Rst.Length + 1];
                    Rst.CopyTo(tmp, 0);
                    tmp[Rst.Length] = str;
                    Rst = tmp;
                }
            }
            if (filter) {
                for (int i = 0; i < Rst.Length; i++) {
                    string str = Rst[i].String;
                    if (str.EndsWith("%K%P")) {
                        Rst[i].String = str.Substring(0, str.Length - "%K%P".Length);
                        Rst[i].Prefix = "%K%P";
                    }
                }
            }
            return Rst;
        }

        public byte[] Export(WS2String[] Entries) {
            if (!Initialized)
                throw new Exception("You Need Import a Script before Export");
            byte[] OutScript = new byte[CuttedScript.Length];
            CuttedScript.CopyTo(OutScript, 0);
            for (int i = Entries.Length - 1; i > -1; i--) {
                if (!Entries[i].initialized)
                    throw new Exception("You Can't Create String Entry");
                OutScript = WriteString(OutScript, Entries[i]);
            }
            return Encrypt(ref OutScript);
        }

        private byte[] WriteString(byte[] outScript, WS2String Entry) {
            byte[] String = StringParse(Entry.String + Entry.Prefix);
            outScript = InsertArray(outScript, String, Entry.Position);
            if (Entry.HaveActor)
                outScript = WriteString(outScript, Entry.Actor);
            return outScript;
        }

        private byte[] InsertArray(byte[] Data, byte[] DataToInsert, int Pos) {
            byte[] tmp = CutAt(Data, Pos);
            byte[] tmp2 = CutAfter(Data, Pos);
            byte[] Rst = new byte[Data.Length + DataToInsert.Length];
            tmp.CopyTo(Rst, 0);
            DataToInsert.CopyTo(Rst, tmp.Length);
            tmp2.CopyTo(Rst, tmp.Length + DataToInsert.Length);
            return Rst;
        }

        private WS2String GetString(int pos) {
            int length = 0;
            while (CuttedScript[pos + length] != 0x0) {
                length++;
            }
            byte[] str = new byte[length];
            for (int ind = 0; ind < length; ind++)
                str[ind] = CuttedScript[pos + ind];
            string _str = ByteParse(str);
            CuttedScript = CutRegion(CuttedScript, pos, length);

            return new WS2String() {
                String = _str,
                Position = pos
            };
        }

        private string ByteParse(byte[] data) {
            return Encoding.GetEncoding(932).GetString(data);
        }
        private byte[] StringParse(string Str) {
            return Encoding.GetEncoding(932).GetBytes(Str);
        }
        private byte[] CutRegion(byte[] Data, int pos, int length) {
            byte[] tmp = CutAt(Data, pos);
            byte[] tmp2 = CutAfter(Data, pos + length);
            byte[] rst = new byte[tmp.Length + tmp2.Length];
            tmp.CopyTo(rst, 0);
            tmp2.CopyTo(rst, tmp.Length);
            return rst;
        }
        private byte[] CutAt(byte[] data, int pos) {
            byte[] rst = new byte[pos];
            for (int i = 0; i < pos; i++)
                rst[i] = data[i];
            return rst;
        }
        private byte[] CutAfter(byte[] data, int pos) {
            byte[] rst = new byte[data.Length - pos];
            for (int i = pos; i < data.Length; i++)
                rst[i-pos] = data[i];
            return rst;
        }
        private bool Equals(byte[] Var1, byte[] Var2, int Pos) {
            if (Var2.Length > Var1.Length)
                return false;
            for (int i = 0; i < Var2.Length; i++)
                if (Var1[i + Pos] != Var2[i])
                    return false;
            return true;
        }

        private byte[] Decrypt(ref byte[] data) {
            for (int i = 0; i < data.Length; i++)
                data[i] = RotateRight(data[i], 2);
            return data;
        }
        private byte[] Encrypt(ref byte[] data) {
            for (int i = 0; i < data.Length; i++)
                data[i] = RotateLeft(data[i], 2);
            return data;
        }
        private byte RotateLeft(byte value, int count) {
            return (byte)((value << count) | (value >> (8 - count)));
        }

        private byte RotateRight(byte value, int count) {
            return (byte)((value >> count) | (value << (8 - count)));
        }
    }

    public class WS2String {
        internal bool initialized = false;
        internal WS2String() {
            initialized = true;
        }
        public string String;
        internal int Position;
        public string Prefix { get; internal set; }

        public bool HaveActor { get; internal set;}
        public WS2String Actor;
    }
}