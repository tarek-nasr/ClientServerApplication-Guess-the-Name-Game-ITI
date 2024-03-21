using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace login
{
    public partial class form2 : Form


    {
        Dictionary<int, string> Roomlist;
        BinaryReader Br;
        BinaryWriter Bw;
        string name;
        string[] rooms;
        NetworkStream Stream;
        Thread chooseCategory;
        int roomID;
        int numberOfPlayers;
        string owner;
        string room;
        string status;
        bool allowjoin;
        string category;
        string word;
        Thread goplay;
        string[] m;
        string[] joinRoom;
        

        public form2()
        {
            InitializeComponent();
            join.Enabled = false;
            watch.Enabled = false;
            Roomlist=new Dictionary<int, string>();
            
        }
        public form2(NetworkStream stream, string n)
        {
            InitializeComponent();
            Stream = stream;
            Bw = new BinaryWriter(stream);
            Br = new BinaryReader(stream);
            this.name = n;
            join.Enabled = false;
            watch.Enabled = false;
            Roomlist = new Dictionary<int, string>();
        }
        void RefreshPage()
        {
            try
            {
                bool flag = true;
                listBox1.Items.Clear();
                Roomlist.Clear();
                Bw.Write("r,get");//request to get rooms from server
                

                while (flag)
                {
                    if (Stream.DataAvailable)
                    {
                        rooms = Br.ReadString().Split(';');
                        flag = false;
                    }
                }
                for (int i = 0; i < rooms.Length ; i++)
                {
                    if (rooms[i].Contains(","))
                    {
                        string[] roomInfo = rooms[i].Split(",");
                        int d = int.Parse(roomInfo[0]);

                        Roomlist.Add(d, rooms[i]);

                        //Room:10,Owner:david,Opponent:mina,Status:playing,Category:Anime,Watchers:3;
                        listBox1.Items.Add($"Room:{roomInfo[0]}  ,Owner:{roomInfo[2]}  ,Opponent:{roomInfo[3]}  ,Status:{roomInfo[4]}  ,Category:{roomInfo[6]}  ,Watchers:{roomInfo[8]}");
                    }
                    
                }
            }
            catch (IOException)
            {
                MessageBox.Show("server is not connected,close and try again");

            }

        }

        private void form2_Load(object sender, EventArgs e)
        {
            RefreshPage();

        }

        private void refresh_Click(object sender, EventArgs e)
        {
            RefreshPage();

        }

        private void create_Click(object sender, EventArgs e)
        {
            chooseCategory = new Thread(openCreate);
            Close();
            chooseCategory.Start();
        }
        void openCreate()
        {
            Application.Run(new create(Stream, name));
        }

        private void join_Click(object sender, EventArgs e)
        {
            m = listBox1.SelectedItem.ToString().Split(",");             //Room:10,Owner:david,Opponent:mina,Status:playing,Category:Anime,Watchers:10
            string[] cell  = m[0].Split(":");
            int i = int.Parse(cell[1]);
            joinRoom = Roomlist[i].Split(",");
            try
            {
                Bw.Write("r,join," + i);
                //wait for the request

                bool flag = true;
                string OwnerMsg;

                while (flag)
                {
                    if (Stream.DataAvailable)
                    {
                        OwnerMsg = Br.ReadString();
                        switch (OwnerMsg)
                        {
                            case "accept":

                                goplay = new Thread(play);
                                Close();
                                goplay.Start();


                                break;
                            case "reject":
                                MessageBox.Show("can't watch");
                                break;

                        }
                        flag = false;
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Server is disconnected,close and try again");
            }
        }
        //click
        private void listBox1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                join.Enabled = true;
                watch.Enabled = true;
            
            }
        }
        void play()
        {
            if (joinRoom[3] =="")
                 Application.Run(new Play(Stream, joinRoom[0], joinRoom[7], joinRoom[2], joinRoom[6], name,"r"));
            else
                Application.Run(new Play(Stream, joinRoom[0], joinRoom[7], joinRoom[2], joinRoom[6], joinRoom[3], "w"));

        }

        private void watch_Click(object sender, EventArgs e)
        {
            m = listBox1.SelectedItem.ToString().Split(",");
            string[] cell = m[0].Split(":");
            int i = int.Parse(cell[1]);
            joinRoom = Roomlist[i].Split(",");
            try
            {
                Bw.Write($"r,watch,{i},i");
                

                bool flag = true;
                string OwnerMsg;

                while (flag)
                {
                    if (Stream.DataAvailable)
                    {
                        OwnerMsg = Br.ReadString();
                        switch (OwnerMsg)
                        {
                            case "watch,accept":

                                goplay = new Thread(play);
                                Close();
                                goplay.Start();


                                break;
                            case "watch,reject":
                                MessageBox.Show("refused");
                                break;

                        }
                        flag = false;
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("Server is disconnected,close and try again");
            }

        }
    }
}
