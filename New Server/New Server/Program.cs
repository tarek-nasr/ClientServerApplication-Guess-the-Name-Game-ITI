using System.Threading;
namespace New_Server
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to our server......");

            
            
            Server server = new Server();
            server.Start();
           
        }

        
    }
}
