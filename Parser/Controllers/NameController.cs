using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;

namespace Parser.Controllers
{
    public class NameController : ApiController
    {
        // GET api/name
        public string[] Get()
        {
            string allHtmlINeed = extractAllHtmlINeed();
            string[] name = extractName(allHtmlINeed);
            return name;
        }

        // GET api/name/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/name
        public void Post([FromBody]string value)
        {
        }

        // PUT api/name/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/name/5
        public void Delete(int id)
        {
        }

        static string extractAllHtmlINeed()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            WebRequest req = HttpWebRequest.Create("http://en.wikipedia.org/w/index.php?title=List_of_Grand_Slam_men%27s_singles_champions&action=edit");
            req.Method = "GET";

            string source;
            using (StreamReader reader = new StreamReader(req.GetResponse().GetResponseStream()))
            {
                source = reader.ReadToEnd();
            }
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(source);
            var textarea = doc.DocumentNode.Descendants("textarea").FirstOrDefault(x => x.Attributes["id"].Value == "wpTextbox1");

            string allTexareaHtml = textarea.InnerHtml;

            int startIndex = allTexareaHtml.IndexOf("1968''");
            int endIndex = allTexareaHtml.LastIndexOf("! Legend");
            int theLength = endIndex - startIndex;
            string allHtmlINeed = allTexareaHtml.Substring(startIndex, theLength);
            return allHtmlINeed;
        }

        static string[] extractName(string allHtmlINeed)
        {
            List<string> nameInList = new List<string>();

            string playerHtml;
            string strForEndIndex;
            int endIndex;

            string savePlease = allHtmlINeed;
            int startNameIndex;
            int endNameIndex;
            int length;
            string name;

            for (int j = 5; j < 50; j++)
            {
                allHtmlINeed = savePlease;
                strForEndIndex = j + "/" + j;
                for (int i = 0; i < 10; i++)
                {

                    endIndex = allHtmlINeed.IndexOf(strForEndIndex);
                    if (endIndex == -1)
                    {
                        break;
                    }
                    else
                    {
                        playerHtml = allHtmlINeed.Substring(0, endIndex);
                        startNameIndex = playerHtml.LastIndexOf("}}");
                        startNameIndex = startNameIndex + 3;
                        endNameIndex = playerHtml.LastIndexOf("&");
                        endNameIndex = endNameIndex - 1;
                        length = endNameIndex - startNameIndex;
                        name = playerHtml.Substring(startNameIndex, length);
                        nameInList.Add(name);
                        allHtmlINeed = allHtmlINeed.Substring(endIndex + 3);
                    }

                }
            }

            string[] nameInArray = new string[nameInList.Count];
            for (int i = 0; i < nameInArray.Length; i++)
            {
                if (i < nameInList.Count)
                {
                    nameInArray[i] = nameInList[i];
                }
            }
            return nameInArray;
        }
    }
}
