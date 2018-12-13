using System;
using System.Collections.Generic;
using System.Text;

namespace NettyServer
{
    public static class Say
    {
        public static string SayHello(string content)
        {
            return $"hello {content}";
        }

        public static string SayByebye(string content)
        {
            return $"byebye {content}";
        }
    }
}
