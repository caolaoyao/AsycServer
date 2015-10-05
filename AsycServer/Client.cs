using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsycServer
{
    public class Client
    {
        public int ID;
        public IPEndPoint tcpAdress, udpAdress;
        AsyUdpServer server;
        Socket socket;
        Stopwatch pingWatch;

        public bool Pinging
        {
            get
            {
                return pingWatch != null;
            }
        }

        public Client(int id, Socket sock, AsyUdpServer serv)
        {
            ID = id;
            server = serv;
            socket = sock;

            tcpAdress = (IPEndPoint)sock.RemoteEndPoint;
            Thread t = new Thread(AliveThread);
            t.Start();
        }

        public void SendAcceptPoll()
        {
            socket.Send(BitConverter.GetBytes(ID));
        }

        void AliveThread()
        {
            while (IsConnected() && server.Active)
            {
                Thread.Sleep(1000);
            }

            Disconnect();
        }

        bool IsConnected()
        {
            try
            {
                if (udpAdress != null) socket.Send(new byte[] { 0 });
                return true;
            }
            catch (SocketException e)
            {
                return false;
            }
            catch (Exception e)
            {
                server.CatchException(e);
                return false;
            }
        }

        public void Send(MessageBuffer msg)
        {
            server.Send(msg, this);
        }

        public void Disconnect()
        {
            if (socket == null) return;

            socket.Close();
            socket = null;
            server.ClientDisconnected(this);
        }

        public void Ping()
        {
            if (Pinging)
            {
                server.PingResult(this, pingWatch.Elapsed.Milliseconds);
                pingWatch = null;
            }
            else
            {
                pingWatch = Stopwatch.StartNew();
                server.Send(new MessageBuffer(new byte[] { AsyUdpServer.pingByte }), this);
            }
        }
    }
}
