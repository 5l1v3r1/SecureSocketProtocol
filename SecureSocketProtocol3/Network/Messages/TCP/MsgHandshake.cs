﻿using ProtoBuf;
using SecureSocketProtocol3.Network.Headers;
using SecureSocketProtocol3.Network.MazingHandshake;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecureSocketProtocol3.Network.Messages.TCP
{
    [ProtoContract]
    internal class MsgHandshake : IMessage
    {
        [ProtoMember(1)]
        public byte[] Data { get; set; }

        public MsgHandshake(byte[] Data)
            : base()
        {
            this.Data = Data;
        }
        public MsgHandshake()
            : base()
        {

        }

        public override void ProcessPayload(SSPClient client)
        {
            SSPClient _client = client as SSPClient;
            if (_client != null)
            {
                byte[] responseData = new byte[0];
                MazeErrorCode errorCode = MazeErrorCode.Error;
                Mazing mazeHandshake = _client.IsServerSided ? _client.serverHS : _client.clientHS;

                errorCode = mazeHandshake.onReceiveData(Data, ref responseData);

                if (responseData.Length > 0)
                {
                    client.Connection.SendMessage(new MsgHandshake(responseData), new SystemHeader());
                }

                client.Connection.HandshakeSync.Value = errorCode;
                if (errorCode != MazeErrorCode.Finished && errorCode != MazeErrorCode.Success)
                {
                    client.Connection.HandshakeSync.Pulse();
                }
                else if (errorCode == MazeErrorCode.Finished)
                {
                    //let's tell it's completed and apply the new key
                    client.Connection.ApplyNewKey(mazeHandshake, mazeHandshake.FinalKey, mazeHandshake.FinalSalt);

                    if (_client.IsServerSided)
                    {
                        try
                        {
                            client.onBeforeConnect();
                        }
                        catch (Exception ex)
                        {
                            client.onException(ex, ErrorType.UserLand);
                            return; //don't send that we're ready since we're clearly not at this point
                        }
                        client.Connection.SendMessage(new MsgInitOk(), new SystemHeader());
                    }
                    else
                    {
                        client.Connection.HandShakeCompleted = true;
                        client.Connection.HandshakeSync.Pulse();
                    }
                }
            }
        }
    }
}