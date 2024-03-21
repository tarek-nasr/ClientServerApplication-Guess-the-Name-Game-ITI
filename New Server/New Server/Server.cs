using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace New_Server
{

    internal class Server
    {
        
        public static int RoomCount { set; get; } = 0;
        
        public Thread ThreadServer { set; get; }
        public Thread HandleClient {  set; get; }
        public  TcpListener Listener { set; get; }

        public IPAddress ip;
        public BinaryReader Br { set; get; }
        public BinaryWriter Bw { set; get; }
        public NetworkStream Stream { set; get; }


        public Server() {
            ip = new IPAddress(new byte[] { 127,0,0,1 });    //pc   192, 168, 1, 10     lap    192,168,1,5    
            Listener = new TcpListener(ip, 12345);
            ThreadServer = new Thread(ServerListen);
            

        }

        public void Start()
        {
            Listener.Start();
            ThreadServer.Start();
        }
        void ServerListen()
        {
            while (true)
            {
                Socket ClientSocket = this.Listener.AcceptSocket(); // accept client
                
                bool flag = true;
                Stream = new NetworkStream(ClientSocket);
                while (flag)
                {
                    if (Stream.DataAvailable)
                    {
                        //store New Client info
                        string PlayerName = new BinaryReader(Stream).ReadString();
                        Console.WriteLine(PlayerName);
                        Player player = new Player(Stream, PlayerName);                       
                        flag = false;
                        //player.Bw.Write("l,ok");
                    }
                }


            }
        }

       

        
    }
}
