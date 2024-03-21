using System.Net.Sockets;
using System.Xml.Linq;

namespace login
{
    public partial class Form1 : Form
    {
        TcpClient client;
        NetworkStream Stream;
        string Name;
        Thread GoWelcome;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void login_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                TcpClient client = new TcpClient("127.0.0.1", 12345);
                Stream = client.GetStream();
                Name = textBox1.Text;
                new BinaryWriter(Stream).Write(Name);
                GoWelcome = new Thread(openWelcome);
                Close();
                GoWelcome.Start();

            }
            else
            {
                MessageBox.Show("UserName is Required!");
            }
        }
        void openWelcome()
        {
            Application.Run(new form2(Stream, Name));
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            //Application.ExitThread();
            //Environment.Exit(Environment.ExitCode);

        }
    }
}
