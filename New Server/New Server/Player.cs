using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;

namespace New_Server
{
    enum playerStatus { PlayingOwner,PlayingOpponent,InRoom,NotPlaying,RequestPlay,Watching}
    internal class Player
    {

        public static Dictionary<int, Player> Players = new Dictionary<int, Player>();
        public static Dictionary<int, Room> Rooms =new Dictionary<int, Room>();

        public static int counter = 0;
        public  int id {  get;set; }
        public NetworkStream Stream { get; set; }
        public string Name { get; set; }
        public BinaryWriter Bw { get; set; }
        public BinaryReader Br { get; set; }
        public playerStatus Status { set; get; }
        public bool IsConnected { set; get; }
        public bool FirstWatch { set; get; } = true;
        public Task clientTask { set; get; }

        public Player(NetworkStream stream, string name)
        {
            Name = name;
            id = counter++;
            this.Stream = stream;
            Console.WriteLine($"Player name is {name} and number {counter}");
            Br = new BinaryReader(Stream);
            Bw = new BinaryWriter(Stream);
            IsConnected = true;
            Status = playerStatus.NotPlaying;
            Players.Add(id, this);
            clientTask = new Task(() => HandleRequests(this)) ;
            clientTask.Start();
        }

        // 0:key 1:name
        public void HandleRequests(Player p)
        {
            
            while (p.IsConnected)
            {
                if (p.Stream.DataAvailable)
                {
                    string m = p.Br.ReadString();
                    string[] ClientMsg = m.Split(',');
                    switch (ClientMsg[0])    //room
                    {
                        case "r":
                            handleRoom(ClientMsg, p);
                            break;
                        case "g": //game
                            HandleGame(ClientMsg, p);
                            break;
                        case "e":
                            HandleAfterGame(ClientMsg, p);
                            break;
                    }
                }
            }

        }

        public void handleRoom(string[] ClientMsg, Player p)
        {

            switch (ClientMsg[1]) //send rooms
            {
                case "get":
                    string RoomsList = "";
                    foreach (var rv in Rooms)
                    {
                        RoomsList += rv.Value.ToString();
                    }
                    p.Bw.Write(RoomsList);
                    Console.WriteLine("***************rooms sent *********************");
                    break;
                case "create":
                    //if (p.Status == "not-playing")
                    
                        Room room = new Room(ClientMsg[2]);
                        Console.WriteLine(ClientMsg[2]);
                        room.Owner = p;
                        p.Status = playerStatus.InRoom;
                        Rooms.Add(room.RoomId, room);
                        Console.WriteLine($"Room { room.RoomId} created by player {p.Name}");
                        p.Bw.Write($"r,creates,{room.RoomId},{room.Word}");
                                   
                    break;
                case "join":
                    int roomId = int.Parse(ClientMsg[2]); //r,join,roomId  
                    if (Rooms[roomId] != null && Rooms[roomId].Opponent==null && p.Status== playerStatus.NotPlaying)  // request to join room empty
                    {
                        Rooms[roomId].Owner.Bw.Write($"{p.Name}: want to join ?");
                        Rooms[roomId].Opponent = p;//*******
                        Rooms[roomId].NumberOfPlayers = 2;
                        Rooms[roomId].Join = false;
                        Rooms[roomId].Status = RoomStatus.waiting;
                        p.Status = playerStatus.InRoom;

                    } //accept opponent
                    else if (Rooms[roomId] != null && Rooms[roomId].Opponent!=null) //r,join,roomId,ao
                    {
                        if (ClientMsg[3] == "ao")
                        {
                            Rooms[roomId].Status = RoomStatus.playing;
                            Rooms[roomId].Opponent.Bw.Write("accept");  // tell the opponent you r accepted
                            Rooms[roomId].Opponent.Status = playerStatus.PlayingOpponent;
                            p.Status = playerStatus.PlayingOwner;
                            //Rooms[roomId].Opponent.Bw.Write("*,n");
                            Console.WriteLine($"Room {roomId} joined by player {p.Name}");


                        }
                        else if (ClientMsg[3] == "ro")
                        {
                            Rooms[roomId].Opponent.Bw.Write("reject"); //reject opponent
                            Rooms[roomId].Status = RoomStatus.waiting;
                            Rooms[roomId].Opponent = null;
                            Rooms[roomId].NumberOfPlayers = 1;
                            Rooms[roomId].Join = true;
                            Rooms[roomId].Opponent.Status = playerStatus.NotPlaying;
                        }
                    }
                    else
                    {
                        p.Bw.Write("r,refreash");
                    }
                    break;
                case "watch":
                    int d = int.Parse(ClientMsg[2]);
                    if (ClientMsg[3] == "i" && Rooms[d] != null && Rooms[d].Status == RoomStatus.playing)
                    {                                           
                            Rooms[d].Watchers.Add(p.id, p);
                            p.Status = playerStatus.Watching;
                            Room.WatchersCount++;
                            p.Bw.Write("watch,accept");
                        Console.WriteLine($"player {p.Name} watching in room number {d}");
                    }else if (ClientMsg[3] == "o" && Rooms[d] != null)
                    {
                        Rooms[d].Watchers.Remove(p.id);
                        p.Status = playerStatus.NotPlaying;
                        Room.WatchersCount--;
                        p.Bw.Write($"watch,reject,{p.Name}");
                    }else if (ClientMsg[3] =="get" && Rooms[d] != null && Rooms[d].Status == RoomStatus.playing)
                    {
                        foreach(string s in Rooms[d].doneFromWord)
                        {
                            p.Bw.Write(s);
                        }
                    }
                    break;
                default:
                    Console.WriteLine("wrong command for room");
                    break;

            }

        }

