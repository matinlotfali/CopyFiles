using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Copy_Files
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int RemSec, RemMin, RemHou;
        private void button1_Click(object sender, EventArgs e)
        {
            Thread A = new Thread(Job);
            A.Start();
            timer1.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            textBox3.Enabled = !checkBox1.Checked;
            label5.Enabled = !checkBox1.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBox1.Text = openFileDialog1.FileName;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                textBox2.Text = saveFileDialog1.FileName;
        }


        void Job()
        {
            Info.state = State.Initializing;            
            Info.Time = DateTime.Now;
            Info.Start = Info.Time;
            BinaryReader File = new BinaryReader(new FileStream(textBox1.Text, FileMode.Open, FileAccess.Read, FileShare.None));
            Info.FileLength = File.BaseStream.Length;
            Info.FileSize = 0;
            Info.FileSize0 = 0;
            byte Byte;
            BinaryWriter Copy;
            if (checkBox1.Checked)            
                Copy = new BinaryWriter(new FileStream(textBox2.Text, FileMode.Create, FileAccess.Write, FileShare.None));            
            else
            {
                Copy = new BinaryWriter(new FileStream(textBox2.Text, FileMode.Append, FileAccess.Write, FileShare.None));
                File.ReadBytes(Convert.ToInt32(textBox3.Text));
                Info.FileSize = Convert.ToInt64(textBox3.Text);
                Info.FileSize0 = Info.FileSize;
            }
            Info.state = State.Copying;
            while (Info.FileLength != Info.FileSize)
            {
                Byte = File.ReadByte();
                Copy.Write(Byte);
                Info.FileSize++;
                if (Info.state == State.Cancel)
                    break;
            }
            Info.state = State.Closing;
            Copy.Close();
            File.Close();
            Info.state = State.Ready;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            groupBox1.Enabled = false;
            button4.Enabled = true;
            switch (Info.state)
            {
                case State.Initializing:
                    label21.Text = "آماده سازی فایل و متغیرها...";
                    break;
                case State.Copying:
                    label21.Text = "در حال کپی کردن...";
                    label9.Text = Info.FileSize.ToString() + " Bytes";
                    label15.Text = Info.FileLength.ToString() + " Bytes";
                    label11.Text = ((int)((Info.FileSize - Info.FileSize0) / ((double)timer1.Interval / 1000) / 1024 / 1024)).ToString() + " MB/s";
                    label7.Text = (Info.FileSize / 1024 / 1024).ToString() + " MB";
                    label1.Text = (Info.FileSize / 1024).ToString() + " KB";
                    label17.Text = (Info.FileSize * 100 / Info.FileLength).ToString() + "%";
                    Info.Time = DateTime.Now;
                    label13.Text = String.Format("{0:D2} : {1:D2} : {2:D2}", (Info.Time - Info.Start).Hours, (Info.Time - Info.Start).Minutes, (Info.Time - Info.Start).Seconds);
                    RemSec = Convert.ToInt32(((Info.FileLength - Info.FileSize) / 1024) / ((Info.FileSize - Info.FileSize0) / ((double)timer1.Interval / 1000) / 1024 + 1)) + 1;
                    if (Info.FileSize == Info.FileLength)
                        RemSec = 0;
                    RemHou = RemSec / 3600;
                    RemSec %= 3600;
                    RemMin = RemSec / 60;
                    RemSec %= 60;
                    label18.Text = String.Format("{0:D2} : {1:D2} : {2:D2}", RemHou, RemMin, RemSec);
                    progressBar1.Maximum = Convert.ToInt32(Info.FileLength / 1024);
                    progressBar1.Value = Convert.ToInt32(Info.FileSize / 1024);

                    Info.FileSize0 = Info.FileSize;
                    Update();
                    break;
                case State.Closing:
                    label21.Text = "بستن فایل...";
                    label21.Update();
                    break;
                case State.Ready:
                    label21.Text = "آماده برای شروع";

                    label9.Text = Info.FileSize.ToString() + " Bytes";
                    label15.Text = Info.FileLength.ToString() + " Bytes";
                    label11.Text = ((int)((Info.FileLength) / (Info.Time - Info.Start).TotalSeconds / 1024 / 1024)).ToString() + " MB/s";
                    label7.Text = (Info.FileSize / 1024 / 1024).ToString() + " MB";
                    label1.Text = (Info.FileSize / 1024).ToString() + " KB";
                    label17.Text = (Info.FileSize * 100 / Info.FileLength).ToString() + "%";
                    Info.Time = DateTime.Now;
                    label13.Text = String.Format("{0:D2} : {1:D2} : {2:D2}", (Info.Time - Info.Start).Hours, (Info.Time - Info.Start).Minutes, (Info.Time - Info.Start).Seconds);
                    RemSec = Convert.ToInt32(((Info.FileLength - Info.FileSize) / 1024) / ((Info.FileSize - Info.FileSize0) / ((double)timer1.Interval / 1000) / 1024 + 1)) + 1;
                    if (Info.FileSize == Info.FileLength)
                        RemSec = 0;
                    RemHou = RemSec / 3600;
                    RemSec %= 3600;
                    RemMin = RemSec / 60;
                    RemSec %= 60;
                    label18.Text = String.Format("{0:D2} : {1:D2} : {2:D2}", RemHou, RemMin, RemSec);
                    progressBar1.Maximum = Convert.ToInt32(Info.FileLength / 1024);
                    progressBar1.Value = Convert.ToInt32(Info.FileSize / 1024);

                    Info.FileSize0 = Info.FileSize;
                    Update();

                    groupBox1.Enabled = true;
                    button4.Enabled = false;
                    timer1.Enabled = false;
                    Update();
                    break;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Info.state = State.Cancel;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Info.state != State.Ready)
                if (MessageBox.Show("آیا واقعا مایلید کپی را متوقف سازید؟","اخطار", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                    Info.state = State.Cancel;
                else
                    e.Cancel = true;
        }
    }
    class Info
    {
        public static DateTime Time, Start;
        public static long FileLength, FileSize, FileSize0;
        public static State state;
    }
    enum State { Ready, Initializing, Copying, Closing, Cancel }

}
