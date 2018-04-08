using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using Shell32;

namespace StartupManager
{
    public partial class Form1 : Form
    {
        private List<StartupObject> startupList;
        public Form1()
        {
            InitializeComponent();
        }

        ///<summary>
        ///Translate StartupType into a human readable string.
        ///</summary>
        private string TranslateType(StartupType type)
        {
            switch(type)
            {
                case StartupType.FOLDER:
                    return "Startup Folder";
                case StartupType.REGISTRY32_CU:
                    return "Registry32 Current User";
                case StartupType.REGISTRY32_LM:
                    return "Registry32 Local Machine";
                case StartupType.REGISTRY64_CU:
                    return "Registry64 Current User";
                case StartupType.REGISTRY64_LM:
                    return "Registry64 Local Machine";
                default:
                    return "Undefined";
            }
        }

        private string FilterPath(string path)
        {
            path = path.Replace('"', ' ');
            path = path.Trim();
            //path = path.Trim('"');
            path = path.Replace("\\", @"\");

            return path;
        }

        private List<StartupObject> EnumerateKey(RegistryKey key, StartupType type)
        {
            List<StartupObject> objList = new List<StartupObject>();

            foreach(string name in key.GetValueNames())
            {
                StartupObject obj = new StartupObject();
                obj.Name = name;
                obj.Path = FilterPath(key.GetValue(name).ToString());
                obj.Type = type;

                Debug.WriteLine(Path.GetDirectoryName(obj.Path));

                objList.Add(obj);
            }

            return objList;
        }

        private List<StartupObject> EnumerateStartupFolder()
        {
            List<StartupObject> objList = new List<StartupObject>();
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Startup); //Environment.SpecialFolder.CommonStartup
            IEnumerable<string> files = Directory.EnumerateFiles(path);
            
            foreach(string name in files)
            {
                if (name.EndsWith(".ini"))
                    continue;

                string objPath = name;

                if (name.EndsWith(".lnk"))
                {
                    Shell shell = new Shell();
                    ShellLinkObject link = shell.NameSpace(path).ParseName(Path.GetFileName(name)).GetLink;

                    objPath = link.Path;
                }

                StartupObject obj = new StartupObject();
                obj.Name = Path.GetFileNameWithoutExtension(name);
                obj.Path = objPath;
                obj.Type = StartupType.FOLDER;

                objList.Add(obj);
            }

            return objList;
        }

        private void AddListview()
        {
            listView1.BeginUpdate();

            foreach(StartupObject obj in startupList)
            {
                ListViewItem item = new ListViewItem();
                item.Name = obj.ID.ToString();
                item.Text = TranslateType(obj.Type);
                item.SubItems.Add(obj.Name);
                item.SubItems.Add(obj.Path);

                listView1.Items.Add(item);
            }

            listView1.EndUpdate();
        }

        private async Task GetObjects()
        {
            startupList = new List<StartupObject>();

            startupList.AddRange(EnumerateKey(RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"), StartupType.REGISTRY32_CU));
            startupList.AddRange(EnumerateKey(RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"), StartupType.REGISTRY32_LM));

            if (Environment.Is64BitOperatingSystem)
            {
                startupList.AddRange(EnumerateKey(RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64).OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"), StartupType.REGISTRY64_CU));
                startupList.AddRange(EnumerateKey(RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"), StartupType.REGISTRY64_LM));
            }

            startupList.AddRange(EnumerateStartupFolder());

            if (listView1.InvokeRequired)
            {
                listView1.BeginInvoke(new Action(() =>
                {
                    AddListview();
                }));
            }
            else
            {
                AddListview();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Task.Run(GetObjects);
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            deleteToolStripMenuItem.Visible = false;
            openFolderToolStripMenuItem.Visible = false;
            openRegistryToolStripMenuItem.Visible = false;
            openFilepathToolStripMenuItem.Visible = false;

            if (listView1.SelectedItems.Count > 0)
            {
                openFilepathToolStripMenuItem.Visible = true;
                deleteToolStripMenuItem.Visible = true;

                try
                {
                    StartupObject obj = startupList.Single(x => x.ID.ToString() == listView1.SelectedItems[0].Name);

                    if (obj.Type == StartupType.FOLDER)
                    {
                        openFolderToolStripMenuItem.Visible = true;
                    }
                    else
                    {
                        openRegistryToolStripMenuItem.Visible = true;
                    }
                }
                catch(Exception)
                {

                }
            }
        }

        private void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                StartupObject obj = startupList.Single(x => x.ID.ToString() == listView1.SelectedItems[0].Name);

                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.Startup));
            }
            catch(Exception)
            {

            }
        }

        private void openFilepathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                StartupObject obj = startupList.Single(x => x.ID.ToString() == listView1.SelectedItems[0].Name);
                Debug.WriteLine(Path.GetDirectoryName(obj.Path));
                Process.Start(Path.GetDirectoryName(obj.Path));
            }
            catch (Exception)
            {

            }
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            Task.Run(GetObjects);
        }
    }
}

