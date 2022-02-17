using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Connection
{
    public class GaemClient
    {
        private Socket socket;
        private Message message;

        public GaemClient(Socket socket)
        {
            this.socket = socket;
            this.message = new Message();
        }

        private void startReceive()
        {
            socket.BeginReceive(message.Buffer, message.StartIndex, message.Resize, SocketFlags.None, ReceiveCallBack, null);
        }
        private void ReceiveCallBack(IAsyncResult iar)
        {
            int length = socket.EndReceive(iar);
            if (length == 0)
                return;
            //string str = Encoding.UTF8.GetString(buffer, 0, length); // ½âÎö
        }
    }
}