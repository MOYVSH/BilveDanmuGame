import socket

def main():
    # 创建一个udp套接字
    udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    # 准备接收方的地址
    host = 'localhost'
    port = 44444    # 同一个端口不能同一时刻被用两次
    dest_addr = (host, port)
    
    while 1:
        # 获得数据
        send_data = input("输入要发送的数据：")
        if send_data == "exit":
            break
        # 可以使用套接字收发数据
        udp_socket.sendto(send_data.encode('utf_8'), dest_addr)

        
    # 关闭套接字收发数据
    udp_socket.close()

if __name__ == "__main__":
    main()
