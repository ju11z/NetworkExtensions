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

    public class ServerLoginResponce
    {
        public string Message { get; init; }

        public bool IsClientLoggedIn { get; init; }

        public ServerLoginResponce(string message, bool isClientLoggedIn)
        {
            Message = message;
            IsClientLoggedIn = isClientLoggedIn;
        }
    }

    public class ClientLoginResponce
    {
        public string Login { get; init; }

        public ClientLoginResponce(string login)
        {
            Login = login;
        }
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

        public static void WriteCustom(this TcpClient client, string message, bool log=true)
        {
            ulong messageSize = (ulong)message.Length;

            byte[] messageSizeBytes = BitConverter.GetBytes(messageSize);
            byte[] messageBytes = ASCIIEncoding.ASCII.GetBytes(message);

            byte[] messageWithSize = messageSizeBytes.Concat(messageBytes).ToArray();

            if(log)
                Console.WriteLine($"Sending {messageSize} bytes. Message: {message}");

            NetworkStream nwStream = client.GetStream();

            nwStream.Write(messageWithSize, 0, messageWithSize.Length);
        }

        public static async Task WriteCustomAsync(this TcpClient client, string message, bool log = true)
        {
            ulong messageSize = (ulong)message.Length;

            byte[] messageSizeBytes = BitConverter.GetBytes(messageSize);
            byte[] messageBytes = ASCIIEncoding.ASCII.GetBytes(message);

            byte[] messageWithSize = messageSizeBytes.Concat(messageBytes).ToArray();

            if (log)
                Console.WriteLine($"Sending {messageSize} bytes. Message: {message}");

            NetworkStream nwStream = client.GetStream();

            await nwStream.WriteAsync(messageWithSize, 0, messageWithSize.Length);
        }

        public static string ReadCustom(this TcpClient client, bool log=true)
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

            if (log)
                Console.WriteLine($"Receiving {dataReceivedSize} bytes. Message: {dataReceived}");

            return dataReceived;
        }

        public static async Task<string> ReadCustomAsync(this TcpClient client, bool log = true)
        {
            NetworkStream nwStream = client.GetStream();

            byte[] receivedMessageSizeBytes = new byte[8];
            await nwStream.ReadAsync(receivedMessageSizeBytes, 0, receivedMessageSizeBytes.Count());
            ulong dataReceivedSize = BitConverter.ToUInt64(receivedMessageSizeBytes, 0);

            byte[] receivedMessageBytes = new byte[dataReceivedSize];
            await nwStream.ReadAsync(receivedMessageBytes, 0, receivedMessageBytes.Length);

            string dataReceived = Encoding.ASCII.GetString(receivedMessageBytes);

            if (dataReceivedSize != (ulong)dataReceived.Length)
                throw new CorruptedDataException($"Not all data was recived correctly. Received message: {dataReceived}. Expected to receive {dataReceivedSize} bytes, but got {dataReceived.Length} bytes instead.");

            if (log)
                Console.WriteLine($"Receiving {dataReceivedSize} bytes. Message: {dataReceived}");

            return dataReceived;
        }

        public static string GenerateRandomLatinString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();

            for (int i = 0; i < length; i++)
            {
                int index = rnd.Next(chars.Length);
                sb.Append(chars[index]);
            }

            return sb.ToString();
        }
    }
}
