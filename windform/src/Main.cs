using envload.Models;
using envload.Utils;
using LoadEnv.Models;
using System.Data;

namespace envload
{
    public partial class Main : Form
    {
        EnvironmentDto envs;

        public Main()
        {
            InitializeComponent();
            Settings s = FileUtils.InitSettings();

            textBoxWorkspace.Text = s.Workspace;
            LoadFiles(s.Workspace != null ? s.Workspace : "C:\\");
            comboBoxFiles.Text = s.LastFile;
            Width = s.Width != 0 ? s.Width : 750;
            Height = s.Height != 0 ? s.Height : 500;
            LoadFile();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.InitialDirectory = textBoxWorkspace.Text;
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    textBoxWorkspace.Text = folderBrowserDialog.SelectedPath;
                    LoadFiles(folderBrowserDialog.SelectedPath);

                }
                dataGridView1.DataSource = null;
            }
        }

        public void LoadFiles(string path)
        {
            string[] files = Directory.GetFiles(path);
            comboBoxFiles.Items.Clear();
            comboBoxFiles.Text = "";
            foreach (string file in files)
                comboBoxFiles.Items.Add(Path.GetFileName(file));
            if (files != null && files.Length > 0)
                comboBoxFiles.SelectedIndex = 0;
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            LoadFile();
        }

        private void buttonInyect_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            FileUtils.InyectEnviroments(envs, false);
            Cursor = Cursors.Default;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            FileUtils.InyectEnviroments(envs, true);
            Cursor = Cursors.Default;
        }

        private void comboBoxFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadFile();
        }

        private void LoadFile()
        {
            if (comboBoxFiles.Text != null && !comboBoxFiles.Text.Equals(""))
            {
                using StreamReader r = new(textBoxWorkspace.Text + "\\" + comboBoxFiles.Text);

                envs = FileUtils.TryDeserialiceEnviroments(r);

                if (envs != null && envs.values != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("Name", typeof(string));
                    dt.Columns.Add("Value", typeof(string));

                    foreach (var v in envs.values)
                    {
                        dt.Rows.Add(v.name, v.value);
                    }

                    dataGridView1.DataSource = dt;
                    dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }



            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Settings s = new()
            {
                Workspace = textBoxWorkspace.Text,
                LastFile = comboBoxFiles.Text,
                Width = Width,
                Height = Height
            };


            FileUtils.Save(FileUtils.pathFileSettings, s);
        }
    }
}