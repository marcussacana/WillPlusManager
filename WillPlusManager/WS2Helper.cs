using System.Collections.Generic;

namespace WillPlusManager {
    public class WS2Helper {
        WS2 Engine;
        public WS2String[] Entries;
        public uint ActorCount;
        byte[] Script;
        public WS2Helper(byte[] Script) {
            this.Script = Script;
            Engine = new WS2(Script, true);
        }

        public string[] Import() {
            Entries = Engine.Import();
            string[] Strings = ListActors(Entries);
            ActorCount = (uint)Strings.LongLength;
            foreach (var Entry in Entries) {
                AppendArray(ref Strings, Entry.String);
            }

            return Strings;
        }

        public byte[] Export(string[] Str) {
            Dictionary<string, string> ActorDB = new Dictionary<string, string>();
            string[] Actors = ListActors(Entries);
            for (uint i = 0; i < Actors.LongLength; i++) {
                ActorDB.Add(Actors[i], Str[i]);
            }

            for (uint i = 0; i < Entries.LongLength; i++) {
                if (Entries[i].HaveActor) {
                    Entries[i].Actor.SetString(ActorDB[Entries[i].Actor.String]);
                }

                Entries[i].SetString(Str[i + Actors.LongLength]);
            }

            return Engine.Export(Entries);
        }

        public void AppendArray<T>(ref T[] Arr, T Value) => AppendArray(ref Arr, new T[] { Value });
        public void AppendArray<T>(ref T[] Arr, T[] DataToAppend) {
            T[] NewArr = new T[Arr.LongLength + DataToAppend.LongLength];
            Arr.CopyTo(NewArr, 0);
            DataToAppend.CopyTo(NewArr, Arr.LongLength);
            Arr = null;
            Arr = NewArr;
        }

        public string[] ListActors(WS2String[] Strings) {
            List<string> Actors = new List<string>();
            foreach (var Str in Strings) {
                if (Str.HaveActor) {
                    if (Actors.Contains(Str.Actor.String))
                        continue;
                    Actors.Add(Str.Actor.String);
                }
            }

            return Actors.ToArray();
        }
    }
}
