using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using HtmlAgilityPack;
using System.Threading;
using System.Text.RegularExpressions;
using ACE;
using System.Security.Cryptography;

namespace Avinash_sConsolidatedEntertainer
{
    public partial class Form1 : Form
    {
        #region Members

        public String tvShowBasePath = String.Empty;
        public String moveiesBasePath = String.Empty;
        public String organizeBasePath = String.Empty;
        public String settingsFile = @"C:\Users\" + Environment.UserName + @"\Documents\A.C.E\Settings.xml";
        public String historyFile = @"C:\Users\" + Environment.UserName + @"\Documents\A.C.E\History.xml";
        public String organizerFile = @"C:\Users\" + Environment.UserName + @"\Documents\A.C.E\OrganizerSettings.xml";
        public int randomPlayCount = 0;
        public int randomPlayLimit = 0;
        public String selectedTVSource = String.Empty;
        public String selectedMovieSource = String.Empty;
        public String selectedTVShow = String.Empty;
        public String selectedSeason = String.Empty;
        public String selectedMovie = String.Empty;
        List<String> acceptedExtensions = new List<string> { ".mkv", ".rmvb", ".avi", ".mp4", ".rm", ".DAT", ".divx", ".wmv" };
        string searchString = String.Empty;
        bool process = false;
        bool subtitles = false;

        #endregion

