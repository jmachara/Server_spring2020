using FileLogger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
/// Stress test for the basic chat app.
/// </summary>
namespace CS3500
{
    class StressTests
    {
        static void Main(string[] args)
        {
            //gets local/remote and the test number
            String test = Console.ReadLine();
            String testNum = Console.ReadLine();
            int testId;

            if (test == "")
            {
                Console.WriteLine("Usage: [Server] | [Client] | [local #] | [remote #]");
                return;
            }

            if (test.Equals("Server"))
            {
                Server.Main(args);
                return;
            }
            else if (test.Equals("Client"))
            {
                Client.Main(args);
                return;
            }
            else if (!(test.Equals("local") || test.Equals("remote")) ||
                !Int32.TryParse(testNum, out testId))
            {
                Console.WriteLine("Failed");
                return;
            }
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
                    ILogger logger = serviceProvider.GetRequiredService<ILogger<StressTests>>();

                    if (testId == 1)
                    {
                        stressTest1(test.Equals("local"), logger);
                    }
                    else if (testId == 2)
                    {
                        stressTest2(test.Equals("local"), logger);
                    }
                    else if (testId == 3)
                    {
                        stressTest3(test.Equals("local"), logger);
                    }

                }
            }
        }
        /// <summary>
        /// Adds 2 clients and they send a basic message each
        /// </summary>
        /// <param name="local"></param>
        /// <param name="logger"></param>
        private static void stressTest1(bool local, ILogger logger)
        {
            if (!local)
            {
                logger.LogError("test is only for local setup");
            }

            Server server = new Server(logger);
            Thread serverThread = new Thread(server.StartServer);
            serverThread.Start();
            Thread.Sleep(1000);

            Client client = new Client(logger);
            Client client2 = new Client(logger);
            client.testMainFunction("127.0.0.1", 1, "Hi World.");
            client2.testMainFunction("127.0.0.1", 1, "Hi World.");
            Thread.Sleep(1000);


        }
        /// <summary>
        /// Adds as many clients as needed and they send a short message upon connection
        /// </summary>
        /// <param name="local"></param>
        /// <param name="logger"></param>
        private static void stressTest2(bool local, ILogger logger)
        {
            if (!local)
            {
                logger.LogError("test is only for local setup");
            }
            int numClients = 1000;
            Server server = new Server(logger);
            Thread serverThread = new Thread(server.StartServer);
            serverThread.Start();
            Thread.Sleep(1000);
            Client[] clientArray = new Client[numClients];
            for (int i = 0; i < numClients; i++)
            {

                Client client = new Client(logger);
                clientArray[i] = client;
                client.testMainFunction("127.0.0.1", i, "Test Message");
                Thread.Sleep(10);
            }
            logger.LogInformation("All Messages Sent");
        }
        /// <summary>
        /// Adds as many clients as needed and they send a long message upon connection
        /// </summary>
        /// <param name="local"></param>
        /// <param name="logger"></param>
        private static void stressTest3(bool local, ILogger logger)
        {
            if (!local)
            {
                logger.LogError("test is only for local setup");
            }
            int numClients = 10;
            Server server = new Server(logger);
            Thread serverThread = new Thread(server.StartServer);
            serverThread.Start();
            Thread.Sleep(1000);
            Client[] clientArray = new Client[numClients];
            for (int i = 0; i < numClients; i++)
            {
                Client client = new Client(logger);
                clientArray[i] = client;
                string inputMessage = longMessageGenerator();
                client.testMainFunction("127.0.0.1", i, inputMessage);
            }
            logger.LogInformation("All Messages Sent");
        }

        /// <summary>
        /// Generates a long message to send in the stress tests
        /// </summary>
        /// <returns></returns>
        public static string longMessageGenerator()
        {
            StringBuilder outputString = new StringBuilder();
            for(int i = 0; i < 1000; i++)
            {
                outputString.Append("aaaaaaaaaa");
            }
            return outputString.ToString() + ".";
        }
    }
}
