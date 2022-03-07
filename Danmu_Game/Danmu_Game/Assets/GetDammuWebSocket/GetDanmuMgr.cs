using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetDanmuMgr : MonoBehaviour
{
    public Button ButtonConnect;
    public string RoomID;
    private LiveRoomConnect room;
    // Start is called before the first frame update
    void Start()
    {
        ButtonConnect.onClick.AddListener(() => OnRoomConnect());
    }
    public async void OnRoomConnect()
    {
        room = new LiveRoomConnect(RoomID);
        room.GetRoomInfo();
        await room.Conncet();
        await room.ReadMessageLoop();
    }
    private void OnDestroy()
    {
        room.Disconnect();
        room.Dispose();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