        public Form1()
        {
            InitializeComponent();

            timer1.Enabled = true;
            timer1.Interval = 1000;
            label14.Text = DateTime.Now.ToString();

            #region Load Preferences

            if (!Directory.Exists(@"C:\Users\" + Environment.UserName + @"\Documents\A.C.E"))
            {
                Directory.CreateDirectory(@"C:\Users\" + Environment.UserName + @"\Documents\A.C.E");
            }

            if (!File.Exists(settingsFile))
            {
                XmlTextWriter xmlWriter = new XmlTextWriter(settingsFile, Encoding.UTF8);
                xmlWriter.WriteStartDocument(true);
                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.Indentation = 2;
                xmlWriter.WriteStartElement("Settings");
                xmlWriter.WriteStartElement("TVShows");
                xmlWriter.WriteStartElement("Source");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("RandomPlayLimit");
                xmlWriter.WriteString("500");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("RandomPlay");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Movies");
                xmlWriter.WriteStartElement("Source");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Organize");
                xmlWriter.WriteStartElement("Download");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Subtitle");
                xmlWriter.WriteStartElement("Status");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.Close();
                MessageBox.Show("Setup is incomplete. You need to setup the source of TV Shows/Movies and Downloads Path for the application to work.\nTV Shows Path: Setup > Directory > TV Shows\nMovies Path: Setup > Directory > Movies\nDownload Path: Setup > Directory > Organize", "Setup Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(settingsFile);
                XmlNode root = xml.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("/Settings/TVShows/Source");
                tvShowBasePath = myNode.InnerText;
                myNode = root.SelectSingleNode("/Settings/Movies/Source");
                moveiesBasePath = myNode.InnerText;
                myNode = root.SelectSingleNode("/Settings/Organize/Download");
                organizeBasePath = myNode.InnerText;
                myNode = root.SelectSingleNode("/Settings/Subtitle/Status");
                subtitles = Convert.ToBoolean(myNode.InnerText);
                if (subtitles)
                    onToolStripMenuItem.Checked = true;
                else
                    offToolStripMenuItem.Checked = true;
                myNode = root.SelectSingleNode("/Settings/TVShows/RandomPlayLimit");
                textBox1.Text = myNode.InnerText;
                myNode = root.SelectSingleNode("/Settings/TVShows/RandomPlay");
                foreach (XmlNode tvshow in myNode.ChildNodes)
                {
                    listBox5.Items.Add(tvshow.InnerText);
                }
                xml = null;

                if (Directory.Exists(tvShowBasePath))
                {
                    foreach (string dir in Directory.GetDirectories(tvShowBasePath))
                    {
                        listBox1.Items.Add(Path.GetFileName(dir));
                    }
                    listBox1.SelectedIndex = 0;
                }
                if (Directory.Exists(moveiesBasePath))
                {
                    foreach (string dir in Directory.GetDirectories(moveiesBasePath))
                    {
                        listBox7.Items.Add(Path.GetFileName(dir));
                    }
                    listBox7.SelectedIndex = 0;
                }
            }

            #endregion

            #region Load Play History

            if (!File.Exists(historyFile))
            {
                XmlTextWriter xmlWriter = new XmlTextWriter(historyFile, Encoding.UTF8);
                xmlWriter.WriteStartDocument(true);
                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.Indentation = 2;
                xmlWriter.WriteStartElement("History");
                xmlWriter.WriteStartElement("TVShows");
                xmlWriter.WriteStartElement("RandomPlayCount");
                xmlWriter.WriteString("0");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("RandomPlayList");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("NormalPlayList");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Movies");
                xmlWriter.WriteStartElement("MovieList");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.Close();
            }
            else
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(historyFile);
                XmlNode root = xml.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("/History/TVShows/RandomPlayCount");
                textBox2.Text = myNode.InnerText;
                
                myNode = root.SelectSingleNode("/History/TVShows/NormalPlayList");
                if (myNode.ChildNodes.Count !=0)
                {
                    string[] temp = myNode.LastChild.InnerText.Split('\\');
                    listBox1.SelectedItem = temp[2];
                    listBox2.SelectedItem = temp[3];
                    if (temp.Length == 6)
                        listBox3.SelectedItem = temp[4];
                    listBox6.ClearSelected();
                    if (temp.Length == 5)
                        listBox6.SelectedItem = temp[4];
                    else
                        listBox6.SelectedItem = temp[5];
                    toolStripStatusLabel1.Text = "Last Played: " + myNode.LastChild.InnerText;
                }
            }

            #endregion

        }

        #region ToolStrip Menu Items

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tVShowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult r = folderBrowserDialog1.ShowDialog();
            if (r == System.Windows.Forms.DialogResult.OK)
            {
                tvShowBasePath = folderBrowserDialog1.SelectedPath + "\\";
                XmlDocument settingsXML = new XmlDocument();
                settingsXML.Load(settingsFile);
                XmlNode root = settingsXML.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("/Settings/TVShows/Source");
                myNode.InnerText = tvShowBasePath;
                settingsXML.Save(settingsFile);

                XmlDocument xml = new XmlDocument();
                xml.Load(settingsFile);
                tvShowBasePath = myNode.InnerText;
                myNode = root.SelectSingleNode("/Settings/Movies/Source");
                moveiesBasePath = myNode.InnerText;
                myNode = root.SelectSingleNode("/Settings/TVShows/RandomPlay");
                foreach (XmlNode tvshow in myNode.ChildNodes)
                {
                    listBox5.Items.Add(tvshow.InnerText);
                }
                xml = null;

                foreach (string dir in Directory.GetDirectories(tvShowBasePath))
                {
                    listBox1.Items.Add(Path.GetFileName(dir));
                }
                listBox1.SelectedIndex = 0;
            }
        }

        private void moviesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult r = folderBrowserDialog1.ShowDialog();
            if (r == System.Windows.Forms.DialogResult.OK)
            {
                moveiesBasePath = folderBrowserDialog1.SelectedPath + "\\";
                XmlDocument settingsXML = new XmlDocument();
                settingsXML.Load(settingsFile);
                XmlNode root = settingsXML.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("/Settings/Movies/Source");
                myNode.InnerText = moveiesBasePath;
                settingsXML.Save(settingsFile);

                foreach (string dir in Directory.GetDirectories(moveiesBasePath))
                {
                    listBox7.Items.Add(Path.GetFileName(dir));
                }
                listBox7.SelectedIndex = 0;
            }
        }

        private void randomPlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label4.Visible = false;
            label13.Visible = false;
            label5.Visible = true;
            label5.Visible = false;
            listBox5.Visible = true;
            button4.Visible = true;
            button5.Visible = true;
            listBox4.Visible = false;
        }

        private void playHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(historyFile);
            XmlNode root = xml.DocumentElement;
            XmlNode myNode = root.SelectSingleNode("/History/Movies/MovieList");
            int j = 1;
            if (myNode.ChildNodes.Count > 0)
            {
                moviesToolStripMenuItem.DropDownItems.Clear();
                for (int i = myNode.ChildNodes.Count - 1; i > 0; i--)
                {
                    moviesToolStripMenuItem1.DropDownItems.Add(myNode.ChildNodes[i].InnerText);
                    moviesToolStripMenuItem1.DropDownItems[moviesToolStripMenuItem1.DropDownItems.Count - 1].Click += new EventHandler(MenuItemClick);
                    j++;
                    if (j == 10)
                    {
                        break;
                    }
                }
                j = 0;
            }
            myNode = root.SelectSingleNode("/History/TVShows/NormalPlayList");
            if (myNode.ChildNodes.Count > 0)
            {
                normalToolStripMenuItem.DropDownItems.Clear();
                for (int i = myNode.ChildNodes.Count - 1; i > 0; i--)
                {
                    normalToolStripMenuItem.DropDownItems.Add(myNode.ChildNodes[i].InnerText);
                    normalToolStripMenuItem.DropDownItems[normalToolStripMenuItem.DropDownItems.Count - 1].Click += new EventHandler(MenuItemClick);
                    j++;
                    if (j == 10)
                    {
                        break;
                    }
                }
                j = 0;
            }
            myNode = root.SelectSingleNode("/History/TVShows/RandomPlayList");
            if (myNode.ChildNodes.Count > 0)
            {
                randomToolStripMenuItem.DropDownItems.Clear();
                for (int i = myNode.ChildNodes.Count - 1; i > 0; i--)
                {
                    randomToolStripMenuItem.DropDownItems.Add(myNode.ChildNodes[i].InnerText);
                    randomToolStripMenuItem.DropDownItems[randomToolStripMenuItem.DropDownItems.Count - 1].Click += new EventHandler(MenuItemClick);
                    j++;
                    if (j == 10)
                    {
                        break;
                    }
                }
                j = 0;
            }
        }

