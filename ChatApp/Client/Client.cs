using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using FileLogger;
using System.Threading;
using Client;
/// <summary> 
/// Author:    Jack Machara 
/// Partner:   [Partner Name or None] 
/// Date:      03/27/20 
/// Course:    CS 3500, University of Utah, School of Computing 
/// Copyright: CS 3500 and Jack Machara - This work may not be copied for use in Academic Coursework. 
/// 
/// I, Jack Machara, certify that I wrote this code from scratch and did not copy it in part or whole from  
/// another source.  All references used in the completion of the assignment are cited in my README file. 
/// 
/// File Contents 
/// This is the Client code of the basic chat app that sends messages to the server.
/// </summary>
namespace CS3500
{

    class SocketState
    {
        public Socket theSocket;
        public byte[] messageBuffer;
        public StringBuilder sb;

        public SocketState(Socket s)
        {
            theSocket = s;
            messageBuffer = new byte[1024];
            sb = new StringBuilder();
        }
    }

    public class Client
    {
        private static int port = 11000;
        private readonly ILogger logger;
        public string username;
        private Socket serverSocket;
        private bool firstLoop = true;
        private bool open = true;
        private bool tryingToConnect = true;
        private StringBuilder messageString;

        public Client(ILogger logger)
        {
            this.logger = logger;
            this.messageString = new StringBuilder();
        }

        public static void Main(string[] args)
        {

            ServiceCollection services = new ServiceCollection();

            using (CustomFileLogProvider provider = new CustomFileLogProvider())
            {
                services.AddLogging(configure =>
                {
                    //configure.AddConsole();
                    configure.AddProvider(provider);
                    configure.SetMinimumLevel(LogLevel.Debug);
                });

                using (ServiceProvider serviceProvider = services.BuildServiceProvider())
                {
                    ILogger<Client> logger = serviceProvider.GetRequiredService<ILogger<Client>>();

                    Client client = new Client(logger);


                    Console.WriteLine("enter server address:");
                    string serverAddr = Console.ReadLine();


                    try
                    {
                        client.ConnectToServer(serverAddr);
                    }
                    catch (Exception e)
                    {
                        //If connecting to the server fails 
                        client.tryingToConnect = false;
                        client.open = false;
                        lock (logger)
                        {
                            logger.LogError($"Error in connection {e}");
                        }
                    }

                    while (client.tryingToConnect)
                    {
                        Console.WriteLine("Connecting...");
                        Thread.Sleep(3000);
                    }
                    if (client.open)
                    {
                        client.ClientMessageListenerLoop();
                    }
                }
            }
        }
        
        /// <summary>
        /// Starts the connection process
        /// </summary>
        /// <param name="serverAddr"></param>
        private void ConnectToServer(string serverAddr)
        {

            // Parse the IP
            IPAddress addr = IPAddress.Parse(serverAddr);
            // Create a socket
            this.serverSocket = new Socket(addr.AddressFamily, SocketType.Stream,
              ProtocolType.Tcp);
            //// Create a socket
            //Socket s2 = new Socket(addr.AddressFamily, SocketType.Stream,
            //  ProtocolType.Tcp);

            SocketState ss1 = new SocketState(serverSocket);
            //SocketState ss2 = new SocketState(s2);

            // Connect
            ss1.theSocket.BeginConnect(addr, port, OnConnected, ss1);
            //ss2.theSocket.BeginConnect(addr, port, OnConnected, ss2);
        }

        /// <summary>
        /// Callback for when a connection is made (see line 62)
        /// Finalizes the connection, then starts a receive loop.
        /// </summary>
        /// <param name="ar"></param>
        private void OnConnected(IAsyncResult ar)
        {
            try
            {

                SocketState theServer = (SocketState)ar.AsyncState;

                // this does not end the connection! this simply acknowledges that we are at the _end_ of the start
                // of the connection phase!
                theServer.theSocket.EndConnect(ar);

                // Start a receive operation
                theServer.theSocket.BeginReceive(theServer.messageBuffer, 0, theServer.messageBuffer.Length, SocketFlags.None,
                    OnReceive, theServer);
                tryingToConnect = false;
            }
            catch (Exception e)
            {
                lock (logger)
                {
                    logger.LogError($"Couldn't recieve message due to : {e}");
                }
                open = false;
                tryingToConnect = false;

            }


        }
        
