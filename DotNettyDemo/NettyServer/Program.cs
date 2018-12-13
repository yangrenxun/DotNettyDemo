using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;
using System;
using System.Threading.Tasks;

namespace NettyServer
{
    static class Program
    {
        static async Task RunServerAsync()
        {
            //ExampleHelper.SetConsoleLogger();

            // 申明一个主回路调度组
            var dispatcher = new DispatcherEventLoopGroup();

            /*
             Netty 提供了许多不同的 EventLoopGroup 的实现用来处理不同的传输。
             在这个例子中我们实现了一个服务端的应用，因此会有2个 NioEventLoopGroup 会被使用。
             第一个经常被叫做‘boss’，用来接收进来的连接。第二个经常被叫做‘worker’，用来处理已经被接收的连接，一旦‘boss’接收到连接，就会把连接信息注册到‘worker’上。
             如何知道多少个线程已经被使用，如何映射到已经创建的 Channel上都需要依赖于 IEventLoopGroup 的实现，并且可以通过构造函数来配置他们的关系。
             */

            // 主工作线程组，设置为1个线程
            IEventLoopGroup bossGroup = dispatcher; // (1)
            // 子工作线程组，设置为1个线程
            IEventLoopGroup workerGroup = new WorkerEventLoopGroup(dispatcher);

            try
            {
                // 声明一个服务端Bootstrap，每个Netty服务端程序，都由ServerBootstrap控制，通过链式的方式组装需要的参数
                var serverBootstrap = new ServerBootstrap(); // (2)
                // 设置主和工作线程组
                serverBootstrap.Group(bossGroup, workerGroup);

                // 申明服务端通信通道为TcpServerChannel
                serverBootstrap.Channel<TcpServerChannel>(); // (3)

                serverBootstrap
                    // 设置网络IO参数等
                    .Option(ChannelOption.SoBacklog, 100) // (5)

                    // 在主线程组上设置一个打印日志的处理器
                    .Handler(new LoggingHandler("SRV-LSTN"))

                    // 设置工作线程参数
                    .ChildHandler(
                        /*
                         * ChannelInitializer 是一个特殊的处理类，他的目的是帮助使用者配置一个新的 Channel。
                         * 也许你想通过增加一些处理类比如DiscardServerHandler 来配置一个新的 Channel 或者其对应的ChannelPipeline 来实现你的网络程序。
                         * 当你的程序变的复杂时，可能你会增加更多的处理类到 pipline 上，然后提取这些匿名类到最顶层的类上。
                         */
                        new ActionChannelInitializer<IChannel>( // (4)
                            channel =>
                            {
                                /*
                                 * 工作线程连接器是设置了一个管道，服务端主线程所有接收到的信息都会通过这个管道一层层往下传输，
                                 * 同时所有出栈的消息 也要这个管道的所有处理器进行一步步处理。
                                 */
                                IChannelPipeline pipeline = channel.Pipeline;

                                // 添加日志拦截器
                                pipeline.AddLast(new LoggingHandler("SRV-CONN"));

                                // 添加出栈消息，通过这个handler在消息顶部加上消息的长度。
                                // LengthFieldPrepender(2)：使用2个字节来存储数据的长度。
                                pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));

                                /*
                                  入栈消息通过该Handler,解析消息的包长信息，并将正确的消息体发送给下一个处理Handler
                                  1，InitialBytesToStrip = 0,       //读取时需要跳过的字节数
                                  2，LengthAdjustment = -5,         //包实际长度的纠正，如果包长包括包头和包体，则要减去Length之前的部分
                                  3，LengthFieldLength = 4,         //长度字段的字节数 整型为4个字节
                                  4，LengthFieldOffset = 1,         //长度属性的起始（偏移）位
                                  5，MaxFrameLength = int.MaxValue, //最大包长
                                 */
                                pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                                // 业务handler
                                pipeline.AddLast("echo", new EchoServerHandler());
                            }));

                // bootstrap绑定到指定端口的行为就是服务端启动服务，同样的Serverbootstrap可以bind到多个端口
                IChannel boundChannel = await serverBootstrap.BindAsync(8888); // (6)

                Console.WriteLine("wait the client input");
                Console.ReadLine();

                // 关闭服务
                await boundChannel.CloseAsync();
            }
            finally
            {
                // 释放指定工作组线程
                await Task.WhenAll( // (7)
                    bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1))
                );
            }
        }

        static void Main() => RunServerAsync().Wait();
    }
}
