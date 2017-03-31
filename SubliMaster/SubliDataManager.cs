using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using System.Xml.Serialization;

namespace SubliMaster
{
    public class SubliDataManager
    {
        /// <summary>
        /// Write Configuration To XML File
        /// </summary>
        /// <param name="category"></param>
        /// <param name="categories"> </param>
        /// <returns></returns>
        public static bool Write(SubliCategories category,string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            try
            {
                var x = new XmlSerializer(category.GetType());
                var writer = new StreamWriter(path);
                x.Serialize(writer, category);
                writer.Close();
                //x.Serialize(writer, categories);
                return true;
            }
            catch
            {
                return false;
            }
            /*
            XmlSerializer serializer = new XmlSerializer(typeof(SubliCategory));
            FileStream fs = new FileStream("Subli.xml", FileMode.Create);
            serializer.Serialize(fs, settings);
            fs.Close();
            */
        }
 
        /// <summary>
        /// Read Settings
        /// </summary>
        /// <returns></returns>
        public static SubliCategories Read(string path)
        {
            var settings = new SubliCategories();
            
            try
            {
                var x = new XmlSerializer(typeof(SubliCategories));
                var reader = new StreamReader(path);
                settings = (SubliCategories)x.Deserialize(reader);
                reader.Close();
                return settings;
            }
            catch
            {
                //return false;
            }
            return settings;
            /*
            XmlSerializer serializer = new XmlSerializer(typeof(SubliCategory));
            FileStream fs = new FileStream("Subli.xml", FileMode.Open);
            settings = (SubliCategory)serializer.Deserialize(fs);
            serializer.Serialize(Console.Out, settings);
            */
        }
    }
}
