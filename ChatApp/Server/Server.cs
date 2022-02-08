using FileLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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
/// Server code for the basic chat app. This code gets the data and distributes it to all of the clients.
/// </summary>
namespace CS3500
{
    
    /// <summary>
    /// A simple server for sending simple text messages to multiple clients
    /// </summary>
    public class Server
    {
        private int totalSentMessages = 0;
        private static bool open = true;
        /// <summary>
        /// A list of all clients currently connected
        /// </summary>
        private List<Socket> clients = new List<Socket>();


        private ILogger logger;
        private byte[] messageBuffer = new byte[1024];

        public Server(ILogger logger)
        {
            this.logger = logger;
        }
        public static void Main(string[] args)
        {
            ServiceCollection services = new ServiceCollection();

            using (CustomFileLogProvider provider = new CustomFileLogProvider())
            {
                services.AddLogging(configure =>
                {
                    configure.AddConsole();
                    configure.AddProvider(provider);
                    configure.SetMinimumLevel(LogLevel.Debug);
                });


                using (ServiceProvider serviceProvider = services.BuildServiceProvider())
                {
                    ILogger<Server> logger = serviceProvider.GetRequiredService<ILogger<Server>>();

                    Server server = new Server(logger);
                    server.StartServer();
                    while (open)
                    {
                       
                    }
                    Console.WriteLine("Goodbye, press enter to close");
                    Console.ReadLine();
                }
            }
        }


        /// <summary>
        /// Start accepting Tcp socket connections from clients
        /// </summary>
        public void StartServer()
        {
            int port = 11000;

            TcpListener listener = new TcpListener(IPAddress.Any, port);
            lock (logger)
            {
                logger.LogInformation("Server Running");
            }


            listener.Start();

            // This begins an "event loop".
            // ConnectionRequested will be invoked when the first connection arrives.
            listener.BeginAcceptSocket(ConnectionRequested, listener);
            // waits for the user to type messages, then sends them
            ClientListenerLoop();
            
        }

        /// <summary>
        /// A callback for invoking when a socket connection is accepted
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectionRequested(IAsyncResult ar)
        {
            TcpListener listener = (TcpListener)ar.AsyncState;
            Socket newClient = listener.EndAcceptSocket(ar);
            // Get the socket
            lock (clients)
            {
                clients.Add(newClient);
                if (clients.Count % 100 == 0)
                    logger.LogInformation($"{clients.Count} Clients connected");
            }
            newClient.BeginReceive(messageBuffer, 0, messageBuffer.Length, SocketFlags.None, OnReceive, newClient);
            // continue an event-loop that will allow more clients to connect
            listener.BeginAcceptSocket(ConnectionRequested, listener);
        }

        /// <summary>
        /// Recieve callback function, sends the message to all avaliable clients
        /// </summary>
        /// <param name="ar"></param>
        private void OnReceive(IAsyncResult ar)
        {
            List<Socket> toRemove = new List<Socket>();
            Socket listener = (Socket)ar.AsyncState;
            try
            {
                int numBytes = listener.EndReceive(ar);

                lock (clients)
                {
                    foreach (Socket s in clients)
                    {
                        try
                        {
                            s.BeginSend(messageBuffer, 0, numBytes, SocketFlags.None, SendCallback, s);
                        }
                        catch (Exception) // Begin Send fails if client is closed
                        {
                            toRemove.Add(s);
                        }
                    }
                    foreach (Socket socket in toRemove)
                    {
                        //remove clients that disconnected when trying to send to them
                        clients.Remove(socket);
                        logger.LogInformation("removed a client");
                    }
                }
                totalSentMessages++;
                // Continue the "event loop" and receive more data
                listener.BeginReceive(messageBuffer, 0, messageBuffer.Length, SocketFlags.None, OnReceive, listener);
            }
            catch (Exception)
            {
                //removes a client when they disconnect
                lock (clients)
                {
                    clients.Remove(listener);
                }
                lock (logger)
                {
                    logger.LogInformation("Client Disconnected");
                }

            }

        }



        /// <summary>
        /// Continuously ask the user for a message to send to the client
        /// </summary>
        private void ClientListenerLoop()
        {
            while (true)
            {
                //The server is just for transfering messages to clients, allows the server to see how many people 
                //are connected and how many messages have been sent.
                Console.WriteLine("The Server is Up and running, Type 'Close' to close the server or 'Info' to see information about the server");
                string ServerMessage = Console.ReadLine();
                if (ServerMessage.Equals("Close"))
                {
                    open = false;
                    break;
                }
                else if (ServerMessage.Equals("Info"))
                {
                    Console.WriteLine(clients.Count.ToString() + $" Clients connected and {totalSentMessages} messages sent");
                }
            }

        }
        /// <summary>
        /// A callback invoked when a send operation completes
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallback(IAsyncResult ar)
        {
            // Nothing much to do here, just conclude the send operation so the socket is happy.
            // 
            //   This code could be useful to update the state of a program once you know a client
            //   has received some information, for example, if you had a counter of successfully sent 
            //   messages you would increment it here.
            //
            Socket client = (Socket)ar.AsyncState;
            long send_length = client.EndSend(ar);
            
            if(totalSentMessages%1000 == 0)
                logger.LogInformation($"{totalSentMessages} messages sent");
        }

    }
}
