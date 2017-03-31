using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

namespace SubliMaster
{
    public partial class FormLogin : Form
    {
        /// <summary>
        /// write a method for each text bopx here
        ///i dont know what is textbox1
        /// </summary>
        /// <returns></returns>
        public string getEmail()
        {
            return textBox1.Text;
        }
        public string getKey()
        {
            return textBox2.Text;
        }

        public int getType()
        {
            return 2;
        }
        
        public FormLogin()
        {
            InitializeComponent();

            string select_str = "";
            XmlDocument doc = new XmlDocument();
            string path = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            doc.Load(path + "/languages.xml");

            XmlNode lang_node = doc.DocumentElement.SelectSingleNode("//Root//languages");

            foreach (XmlNode chldNode in lang_node.ChildNodes)
            {
                if (chldNode.InnerText.Split(':')[1].Trim() == "default")
                {
                    select_str = chldNode.InnerText.Split(':')[0].Trim();
                }

            }

            XmlNode Root_node = doc.DocumentElement.SelectSingleNode("//Root");
            foreach (XmlNode chldNode in Root_node.ChildNodes)
            {
                string str = chldNode.Attributes["name"].InnerXml.Trim();
                if (str == select_str)
                {
                    lbl_heading.Text = chldNode["lable_1_text"].InnerText;
                    lbl_email.Text = chldNode["registration_email"].InnerText;
                    lbl_key.Text = chldNode["registration_key"].InnerText;             
                    btn_order.Text = chldNode["btn_order_online"].InnerText;
                    btn_ok.Text = chldNode["btn_ok"].InnerText;
                    btn_cancel.Text = chldNode["btn_cancle"].InnerText;
                }
            }
            doc.Save(path + "/languages.xml");

        }
    }
}
