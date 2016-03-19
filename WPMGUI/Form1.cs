using System;
using WillPlusManager;
using System.Windows.Forms;

namespace WPMGUI {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            MessageBox.Show("This GUI don't is a stable translation tool, this program is a Demo for my dll, the \"WillPlusManager.dll\" it's a opensoruce project to allow you make your program to edit any ws2 file, or extract/repack arc files.\n\nHow to use:\n*Rigth Click in the window to open or save the file\n*Select the string in listbox and edit in the text box\n*Press enter to update the string\n\nThis program is unstable!");
        }
        WS2 Engine;
        WS2String[] Strings;
        private void openToolStripMenuItem_Click(object sender, EventArgs e) {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "All WillPlus 2 Scripts | *.ws2";
            DialogResult dr = fd.ShowDialog();
            if (dr == DialogResult.OK) {
                Engine = new WS2(System.IO.File.ReadAllBytes(fd.FileName), true);
                Strings = Engine.Import();
                foreach (WS2String str in Strings) {
                    listBox1.Items.Add(str.String);
                }
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                int i = listBox1.SelectedIndex;
                if (Strings[i].HaveActor)
                    this.Text = Strings[i].Actor.String;
                else
                    this.Text = "No Actor - ID: " + i;
                textBox1.Text = Strings[i].String;
            }
            catch { }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e) {
            SaveFileDialog fd = new SaveFileDialog();
            fd.Filter = "All WillPlus 2 Scripts | *.ws2";
            DialogResult dr = fd.ShowDialog();
            if (dr == DialogResult.OK) {
                for (int i = 0; i < Strings.Length; i++)
                    Strings[i].String = listBox1.Items[i].ToString();
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
                File[] Entries = WP2Arc.Open(System.IO.File.ReadAllBytes(FD.FileName));
                string dir = FD.FileName + "~\\";
                if (System.IO.Directory.Exists(dir))
                    System.IO.Directory.Delete(dir, true);
                System.IO.Directory.CreateDirectory(dir);
                foreach (File file in Entries)
                    System.IO.File.WriteAllBytes(dir + file.fileName, file.Content);
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
                string[] FilesNames = System.IO.Directory.GetFiles(BD.SelectedPath, "*.*", System.IO.SearchOption.TopDirectoryOnly);
                File[] Entries = new File[FilesNames.Length];
                for (int i = 0; i < Entries.Length; i++) {
                    Entries[i] = new File {
                        fileName = System.IO.Path.GetFileName(FilesNames[i]),
                        Content = System.IO.File.ReadAllBytes(FilesNames[i])
                    };
                }
                System.IO.File.WriteAllBytes(FD.FileName, WP2Arc.GenArc(Entries));
            }
        }
    }
}
