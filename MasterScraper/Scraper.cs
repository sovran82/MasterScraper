using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Hap = HtmlAgilityPack;

namespace MasterScraper
{

    abstract class HapScraper
    {
        private string _downLoadFolderPath;
        private IErrorLogger _errorLogger;
        private bool _isProperlyInitialized;

        public HapScraper(string downloadFolderPath, IErrorLogger errorLogger)
        {
            _downLoadFolderPath = downloadFolderPath;
            _errorLogger = errorLogger;
            _isProperlyInitialized = createDownloadFolder();
        }

        public bool IsProperlyInitilized()
        {
            return _isProperlyInitialized;
        }

        private bool createDownloadFolder()
        {
            if (!Directory.Exists(_downLoadFolderPath))
            {
                try
                {
                    Directory.CreateDirectory(_downLoadFolderPath);
                    return true;
                }
                catch
                {
                    string error = "Diretory Coudn't be created: " + _downLoadFolderPath;
                    _errorLogger.Log(error);
                    return false;
                }
            }
            return false;
        }

        public string GetHtmlText(string url)
        {
            string responseText = null;
            WebRequest webRequest = WebRequest.Create(url);
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
            catch (WebException exception)
            {
                _errorLogger.Log(exception.Message);
            }
            return responseText;
        }

        public bool SaveFile(string url, bool overwrite = false)
        {
            string filePath = _downLoadFolderPath + "\\" + WebUtility.UrlDecode(Path.GetFileName(url));

            if (overwrite == false)
            {
                if (File.Exists(filePath))
                {
                    string error = "file already exists: " + filePath;
                    _errorLogger.Log(error);
                    return true;
                };
            }

            using (WebClient myWebClient = new WebClient())
            {
                try
                {
                    _errorLogger.Log("downloading: " + url);
                    myWebClient.DownloadFile(url, filePath);
                    return true;
                }
                catch (Exception ex)
                {
                    _errorLogger.Log(ex.Message);
                    return false;
                }
            }
        }


        public static List<string> BuildDynamicUrlList(string Url, string replacableString, List<string> replaceStrings)
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

            var result = htmlDocument.DocumentNode.Descendants()
                .Where(x => x.Attributes.Contains("class") && @class.Contains(x.Attributes["class"].Value)).ToList();
            return result;
        }

    }


}
