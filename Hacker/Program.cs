using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net;
using Newtonsoft.Json;
using System.IO;

namespace Hacker
{
    //Simple struct for a message type
    struct Message
    {
        public string type;
        public string message;
    }

    class Program
    {

        static Client client = null; //Global Client declaration

        static void Main(string[] args)
        {
            //Argument Handling
            bool isHost = false;
            
            if (args.Length > 0)
            {
                if (args[0] == "-host")
                {
                    isHost = true;
                }
            }
            //Runs startup
            startup(isHost);

            Console.ReadKey(); //Keeps window open at end of program to view final results

        }

        static void startup(bool isHost)
        {
            Console.Title = "Hacker Chat";


            client = new Client(isHost); //Initializes the client
            //starts Threads for Sending and Recieving data.
            if (client != null)
            {
                Thread th = new Thread(new ThreadStart(receiveThread));
                Thread th2 = new Thread(new ThreadStart(sendThread));

                th.Start();
                th2.Start();

            }
        }
        //Recieves data and performs appropriate tasks.
        static void receiveThread()
        {
            while (true)
            {
                Message received = client.receiveData();

                if (received.type == "message")
                {
                    Console.WriteLine(client.nickname + ": " + received.message);
                }
            }
        }
        //Takes user input to send to other user
        static void sendThread()
        {
            string read = "";
            bool reset = false;
            
            
            while (true)
            {
                var a = Console.ReadKey();
                if (a.Key == ConsoleKey.Enter && !reset)
                {
                    client.sendMessage(read);
                    reset = true;
                    read = "";
                    Console.WriteLine();
                }
                else if (char.IsLetterOrDigit(a.KeyChar) || char.IsPunctuation(a.KeyChar))
                {
                    read += a.KeyChar;
                    reset = false;
                }
            }
        }
        //Changes message type based on User Input and ensures proper tags


    }
}