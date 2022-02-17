import socket

def main():
    # 创建一个udp套接字
    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    # 绑定本地的相关信息，如果一个网络程序不绑定，则系统会随机分配
    port = 44444
    local_addr = ('', port)  # ip地址和端口号，ip一般不写，表示本机的人和一个ip
    udp_socket.bind(local_addr)  # 收数据的一方要绑定端口 

    # 等待接收对方发送的数据
    # 这个变量是个元组 包含 对方发的内容 和 一个包含对方ip和端口的元组
    recv_data = udp_socket.recvfrom(1024)

    # 解析 收到的数据
    recv_msg = recv_data[0]
    send_addr = recv_data[1]

    print("%s:%s" % (str(send_addr), recv_msg.decode("utf-8")))
    # while 1: 
    #     # 获得数据
    #     send_data = input("输入要发送的数据：")
    #     if send_data == "exit":
    #         break
    #     # 可以使用套接字收发数据
    #     udp_socket.sendto(send_data.encode('utf_8'), dest_addr)

    # 关闭套接字收发数据
    udp_socket.close()

if __name__ == "__main__":
    main()
