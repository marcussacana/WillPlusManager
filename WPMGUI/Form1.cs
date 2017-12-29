using System;
using WillPlusManager;
using System.Windows.Forms;
using System.IO;

namespace WPMGUI {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            MessageBox.Show("This GUI don't is a stable translation tool, this program is a Demo for my dll, the \"WillPlusManager.dll\" it's a opensoruce project to allow you make your program to edit any ws2 file, or extract/repack arc files.\n\nHow to use:\n*Rigth Click in the window to open or save the file\n*Select the string in listbox and edit in the text box\n*Press enter to update the string\n\nThis program is unstable!");
        }
        WS2Helper Engine;
        string[] Strings;
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "All WillPlus 2 Scripts | *.ws2";
            DialogResult dr = fd.ShowDialog();
            if (dr == DialogResult.OK) {
                Engine = new WS2Helper(System.IO.File.ReadAllBytes(fd.FileName));
                Strings = Engine.Import();
                foreach (string str in Strings) {
                    listBox1.Items.Add(str);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                int i = listBox1.SelectedIndex;
                if (i >= Engine.ActorCount && Engine.Entries[i - Engine.ActorCount].HaveActor)
                    Text = Engine.Entries[i - Engine.ActorCount].Actor.String + " - ID: " + i;
                else
                    Text = "No Actor - ID: " + i;
                textBox1.Text = Strings[i];
            }
            catch { }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog fd = new SaveFileDialog();
            fd.Filter = "All WillPlus 2 Scripts | *.ws2";
            DialogResult dr = fd.ShowDialog();
            if (dr == DialogResult.OK) {
                for (int i = 0; i < Strings.Length; i++)
                    Strings[i] = listBox1.Items[i].ToString();
                System.IO.File.WriteAllBytes(fd.FileName, Engine.Export(Strings));
             }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) {
            if (e.KeyChar == '\n' || e.KeyChar == '\r') {
                int i = listBox1.SelectedIndex;
                listBox1.Items[i] = textBox1.Text;
            }
        }

        private void extractFilesToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog FD = new OpenFileDialog();
            FD.Filter = "All WillPlus 2 Arc Packgets | *.arc";
            DialogResult DR = FD.ShowDialog();
            if (DR == DialogResult.OK) {
                using (Stream Packget = new StreamReader(FD.FileName).BaseStream) {
                    var Entries = WP2Arc.Import(Packget);
                    string dir = FD.FileName + "~\\";
                    if (Directory.Exists(dir))
                        Directory.Delete(dir, true);
                    Directory.CreateDirectory(dir);
                    foreach (var file in Entries) {
                        using (Stream Writer = new StreamWriter(dir + file.FileName).BaseStream) {
                            file.Content.CopyTo(Writer);
                        }
                    }
                }
                MessageBox.Show("Extracted");
            }
        }

        private void createPackgetToolStripMenuItem_Click(object sender, EventArgs e) {
            FolderBrowserDialog BD = new FolderBrowserDialog();
            BD.ShowNewFolderButton = false;
            DialogResult dr = BD.ShowDialog();
            SaveFileDialog FD = new SaveFileDialog();
            FD.Filter = "All WillPlus 2 Arc Packgets | *.arc";
            DialogResult dr2 = FD.ShowDialog();
            if (dr == DialogResult.OK && dr2 == DialogResult.OK) {
                string[] FilesNames = Directory.GetFiles(BD.SelectedPath, "*.*", SearchOption.TopDirectoryOnly);
                var Entries = new WillPlusManager.File[FilesNames.Length];
                for (int i = 0; i < Entries.Length; i++) {
                    Entries[i] = new WillPlusManager.File {
                        FileName = Path.GetFileName(FilesNames[i]),
                        Content = new StreamReader(FilesNames[i]).BaseStream
                    };
                }
                using (Stream Output = new StreamWriter(FD.FileName).BaseStream) {
                    WP2Arc.Export(Entries, Output, true);
                }
            }
        }

        private void decryptToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK) {
                byte[] file = System.IO.File.ReadAllBytes(ofd.FileName);
                WS2 ws = new WS2(file, true);
                file = ws.Decrypt(ref file);
                System.IO.File.WriteAllBytes(ofd.FileName, file);
            }
        }
    }
}
