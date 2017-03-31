using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Windows.Forms;

namespace SubliMaster
{
    public partial class NewGroup : Form
    {
        private SubliCategories subliCategories;
        public NewGroup()
        {
            InitializeComponent();
            language_data(1);
        }

        private void NewGroup_Load(object sender, EventArgs e)
        {

        }

        private void language_data(int type_id)
        {
            string bstre = "";
            if (type_id == 1)
            {
                var xml_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var subli1 = SubliDataManager.Read(xml_path + "\\Subli.xml");
                subliCategories = subli1;
                if (subli1.Categories != null)
                {
                    var subli = subli1.Categories[0];
                    bstre = subli.Language;
                }
            }

            XmlDocument doc = new XmlDocument();
            string path = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            doc.Load(path + "/languages.xml");
            XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
            foreach (XmlNode chldNode in node.ChildNodes)
            {
                string astre = chldNode.Attributes["name"].InnerXml.Trim();
                if (astre == bstre)
                {

                    this.Text = chldNode["new_group"].InnerText;
                    label1.Text = chldNode["new_group"].InnerText;
                    btnSave.Text = chldNode["save"].InnerText;
                    btnCancel.Text = chldNode["cancel"].InnerText;
                }
            }
            doc.Save(path + "/languages.xml");
        }
    }
}
