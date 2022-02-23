using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

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

        public void startReceive()
        {
            socket.BeginReceive(message.Buffer, message.StartIndex, message.Resize, SocketFlags.None, ReceiveCallBack, null);
        }
        private void ReceiveCallBack(IAsyncResult iar)
        {
            try
            {
                if (socket == null && socket.Connected == false)
                {
                    Debug.LogError("未创建socket或socket未连接");
                    return;
                }

                int length = socket.EndReceive(iar);
                if (length == 0)
                    return;
                
                message.ReadBuffer(length);
                startReceive();
            }
            catch (Exception)
            {
                startReceive();
                throw;
            }
        }
    }
}