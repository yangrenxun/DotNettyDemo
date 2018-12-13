using DotNetty.Buffers;
using DotNetty.Transport.Channels;
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
        public override void ChannelRead(IChannelHandlerContext context, object message) // (2)
        {
            if (message is IByteBuffer buffer)    // (3)
            {
                Console.WriteLine("Received from client: " + buffer.ToString(Encoding.UTF8));
            }

            context.WriteAsync(message); // (4)
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
