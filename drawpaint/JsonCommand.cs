using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drawpaint
{
    class JsonCommand
    {
        public string Dst;
        public Message Msg;
    }
    public class Message
    {

        public string Type;
        public string P1;
        public string P2;
        public string P3;

    }
}
