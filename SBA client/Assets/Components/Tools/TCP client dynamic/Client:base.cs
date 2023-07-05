using System;
using System.Net.Sockets;
using static EasyConsole;

namespace Net
{
    partial class Client
    {
        public Client instance;
        private Socket Socket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public float TimeoutSec { set { Socket.ReceiveTimeout = (int)MathF.Round(value*1000,0); } get { return Socket.ReceiveTimeout/1000; } }
        public int MaxByteReceived;
        public int PORT;
        public string IP;

        /// <param name="timeout"> seconds </param>
        public void setUp(int port, string ip = "127.0.0.1", int max_byte_received = 2048, int timeout = 15)
        {
            IP = ip;
            PORT = port;
            MaxByteReceived = max_byte_received;
            TimeoutSec = timeout;
        }

        /// <summary> Make sure the client is set up before connecting. </summary>
        public bool Connect(int max_try = 3)
        {
            int i = 0;

            while (!Socket.Connected)
            {
                if(i >= max_try) return false;
                try
                {
                    i++;
                    print("Connecting: " + i);
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    Socket.Connect(IP, PORT);
                }
                catch (SocketException e)
                {
                    Console.Clear();
                    print("connect error: " + e.Message);
                }
            }

            Console.Clear();
            print("Connected");
            print(@"Type ""exit"" to properly disconnect");
            return true;
        }
        public void DisconnectAndRenew()
        {
            if(Socket.Connected) Socket.Shutdown(SocketShutdown.Both);
            Socket.Close();
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public bool SendBytes(byte[] b)
        {
            try
            {
                Socket.Send(b, 0, b.Length, SocketFlags.None);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("send byte error: " + e);
                return false;
            }
        }

        /// <returns> return received bytes. On error, return null. </returns>
        public byte[] ReceiveBytes()
        {

            byte[] buffer = new byte[MaxByteReceived];
            int length;
            try
            {
                length = Socket.Receive(buffer, MaxByteReceived,SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine("Receive byte error: " + e);
                return null;
            }
            if (length == 0) return null;
            return new ArraySegment<byte>(buffer, 0, length).ToArray();//exp point: Array or ToArray
        }
        /// <returns> return length of the received bytes. On error, return null. </returns>
        public int? ReceiveBytes(byte[] buffer)
        {
            int length;
            try
            {
                length = Socket.Receive(buffer, MaxByteReceived, SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine("Receive byte error: " + e);
                return null;
            }
            if (length == 0) return null;
            return length;
        }


    }
}
