using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region 获取的JSON对应的类
public class Host_server_list
{
    public string host { get; set; }
    public string port { get; set; }
    public string wss_port { get; set; }
    public string ws_port { get; set; }
}

public class Server_list
{
    public string host { get; set; }
    public string port { get; set; }
}

public class Data
{
    public string refresh_row_factor { get; set; }
    public string refresh_rate { get; set; }
    public string max_delay { get; set; }
    public string port { get; set; }
    public string host { get; set; }
    public List<Host_server_list> host_server_list { get; set; }
    public List<Server_list> server_list { get; set; }
    public string token { get; set; }
}

public class DanmuGetConf
{
    public string code { get; set; }
    public string msg { get; set; }
    public string message { get; set; }
    public Data data { get; set; }
}
#endregion

#region 发送认证包的JSON对应的类
public class AuthParam
{
    public string uid { get; set; }
    public string roomid { get; set; }
    public string protover { get; set; }
    public string platform { get; set; }
    public string type { get; set; }
    public string key { get; set; }
}
#endregion