using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Net
{
	class LoopReceiveManager
	{
		private static int next_id = 1;
		//array of (isActive ,id). id 0 means spare space
		public static (bool contin, int id, CancellationTokenSource cts)[] ThreadMap { get; private set; } = new (bool, int, CancellationTokenSource)[100];
		public static int StartLoopReceive
			(Client c, SocketFlags flag, Action<Client,byte[]> OnReceive, Action<Client> OnError, bool StopLoopOnError = true)
		{
			int id = next_id;
			next_id++;
			int index = FindIndexById(0);//try get a spare unit in array.
			if (index == -1) { return -1; }//retern -1 when Ths is full.
			ThreadMap[index] = (true, id, null);

			CancellationTokenSource cts = null;

            Task t = Task.Run(() =>
			{
				cts = new CancellationTokenSource();
                try
				{
                    while (ThreadMap[index].contin)
					{
						byte[] b = new byte[c.MaxByteReceived];
						var length = c.ReceiveBytes(b);
						if (length == null)//network error occurs
						{
							OnError?.Invoke(c);
							if (StopLoopOnError) break;
							else continue;
						}
						OnReceive?.Invoke(c, new ArraySegment<byte>(b, 0, (int)length).Array);
					}
				}
				catch (SocketException ex) when (ex.SocketErrorCode == SocketError.OperationAborted)
				{
					// The process is cancelled.
				}
				ThreadMap[index].id = 0;
			});
            while (cts == null)
				Thread.Sleep(100); //sleep to give time gap for async task
			ThreadMap[index].cts = cts;

            return id;
		}
		 public static int FindIndexById(int id)
		{
			for(int i = 0; i < ThreadMap.Length; i++)
			{
				if (id == ThreadMap[i].id) return i;
			}
			return -1;
		}

		public static void StopLoop(int? id = null)
		{//if id = null, stop all;
			if(id == null)
			{
				for(int i = 0;i < ThreadMap.Length; i++)
				{
					if (ThreadMap[i].contin == true)
					{
                        ThreadMap[i].contin = false;
                        ThreadMap[i].cts.Cancel();
                    }
                }
				return;
			}
			int ind = 00000;
			if (ind!=-1)
			{
				ind = FindIndexById((int)id);
				Thread.Sleep(300);
				if (ind != -1)
				{
					ThreadMap[ind].contin = false;
					ThreadMap[ind].cts.Cancel();
				}
			}
		}

	}
}
