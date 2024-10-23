using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace TCPClientExtensions
{
    
    public class CustomQuery
    {
        public int QueryNumber { get; set; }
        public int ThreadNumber { get; set; }
        public int RandomInt { get; set; }

        public string ToJson() => JsonSerializer.Serialize(this);
    }

    public enum ResponceCode
    {
        Success=200,
        ClientError=400,
        ServerError=500
    }

    public class CustomResponce
    {
        public int QueryNumber { get; set; }
        public string Responce { get; set; }
        public ResponceCode ResponseCode { get; set; } = ResponceCode.Success;

        public string ToJson() => JsonSerializer.Serialize(this);
    }

    public class CorruptedDataException : Exception
    {
        public CorruptedDataException()
        {
        }

        public CorruptedDataException(string message) : base(message)
        {
        }

        public CorruptedDataException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public static class ExtendedTCPClient
    {
        const byte MESSAGE_SIZE_BYTES = 8;

        public static void WriteCustom(this TcpClient client, string message)
        {
            ulong messageSize = (ulong)message.Length;

            byte[] messageSizeBytes = BitConverter.GetBytes(messageSize);
            byte[] messageBytes = ASCIIEncoding.ASCII.GetBytes(message);

            byte[] messageWithSize = messageSizeBytes.Concat(messageBytes).ToArray();

            Console.WriteLine($"Sending {messageSize} bytes. Message: {message}");

            NetworkStream nwStream = client.GetStream();

            nwStream.Write(messageWithSize, 0, messageWithSize.Length);
        }

        public static string ReadCustom(this TcpClient client)
        {
            NetworkStream nwStream = client.GetStream();

            byte[] receivedMessageSizeBytes = new byte[8];
            nwStream.Read(receivedMessageSizeBytes, 0, receivedMessageSizeBytes.Count());
            ulong dataReceivedSize = BitConverter.ToUInt64(receivedMessageSizeBytes, 0);

            byte[] receivedMessageBytes = new byte[dataReceivedSize];
            nwStream.Read(receivedMessageBytes, 0, receivedMessageBytes.Length);

            string dataReceived = Encoding.ASCII.GetString(receivedMessageBytes);

            if (dataReceivedSize != (ulong)dataReceived.Length)
                throw new CorruptedDataException($"Not all data was recived correctly. Received message: {dataReceived}. Expected to receive {dataReceivedSize} bytes, but got {dataReceived.Length} bytes instead.");

            Console.WriteLine($"Receiving {dataReceivedSize} bytes. Message: {dataReceived}");

            return dataReceived;
        }
    }
}
