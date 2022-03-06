using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanmuHead
{
    /// <summary>
    /// �ܳ��� (Э��ͷ + ���ݳ���)
    /// </summary>
    public int PacketLength;

    /// <summary>
    /// ͷ���� 
    /// </summary>
    public short HeaderLength;

    /// <summary>
    /// �汾
    /// </summary>
    public short Version;

    /// <summary>
    /// �������� (��Ϣ����)
    /// </summary>
    public int Action;

    /// <summary>
    /// ����, �̶�Ϊ1
    /// </summary>
    public int Parameter;

    /// <summary>
    /// �������ж�ȡ������ת��Ϊ��Ϣͷ
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static DanmuHead BufferToDanmuHead(byte[] buffer)
    {
        if (buffer.Length < 16)
        {
            throw new ArgumentException();
        }

        return new DanmuHead
        {
            PacketLength = EndianBitConverter.EndianBitConverter.BigEndian.ToInt32(buffer, 0),
            HeaderLength = EndianBitConverter.EndianBitConverter.BigEndian.ToInt16(buffer, 4),
            Version = EndianBitConverter.EndianBitConverter.BigEndian.ToInt16(buffer, 6),
            Action = EndianBitConverter.EndianBitConverter.BigEndian.ToInt32(buffer, 8),
            Parameter = EndianBitConverter.EndianBitConverter.BigEndian.ToInt32(buffer, 12),
        };
    }

    /// <summary>
    /// �������ݲ��ֵĳ���
    /// </summary>
    /// <returns>
    ///    PacketLength - HeaderLength ��������:int
    /// </returns>
    public int MessageLength()
    {
        return PacketLength - HeaderLength;
    }
}
