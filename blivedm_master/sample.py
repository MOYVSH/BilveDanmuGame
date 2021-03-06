# -*- coding: utf-8 -*-
import asyncio
from distutils.log import debug
import random
import enum
import blivedm
from serverTCP import *

from protobuf import DanmuGameProtocol_pb2 as DGP

# 直播间ID的取值看直播间URL
TEST_ROOM_IDS = [
    419850,#自己的
    #3044248,#zc
]


async def main():
    await run_single_client()
    #await run_multi_client()


async def run_single_client():
    """
    演示监听一个直播间
    """
    print("run_single_client")
    room_id = random.choice(TEST_ROOM_IDS)
    # 如果SSL验证失败就把ssl设为False，B站真的有过忘续证书的情况
    client = blivedm.BLiveClient(room_id, ssl=True)
    handler = MyHandler()
    client.add_handler(handler)

    client.start()
    try:
        # 演示5秒后停止
        await asyncio.sleep(1)
        #client.stop()
        await client.join()
    finally:
        await client.stop_and_close()


async def run_multi_client():
    """
    演示同时监听多个直播间
    """
    clients = [blivedm.BLiveClient(room_id) for room_id in TEST_ROOM_IDS]
    handler = MyHandler()
    for client in clients:
        client.add_handler(handler)
        client.start()

    try:
        await asyncio.gather(*(
            client.join() for client in clients
        ))
    finally:
        await asyncio.gather(*(
            client.stop_and_close() for client in clients
        ))


class MyHandler(blivedm.BaseHandler):
    # # 演示如何添加自定义回调
    # _CMD_CALLBACK_DICT = blivedm.BaseHandler._CMD_CALLBACK_DICT.copy()
    #
    # # 入场消息回调
    # async def __interact_word_callback(self, client: blivedm.BLiveClient, command: dict):
    #     print(f"[{client.room_id}] INTERACT_WORD: self_type={type(self).__name__}, room_id={client.room_id},"
    #           f" uname={command['data']['uname']}")
    # _CMD_CALLBACK_DICT['INTERACT_WORD'] = __interact_word_callback  # noqa

    async def _on_heartbeat(self, client: blivedm.BLiveClient, message: blivedm.HeartbeatMessage):
        print(f'[{client.room_id}] 当前人气值：{message.popularity}')
    # 普通弹幕
    async def _on_danmaku(self, client: blivedm.BLiveClient, message: blivedm.DanmakuMessage):
        self.SendPack(DGP.MessageType.danmaku, message.uid, message.uname, message.msg)
        print(f'[{client.room_id}] 用户：{message.uname} 留言：{message.msg}')

    # 礼物消息
    async def _on_gift(self, client: blivedm.BLiveClient, message: blivedm.GiftMessage):
        mainpack = DGP.MainPack()
        mainpack.MessageType = DGP.MessageType.gift
        mainpack.UserID = message.uid
        mainpack.UserName = message.uname
        mainpack.UserText = '%s %s'%(message.gift_name,message.num)
        msg = mainpack.SerializeToString()
        head = (len(msg)).to_bytes(4, byteorder='little')
        server.send_msg(head)
        server.send_msg(msg)
        print(f'[{client.room_id}] {message.uname} 赠送{message.gift_name}x{message.num}'f' （{message.coin_type}瓜子x{message.total_coin}）')

    # 上舰消息
    async def _on_buy_guard(self, client: blivedm.BLiveClient, message: blivedm.GuardBuyMessage):
        mainpack = DGP.MainPack()
        mainpack.MessageType = DGP.MessageType.guard
        mainpack.UserID = message.uid
        mainpack.UserName = message.username
        mainpack.UserText = message.gift_name
        msg = mainpack.SerializeToString()
        head = (len(msg)).to_bytes(4, byteorder='little')
        server.send_msg(head)
        server.send_msg(msg)
        print(f'[{client.room_id}] {message.username} 购买{message.gift_name}')
    
    # SuperChat
    async def _on_super_chat(self, client: blivedm.BLiveClient, message: blivedm.SuperChatMessage):
        mainpack = DGP.MainPack()
        mainpack.MessageType = DGP.MessageType.superchat
        mainpack.UserID = message.uid
        mainpack.UserName = message.uname
        mainpack.UserText = message.message
        msg = mainpack.SerializeToString()
        head = (len(msg)).to_bytes(4, byteorder='little')
        server.send_msg(head)
        server.send_msg(msg)
        print(f'[{client.room_id}] 醒目留言 ¥{message.price} {message.uname}：{message.message}')


    def SendPack(self, type, id, name, message):
        mainpack = DGP.MainPack()
        mainpack.MessageType = type
        mainpack.UserID = id
        mainpack.UserName = name
        mainpack.UserText = message
        msg = mainpack.SerializeToString()
        head = (len(msg)).to_bytes(4, byteorder='little')
        server.send_msg(head)
        server.send_msg(msg)

server = serverTCP()
if __name__ == '__main__':
    server.__init__()
    server.BeginReceive()
    asyncio.get_event_loop().run_until_complete(main())