        private void MenuItemClick(object sender, EventArgs e)
        {
            string[] path = sender.ToString().Split('\\');
            if (path.Contains("TV Shows"))
            {
                tabControl1.SelectedTab = tabPage1;
                listBox1.SelectedItem = path[2];
                listBox2.SelectedItem = path[3];
                if (path.Length == 6)
                    listBox3.SelectedItem = path[4];
                listBox6.ClearSelected();
                if (path.Length == 5)
                    listBox6.SelectedItem = path[4];
                else
                    listBox6.SelectedItem = path[5];
            }
            else
            {
                tabControl1.SelectedTab = tabPage2;
                listBox7.SelectedItem = path[2];
                if (path.Length == 5)
                {
                    listBox8.SelectedItem = path[3];
                    listBox10.SelectedItem = path[4];
                }
                else
                {
                    listBox9.SelectedItem = path[3];
                }
            }
        }

        private void organizeToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            DialogResult r = folderBrowserDialog1.ShowDialog();
            if (r == System.Windows.Forms.DialogResult.OK)
            {
                organizeBasePath = folderBrowserDialog1.SelectedPath + "\\";
                XmlDocument settingsXML = new XmlDocument();
                settingsXML.Load(settingsFile);
                XmlNode root = settingsXML.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("/Settings/Organize/Download");
                myNode.InnerText = organizeBasePath;
                settingsXML.Save(settingsFile);
            }
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = @"C:\Users\Avinash\Downloads\TEMP";

            string destPath = String.Empty;
            Dictionary<string, List<string>> tvs = new Dictionary<string, List<string>>();

            if (File.Exists(organizerFile))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(organizerFile);
                XmlNode root = xml.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("/Settings/DestinationFolder");
                destPath = myNode.InnerText;

                myNode = root.SelectSingleNode("/Settings/Folders");
                myNode.RemoveAll();
                string[] directories = Directory.GetDirectories(destPath);
                foreach (string dir in directories)
                {
                    XmlElement nodeTobeAdded = xml.CreateElement(Path.GetFileName(dir));
                    myNode.AppendChild(nodeTobeAdded);
                    xml.Save(organizerFile);
                    myNode = root.SelectSingleNode("/Settings/Folders/" + Path.GetFileName(dir));

                    string[] directories1 = Directory.GetDirectories(dir);
                    List<string> temp1 = new List<string>();
                    foreach (string dir1 in directories1)
                    {
                        nodeTobeAdded = xml.CreateElement("TVShow");
                        nodeTobeAdded.InnerText = Path.GetFileName(dir1);
                        myNode.AppendChild(nodeTobeAdded);
                        xml.Save(organizerFile);
                        temp1.Add(Path.GetFileName(dir1));
                    }
                    myNode = root.SelectSingleNode("/Settings/Folders");
                    tvs.Add(Path.GetFileName(dir), temp1);
                }

