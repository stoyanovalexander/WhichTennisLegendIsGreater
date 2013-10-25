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
    public class YearOfTitleController : ApiController
    {
        // GET api/yearoftitle
        public int[,] Get()
        {
            HttpClient client = new HttpClient();

            string allHtmlINeed = extractAllHtmlINeed();
            string[] allInfoByYear = ExtractAllInfoByYear(allHtmlINeed);
            string[] name = extractName(allHtmlINeed);
            int[] titlesCount = extractTitlesCount(allHtmlINeed);
            List<string> yearAndNameStr = new List<string>();
            yearAndNameStr = extractYearAndNameStr(allInfoByYear, name);
            List<int> sortedTitles = extractSortedTitles(name, titlesCount, yearAndNameStr, name.Length);
            int max = findMaxCountOfTitles(titlesCount);
            int[,] eachPlayerTitles = extractPlayerTitles(name, titlesCount, sortedTitles, max);

            return eachPlayerTitles;
        }

        // GET api/yearoftitle/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/yearoftitle
        public void Post([FromBody]string value)
        {
        }

        // PUT api/yearoftitle/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/yearoftitle/5
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

        static string[] ExtractAllInfoByYear(string allHtmlINeed)
        {
            int lastYearInTheData = 0;
            // 9999 is with idea to be a number away bigger than 2013
            for (int i = 1968; i < 9999; i++)
            {
                lastYearInTheData = allHtmlINeed.IndexOf(i.ToString());
                if (lastYearInTheData == -1)
                {
                    lastYearInTheData = i - 1;
                    break;
                }
            }

            int countOfYearsInTheData = (lastYearInTheData - 1968) + 1;
            string[] openEraEachYaerAllData = new string[countOfYearsInTheData];
            int startData = 0;
            int endData = 0;
            int lenghtOfData = 0;

            allHtmlINeed = allHtmlINeed + (lastYearInTheData + 1);

            for (long i = 1968, j = 0, k = 1969; i <= lastYearInTheData; i++, j++, k++)
            {
                startData = allHtmlINeed.IndexOf(i.ToString());
                endData = allHtmlINeed.IndexOf(k.ToString());
                lenghtOfData = endData - startData;
                openEraEachYaerAllData[j] = allHtmlINeed.Substring(startData, lenghtOfData);
                //Console.WriteLine();
                //Console.WriteLine();
                //Console.WriteLine(openEraEachYaerAllData[j]);
            }
            return openEraEachYaerAllData;
        }

        static List<string> extractYearAndNameStr(string[] allInfoByYear, string[] name)
        {
            List<string> yearAndName = new List<string>();
            string strYearAndName;
            int startIndex = 0;
            int indexOfPlayer = 0;
            for (int i = 0; i < allInfoByYear.Length; i++)
            {
                for (int j = 0; j < name.Length; j++)
                {
                    startIndex = 0;
                    // 4 is the max count of grand slam title which player can won for year.
                    for (int k = 0; k < 4; k++)
                    {
                        indexOfPlayer = allInfoByYear[i].IndexOf(name[j], startIndex);
                        if (indexOfPlayer != -1)
                        {
                            strYearAndName = name[j] + "," + (1968 + i);
                            yearAndName.Add(strYearAndName);
                            startIndex = indexOfPlayer + 2;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            //yearAndName.Sort();
            return yearAndName;
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

        static int[] extractTitlesCount(string allHtmlINeed)
        {
            List<int> theCountInList = new List<int>();
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
                        theCountInList.Add(j);
                        allHtmlINeed = allHtmlINeed.Substring(endIndex + 3);
                    }

                }
            }

            int[] theCountInArray = new int[theCountInList.Count];
            for (int i = 0; i < theCountInArray.Length; i++)
            {
                if (i == 3)
                {
                    theCountInArray[i] = 5;
                }
                else if (i == 6)
                {
                    theCountInArray[i] = 4;
                }
                else if (i == 10)
                {
                    theCountInArray[i] = 5;
                }
                else if (i < theCountInList.Count)
                {
                    theCountInArray[i] = theCountInList[i];
                }
            }

            return theCountInArray;
        }

        // Pull all years of the titles in a list after that I will use titles count per player to find the years with title for each player.
        static List<int> extractSortedTitles(string[] name, int[] titlesCount, List<string> yearAndNameStr, int lenght)
        {
            List<int> sortedTitles = new List<int>();
            string nameStr;
            int indexOfComa = 0;
            int year;

            for (int i = 0; i < lenght; i++)
            {
                for (int j = 0; j < yearAndNameStr.Count(); j++)
                {
                    indexOfComa = yearAndNameStr[j].IndexOf(",");
                    nameStr = yearAndNameStr[j].Substring(0, indexOfComa);
                    if (nameStr == name[i])
                    {
                        year = int.Parse(yearAndNameStr[j].Substring(indexOfComa + 1));
                        sortedTitles.Add(year);
                    }
                }
            }
            return sortedTitles;
        }

        static int findMaxCountOfTitles(int[] titlesCount)
        {
            int max = 0;
            for (int i = 0; i < titlesCount.Length; i++)
            {
                if (titlesCount[i] > max)
                {
                    max = titlesCount[i];
                }
            }
            return max;
        }

        static int[,] extractPlayerTitles(string[] name, int[] titlesCount, List<int> sortedTitles, int max)
        {
            int[,] eachPlayerTitles = new int[name.Length, max + 1];
            for (int i = 0; i < name.Length; i++)
            {
                for (int j = 0; j < max + 1; j++)
                {
                    if (j < titlesCount[i])
                    {
                        eachPlayerTitles[i, j] = sortedTitles.First();
                        sortedTitles.RemoveAt(0);
                    }
                    else
                    {
                        eachPlayerTitles[i, j] = 0;
                    }
                }
            }
            return eachPlayerTitles;
        }

    }
}
