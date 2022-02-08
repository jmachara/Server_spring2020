using Microsoft.Extensions.Logging;
using System;
using System.IO;
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
/// Custom logger that logs information to a file.
/// </summary>
namespace FileLogger
{
    class CustomFileLogger : ILogger
    {
        private FileStream logStream;
        private StreamWriter logWriter;
        /// <summary>
        /// Opens a File named Log_categoryName in the current directory and creates a streamwriter to write to the file. 
        /// </summary>
        /// <param name="categoryName"></param>
        public CustomFileLogger(String categoryName)
        {
            String currentDirectory = Environment.CurrentDirectory;
            logStream = File.Open(Path.Combine(currentDirectory, $"Log_{categoryName}.txt"), FileMode.Append);
            logWriter = new StreamWriter(logStream);
        }
        /// <summary>
        /// Closes the File
        /// </summary>
        internal void Dispose()
        {
            logWriter.Dispose();
            logWriter.Close();
        }

        /// <summary>
        /// This method is used to tell the user where in the code the logger is.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state">the state of the logger</param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Logs the events of the chat app to a file.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            string formatterMessage = formatter(state, exception);
            String logMessage = "-" + logLevel.ToString().Substring(0, 5) + "-" + formatterMessage;
            logWriter.WriteLine(logMessage.AddTimeAndThread());
        }

    }

}