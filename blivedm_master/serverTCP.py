from socket import *

class serverTCP:
    # 创建套接字
    tcp_socket = None
    # 绑定本地信息
    loc_port = 9787
    #创建客户端socket列表
    Clients=[]

    def send_msg(self, msg: "str"):
        for clientSocket,clientAddr in self.Clients:
            clientSocket.send(msg)

    def BeginReceive(self):
        while True:
            try:
                newClient = self.tcp_socket.accept()
            except Exception as result:#如果没有客户端连接则产生一个异常  
                pass
            else:#如果有客户端连接，则将新的客户端设置为非阻塞，并添加到客户端列表中
                newClient[0].setblocking(0)
                self.Clients.append(newClient)
            Clients_invalid=[]#创建无效的客户端列表
            for clientSocket,clientAddr in self.Clients:
                try:
                    # print("尝试连接客户端")
                    msg = '123'
                    clientSocket.send(msg.encode('utf-8'))#通过发送数据判断客户端是否在线
                except:#客户端不在线
                    # print("客户端不在线")
                    clientSocket.close()
                    Clients_invalid.append((clientSocket,clientAddr))#将客户端计入无效列表
                else:
                    # print("客户端连接成功")
                    try:
                        recvData = clientSocket.recv(1024)
                        if len(recvData) > 0:
                            print(recvData)
                        else:
                            pass
                    except:#接收异常则忽略
                        pass
            for client in Clients_invalid:
                self.Clients.remove(client)
            if len(self.Clients) > 0 :
                break


    def __init__(self):
        self.tcp_socket = socket(AF_INET, SOCK_STREAM)# 创建套接字
        self.tcp_socket.bind(('localhost',self.loc_port))# 绑定端口
        self.tcp_socket.listen(128)# 默认的套接字由主动变为监听
        self.tcp_socket.setblocking(0)