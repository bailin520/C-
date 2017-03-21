using BaseServer;
using Fleck;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WebSocket4Net;

namespace drawpaint
{
    public class MessageHandler
    {
        public delegate void WebsocketState(string state);
        public event WebsocketState WebsocketStateEvent;

        public Drawing draw;
        string WebsocketIp;
        int WebsocketPort;

        private WebSocket websocketClient = null;
        private bool webSocketConnState = false;
        /// <summary>
        /// 自动查询3网段ip
        /// </summary>
        /// <param name="draw"></param>
        public MessageHandler(Drawing draw)
        {
            List<string> temp = new List<string>();
            temp=IpInfo.GetAddressIP();
            /*foreach (string index in temp)
            {
                try
                {
                    string[] ay = index.Split('.');
                    if (ay[2] == "3")
                    {
                        WebsocketIp = index;
                        goto IPLABLE;
                    }     
                }
                catch(Exception e)
                { continue;
                }
                         
            }*/

            WebsocketIp = System.Configuration.ConfigurationManager.AppSettings["WebsockectIp"];
            WebsocketPort = int.Parse(System.Configuration.ConfigurationManager.AppSettings["WebsockectPort"]);
IPLABLE:            
            this.draw = draw;
            //Task.Factory.StartNew(WebSocketSvrThread);
            StartWebsocketClientThread(WebsocketIp, WebsocketPort);
        }

        public void StartWebsocketClientThread(string websocketip, int websocketport)
        {
            WebSocketPara para;
            para.ip = websocketip;
            para.port = websocketport;

            Thread pThread = new Thread(new ParameterizedThreadStart(WebSocketClientThread));
            pThread.IsBackground = true;
            pThread.Start(para);

        }

        private void WebSocketClientThread(object obj)
        {
            WebSocketPara wspara = (WebSocketPara)obj;
            string wsstr = string.Format("ws://{0}:{1}", wspara.ip, wspara.port);


            websocketClient = new WebSocket(wsstr);
            websocketClient.Opened += new EventHandler(websocket_Opened);
            websocketClient.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(websocket_Error);
            websocketClient.Closed += new EventHandler(websocket_Closed);
            websocketClient.MessageReceived += new EventHandler<WebSocket4Net.MessageReceivedEventArgs>(websocket_MessageReceived);
            while (true)
            {
                if (!webSocketConnState)
                {
                    try
                    {
                        websocketClient.Open();
                    }
                    catch
                    {
                        continue;
                    }

                }
                Thread.Sleep(5000);
            }

        }

        private void websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            this.webSocketConnState = false;
        }

        

        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            JsonCommand webjson = (JsonCommand)JsonConvert.DeserializeObject(e.Message, typeof(JsonCommand));

            try
            {
                CommandHandler(webjson);
            }
            catch (Exception ee)
            {
                return;
            }
        }

        private void websocket_Closed(object sender, EventArgs e)
        {
            Console.WriteLine("websocket_Closed");
            WebsocketStateEvent("Disconnect");
            this.webSocketConnState = false;
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("websocket_Opened");
            WebsocketStateEvent("OK");
            this.webSocketConnState = true;
            websocketClient.Send("DrawPaint-" + "{\"messageType\":\"HandShark\"}");
        }

