using System;
using System.Text;

namespace EasyRabbitMqClient.Core.Extensions
{
    public static class ByteExtensions
    {
        public static string AsString(this object obj)
        {
            switch (obj)
            {
                case byte[] body:
                    return Encoding.UTF8.GetString(body);
                case ReadOnlyMemory<byte> body:
                    return Encoding.UTF8.GetString(body.Span);
            }

            return obj.ToString();
        }
    }
}