using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.IO;
using System.Threading;


namespace WindowsFormsApplaba2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            raycastingPickcherbox.Image = new Bitmap(1, 1);
        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }

        public int size;
        public float MAX;
        public float MIN;
        public bool flag;
        public float isosurfValue1;
        public float isosurfValue2;
        public float isosurfValue3;
        public static float[,,] array;

        private void открытьUint8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bin reader = new Bin();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "RAW files | *.RAW; | All Files(*.*) | *.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.FileName;
                if (File.Exists(path))
                {
                    string[] tmpArray = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    string fileName = tmpArray.Last();
                    tmpArray = fileName.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                    size = int.Parse(tmpArray.First());
                    reader.readUNIT8(path, size);
                }
            }
        }
        private void открытьFloat32ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Bin reader = new Bin();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "RAW files | *.RAW; | All Files(*.*) | *.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.FileName;
                if (File.Exists(path))
                {
                    string[] tmpArray = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                    string fileName = tmpArray.Last();
                    tmpArray = fileName.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                    size = int.Parse(tmpArray.First());
                    reader.readFLOAT32(path, size);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            parse();
        }

        void parse()
        {
            float.TryParse(MaxBox.Text, out MAX);
            float.TryParse(MinBox.Text, out MIN);
            float.TryParse(TreshHold.Text, out isosurfValue1);
            float.TryParse(treshbox2.Text, out isosurfValue2);
            float.TryParse(treshbox3.Text, out isosurfValue3);
        }

        public Thread[] niceiniz(int Tnumber)
        {
            Thread[] threadArray = new Thread[Tnumber];
            for (int i = 0; i < Tnumber; i++)
            {
                threadArray[i] = new Thread(TreadFucn);
                threadArray[i].IsBackground = true;
            }

            return threadArray;
        }
        public void threadStart(Thread[] threadArray, int Tnumber)
        {
            for (int i = 0; i < Tnumber; i++)
            { threadArray[i].Start(); }
            while (
                    (threadArray[0].IsAlive) &&
                    (threadArray[Tnumber - 1].IsAlive)
                    )
            {

                if (counter > (rgbValues.Length / 100))
                {
                    progressBar1.Value = counter / (rgbValues.Length / 100);
                }
                Thread.Sleep(0);
            }

        }
        public void threadEnd(Thread[] threadArray, int Tnumber)
        {
            for (int i = 0; i < Tnumber; i++)
            { threadArray[i].Abort(); }
        }


        public float[,,] points;
        public int raySize;
        public float[] direction;
        public int[] exist;
        public byte[] rgbValues;

        public int counter = 0;

        private void button2_Click(object sender, EventArgs e)
        {
            parse();
            raySize = (int)RenderQuality.Value;
            Bitmap resultImage = new Bitmap(raycastingPickcherbox.Image, new Size(raySize, raySize));
            direction = new float[3];
            float[] P0 = new float[3];
            points = new float[raySize, raySize, 3];
            P0[0] = (float)X0.Value;
            P0[1] = (float)Y0.Value;
            P0[2] = (float)Z0.Value;
            direction = generateZeroVector(P0);
            points = RayCasting(direction, points, raySize, P0);
            Rectangle rect = new Rectangle(0, 0, resultImage.Width, resultImage.Height);
            System.Drawing.Imaging.BitmapData bmpData =
               resultImage.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                resultImage.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * resultImage.Height;
            int stride = Math.Abs(bmpData.Stride);
            rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            counter = 0;
            exist = new int[rgbValues.Length / 4];
            int threadQuantity = 4;

            Thread[] threadArray = niceiniz(threadQuantity);
            threadStart(threadArray, threadQuantity);
            threadEnd(threadArray, threadQuantity);

            progressBar1.Value = 0;
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            resultImage.UnlockBits(bmpData);

            raycastingPickcherbox.Image = resultImage;
            raycastingPickcherbox.Refresh();
        }

        float[,,] RayCasting(float[] direction, float[,,] Plane, int Rsize, float[] P0)
        {
            float Q = (float)Math.Atan(Math.Sqrt(direction[0] * direction[0] + direction[1] * direction[1]) / direction[2]);
            float fi = (float)Math.Atan(direction[1] / direction[0]);
            float koef = (float)Rsize / size;
            double Qx = Q;
            double fix = fi + Math.PI / 2;
            double Qy = Q + Math.PI / 2;
            double fiy = fi;
            float temp1 = (float)((koef * Math.Sin(Qx) * Math.Cos(fix) + (koef * Math.Sin(Qy) * Math.Cos(fiy))) * ((koef * Math.Sin(Qx) * Math.Cos(fix)) + (koef * Math.Sin(Qy) * Math.Cos(fiy))));
            temp1 += (float)(((koef * Math.Sin(Qx) * Math.Sin(fix)) + (koef * Math.Sin(Qy) * Math.Sin(fiy))) * ((koef * Math.Sin(Qx) * Math.Sin(fix)) + (koef * Math.Sin(Qy) * Math.Sin(fiy))));
            temp1 += (float)(koef * Math.Cos(Qx) + koef * Math.Cos(Qy) * koef * Math.Cos(Qx) + koef * Math.Cos(Qy));
            temp1 = (float)Math.Sqrt(temp1);
            temp1 = (temp1 + temp1 * (float)(Zoom.Value - 100) / 100f) / 3;
            koef = koef / temp1 / temp1;
            for (int x = -Rsize / 2; x < Rsize / 2; x++)
                for (int y = -Rsize / 2; y < Rsize / 2; y++)
                {
                    Plane[Rsize / 2 + x, Rsize / 2 + y, 0] = P0[0] + x * (float)(koef * Math.Sin(Qx) * Math.Cos(fix)) + y * (float)(koef * Math.Sin(Qy) * Math.Cos(fiy));
                    Plane[Rsize / 2 + x, Rsize / 2 + y, 1] = P0[1] + x * (float)(koef * Math.Sin(Qx) * Math.Sin(fix)) + y * (float)(koef * Math.Sin(Qy) * Math.Sin(fiy));
                    Plane[Rsize / 2 + x, Rsize / 2 + y, 2] = P0[2] + x * (float)(koef * Math.Cos(Qx)) + y * (float)(koef * Math.Cos(Qy));
                }
            return Plane;
        }
        float[] generateZeroVector(float[] P0)
        {
            float[] d = new float[3];
            d[0] = (size / 2f - P0[0]);
            d[1] = (size / 2f - P0[1]);
            d[2] = (size / 2f - P0[2]);
            float max = Math.Abs(d[0]);
            for (int i = 1; i < d.Length; i++)
                if (max < Math.Abs(d[i]))
                    max = Math.Abs(d[i]);
            for (int i = 0; i < d.Length; i++)
                d[i] = d[i] / max;
            return d;
        }

        byte[] intenseColors(ref float intense0, byte[] tem)
        {
            float temintens = intense0;
            byte[] temARGB = new byte[4];
            temARGB = tem;
            temARGB[3] += 85;
            temARGB[1] = (byte)(255 - Clamp((int)((temintens - MIN) * 255 / (MAX - MIN)), 0, 255));
            temARGB[2] = (byte)(255 - Clamp((int)(255 * Math.Sin(Math.PI * ((temintens - MIN) / (MAX - MIN)) / 2)), 0, 255));
            temARGB[0] = (byte)(255 - Clamp((int)((temintens - MIN) * 255 / (MAX - MIN)), 0, 255));
            return temARGB;
        }

        byte[] RayPix2(float x0, float y0, float z0, float[] d)
        {
            //float[] tempintens = calculateIntense(x0, y0, z0, d[0], d[1], d[2]);
            float[] tempintens = { 0f, 0f,  };
            byte[] tempARGB = new byte[4];
            if (tempintens[0] != 0)
            {
                if (tempintens[0] > isosurfValue1)
                {
                    tempARGB = intenseColors(ref tempintens[0], tempARGB);
                }
                if (tempintens[1] > isosurfValue2)
                {
                    tempARGB = intenseColors(ref tempintens[1], tempARGB);
                }
                if (tempintens[2] > isosurfValue3)
                {
                    tempARGB = intenseColors(ref tempintens[2], tempARGB);
                }
                return tempARGB;
            }
            else return tempARGB;
        }

        float[] calculateIntense(float x0, float y0, float z0, float dx, float dy, float dz)
        {
            int visibility = (int)numericUpDown4.Value;
            float vis = 95 / 100 + visibility / 20;

            int layers = 3;
            float[] intens = new float[layers];
            float x = x0;
            float y = y0;
            float z = z0;
            float[] transparency = new float[8];
            for (int ii = 0; ii < 8; ii++)
            {
                transparency[ii] = vis + ii / 10;
                if (transparency[ii] > 0.99)
                    transparency[ii] = 1;
            }
            int[] intersectIsosurfCount = new int[layers];
            foreach (int i in intersectIsosurfCount)
            {
                intersectIsosurfCount[i] = 1;
            }
            for (int i = 0; i < 1000; i++)
            {
                if ((i + 1) % 100 == 0)
                {
                    if ((x > size) && (dx > 0) || (x < 0) && (dx < 0) || 
                        (y > size) && (dy > 0) || (y < 0) && (dy < 0) ||
                        (z > size) && (dz > 0) || (z < 0) && (dz <= 0))
                        return intens;
                }
                if ((x > 0) && (x < size) && (y > 0) && (y < size) && (z > 0) && (z < size))
                {
                    if ((intersectIsosurfCount[0] > 1) && (intersectIsosurfCount[0] < 8))
                    {

                        intens[0] += Bin.array[(int)x, (int)y, (int)z] * (1 - transparency[intersectIsosurfCount[0]]) / intersectIsosurfCount[0];
                        intersectIsosurfCount[0]++;
                    }
                    else if ((Bin.array[(int)x, (int)y, (int)z] > isosurfValue1) && (intersectIsosurfCount[0] < 8))
                    {
                        intens[0] += (vis) * Bin.array[(int)x, (int)y, (int)z];
                        intersectIsosurfCount[0]++;
                    }
                    if ((intersectIsosurfCount[1] > 1) && (intersectIsosurfCount[1] < 8))
                    {
                        intens[1] += Bin.array[(int)x, (int)y, (int)z] * (1 - transparency[intersectIsosurfCount[1]]) / intersectIsosurfCount[1];
                        intersectIsosurfCount[1]++;
                    }
                    else if ((Bin.array[(int)x, (int)y, (int)z] > isosurfValue2) && (intersectIsosurfCount[1] < 8))
                    {
                        intens[1] += (vis) * Bin.array[(int)x, (int)y, (int)z];
                        intersectIsosurfCount[1]++;
                    }
                    if ((intersectIsosurfCount[2] > 1) && (intersectIsosurfCount[2] < 8))

                    {
                        intens[2] += Bin.array[(int)x, (int)y, (int)z] * (1 - transparency[intersectIsosurfCount[2]]) / intersectIsosurfCount[2];
                        intersectIsosurfCount[2]++;
                        if (intersectIsosurfCount[2] == 8)
                            return intens;
                    }
                    else if ((Bin.array[(int)x, (int)y, (int)z] > isosurfValue3) && (intersectIsosurfCount[2] < 8))

                    {
                        intens[2] += (vis)*Bin.array[(int)x, (int)y, (int)z];
                        intersectIsosurfCount[2]++;
                    }

                }
                else if (i == 0)
                {
                    int Jump = 0;

                    int jumpx = (int)((Math.Abs(x - size / 2) - size) / Math.Abs(dx));
                    int jumpy = (int)((Math.Abs(y - size / 2) - size) / Math.Abs(dy));
                    int jumpz = (int)((Math.Abs(z - size / 2) - size) / Math.Abs(dz));
                    Jump = (int)Math.Min(jumpx, Math.Min(jumpy, jumpz));
                   
                    x += dx * Jump;
                    y += dy * Jump;
                    z += dz * Jump;
                }
                x += dx;
                y += dy;
                z += dz;
            }
            return intens;


        }
        public void TreadFucn()
        {

            for (int i = 0; i < rgbValues.Length / 4 - rgbValues.Length / 1600; i += 4)
            {
                counter += 4;
                int currentnum = i;

                if (exist[currentnum] == 1)
                {
                    int diff = 1;
                    while (true)
                    {
                        if (exist[Clamp(currentnum + diff, 0, (rgbValues.Length / 4) - 1)] == 0)
                        {
                            currentnum = Clamp(currentnum + diff, 0, (rgbValues.Length / 4) - 1);
                            break;
                        }
                        else if (exist[Clamp(currentnum - diff, 0, (rgbValues.Length / 4) - 1)] == 0)
                        {
                            currentnum = Clamp(currentnum - diff, 0, (rgbValues.Length / 4) - 1);
                            break;
                        }
                        else diff++;
                    }
                }
                exist[currentnum] = 1;
                byte[] tempARGB = RayPix2(points[currentnum / raySize, currentnum % raySize, 0], points[currentnum / raySize, currentnum % raySize, 1], points[currentnum / raySize, currentnum % raySize, 2], direction);
                //byte[] tempARGB = { 0, 0, 0, 0 };
                rgbValues[currentnum * 4] = tempARGB[0];
                rgbValues[currentnum * 4 + 1] = tempARGB[1];
                rgbValues[currentnum * 4 + 2] = tempARGB[2];
                rgbValues[currentnum * 4 + 3] = tempARGB[3];
            }

        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {

        }

        private void RenderQuality_ValueChanged(object sender, EventArgs e)
        {

        }
    
    }
}
