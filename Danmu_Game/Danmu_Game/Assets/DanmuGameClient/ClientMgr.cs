using Connection;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ClientMgr : MonoBehaviour
{
    public Button Button_Ping;
    private GaemClient client;
    private Socket socket;
    // Start is called before the first frame update
    void Start()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("localhost", 9787);//���ӵ������
        client = new GaemClient(socket);
        socket.Send(Encoding.UTF8.GetBytes("���ӳɹ���"));
        client.startReceive();
        Button_Ping.onClick.AddListener(onclick);
    }
    void onclick()
    {
        socket.Send(Encoding.UTF8.GetBytes("���ӳɹ���"));
    }
    private void OnDestroy()
    {
        socket.Close();
    }
}
