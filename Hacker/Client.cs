using System;
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
        enum Encryption
        {
            NONE,
            ROT13
        }
    class Client
    {
        public NetworkStream stream { get; set; }
        public string nickname = "Other user";

        private TcpClient tcp = null;
        private Stream outputStream = null;
        private int port { get; set; }
        private string ip { get; set; }
        

        public Encryption encryptionType = Encryption.NONE;

        //Initializes Client with the default values.
        public Client(bool isHost = false, string inputAddress = "127.0.0.1", int inputPort = 6789)
        {

            outputStream = Console.OpenStandardOutput();
            ip = inputAddress;
            port = inputPort;
            if (isHost)
            {
                host();
            }
            else
            {
                client();
            }

            stream = tcp.GetStream();
            outputToStream(getCurrentTime() + "Connected to " + ip + ":" + port);


        }
        //Starts Connection as host
        private void host()
        {
            TcpListener host = new TcpListener(IPAddress.Parse(ip), port);
            host.Start();

            outputToStream(getCurrentTime() + "Listening on " + ip + ":" + port);
            tcp = host.AcceptTcpClient();

            outputToStream(getCurrentTime() + " " + DateTime.Now.ToShortTimeString() + ": User Connected");
        }
        //Starts connection as client
        private void client()
        {
            tcp = new TcpClient("localhost", port);
        }

        string getCurrentTime(string dateFormat = "MM/dd/yyyy h:mm:ss ")
        {
            return DateTime.Now.ToString(dateFormat);
        }

        static Message rot13(Message m)
        {
            if (m.type == "message")
            {
                string firstHalfCharacters  = "abcdefghijklmABCDEFGHIJKLM";
                string secondHalfCharacters = "nopqrstuvwxyzNOPQRSTUVWXYZ";

                char[] firstHalf = firstHalfCharacters.ToCharArray();
                char[] secondHalf = secondHalfCharacters.ToCharArray();

                string cipherMessage = "";

                foreach (char c in m.message)
                {

                    if (firstHalfCharacters.Contains(c))
                    {
                        int value = firstHalfCharacters.IndexOf(c);
                        cipherMessage += secondHalf[value];
                    }
                    else if (secondHalfCharacters.Contains(c))
                    {
                        int value = secondHalfCharacters.IndexOf(c);
                        cipherMessage += firstHalf[value];
                    }
                    else
                    {
                        cipherMessage += c;
                    }

                }

                m.message = cipherMessage;
            }
            return m;
        }

        public Message receiveData()
        {
            byte[] receieveData = new byte[100000];
            int bytes = stream.Read(receieveData, 0, receieveData.Length);

            Message msg = JsonConvert.DeserializeObject<Message>(Encoding.ASCII.GetString(receieveData, 0, bytes));

            if (encryptionType == Encryption.ROT13)
            {
                msg = rot13(msg);
            }

            if (msg.type == "nickChange")
            {
                nickname = msg.message;
            }
            return msg;
        }

        public void sendMessage(string messageInput)
        {
            Message tempMessage;

            byte[] sendData;

            tempMessage = scanInput(messageInput);

            if (encryptionType == Encryption.ROT13)
            {
                tempMessage = rot13(tempMessage);
            }

            sendData = System.Text.Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(tempMessage));
            stream.Write(sendData, 0, sendData.Length);
        }

        Message scanInput(string i)
        {
            Message m = new Message();

            if (i.StartsWith("/nick"))
            {
                m.type = "nickChange";
                m.message = i.Remove(0, 5).Trim();
            }
            else if (i.StartsWith("/crypt"))
            {
                string encryptionToUse = i.Remove(0, 6).Trim();
                
                if (encryptionToUse.ToUpper() == "ROT13")
                {
                    outputToStream(getCurrentTime() + "ROT13 cipher enabled");
                    encryptionType = Encryption.ROT13;
                }
                else if (encryptionToUse.ToUpper() == "OFF")
                {
                    outputToStream(getCurrentTime() + "rot13 cipher off");
                    encryptionType = Encryption.NONE;
                }
                m.type = "nickChange";
                m.message = nickname;
                
            }
            else
            {
                m.type = "message";
                m.message = i;
            }

            return m;

        }
        void outputToStream(string message)
        {
            byte[] dataMessage = Encoding.ASCII.GetBytes(message);
            int length = dataMessage.Length;

            outputStream.Write(dataMessage, 0, length);
            outputStream.Write(Encoding.ASCII.GetBytes("\n"), 0, Encoding.ASCII.GetBytes("\n").Length);
        }
    }
}