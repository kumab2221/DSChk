using System.Collections.Specialized;

namespace DSChk
{
    public partial class Form1 : Form
    {
        HashSet<string> hashlist;

        public Form1()
        {
            InitializeComponent();
            hashlist = new HashSet<string>();
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            IDataObject? data = e.Data;
            if (data is null) return;
            string[] sFileName = (string[])data.GetData(DataFormats.FileDrop, false);

            if (sFileName.Length <= 0) return;
            TextBox? targetTextBox = sender as TextBox;

            if (targetTextBox is null)
                return;

            targetTextBox.Text = "";
            targetTextBox.Text = sFileName[0];
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            IDataObject? data = e.Data;
            if (data is null) return;
            string[] sFileName = (string[])data.GetData(DataFormats.FileDrop);

            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                foreach (string sTemp in sFileName)
                    if (Directory.Exists(sTemp) == false) return;
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            fbd.Description = "フォルダを指定してください。";
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            fbd.ShowNewFolderButton = true;
            if (fbd.ShowDialog(this) == DialogResult.OK)
                textBox1.Text = fbd.SelectedPath;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MakeTreeView();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            string fileName = "Output" + dt.ToString("yyyyMMdd") + ".csv";
            using (StreamWriter sw = new StreamWriter(fileName, false, System.Text.Encoding.UTF8))
            {
                DirectoryInfo dirList = new DirectoryInfo(textBox1.Text);
                OutputDirectorySize(sw, dirList, 0);
            }
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            try
            {
                TreeNode? node = e.Node;
                String? path = node?.FullPath;

                if (path == null) return;
                path = System.Text.RegularExpressions.Regex.Replace(path, " \\([0-9,]*KB\\)", "");
                if (hashlist.Contains(path))
                    return;

                node?.Nodes.Clear();

                DirectoryInfo dirList = new DirectoryInfo(path);

                //サブフォルダのサイズを合計していく
                foreach (DirectoryInfo di in dirList.GetDirectories())
                {
                    var subNode = node?.Nodes.Add(di.Name);
                    long ssize = 0;
                    ssize = SetNode(subNode, di, false);
                    if (subNode is not null)
                        subNode.Text += " (" + (ssize / 1024).ToString("#,0") + "KB)";
                }

                hashlist.Add(path);

            }
            catch (IOException ie)
            {

            }

        }

        private void MakeTreeView()
        {

            treeView1.Nodes.Clear();
            var node = treeView1.Nodes.Add(textBox1.Text);
            var dirList = new DirectoryInfo(textBox1.Text);
            long size = 0;

            //サブフォルダのサイズを合計していく
            foreach (DirectoryInfo di in dirList.GetDirectories())
            {
                var subNode = node?.Nodes.Add(di.Name);
                long ssize = 0;
                ssize = SetNode(subNode, di, false);
                if (subNode is not null)
                    subNode.Text += " (" + (ssize / 1024).ToString("#,0") + "KB)";
                size += ssize;
            }

            node.Text += " (" + (size / 1024).ToString("#,0") + "KB)";
        }


        private long SetNode(TreeNode? node, DirectoryInfo dirInfo, bool recursion)
        {
            long csize = 0;

            //フォルダ内の全ファイルの合計サイズを計算する
            foreach (FileInfo fi in dirInfo.GetFiles())
                csize += fi.Length;

            //サブフォルダのサイズを合計していく
            foreach (DirectoryInfo di in dirInfo.GetDirectories())
            {
                long ssize = 0;
                var subNode = node?.Nodes.Add(di.Name);
                if (recursion)
                {
                    ssize = SetNode(subNode, di, recursion);
                    if (subNode is not null)
                        subNode.Text += " (" + (ssize / 1024).ToString("#,0") + "KB)";
                }
                else
                {
                    ssize = SetNode(null, di, recursion);
                }
                csize += ssize;
            }
            return csize;
        }

        

        public static long OutputDirectorySize( StreamWriter sw, DirectoryInfo dirInfo, int count)
        {
            long size = 0;

            //フォルダ内の全ファイルの合計サイズを計算する
            foreach (FileInfo fi in dirInfo.GetFiles())
                size += fi.Length;

            //サブフォルダのサイズを合計していく
            foreach (DirectoryInfo di in dirInfo.GetDirectories())
                size += OutputDirectorySize( sw, di, count++);

            //結果を返す
            sw.WriteLine(String.Format("{0},{1}", dirInfo.FullName, " (" + (size / 1024).ToString("#,0") + "KB)"));

            return size;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = Properties.Settings.Default.path;

            dataGridView1.RowCount = 100;
            var pathLit = Properties.Settings.Default.pathList;
            var outputList = Properties.Settings.Default.outputFlg;
            if (pathLit == null || outputList == null)
            {
                for (int i = 0; i < 100; i++)
                {
                    dataGridView1[1, i].Value = "";
                    dataGridView1[0, i].Value = false;
                }
                return;
            }
            for (int i=0; i<100; i++)
            {
                dataGridView1[1, i].Value = pathLit[i];
                if (outputList[i] == "0")
                    dataGridView1[0, i].Value = false;
                else
                    dataGridView1[0, i].Value = true;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.path = textBox1.Text;

            string[] pathArray = new string[100];
            string[] flgArray = new string[100];
            
            var pathCol = new StringCollection();
            var flgCol = new StringCollection();

            for (int i = 0; i < 100; i++)
            {
                pathArray[i] = dataGridView1[1, i].Value?.ToString() ?? "";

                bool flg = (bool)(dataGridView1[0, i].Value);
                if (flg)
                    flgArray[i] = "1";
                else
                    flgArray[i] = "0";

            }

            pathCol.AddRange(pathArray);
            flgCol.AddRange(flgArray);

            Properties.Settings.Default.pathList = pathCol;
            Properties.Settings.Default.outputFlg = flgCol;

            Properties.Settings.Default.Save();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 100; ++i)
            {
                dataGridView1[0, i].Value = false;
                dataGridView1[1, i].Value = "";
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;
            var dirName = "output" + dt.ToString("yyyyMMdd");
            if (!System.IO.Directory.Exists(dirName))
                System.IO.Directory.CreateDirectory(dirName);

            for (int i = 0; i < 100; ++i)
            {
                if ((bool)dataGridView1[0, i].Value == false) continue;

                var path = dataGridView1[1, i].Value.ToString();
                if (System.IO.Directory.Exists(path) == false) continue;

                var fileName = dirName + "\\" + dt.ToString("yyyyMMdd") + "_" + i.ToString() + ".csv";
                using (StreamWriter sw = new StreamWriter(fileName, false, System.Text.Encoding.UTF8))
                {
                    DirectoryInfo dirList = new DirectoryInfo(path);
                    OutputDirectorySize(sw, dirList, 0);
                }
            }
            MessageBox.Show("完了しました");
        }
    }
}