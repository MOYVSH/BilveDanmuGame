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

    //��Ϣ�汾��,���ڹ̶�Ϊ2
    private const short ProtocolVersion = 2;
    //��Ϣͷ�ĳ���,���ڰ汾�̶�Ϊ16
    //DanmuHead�ķ���BufferToDanmuHead���и�д����16,����������޸�Ҫһ���޸�
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
                //���ͷ��Ϣ�ĳ��Ȳ���16,����Action�ĵ�ֵ����8 (������������֤�����Ӧ�ĵ�һ������)
                //���Ǵ�����Ĵ���
                throw new Exception("ͷ��Ϣ���ȴ����Actionֵ��Ϊ8");
            }
            var dataBuffer = new byte[danmuHead.PacketLength - danmuHead.HeaderLength];
            await _roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
            var s = Encoding.Default.GetString(dataBuffer);
            var data = JObject.Parse(s);
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
    /// ��ȡֱ������Ϣ
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
    /// ������Ϣ�ķ���
    /// </summary>
    /// <param name="action"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    public Task SendSocketDataAsync(int action, string body)
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
    /// �ر����ӵķ���
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
        while (connected)
        {
            var headBuffer = new byte[ProtocolHeadLength];
            //�ȶ�ȡһ��ͷ��Ϣ
            await _roomStream.ReadAsync(headBuffer, 0, ProtocolHeadLength);
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
                await _roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
                var audiences = EndianBitConverter.EndianBitConverter.BigEndian.ToInt32(dataBuffer, 0);
                _messageHandler.AudiencesHandlerAsync(audiences);
                continue;
            }

            string tmpData;
            JObject json = null;
            if (danmuHead.Action == 5 && danmuHead.Version == ProtocolVersion)
            {
                //��Ч����Ϊ�����Ļ���������������
                //��ȡ���ݷ��뻺����
                dataBuffer = new byte[danmuHead.MessageLength()];
                await _roomStream.ReadAsync(dataBuffer, 0, danmuHead.MessageLength());
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
                            _messageDispatcher.DispatchAsync(json, _messageHandler);
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
    /// ����Զ��Api��ȡ��Ӧ�����ַ���
    /// </summary>
    /// <param name="Url">Api��ַ</param>
    /// <param name="parameters">���ݲ�����ֵ��</param>
    /// <param name="contentType">��������Ĭ��application/x-www-form-urlencoded</param>
    /// <param name="methord">����ʽĬ��POST</param>
    /// <param name="timeout">��ʱʱ��Ĭ��300000</param>
    /// <returns>��Ӧ�ַ���</returns>
    public object GetHttpWebResponseReturnString(string Url, Dictionary<string, object> parameters, string contentType = "application/x-www-form-urlencoded", string methord = "POST", int timeout = 300000)
    {
        string result = string.Empty;
        string responseText = string.Empty;
        try
        {
            if (string.IsNullOrEmpty(Url))
            {
                return "����apiURlΪ��";
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
                result = "Զ�̷�������Ӧ�����Ժ�����";
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            result = "�����쳣�����Ժ�����";
        }
        return result;
    }
}

