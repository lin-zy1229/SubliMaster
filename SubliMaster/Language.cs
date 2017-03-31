using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubliMaster
{
    public class Language
    {
        public enum LangKind { English, German};
        public static LangKind curLang = LangKind.English;

        public static Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>();

     
        public Language()
        {
        }

        static Language() {
            
            add2Dic("Get Pro Version", "Get Pro Version", "Zur Pro Version");
            add2Dic("Options", "Options", "Optionen");
            add2Dic("Apply All", "Apply All", "Anwenden");
            add2Dic("Opacity", "Opacity", "Deckkraft");
            add2Dic("Sort by", "Sort by", "Ordnen nach");

            add2Dic("Delete", "Delete", "Löschen");

            add2Dic(
               "My Selection",
                    "My Selection",
                    "Meine Wahl");
            add2Dic(
               "Introduction & News", 
                    "Introduction & News",
                    "Anleitung & News");
            add2Dic(
               "Introduction & News", 
                    "Introduction & News",
                    "Anleitung & News");

            add2Dic(
                "Category", 
                    "Category",
                    "Categorie" );
            add2Dic(
                "Group", 
                    "Group",
                    "Gruppe" );
            add2Dic(
                "Images", 
                    "Images",
                    "Bilder" );
            add2Dic(
                "Suggestions", 
                    "Suggestions",
                    "Suggestionen" );

            add2Dic(
               "This is a Pro version feature.",
                   "This is a Pro version feature.",
                   "Dies ist eine Funktion der Pro Version.");
            add2Dic(
                "Get your license here.",
                    "Get your license here.",
                    "Hol dir hier deine Lizenz.");

        }

        public static void add2Dic(string key, params string [] values)
        {
            if (dictionary.ContainsKey(key))
                dictionary.Remove(key);
            dictionary.Add(key, values);
        }

        public static void setCurLang(string lang)
        {
            switch ( lang.Trim().ToLower() ){
                case "english":
                    curLang = LangKind.English;
                    break;
                case "german":
                    curLang = LangKind.German;
                    break;
                default:
                    curLang = LangKind.English;
                    break;
            }
        }

        public static string getString(string key)
        {
            string[] values;
            if (dictionary.TryGetValue(key, out values))
                return values[(int)curLang];
            return "";
        }
    }
}
