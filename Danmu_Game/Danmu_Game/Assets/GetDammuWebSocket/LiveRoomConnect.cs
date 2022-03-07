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

    //消息版本号,现在固定为2
    private const short ProtocolVersion = 2;
    //消息头的长度,现在版本固定为16
    //DanmuHead的方法BufferToDanmuHead中有个写死的16,如果后续有修改要一起修改
    private const int ProtocolHeadLength = 16;
    private IMessageHandler messageHandler;
    private IMessageDispatcher messageDispatcher;

    private bool connected = false;
    Dictionary<string, object> parameters = new Dictionary<string, object>();

    private bool roomStreamDisposed = false;
    /// <summary>
    /// 通过房间ID初始化房间连接器
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
    /// 通过房间ID房间真实信息
    /// </summary>
    /// <param name="roomID"></param>
    public void GetRoomInfo()
    {
        var Info = BaseTools.GetRoomInfo(RoomID, null);
        roodIDReal = Info.ID;                               //房间的真实ID
        heartbeat_interval = Info.HI;                       //房间的心跳间隔
    }

    /// <summary>
    /// 连接房间弹幕服务器，并开启心跳循环
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
                //如果头信息的长度不是16,或者Action的的值不是8 (服务器接受认证包后回应的第一个数据)
                //这是错误处理的代码
                throw new Exception("头信息长度错误或Action值不为8");
            }
            var dataBuffer = new byte[danmuHead.PacketLength - danmuHead.HeaderLength];
            await roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
            var data = JObject.Parse(Encoding.Default.GetString(dataBuffer));
            if (int.Parse(data["code"].ToString()) != 0)
            {
                throw new Exception("code值不为8");
            }

            connected = true;
            //循环发送心跳信息
            SendHeartbeatLoop();
        }
        catch (Exception ex)
        {
            Debug.LogError("连接失败：" + ex);
            throw;
        }
    }

    /// <summary>
    /// 连接到房间的弹幕服务器
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
        //解析域名,拿取IP地址,用于连接
        var chatHost = tmpData["data"]["host"].ToString();
        var ips = await Dns.GetHostAddressesAsync(chatHost);
        //连接的端口
        var chatPort = int.Parse(tmpData["data"]["port"].ToString());
        System.Random random = new System.Random();
        //随机一个选择域名解析出来的IP,负载均衡
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
    /// 发送进入直播间的消息
    /// </summary>
    /// <param name="token">可以为空不影响</param>
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
    /// 循环发送心跳,禁止重复调用
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
                    //休眠30秒
                    await Task.Delay(heartbeat_interval);
                }
                catch (Exception e)
                {
                    connected = false;
                    Console.WriteLine("发送心跳失败");
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
    /// 发送消息的方法
    /// </summary>
    /// <param name="action"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    private Task SendSocketDataAsync(int action, string body)
    {
        return SendSocketDataAsync(ProtocolHeadLength, ProtocolVersion, action, 1, body);
    }
    /// <summary>
    /// 发送消息的方法
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
    /// 关闭连接的方法
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
            //错误处理
            throw e;
        }
    }


    /// <summary>
    /// 循环读取消息,禁止重复调用
    /// </summary>
    /// <returns></returns>
    public async Task ReadMessageLoop()
    {
        JObject json = null;
        while (connected)
        {
            
            var headBuffer = new byte[ProtocolHeadLength];

            //先读取一次头信息

            await roomStream.ReadAsync(headBuffer, 0, ProtocolHeadLength);

               
            

            //解析头信息
            DanmuHead danmuHead = DanmuHead.BufferToDanmuHead(headBuffer);
            //判断协议
            if (danmuHead.HeaderLength != ProtocolHeadLength)
            {
                continue;
            }

            //初始化一个放数据的byte数组
            byte[] dataBuffer;
            if (danmuHead.Action == 3)
            {
                //给服务器发送心跳信息后的回应信息,所带的数据是直播间的观看人数(人气值)
                dataBuffer = new byte[danmuHead.MessageLength()];
                await roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
                var audiences = EndianBitConverter.EndianBitConverter.BigEndian.ToInt32(dataBuffer, 0);
                messageHandler.AudiencesHandlerAsync(audiences);
                continue;
            }

            string tmpData;
            if (danmuHead.Action == 5 && danmuHead.Version == ProtocolVersion)
            {
                //有效负载为礼物、弹幕、公告等内容数据
                //读取数据放入缓冲区
                dataBuffer = new byte[danmuHead.MessageLength()];
                await roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
                //之后把数据放入到内存流
                string jsonStr;
                using (var ms = new MemoryStream(dataBuffer, 2, danmuHead.MessageLength() - 2))
                {
                    //使用内存流生成解压流(压缩流) 
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
                        //读数据超出长度
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