//-------------------------------------------------------------------------


        private void WebSocketSvrThread()
        {
            if (WebsocketIp == null && WebsocketPort == 0)
            {
                //throw new ArgumentNullException("没有初始化websocket参数");
            }
            
            var allSockets = new List<IWebSocketConnection>();
            string str = string.Format("ws://{0}:{1}", this.WebsocketIp, this.WebsocketPort);
            var server = new WebSocketServer(str);

            server.Start(socket =>
            {
              
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open!");
                    allSockets.Add(socket);
                    WebsocketStateEvent("Open");
                    
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Close!");
                    allSockets.Remove(socket);
                    WebsocketStateEvent("Close");
                };
                socket.OnMessage = message =>
                {
                    
                    Console.WriteLine(message);
                    Queue<string> commandQueue = new Queue<string>();
                    try
                    {
                        int startindex = message.IndexOf('{');
                        while (true)
                        {

                            int endindex = message.IndexOf('}', startindex) + 1;
                            string temp = "";
                            temp = message.Substring(startindex, endindex - startindex + 1);

                            commandQueue.Enqueue(temp);
                            startindex = endindex + 1;
                            if (endindex + 1 >= message.Length)
                            {
                                break;
                            }
                        }
                        while (commandQueue.Count > 0)
                        {
                            string mydata = commandQueue.Dequeue();
                            //Console.WriteLine(mydata + "-end\n");
                            JsonCommand webjson = (JsonCommand)JsonConvert.DeserializeObject(mydata, typeof(JsonCommand));

                            try {
                                CommandHandler(webjson);
                            }
                            catch(Exception e)
                            {
                                continue;
                            }
                           

                        }


                    }
                    catch (Exception ee)
                    {
                        //MessageBox.Show(ee.ToString());
                        //continue;
                    }

                };
            });
        }

        private Point PhoneScreenSize = new Point();
        private Point PosOffsst;
        void CommandHandler(JsonCommand webjson)
        {
            //颜色处理
            switch (webjson.Msg.Type)
            {
                
                case "PhoneScrennSize":     
                //适用于手机屏幕宽高均小于电脑屏幕宽高。若手机分辨率有一大于电脑屏幕分辨率则
                //不具备通用性
                    int PhoneScreenWidth = int.Parse(webjson.Msg.P1);  
                    int PhoneScreenHeight= int.Parse(webjson.Msg.P2);  
                    int  currentScreenWidth= Screen.PrimaryScreen.Bounds.Width;
                    int  currentScreenHeight= Screen.PrimaryScreen.Bounds.Height;

                    if (currentScreenWidth - PhoneScreenWidth>0 &&currentScreenHeight - PhoneScreenHeight>0)
                    {
                        PosOffsst.X = (currentScreenWidth - PhoneScreenWidth) / 2;
                        PosOffsst.Y = (currentScreenHeight - PhoneScreenHeight) / 2;
                    }
                    break;

                case "MM":
                    #region
                    int px = int.Parse(webjson.Msg.P1);
                    int py = int.Parse(webjson.Msg.P2);
                    Point point = new Point(px, py);
                    draw.DrawStart(point);
                    
                    //this.draw.form.drawpic.Refresh();
                    this.draw.form.Reflash();
                    break;
                    #endregion

                case "MS":
                    #region
                    int sx = int.Parse(webjson.Msg.P1);
                    int sy = int.Parse(webjson.Msg.P2) ;
                    Point startPos = new Point(sx, sy);
                    draw.p1 = startPos;
                    draw.p2 = startPos;
                    
                    this.draw.g = Graphics.FromImage(this.draw.form.drawpic.Image);
                    draw.drawing = true;
                          
                    break;
                    #endregion
                case "ME":
                    draw.drawing = false;
                    this.draw.g.Dispose();
                    break;
                   

                case "BC":
                    #region
                    int r, g, b;
                    
                    r =StringHexToInt.StrHexToInt(webjson.Msg.P1);
                    g = StringHexToInt.StrHexToInt(webjson.Msg.P2);
                    b = StringHexToInt.StrHexToInt(webjson.Msg.P3);
                    draw.color = Color.FromArgb(255,r,g,b);
                    Console.Write(r + "：" + g + "：" + b);
                    break;
                    #endregion

                case "BS":
                    #region
                    draw.borderSize = int.Parse(webjson.Msg.P1);
                    break;
                    #endregion

                case "BSt":
                    #region
                    switch (webjson.Msg.P1)
                    {
                        case "Ink":
                            draw.curItem = Item.Pen;
                            break;
                        
                        case "EraseByPoint":
                            draw.curItem = Item.Eraser;
                            break;
                   
                    }
                    break;
                    #endregion



                case "Command":
                    #region
                    switch (webjson.Msg.P1)
                    {
                       
                        case "New":    //新建画布
                            //draw.ClearDraw();
                            this.draw.form.ClearDrawed();
                            break;
                        case "Save":    //保存截图
                            //draw.form.pictureBox1.Image.Save
                            
                            SaveFileDialog sfd = new SaveFileDialog();
                            
                            sfd.InitialDirectory = Directory.GetCurrentDirectory();
                            sfd.Filter = "png文件(*.png)|*.png";
                            if(sfd.ShowDialog() == DialogResult.OK)
                            {
                                draw.form.pictureBox1.Image.Save(sfd.FileName, ImageFormat.Png);
                            }
                            break;
                        case "Close":     //关闭程序     
                            this.draw.form.WindowClose();
                            break;
                    }
                    break;
                    #endregion
            }
        }
    }
    struct WebSocketPara
    {

        public string ip;
        public int port;
    }
}
