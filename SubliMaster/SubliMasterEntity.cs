using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using System.Xml.Serialization;
using System.Threading;

namespace SubliMaster
{
    [XmlRoot("SubliCategories")]
    public class SubliCategories
    {
        [XmlArray("CategoriesArray")]
        [XmlArrayItem("Category")]
        public List<SubliCategory> Categories { get; set; }
    }
    [XmlRoot("SubliCategory")]
    public class SubliCategory
    {
        [XmlElement("CategoryName")]
        public string CategoryName { get; set; }
        [XmlElement("StartupMode")]
        public string StartupMode { get; set; }
        [XmlElement("Language")]
        public string Language { get; set; }

        [XmlArray("GroupsArray")]
        [XmlArrayItem("Group")]
        public List<SubliGroup> Groups { get; set; }        
    }

    [XmlRoot("SubliGroup")]
    public class SubliGroup
    {
        [XmlElement("GroupName")]
        public string GroupName { get; set; }

        public SubliImageEntity SubliImage { get; set; }
        public SubliSuggestionsEntity SubliSuggestion { get; set; }
    }

    [XmlRoot("SubliImageEntity")]
    public class SubliImageEntity
    {
        [XmlElement("SplashingPeriodInSeconds")]
        public int SplashingPeriodInSeconds { get; set; }

        [XmlElement("SplashDisplayForMilliSeconds")]
        public int SplashDisplayForMilliSeconds { get; set; }

        [XmlElement("SplashPosition")]
        public int SplashPosition { get; set; }

        [XmlElement("Transparency")]
        public int Transparency { get; set; }

        [XmlElement("Opacity")]
        public int Opacity { get; set; }

        [XmlElement("SortingOrder")]
        public int SortingOrder { get; set; }

        [XmlArray("ImageFilesArray")]
        [XmlArrayItem("ImageFileName")]
        public List<string> ImageFiles { get; set; }

        private System.Threading.Timer RefreshTimer;
        public void StartImageSplash()
        {
            RefreshTimer = new System.Threading.Timer(new System.Threading.TimerCallback(Timer_Elapsed), null, SplashingPeriodInSeconds*1000, System.Threading.Timeout.Infinite);
        }
        public void StopImageSplash()
        {
            RefreshTimer.Dispose();
        }

        private void Timer_Elapsed(object sender)
        {
            RefreshTimer.Dispose();
            foreach (var img in ImageFiles)
            {
                if (img.Split(',')[3] == "y")
                {
                    SplashScreen ss = new SplashScreen();
                    Thread splashthread = new Thread(new ParameterizedThreadStart(ss.ShowSplashScreen));
                    splashthread.Start((object)new SubliCurrentImages { CurrentImage = img, SubliImagesEntity = this });
                    int milliseconds = SplashDisplayForMilliSeconds;
                    Thread.Sleep(milliseconds);
                    splashthread.Abort();
                    ss.CloseSplashScreen();                    
                }
            }
            RefreshTimer = new System.Threading.Timer(new System.Threading.TimerCallback(Timer_Elapsed), null, SplashingPeriodInSeconds * 1000, System.Threading.Timeout.Infinite);
        }
    }
    public class SubliCurrentSuggestions
    {
        public SubliSuggestionsEntity SubliSuggestionsEntity { get; set; }
        public String CurrentSuggestion { get; set; }
    }
    public class SubliCurrentImages
    {
        public SubliImageEntity SubliImagesEntity { get; set; }
        public String CurrentImage { get; set; }
    }
    [XmlRoot("SubliSuggestionsEntity")]
    public class SubliSuggestionsEntity
    {
        [XmlElement("SplashingPeriodInSeconds")]
        public int SplashingPeriodInSeconds { get; set; }

        [XmlElement("SplashDisplayForMilliSeconds")]
        public int SplashDisplayForMilliSeconds { get; set; }

        [XmlElement("SplashPosition")]
        public int SplashPosition { get; set; }

        [XmlElement("Transparency")]
        public int Transparency { get; set; }

        [XmlElement("Opacity")]
        public int Opacity { get; set; }

        [XmlElement("SelectedFamily")]
        public String SelectedFont { get; set; }
        
        [XmlElement("FontColor")]
        public String FontColor { get; set; }

        [XmlElement("BackgroundColor")]
        public String BackgroundColor { get; set; }

        [XmlElement("Language")]
        public string Language { get; set; }

        [XmlArray("SuggestionsArray")]
        [XmlArrayItem("Suggestion")]
        public List<string> Suggestions { get; set; }

        private System.Threading.Timer RefreshTimer;
        public void StartSuggestionSplash()
        {
            RefreshTimer = new System.Threading.Timer(new System.Threading.TimerCallback(Timer_Elapsed), null, SplashingPeriodInSeconds * 1000, System.Threading.Timeout.Infinite);
        }
        public void StopSuggestionSplash()
        {
            RefreshTimer.Dispose();
        }
        private void Timer_Elapsed(object sender)
        {
            RefreshTimer.Dispose();
            foreach (var splsh in Suggestions)
            {
                TextSplashScreen ss = new TextSplashScreen();
                Thread splashthread = new Thread(new ParameterizedThreadStart(ss.ShowSplashScreen));
                splashthread.Start((object)new SubliCurrentSuggestions { CurrentSuggestion = splsh, SubliSuggestionsEntity = this });
                int milliseconds = SplashDisplayForMilliSeconds;
                Thread.Sleep(milliseconds);
                ss.CloseSplashScreen();
            }
            RefreshTimer = new System.Threading.Timer(new System.Threading.TimerCallback(Timer_Elapsed), null, SplashingPeriodInSeconds * 1000, System.Threading.Timeout.Infinite);
        }
    }
}
