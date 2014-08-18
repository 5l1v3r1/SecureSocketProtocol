﻿using ProtoBuf;
using SecureSocketProtocol3.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SecureSocketProtocol3.Network.Messages
{
    public abstract class IMessage
    {
        /// <summary> This is the message in raw size we received </summary>
        public int RawSize { get; set; }

        /// <summary> This is the message in raw size after decompression </summary>
        public int DecompressedRawSize { get; set; }

        public IMessage()
        {

        }

        public abstract void ProcessPayload(SSPClient client);

        public static byte[] Serialize(IMessage message)
        {
            using (MemoryStream ms = new MemoryStream())
            using (PayloadWriter pw = new PayloadWriter())
            {
                Serializer.Serialize(ms, message);
                pw.WriteUShort((ushort)ms.Length);
                pw.WriteBytes(ms.ToArray());
                return pw.ToByteArray();
            }
        }

        public static IMessage DeSerialize(Type HeaderType, PayloadReader pr)
        {
            ushort size = pr.ReadUShort();
            byte[] data = pr.ReadBytes(size);
            return (IMessage)Serializer.Deserialize(new MemoryStream(data), HeaderType);
        }
    }
}
