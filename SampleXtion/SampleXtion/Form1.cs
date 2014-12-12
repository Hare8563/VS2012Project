using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XtionLib;
using LabImg;
using OpenCvSharp;

namespace SampleXtion
{
    public partial class Form1 : Form
    {
        XtionUtility xtionData;

        public Form1()
        {
            InitializeComponent();
            xtionData = new XtionUtility(640,480, NodeMode.VGA);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null) pictureBox1.Image.Dispose();
            if (pictureBox3.Image != null) pictureBox3.Image.Dispose();

            Bitmap img = xtionData.doNormalDepth();
            Bitmap bmp=xtionData.doColor();

            CommonUtility.FillPicBox(bmp, pictureBox1);
            CommonUtility.FillPicBox(img, pictureBox3);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = xtionData.loadColorBar(pictureBox2.Width, pictureBox2.Height);
            timer1.Start();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string path = Environment.CurrentDirectory;
            xtionData.SaveDepthCSV(path);
        }
    }
}
