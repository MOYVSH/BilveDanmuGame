using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace Connection
{
    public class Client : MonoBehaviour
    {
        private Socket socket;
        private byte[] buffer = new byte[1024];
        private void Awake()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("localhost", 9787);//连接到服务端
            StartReceive();
            send();
        }

        private void StartReceive()
        {
            socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallBack, null);
        }
        private void ReceiveCallBack(IAsyncResult iar)
        {
            int length = socket.EndReceive(iar);
            if (length == 0)
                return;
            string str = Encoding.UTF8.GetString(buffer, 0, length); // 解析
            Debug.Log(str);
            StartReceive();
        }

        private void send()
        {
            socket.Send(Encoding.UTF8.GetBytes("连接成功！"));
        }
    }
}