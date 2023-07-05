using System;
using System.Net.Sockets;

namespace Net
{
	partial class Client : LoopReceiveManager
	{
		private int LoopID = -1;//-1: impossible id
		public Action<Client, byte[]> OnLoopReceived;
		public Action<Client> OnLoopError;
		public bool StopOnError;

		public void SetupLoopRecv(Action<Client, byte[]> onLoopReceived = null, Action<Client> whenLoopError = null)
		{
            OnLoopReceived = onLoopReceived;
            OnLoopError = whenLoopError;
        }
        /// <summary> please SetupLoopRecv before turning loop-receive on </summary>
        /// <param name="StopOnError"> whether to turn off the loop on error </param>
        public void TurnLoopReceive(bool On, bool StopOnError = true)
		{
			this.StopOnError = StopOnError;
			if (On)
			{
				if (FindIndexById(LoopID) == -1) LoopID = StartLoopReceive(this, SocketFlags.None, OnReceive: OnLoopReceived, OnError: _OnError, StopLoopOnError: StopOnError);
			}
			else
			{
				StopLoop(LoopID);
				LoopID = -1;
			}
		}
		private void _OnError(Client c)
		{
			if (StopOnError)
				TurnLoopReceive(false);

			OnLoopError?.Invoke(c);
		}
		public bool LoopReceiveIsActive
		{
			get
			{
				if (LoopID == -1) return false;
				int ind = FindIndexById(LoopID);
				return (ind != -1 || ThreadMap[ind].contin);
			}
		}
	}
}
