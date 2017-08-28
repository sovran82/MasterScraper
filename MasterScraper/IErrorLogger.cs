using System;
using System.Collections.Generic;
using System.Diagnostics; 

namespace MasterScraper
{
    interface IErrorLogger
    {
        void Log(string error);
    }

    class OutputWindowLogger : IErrorLogger
    {
        public void Log(string error)
        {
            Debug.WriteLine(error);
        }
    }

    class ConsoleLogger : IErrorLogger
    {
        public void Log(string error)
        {
            Console.WriteLine(error);
        }
    }
}