                xml = null;
            }
            else
            {
                destPath = tvShowBasePath;

                XmlTextWriter xmlWriter = new XmlTextWriter(organizerFile, Encoding.UTF8);
                xmlWriter.WriteStartDocument(true);
                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.Indentation = 4;
                xmlWriter.WriteStartElement("Settings");
                xmlWriter.WriteStartElement("DestinationFolder");
                xmlWriter.WriteString(destPath);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement("Folders");

                string[] directories = Directory.GetDirectories(destPath);
                foreach (string dir in directories)
                {
                    xmlWriter.WriteStartElement(Path.GetFileName(dir));

                    string[] directories1 = Directory.GetDirectories(dir);
                    List<string> temp1 = new List<string>();
                    foreach (string dir1 in directories1)
                    {
                        xmlWriter.WriteStartElement("TVShow");
                        xmlWriter.WriteString(Path.GetFileName(dir1));
                        temp1.Add(Path.GetFileName(dir1));
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                    tvs.Add(Path.GetFileName(dir), temp1);
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.Close();

            }
            MoveFilesForMe();
            Thread organize = new Thread(() => Organize(getFilesOfDifferentExtension(path, acceptedExtensions), tvs, destPath));
            organize.Start();
        }

        private void onToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.onToolStripMenuItem.Checked = true;
            this.offToolStripMenuItem.Checked = false;
            subtitles = true;
            XmlDocument settingsXML = new XmlDocument();
            settingsXML.Load(settingsFile);
            XmlNode root = settingsXML.DocumentElement;
            XmlNode myNode = root.SelectSingleNode("/Settings/Subtitle/Status");
            myNode.InnerText = subtitles.ToString();
            settingsXML.Save(settingsFile);
        }

        private void offToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.onToolStripMenuItem.Checked = false;
            this.offToolStripMenuItem.Checked = true;
            subtitles = false;
            XmlDocument settingsXML = new XmlDocument();
            settingsXML.Load(settingsFile);
            XmlNode root = settingsXML.DocumentElement;
            XmlNode myNode = root.SelectSingleNode("/Settings/Subtitle/Status");
            myNode.InnerText = subtitles.ToString();
            settingsXML.Save(settingsFile);
        }
        #endregion

        #region ListBoxes

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            string source = listBox1.SelectedItem.ToString();
            selectedTVSource = tvShowBasePath + source + "\\";
            foreach (string dir in Directory.GetDirectories(selectedTVSource))
            {
                listBox2.Items.Add(Path.GetFileName(dir));
            }
            listBox2.SelectedIndex = 0;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox3.Items.Clear();
            string tvshow = listBox2.SelectedItem.ToString();
            selectedTVShow = selectedTVSource + tvshow + "\\";
            string[] temp = Directory.GetDirectories(selectedTVShow);
            Array.Sort(temp, new MyComparer());
            foreach (string dir in temp)
            {
                listBox3.Items.Add(Path.GetFileName(dir));
            }
            if (temp.Length == 0)
            {
                listBox6.Items.Clear();
                foreach (string file in Directory.GetFiles(selectedTVShow))
                {
                    if (acceptedExtensions.Contains(Path.GetExtension(file)))
                        listBox6.Items.Add(Path.GetFileName(file));
                }
                listBox6.SelectedIndex = 0;
            }
            else
            {
                listBox3.SelectedIndex = listBox3.Items.Count - 1;
            }
        }

        private void listBox2_MouseDown(object sender, MouseEventArgs e)
        {
            listBox3.Items.Clear();
            string tvshow = listBox2.SelectedItem.ToString();
            selectedTVShow = selectedTVSource + tvshow + "\\";
            string[] temp = Directory.GetDirectories(selectedTVShow);
            Array.Sort(temp, new MyComparer());
            foreach (string dir in temp)
            {
                listBox3.Items.Add(Path.GetFileName(dir));
            }
            if (temp.Length == 0)
            {
                listBox6.Items.Clear();
                foreach (string file in Directory.GetFiles(selectedTVShow))
                {
                    if (acceptedExtensions.Contains(Path.GetExtension(file)))
                        listBox6.Items.Add(Path.GetFileName(file));
                }
                listBox6.SelectedIndex = 0;
            }
            else
            {
                listBox3.SelectedIndex = 0;
            }
            string s = tvShowBasePath + listBox1.SelectedItem.ToString() + "\\" + listBox2.SelectedItem.ToString();
            DragDropEffects dde1 = DoDragDrop(s, DragDropEffects.All);
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox6.Items.Clear();
            string season = listBox3.SelectedItem.ToString();
            selectedSeason = selectedTVShow + season + "\\";
            foreach (string file in Directory.GetFiles(selectedSeason))
            {
                if(acceptedExtensions.Contains(Path.GetExtension(file)))
                    listBox6.Items.Add(Path.GetFileName(file));
            }
            listBox6.SelectedIndex = listBox6.Items.Count - 1;
        }

        private void listBox4_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void listBox4_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string str = (string)e.Data.GetData(DataFormats.StringFormat);
                if (!listBox4.Items.Contains(str))
                {
                    listBox4.Items.Add(str);
                }
            }
        }

        private void listBox4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox4.Items.Count != 0)
            {
                string itemToBeRemoved = listBox4.SelectedItem.ToString();
                List<string> itemsRemaining = new List<string>();
                foreach (string item in listBox4.Items)
                {
                    if (item != itemToBeRemoved)
                    {
                        itemsRemaining.Add(item);
                    }
                }
                listBox4.Items.Clear();
                foreach (string item in itemsRemaining)
                {
                    listBox4.Items.Add(item);
                }
            }
        }

        private void listBox5_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void listBox5_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string str = (string)e.Data.GetData(DataFormats.StringFormat);
                if (!listBox5.Items.Contains(str))
                {
                    listBox5.Items.Add(str);
                }
            }
        }

        private void listBox6_MouseDown(object sender, MouseEventArgs e)
        {
            this.listBox6.DoDragDrop(selectedSeason + listBox6.SelectedItem.ToString(), DragDropEffects.Copy);
        }

        private void listBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox8.Items.Clear();
            listBox9.Items.Clear();
            string source = listBox7.SelectedItem.ToString();
            selectedMovieSource = moveiesBasePath + source + "\\";
            foreach (string dir in Directory.GetDirectories(selectedMovieSource))
            {
                listBox8.Items.Add(Path.GetFileName(dir));
            }
            foreach (string file in Directory.GetFiles(selectedMovieSource))
            {
                if(acceptedExtensions.Contains(Path.GetExtension(file)))
                    listBox9.Items.Add(Path.GetFileName(file));
            }
            listBox8.SelectedIndex = 0;
        }

        private void listBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox10.Items.Clear();
            string movie = listBox8.SelectedItem.ToString();
            selectedMovie = selectedMovieSource + movie + "\\";
            foreach (string file in Directory.GetFiles(selectedMovie))
            {
                if (acceptedExtensions.Contains(Path.GetExtension(file)))
                    listBox10.Items.Add(Path.GetFileName(file));
            }
        }

        private void listBox9_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listBox9.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                XmlDocument historyXML = new XmlDocument();
                historyXML.Load(historyFile);
                XmlNode root = historyXML.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("/History/Movies/MovieList");
                XmlElement nodeTobeAdded = historyXML.CreateElement("movie");
                nodeTobeAdded.InnerText = moveiesBasePath + listBox7.SelectedItem.ToString() + "\\" + listBox9.Items[index];
                myNode.AppendChild(nodeTobeAdded);
                historyXML.Save(historyFile);

                Process.Start(moveiesBasePath + listBox7.SelectedItem.ToString() + "\\" + listBox9.Items[index]);
            }
        }

        private void listBox10_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = this.listBox10.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                XmlDocument historyXML = new XmlDocument();
                historyXML.Load(historyFile);
                XmlNode root = historyXML.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("/History/Movies/MovieList");
                XmlElement nodeTobeAdded = historyXML.CreateElement("movie");
                nodeTobeAdded.InnerText = moveiesBasePath + listBox7.SelectedItem.ToString() + "\\" + listBox8.SelectedItem.ToString() + "\\" + listBox10.Items[index].ToString();
                myNode.AppendChild(nodeTobeAdded);
                historyXML.Save(historyFile);
                Process.Start(moveiesBasePath + listBox7.SelectedItem.ToString() + "\\" + listBox8.SelectedItem.ToString() + "\\" + listBox10.Items[index].ToString());
            }
        }

        #endregion

        #region Buttons

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox6.SelectedItems.Count != 0)
            {
                Process p = new Process();
                XmlDocument historyXML = new XmlDocument();
                historyXML.Load(historyFile);
                XmlNode root = historyXML.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("/History/TVShows/NormalPlayList");
                foreach (string epi in listBox6.SelectedItems)
                {
                    if (myNode.ChildNodes.Count==0 || myNode.LastChild.InnerText != selectedSeason + epi)
                    {
                        XmlElement nodeTobeAdded = historyXML.CreateElement("tvshow");
                        nodeTobeAdded.InnerText = selectedSeason + epi;
                        myNode.AppendChild(nodeTobeAdded);
                        historyXML.Save(historyFile);
                    }

                    p.StartInfo.FileName = selectedSeason + epi;
                    p.Start();
                    p.WaitForInputIdle(0);
                    toolStripStatusLabel1.Text = "Last Played: " + selectedSeason + epi;
                }
            }
            else
            {
                MessageBox.Show("Please select atleast one episode!", "Null selection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox5.Items.Count > 0)
            {
                Random rnd = new Random();
            REPEAT: string tvShowPath = listBox5.Items[rnd.Next(listBox5.Items.Count)].ToString();
                listBox1.SelectedItem = tvShowPath.Split('\\')[2];
                listBox2.SelectedItem = tvShowPath.Split('\\')[3];
                if (listBox3.Items.Count > 0)
                {
                    listBox3.SelectedIndex = rnd.Next(listBox3.Items.Count);
                    tvShowPath += "\\" + listBox3.SelectedItem.ToString();
                }
                listBox6.ClearSelected();
                listBox6.SelectedIndex = rnd.Next(listBox6.Items.Count);
                string file = tvShowPath + "\\" + listBox6.SelectedItem.ToString();

                XmlDocument xml = new XmlDocument();
                xml.Load(historyFile);
                XmlNode root = xml.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("/History/TVShows/RandomPlayList");
                bool alreadyPlayed = false;
                foreach (XmlNode cn in myNode.ChildNodes)
                {
                    if (cn.InnerText == file)
                    {
                        alreadyPlayed = true;
                        break;
                    }
                }
                if (!alreadyPlayed && (int.Parse(textBox1.Text) >= int.Parse(textBox2.Text)))
                {
                    int count = int.Parse(textBox2.Text);
                    textBox2.Text = (++count).ToString();
                    XmlElement nodeTobeAdded = xml.CreateElement("episode");
                    nodeTobeAdded.InnerText = file;
                    myNode.AppendChild(nodeTobeAdded);
                    myNode = root.SelectSingleNode("/History/TVShows/RandomPlayCount");
                    myNode.InnerText = textBox2.Text;
                    xml.Save(historyFile);
                    Process.Start(file);
                    toolStripStatusLabel1.Text = "Last Played: " + file;
                }
                else if (int.Parse(textBox1.Text) < int.Parse(textBox2.Text))
                {
                    textBox2.Text = "0";
                    myNode = root.SelectSingleNode("/History/TVShows/RandomPlayCount");
                    myNode.InnerText = textBox2.Text;
                    myNode = root.SelectSingleNode("/History/TVShows/RandomPlayList");
                    myNode.RemoveAll();
                    xml.Save(historyFile);
                }
                else
                {
                    goto REPEAT;
                }
            }
            else
            {
                MessageBox.Show("Please go to the below path and add your random TV Show preferences.\nRandom Play Preferenc Path: Setup > Random Play", "No Ranom TV Show Preferences found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox4.Items.Count != 0)
            {
                Process p = new Process();
                XmlDocument historyXML = new XmlDocument();
                historyXML.Load(historyFile);
                XmlNode root = historyXML.DocumentElement;
                XmlNode myNode = root.SelectSingleNode("/History/TVShows/NormalPlayList");
                foreach (string epi in listBox4.Items)
                {
                    if (myNode.LastChild.InnerText != epi)
                    {
                        XmlElement nodeTobeAdded = historyXML.CreateElement("tvshow");
                        nodeTobeAdded.InnerText = epi;
                        myNode.AppendChild(nodeTobeAdded);
                        historyXML.Save(historyFile);
                    }
                    
                    p.StartInfo.FileName = epi;
                    p.Start();
                    p.WaitForInputIdle(0);
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            List<string> toBeRemoved = new List<string>();
            foreach (string itm in listBox5.SelectedItems)
            {
                toBeRemoved.Add(itm);
            }
            List<string> toBeKept = new List<string>();
            foreach (var item in listBox5.Items)
            {
                if (!toBeRemoved.Contains(item))
                {
                    toBeKept.Add(item.ToString());
                }
            }
            listBox5.Items.Clear();
            foreach (string itm in toBeKept)
            {
                listBox5.Items.Add(itm);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(settingsFile);
            XmlNode root = xml.DocumentElement;
            XmlNode myNode = root.SelectSingleNode("/Settings/TVShows/RandomPlay");
            myNode.RemoveAll();
            foreach (string rtvs in listBox5.Items)
            {
                XmlElement nodeTobeAdded = xml.CreateElement("tvshow");
                nodeTobeAdded.InnerText = rtvs;
                bool present = false;
                foreach (XmlNode node in myNode.ChildNodes)
                {
                    if (node == nodeTobeAdded)
                    {
                        present = true;
                        break;
                    }
                }
                if (!present)
                {
                    myNode.AppendChild(nodeTobeAdded);
                }
            }
            myNode = root.SelectSingleNode("/Settings/TVShows/RandomPlayLimit");
            if (textBox1.Text != String.Empty)
            {
                myNode.InnerText = textBox1.Text;
                xml.Save(settingsFile);

                label5.Visible = false;
                label13.Visible = true;
                listBox5.Visible = false;
                button4.Visible = false;
                button5.Visible = false;
                listBox4.Visible = true;
            }
            else
            {
                toolTip1.Show("Please enter a value in Random Limit Value", this.textBox1);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox2.Text = "0";
            XmlDocument xml = new XmlDocument();
            xml.Load(historyFile);
            XmlNode root = xml.DocumentElement;
            XmlNode myNode = root.SelectSingleNode("/History/TVShows/RandomPlayList");
            myNode = root.SelectSingleNode("/History/TVShows/RandomPlayCount");
            myNode.InnerText = textBox2.Text;
            myNode = root.SelectSingleNode("/History/TVShows/RandomPlayList");
            myNode.RemoveAll();
            myNode = root.SelectSingleNode("/History/TVShows/NormalPlayList");
            xml.Save(historyFile);
        }

        #endregion

        #region TextBox

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (this.textBox1.Text != String.Empty)
            {
                string temp = String.Empty;
                foreach (char c in textBox1.Text)
                {
                    if (c < '0' || c > '9')
                    {
                        textBox1.Text = textBox1.Text.Substring(0, textBox1.Text.Length - 1);
                        toolTip1.Show("Numbers Only", this.textBox1);
                        textBox1.SelectionStart = textBox1.Text.Length;
                        textBox1.SelectionLength = 0;
                    }
                }
            }
        }

        #endregion

        #region TabControl

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 1)
            {
                if (listBox7.Items.Count == 0)
                {
                    MessageBox.Show("Setup is incomplete. You need to setup the source of Movies for the application to work.\nMovies Path: Setup > Source Directory > Movies", "Setup Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                if (listBox1.Items.Count == 0)
                {
                    MessageBox.Show("Setup is incomplete. You need to setup the source of TV Shows for the application to work.\nTV Shows Path: Setup > Source Directory > TV Shows", "Setup Incomplete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        #endregion

        #region System Time

        private void timer1_Tick(object sender, EventArgs e)
        {
            label14.Text = DateTime.Now.ToString();
        }

        #endregion

        #region GroupBox

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        #endregion 

        #region Helpers

        private List<String> getFilesOfDifferentExtension(string path, List<string> extensions)
        {
            List<String> tem = new List<string>();
            foreach (string ext in extensions)
            {
                tem.AddRange(Directory.GetFiles(path, "*" + ext).ToList<String>());
            }
            return tem;
        }

        private void Organize(List<string> files, Dictionary<string, List<string>> tvs, string destPath)
        {
            process = true;
            foreach (string file in files)
            {
                string fn = RemoveSpecialCharacters(Path.GetFileNameWithoutExtension(file));
                string pattern5 = @"S+\d\d+E+\d\d+E\d\d";
                string pattern1 = @"S+\d\d+E+\d\d";
                string pattern2 = @"S+\d+E+\d\d";
                string pattern3 = @"\d\d\d\d";
                string pattern4 = @"\d\d\d";
                int pattern = 0;
                char[] split = { 's', 'S', 'e', 'E' };
                bool copied = false;
                foreach (KeyValuePair<string, List<string>> kvp in tvs)
                {
                    foreach (string item in kvp.Value)
                    {
                        if (fn.ToLower().Contains(item.ToLower()))
                        {
                            int season = 0;
                            string episode = String.Empty;
                            Match match = Regex.Match(fn, pattern5, RegexOptions.IgnoreCase);
                            if (match != Match.Empty)
                                pattern = 3;
                            if (match == Match.Empty)
                            {
                                match = Regex.Match(fn, pattern1, RegexOptions.IgnoreCase);
                                if (match != Match.Empty)
                                {
                                    pattern = 1; goto GOTPATTERN;
                                }
                            }
                            if (match == Match.Empty)
                            {
                                match = Regex.Match(fn, pattern2, RegexOptions.IgnoreCase);
                                if (match != Match.Empty)
                                {
                                    pattern = 1; goto GOTPATTERN;
                                }
                            }
                            if (match == Match.Empty)
                            {
                                match = Regex.Match(fn, pattern3, RegexOptions.IgnoreCase);
                                if (match != Match.Empty)
                                {
                                    pattern = 2; goto GOTPATTERN;
                                }
                            }
                            if (match == Match.Empty)
                            {
                                match = Regex.Match(fn, pattern4, RegexOptions.IgnoreCase);
                                if (match != Match.Empty)
                                {
                                    pattern = 2; goto GOTPATTERN;
                                }
                            }
                            if (match == Match.Empty)
                            {
                                this.BeginInvoke((Action)(() => toolStripStatusLabel1.Text = "\nNo matches for: " + fn));
                            }
                GOTPATTERN: switch (pattern)
                            {
                                case 1:
                                    season = Convert.ToInt32(match.ToString().Split(split, StringSplitOptions.RemoveEmptyEntries)[0]);
                                    episode = match.ToString().Split(split, StringSplitOptions.RemoveEmptyEntries)[1];
                                    break;
                                case 2:
                                    season = (Convert.ToInt32(match.ToString()) / 100);
                                    episode = (Convert.ToInt32(match.ToString()) % 100).ToString("D2");
                                    break;
                                case 3:
                                    season = Convert.ToInt32(match.ToString().Split(split, StringSplitOptions.RemoveEmptyEntries)[0]);
                                    episode = match.ToString().Split(split, StringSplitOptions.RemoveEmptyEntries)[1] + "-" + match.ToString().Split(split, StringSplitOptions.RemoveEmptyEntries)[2];
                                    break;
                                default:
                                    continue;
                            }

                            string destination = Path.Combine(destPath, kvp.Key, item, "Season " + season.ToString());
                            if (!Directory.Exists(destination))
                            {
                                Directory.CreateDirectory(destination);
                            }
                            if (!File.Exists(destination + "\\" + season.ToString() + episode + "-" + Path.GetExtension(file)))
                            {
                                File.Move(file, destination + "\\" + season.ToString() + episode + "-" + Path.GetExtension(file));
                                this.BeginInvoke((Action)(() => toolStripStatusLabel1.Text = "Moved: " + Path.GetFileNameWithoutExtension(file) + " to " + destination));
                            }
                            copied = true;
                            break;
                        }
                    }
                    if (copied)
                    {
                        break;
                    }
                }
            }
            Directory.Delete(@"C:\Users\Avinash\Downloads\TEMP", true);
            this.BeginInvoke((Action)(() => toolStripStatusLabel1.Text = String.Empty));
            process = false;
        }

        private string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(" ");
                }
            }
            return sb.ToString();
        }

        private void MoveFilesForMe()
        {
            string src = @"C:\Users\Avinash\Downloads";

            string dest = Path.Combine(src, "TEMP");
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            List<String> files = getFilesOfDifferentExtension(src, acceptedExtensions);
            foreach (string file in files)
            {
                File.Move(file, Path.Combine(dest, Path.GetFileName(file)));
            }

            string[] directories = Directory.GetDirectories(src);
            foreach (string dire in directories)
            {
                if (!dire.Contains("TEMP"))
                {
                    files = getFilesOfDifferentExtension(dire, acceptedExtensions);
                    foreach (string file in files)
                    {
                        File.Move(file, Path.Combine(dest, Path.GetFileName(file)));
                    }
                    Directory.Delete(dire, true);
                }
            }
        }

        private string CalculateCheckSum(String fileName)
        {
            byte[] result;
            using (Stream input = File.OpenRead(fileName))
            {
                result = ComputeMovieHash(input);
            }
            return ToHexadecimal(result);
        }

        private byte[] ComputeMovieHash(Stream input)
        {
            long lhash, streamsize;
            streamsize = input.Length;
            lhash = streamsize;

            long i = 0;
            byte[] buffer = new byte[sizeof(long)];
            while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }

            input.Position = Math.Max(0, streamsize - 65536);
            i = 0;
            while (i < 65536 / sizeof(long) && (input.Read(buffer, 0, sizeof(long)) > 0))
            {
                i++;
                lhash += BitConverter.ToInt64(buffer, 0);
            }
            input.Close();
            byte[] result = BitConverter.GetBytes(lhash);
            Array.Reverse(result);
            return result;
        }

        private string ToHexadecimal(byte[] bytes)
        {
            StringBuilder hexBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                hexBuilder.Append(bytes[i].ToString("x2"));
            }
            return hexBuilder.ToString();
        }
        #endregion

        #region Labels

        private void label1_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(tvShowBasePath, listBox1.SelectedItem.ToString()));
        }

        private void label2_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(tvShowBasePath, listBox1.SelectedItem.ToString(), listBox2.SelectedItem.ToString()));
        }

        private void label6_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(tvShowBasePath, listBox1.SelectedItem.ToString(), listBox2.SelectedItem.ToString(), listBox3.SelectedItem.ToString()));
        }

        private void label7_Click(object sender, EventArgs e)
        {
            Process.Start(Path.Combine(moveiesBasePath, listBox7.SelectedItem.ToString()));
        }

        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (toolStripStatusLabel1.Text.Contains("Moved:") || process)
            {
                var dr = MessageBox.Show("Files are being moved. Do you really want to quit?", "Background Process running", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == System.Windows.Forms.DialogResult.Yes)
                {
                    e.Cancel = false;
                    process = false;
                    Application.Exit();
                }
                else
                    e.Cancel = true;
            }
        }

        private void copyToPhoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.ShowDialog();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                String url = "http://sandbox.thesubdb.com/?action=search&hash=" + CalculateCheckSum(selectedSeason + listBox6.SelectedItem.ToString());
                Console.WriteLine(url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "SubDB";
                request.ProtocolVersion = new Version("1.0");
                //request.Headers.Add("ACE/1.0; http://github.com/avkad1/ace-cli");
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    string html = reader.ReadToEnd();
                    Console.WriteLine(html);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }

    public class MyComparer : IComparer<string>
    {

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        static extern int StrCmpLogicalW(String x, String y);

        public int Compare(string x, string y)
        {
            return StrCmpLogicalW(x, y);
        }

    }
}
