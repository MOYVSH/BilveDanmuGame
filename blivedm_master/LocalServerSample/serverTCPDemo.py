import socket

from protobuf import test_pb2 as DGP
mainpack = DGP.MainPack()
mainpack.UserName = "MOYV"
mainpack.UserText = "test"
mainpack.ip = "192.168.1.1"
mainpack.id = 2
send_msg =mainpack.SerializeToString()
print(send_msg)

def main():
    # 创建套接字
    tcp_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    # 绑定本地信息
    loc_port = 9787
    tcp_socket.bind(('localhost',loc_port))
    # 默认的套接字由主动变为监听
    tcp_socket.listen(128)

    # 等待客户端连接
    # 循环的目的调用多次accept 从而为多个客户端服务
    while True:
        # client_socket 为这个客户端服务
        # client_addr 这个客户端的地址
        print("等待新客户端的到来...")
        new_client_socket,client_addr = tcp_socket.accept()
        print("新客户端的到来%s" % str(client_addr))

        # 循环的目的为一个客户端服务多次
        while True:
            # 接受客户端发送过来的请求
            recv_data = new_client_socket.recv(1024)
            print("客户端发送过来的请求是%s" % recv_data.decode("utf-8"))

            # 如果recv解堵塞，两种方式：
            # 1.客户端发送过来数据
            # 2.客户端调用了close导致的
            if recv_data:
                # 回发一些消息给客户端
                #new_client_socket.send("喝汤多是一件美逝".encode("utf-8"))

                new_client_socket.send(send_msg)
            else:
                break

        # 关闭套接字
        new_client_socket.close()
    tcp_socket.close()
if __name__ =="__main__":
    main()
