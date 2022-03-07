using System;
using System.IO;
using System.IO.Compression;

using System.Text;

using System.Net;
using System.Net.Http;
using System.Net.Sockets;

using System.Threading.Tasks;
using System.Collections.Generic;

using BilibiliUtilities.Live.Lib;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;


public class LiveRoomConnect : IDisposable
{
    public string RoomID;
    private string roodIDReal;
    private int heartbeat_interval = 30;
    private string host_server_token;
    private TcpClient tcpClient = new TcpClient();
    private HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    private Stream roomStream;

    //��Ϣ�汾��,���ڹ̶�Ϊ2
    private const short ProtocolVersion = 2;
    //��Ϣͷ�ĳ���,���ڰ汾�̶�Ϊ16
    //DanmuHead�ķ���BufferToDanmuHead���и�д����16,����������޸�Ҫһ���޸�
    private const int ProtocolHeadLength = 16;
    private IMessageHandler messageHandler;
    private IMessageDispatcher messageDispatcher;

    private bool connected = false;
    Dictionary<string, object> parameters = new Dictionary<string, object>();

    private bool roomStreamDisposed = false;
    /// <summary>
    /// ͨ������ID��ʼ������������
    /// </summary>
    /// <param name="roomID"></param>
    public LiveRoomConnect(string roomID)
    {
        RoomID = roomID;
        this.tcpClient = new TcpClient();
        this.httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        this.messageHandler = new LiveHandler();
        this.messageDispatcher = new MessageDispatcher();
    }
    public void Dispose()
    {
        if (connected)
            Disconnect();
        tcpClient?.Dispose();
        roomStream?.Close();
        roomStream?.Dispose();
        roomStreamDisposed = true;
        httpClient?.Dispose();
    }

    /// <summary>
    /// ͨ������ID������ʵ��Ϣ
    /// </summary>
    /// <param name="roomID"></param>
    public void GetRoomInfo()
    {
        var Info = BaseTools.GetRoomInfo(RoomID, null);
        roodIDReal = Info.ID;                               //�������ʵID
        heartbeat_interval = Info.HI;                       //������������
    }

    /// <summary>
    /// ���ӷ��䵯Ļ������������������ѭ��
    /// </summary>
    /// <returns></returns>
    public async Task Conncet()
    {
        try
        {
            bool b1 = await ConnectRoom(tcpClient, roodIDReal);
            roomStream = tcpClient.GetStream();
            await SendJoinMsgAsync(host_server_token);
            var headBuffer = new byte[ProtocolHeadLength];
            await roomStream.ReadAsync(headBuffer, 0, headBuffer.Length);
            DanmuHead danmuHead = DanmuHead.BufferToDanmuHead(headBuffer);
            if (danmuHead.HeaderLength != ProtocolHeadLength || danmuHead.Action != 8)
            {
                //���ͷ��Ϣ�ĳ��Ȳ���16,����Action�ĵ�ֵ����8 (������������֤�����Ӧ�ĵ�һ������)
                //���Ǵ�����Ĵ���
                throw new Exception("ͷ��Ϣ���ȴ����Actionֵ��Ϊ8");
            }
            var dataBuffer = new byte[danmuHead.PacketLength - danmuHead.HeaderLength];
            await roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
            var data = JObject.Parse(Encoding.Default.GetString(dataBuffer));
            if (int.Parse(data["code"].ToString()) != 0)
            {
                throw new Exception("codeֵ��Ϊ8");
            }

            connected = true;
            //ѭ������������Ϣ
            SendHeartbeatLoop();
        }
        catch (Exception ex)
        {
            Debug.LogError("����ʧ�ܣ�" + ex);
            throw;
        }
    }