        /// <summary>
        /// Loop to send messages from the client
        /// </summary>
        private void ClientMessageListenerLoop()
        {
            if (firstLoop)
            {
                Console.WriteLine("Enter a Username between 1 and 10 characters");
                string potentialUsername = Console.ReadLine();
                if (potentialUsername.Length == 0 || potentialUsername.Length > 10)
                {
                    while (potentialUsername.Length == 0 || potentialUsername.Length > 10)
                    {
                        Console.WriteLine("Enter a valid Username");
                        potentialUsername = Console.ReadLine();
                    }
                }
                this.username = potentialUsername;
                firstLoop = false;
                Console.WriteLine($"Welcome to the chatServer {username}! Type a message ending with a period to send it to other users. Messages have to " +
                    $"less than 1000 characters long");
            }
            while (true)
            {
                messageString.Append(Console.ReadLine());
                //Catches spam messages
                if(messageString.Length > 1000)
                {
                    Console.WriteLine("Sorry, Messages Cannot exceed 1000 characters. Message will not be sent.\n" +
                        "remember to end every sentence with '.' (a period)");
                    messageString.Clear();
                }
                //Checks if the message can be sent, if not adds it to a string and waits for the '.' at the end of the sentence
                if (messageString.ToString()[messageString.Length - 1].Equals('.'))
                {
                    
                    byte[] messageBytes = Encoding.UTF8.GetBytes(messageString.ToString().AddUsername(username));
                    try
                    {
                        //sending the message loop
                        serverSocket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, OnSend, serverSocket);
                        messageString.Clear();
                    }
                    catch (Exception e)
                    {
                        lock (logger)
                        {
                            logger.LogDebug($"Message Failed to send due to {e}");
                        }
                        messageString.Clear();
                    }
                }
            }
        }
        /// <summary>
        /// Sending message call back function
        /// </summary>
        /// <param name="ar"></param>
        private void OnSend(IAsyncResult ar)
        {
            Socket client = (Socket)ar.AsyncState;
            long send_length = client.EndSend(ar);
            //used for basic stress test, but commented out for bigger ones.
            //lock (logger)
            //{
            //    logger.LogInformation($"Sent a message of length {send_length}");
            //}
            ClientMessageListenerLoop();
        }
        


        /// <summary>
        /// Callback for when a receive operation completes (see BeginReceive)
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                SocketState theServer = (SocketState)ar.AsyncState;
                int numBytes = theServer.theSocket.EndReceive(ar);

                string message = Encoding.UTF8.GetString(theServer.messageBuffer, 0, numBytes);

                theServer.sb.Append(message);

                ProcessMessages(theServer.sb);

                // Continue the "event loop" and receive more data
                theServer.theSocket.BeginReceive(theServer.messageBuffer, 0, theServer.messageBuffer.Length, SocketFlags.None,
                    OnReceive, theServer);
            }
            catch (Exception e)
            {
                logger.LogError($"Client couldnt recieve message due to : {e}");
                open = false;
            }
        }


        /// <summary>
        /// Look for complete messages (terminated by a '.'), 
        /// then print and remove them from the string builder.
        /// </summary>
        /// <param name="sb"></param>
        private void ProcessMessages(StringBuilder sb)
        {
            string totalData = sb.ToString();
            string[] parts = Regex.Split(totalData, @"(?<=[\.])");

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;

                // Ignore last message if incomplete
                if (p[p.Length - 1] != '.')
                    break;
                Console.WriteLine(p);
                sb.Remove(0, p.Length);

            }
        }



        /*
         * Methods used in the StressTest Class to test the functionality of the chat app
         */


        /// <summary>
        /// A copy of the main function to call in tests
        /// </summary>
        public void testMainFunction(string serverAddr, int botNum, string message)
        {

            Client client = new Client(logger);
            try
            {
                client.ConnectToServer(serverAddr);
            }
            catch (Exception e)
            {
                //If connecting to the server fails 
                client.tryingToConnect = false;
                client.open = false;
                lock (logger)
                {
                    logger.LogError($"Error in connection {e}");
                }
            }

            while (client.tryingToConnect)
            {
            }
            if (client.open)
            {
                client.sendMessageTestUseOnly(botNum, message);
            }
        }

        /// <summary>
        /// Callback method for sending a method in the stesss test class
        /// </summary>
        /// <param name="ar"></param>
        private void OnSendTest(IAsyncResult ar)
        {
            try
            {

                Socket client = (Socket)ar.AsyncState;
            }
            catch (Exception)
            {
                logger.LogError("Failed to send properly");
            }
        }

        /// <summary>
        /// Method to send messages in the stress test class.
        /// </summary>
        /// <param name="message"></param>
        private void sendMessageTestUseOnly(int botNumber, string message)
        {
            this.username = $"TestBot{botNumber}";
            if (message.Length > 1000)
            {
                logger.LogInformation($"user: {username} tried to send a message that is too long.");
                messageString.Clear();
            }

            byte[] messageBytes = Encoding.UTF8.GetBytes(message.AddUsername(username));
            try
            {
                serverSocket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, OnSendTest, serverSocket);
            }
            catch (Exception e)
            {
                lock (logger)
                {
                    logger.LogError($"Message Failed to send due to : {e}");
                }
            }
        }
    }
}

