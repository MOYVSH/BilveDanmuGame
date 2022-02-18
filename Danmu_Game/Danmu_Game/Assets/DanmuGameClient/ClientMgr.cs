using Connection;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class ClientMgr : MonoBehaviour
{
    private GaemClient client;
    private Socket socket;
    // Start is called before the first frame update
    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("localhost", 9787);//连接到服务端
        client = new GaemClient(socket);
        socket.Send(Encoding.UTF8.GetBytes("连接成功！"));
        client.startReceive();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
