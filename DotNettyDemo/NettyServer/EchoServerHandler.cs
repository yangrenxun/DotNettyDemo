using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace NettyServer
{
    /// <summary>
    /// 服务端处理事件函数
    /// </summary>
    public class EchoServerHandler : ChannelHandlerAdapter // ChannelHandlerAdapter 业务继承基类适配器 // (1)
    {
        /// <summary>
        /// 管道开始读
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer buffer)
            {
                Console.WriteLine($"message length is {buffer.Capacity}");
                var bstr = buffer.ToString(Encoding.UTF8);
                var obj = JsonConvert.DeserializeObject<Dictionary<string, string>>(bstr.Replace(")", "")); // (1)

                byte[] msg = null;
                if (obj["func"].Contains("sayHello"))  // (2)
                {
                    msg = Encoding.UTF8.GetBytes(Say.SayHello(obj["username"]));
                }

                if (obj["func"].Contains("sayByebye")) // (2)
                {
                    msg = Encoding.UTF8.GetBytes(Say.SayByebye(obj["username"]));
                }

                if (msg == null) return;
                // 设置Buffer大小
                var b = Unpooled.Buffer(msg.Length, msg.Length); // (3)
                IByteBuffer byteBuffer = b.WriteBytes(msg); // (4)
                context.WriteAsync(byteBuffer); // (5)
            }
        }

        /// <summary>
        /// 管道读取完成
        /// </summary>
        /// <param name="context"></param>
        public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush(); // (5)

        /// <summary>
        /// 出现异常
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exception"></param>
        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine("Exception: " + exception);
            context.CloseAsync();
        }
    }
}
