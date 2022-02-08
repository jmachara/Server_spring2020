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
/// Extension to a string to add the time and thread to the message
/// </summary>
namespace FileLogger
{
    static class AddTimeAndThreadExtension
    {
        public static string AddTimeAndThread(this String logMessage)
        {
            string newLogMessage = DateTime.Now.ToString() + " {" + Thread.CurrentThread.ManagedThreadId + "} - ";
            return newLogMessage + logMessage;
        }
    }
}
