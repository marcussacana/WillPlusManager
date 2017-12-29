using AdvancedBinary;
using System.IO;
using System.Text;

namespace WillPlusManager {
    public static class WP2Arc {
        static uint HeaderLen = (uint)Tools.GetStructLength(new ArcHeader());
        static public File[] Import(Stream Packget) {
            StructReader Reader = new StructReader(Packget, Encoding: Encoding.Unicode);
            ArcHeader Header = new ArcHeader();
            Reader.ReadStruct(ref Header);
            Header.BaseOffset += HeaderLen;

            File[] Files = new File[Header.Count];
            for (uint i = 0; i < Files.LongLength; i++) {
                File Entry = new File();
                Reader.ReadStruct(ref Entry);

                Entry.Content = new VirtStream(Packget, Entry.Offset + Header.BaseOffset, Entry.Length);

                Files[i] = Entry;
            }

            return Files;
        }

        
        static public void Export(File[] Files, Stream Output, bool CloseStreams = true) {
            StructWriter Writer = new StructWriter(Output, Encoding: Encoding.Unicode);

            uint DataInfoLen = 0;
            foreach (File f in Files)
                DataInfoLen += (uint)Tools.GetStructLength(f, Encoding.Unicode);

            ArcHeader Header = new ArcHeader() {
                Count = (uint)Files.LongLength,
                BaseOffset = DataInfoLen
            };

            Writer.WriteStruct(ref Header);

            uint Ptr = 0;
            for (uint i = 0; i < Files.LongLength; i++) {
                Files[i].Offset = Ptr;
                Files[i].Length = (uint)Files[i].Content.Length;

                Ptr += Files[i].Length;

                Writer.WriteStruct(ref Files[i]);
            }

            for (uint i = 0; i < Files.LongLength; i++) {
                int Readed = 0;
                uint TotalLen = 0;
                byte[] Buffer = new byte[1024 * 1024];
                do {
                    Readed = Files[i].Content.Read(Buffer, 0, Buffer.Length);
                    Output.Write(Buffer, 0, Readed);
                    TotalLen += (uint)Readed;
                } while (Readed > 0);
                Output.Flush();

                //System.Diagnostics.Debug.Assert(TotalLen == Files[i].Length);

                if (CloseStreams)
                    Files[i].Content.Close();
            }

            if (CloseStreams)
                Writer.Close();
        }

    }

    public struct ArcHeader {
        public uint Count;
        public uint BaseOffset;
    }

    public struct File {
        public uint Length;
        public uint Offset;

        [UCString]
        public string FileName;

        [Ignore]
        public Stream Content;
    }
}
