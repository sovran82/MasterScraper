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
    class Program
    {

        static void Main()
        {
            var scraper = new GeetabitanMidiScraper(); //new PagalWorldScraper();
            scraper.Run();
        }

        class Scraper
        {
            public static string DownLoadFolder { get; set; }
            public bool ScraperInit(string downloadFolder)
            {
                DownLoadFolder = downloadFolder;
                return initialize();
            }
            public static string ErrorMessage { get; set; }
            private bool initialize()
            {
                //create download folder if not exists:
                if (!Directory.Exists(DownLoadFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(DownLoadFolder);
                        return true;
                    }
                    catch
                    {
                        ErrorMessage = "Diretory Coudn't be created: " + DownLoadFolder;
                        return false;
                    }
                }
                return false;
            }

            public static string GetHtmlText(string url)
            {
                string responseText = null;
                WebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                try
                {
                    using (WebResponse response = webRequest.GetResponse())
                    {
                        using (Stream dataStream = response.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(dataStream))
                            {
                                responseText = reader.ReadToEnd();
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    var exception = ex.Status;
                    if (ex.Status != WebExceptionStatus.ConnectFailure)
                    {
                        // return null;

                    }
                    Console.WriteLine(exception);
                }
                return responseText;
            }

            public static bool SaveFile(string url)
            {
                using (WebClient myWebClient = new WebClient())
                {
                    try
                    {
                        myWebClient.DownloadFile(url,  DownLoadFolder + "\\" + Path.GetFileName(url));
                        return true;
                    }
                    catch(Exception ex)
                    {
                        ErrorMessage = ex.Message;
                        return false;
                    }
                    
                }
                return false;
            }


            public List<string> GetDynamicUrls(string Url, string replacableString, List<string> replaceStrings)
            {
                var urls = new List<string>();
                foreach (var item in replaceStrings)
                {
                    urls.Add(Url.Replace(replacableString, item));
                }
                return urls;
            }

            public static List<Hap.HtmlNode> GetTagsWithClass(Hap.HtmlDocument htmlDocument, string html, List<string> @class)
            {
                // LoadHtml(html);           
                var result = htmlDocument.DocumentNode.Descendants()
                    .Where(x => x.Attributes.Contains("class") && @class.Contains(x.Attributes["class"].Value)).ToList();
                return result;
            }


            // var findclasses = _doc.DocumentNode
            //.Descendants( "div" )
            //.Where(d => 
            //    d.Attributes.Contains("class")
            //    &&
            //    d.Attributes["class"].Value.Contains("float")
            //);

        }


        class GeetabitanMidiScraper
        {
            static string downloadFolder = "Geetabitan.com/MidiFiles";
            public Scraper scraper = new Scraper();

            public void Run()
            {
                scraper.ScraperInit(downloadFolder);
                start();
            }

            private void start()
            {

                var urls = scraper.GetDynamicUrls("http://www.geetabitan.com/lyrics/list-index/staff-notation/available-snm-{Alphabet}.html",
                     "{Alphabet}", "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray().Select(x => x.ToString()).ToList()).ToArray();

                List<string> midiPageUrls = new List<string>();

                for (int i = 0; i < urls.Length; i++)
                {
                    Debug.WriteLine("visiting: " + urls[i]);

                    var htmlText = Scraper.GetHtmlText(urls[i]);
                    if (string.IsNullOrWhiteSpace(htmlText)) continue;

                    Hap.HtmlDocument doc = new Hap.HtmlDocument();
                    doc.LoadHtml(htmlText);

                    var lyricsUrls = doc.DocumentNode.Descendants("ul")
                        .Where(d => d.Attributes.Contains("class")&&d.Attributes["class"].Value.Contains("nav-pills-list"))
                        .Take(1).ToList()[0].Descendants("a").Select(a=>a.Attributes["href"]).Select(a=>a.Value).Select(a=> "http://www.geetabitan.com" + a.ToString())
                        .ToList()  ;

                    
                    midiPageUrls.AddRange( lyricsUrls);

                }

                List<string> midiUrls = new List<string>();
                for (int i = 0; i < midiPageUrls.Count; i++)
                {
                    Debug.WriteLine("visiting: " + midiPageUrls[i]);


                    var htmlText = Scraper.GetHtmlText(midiPageUrls[i]);
                    if (string.IsNullOrWhiteSpace(htmlText)) continue;

                    Hap.HtmlDocument doc = new Hap.HtmlDocument();
                    doc.LoadHtml(htmlText);

                    var midiUrl = doc.DocumentNode.Descendants("a").Where(a => a.InnerText.ToLower().Contains("download midi"))
                        .Take(1).Select(a => a.Attributes["href"]).Select(a=>a.Value).Select(a=> "http://www.geetabitan.com/lyrics" + a.ToString().Substring(2)).ToList()[0];
                    midiUrls.Add(midiUrl);
                }

                foreach (var url in midiUrls)
                {
                    Debug.WriteLine("downloading: " + url);

                    Scraper.SaveFile( url  );
                }

            }




          

            //public LinkPage getLinkPage(string linkPageUrl)
            //{

            //    LinkPage parentLinkPage = new LinkPage() { Url = linkPageUrl };
            //    LinkPage currentLinkPage = parentLinkPage;

            //    while (currentLinkPage != parentLinkPage && currentLinkPage.IsDeadEnd != true)
            //    {
            //        List<string> childLinks = getChildLinks(currentLinkPage.Url);
            //        currentLinkPage.ChildLinkPages = new List<LinkPage>(childLinks.Count);
            //        for (int i = 0; i < childLinks.Count; i++)
            //        {
            //            currentLinkPage.ChildLinkPages[i].Url = childLinks[i];
            //        }

            //        if (childLinks.Count == 0)
            //        {
            //            currentLinkPage.IsDeadEnd = true;
            //            currentLinkPage.MidiLink = getMidiLink(currentLinkPage.Url);
            //            currentLinkPage = currentLinkPage == parentLinkPage ? currentLinkPage : currentLinkPage.ParentLink;
            //            continue;
            //        }
            //        else if (childLinks.Count > 0)
            //        {
            //            for (int i = 0; i < childLinks.Count; i++)
            //            {
            //                string link = childLinks[i];
            //                LinkPage linkPage = new LinkPage() { Url = link };
            //                currentLinkPage = linkPage;
            //                getLinkPage(childLinks[i]);
            //            }
            //        }
            //    }

            //    return null;
            //}
           
            
            //public string getMidiLink(string pageUrl)
            //{
            //    string htmlText = GetHtmlText(pageUrl);
            //    string midiLink = null;
            //    List<string> childLinks = new List<string>();

            //    Hap.HtmlDocument htmDoc = new Hap.HtmlDocument();
            //    htmDoc.LoadHtml(htmlText);

            //    Hap.HtmlNodeCollection targetElements = htmDoc.DocumentNode.SelectNodes("//table[@data-test]");

            //    List<string> Links = new List<string>();


            //    return midiLink;
            //}
            //public List<string> getChildLinks(string url)
            //{

            //    string htmlText = GetHtmlText(url);

            //    List<string> childLinks = new List<string>();

            //    Hap.HtmlDocument htmDoc = new Hap.HtmlDocument();
            //    htmDoc.LoadHtml(htmlText);
            //    List<string> dateCaptureUrls = new List<string>();
            //    List<string> anchorTexts = new List<string>();
            //    List<string> anchorTextLangs = new List<string>();

            //    Hap.HtmlNodeCollection targetElements = htmDoc.DocumentNode.SelectNodes("//table[@data-test]");

            //    List<string> Links = new List<string>();

            //    for (int i = 0; i < targetElements.Count; i++)
            //    {
            //        string link = "";
            //        Links.Add(link);

            //    }
            //    return childLinks;
            //}



            public class LinkPage
            {
                public string Url;
                public bool IsMidi;
                private string _midiLink;
                public string MidiLink
                {
                    get { return _midiLink; }
                    set
                    {
                        if (value.Substring(value.Length - 4, 4) == ".mid")
                        {
                            _midiLink = value;
                            this.IsMidi = true;
                        }
                    }
                }
                public bool IsDeadEnd;
                public bool IsAllScraped;
                public LinkPage ParentLink;
                public List<LinkPage> ChildLinkPages;
                public bool NextPageExists { get; private set; }
                public string NextPageUrl { get; private set; }


            }

        }



        class PagalWorldScraper : Scraper
        {
            public const string MasterUrl = @"https://pagalworld.me/category/5362/Midi%20polyphonic%20ringtones.html";
            public string DownLoadFolder = "/MidiFiles";

            public PagalWorldScraper()
            {
                if (!createDirectory(DownLoadFolder))
                {
                    Debug.WriteLine("Couldn't create directory: " + DownLoadFolder);
                    return;
                }
            }
            private bool createDirectory(string directoryLocation)
            {

                return false;
            }
            private bool saveFile(string fileName, byte[] fileBytes)
            {

                return false;
            }
            public void Start()
            {
                //LinkPage masterLinkPage = new LinkPage() { Url = MasterUrl };

                LinkPage totalLinkPage = getLinkPage(MasterUrl);
            }

            public LinkPage getLinkPage(string linkPageUrl)
            {

                LinkPage parentLinkPage = new LinkPage() { Url = linkPageUrl };
                LinkPage currentLinkPage = parentLinkPage;

                while (currentLinkPage != parentLinkPage && currentLinkPage.IsDeadEnd != true)
                {
                    List<string> childLinks = getChildLinks(currentLinkPage.Url);
                    currentLinkPage.ChildLinkPages = new List<LinkPage>(childLinks.Count);
                    for (int i = 0; i < childLinks.Count; i++)
                    {
                        currentLinkPage.ChildLinkPages[i].Url = childLinks[i];
                    }

                    if (childLinks.Count == 0)
                    {
                        currentLinkPage.IsDeadEnd = true;
                        currentLinkPage.MidiLink = getMidiLink(currentLinkPage.Url);
                        currentLinkPage = currentLinkPage == parentLinkPage ? currentLinkPage : currentLinkPage.ParentLink;
                        continue;
                    }
                    else if (childLinks.Count > 0)
                    {
                        for (int i = 0; i < childLinks.Count; i++)
                        {
                            string link = childLinks[i];
                            LinkPage linkPage = new LinkPage() { Url = link };
                            currentLinkPage = linkPage;
                            getLinkPage(childLinks[i]);
                        }
                    }
                }

                return null;
            }
            public string getMidiLink(string pageUrl)
            {
                string htmlText = GetHtmlText(pageUrl);
                string midiLink = null;
                List<string> childLinks = new List<string>();

                Hap.HtmlDocument htmDoc = new Hap.HtmlDocument();
                htmDoc.LoadHtml(htmlText);

                Hap.HtmlNodeCollection targetElements = htmDoc.DocumentNode.SelectNodes("//table[@data-test]");

                List<string> Links = new List<string>();


                return midiLink;
            }
            public List<string> getChildLinks(string url)
            {

                string htmlText = GetHtmlText(url);

                List<string> childLinks = new List<string>();

                Hap.HtmlDocument htmDoc = new Hap.HtmlDocument();
                htmDoc.LoadHtml(htmlText);
                List<string> dateCaptureUrls = new List<string>();
                List<string> anchorTexts = new List<string>();
                List<string> anchorTextLangs = new List<string>();

                Hap.HtmlNodeCollection targetElements = htmDoc.DocumentNode.SelectNodes("//table[@data-test]");

                List<string> Links = new List<string>();

                for (int i = 0; i < targetElements.Count; i++)
                {
                    string link = "";
                    Links.Add(link);

                }
                return childLinks;
            }
            public class LinkPage
            {
                public string Url;
                public bool IsMidi;
                private string _midiLink;
                public string MidiLink
                {
                    get { return _midiLink; }
                    set
                    {
                        if (value.Substring(value.Length - 4, 4) == ".mid")
                        {
                            _midiLink = value;
                            this.IsMidi = true;
                        }
                    }
                }
                public bool IsDeadEnd;
                public bool IsAllScraped;
                public LinkPage ParentLink;
                public List<LinkPage> ChildLinkPages;
                public bool NextPageExists { get; private set; }
                public string NextPageUrl { get; private set; }


            }

        }


    }


}

