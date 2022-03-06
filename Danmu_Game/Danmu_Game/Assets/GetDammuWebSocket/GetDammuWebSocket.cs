using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Http;
using UnityEngine.UI;
using BilibiliUtilities.Live.Lib;
using System.IO.Compression;
using BilibiliUtilities.Test.LiveLib;

public  class GetDammuWebSocket : MonoBehaviour
{
    public Button ButtonConnect;
    public string RoomID = "419850";
    [SerializeField] private string roodIDReal;
    private int heartbeat_interval = 30;
    private string host_server_token;
    private TcpClient tcpClient = new TcpClient();
    private HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
    private Stream _roomStream;

    //消息版本号,现在固定为2
    private const short ProtocolVersion = 2;
    //消息头的长度,现在版本固定为16
    //DanmuHead的方法BufferToDanmuHead中有个写死的16,如果后续有修改要一起修改
    private const int ProtocolHeadLength = 16;
    private IMessageHandler _messageHandler;
    private IMessageDispatcher _messageDispatcher;

    private bool connected = false;
    Dictionary<string, object> parameters = new Dictionary<string, object>();
    private void Start()
    {
        ButtonConnect.onClick.AddListener(()=> {
            onclick();
        });
    }
    private void OnDestroy()
    {
        Disconnect();
    }

    public async void onclick()
    {
        if (connected)
            return;
        _messageHandler = new LiveHandler();
        _messageDispatcher = new MessageDispatcher();
        GetRppmInfo();
        await conncet();
        await ReadMessageLoop();
    }
    public async Task conncet()
    {
        try
        {
            await ConnectRoom(tcpClient, roodIDReal);
            _roomStream = tcpClient.GetStream();
            await SendJoinMsgAsync(host_server_token);
            var headBuffer = new byte[ProtocolHeadLength];
            await _roomStream.ReadAsync(headBuffer, 0, headBuffer.Length);
            DanmuHead danmuHead = DanmuHead.BufferToDanmuHead(headBuffer);
            if (danmuHead.HeaderLength != ProtocolHeadLength || danmuHead.Action != 8)
            {
                //如果头信息的长度不是16,或者Action的的值不是8 (服务器接受认证包后回应的第一个数据)
                //这是错误处理的代码
                throw new Exception("头信息长度错误或Action值不为8");
            }
            var dataBuffer = new byte[danmuHead.PacketLength - danmuHead.HeaderLength];
            await _roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
            var s = Encoding.Default.GetString(dataBuffer);
            var data = JObject.Parse(s);
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
    /// 获取直播间信息
    /// </summary>
    public void GetRppmInfo()
    {
        string urlRoomInfo = $"https://api.live.bilibili.com/room/v1/Room/room_init?id={RoomID}";
        string jsonRoomInfo = (string)GetHttpWebResponseReturnString(urlRoomInfo, parameters);
        JObject objRoomInfo = JObject.Parse(jsonRoomInfo);
        roodIDReal = objRoomInfo["data"]["room_id"].ToString();

        string url = $"https://api.live.bilibili.com/room/v1/Danmu/getConf?room_id={roodIDReal}&platform=pc&player=web";
        string json = (string)GetHttpWebResponseReturnString(url, parameters);
        DanmuGetConf conf = JsonConvert.DeserializeObject<DanmuGetConf>(json);
        heartbeat_interval = Convert.ToInt32(conf.data.max_delay);
    }
    public async Task<bool> ConnectRoom(TcpClient tcpClient,string roomId)
    {
        if (roomId.Equals(""))
        {
            return false;
        }
        var tmpData = JObject.Parse(await _httpClient.GetStringAsync($"https://api.live.bilibili.com/room/v1/Danmu/getConf?room_id={roomId}&platform=pc&player=web"));
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
    public async Task<bool> SendJoinMsgAsync(string token)
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
    /// 发送消息的方法
    /// </summary>
    /// <param name="action"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public Task SendSocketDataAsync(int action, string body)
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
    public async Task SendSocketDataAsync(short headLength, short version, int action, int param, string body)
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
        await _roomStream.WriteAsync(buffer, 0, buffer.Length);
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
    /// 关闭连接的方法
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Disconnect()
    {
        try
        {
            connected = false;
            tcpClient.Dispose();
            _roomStream = null;
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
        while (connected)
        {
            var headBuffer = new byte[ProtocolHeadLength];
            //先读取一次头信息
            await _roomStream.ReadAsync(headBuffer, 0, ProtocolHeadLength);
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
                await _roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
                var audiences = EndianBitConverter.EndianBitConverter.BigEndian.ToInt32(dataBuffer, 0);
                _messageHandler.AudiencesHandlerAsync(audiences);
                continue;
            }

            string tmpData;
            JObject json = null;
            if (danmuHead.Action == 5 && danmuHead.Version == ProtocolVersion)
            {
                //有效负载为礼物、弹幕、公告等内容数据
                //读取数据放入缓冲区
                dataBuffer = new byte[danmuHead.MessageLength()];
                await _roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
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
                            _messageDispatcher.DispatchAsync(json, _messageHandler);
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
            await _roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
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
                _messageDispatcher.DispatchAsync(json, _messageHandler);
            }
        }
    }

    /// <summary>
    /// 请求远程Api获取响应返回字符串
    /// </summary>
    /// <param name="Url">Api地址</param>
    /// <param name="parameters">传递参数键值对</param>
    /// <param name="contentType">内容类型默认application/x-www-form-urlencoded</param>
    /// <param name="methord">请求方式默认POST</param>
    /// <param name="timeout">超时时间默认300000</param>
    /// <returns>响应字符串</returns>
    public object GetHttpWebResponseReturnString(string Url, Dictionary<string, object> parameters, string contentType = "application/x-www-form-urlencoded", string methord = "POST", int timeout = 300000)
    {
        string result = string.Empty;
        string responseText = string.Empty;
        try
        {
            if (string.IsNullOrEmpty(Url))
            {
                return "请求apiURl为空";
            }

            StringBuilder postData = new StringBuilder();
            if (parameters != null && parameters.Count > 0)
            {
                foreach (var p in parameters)
                {
                    if (postData.Length == 0)
                    {
                        postData.AppendFormat("{0}={1}", p.Key, p.Value);
                    }
                    else
                    {
                        postData.AppendFormat("&{0}={1}", p.Key, p.Value);
                    }
                }
            }

            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(Url);
            myRequest.Proxy = null;
            myRequest.Timeout = timeout;
            myRequest.ServicePoint.MaxIdleTime = 1000;
            if (!string.IsNullOrEmpty(contentType))
            {
                myRequest.ContentType = contentType;
            }
            myRequest.ServicePoint.Expect100Continue = false;
            myRequest.Method = methord;
            byte[] postByte = Encoding.UTF8.GetBytes(postData.ToString());
            myRequest.ContentLength = postData.Length;

            using (Stream writer = myRequest.GetRequestStream())
            {
                writer.Write(postByte, 0, postData.Length);
            }

            using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8))
                {
                    responseText = reader.ReadToEnd();
                }
            }
            if (!string.IsNullOrEmpty(responseText))
            {
                result = responseText;
            }
            else
            {
                result = "远程服务无响应，请稍后再试";
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            result = "请求异常，请稍后再试";
        }
        return result;
    }
}

