using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace WhichTennisLegendIsBigger.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : WhichTennisLegendIsBigger.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _image = null;
        private String _imagePath = null;
        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._imagePath != null)
                {
                    this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
                }
                return this._image;
            }

            set
            {
                this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

  
    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        static int[] CountingTitles(int[,] grandSlamTitles)
        {
            int[] titlesCount = new int[grandSlamTitles.GetLength(0)];


            for (int i = 0; i < grandSlamTitles.GetLength(0); i++)
            {
                for (int j = 0; j < grandSlamTitles.GetLength(1); j++)
                {
                    if (grandSlamTitles[i, j] == 0)
                    {
                        break;
                    }
                    else
                    {
                        titlesCount[i]++;
                    }
                }
            }

            return titlesCount;
        }

        // Wiining years means the years between the first and the last title
        static int[] CalculatingWinningYears(int[,] grandSlamTitles)
        {
            int[] winningYears = new int[grandSlamTitles.GetLength(0)];
            for (int i = 0; i < grandSlamTitles.GetLength(0); i++)
            {
                for (int j = 0; j < grandSlamTitles.GetLength(1); j++)
                {
                    if ((grandSlamTitles[i, j] == 0))
                    {
                        winningYears[i] = grandSlamTitles[i, (j - 1)] - grandSlamTitles[i, 0];
                        break;
                    }
                }
            }
            return winningYears;
        }

        // Longest time between two consecative titles
        static int[] CalculatingLongestTime(int[,] grandSlamTitles, int[] titlesCount)
        {
            int[] longestTime = new int[grandSlamTitles.GetLength(0)];
            int timeBetween = 0;
            for (int i = 0; i < grandSlamTitles.GetLength(0); i++)
            {
                for (int j = 0; j < titlesCount[i] - 1; j++)
                {
                    timeBetween = grandSlamTitles[i, j + 1] - grandSlamTitles[i, j];
                    if (timeBetween > longestTime[i])
                    {
                        longestTime[i] = timeBetween;
                    }
                }
            }
            return longestTime;
        }

        static int[] FindFirstYearWithTitle(int[,] grandSlamTitles)
        {
            int[] firstTitle = new int[grandSlamTitles.GetLength(0)];
            for (int i = 0; i < grandSlamTitles.GetLength(0); i++)
            {
                firstTitle[i] = grandSlamTitles[i, 0];
            }

            return firstTitle;
        }

        static int[] FindLastYearWithTitle(int[,] grandSlamTitles)
        {
            int[] lastTitle = new int[grandSlamTitles.GetLength(0)];

            for (int i = 0; i < grandSlamTitles.GetLength(0); i++)
            {
                for (int j = 0; j < grandSlamTitles.GetLength(1); j++)
                {
                    if (grandSlamTitles[i, j] == 0)
                    {
                        lastTitle[i] = grandSlamTitles[i, (j - 1)];
                        break;
                    }
                }
            }

            return lastTitle;
        }

        static double[] CalculeteTitlesValueByOponents(int[,] grandSlamTitles, int[] firstTitle,
            int[] lastTitle, int[] titlesCount)
        {
            double[] sumOfTitlesValueByOponents = new double[firstTitle.Length];
            for (int i = 0; i < firstTitle.Length; i++)
            {
                for (int j = 0; j < firstTitle.Length; j++)
                {
                    for (int k = 0; k < titlesCount[i]; k++)
                    {
                        if ((grandSlamTitles[i, k] >= firstTitle[j]) && (grandSlamTitles[i, k] <= lastTitle[j]) && (titlesCount[j] >= 12))
                        {
                            sumOfTitlesValueByOponents[i] = sumOfTitlesValueByOponents[i] + 6;
                        }

                        else if ((grandSlamTitles[i, k] >= firstTitle[j]) && (grandSlamTitles[i, k] <= lastTitle[j]) && (titlesCount[j] >= 7))
                        {
                            sumOfTitlesValueByOponents[i] = sumOfTitlesValueByOponents[i] + 3;
                        }

                        //else if ((grandSlamTitles[i, k] >= firstTitle[j]) && (grandSlamTitles[i, k] <= lastTitle[j]) && (compensatedResult[j] >= 10))
                        //{
                        //    sumOfTitlesValueByOponents[i] = sumOfTitlesValueByOponents[i] + 0.25;
                        //}
                    }
                }
                sumOfTitlesValueByOponents[i] = sumOfTitlesValueByOponents[i] / titlesCount[i];
            }
            return sumOfTitlesValueByOponents;
        }

        static double[] CalculatingFinalResultBase(int[] titlesCount, double[] titlesValueByOponent)
        {
            double[] finalResultBase = new double[titlesCount.Length];

            for (int i = 0; i < titlesCount.Length; i++)
            {
                finalResultBase[i] = titlesCount[i] + titlesValueByOponent[i];
            }

            return finalResultBase;
        }

        static string[,] CompareByTitlesCount(int[] titleCount, string[] name)
        {
            string[,] comparationResults = new string[titleCount.Length, titleCount.Length];

            for (int i = 0; i < titleCount.Length; i++)
            {
                for (int j = 0; j < titleCount.Length; j++)
                {
                    if (i == j)
                    {
                        comparationResults[i, j] = "Unusable";
                    }
                    else
                    {
                        if (titleCount[i] > titleCount[j])
                        {
                            comparationResults[i, j] = "ROUND 1: " +
                                " By the count of their grand slam titles the result is: \n" +
                                name[i] + " : " + titleCount[i] + "     " + name[j] + " : " + titleCount[j] + "\n" +
                                "      Wins:" + name[i].ToUpper() + " !!!" + "\n\n";
                        }

                        if (titleCount[i] < titleCount[j])
                        {
                            comparationResults[i, j] = "ROUND 1: " +
                                " By the count of their grand slam titles the result is: \n" +
                                name[i] + " : " + titleCount[i] + "     " + name[j] + " : " + titleCount[j] + "\n" +
                                "      Wins:" + name[j].ToUpper() + " !!!" + "\n\n";
                        }

                        if (titleCount[i] == titleCount[j])
                        {
                            comparationResults[i, j] = "ROUND 1: " +
                                " By the count of their grand slam titles the result is: \n" +
                                name[i] + " : " + titleCount[i] + "     " + name[j] + " : " + titleCount[j] + "\n" +
                                "      The result is  " + "EQUAL" + " !!!" + "\n\n";
                        }
                    }
                }
            }
            return comparationResults;
        }

        // longest time between two consecative titles
        static string[,] CompareByLongestTime(int[] longestTime, string[] name)
        {
            string[,] comparationResults = new string[longestTime.Length, longestTime.Length];

            for (int i = 0; i < longestTime.Length; i++)
            {
                for (int j = 0; j < longestTime.Length; j++)
                {
                    if (i == j)
                    {
                        comparationResults[i, j] = "Unusable";
                    }
                    else
                    {
                        if (longestTime[i] > longestTime[j])
                        {
                            comparationResults[i, j] = "ROUND 2: " +
                                "By the longest time between two consecative titles \n" +
                                name[i] + " : " + longestTime[i] + "     " + name[j] + " : " + longestTime[j] + "\n" +
                                "      Wins:" + name[i].ToUpper() + " !!!" + "\n\n";
                        }

                        if (longestTime[i] < longestTime[j])
                        {
                            comparationResults[i, j] = "ROUND 2: " +
                                "By the longest time between two consecative titles \n" +
                                name[i] + " : " + longestTime[i] + "     " + name[j] + " : " + longestTime[j] + "\n" +
                                "      Wins:" + name[j].ToUpper() + " !!!" + "\n\n";
                        }

                        if (longestTime[i] == longestTime[j])
                        {
                            comparationResults[i, j] = "ROUND 2: " +
                                "By the longest time between two consecative titles \n" +
                                name[i] + " : " + longestTime[i] + "     " + name[j] + " : " + longestTime[j] + "\n" +
                                "      The result is  " + "EQUAL" + " !!!" + "\n\n";
                        }
                    }
                }
            }
            return comparationResults;
        }

        static string[,] CompareByWinningYearsLength(int[] winningYears, string[] name)
        {
            string[,] comparationResults = new string[winningYears.Length, winningYears.Length];

            for (int i = 0; i < winningYears.Length; i++)
            {
                for (int j = 0; j < winningYears.Length; j++)
                {
                    if (i == j)
                    {
                        comparationResults[i, j] = "Unusable";
                    }
                    else
                    {
                        if (winningYears[i] > winningYears[j])
                        {
                            comparationResults[i, j] = "ROUND 3: " +
                                "By distance between first and last title (How long time they were successful) \n" +
                                name[i] + " : " + winningYears[i] + "     " + name[j] + " : " + winningYears[j] + "\n" +
                                "      Wins:" + name[i].ToUpper() + " !!!" + "\n\n";
                        }

                        if (winningYears[i] < winningYears[j])
                        {
                            comparationResults[i, j] = "ROUND 3: " +
                                "By distance between first and last title(How long time they were successful) \n" +
                                name[i] + " : " + winningYears[i] + "     " + name[j] + " : " + winningYears[j] + "\n" +
                                "      Wins:" + name[j].ToUpper() + " !!!" + "\n\n";
                        }

                        if (winningYears[i] == winningYears[j])
                        {
                            comparationResults[i, j] = "ROUND 3: " +
                                "By distance between first and last title(How long time they were successful) \n" +
                                name[i] + " : " + winningYears[i] + "     " + name[j] + " : " + winningYears[j] + "\n" +
                                "      The result is  " + "EQUAL" + " !!!" + "\n\n";
                        }
                    }
                }
            }
            return comparationResults;
        }

        static string[,] CompareTitlesValueByOponent(double[] titlesValueByOponentIs, string[] name)
        {
            string[,] comparationResults = new string[titlesValueByOponentIs.Length, titlesValueByOponentIs.Length];

            for (int i = 0; i < titlesValueByOponentIs.Length; i++)
            {
                for (int j = 0; j < titlesValueByOponentIs.Length; j++)
                {
                    if (i == j)
                    {
                        comparationResults[i, j] = "Unusable";
                    }
                    else
                    {
                        if (titlesValueByOponentIs[i] > titlesValueByOponentIs[j])
                        {
                            comparationResults[i, j] = "ROUND 4: " +
                                "By the strenght of oponent who played when the player won each his title \n" +
                                name[i] + " : " + titlesValueByOponentIs[i] + "     " + name[j] + " : " + titlesValueByOponentIs[j] + "\n" +
                                "      Wins:" + name[i].ToUpper() + " !!!" + "\n\n";
                        }

                        if (titlesValueByOponentIs[i] < titlesValueByOponentIs[j])
                        {
                            comparationResults[i, j] = "ROUND 4: " +
                                "By the strenght of oponent who played when the player won each his title \n" +
                                name[i] + " : " + titlesValueByOponentIs[i] + "     " + name[j] + " : " + titlesValueByOponentIs[j] + "\n" +
                                "      Wins:" + name[j].ToUpper() + " !!!" + "\n\n";
                        }

                        if (titlesValueByOponentIs[i] == titlesValueByOponentIs[j])
                        {
                            comparationResults[i, j] = "ROUND 4: " +
                                "By the strenght of oponent who played when the player won each his title \n" +
                                name[i] + " : " + titlesValueByOponentIs[i] + "     " + name[j] + " : " + titlesValueByOponentIs[j] + "\n" +
                                "      The result is  " + "EQUAL" + " !!!" + "\n\n";
                        }
                    }
                }
            }
            return comparationResults;
        }

        static string[,] CompareByFinalResult(double[] finalResultBase, int[] winningYears, int[] longestTimeBetween, int[] titlesCount,
             double[] titlesValueByOponent, string[] name)
        {
            double iResult = 0;
            double jResult = 0;
            string[,] ResultOfComparation = new string[finalResultBase.Length, finalResultBase.Length];
            for (int i = 0; i < finalResultBase.Length; i++)
            {

                for (int j = 0; j < finalResultBase.Length; j++)
                {
                    iResult = finalResultBase[i];
                    jResult = finalResultBase[j];
                    if (i == j)
                    {
                        ResultOfComparation[i, j] = "Unusable";
                    }
                    else
                    {
                        if (winningYears[i] > winningYears[j])
                        {
                            iResult = iResult + 0.5;
                        }
                        if (winningYears[i] < winningYears[j])
                        {
                            jResult = jResult + 0.5;
                        }


                        if (longestTimeBetween[i] > longestTimeBetween[j])
                        {
                            iResult = iResult + 0.5;
                        }
                        if (longestTimeBetween[i] < longestTimeBetween[j])
                        {
                            jResult = jResult + 0.5;
                        }

                        if (titlesCount[i] > titlesCount[j])
                        {
                            iResult = iResult + 0.5;
                        }
                        if (titlesCount[i] < titlesCount[j])
                        {
                            jResult = jResult + 0.5;
                        }

                        if (titlesValueByOponent[i] > titlesValueByOponent[j])
                        {
                            iResult = iResult + 0.5;
                        }
                        if (titlesValueByOponent[i] < titlesValueByOponent[j])
                        {
                            jResult = jResult + 0.5;
                        }

                        if (iResult > jResult)
                        {
                            ResultOfComparation[i, j] = "FINAL RESULT : " +
                                "By all components the final result is: \n" +
                                name[i] + " : " + iResult + "     " + name[j] + " : " + jResult + "\n" +
                                "      Wins:" + name[i].ToUpper() + " !!!" + "\n\n";
                        }

                        if (iResult < jResult)
                        {
                            ResultOfComparation[i, j] = "FINAL RESULT : " +
                                "By all components the final result is: \n" +
                                name[i] + " : " + iResult + "     " + name[j] + " : " + jResult + "\n" +
                                "      Wins:" + name[j].ToUpper() + " !!!" + "\n\n";
                        }

                        if (iResult == jResult)
                        {
                            ResultOfComparation[i, j] = "FINAL RESULT : " +
                                "By all components the final result is: \n" +
                                name[i] + " : " + iResult + "     " + name[j] + " : " + jResult + "\n" +
                                "      The result is  " + "EQUAL" + " !!!" + "\n";
                        }
                    }
                }
            }
            return ResultOfComparation;
        }


        private static Task<string> MakeAsyncRequest(string file)
        {
            var request = WebRequest.CreateHttp(file);
            request.ContentType = "application/json";
            request.Method = "GET";
            var task = Task.Factory.FromAsync(request.BeginGetResponse,
                (asyncResult) => request.EndGetResponse(asyncResult),
                (object)null);

            return task.ContinueWith(t => ReadStreamFromResponse(t.Result));
        }

        private static string ReadStreamFromResponse(WebResponse response)
        {
            string strContent = null;
            using (var responseStream = response.GetResponseStream())
            using (var sr = new StreamReader(responseStream))
            {
                strContent = sr.ReadToEnd();

                return strContent;
            }
        }

        public SampleDataSource()
        {
            var please = MakeAsyncRequest("http://localhost:63174/api/name");
            string namesStr = please.Result;
            var theName = JsonConvert.DeserializeObject<string[]>(namesStr);

            please = MakeAsyncRequest("http://localhost:63174/api/yearoftitle");
            string yearsOfTitlesStr=please.Result;
            var theYearOfTitle = JsonConvert.DeserializeObject<int[,]>(yearsOfTitlesStr);

            int[] titlesCount = CountingTitles(theYearOfTitle);
            int[] winningYears = CalculatingWinningYears(theYearOfTitle);
            int[] yearOfFirstTitle = FindFirstYearWithTitle(theYearOfTitle);
            int[] yearOfLastTitle = FindLastYearWithTitle(theYearOfTitle);
            int[] longestTime = CalculatingLongestTime(theYearOfTitle, titlesCount);
            double[] bonusFromOponents = CalculeteTitlesValueByOponents(theYearOfTitle, yearOfFirstTitle, yearOfLastTitle, titlesCount);

            double[] finalResultBase = CalculatingFinalResultBase(titlesCount, bonusFromOponents);
            string[,] comparationByLongestTimeBetween = CompareByLongestTime(longestTime, theName);
            string[,] comparationByTitlesCount = CompareByTitlesCount(titlesCount, theName);
            string[,] comparationByWinningYearsLength = CompareByWinningYearsLength(winningYears, theName);
            string[,] comparationByTitlesValueByOponent = CompareTitlesValueByOponent(bonusFromOponents, theName);
            string[,] finalResult = CompareByFinalResult(finalResultBase, winningYears, longestTime, titlesCount, bonusFromOponents, theName);

            string[] descriptionString =
            {
                "Stefan Edberg popular with his elegant serve voley tactic, he played three finals on wimbledon against Becker.",
                "Boris Becker youngest Wimbledon open era champion 17 years.",
                "Novak Djockovic the player popular with his retur and sence of humor.",
                "John Newcombe has been popular with frequently coming up with a second-serve ace.",
                "John Mcenroe great sinles player with strange character who has and a lot doubles titles.",
                "Mats Vilander has won three grand slams in 1988 and with the help of Stefan Edber create Sweedish Grand Slam.",
                "Ken Rosewall is the oldest player in open era won a grand slam title Australian open on 37.",
                "Jimmy Connors he holds the record for singles titles in the open era 109.",
                "Ivan Lendl lost his first four grand slam finals but on the fifth create unbelivable comeback agains Mcenroe and win.",
                "Andre Aggasi the player who change the game with his early taking of the boll.",
                "Rod Laver is the only player in open era made Grand Slam !!!",
                "Bjorn Borg retired on age only 26.",
                "Rafael Nadal the only player with 8 grand slams on wan slam French open.",
                "Pete Sampras the player with perfect serve which help him to won 7 Wimbledon titles, equal with Federer.",
                "Roger Federer the record holder with his 17 grand slam titles.",

                // I add this 20 descriptions in case that new player take his fifth title and I have to add for him picture and description.
                // This description will apear before I see for who I have to add discription and add it. 
                // I add them this way because I do not know what will hapend in the feature.
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
                "Soon description and Picture for this new big champion will be added.",
            };

            string[] ImageString = 
            { 
                "Assets/01.StefanEdberg.png",
                "Assets/02.BorisBecker.png",
                "Assets/03.NovakDjockovic.png",
                "Assets/04.JohnNewcombe.png",
                "Assets/05.JohnMcenroe.png",
                "Assets/06.MatsVilander.png",
                "Assets/07.KenRosewall.png",
                "Assets/08.JimmyConnors.png",
                "Assets/09.IvanLendl.png",
                "Assets/10.AndreAggasi.png",
                "Assets/11.RodLaver.png",
                "Assets/12.BjornBorg.png",
                "Assets/13.RafaelNadal.png",
                "Assets/14.PeteSampras.png",
                "Assets/15.RogerFederer.png",

                // I add this 20 same pictures in case that new player take his fifth title and I have to add for him picture and description.
                // This picture will apear before I see for who I have to add picture and add it. 
                // I add them this way because I do not know what will hapend in the feature.
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
                "Assets/Logo.png",
            };

            SampleDataGroup[] group = new SampleDataGroup[15];
            for (int i = 0; i < 15; i++)
            {
                group[i] = new SampleDataGroup("Group-" + (i + 1),
                            theName[i],
                            theName[i] + " Grand Slam titles: " + titlesCount[i],
                            ImageString[i],
                            descriptionString[i]);
                for (int j = 0; j < 15; j++)
                {
                    if (i != j)
                    {
                        group[i].Items.Add(new SampleDataItem("Group-" + (i + 1) + "-Item-" + (j + 1),
                                 theName[j],
                                    theName[j] + " Grand Slam titles: " + titlesCount[j],
                                    ImageString[j],
                                    descriptionString[j],
                            comparationByTitlesCount[i, j] +
                            comparationByLongestTimeBetween[i, j] +
                            comparationByWinningYearsLength[i, j] +
                            comparationByTitlesValueByOponent[i, j] +
                            finalResult[i, j],
                                group[i]));
                    }
                }
                this.AllGroups.Add(group[i]);
            }
        }
    }
}
