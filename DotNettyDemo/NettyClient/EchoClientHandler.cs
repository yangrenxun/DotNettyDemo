using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NettyClient
{
    public class EchoClientHandler : ChannelHandlerAdapter
    {
        IByteBuffer initialMessage;

        public override void ChannelActive(IChannelHandlerContext context) => context.WriteAndFlushAsync(this.initialMessage);

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer byteBuffer)
            {
                Console.WriteLine("Received from server: " + byteBuffer.ToString(Encoding.UTF8));
            }
        }

        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }

        public EchoClientHandler()
        {
            var hello = new Dictionary<string, string>
    {
        {"func", "sayHello"},
        {"username", "stevelee"}
    };
            SendMessage(ToStream(JsonConvert.SerializeObject(hello)));
        }

        private byte[] ToStream(string msg)
        {
            Console.WriteLine($"string length is {msg.Length}");
            using (var stream = new MemoryStream()) // (2)
            {
                Serializer.Serialize(stream, msg);
                return stream.ToArray();
            }
        }

        private void SendMessage(byte[] msg)
        {
            Console.WriteLine($"byte length is {msg.Length}");
            initialMessage = Unpooled.Buffer(msg.Length, msg.Length);
            initialMessage.WriteBytes(msg); // (3)
        }
    }
}
