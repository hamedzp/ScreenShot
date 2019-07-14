using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShotChanges
{
    public partial class Form1 : Form
    {
        Thread thScreenShot;
        bool goOn = true;
        ScreenCapture sc = new ScreenCapture();
        Bitmap thisImage = null;
        Bitmap lastImage = null;
        int trigPercent = 0;
        string savePath = "";
        int imageCntr = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(folderBrowserDialog1.ShowDialog()==DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                start();
            }
            else
                MessageBox.Show("Set Path and Value");
        }

        void start()
        {

            if (textBox2.Text != "")
            {
                trigPercent = Convert.ToInt32(textBox2.Text);
                savePath = textBox1.Text;
                imageCntr = 0;
                stop();

                if (savePath != "")
                {
                    goOn = true;
                    thScreenShot = new Thread(new ThreadStart(capture));
                    thScreenShot.Start();
                }
            }
        }

        void stop()
        {
            goOn = false;
            if (thScreenShot != null)
            {
                thScreenShot.Abort();
            }
        }

        void capture()
        { 
            while(goOn)
            {
                thisImage = sc.CaptureScreen();

                if(lastImage!=null)
                {
                    if(compare2Image(thisImage,lastImage)>=trigPercent)
                    {
                        thisImage.Save(savePath + "\\" + DateTime.Now.ToString("yyMMddHHmmssfff")+".jpg", ImageFormat.Jpeg);
                        imageCntr++;
                    }
                }
                lastImage = thisImage;

                label1.Invoke(new MethodInvoker(delegate { label1.Text = imageCntr.ToString(); }));

                GC.Collect();
                Thread.Sleep(500);
            }

        }

        internal int compare2Image(Bitmap vorudi1,Bitmap vorudi2)
        {
            int changeCntr = 0;
            int myheight = vorudi1.Height;
            int mywidth = vorudi1.Width;

            BitmapData data1 = vorudi1.LockBits(new Rectangle(0, 0, vorudi1.Width, vorudi1.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            BitmapData data2 = vorudi2.LockBits(new Rectangle(0, 0, vorudi2.Width, vorudi2.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                byte* ptr1 = (byte*)data1.Scan0;
                byte* ptr2 = (byte*)data2.Scan0;

                int remain1 = data1.Stride - data1.Width * 3;
                int remain2 = data2.Stride - data2.Width * 3;


                for (int i = 0; i < myheight; i++)
                {
                    for (int j = 0; j < mywidth; j++)
                    {
                        if((*ptr1)!=(*ptr2) || *(ptr1 + 1)!=*(ptr2 + 1) || *(ptr1 + 2)!=*(ptr2 + 2))
                        {
                            changeCntr++;
                        }

                        ptr1 += 3;
                        ptr2 += 3;
                    }
                    ptr1 += remain1;
                    ptr2 += remain2;
                }

            }

            vorudi1.UnlockBits(data1);
            vorudi2.UnlockBits(data2);

            return changeCntr*100/(myheight*mywidth);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stop();
        }


    }
}
