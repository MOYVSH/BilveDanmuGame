# 1.导入库
from socket import *

# 单工 半双工 全双工
class serverUDP:
    udp_socket = None
    # 写死的本机的ip和端口
    dest_ip = '127.0.0.1'
    dest_port = 9712
    loc_port = 9787

    def send_msg(self, udp_socket:"socket", msg: "str"):
        udp_socket.sendto(msg, (self.dest_ip, self.dest_port))

    def recv_msg(self, udp_socket:"socket"):
        recv_data = udp_socket.recvfrom(1024)
        recv_msg = recv_data[0]
        send_addr = recv_data[1]
        print("%s:%s" % (str(send_addr), recv_msg.decode("utf-8")))
        return recv_msg

    def close_socket(self ,udp_socket:"socket"):
        udp_socket.close()

    def __init__(self):
        # 创建套接字
        self.udp_socket = socket(AF_INET, SOCK_DGRAM)
        # 绑定信息
        self.udp_socket.bind(('127.0.0.1', self.loc_port))
        # 循环处理接下来要做的事情
        # while True:
        #     # 发送
        #     self.send_msg(udp_socket,"test")
        #     # 接收并显示
        #     # recv_data = self.recv_msg(udp_socket)
        #     # if str(recv_data) == "exit":
        #     #     break
        # # 关闭套接字
        # udp_socket.close()
