using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System;
using DanmuGameProtocol;

namespace Connection
{
    public class Message
    {
        public List<MainPack> msgList = new List<MainPack>();
        private byte[] buffer = new byte[1024];
        private int startIndex; //��ǰ�洢�˶�������

        public byte[] Buffer
        {
            get
            {
                return buffer;
            }
        }

        public int StartIndex
        {
            get
            {
                return startIndex;
            }
        }

        public int Resize
        {
            get
            {
                return buffer.Length - startIndex;
            }
        }

        /// <summary>
        /// ���յ�����Ϣ����
        /// </summary>
        /// <param name="length"></param>
        public void ReadBuffer(int length)
        {
            if (length < 4) return;
            startIndex += length;
            if (startIndex <= 4) return;
            //countΪ���峤��
            int count = BitConverter.ToInt32(buffer, 0);//������ͷ(��ͷ����ǰ�������ݳ���)  ��������ǰ�ĸ��ֽ�
            while (true)
            {
                if (startIndex >= count + 4)
                {
                    MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 4, length);
                    Array.Copy(buffer, count + 4, buffer, 0, startIndex - count - 4);
                    startIndex -= count + 4;
                    msgList.Add(pack);
                }
                else
                {
                    break;
                }
            }
        }
    }
}

