using System;
using System.Text;

namespace WillPlusManager
{
    public class WS2 {
        private byte[] script;
        private byte[] CuttedScript;
        private bool Initialized = false;
        private bool filter;
        public WS2(byte[] Script, bool Filter) {
            script = Decrypt(Script);
            Initialized = true;
            filter = Filter;
        }

        public WS2String[] Import() {
            CuttedScript = new byte[script.Length];
            script.CopyTo(CuttedScript, 0);
            WS2String[] Rst = new WS2String[0];
            byte[] TxtEntry = StringParse("char\x0");
            byte[] CharName = StringParse("%LC");
            byte[] NewCharName = StringParse("%LF");
            byte[] ChoiceOP = new byte[] { 0x00, 0xFF, 0x0F, 0x02 };//0xFF = any, the opcode is just 0x0F02
            WS2String Actor = null;
            bool AskMerged = true;
            for (int i = 0; i < CuttedScript.Length; i++) {
                if (Equals(CuttedScript, ChoiceOP, i)) {
                    i += ChoiceOP.Length;
                    if (CuttedScript[i] == 0x00)
                        continue;

                    bool Continue = true;
                    while (i < CuttedScript.Length && Continue) {
                        i += 2;
                        WS2String str = GetString(i);
                        i += 4;

                        while (CuttedScript[i] != 0x00)
                            i++;

                        if (CuttedScript[++i] == 0xFF)
                            Continue = false;

                        WS2String[] tmp = new WS2String[Rst.Length + 1];
                        Rst.CopyTo(tmp, 0);
                        tmp[Rst.Length] = str;
                        Rst = tmp;
                    }
                }
                if (Equals(CuttedScript, CharName, i)) {
                    i += CharName.Length;
                    Actor = GetString(i);
                    AskMerged = false;
                }
                if (Equals(CuttedScript, NewCharName, i)) {
                    i += NewCharName.Length;
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
                        Rst[i].SetString(str.Substring(0, str.Length - "%K%P".Length));
                        Rst[i].Sufix = "%K%P";
                    }
                    if (Rst[i].HaveActor) {
                        if (Rst[i].Actor.String.StartsWith("%LC")) {
                            Rst[i].Actor.Prefix = "%LC";
                            Rst[i].Actor.SetString(Rst[i].Actor.String.Substring(3, Rst[i].Actor.String.Length - 3));
                        }
                        if (Rst[i].Actor.String.StartsWith("%LF")) {
                            Rst[i].Actor.Prefix = "%LF";
                            Rst[i].Actor.SetString(Rst[i].Actor.String.Substring(3, Rst[i].Actor.String.Length - 3));
                        }
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
            return Encrypt(OutScript);
        }

        private byte[] WriteString(byte[] outScript, WS2String Entry) {
            if (Entry.String.Length > Entry.str.Length)
                throw new Exception("String \"" + Entry.String + "\" Are too big.");
            while (Entry.String.Length < Entry.str.Length)
                Entry.String += @" ";
            byte[] String = StringParse(Entry.Prefix + Entry.String + Entry.Sufix);
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
            WS2String ws2 = new WS2String();
            ws2.Position = pos;
            ws2.SetString(_str);
            return ws2;
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
            if (Var2.Length + Pos > Var1.Length)
                return false;
            for (int i = 0; i < Var2.Length; i++)
                if (Var1[i + Pos] != Var2[i] && Var2[i] != 0xFF)
                    return false;
            return true;
        }

        public byte[] Decrypt(byte[] data) {
            byte[] NewData = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                NewData[i] = RotateRight(data[i], 2);
            return NewData;
        }
        public byte[] Encrypt(byte[] data) {
            byte[] NewData = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                NewData[i] = RotateLeft(data[i], 2);
            return NewData;
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
        internal string str;
        internal void SetString(string txt) {
            str = txt;
            String = txt;
        }
        internal int Position;
        public string Sufix { get; internal set; }
        public string Prefix { get; internal set; }

        public bool HaveActor { get; internal set;}
        public WS2String Actor;
    }
}
