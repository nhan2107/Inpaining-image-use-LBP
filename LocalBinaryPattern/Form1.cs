using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LocalBinaryPattern
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //1. Open a file open browser and ask user for entering an Image
            // The region to be inpainted must be colored with Red in Paint
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (ofd.FileName == null)
                return;
            pictureBox1.Image = Bitmap.FromFile(ofd.FileName);
            // The image may of of any size. resize it to fit picturebox1. We have used 256x256 standard size
            Resize();
            // Obtain mask from source image
            
            
        }
        Bitmap ObtainMask(Bitmap Src)
        {

            //1.  Take a local image bmp which we will use for mark scanning
            Bitmap bmp = (Bitmap)Src.Clone();
            int NumRow = pictureBox1.Height;
            int numCol = pictureBox1.Width;
            //2. Mask image is initially an image of the same size of source
            Bitmap mask = new Bitmap(pictureBox1.Width, pictureBox1.Height);// GRAY is the resultant matrix 
            //3. We will define a structuring element of size bnd for dilating the mask
            // You can obtain this mask thickening variable from user. but 3 is standard for 256x256 images
            int bnd=3;
            //4. Loop through the rows and columns of images
            for (int i = 0; i < NumRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    Color c = bmp.GetPixel(j, i);// Extract the color of a pixel 
                    int rd = c.R; int gr = c.G; int bl = c.B;// extract the red,green, blue components from the color.
                    // Note that you had painted the region with red. But image is resized, which is resampling
                    // in resizing process, the color tend to get changed. so we will look for more redish pixels rater than red.
                    //you can also update this by picking the mask color through mouse click.
                    if ((rd > 220) && (gr < 80) && (bl < 80))
                    {
                        Color c2 = Color.FromArgb(255, 255, 255);
                        //5. set the marked pixel od Is as white in Im
                        mask.SetPixel(j, i, c2);

                        //6. Perform dilation( extending the mask area to nullify edge effect
                        for (int ib = i - bnd; ib < i + bnd; ib++)
                        {
                            for (int jb = j - bnd; jb < j + bnd; jb++)
                            {
                                try
                                {
                                    // see we are making the boundary of pixels also white
                                    mask.SetPixel(jb, ib, c2);
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                    else
                    {
                        //7. all other pixels are black
                        Color c2 = Color.FromArgb(0, 0, 0);
                        mask.SetPixel(j, i, c2);
                        try
                        {

                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
            return mask;
        }
        void Resize()
        {
            Bitmap bmp = new Bitmap(pictureBox1.Image, new Size(pictureBox1.Width, pictureBox1.Height)); pictureBox1.Image = bmp;
        }
        Bitmap GrayConversion(Bitmap srcBmp)
        {
            Bitmap bmp = srcBmp;
            int NumRow = pictureBox1.Height;
            int numCol = pictureBox1.Width;
            Bitmap GRAY = new Bitmap(pictureBox1.Width, pictureBox1.Height);// GRAY is the resultant matrix 

            for (int i = 0; i < NumRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    Color c = bmp.GetPixel(j, i);// Extract the color of a pixel 
                    int rd = c.R; int gr = c.G; int bl = c.B;// extract the red,green, blue components from the color.
                    double d1 = 0.2989 * (double)rd + 0.5870 * (double)gr + 0.1140 * (double)bl;
                    int c1 = (int)Math.Round(d1);
                    Color c2 = Color.FromArgb(c1, c1, c1);
                    GRAY.SetPixel(j, i, c2);
                }
            }
            return  GRAY;
        }
        double Bin2Dec(List<int> bin)
        {
            double d = 0;

            for (int i = 0; i < bin.Count; i++)
            {
                d += bin[i]*Math.Pow(2, i);
            }
            return d;
        }
        Bitmap LBP(Bitmap srcBmp,int R)
        {
            // We want to get LBP image from srcBmp and window R
            Bitmap bmp = srcBmp;
            //1. Extract rows and columns from srcImage . Note Source image is Gray scale Converted Image
            int NumRow = srcBmp.Height;
            int numCol = srcBmp.Width;
            Bitmap lbp = new Bitmap(numCol, NumRow);
            Bitmap GRAY = new Bitmap(pictureBox1.Width, pictureBox1.Height);// GRAY is the resultant matrix 
            double[,] MAT = new double[numCol, NumRow];
            double max = 0.0;
            //2. Loop through Pixels
            for (int i = 0; i < NumRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                  //  Color c1=Color.FromArgb(0,0,0);
                    MAT[j, i] = 0;
                    //lbp.SetPixel(j, i,c1) ;
                    
                    
                    //define boundary condition, other wise say if you are looking at pixel (0,0), it does not have any suitable neighbors
                    if ((i > R) && (j > R) && (i < (NumRow - R)) && (j < (numCol - R)))
                    {
                        // we want to store binary values in a List
                        List<int> vals = new List<int>();
                        try
                        {
                            for (int i1 = i - R; i1 < (i + R); i1++)
                            {
                                for (int j1 = j - R; j1 < (j + R); j1++)
                                {
                                    int acPixel = srcBmp.GetPixel(j, i).R;
                                    int nbrPixel = srcBmp.GetPixel(j1, i1).R;
                                    // 3. This is the main Logic of LBP
                                    if (nbrPixel > acPixel)
                                    {
                                        vals.Add(1);

                                    }
                                    else
                                    {
                                        vals.Add(0);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                        //4. Once we have a list of 1's and 0's , convert the list to decimal
                        // Also for normalization purpose calculate Max value
                        double d1 = Bin2Dec(vals);
                        MAT[j, i] = d1;
                        if (d1 > max)
                        {
                            max = d1;
                        }
                        }
                    
                    //////////////////

                    
                    
                }
            }
            //5. Normalize LBP matrix MAT an obtain LBP image lbp
            lbp = NormalizeLbpMatrix(MAT, lbp, max);
            return lbp;
        }
        Bitmap NormalizeLbpMatrix(double[,]Mat,Bitmap lbp,double max)
        {
            int NumRow = lbp.Height;
            int numCol = lbp.Width;
            for (int i = 0; i < NumRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {
                    // see the Normalization process of dividing pixel by max value and multiplying with 255
                    double d = Mat[j, i] / max;
                    int v = (int)(d * 255);
                    Color c = Color.FromArgb(v, v, v);
                    lbp.SetPixel(j, i, c);
                }
            }
            return lbp;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = GrayConversion((Bitmap)pictureBox1.Image);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = LBP((Bitmap)pictureBox2.Image, int.Parse(textBox1.Text));
        }

        private void pictureBox4_MouseDown(object sender, MouseEventArgs e)
        {
            
        }
        Bitmap Rslt;
        int Si=0;
        void Inpaint()
        {
            //1. Obtain Mask
            Bitmap mask = (Bitmap)pictureBox4.Image;
            int NumRow = pictureBox1.Height;
            int numCol = pictureBox1.Width;
            //2. Define resultant image same as source region. As algo proceeds, we need to replace the marked blocks
            
            Rslt = (Bitmap)pictureBox1.Image;// GRAY is the resultant matrix 
            Bitmap src = (Bitmap)pictureBox3.Image;

            //3. Define the block for Inpainting purpose
            int Blk = int.Parse(textBox2.Text);
            for (int i = 0; i < NumRow; i++)
            {
                for (int j = 0; j < numCol; j++)
                {

                    Color c = mask.GetPixel(j, i);// Extract the color of a pixel from mask (p) 
                    int rd = c.R; int gr = c.G; int bl = c.B;// extract the red,green, blue components from the color.
                    int ti = -1, tj = -1;
                    double dst = 99999999999999.0;
                    //4. check if the pixel is white ( that means marked pixel in source)
                    if ((rd == 255) && (gr == 255) && (bl == 255))
                    {
                        //5. Generate the neighbors List
                        List<int[]> Nbrs = new List<int[]>();
                        for (int i1 = i - Blk; i1 < i + Blk; i1++)
                        {
                            for (int j1 = j; j1 < j + Blk; j1++)
                            {
                                try
                                {
                                    Color c1 = src.GetPixel(j1, i1);// Extract the color of a pixel from LBP image 
                                    int rd1 = c1.R; int gr1 = c1.G; int bl1 = c1.B;// extract the red,green, blue components from the color.

                                    Color c2 = mask.GetPixel(j1, i1);// Extract the color of a mask pixel 
                                    // remember list can not contain a pixel which also is within mask region

                                    int rd2 = c2.R; int gr2 = c2.G; int bl2 = c2.B;// extract the red,green, blue components from the color.
                                    // form the list with non marked pixel
                                    if ((rd2 == 0) && (gr2 == 0) && (bl2 == 0))
                                    {
                                        // add first pixel as it is, as there is nothing to compare for
                                        if (Nbrs.Count == 0)
                                        {
                                            Nbrs.Add(new int[] { i1, j1 });
                                        }
                                        else
                                        {
                                            double d = 0;
                                            //6. calculate mean distance of the current pixel with all neighbors
                                            for (int k = 0; k < Nbrs.Count; k++)
                                            {
                                                int[] pos = Nbrs[k];
                                                d = d + Math.Abs(Rslt.GetPixel(pos[1], pos[0]).R - rd2);
                                            }
                                            d = d / (double)Nbrs.Count;
                                            // 7. update ps value which will be used to replace p in original image
                                            if (d < dst)
                                            {
                                                dst = d;
                                                ti = i1;
                                                tj = j1;
                                            }
                                        }
                                    }

                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        //8. replace p with ps in the actual image
                        Rslt.SetPixel(j, i, Rslt.GetPixel(tj, ti));
                        System.Threading.Thread.Sleep(10);

                    }
                    else
                    {
              
                    }
                }
            }
           // pictureBox5.Image = Rslt;
            //timer1.Enabled = false;
            s = "DONE";
        }
        System.Threading.Thread t;
        string s = "processing";
        private void button4_Click(object sender, EventArgs e)
        {
            Si = 0;
            this.Text = "Processing";
             t= new System.Threading.Thread(Inpaint);
            t.Start();
            timer1.Enabled = true;
                
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            pictureBox5.Image.Save("Rslt.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            if(t.ThreadState==System.Threading.ThreadState.Running)
            t.Suspend();

            try
            {
                pictureBox5.Image = (Bitmap)Rslt.Clone();
                pictureBox5.Image.Save("Result-" + Si + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                Si++;
            }
            catch (Exception ex)
            {
            }
            if (t.ThreadState == System.Threading.ThreadState.Suspended)
            t.Resume();
            this.Text = s;
            if (s == "DONE")
            {
                return;
            }
            timer1.Enabled = true;
        }

        public Image OverlayImage(Image baseImage, Image topImage, float transparency)
        {
            System.Drawing.Imaging.ImageAttributes ia = new System.Drawing.Imaging.ImageAttributes();
            System.Drawing.Imaging.ColorMatrix cm = new System.Drawing.Imaging.ColorMatrix();
            cm.Matrix33 = transparency;
            ia.SetColorMatrix(cm);
            Graphics g = null;
            try
            {
                g = Graphics.FromImage(baseImage);
                Rectangle rect = new Rectangle((int)(topImage.Width * 0.1), (int)(topImage.Height * 0.2), (int)(topImage.Width * 0.8), (int)(topImage.Height * 0.6));
                // YOU MAY DEFINE THE RECT AS THIS AS WELL
                //Rectangle rect = new Rectangle(0, 0, baseImage.Width,      baseImage.Height, baseImage.Width);
                g.DrawImage(topImage, rect, 0, 0, topImage.Width, topImage.Height, GraphicsUnit.Pixel, ia);
                return baseImage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                g.Dispose();
                topImage.Dispose();
            }

        }
        private void button7_Click(object sender, EventArgs e)
        {
            pictureBox3.Image = OverlayImage(pictureBox1.Image, pictureBox4.Image,0.4f);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //1. Open a file open browser and ask user for entering an Image
            // The region to be inpainted must be colored with Red in Paint
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (ofd.FileName == null)
                return;
            pictureBox4.Image = Bitmap.FromFile(ofd.FileName);
            // The image may of of any size. resize it to fit picturebox1. We have used 256x256 standard size
            Bitmap bmp = new Bitmap(pictureBox4.Image, new Size(pictureBox4.Width, pictureBox4.Height)); 
            pictureBox3.Image = bmp;
            // Obtain mask from source image
        }
    }
}
