using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tracer
{
    public partial class Form1 : Form
    {
        RayTracer tracer = new RayTracer(); 
        public Form1()
        {
            InitializeComponent();
            SetClientSizeCore(RayTracer.ImageWidth, RayTracer.ImageHeight);
        }


        private unsafe void Form1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            var bmp = new Bitmap(RayTracer.ImageWidth, RayTracer.ImageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var lockData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);

            var ptr = (byte*)lockData.Scan0.ToPointer();
            tracer.Run(ptr, lockData.Stride);
            bmp.UnlockBits(lockData);

            g.DrawImage(bmp, rect);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
