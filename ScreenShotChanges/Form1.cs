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
        Thread thMousePosGrabber;
        Thread thClickSecreenShot;
        bool goOn = false;
        bool goOn2 = false;
        ScreenCapture sc = new ScreenCapture();
        Bitmap thisImage = null;
        Bitmap lastImage = null;
        int trigPercent = 0;
        string savePath = "";
        int imageCntr = 0;

        int width = 2560;
        int height = 1600;
        int clickX=0;
        int clickY=0;
        Point leftTop = new Point(0,0);
        Point rightBottom = new Point(2560,1600);

        Point mouseClickPoint = new Point(0, 0);
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {


            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "" && textBox6.Text != "" && textBox5.Text != "" && textBox8.Text != "" && textBox7.Text != "")
            {
                start();
            }
            else
                MessageBox.Show("Set Path and Value");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        void start()
        {

            if (textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "" && textBox6.Text != "" && textBox5.Text != "" && textBox8.Text != "" && textBox7.Text != "")
            {
                trigPercent = Convert.ToInt32(textBox2.Text);
                width = Convert.ToInt32(textBox3.Text);
                height = Convert.ToInt32(textBox4.Text);
                savePath = textBox1.Text;
                imageCntr = 0;
                stop();

                leftTop = new Point(Convert.ToInt32(textBox6.Text), Convert.ToInt32(textBox5.Text));
                rightBottom = new Point(Convert.ToInt32(textBox8.Text), Convert.ToInt32(textBox7.Text));

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
                thisImage = sc.CaptureScreen(width,height);

                if(lastImage!=null)
                {
                    if(compare2Image(thisImage,lastImage)>=trigPercent)
                    {
                        
                       cropImage( thisImage,new Rectangle(leftTop.X,leftTop.Y,rightBottom.X-leftTop.X,rightBottom.Y-leftTop.Y)).Save(savePath + "\\" + DateTime.Now.ToString("yyMMddHHmmssfff")+".jpg", ImageFormat.Jpeg);
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

        Bitmap cropImage(Bitmap src, Rectangle cropRect)
        {
            Bitmap target = new Bitmap(cropRect.Width, cropRect.Height);

            using(Graphics g = Graphics.FromImage(target))
            {
               g.DrawImage(src, new Rectangle(0, 0, target.Width, target.Height), 
                                cropRect,                        
                                GraphicsUnit.Pixel);
            }
            return target;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stop();
            stopMouseGrabber();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            stop();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        int getMouseLocStatus = 0;
        private void button4_Click(object sender, EventArgs e)
        {
            
            if (goOn2)
            {
                stopMouseGrabber();
            }
            else
            {
                stopMouseGrabber();
                //right Left
                getMouseLocStatus = 1;
                goOn2 = true;
                thMousePosGrabber = new Thread(new ThreadStart(mouseGrabber));
                thMousePosGrabber.Start();
            }
        }

        void stopMouseGrabber()
        {
            goOn2 = false;
            if(thMousePosGrabber!=null)
            {
                thMousePosGrabber.Abort();
            }
        }

        void mouseGrabber()
        {
            while(goOn2)
            {
                if(getMouseLocStatus==2)
                {
                    textBox8.Invoke(new MethodInvoker(delegate { textBox8.Text = Cursor.Position.X.ToString(); }));
                    textBox7.Invoke(new MethodInvoker(delegate { textBox7.Text = Cursor.Position.Y.ToString(); }));
                }
                else
                {
                    if (getMouseLocStatus == 1)
                    {
                        textBox6.Invoke(new MethodInvoker(delegate { textBox6.Text = Cursor.Position.X.ToString(); }));
                        textBox5.Invoke(new MethodInvoker(delegate { textBox5.Text = Cursor.Position.Y.ToString(); }));
                    }
                }
            }
        }

        
        private void button5_Click(object sender, EventArgs e)
        {
            //
            if (goOn2)
            {
                stopMouseGrabber();
            }
            else
            {
                stopMouseGrabber();
                getMouseLocStatus = 2;
                goOn2 = true;
                thMousePosGrabber = new Thread(new ThreadStart(mouseGrabber));
                thMousePosGrabber.Start();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "" && textBox6.Text != "" && textBox5.Text != "" && textBox8.Text != "" && textBox7.Text != "" && textBox10.Text != "" && textBox9.Text != "")
            {
                start2();
            }
            else
                MessageBox.Show("Set Path and Value");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            stop2();
        }

        void start2()
        {

            if (textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "" && textBox6.Text != "" && textBox5.Text != "" && textBox8.Text != "" && textBox7.Text != "")
            {
                trigPercent = Convert.ToInt32(textBox2.Text);
                width = Convert.ToInt32(textBox3.Text);
                height = Convert.ToInt32(textBox4.Text);
                savePath = textBox1.Text;
                imageCntr = 0;
                stop2();

                leftTop = new Point(Convert.ToInt32(textBox6.Text), Convert.ToInt32(textBox5.Text));
                rightBottom = new Point(Convert.ToInt32(textBox8.Text), Convert.ToInt32(textBox7.Text));

                clickX = Convert.ToInt32(textBox9.Text);
                clickY = Convert.ToInt32(textBox10.Text);
                if (savePath != "")
                {
                    goOn = true;
                    thClickSecreenShot = new Thread(new ThreadStart(capture2));
                    thClickSecreenShot.Start();
                }
            }
        }

        void stop2()
        {
            goOn = false;
            if (thScreenShot != null)
            {
                thClickSecreenShot.Abort();
            }
        }

        void capture2()
        {
            while (goOn)
            {
                this.Invoke(new MethodInvoker(delegate { mouseClick.ClickOnPoint(this.Handle, new Point(clickX, clickY)); }));
                

                Thread.Sleep(300);

                thisImage = sc.CaptureScreen(width, height);

                if (lastImage != null)
                {
                    if (compare2Image(thisImage, lastImage) >= trigPercent)
                    {

                        cropImage(thisImage, new Rectangle(leftTop.X, leftTop.Y, rightBottom.X - leftTop.X, rightBottom.Y - leftTop.Y)).Save(savePath + "\\" + DateTime.Now.ToString("yyMMddHHmmssfff") + ".jpg", ImageFormat.Jpeg);
                        imageCntr++;
                    }
                }
                lastImage = thisImage;

                label7.Invoke(new MethodInvoker(delegate { label7.Text = imageCntr.ToString(); }));

                GC.Collect();
                Thread.Sleep(200);
            }

        }
    }
}
