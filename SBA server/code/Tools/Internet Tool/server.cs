using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Net
{
    class server
    {
        public static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        public static int max_users = 8;

        private static int MaxByteReceived = 2048;
        //private static int PORT;
        private static readonly byte[] LoopBuffer = new byte[MaxByteReceived];

        private static Action<byte[],Socket> OnLoopReceive;
        public static Action<Socket> OnLoopError;
        public static void SetReceiveCallback(Action<byte[], Socket> function)
        {
            OnLoopReceive = function;
        }
        public static void StartServer(int port, int max_bytes_received = 2048)
        {
            MaxByteReceived = max_bytes_received;
            Console.WriteLine("...Starting server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(LoopAccept, null);
            Console.WriteLine("==Server started==");
        }

        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        public static void DisconnectAll()
        {
            foreach (Socket socket in clientSockets)
            {
                Disconnect(socket);
            }
            serverSocket.Close();
        }
        public static void Disconnect(Socket s)
        {
            s.Shutdown(SocketShutdown.Both);
            s.Close();
            clientSockets.Remove(s);
        }

        public static void SendToAll(byte[] b)
        {
            bool[] succ = new bool[clientSockets.Count];
            int i = 0;
            foreach (Socket s in clientSockets)
            { 
                succ[i] = Send(b, s);
                i++;
            }
        }
        public static bool Send(byte[] b, Socket s)
        {
            try
            {
                s.Send(b);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static void LoopAccept(IAsyncResult AR)
        {//connect
            Socket socket;
            try
            {
                //AcceptConnect
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException ex) //this error is due to connected client send a connection request
            {
                //loopreceive should automaticaly and sync.ly disconnect the client If there is error on receive (due to client disconnect)
                //so, generally, ignoring this new connection request is enough;
                Console.WriteLine("ObjectDisposedException:"+ex);
                //WhenLoopError?.Invoke(null);
                return;
            }
            if (clientSockets.Count >= max_users)
            {
                Disconnect(socket);
            }
            else
            {
                clientSockets.Add(socket);
                socket.BeginReceive(LoopBuffer, 0, MaxByteReceived, SocketFlags.None, LoopReceive, socket);
                Console.WriteLine("Client connected: " + (socket.RemoteEndPoint as IPEndPoint).Address);
            }
            serverSocket.BeginAccept(LoopAccept,null);
        }
        public static void LoopReceive(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int length;

            try
            {
                length = current.EndReceive(AR);

                byte[] recBuf = new byte[length];
                Array.Copy(LoopBuffer, recBuf, length);
                OnLoopReceive?.Invoke(recBuf, current);

                //start listen to current client again
                current.BeginReceive(LoopBuffer, 0, MaxByteReceived, SocketFlags.None, LoopReceive, current);
            }
            catch (SocketException)//usually due to client disconnect
            {
                Console.WriteLine("Client disconnected: " + (current.RemoteEndPoint as IPEndPoint).Address);
                Disconnect(current);
                OnLoopError?.Invoke(current);
                return;
            }
            catch (ObjectDisposedException)
            {
                //in this case, socket is disposed, the socket is removed from the ClientSockets <list>
                //so no need to use Disconnect();
                Console.WriteLine("Client disconnected with a return signal: " + current.ToString());
                //Console.WriteLine("no. of clients: " + clientSockets.Count);
            }


        }

        
    }
}