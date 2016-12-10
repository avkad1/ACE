using Shell32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ACE
{
    public partial class Form2 : Form
    {
        Folder pathToCopy;

        public Form2()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItems.Count > 0)
            {
                List<String> filesToCopy = new List<string>();
                foreach (int index in listBox1.SelectedIndices)
                {
                    filesToCopy.Add(listBox1.Items[index].ToString());
                }

                foreach (string file in filesToCopy)
                {
                    Folder destFolder = Path2Folder(Path.GetDirectoryName(file));
                    FolderItem fileToCopy = null;
                    foreach (FolderItem fi in destFolder.Items())
                    {
                        if(file.Contains(fi.Name)){
                            fileToCopy = fi;
                            break;
                        }
                    }
                    if (fileToCopy != null)
                    {
                        pathToCopy.CopyHere(fileToCopy, 4);
                    }
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(@"C:\Users\" + Environment.UserName + @"\Documents\A.C.E\Settings.xml");
            XmlNode root = xml.DocumentElement;
            XmlNode myNode = root.SelectSingleNode("/Settings/TVShows/Source");
            DirectoryInfo dir = new DirectoryInfo(myNode.InnerText + "\\Current\\");
            List<FileInfo> files = new List<FileInfo>();
            List<String> acceptedExtensions = new List<string> { ".mkv", ".rmvb", ".avi", ".mp4", ".rm", ".DAT", ".divx", ".wmv" };
            foreach (string ext in acceptedExtensions)
            {
                FileInfo[] fls = dir.GetFiles("*" + ext, SearchOption.AllDirectories).OrderByDescending(p => p.CreationTime).ToArray();
                files.AddRange(fls);
            }
            FileInfo[] flss = files.OrderByDescending(p => p.CreationTime).ToArray();

            foreach (FileInfo file in flss)
            {
                if (listBox1.Items.Count <= 50)
                    listBox1.Items.Add(file.FullName);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Shell shell = new Shell();
            Folder folder = shell.BrowseForFolder((int)Hwnd, "Choose Folder", 0, 0);
            if (folder != null)
            {
                FolderItem fi = (folder as Folder3).Self;
                pathToCopy = folder;
                textBox1.Text = fi.Name;
            }
        }

        public int Hwnd { get; private set; }

        public static Folder Path2Folder(string path)
        {
            Shell shell = new Shell();
            return shell.NameSpace(path);
        }
    }
}
