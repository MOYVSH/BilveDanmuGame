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
        public Message message;
        private Socket socket;

        public GaemClient(Socket socket)
        {
            this.socket = socket;
            this.message = new Message();
        }
        #region Connect
        /// <summary>
        /// 连接到服务端
        /// </summary>
        public void SrartConnect()
        {
            socket.BeginConnect("127.0.0.1", 9787, ConnectCallBack, socket);
        }
        public void ConnectCallBack(IAsyncResult iar)
        {
            try
            {
                Socket socket = (Socket)iar.AsyncState;
                socket.EndConnect(iar);
                StartReceive();
            }
            catch (SocketException ex)
            {
                Debug.LogError("Connect fail:" + ex.ToString());
            }
        }
        #endregion

        #region Receive
        /// <summary>
        /// 开始接收消息
        /// </summary>
        public void StartReceive()
        {
            socket.BeginReceive(message.Buffer, message.StartIndex, message.Resize, SocketFlags.None, ReceiveCallBack, socket);
        }
        public void ReceiveCallBack(IAsyncResult iar)
        {
            try
            {
                Socket socket = (Socket)iar.AsyncState;
                int Length = socket.EndReceive(iar);
                message.ReadBuffer(Length);
                StartReceive();
            }
            catch (SocketException ex)
            {
                Debug.LogError("Connect fail:" + ex.ToString());
            }
        }
        #endregion

        #region Send
        public void Send(string str)
        {
            byte[] sendByte = Encoding.UTF8.GetBytes(str);
            socket.BeginSend(sendByte, 0, sendByte.Length, 0, SendCallBack, socket);
        }
        private void SendCallBack(IAsyncResult iar)
        {
            try
            {
                Socket socket = (Socket)iar.AsyncState;
                int count = socket.EndSend(iar);
                Debug.LogError("Send Successful:" + count);
            }
            catch (SocketException ex)
            {
                Debug.LogError("Send fail:"+ex.ToString());
            }
        }
        #endregion
    }
}