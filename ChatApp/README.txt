Author:     Jack Machara
Partner:    None
Date:       03/23/20
Course:     CS 3500, University of Utah, School of Computing
GitHub ID:  jmachara
Repo:       https://github.com/uofu-cs3500-spring20/assignment-seven-logging-and-networking-isolation-inc
Commit #:   e17559defc9826620d0c3e844e09563869280f27
Assignment: Assignment #7 - Logging and Networking

Copyright:  CS 3500 and Jack Machara - This work may not be copied for use in Academic Coursework.

1. Comments to Evaluators:
        I thought for this assignment we had to have our clients be able to send messages, so i implemented that, and because I wanted this
    program to be like a basic chat app, i took away the sending message feature of the server and just had it relay messages, though adding 
    this back in wouldn't be too difficult. The clients message wont send until there is a '.' at the end of the sentence.
        I have the server display how many clients are connected and the total number of messages sent out. Through stress testing all of the 
    errors I came across were due to the stress test running async and closing the client, then trying to log which threw an error. I fixed this 
    by adding a copy of the main method to call in the tests which allowed me to connect 1000 users and each of them send a message when connecting.
        I think because I misinterpretted the assignment and implemented the clients sending messages it helped me understand how networking 
   connections better, but it was a lot more work than i wanted. The only reason I didn't find a lot of issues when stress testing is I put 
   locks on things before implementing the stress test class and walked myself through what the code was doing over and over until I understood 
   what was happening. 
        I decided to catch messages longer than 1000 characters because In chat servers its annoying when people spam huge messages so I decided this is 
  a simple way to fix that issue. Also catch usernames > 10 characters long.One problem i couldnt figure out is when doing extrememly long messages
  in the stresstest, at the end all of the messages get sent to the console, but I think this is because all of the threads are coming back to the main
  one and printing what they tried to send to the console, because it doesn't show up in the file log or when i send long messages with using powershell. 
        I'm sorry I didn't complete the assignment like we were expected to do it because I thought we needed to add the client end of sending messages
  to the app like windows messanger does, but when i saw that we didn't have to do that I was already basically done. 
   
   Implementation:
       Users type in a username which will precede every message sent by them.
       Logging file is made in bin/Debug/netcoreapps3.1, but I couldnt find a way to fix this through a solid hour of googling
       Server is just a middle man for clients. You can type 'Info' into the server to see details about number of clients and 
            how many messages have been sent or type 'Close' to close the server peacefully
       Users are limited to 1000 characters per message to prevent spamming.

    StressTests:
        I had 3 stress tests and i believe that they showed me that my application was running how i wanted it. The first one just had 2 clients
        join and they each sent a message.Tests 2 and 3 had any number of clients join (i did up to 1000) and send a message when they connected, 
        but in test 2 they sent a short message and in test 3 they sent a long message, which was caught by the less than 1000 character filter to
        prevent spam.

    Branching:
       None - I worked alone and didnt see a need of a branch during coding because i didn't change too much at a time.
    Fixes: 
        Adding locks to the logger so multiple threads can't access it at the same time as well as locking the client list in the server.
        Added the sending message function to the clients and made the server a message distributor (I thought we needed to do this for the assignment
            as stated above).
        Passing the listener in as a parameter instead of having a global variable
        Replaces the Thread.Sleep(10000) with a while loop that keeps the program open until user of the server types 'Close' into it, then prints a goodbye
            message and closes the window.
       
       
            


2. Assignment Specific Topics
            Hours Estimated/Worked         Assignment                                   Note
                     8    /   17    - Assignment 7 - Logging and Networking             Spend a lot of time trying to figure the logger out before professor
                                                                                        De St Germain showed us what he was looking for in lecture thursday.

3. Consulted Peers:

N/A

4. References:

    1. PluralSight - Getting Started with Dependency Injection in .NET
    2. Using The ILogger BeginScope In ASP.NET Core - https://dotnetcoretutorials.com/2018/04/12/using-the-ilogger-beginscope-in-asp-net-core/
    3. Append lines to a file using a StreamWriter - https://stackoverflow.com/questions/7306214/append-lines-to-a-file-using-a-streamwriter
    4. How to write to a text file - https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/file-system/how-to-write-to-a-text-file
    5. Environment.CurrentDirectory Property - https://docs.microsoft.com/en-us/dotnet/api/system.environment.currentdirectory?view=netframework-4.8