using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace login
{


    public partial class create : Form
    {
        NetworkStream Stream;
        BinaryReader Br;
        BinaryWriter Bw;
        string[] IdWord;
        string name;
        Thread GoPlay;
        bool flag;
        bool suddenClose;
        public create(NetworkStream streamCons, string nameCons)
        {
            InitializeComponent();
            Stream = streamCons;
            Bw = new BinaryWriter(Stream);
            Br = new BinaryReader(Stream);
            name = nameCons;
            suddenClose = true;
        }

        private void create_Load(object sender, EventArgs e)
        {

        }

        private void select_Click(object sender, EventArgs e)
        {
            try
            {
                //requst to create room
                Bw.Write("r,create,"+categoryselect.SelectedItem );
                flag = true;
                suddenClose = false;
                while (flag)
                {
                    if (Stream.DataAvailable)
                    {
                        IdWord = Br.ReadString().Split(',');
                        flag = false;
                    }
                }

                GoPlay = new Thread(openPlay);
                Close();
                GoPlay.Start();
            }
            catch (IOException)
            {
                MessageBox.Show("server is disconnected,close and try again");
            }

        }

        void openPlay()
        {
            Application.Run(new Play(Stream, IdWord[2], IdWord[3], name, categoryselect.SelectedItem.ToString(),"","r"));
        }
    }
    }

