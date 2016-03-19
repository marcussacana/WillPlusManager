using System;
using System.Text;

namespace WillPlusManager {
    public class WP2Arc {
        static public File[] Open(byte[] Packget) {
            int Count = GetOffset(Packget, 0x0);
            int DataTable = GetOffset(Packget, 0x4) + 0x8; //+ HeaderSize
            File[] Files = new File[Count];
            int Now = 0;
            for (int i = 0x8; i < DataTable;) {
                int Length = GetOffset(Packget, i);
                int Postion = GetOffset(Packget, i + 4) + DataTable;
                i += 8;
                int pointer = i;
                byte[] Fname = new byte[0];
                while (!(Packget[pointer - 1] == 0x0 && Packget[pointer] == 0x0)) {
                    byte[] tmp = new byte[Fname.Length + 1];
                    Fname.CopyTo(tmp, 0);
                    tmp[Fname.Length] = Packget[pointer];
                    Fname = tmp;
                    pointer++;
                }
                string FileName = ByteParse(Fname);
                i = pointer + 2;
                byte[] FileData = new byte[Length];
                for (int ind = 0; ind < Length; ind++)
                    FileData[ind] = Packget[Postion + ind];
                File Entry = new File() {
                    fileName = FileName,
                    Content = FileData
                };
                Files[Now] = Entry;
                Now++;
            }
            return Files;
        }

        static public byte[] GenArc(File[] Entries) {
            byte[] Header = new byte[8];
            GenOffset(Entries.Length).CopyTo(Header, 0);
            byte[] ArcEntries = new byte[0];
            byte[] DataTable = new byte[0];
            for (int i = 0; i < Entries.Length; i++) {
                byte[] EntryHeader = new byte[8];
                GenOffset(Entries[i].Content.Length).CopyTo(EntryHeader, 0);
                GenOffset(DataTable.Length).CopyTo(EntryHeader, 4);
                byte[] FName = StringParse(Entries[i].fileName + "\x0");
                //Create Entry
                byte[] tmp = new byte[ArcEntries.Length + EntryHeader.Length + FName.Length];
                ArcEntries.CopyTo(tmp, 0);
                EntryHeader.CopyTo(tmp, ArcEntries.Length);
                FName.CopyTo(tmp, ArcEntries.Length + EntryHeader.Length);
                ArcEntries = tmp;
                //Write File 
                tmp = new byte[DataTable.Length + Entries[i].Content.Length];
                DataTable.CopyTo(tmp, 0);
                Entries[i].Content.CopyTo(tmp, DataTable.Length);
                DataTable = tmp; 
            }
            GenOffset(ArcEntries.Length).CopyTo(Header, 4);
            byte[] rst = new byte[Header.Length + ArcEntries.Length + DataTable.Length];
            Header.CopyTo(rst, 0);
            ArcEntries.CopyTo(rst, Header.Length);
            DataTable.CopyTo(rst, Header.Length + ArcEntries.Length);
            return rst;
        }

        private static byte[] GenOffset(int value) {
            byte[] data = BitConverter.GetBytes(value);
            while (data.Length % 4 != 0) {
                byte[] tmp = new byte[data.Length+1];
                data.CopyTo(tmp, 0);
                data = tmp;
            }
            return data;
        }
        private static string ByteParse(byte[] str) {
            return Encoding.Unicode.GetString(str);
        }
        private static byte[] StringParse(string str) {
            return Encoding.Unicode.GetBytes(str);
        }
        private static int GetOffset(byte[] data, int Pos) {
            byte[] Variable = new byte[4];
            for (int i = 0; i < Variable.Length; i++)
                Variable[i] = data[Pos+i];
            return BitConverter.ToInt32(Variable, 0);
        }

    }
    public class File {
        public string fileName;
        public byte[] Content;
    }
}