    /// <summary>
    /// ���ӵ�����ĵ�Ļ������
    /// </summary>
    /// <param name="tcpClient"></param>
    /// <param name="roodIDReal"></param>
    /// <returns></returns>
    private async Task<bool> ConnectRoom(TcpClient tcpClient, string roodIDReal)
    {
        if (roodIDReal.Equals(""))
        {
            return false;
        }
        var tmpData = JObject.Parse(await httpClient.GetStringAsync($"https://api.live.bilibili.com/room/v1/Danmu/getConf?room_id={roodIDReal}&platform=pc&player=web"));
        //��������,��ȡIP��ַ,��������
        var chatHost = tmpData["data"]["host"].ToString();
        var ips = await Dns.GetHostAddressesAsync(chatHost);
        //���ӵĶ˿�
        var chatPort = int.Parse(tmpData["data"]["port"].ToString());
        System.Random random = new System.Random();
        //���һ��ѡ����������������IP,���ؾ���
        await tcpClient.ConnectAsync(ips[random.Next(ips.Length)], chatPort);
        if (!tcpClient.Connected)
        {
            return false;
        }
        if (!tcpClient.GetStream().CanWrite)
        {
            return false;
        }
        return true;
    }
    /// <summary>
    /// ���ͽ���ֱ�������Ϣ
    /// </summary>
    /// <param name="token">����Ϊ�ղ�Ӱ��</param>
    /// <returns></returns>
    private async Task<bool> SendJoinMsgAsync(string token)
    {
        var packageModel = new Dictionary<string, object>
            {
                {"roomid", Convert.ToInt32(roodIDReal)},
                {"uid", 0},
                {"protover", ProtocolVersion},
                {"token", token},
                {"platform", "web"},
                {"type", 2}
            };
        var body = JsonConvert.SerializeObject(packageModel);
        await SendSocketDataAsync(7, body);
        return true;
    }
    /// <summary>
    /// ѭ����������,��ֹ�ظ�����
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private async Task SendHeartbeatLoop()
    {
        try
        {
            connected = tcpClient.Connected;

            while (connected)
            {
                try
                {
                    await SendSocketDataAsync(ProtocolHeadLength, ProtocolVersion, 2, 1, "");
                    //����30��
                    await Task.Delay(heartbeat_interval);
                }
                catch (Exception e)
                {
                    connected = false;
                    Console.WriteLine("��������ʧ��");
                    throw e;
                }
            }
        }
        catch (Exception e)
        {
            Disconnect();
            throw e;
        }
    }
    /// <summary>
    /// ������Ϣ�ķ���
    /// </summary>
    /// <param name="action"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    private Task SendSocketDataAsync(int action, string body)
    {
        return SendSocketDataAsync(ProtocolHeadLength, ProtocolVersion, action, 1, body);
    }
    /// <summary>
    /// ������Ϣ�ķ���
    /// </summary>
    /// <param name="headLength"></param>
    /// <param name="version"></param>
    /// <param name="action"></param>
    /// <param name="param"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    private async Task SendSocketDataAsync(short headLength, short version, int action, int param, string body)
    {
        var data = Encoding.UTF8.GetBytes(body);
        var packageLength = data.Length + headLength;

        var buffer = new byte[packageLength];
        var ms = new MemoryStream(buffer);
        await ms.WriteAsync(EndianBitConverter.EndianBitConverter.BigEndian.GetBytes(buffer.Length), 0, 4);
        await ms.WriteAsync(EndianBitConverter.EndianBitConverter.BigEndian.GetBytes(headLength), 0, 2);
        await ms.WriteAsync(EndianBitConverter.EndianBitConverter.BigEndian.GetBytes(version), 0, 2);
        await ms.WriteAsync(EndianBitConverter.EndianBitConverter.BigEndian.GetBytes(action), 0, 4);
        await ms.WriteAsync(EndianBitConverter.EndianBitConverter.BigEndian.GetBytes(param), 0, 4);
        if (data.Length > 0)
        {
            await ms.WriteAsync(data, 0, data.Length);
        }
        await roomStream.WriteAsync(buffer, 0, buffer.Length);
    }
    /// <summary>
    /// �ر����ӵķ���
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Disconnect()
    {
        try
        {
            connected = false;
            tcpClient.Dispose();
            roomStream = null;
        }
        catch (Exception e)
        {
            //������
            throw e;
        }
    }


    /// <summary>
    /// ѭ����ȡ��Ϣ,��ֹ�ظ�����
    /// </summary>
    /// <returns></returns>
    public async Task ReadMessageLoop()
    {
        JObject json = null;
        while (connected)
        {
            
            var headBuffer = new byte[ProtocolHeadLength];

            //�ȶ�ȡһ��ͷ��Ϣ

            await roomStream.ReadAsync(headBuffer, 0, ProtocolHeadLength);

               
            

            //����ͷ��Ϣ
            DanmuHead danmuHead = DanmuHead.BufferToDanmuHead(headBuffer);
            //�ж�Э��
            if (danmuHead.HeaderLength != ProtocolHeadLength)
            {
                continue;
            }

            //��ʼ��һ�������ݵ�byte����
            byte[] dataBuffer;
            if (danmuHead.Action == 3)
            {
                //������������������Ϣ��Ļ�Ӧ��Ϣ,������������ֱ����Ĺۿ�����(����ֵ)
                dataBuffer = new byte[danmuHead.MessageLength()];
                await roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
                var audiences = EndianBitConverter.EndianBitConverter.BigEndian.ToInt32(dataBuffer, 0);
                messageHandler.AudiencesHandlerAsync(audiences);
                continue;
            }

            string tmpData;
            if (danmuHead.Action == 5 && danmuHead.Version == ProtocolVersion)
            {
                //��Ч����Ϊ�����Ļ���������������
                //��ȡ���ݷ��뻺����
                dataBuffer = new byte[danmuHead.MessageLength()];
                await roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
                //֮������ݷ��뵽�ڴ���
                string jsonStr;
                using (var ms = new MemoryStream(dataBuffer, 2, danmuHead.MessageLength() - 2))
                {
                    //ʹ���ڴ������ɽ�ѹ��(ѹ����) 
                    var deflate = new DeflateStream(ms, CompressionMode.Decompress);
                    var headerbuffer = new byte[ProtocolHeadLength];
                    try
                    {
                        while (true)
                        {
                            await deflate.ReadAsync(headerbuffer, 0, ProtocolHeadLength);
                            danmuHead = DanmuHead.BufferToDanmuHead(headerbuffer);
                            var messageBuffer = new byte[danmuHead.MessageLength()];
                            var readLength = await deflate.ReadAsync(messageBuffer, 0, danmuHead.MessageLength());
                            jsonStr = Encoding.UTF8.GetString(messageBuffer, 0, danmuHead.MessageLength());
                            if (readLength == 0)
                            {
                                break;
                            }
                            json = JObject.Parse(jsonStr);
                            messageDispatcher.DispatchAsync(json, messageHandler);
                        }
                        continue;
                    }
                    catch (Exception e)
                    {
                        //�����ݳ�������
                        Debug.LogError(e);
                        throw;
                    }
                }
            }

            dataBuffer = new byte[danmuHead.MessageLength()];
            await roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
            tmpData = Encoding.UTF8.GetString(dataBuffer);
            try
            {
                json = JObject.Parse(tmpData);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw e;
            }
            if (!"DANMU_MSG".Equals(json["cmd"].ToString()) && !"SEND_GIFT".Equals(json["cmd"].ToString()))
            {
                messageDispatcher.DispatchAsync(json, messageHandler);
            }
        }
    }
}