        public void HandleGame(string[] ClientMsg,Player p) //g,roomId,c:2:3
        {
            int d = int.Parse(ClientMsg[1]);        
            
            //starting the game first time
            if (Rooms[d] != null && Rooms[d].Status== RoomStatus.playing)
            {
                //decide which player is sending
                Player p2;  
                if (p.Status == playerStatus.PlayingOwner)
                {
                    p2 = Rooms[d].Opponent;
                }
                else if(p.Status == playerStatus.PlayingOpponent)
                {
                    p2 = Rooms[d].Owner;
                }
                else
                {
                    Console.WriteLine("not authorized action");
                    return; // not authorized player
                }
                
                
                string[] symbols = ClientMsg[2].Split(':');
                int n = symbols.Length;
                //wrong char
                if (symbols[0]=="*") 
                {
                    
                     p2.Bw.Write("*,a");  // send other plyer that char was wrong and it is his turn
                    Rooms[d].doneFromWord.Add("*,n");
                    if (Rooms[d].Watchers.Count > 0) { 

                        // if there is watchers send to them
                         foreach(Player w in  Rooms[d].Watchers.Values)
                         {
                             w.Bw.Write("*,n"); //broadcast the result and and make watchers in active
                         }
                     }
               //-----------------------------------------------------------------------------------------------------------------
                     //correct character
                }else if (symbols[0].All(char.IsLetter)) { 

                    //first step to remove the chars from dict
                    if(Rooms[d].WordDict.Count >= n-1) {
                        for(int i = 1; i < n; i++)
                        {
                            int ind = int.Parse(symbols[i]);
                            Rooms[d].WordDict.Remove(ind);
                        }
                        Console.WriteLine($"The number of remaining characters is {Rooms[d].WordDict.Count}");
                    }

                    //check if there is still characters in dict mean the game continues
                    if (Rooms[d].WordDict.Count > 0)
                    {
                        p2.Bw.Write(ClientMsg[2]);
                        Rooms[d].doneFromWord.Add(ClientMsg[2]);
                        if (Rooms[d].Watchers.Count > 0)
                        {
                            foreach (Player w in Rooms[d].Watchers.Values)
                            {
                                w.Bw.Write(ClientMsg[2]);
                            }
                        }
                    }
                    else  // game ended
                    {
                        p2.Bw.Write(ClientMsg[2]);
                        foreach (Player w in Rooms[d].Watchers.Values)
                        {
                            w.Bw.Write(ClientMsg[2]);
                        }
                        Rooms[d].doneFromWord.Add(ClientMsg[2]);

                        p.Bw.Write("win");
                        Rooms[d].winner = p.Name;
                        Console.WriteLine($"player {p.Name} in room {d} won ");
                        p2.Bw.Write("lose");
                        Rooms[d].Status = RoomStatus.waiting;
                        Rooms[d].loser = p2.Name;
                        Rooms[d].Score();
                    }

                }
            }            
        }

