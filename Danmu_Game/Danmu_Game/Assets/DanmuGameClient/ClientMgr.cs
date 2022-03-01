using Connection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using DanmuGameProtocol;

public class ClientMgr : MonoBehaviour
{
    public Button Button_Ping;
    private GaemClient client;
    private Socket socket;
    // Start is called before the first frame update
    void Start()
    {
        Button_Ping.onClick.AddListener(StartClient);
    }
    private void StartClient()
    {
        if (socket == null)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.NoDelay = true;
            client = new GaemClient(socket);
            client.SrartConnect();
        }
    }
    private void Update()
    {
        if (client == null) return;
        if (client.message.msgList.Count <= 0) return;
        
        MainPack pack = client.message.msgList[0];
        client.message.msgList.RemoveAt(0);

        Debug.Log((MessageType)pack.MessageType + ":" + pack.UserName + ":" + pack.UserText);
    }

    private void OnDestroy()
    {
        socket.Close();
    }
}
