namespace DSChk
{
    public partial class Form1 : Form
    {


        public Form1()
        {
            InitializeComponent();
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
            treeView1.Nodes.Clear();
            var node = treeView1.Nodes.Add(textBox1.Text);
            var dirList = new DirectoryInfo(textBox1.Text);

            long size = SetNode(node, dirList, false);
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
                var subNode = node?.Nodes.Add(di.Name);
                long ssize = 0;
                if (recursion)
                    ssize = SetNode(subNode, di, recursion);
                else
                    ssize = SetNode(null, di, recursion);

                if (subNode is not null)
                    subNode.Text += " (" + (ssize / 1024).ToString("#,0") + "KB)";
                csize += ssize;
            }

            return csize;
        }

        private void button2_Click(object sender, EventArgs e)
        {
        
        }

        public static long GetDirectorySize(DirectoryInfo dirInfo)
        {
            long size = 0;

            //フォルダ内の全ファイルの合計サイズを計算する
            foreach (FileInfo fi in dirInfo.GetFiles())
                size += fi.Length;

            //サブフォルダのサイズを合計していく
            foreach (DirectoryInfo di in dirInfo.GetDirectories())
                size += GetDirectorySize(di);

            //結果を返す
            return size;
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            try
            {
                TreeNode? node = e.Node;
                String? path = node?.FullPath;
                node?.Nodes.Clear();

                if (path == null) return;
                path = System.Text.RegularExpressions.Regex.Replace(path, " \\([0-9,]*KB\\)$", "");


                DirectoryInfo dirList = new DirectoryInfo(path);
                long size = SetNode(node, dirList, false);

            }
            catch (IOException ie)
            {
            }

        }
    }
}