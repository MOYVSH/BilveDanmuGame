using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;

namespace Connection
{
    public class Message
    {
        private byte[] buffer = new byte[1024];
        private int startIndex;

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

        public void ReadBuffer(int length)
        { 
        
        }
    }
}

