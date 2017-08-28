using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hap = HtmlAgilityPack;

namespace MasterScraper
{

    class PagalWorldScraper : HapScraper 
    {
        public const string MasterUrl = @"https://pagalworld.me/category/5362/Midi%20polyphonic%20ringtones.html";
        static string _downloadFolderpath; 

        public PagalWorldScraper(string downloadFolder,IErrorLogger errorLogger ) :base(downloadFolder, errorLogger)
        {
            _downloadFolderpath = downloadFolder;
        }

        class Page
        {
            public string Title { get; set; }
            public string Url { get; set; }
        }

        class PagalWorldPage
        {
            public PagalWorldPage ParentPage;
            public string HtmlText { get; set; }
            public string Url { get; set; }
            public string DownloadFolderPath { get; private set; }
            public List<PagalWorldPage> ChildPages { get; private set; }
            public List<string> MidiUrls { get; private set; }
            public bool HasChildren { get; private set; }
            Hap.HtmlDocument document;

            HapScraper _hapScraper;

            public PagalWorldPage(string url, string downLoadFolderName, HapScraper hapScraper, PagalWorldPage parentPage = null)
            {
                _hapScraper = hapScraper;
                this.Url = url;
                if (string.IsNullOrWhiteSpace(downLoadFolderName))
                {
                    throw new NotImplementedException("download folder name is neccessary!");
                }

                if (parentPage != null)
                {
                    ParentPage = parentPage;
                    this.DownloadFolderPath = parentPage.DownloadFolderPath + "\\" + downLoadFolderName;
                }
                else
                {
                    this.DownloadFolderPath = downLoadFolderName;
                }

                if (document == null) document = new Hap.HtmlDocument();
                if (MidiUrls == null) MidiUrls = new List<string>();
                if (ChildPages == null) ChildPages = new List<PagalWorldPage>();
            }
            public void AddChildPage(PagalWorldPage childPage)
            {
                this.ChildPages.Add(childPage);
            }
            public void AddMidiUrl(string url)
            {
                this.MidiUrls.Add(url);
            }

            private PagalWorldPage getNextChildPage()
            {
                PagalWorldPage childPage = (PagalWorldPage)this.ChildPages.Select(p => p.HasChildren != true).Take(1);
                return childPage;
            }

            public void ExtractContents()
            {
                HtmlText = _hapScraper.GetHtmlText(this.Url);
                document.LoadHtml(HtmlText);

                extractAllPages();
                extractAllMidiUrls();
                
                saveMidis(MidiUrls);
            }
            private void saveMidis(List<string> midiUrls)
            {
                for (int i = 0; i < midiUrls.Count; i++)
                {
                   _hapScraper.SaveFile(midiUrls[i]);
                }
            }


            private void extractAllPages()
            {
                
                List<Page> pageUrls = new List<Page>();
                var divContainers = document.DocumentNode.Descendants("div").Where(div => div.Id == "genreMoreList").Take(1).ToList();
                if (divContainers.Count > 0)
                {
                    divContainers[0].Descendants("ul").Where(ul => ul.Attributes.Contains("class") && ul.Attributes["class"].Value.Contains("img-link"))
                       .Take(1).ToList()[0].Descendants("li").ToList().ForEach(li =>
                       {
                           li.Descendants("a").ToList().ForEach(a =>
                           {
                               Page page = new Page();
                               if (a.Attributes.Contains("href"))
                               {
                                   page.Url = a.Attributes["href"].Value;
                                   page.Title = a.InnerText;
                                   pageUrls.Add(page);
                               }
                           });
                       });

                };
                 

                if (pageUrls.Count > 0)
                {
                    for (int i = 0; i < pageUrls.Count; i++)
                    {
                        PagalWorldPage page = new PagalWorldPage(pageUrls[i].Url, pageUrls[i].Title,_hapScraper, this);

                        //extract contents of child pages
                        page.ExtractContents();
                        this.ChildPages.Add(page);
                    }
                }
                else
                {
                    this.HasChildren = true;
                }
            }

            private void extractAllMidiUrls()
            {
                var midiDiv = document.DocumentNode.Descendants("div").Where(div => div.Attributes.Contains("class") && div.Attributes["class"].Value == "menu_row" && div.InnerText.Contains("[ Download File ]"))
                    .Take(1);

                if (midiDiv.Count() > 0)
                {

                    var midiUrl = midiDiv.ToList()[0].Descendants("a").SingleOrDefault().Attributes["href"].Value;
                    this.ParentPage.MidiUrls.Add(midiUrl);
                }
            }

        }

         public void Run()
        {
            PagalWorldPage pagalWorldPage = new PagalWorldPage(MasterUrl, "ExtractedMidis", this);
            pagalWorldPage.ExtractContents();

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