        public void HandleAfterGame(string[] ClientMsg,Player p) //e,roomId, yp || np
        {
            int d = int.Parse(ClientMsg[1]);
            if(Rooms[d] != null && Rooms[d].Status == RoomStatus.waiting && Rooms[d].WordDict.Count == 0)
            {
                Player p2;
                if (p.Status == playerStatus.PlayingOwner)
                {
                    p2 = Rooms[d].Opponent;
                }
                else if (p.Status == playerStatus.PlayingOpponent)
                {
                    p2 = Rooms[d].Owner;
                }
                else
                {
                    Console.WriteLine($"not authorized action on Room number {Rooms[d].RoomId}");
                    return; // not authorized player
                }
                switch (ClientMsg[2])
                {
                    case "yp": //e,roomId,yp
                        if(p2.Status == playerStatus.RequestPlay)
                        {
                            Rooms[d].SelectedWord();
                            Rooms[d].Status = RoomStatus.playing;
                            Rooms[d].Owner.Status=playerStatus.PlayingOwner;
                            Rooms[d].Opponent.Status=playerStatus.PlayingOpponent;
                            Rooms[d].Owner.Bw.Write($"start game,{Rooms[d].Word},{Rooms[d].Owner.Name},{Rooms[d].Opponent.Name},o");
                            Rooms[d].Opponent.Bw.Write($"start game,{Rooms[d].Word},{Rooms[d].Owner.Name},{Rooms[d].Opponent.Name},r");
                           
                            foreach (Player w in Rooms[d].Watchers.Values)
                            {
                                w.Status = playerStatus.NotPlaying;
                                w.Bw.Write($"watch,{Rooms[d].Owner.Name} is playing again with {Rooms[d].Opponent.Name} do you want to continue watching ?");
                            }
                            Rooms[d].Watchers.Clear();
                            Console.WriteLine($"Player {p.Name} want to play again");
                            Console.WriteLine($"game on room {Rooms[d].RoomId} starting again");
                        }
                        else if (p2.Status == playerStatus.NotPlaying) //one no and the other yes
                        {
                            Rooms[d].SelectedWord();
                            Rooms[d].Status = RoomStatus.waiting;
                            Rooms[d].Owner = p;
                            Rooms[d].Opponent = null;
                            p.Status = playerStatus.InRoom;
                            Rooms[d].NumberOfPlayers = 1;
                            Rooms[d].Join = true;
                            p.Bw.Write($"play new,{Rooms[d].Word}");//***********************************************

                            foreach (Player w in Rooms[d].Watchers.Values)
                            {
                                w.Status = playerStatus.NotPlaying;
                                w.Bw.Write("watch,reject,end");
                            }
                            Rooms[d].Watchers.Clear();
                        }
                        else
                        {
                            p.Status = playerStatus.RequestPlay;
                            Console.WriteLine($"Player {p.Name} want to play again");
                        }
                        break;
                    case "np":  //e,roomId,np
                        if(p2.Status == playerStatus.RequestPlay) {
                            Rooms[d].SelectedWord();
                            Rooms[d].Status = RoomStatus.waiting;
                            Rooms[d].Owner = p2;
                            Rooms[d].Opponent = null;
                            p2.Status = playerStatus.InRoom;
                            Rooms[d].NumberOfPlayers = 1;
                            Rooms[d].Join = true;
                            p2.Bw.Write($"play new,{Rooms[d].Word}");
                            p.Status = playerStatus.NotPlaying;
                            //p.Bw.Write("l,end game");

                        }
                        else if(p2.Status == playerStatus.NotPlaying)
                        {
                            p.Status = playerStatus.NotPlaying;
                            //p.Bw.Write("l,end game");
                            //Rooms[d].Opponent.Bw.Write("l,end game");
                            //Rooms[d].Owner.Bw.Write("l,end game");
                            Console.WriteLine($"player {p.Name} has left");
                            Rooms.Remove(d);
                            Console.WriteLine($"room number {d} is removed");
                        }
                        else
                        {
                            p.Status = playerStatus.NotPlaying;
                            //p.Bw.Write("l,end game");
                            Console.WriteLine($"player {p.Name} has left");
                        }
                        


                        //handle watchers
                        foreach (Player w in Rooms[d].Watchers.Values)
                        {
                            w.Status = playerStatus.NotPlaying;
                            w.Bw.Write("watch,reject,end");
                        }
                        Rooms[d].Watchers.Clear();
                        
                        break;
                }

            }
        }



    }
}
