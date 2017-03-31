using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics;

namespace SubliMaster
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


            XmlDocument doc = new XmlDocument();
            string path = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            doc.Load(path + "/languages.xml");
            XmlNode node = doc.DocumentElement.SelectSingleNode("//Root//languages");

            var langList = new List<GenericList>();
            foreach (XmlNode chldNode in node.ChildNodes)
            {
                GenericList g = new GenericList { DisplayMember = chldNode.InnerText.Split(':')[0].Trim(), ValueMember = chldNode.InnerText.Split(':')[0].Trim() };
                langList.Add(g);
            }


            cmb_languages.ValueMember = "ValueMember";
            cmb_languages.DisplayMember = "DisplayMember";
            cmb_languages.DataSource = langList;


            XmlNode Root_node = doc.DocumentElement.SelectSingleNode("//Root");
            foreach (XmlNode chldNode in Root_node.ChildNodes)
            {
                string str = chldNode.Attributes["name"].InnerXml.Trim();
                if (str == "English")
                {
                    this.Text = chldNode["title"].InnerText;
                    lbl_text_1.Text = chldNode["lable_text_1"].InnerText;
                    lbl_text_2.Text = chldNode["lable_text_2"].InnerText;
                    btn_register.Text = chldNode["btn_register"].InnerText;
                    btn_signup.Text = chldNode["btn_signup"].InnerText;
                }
            }
            doc.Save(path + "/languages.xml");
        }

        private void cmb_languages_SelectedValueChanged(object sender, EventArgs e)
        {

            string select_str = Convert.ToString(cmb_languages.SelectedValue).Split(':')[0].Trim();
            XmlDocument doc = new XmlDocument();
            string path = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            doc.Load(path + "/languages.xml");

            XmlNode lang_node = doc.DocumentElement.SelectSingleNode("//Root//languages");

            foreach (XmlNode chldNode in lang_node.ChildNodes)
            {
                if (chldNode.InnerText.Split(':')[0].Trim() == select_str)
                {
                    chldNode.InnerText = chldNode.InnerText.Split(':')[0].Trim()+":default";
                }
               else
               {
                   chldNode.InnerText = chldNode.InnerText.Split(':')[0].Trim() + ":no";
               }
                
            }
            
            XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
            try
            {
                foreach (XmlNode chldNode in node.ChildNodes)
                {
                    string language = chldNode.Attributes["name"].InnerXml.Trim();
                    if (language == select_str)
                    {
                        this.Text = chldNode["title"].InnerText;
                        lbl_text_1.Text = chldNode["lable_text_1"].InnerText;
                        lbl_text_2.Text = chldNode["lable_text_2"].InnerText;
                        btn_register.Text = chldNode["btn_register"].InnerText;
                        btn_signup.Text = chldNode["btn_signup"].InnerText;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            doc.Save(path + "/languages.xml");
        }

    }
}
