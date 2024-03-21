using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace login
{
    public partial class Play : Form

    {
        NetworkStream Stream;
        BinaryWriter Bw;
        BinaryReader Br;

        string trueName;
        string OwnerName;
        string OpponentName;
        string Category;

        string PlayerStatus;
        string RoomId;
        string Word;
        bool flag;
        bool flagListen;
        bool suddenclose;
        Thread GetMsgRequest;
        Thread PlayerListener;
        List<string> selected;

        StringBuilder HiddenWordBuilder = new StringBuilder();
        public Play(NetworkStream StreamCons, string RoomIdCons, string WordCons, string nameCons, string categoryCons, string OpponentNameCons, string o)
        {
            InitializeComponent();
            Stream = StreamCons;
            Bw = new BinaryWriter(Stream);
            Br = new BinaryReader(Stream);
            GetMsgRequest = new Thread(Request);
            PlayerListener = new Thread(listen);
            selected = new List<string>();
            RoomId = RoomIdCons;
            OwnerName = nameCons;
            lblOwner.Text = nameCons;
            OpponentName = OpponentNameCons;
            lblOpponent.Text = OpponentNameCons;
            Category = categoryCons;
            lblCat.Text = categoryCons;
            exit.Visible = false;


            Word = WordCons.ToLower();
            HiddenWordBuilder.Append(string.Empty.PadLeft(Word.Length, '-'));
            //lbl_word.Hide();

            lbl_word.Text = HiddenWordBuilder.ToString();
            //lbl_word.Text = Word;
            flag = true;
            flagListen = true;
            groupBox_Key.Enabled = false;

            if (o == "o")
            {
                trueName = nameCons;
                plone.Text = "Playing";
                pltwo.Text = "";
                PlayerListener.Start();
                groupBox_Key.Enabled = true;
                PlayerStatus = "owner";

            }
            else
            {
                if (OpponentName == "")
                {
                    GetMsgRequest.Start();
                    trueName = nameCons;
                    PlayerStatus = "owner";
                }
                else
                {
                    plone.Text = "Playing";
                    pltwo.Text = "";
                    trueName = OpponentName;
                    PlayerStatus = "opponent";
                    PlayerListener.Start();
                }
            }

            if (o == "w")
            {
                PlayerStatus = "watcher";
                exit.Visible = true;
            }
            //else
            //{
            //    exit.Visible=false;
            //}
            

        }

        private void Play_Load(object sender, EventArgs e)
        {
            if (PlayerStatus == "watcher")
            {
                Bw.Write($"r,watch,{RoomId},get");
            }
        }

        void Request()
        {
            try
            {
                string Msg;
                while (flag)
                {
                    if (Stream.DataAvailable)
                    {
                        Msg = Br.ReadString();
                        DialogResult Result = MessageBox.Show(Msg, "Request", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                        switch (Result)
                        {
                            case DialogResult.Yes:
                                //play
                                flag = false;
                                lblOpponent.Invoke(new Action(() =>
                                {
                                    lblOpponent.Text = Msg.Split(':')[0];
                                    OpponentName = Msg.Split(':')[0];
                                }));

                                lbl_word.Invoke(new Action(() =>
                                {
                                    groupBox_Key.Enabled = true;
                                    plone.Text = "Playing";
                                    pltwo.Text = "";
                                    lbl_word.Show();

                                }));

                                Bw.Write("r,join," + RoomId + ",ao");
                                PlayerListener.Start();
                                break;
                            case DialogResult.No:
                                Bw.Write("r,join," + RoomId + ",ro");
                                break;
                        }
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("server is disconnected,close and try again");
            }
        }
        void listen()
        {
            try
            {
                while (flagListen)
                {
                    if (Stream.DataAvailable)
                    {
                        string[] s = Br.ReadString().Split(',');
                        if (s[0] == "*")
                        {
                            Invoke(() =>
                            {
                                string tmp;
                                tmp = plone.Text;
                                plone.Text = pltwo.Text;
                                pltwo.Text = tmp;
                            });
                            if (s.Length > 1 && s[1] == "a")
                            {
                                Invoke(() => { GroupBoxReverse(1); });
                            }
                            else if (s.Length > 1 && s[1] == "n")
                            {
                                Invoke(() => { GroupBoxReverse(0); });
                            }
                        }
                        else if (s[0].Contains(":"))
                        {
                            string[] keys = s[0].Split(":");
                            int indexesLength = keys.Length - 1;
                            selected.Add(keys[0]);
                            for (int i = 1; i <= indexesLength; i++)
                            {
                                int j = int.Parse(keys[i]);
                                HiddenWordBuilder.Remove(j, 1);
                                HiddenWordBuilder.Insert(j, keys[0]);
                                Invoke(() => { lbl_word.Text = HiddenWordBuilder.ToString(); });
                            }
                        }
                        else if (s[0] == "win")
                        {
                            Invoke(() =>
                            {
                                DialogResult status = DialogResult.None;
                                status = MessageBox.Show("Winner Winner chicken dinner !!\n Do you want to play again ?!", "Congratulation", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                                switch (status)
                                {
                                    case DialogResult.Yes:
                                        Bw.Write("e," + RoomId + ",yp");
                                        break;
                                    case DialogResult.No:
                                        Bw.Write("e," + RoomId + ",np");
                                        flagListen = false;
                                        suddenclose = false;
                                        Thread goWelcome = new Thread(() => Application.Run(new form2(Stream, trueName)));

                                        this.Invoke(new Action(() => Close()));
                                        goWelcome.Start();
                                        break;
                                }
                            });
                        }
                        else if (s[0] == "lose")
                        {
                            Invoke(() =>
                            {
                                DialogResult status = DialogResult.None;
                                status = MessageBox.Show("Good luck next time !!\n Do you want to play again ?!", "Try again", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                                switch (status)
                                {
                                    case DialogResult.Yes:
                                        Bw.Write("e," + RoomId + ",yp");
                                        //MessageBox.Show("waiting for other player's response");
                                        break;

                                    case DialogResult.No:
                                        Bw.Write("e," + RoomId + ",np");
                                        flagListen = false;
                                        suddenclose = false;
                                        Thread goWelcome = new Thread(() => Application.Run(new form2(Stream, trueName)));

                                        this.Invoke(new Action(() => Close()));
                                        goWelcome.Start();
                                        break;
                                }
                            });
                        }
                        else if (s[0] == "start game")
                        {

                            this.Invoke(new Action(() =>
                            {
                                flagListen = false;
                                suddenclose = false;
                                Thread gotoPlay = new Thread(() => Application.Run(new Play(Stream, RoomId, s[1], s[2], Category, s[3], s[4])));

                                Close();
                                gotoPlay.Start();
                            }));

                        }
                        else if (s[0] == "l")
                        {
                            if (s[1] == "end game")
                            {
                                flagListen = false;
                                suddenclose = false;
                                Thread goWelcome = new Thread(() => Application.Run(new form2(Stream, trueName)));

                                this.Invoke(new Action(() => Close()));
                                goWelcome.Start();


                            }
                        }
                        else if (s[0] == "watch" && s[1]=="reject") {
                            if (s[2] == "end")
                            {
                                MessageBox.Show("game ended");
                            }

                            flagListen = false;
                            suddenclose = false;
                            Thread goWelcome = new Thread(() => Application.Run(new form2(Stream, s[2])));

                            this.Invoke(new Action(() => Close()));
                            goWelcome.Start();
                        }
                        else if (s[0]=="play new")
                        {
                            this.Invoke(new Action(() =>
                            {
                                flagListen = false;
                                suddenclose = false;
                                Thread gotoPlay = new Thread(() => Application.Run(new Play(Stream, RoomId, s[1],trueName, Category, "", "r")));

                                Close();
                                gotoPlay.Start();
                            }));

                        }
                        
                    }
                }
            }
            catch (IOException)
            {
                MessageBox.Show("server is disconnected,close and try again");
            }
        }
        //-----------------------------------------
        private void GroupBoxReverse(int flag)
        {
            if (flag == 1)
            {
                groupBox_Key.Enabled = true;
                foreach (Control btn in groupBox_Key.Controls)
                {
                    if (btn is Button)
                    {
                        Button button = (Button)btn;
                        string buttonText = button.Text;
                        if (selected.Contains(buttonText.ToLower()))
                            button.Enabled = false;

                    }
                }
            }
            else
            {
                groupBox_Key.Enabled = false;
            }
        }

        static List<int> FindLetterIndexes(string word, char letter)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] == letter)
                {
                    indexes.Add(i);
                }
            }
            return indexes;
        }

        private void btnA_Click(object sender, EventArgs e)
        {
            char letter = char.Parse(((Button)sender).Text.ToLower());

            ((Button)sender).Enabled = false;

            List<int> indexes = FindLetterIndexes(Word, letter);

            if (indexes.Count > 0)
            {

                Bw.Write($"g,{RoomId},{letter}:{string.Join(":", indexes)}");
                GroupBoxReverse(1);
                foreach (int i in indexes)
                {
                    HiddenWordBuilder.Remove(i, 1);
                    HiddenWordBuilder.Insert(i, letter);
                    lbl_word.Text = HiddenWordBuilder.ToString();
                }

            }
            else
            {
                Bw.Write($"g,{RoomId},*");
                string tmp;
                tmp = plone.Text;
                plone.Text = pltwo.Text;
                pltwo.Text = tmp;
                GroupBoxReverse(0);
            }
        }

        private void exit_Click(object sender, EventArgs e)
        {
            Bw.Write("r,watch," + RoomId + ",o");
           
 


        }
    }
}

