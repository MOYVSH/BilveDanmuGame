using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
public static class BaseTools
{
    /// <summary>
    /// 把一个实例转换成字典并赋值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Dictionary<string, string> ConvertClassToDic<T>(T data)
    {
        Type type = typeof(T);
        PropertyInfo[] Props = type.GetProperties();
        Dictionary<string, string> Dic = new Dictionary<string, string>();
        for (int i = 0; i < Props.Length; i++)
        {
            string Name = Props[i].Name;
            Dic.Add(Name, (string)Props[i].GetValue(data));
        }
        return Dic;
    }

    public static (string ID,int HI) GetRoomInfo(string RoomID,Dictionary<string, object> parameters)
    {
        string urlRoomInfo = $"https://api.live.bilibili.com/room/v1/Room/room_init?id={RoomID}";
        string jsonRoomInfo = (string)GetHttpWebResponseReturnString(urlRoomInfo, parameters);
        JObject objRoomInfo = JObject.Parse(jsonRoomInfo);
        var roodIDReal = objRoomInfo["data"]["room_id"].ToString();

        string url = $"https://api.live.bilibili.com/room/v1/Danmu/getConf?room_id={roodIDReal}&platform=pc&player=web";
        string json = (string)GetHttpWebResponseReturnString(url, parameters);
        DanmuGetConf conf = JsonConvert.DeserializeObject<DanmuGetConf>(json);
        var heartbeat_interval = Convert.ToInt32(conf.data.max_delay);
        return (roodIDReal, heartbeat_interval);
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
    public static object GetHttpWebResponseReturnString(string Url, Dictionary<string, object> parameters, string contentType = "application/x-www-form-urlencoded", string methord = "POST", int timeout = 300000)
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



    //反射参考
    public static void InitTextData<T>(T data, Dictionary<string, Text> dictText)
    {
        if (data == null || dictText == null) return;

        Type type = typeof(T);
        var targetTypes = new List<Type>() { typeof(string), typeof(int), typeof(float) };
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);//成员
                                                                                 //type.GetProperties();//属性
                                                                                 //TODO 目前是public的成员变量，不包括属性。后续需要屈服数据类型的特性

        Text tmp;
        for (int i = 0; i < fields.Length; i++)
        {
            //只处理指定类型
            if (targetTypes.Contains(fields[i].FieldType))
            {
                string str = fields[i].GetValue(data)?.ToString();
                if (dictText.TryGetValue(fields[i].Name, out tmp))
                    tmp.text = str;
            }
        }
    }

    //Task参考
    static Task<int> CreateTask(string name)
    {
        return new Task<int>(() => TaskMethod(name));
    }
    static int TaskMethod(string name)
    {
        Console.WriteLine("Task {0} 运行在线程id为{1}的线程上。是否是线程池中线程？:{2}",
        name, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);
        Thread.Sleep(2000);
        return 42;
    }
}
