using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace drawpaint
{

    public enum Item
    {
        Pen,
        Line,
        Rectangle,
        Elipse,
        Eraser
    }

    public class Drawing
    {
        public delegate void MouseDelegate(object sender, MouseEventArgs e);
        public Point p1, p2 , endP;//定义两个点（启点，终点）  
        public  bool drawing = false;//设置一个启动标志
        public Form1 form;
        public Color color = new Color();
        public Item curItem = Item.Pen;
        public int borderSize = 1;
        private Pen pen;
        public Graphics g;
        System.Drawing.Bitmap image = null;
        public Drawing(Form1 form)
        {
            this.form = form;
            color = Color.Black;
            pen = new Pen(Color.Black, 1);
            this.form.drawpic.Controls.Clear();
            //g = form.drawpic.CreateGraphics();
            image = new Bitmap(this.form.drawpic.Width, this.form.drawpic.Height);
            Graphics.FromImage(image).Clear(this.form.drawpic.BackColor);
            this.form.drawpic.Image = (Bitmap)image.Clone();
            //g = form.drawpic.CreateGraphics();
             
        }

        public void ClearDraw()
        {
            //Graphics g = this.form.drawpic.CreateGraphics();
            //g.Clear(SystemColors.Control);
            g = Graphics.FromImage(this.form.drawpic.Image);
            g.Clear(SystemColors.Control);
            this.form.drawpic.Refresh();
        
        }

        /// <summary>
        /// 保存绘制的图像
        /// </summary>
        /// <param name="fileName">文件名字</param>
        
        public void SaveImge()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = "E:\\";
            sfd.Filter = "png文件(*.png)|*.png";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                
                //this.form.drawpic.Image = (Image)image.Clone();
                 this.form.drawpic.Image.Save(sfd.FileName,ImageFormat.Png);
            }
        }

        public void MouseHandler(object sender, MouseEventArgs e,MouseDelegate mouse)
        {
            mouse(sender, e);
        }
        public void MouseDownHandler(object sender, MouseEventArgs e)
        {
            p1 = new Point(e.X, e.Y);
            p2 = new Point(e.X, e.Y);
            g = Graphics.FromImage(this.form.drawpic.Image);
            drawing = true;    
        }

        public void MouseUpHandler(object sender, MouseEventArgs e)
        {
            drawing = false;
            g.Dispose();
        
        }

        public void MouseMoveHandler(object sender, MouseEventArgs e)
        {

           
            if (e.Button == MouseButtons.Left)
            {
                if (drawing)
                {
                    Point currentPoint = new Point(e.X, e.Y);
                    DrawStart(currentPoint);
                    this.form.drawpic.Refresh();
                }

            }  
        }

        public void DrawStart(Point curPoint)
        {
            Point currentPoint = curPoint;
            switch (curItem)
            {

                case Item.Pen:
                    //drawing = true;  
                    //Point currentPoint = new Point(e.X, e.Y);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//消除锯齿
                    g.SmoothingMode = SmoothingMode.HighQuality;

                    pen.Width = borderSize;
                    pen.Color = color;
                    pen.StartCap = pen.EndCap = LineCap.Round;
                    pen.Alignment = PenAlignment.Center;

                    g.DrawLine(pen, p2, currentPoint);
                    p2.X = currentPoint.X;
                    p2.Y = currentPoint.Y;
                    break;
                case Item.Eraser:
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;//消除锯齿
                    g.SmoothingMode = SmoothingMode.HighQuality;

                    pen.Width = borderSize;
                    //pen.Color = SystemColors.Control;
                    pen.Color = this.form.drawpic.BackColor;
                    pen.StartCap = pen.EndCap = LineCap.Round;
                    pen.Alignment = PenAlignment.Center;

                    g.DrawLine(pen, p2, currentPoint);
                    p2.X = currentPoint.X;
                    p2.Y = currentPoint.Y;

                    break;

            }
            
        }
        
   }    
}
