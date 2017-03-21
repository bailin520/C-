
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace drawpaint
{
    public partial class Form1 : Form
    {

        delegate void ReflashDraw();
        delegate void SetWebSocketLblState(string state);
        public void Reflash()
        {
            if (this.drawpic.InvokeRequired)
            {
                ReflashDraw rd = new ReflashDraw(Reflash);
                this.Invoke(rd, new object[] { });
            }
            else
            {
                this.drawpic.Refresh();
            }
        }

        delegate void WindowCloseDelegate();
        public void WindowClose()
        {
            if (this.drawpic.InvokeRequired)
            {
                WindowCloseDelegate wcd = new WindowCloseDelegate(WindowClose);
                this.Invoke(wcd, new object[] { });
            }
            else
            {
                this.Close(); 
            }
           
        }
        private void SetLblState(string state)
        {
            this.LblState.Text = state;
        }
        

        delegate void ClearDrawDelegate();
        public void ClearDrawed()
        {
            if (this.drawpic.InvokeRequired)
            {
                ClearDrawDelegate wcd = new ClearDrawDelegate(ClearDrawed);
                this.Invoke(wcd, new object[] { });
            }
            else
            {
                this.drawing.ClearDraw();
            }

        }

        public Form1()
        {
            InitializeComponent();
            //Console.Write(SystemColors.Control.R);
            //Console.Write(SystemColors.Control.G);
            //Console.Write(SystemColors.Control.B);
        }

       

        private void lblclose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lblclose_MouseHover(object sender, EventArgs e)
        {
            lblclose.BackColor = Color.Red;
        }

        private void lblclose_MouseLeave(object sender, EventArgs e)
        {
            lblclose.BackColor = SystemColors.AppWorkspace;
        }



        Drawing drawing ;
        private void drawpic_MouseDown(object sender, MouseEventArgs e)
        {
            if (drawing != null)
            //回调鼠标按下消息
            drawing.MouseHandler(sender, e,new drawpaint.Drawing.MouseDelegate(drawing.MouseDownHandler));
        }

        private void drawpic_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawing!=null)
            //回调鼠标移动消息
            drawing.MouseHandler(sender, e, new drawpaint.Drawing.MouseDelegate(drawing.MouseMoveHandler));
        }

        private void drawpic_MouseUp(object sender, MouseEventArgs e)
        {
            if (drawing != null)
            //回调鼠标弹起消息
            drawing.MouseHandler(sender, e, new drawpaint.Drawing.MouseDelegate(drawing.MouseUpHandler));
        }

        private void drawpic_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            drawing = new Drawing(this);
            comboBox1.SelectedIndex = 0;
            MessageHandler mh = new MessageHandler(drawing);
            mh.WebsocketStateEvent+=mh_WebsocketStateEvent;   


        }

        private void mh_WebsocketStateEvent(string state)
        {
            SetWebSocketLblState sss = new SetWebSocketLblState(SetLblState);
            this.Invoke(sss, state);
            this.LblState.Text = state;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            drawing.ClearDraw();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            drawing.curItem = Item.Pen;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            drawing.curItem = Item.Line;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            drawing.curItem = Item.Rectangle;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            drawing.curItem = Item.Eraser;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            drawing.borderSize = comboBox1.SelectedIndex;
            if(comboBox1.SelectedIndex ==10)
            {
                drawing.borderSize = 20;
            }
        }

        private void lbPaintColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    lbPaintColor.BackColor = colorDialog.Color;
                    drawing.color = colorDialog.Color;
                    //Console.Write(drawing.color.R);
                    //Console.Write(drawing.color.G);
                    //Console.Write(drawing.color.B);
                }
                    
            }
        }

       
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Cyan, 0, 0, this.Width - 1, this.Height - 1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            drawing.SaveImge();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
    }
}
