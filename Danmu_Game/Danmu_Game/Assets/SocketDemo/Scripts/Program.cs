using System.Net.Sockets;
using System.Text;
using System.Net;
using System;

namespace Connection
{
    public class Program
    {
        private static Socket socket;
        private static byte[] buffer = new byte[1024];

        static void main(string[] args)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Any, 9787));
            socket.Listen(128);//参数为连接队列的最大长度
            StartAccept();
        }
        private static void StartAccept()
        {
            socket.BeginAccept(AcceptCallBack, null);
        }
        private static void AcceptCallBack(IAsyncResult iar)
        {
            Socket client = socket.EndAccept(iar);
            StartReceive(client);
            StartAccept();
        }

        private static void StartReceive(Socket client)
        {
            client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveCallBack, client);
        }
        private static void ReceiveCallBack(IAsyncResult iar)
        {
            Socket client = iar.AsyncState as Socket;
            int length = socket.EndReceive(iar);
            if (length == 0)
                return;
            string str = Encoding.UTF8.GetString(buffer, 0, length); // 解析
            StartReceive(client);
        }
    }
}