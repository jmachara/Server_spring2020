using Microsoft.Extensions.Logging;
using System;
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
/// Provides the custom file logger
/// </summary>
namespace FileLogger
{
    public class CustomFileLogProvider : ILoggerProvider
    {
        CustomFileLogger logger;

        public ILogger CreateLogger(string categoryName)
        {
            this.logger = new CustomFileLogger(categoryName);
            return logger;
        }
        public void Dispose()
        {
           logger.Dispose();
        }
    }
}