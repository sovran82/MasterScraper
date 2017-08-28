using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hap = HtmlAgilityPack;

namespace MasterScraper
{
    /// <remarks>
    /// Created by Ujjal Das
    /// </remarks> 
    /// <summary>
    /// methods and functions are self-documented 
    /// </summary>
    class Program
    {
        static void Main()
        {
            string downloadFolder = "pagalworld.me/MidiFiles";
            PagalWorldScraper pagalWorldScraper = new PagalWorldScraper(downloadFolder, new OutputWindowLogger());
            pagalWorldScraper.Run();
        }
         
    }


}

