using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Threading;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Web;
using System.Runtime.InteropServices;
using CefSharp.WinForms;
using CefSharp;

namespace SubliMaster
{
    public partial class SubliMasterMain : Form
    {
        public enum VersionType {FREE, PRO};
        public static VersionType curVersion = VersionType.PRO;

        public enum Section { HOME, SELECTION, TEST_AREA, INTRODUCTION };
        const Section INIT_SECTION = Section.HOME;
        Section curSection = INIT_SECTION;
        private bool access = true;
        private RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        private SubliCategories subliCategories;
        private bool bool_stop = false;
        private bool flag = true;
        private TreeNode tn;
        private string srt = "";
        private const int bytesPerPixel = 4;

        //gert added
        Label[] secLabels;
        Label[] secUnderLabels;
        public SubliMasterMain()
        {


            check_registry(out flag);
            if (flag)
            {
                InitializeComponent();
                FillAndBindLists();
                language_data(1);

                //added by gert
                //webBrowser1.Visible = true;
                secLabels = new Label[]{label2, label13, label12, label3};
                secUnderLabels = new Label[] { label14, label15, label16, label17 };
                curSection = INIT_SECTION;
                selectSection(curSection);
                this.DoubleBuffered = true;
                this.SetStyle(ControlStyles.ResizeRedraw, true);

                gboxHome.MouseDown += new MouseEventHandler(this.Control_MouseDown);
                gboxSelection.MouseDown += new MouseEventHandler(this.Control_MouseDown);
                gboxTestArea.MouseDown += new MouseEventHandler(this.Control_MouseDown);
                gboxIntroAndNews.MouseDown += new MouseEventHandler(this.Control_MouseDown);

                button15.Cursor = Cursors.Hand;

                initComponentsByVersion();

                //secLabels[(int)INIT_SECTION].Tag = "1";
                //
                // web browser
                //

                // 
                // webBrowser1
                // 
                InitBrowser();

                foreach(Form a in Application.OpenForms){
                    
                    a.TopMost = true;
                    a.TopLevel = true;
                }


                //webBrowser1.ScriptErrorsSuppressed = true;
                //webBrowser1.Url = new Uri(@"https://www.lerntipp.com/subli");
                //webBrowser1.Visible = true;
            }
            else
            {
                this.Dispose();
                this.Close();
            }
            /* InitializeComponent();
            FillAndBindLists();
            language_data(1);*/
        }

        private void initComponentsByVersion()
        {
            // pro version button
            button15.Visible = (curVersion == VersionType.FREE);
            trkOpacity.Enabled = (curVersion == VersionType.PRO);
            btnChangeFont.Enabled = (curVersion == VersionType.PRO);
            linkLabel1.Visible = (curVersion == VersionType.FREE);
        }

        public ChromiumWebBrowser browser;
        public void InitBrowser()
        {
            Cef.Initialize(new CefSettings());
            browser = new ChromiumWebBrowser("https://www.lerntipp.com/subli");
            //browser.Refresh();

            this.gboxIntroAndNews.Controls.Add(browser);
            browser.Dock = DockStyle.None;
            browser.FrameLoadEnd += WebBrowserFrameLoadEnded;
        }
        private void WebBrowserFrameLoadEnded(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                //browser.ViewSource();
                browser.GetSourceAsync().ContinueWith(taskHtml =>
                {
                    var html = taskHtml.Result;
                    Console.WriteLine(html);
                });
            }
        }
        /**
         * Gert added for Window dragging
         */
        private const int cGrip = 16;      // Grip size
        private const int cCaption = 32;   // Caption bar height;

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle rc = new Rectangle(this.ClientSize.Width - cGrip, this.ClientSize.Height - cGrip, cGrip, cGrip);
            ControlPaint.DrawSizeGrip(e.Graphics, this.BackColor, rc);
            //rc = new Rectangle(0, 0, this.ClientSize.Width, cCaption);
            //e.Graphics.FillRectangle(Brushes.DarkBlue, rc);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {  // Trap WM_NCHITTEST
                Point pos = new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16);
                pos = this.PointToClient(pos);
                if (pos.Y < cCaption)
                {
                    m.Result = (IntPtr)2;  // HTCAPTION
                    return;
                }
                if (pos.X >= this.ClientSize.Width - cGrip && pos.Y >= this.ClientSize.Height - cGrip)
                {
                    m.Result = (IntPtr)17; // HTBOTTOMRIGHT
                    return;
                }
            }
            base.WndProc(ref m);
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Control_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((sender == this || sender == panel1) && WindowState == FormWindowState.Maximized)
                return;
            if ((sender == this || sender == panel1) && e.Clicks == 2)
                return;

            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }


        #region checking registry for registration

        /// <summary>
        /// check regiostry value
        /// </summary>
        /// <param name="flag"></param>
        private void check_registry(out bool flag)
        {

            string response_text = "";
            try
            {
                response_text = rkApp.GetValue("SubliDesk_registered").ToString();
            }
            catch { }
            if (response_text == "true-2")
            {
                flag = true;
            }

            else
            {
               

                flag = true;


                /* commented by gert
                flag = first_screen();
                
                if (first_screen())
                    flag = true;
                else
                    flag = false;
                */
            }
        }

        /// <summary>
        /// redirect user to site
        /// </summary>
        private void nameSomeThinguWant()
        {
            try
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "firefox";
                p.StartInfo.Arguments = "http:www.sublidesk.com/purchase.php";
                bool flagp = p.Start();
            }
            catch
            {

            }

        }


        /// <summary>
        /// if not registered in registry
        /// </summary>
        /// <returns></returns>
        private bool first_screen()
        {
            bool returning = true;
            var newform = new Form1();

            DialogResult res_fir = newform.ShowDialog();

            /*register button of Form1*/

            if (res_fir == DialogResult.Yes)
            {
                System.Diagnostics.Process p = System.Diagnostics.Process.Start("http:www.sublidesk.com/purchase.php");
                returning = false;
            }

            /*signup button of Form1*/

            else if (res_fir == DialogResult.OK)
            {

                var form_login = new FormLogin();

                DialogResult res = form_login.ShowDialog();

                /*order online button of FormLogin*/

                if (res == DialogResult.Yes)
                {
                    nameSomeThinguWant();
                    returning = false;
                }

                /*Ok button of FormLogin*/

                else if (res == DialogResult.OK)
                {
                    try
                    {
                        string email_text = "id=" + form_login.getEmail().Trim();
                        string key_text = "&value=" + form_login.getKey().Trim();
                        string type_text = "&type=" + form_login.getType().ToString().Trim();


                        //string query_url = @"http://localhost/HTML/validate.php?" + email_text + key_text + type_text;// localhost url
                        string query_url = @"http://sublidesk.com/validate.php?" + email_text + key_text + type_text;// live url

                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(query_url);

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        StreamReader input = new StreamReader(response.GetResponseStream());

                        DataSet dsTest = new DataSet();

                        dsTest.ReadXml(input);

                        int i, j, varTotCol = dsTest.Tables[0].Columns.Count, varTotRow = dsTest.Tables[0].Rows.Count;

                        for (j = 0; j < varTotRow; j++)
                        {
                            for (i = 0; i < varTotCol; i++)
                            {
                                string str = dsTest.Tables[0].Rows[j].ItemArray[0].ToString();
                                if (str != "false")
                                {
                                    // case of true validated do nothing
                                    if (str.Split('-')[1].Trim() == "2")
                                    {
                                        rkApp.SetValue("SubliDesk_registered", str);
                                    }
                                    else
                                    {
                                        returning = false;
                                    }
                                }

                                else
                                {
                                    string stri = "";
                                    string text = "";
                                    var xml_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                                    var subli1 = SubliDataManager.Read(xml_path + "\\Subli.xml");
                                    subliCategories = subli1;
                                    if (subli1.Categories != null)
                                    {
                                        var subli = subli1.Categories[0];
                                        stri = subli.Language;
                                    }

                                    XmlDocument doc = new XmlDocument();
                                    string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                                    doc.Load(pathqw + "/languages.xml");
                                    XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
                                    foreach (XmlNode chldNode in node.ChildNodes)
                                    {
                                        string astring = chldNode.Attributes["name"].InnerXml.Trim();
                                        //
                                        if (astring == stri)
                                        {
                                            text = chldNode["invalid_details"].InnerText;
                                        }
                                    }
                                    MessageBox.Show(text);
                                    returning = false;
                                }
                            }
                        }

                    }
                    catch
                    {
                        returning = false;
                    }
                }
                /*cancel button of FormLogin*/
                else if (res == DialogResult.Cancel)
                {
                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    Process.GetCurrentProcess().Kill();
                }
            }
            else
            {
                Process.GetCurrentProcess().Kill();
            }

            return returning;
        }

        #endregion


        #region Add , Remove , Clear all

        /// <summary>
        /// add new image to list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripBtnAddImage_Click(object sender, EventArgs e)
        {

            addImage();
        }

        private void addImage()
        {
            if (!access)
            {
                string returned_str = show_upgrade_Message();
                MessageBox.Show(returned_str.Split('%')[1].Trim(), returned_str.Split('%')[0].Trim(), MessageBoxButtons.OK);
            }
            else
            {
                imageFilesOpenDialog.Filter = "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF;";
                if (imageFilesOpenDialog.ShowDialog() == DialogResult.OK)
                {
                    //lstViewItems.Items.Clear();
                    string fullPath = imageFilesOpenDialog.FileName;
                    string fileName = imageFilesOpenDialog.SafeFileName;

                    var di = new DirectoryInfo(fullPath.Replace(fileName, ""));
                    int i = 1;
                    var selectedFiles = imageFilesOpenDialog.SafeFileNames;

                    foreach (var file in di.GetFileSystemInfos())
                    {
                        if (selectedFiles.Contains(file.Name))
                        {
                            var fi = new FileInfo(file.FullName);
                            var fi1 = new FileInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                                           "\\" + fi.Name);
                            if (!fi1.Exists)
                                fi.CopyTo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                                           "\\" + fi.Name);
                            var item = new ListViewItem(new[] { fi.Name, file.Extension.TrimStart('.'), Convert.ToString(fi.Length / 1024) });
                            item.Checked = true;
                            lstViewItems.Items.Add(item);
                            i++;

                        }
                    }
                }
            }
        }
        private void addImage2()
        {
            if (!access)
            {
                string returned_str = show_upgrade_Message();
                MessageBox.Show(returned_str.Split('%')[1].Trim(), returned_str.Split('%')[0].Trim(), MessageBoxButtons.OK);
            }
            else
            {
                if (curVersion == VersionType.PRO )
                {

                }
                else if (curVersion == VersionType.FREE)
                {
                    if(listBox1.SelectedIndex == 0)
                    {
                        MessageBox.Show("Can't add more affirmations to default group in FREE version.", "SubliDesk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else if (listBox1.SelectedIndex >= 1)
                    {
                        if (listViewAffImages.Items.Count >= FreeAffirmationCount)
                        {
                            MessageBox.Show("Can't add affirmations more than " + FreeAffirmationCount + " in FREE version.", "SubliDesk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                imageFilesOpenDialog.Filter = "Images (*.BMP;*.JPG;*.GIF)|*.BMP;*.JPG;*.GIF;";
                if (imageFilesOpenDialog.ShowDialog() == DialogResult.OK)
                {
                    //lstViewItems.Items.Clear();
                    string fullPath = imageFilesOpenDialog.FileName;
                    string fileName = imageFilesOpenDialog.SafeFileName;

                    var di = new DirectoryInfo(fullPath.Replace(fileName, ""));
                    int i = 1;
                    var selectedFiles = imageFilesOpenDialog.SafeFileNames;

                    if(curVersion == VersionType.PRO)
                    {

                    }
                    else if (curVersion == VersionType.FREE)
                    {
                        selectedFiles = selectedFiles.Take(Math.Max(0, Math.Min(selectedFiles.Length, FreeAffirmationCount - listViewAffImages.Items.Count))).ToArray();
                    }

                    foreach (var file in di.GetFileSystemInfos())
                    {
                        if (selectedFiles.Contains(file.Name))
                        {
                            var fi = new FileInfo(file.FullName);
                            var fi1 = new FileInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                                           "\\" + fi.Name);
                            if (!fi1.Exists)
                                fi.CopyTo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +
                                                           "\\" + fi.Name);
                            var item = new ListViewItem(new[] { fi.Name, file.Extension.TrimStart('.'), Convert.ToString(fi.Length / 1024) });

                            // followed by client, gert
                            // uncheck default check of image aff.
                            //
                            //item.Checked = true;

                            listViewAffImages.Items.Add(item);
                            i++;
                            //gert added
                            lblChangedMark.Visible = true;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// remove single selected item from list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripBtnRemove_Click(object sender, EventArgs e)
        {
            var selectedItem = lstViewItems.SelectedItems;
            if (selectedItem.Count > 0)
            {
                lstViewItems.Items.Remove(selectedItem[0]);
            }
        }

        /// <summary>
        /// Clear all Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripBtnClearAll_Click(object sender, EventArgs e)
        {
            string stri = "";
            string text1 = "";
            string text2 = "";
            var xml_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var subli1 = SubliDataManager.Read(xml_path + "\\Subli.xml");
            subliCategories = subli1;
            if (subli1.Categories != null)
            {
                var subli = subli1.Categories[0];
                stri = subli.Language;
            }

            XmlDocument doc = new XmlDocument();
            string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            doc.Load(pathqw + "/languages.xml");
            XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
            foreach (XmlNode chldNode in node.ChildNodes)
            {
                string astring = chldNode.Attributes["name"].InnerXml.Trim();
                //
                if (astring == stri)
                {
                    text1 = chldNode["clear_image_list"].InnerText;
                    text2 = chldNode["are_you_sure_to_clear"].InnerText;
                }
            }
            if (MessageBox.Show(text2, text1, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                lstViewItems.Items.Clear();
            }
        }


        #endregion



        #region Load Images and suggestions from Xml


        /// <summary>
        ///  on load ,load suggestions from xml
        /// </summary>
        private void LoadSuggestions()
        {
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var subli1 = SubliDataManager.Read(path + "\\Subli.xml");
            subliCategories = subli1;
            if (subli1.Categories != null)
            {
                var subli = subli1.Categories[0];

                if (curVersion == VersionType.FREE)
                {
                    cmbSuggestionsPeriodSeconds.SelectedValue = 5;
                    cmbSuggestionDurationInMilliseconds.SelectedValue = 271;
                    cmbSplashingPosition.SelectedValue = 1;
                    trkOpacity.Value = 100;
                    lblSelectedFont.Font = rtxtSuggestions.Font = new Font(FontFamily.GenericSansSerif, 16);
                    lblSelectedFont.ForeColor = rtxtSuggestions.ForeColor = Color.Red;
                }
                else if (curVersion == VersionType.PRO)
                {
                    cmbSplashingPosition.SelectedValue = Convert.ToInt32(subli.Groups[0].SubliImage.SplashPosition);

                    cmbSuggestionDurationInMilliseconds.SelectedValue = subli.Groups[0].SubliSuggestion.SplashDisplayForMilliSeconds;
                    cmbSuggestionsPeriodSeconds.SelectedValue = Convert.ToInt32(subli.Groups[0].SubliSuggestion.SplashingPeriodInSeconds);

                    trkOpacity.Value = subli.Groups[0].SubliImage.Opacity;
                    

                    lblSelectedFont.Font = rtxtSuggestions.Font =
                                           new FontConverter().ConvertFromString(
                                               subli.Groups[0].SubliSuggestion.SelectedFont) as Font;

                    lblSelectedFont.ForeColor =
                        rtxtSuggestions.ForeColor = Color.FromName(subli.Groups[0].SubliSuggestion.FontColor);
                }

                cmbSortingOrder.SelectedValue = Convert.ToInt32(subli.Groups[0].SubliImage.SortingOrder);

                chkTransparency.Checked = false;
                if (subli.Groups[0].SubliImage.Transparency == 1)
                {
                    chkTransparency.Checked = true;
                }
                lstViewItems.Items.Clear();

                lblCurrentCategory.Text = subli1.Categories[0].CategoryName + " > " + subli1.Categories[0].Groups[0].GroupName;
                foreach (var img in subli.Groups[0].SubliImage.ImageFiles)
                {
                    var item =
                        new ListViewItem(new[]
                                             {
                                                 img.Split(',')[0], img.Split(',')[1], img.Split(',')[2]
                                             });
                    if (img.Split(',')[3] == "y")
                    {
                        item.Checked = true;
                    }
                    lstViewItems.Items.Add(item);
                }
                foreach (var img in subli.Groups[0].SubliSuggestion.Suggestions)
                {
                    rtxtSuggestions.Text += img + "\n";
                }
                
                LoadSuggestionsTree(subli1, subli1.Categories[0].CategoryName);
            }
        }

        /// <summary>
        /// on change of treenode selection load suggestions from xml
        /// </summary>
        /// <param name="category"></param>
        /// <param name="grp"></param>
        private void LoadSuggestions(String category, String grp, string tab, int pou)
        {
            var sugg = (from sc in subliCategories.Categories
                        where sc.CategoryName == category
                        select sc).FirstOrDefault();
            var g = (from gp in sugg.Groups
                     where gp.GroupName == grp
                     select gp).FirstOrDefault();

            if (g != null)
            {

                cmbSuggestionsPeriodSeconds.SelectedValue = g.SubliSuggestion.SplashingPeriodInSeconds;
                cmbSuggestionDurationInMilliseconds.SelectedValue = g.SubliSuggestion.SplashDisplayForMilliSeconds;

                cmbSplashingPosition.SelectedValue = Convert.ToInt32(g.SubliImage.SplashPosition);
                trkOpacity.Value = g.SubliImage.Opacity;
                cmbSortingOrder.SelectedValue = Convert.ToInt32(g.SubliImage.SortingOrder);
                lblSelectedFont.Font = rtxtSuggestions.Font =
                                       new FontConverter().ConvertFromString(
                                           g.SubliSuggestion.SelectedFont) as Font;

                lblSelectedFont.ForeColor = rtxtSuggestions.ForeColor = Color.FromName(g.SubliSuggestion.FontColor);

                chkTransparency.Checked = false;
                if (g.SubliImage.Transparency == 1)
                {
                    chkTransparency.Checked = true;
                }

                lstViewItems.Items.Clear();
                if (pou == 0)
                {
                    lblCurrentCategory.Text = category + " > " + grp;
                }
                else
                {
                    lblCurrentCategory.Text = category + " > " + grp + " > " + tab;
                }


                foreach (var img in g.SubliImage.ImageFiles)
                {
                    var item =
                        new ListViewItem(new[]
                                             {
                                                 img.Split(',')[0], img.Split(',')[1], img.Split(',')[2]
                                             });
                    if (img.Split(',')[3] == "y")
                    {
                        item.Checked = true;
                    }
                    lstViewItems.Items.Add(item);
                }
                rtxtSuggestions.Text = "";
                foreach (var img in g.SubliSuggestion.Suggestions)
                {
                    rtxtSuggestions.Text += img + "\n";
                }

                if (tab == "Images")
                {
                    tabControls.SelectedTab = tabImages;
                }
                else
                {
                    tabControls.SelectedTab = tabSuggestions;
                }
            }
        }

        // gert, load on new list views
        private void LoadSuggestions2(String category, String grp, string tab, int pou)
        {
            listViewAffImages.Items.Clear();
            listViewAffSuggestions.Items.Clear();

            var sugg = (from sc in subliCategories.Categories
                        where sc.CategoryName == category
                        select sc).FirstOrDefault();

            var g = (from gp in sugg.Groups
                     where gp.GroupName == grp
                     select gp).FirstOrDefault();

            if (g != null)
            {

                if (curVersion == VersionType.FREE)
                {
                    cmbSuggestionsPeriodSeconds.SelectedValue = globalSplashingPeriodInSecondsFree;
                    cmbSuggestionDurationInMilliseconds.SelectedValue = globalDurationInMillisecondsFree;
                    cmbSplashingPosition.SelectedValue = globalPositionFree;
                    trkOpacity.Value = globalOpacityFree;
                    lblSelectedFont.Font = rtxtSuggestions.Font = new Font(FontFamily.GenericSansSerif, globalFontSizeFree);
                    lblSelectedFont.ForeColor = rtxtSuggestions.ForeColor = globalFontColorFree;
                }
                else if(curVersion == VersionType.PRO)
                {
                    cmbSuggestionsPeriodSeconds.SelectedValue = g.SubliSuggestion.SplashingPeriodInSeconds;
                    cmbSuggestionDurationInMilliseconds.SelectedValue = g.SubliSuggestion.SplashDisplayForMilliSeconds;
                    cmbSplashingPosition.SelectedValue = Convert.ToInt32(g.SubliImage.SplashPosition);
                    trkOpacity.Value = g.SubliImage.Opacity;
                    lblSelectedFont.Font = rtxtSuggestions.Font = new FontConverter().ConvertFromString(g.SubliSuggestion.SelectedFont) as Font;
                    lblSelectedFont.ForeColor = rtxtSuggestions.ForeColor = Color.FromName(g.SubliSuggestion.FontColor);
                }

                cmbSortingOrder.SelectedValue = Convert.ToInt32(g.SubliImage.SortingOrder);

                chkTransparency.Checked = false;
                if (g.SubliImage.Transparency == 1)
                {
                    chkTransparency.Checked = true;
                }

                lstViewItems.Items.Clear();

                /*
                if (pou == 0)
                {
                    lblCurrentCategory.Text = category + " > " + grp;
                }
                else
                {
                    lblCurrentCategory.Text = category + " > " + grp + " > " + tab;
                }
                */
                List<string> imgs = g.SubliImage.ImageFiles;
                List<string> sugs = g.SubliSuggestion.Suggestions;

                if(curVersion == VersionType.PRO)
                {

                }else if (curVersion == VersionType.FREE)
                {
                    imgs = imgs.GetRange(0, Math.Min(imgs.Count, FreeAffirmationCount));
                    sugs = sugs.GetRange(0, Math.Min(sugs.Count, FreeAffirmationCount));
                }

                foreach (var img in imgs)
                {
                    var item =
                        new ListViewItem(new[]
                                             {
                                                 img.Split(',')[0], img.Split(',')[1], img.Split(',')[2]
                                             });
                    if (img.Split(',')[3] == "y")
                    {
                        item.Checked = true;
                    }
                    listViewAffImages.Items.Add(item);
                }

                rtxtSuggestions.Text = "";
                
                foreach (var img in sugs)
                {
                    rtxtSuggestions.Text += img + "\n";
                    var item =
                        new ListViewItem(img);
                    listViewAffSuggestions.Items.Add(item);
                }
                

                if (tab == "Images")
                {
                    tabControls.SelectedTab = tabImages;
                }
                else
                {
                    tabControls.SelectedTab = tabSuggestions;
                }
            }
        }
        #endregion



        #region new category, new group, delete node

        /// <summary>
        /// New Category Click, New window opened , new category saved and tree node added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNewCategory_Click(object sender, EventArgs e)
        {
            this.newCategory();
            return;

            if (!access)
            {

                string returned_str = show_upgrade_Message();
                MessageBox.Show(returned_str.Split('%')[1].Trim(), returned_str.Split('%')[0].Trim(), MessageBoxButtons.OK);
            }
            else
            {
                string stri = "";
                string text1 = "";
                string text2 = "";
                var xml_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var subli1 = SubliDataManager.Read(xml_path + "\\Subli.xml");
                subliCategories = subli1;
                if (subli1.Categories != null)
                {
                    var subli = subli1.Categories[0];
                    stri = subli.Language;
                }
                //
                // Create New Category Dialog
                //
                var newCategory = new NewCategory();
                if (newCategory.ShowDialog() == DialogResult.OK)
                {
                    if (newCategory.txtCategoryName.Text == "")
                    {

                        XmlDocument doc = new XmlDocument();
                        string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                        doc.Load(pathqw + "/languages.xml");
                        XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
                        //
                        // Find error text in selected language, gert commented
                        //
                        foreach (XmlNode chldNode in node.ChildNodes)
                        {
                            string astring = chldNode.Attributes["name"].InnerXml.Trim();
                            if (astring == stri)
                            {
                                text1 = chldNode["invalid"].InnerText;
                                text2 = chldNode["category_name_cannot_be_blank"].InnerText;
                                //
                                //gert added
                                //
                                return;
                            }
                        }

                        MessageBox.Show(text2, text1, MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                        return;
                    }
                    var sCat = (from sc in subliCategories.Categories
                                where sc.CategoryName == newCategory.txtCategoryName.Text
                                select sc).FirstOrDefault();
                    //
                    // If exist category, gert commented
                    //
                    if (sCat != null)
                    {
                        XmlDocument doc = new XmlDocument();
                        string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                        doc.Load(pathqw + "/languages.xml");
                        XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
                        foreach (XmlNode chldNode in node.ChildNodes)
                        {
                            string astring = chldNode.Attributes["name"].InnerXml.Trim();
                            //
                            if (astring == stri)
                            {
                                text1 = chldNode["duplicate_record"].InnerText;
                                text2 = chldNode["category_already_exists"].InnerText;
                                //
                                // gert added
                                //
                                return;
                            }
                        }
                        MessageBox.Show(text2, text1, MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                    }
                    //
                    // If new category, gert commented
                    //
                    else
                    {
                       
                        subliCategories.Categories.Add(new SubliCategory
                        {
                            CategoryName = newCategory.txtCategoryName.Text,
                            Language = cmbLanguage.SelectedValue.ToString(),
                            Groups = new List<SubliGroup>{
                                new SubliGroup()
                                {
                                    GroupName = "",
                                    SubliImage = new SubliImageEntity
                                    {
                                        ImageFiles = new List<string>(),
                                        SortingOrder = 1,
                                        SplashDisplayForMilliSeconds = 1000,
                                        SplashPosition = 1,
                                        SplashingPeriodInSeconds = 1,
                                        Opacity = 0,
                                        Transparency=0
                                    },
                                    SubliSuggestion = new SubliSuggestionsEntity
                                    {
                                        Suggestions = new List<string>(),
                                        SplashPosition = 1,
                                        Opacity = 0,
                                        Transparency=0,
                                        SelectedFont = new FontConverter().ConvertToString(new Label().Font),
                                        BackgroundColor = Color.Transparent.Name,
                                        FontColor = new Label().ForeColor.Name,
                                        Language = cmbLanguage.SelectedValue.ToString(),
                                        SplashDisplayForMilliSeconds = 1000,
                                        SplashingPeriodInSeconds = 1
                                    }
                                }
                            }
                        });

                        lblCurrentCategory.Text = newCategory.txtCategoryName.Text;
                        LoadFormDefaults();
                        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
                        LoadSuggestionsTree(subliCategories, newCategory.txtCategoryName.Text);
                    }
                }
            }
        }

        private void saveCategory(string categoryName)
        {
            
            //
            // If new category, gert commented
            //
            //else
            {
                

                lblCurrentCategory.Text = categoryName;
                LoadFormDefaults();
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
                LoadSuggestionsTree(subliCategories, categoryName);
            }
        }

        private void addCategory(string categoryName)
        {
            subliCategories.Categories.Add(new SubliCategory
            {
                CategoryName = categoryName,
                Language = cmbLanguage.SelectedValue.ToString(),
                Groups = new List<SubliGroup>{
                                /*
                                new SubliGroup()
                                {
                                    GroupName = "",
                                    SubliImage = new SubliImageEntity
                                    {
                                        ImageFiles = new List<string>(),
                                        SortingOrder = 1,
                                        SplashDisplayForMilliSeconds = 1000,
                                        SplashPosition = 1,
                                        SplashingPeriodInSeconds = 1,
                                        Opacity = 0,
                                        Transparency=0
                                    },
                                    SubliSuggestion = new SubliSuggestionsEntity
                                    {
                                        Suggestions = new List<string>(),
                                        SplashPosition = 1,
                                        Opacity = 0,
                                        Transparency=0,
                                        SelectedFont = new FontConverter().ConvertToString(new Label().Font),
                                        BackgroundColor = Color.Transparent.Name,
                                        FontColor = new Label().ForeColor.Name,
                                        Language = cmbLanguage.SelectedValue.ToString(),
                                        SplashDisplayForMilliSeconds = 1000,
                                        SplashingPeriodInSeconds = 1
                                    }
                                }*/
                            }
            });
        }

        private void newCategory()
        {
            if (!access)
            {

                string returned_str = show_upgrade_Message();
                MessageBox.Show(returned_str.Split('%')[1].Trim(), returned_str.Split('%')[0].Trim(), MessageBoxButtons.OK);
            }
            else
            {
                if (curVersion == VersionType.PRO)
                {

                }
                else if (curVersion == VersionType.FREE)
                {
                    if (listBox1.Items.Count >= FreeCategoryCount)
                    {
                        MessageBox.Show("Available count of categories limited in Free version.", "SubliDesk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                string stri = "";
                string text1 = "";
                string text2 = "";
                var xml_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var subli1 = SubliDataManager.Read(xml_path + "\\Subli.xml");
                //gert commented
                //subliCategories = subli1;
                if (subli1.Categories != null)
                {
                    var subli = subli1.Categories[0];
                    stri = subli.Language;
                }
                //
                // Create New Category Dialog
                //
                var newCategory = new NewCategory();
                if (newCategory.ShowDialog() == DialogResult.OK)
                {
                    if (newCategory.txtCategoryName.Text == "")
                    {

                        XmlDocument doc = new XmlDocument();
                        string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                        doc.Load(pathqw + "/languages.xml");
                        XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
                        //
                        // Find error text in selected language, gert commented
                        //
                        foreach (XmlNode chldNode in node.ChildNodes)
                        {
                            string astring = chldNode.Attributes["name"].InnerXml.Trim();
                            if (astring == stri)
                            {
                                text1 = chldNode["invalid"].InnerText;
                                text2 = chldNode["category_name_cannot_be_blank"].InnerText;
                                //
                                //gert added
                                //
                                return;
                            }
                        }

                        MessageBox.Show(text2, text1, MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                        return;
                    }


                    var sCat = (from sc in subliCategories.Categories
                                where sc.CategoryName == newCategory.txtCategoryName.Text
                                select sc).FirstOrDefault();
                    //
                    // If exist category, gert commented
                    //
                    if (sCat != null)
                    {
                        XmlDocument doc = new XmlDocument();
                        string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                        doc.Load(pathqw + "/languages.xml");
                        XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
                        foreach (XmlNode chldNode in node.ChildNodes)
                        {
                            string astring = chldNode.Attributes["name"].InnerXml.Trim();
                            //
                            if (astring == stri)
                            {
                                text1 = chldNode["duplicate_record"].InnerText;
                                text2 = chldNode["category_already_exists"].InnerText;
                                //
                                // gert added
                                //
                                return;
                            }
                        }
                        MessageBox.Show(text2, text1, MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);

                    }
                    else
                    {
                        string categoryName = newCategory.txtCategoryName.Text.Trim();
                        addCategory(categoryName);
                        //
                        //gert commented, not save in just adding category
                        //

                        //saveCategory(newCategory.txtCategoryName.Text.Trim());

                        int oldSelIndex = listBox1.SelectedIndex;
                        //
                        // Check if exist same value, anyway let's skip this validating since dialog will do it.
                        //
                        /*
                        if (listBox1.Items.Cast<Object>().Any(x => x.ToString() == newValue))
                        {
                            return;
                        }
                        */
                        //
                        // ADD Value (Add new value and remove old value)
                        //

                        listBox1.Items.Insert(listBox1.Items.Count, categoryName);
                        //
                        // Change Database Value
                        //
                        //subliCategories.Categories[listBox1.Items.Count].CategoryName = categoryName;
                        //
                        // Restore old selected index
                        //
                        listBox1.SelectedIndex = oldSelIndex;
                        //
                        // 
                        //
                        lblChangedMark.Visible = true;
                    }
                } // new category dialog
            }
        }


        /// <summary>
        /// New Group Button Click, New window opened and New Group saved, tree node created
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNewGroup_Click(object sender, EventArgs e)
        {

            if (!access)
            {
                string returned_str = show_upgrade_Message();
                MessageBox.Show(returned_str.Split('%')[1].Trim(), returned_str.Split('%')[0].Trim(), MessageBoxButtons.OK);
            }
            else
            {
                if (curVersion == VersionType.PRO)
                {

                }
                else if (curVersion == VersionType.FREE)
                {
                    if (listBox1.SelectedIndex == 0) //default category
                    {
                        /*if (listviewGroup.Items.Count >= FreeDefaultGroupCount)
                        {
                            MessageBox.Show("Available groups count limited to " + FreeDefaultGroupCount + " in the default category.", "SubliDesk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }*/
                        
                            MessageBox.Show("Not allowed to add any group in the default category.", "SubliDesk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        
                    }
                    else if (listBox1.SelectedIndex >= 1) //custome category
                    {
                        if (listviewGroup.Items.Count >= FreeCustomeGroupCount)
                        {
                            MessageBox.Show("Available groups count limited to " + FreeCustomeGroupCount + " in the custom category.", "SubliDesk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select a category.", "SubliDesk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }


                string stri = "";
                string text1 = "";
                string text2 = "";
                var xml_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var subli1 = SubliDataManager.Read(xml_path + "\\Subli.xml");
                //subliCategories = subli1;
                if (subli1.Categories != null)
                {
                    var subli = subli1.Categories[0];
                    stri = subli.Language;
                }

                var newGroup = new NewGroup();
                if (newGroup.ShowDialog() == DialogResult.OK)
                {
                    if (newGroup.txtGroupName.Text == "")
                    {
                        XmlDocument doc = new XmlDocument();
                        string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                        doc.Load(pathqw + "/languages.xml");
                        XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
                        foreach (XmlNode chldNode in node.ChildNodes)
                        {
                            string astring = chldNode.Attributes["name"].InnerXml.Trim();
                            //
                            if (astring == stri)
                            {
                                text1 = chldNode["invalid"].InnerText;
                                text2 = chldNode["group_name_cannot_be_blank"].InnerText;
                            }
                        }
                        MessageBox.Show(text2, text1, MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                        return;
                    }

                    foreach (var cats in subliCategories.Categories)
                    {
                        var g = (from sc in cats.Groups
                                 where sc.GroupName == newGroup.txtGroupName.Text
                                 select sc).FirstOrDefault();
                        if (g != null)
                        {
                            XmlDocument doc = new XmlDocument();
                            string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                            doc.Load(pathqw + "/languages.xml");
                            XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
                            foreach (XmlNode chldNode in node.ChildNodes)
                            {
                                string astring = chldNode.Attributes["name"].InnerXml.Trim();
                                //
                                if (astring == stri)
                                {
                                    text1 = chldNode["duplicate_record"].InnerText;
                                    text2 = chldNode["group_already_exists_in"].InnerText;
                                }
                            }

                            MessageBox.Show(text2 + " " + cats.CategoryName, text1, MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    var cat = (from sc in subliCategories.Categories
                               where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()

                               select sc).FirstOrDefault();

                    var grp = (from sg in cat.Groups
                               where sg.GroupName == newGroup.txtGroupName.Text
                               select sg).FirstOrDefault();

                    var emptyGrp = (from sg in cat.Groups
                                    where sg.GroupName == ""
                                    select sg).FirstOrDefault();
                    if (emptyGrp != null)
                    {
                        cat.Groups.Remove(emptyGrp);
                        
                    }

                    if (grp != null)
                    {

                        XmlDocument doc = new XmlDocument();
                        string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                        doc.Load(pathqw + "/languages.xml");
                        XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
                        foreach (XmlNode chldNode in node.ChildNodes)
                        {
                            string astring = chldNode.Attributes["name"].InnerXml.Trim();
                            //
                            if (astring == stri)
                            {
                                text1 = chldNode["duplicate_record"].InnerText;
                                text2 = chldNode["group_already_exists_in_selected_cat"].InnerText;
                            }
                        }
                        MessageBox.Show(text2, text1, MessageBoxButtons.OK,
                                        MessageBoxIcon.Warning);
                        return;
                    }
                    string groupName = newGroup.txtGroupName.Text.Trim();
                    addGroup(cat, groupName);

                    //listBox1_SelectedIndexChanged(null, null);
                    //listBox2.Items.Insert(listBox2.Items.Count, groupName);
                    listviewGroup.Items.Add(new ListViewItem(groupName));
                    if (listviewGroup.SelectedIndices.Count == 0)
                    {
                        listviewGroup.Items[listviewGroup.Items.Count - 1].Selected = true;
                        listviewGroup.Select();
                    }
                    

                    //saveGroup();

                    lblChangedMark.Visible = true;
                }
            }
        }

        private void saveGroup()
        {
            LoadFormDefaults();
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
            LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());
        }

        private void addGroup(SubliCategory cat, string groupName)
        {
            cat.Groups.Add(new SubliGroup()
            {
                GroupName = groupName,
                SubliImage = new SubliImageEntity
                {
                    ImageFiles = new List<string>(),
                    SortingOrder = 1,
                    SplashDisplayForMilliSeconds = 1000,
                    SplashPosition = 1,
                    SplashingPeriodInSeconds = 1,
                    Opacity = 0,
                    Transparency = 0

                },
                SubliSuggestion = new SubliSuggestionsEntity
                {
                    Suggestions = new List<string>(),
                    SplashPosition = 1,
                    Opacity = 0,
                    Transparency = 0,
                    SelectedFont = new FontConverter().ConvertToString(new Label().Font),
                    BackgroundColor = Color.Transparent.Name,
                    FontColor = new Label().ForeColor.Name,
                    Language = cmbLanguage.SelectedValue.ToString(),
                    SplashDisplayForMilliSeconds = 1000,
                    SplashingPeriodInSeconds = 1
                }

            });
            lblCurrentCategory.Text = lblCurrentCategory.Text.Split('>')[0].Trim() + " > " + groupName;
        }

        /// <summary>
        /// delete Node Button Click Event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDeleteNode_Click(object sender, EventArgs e)
        {

            if (!access)
            {

                string returned_str = show_upgrade_Message();
                MessageBox.Show(returned_str.Split('%')[1].Trim(), returned_str.Split('%')[0].Trim(), MessageBoxButtons.OK);
            }
            else
            {
                try
                {
                    var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    switch (trvSuggestions.SelectedNode.Level)
                    {
                        case 0:
                            lblCurrentCategory.Text = trvSuggestions.SelectedNode.PrevNode.Text + " > " + trvSuggestions.SelectedNode.PrevNode.Nodes[0].Text;
                            DeleteNode(trvSuggestions.SelectedNode.Text, "", "", 0);
                            break;
                        case 1:
                            lblCurrentCategory.Text = trvSuggestions.SelectedNode.Parent.Text;
                            if (trvSuggestions.SelectedNode.Index > 0)
                                lblCurrentCategory.Text = trvSuggestions.SelectedNode.Parent.Text + " > " + trvSuggestions.SelectedNode.PrevNode.Text;
                            DeleteNode(trvSuggestions.SelectedNode.Parent.Text, trvSuggestions.SelectedNode.Text, "", 1);
                            break;
                        case 2:
                            lblCurrentCategory.Text = trvSuggestions.SelectedNode.Parent.Parent.Text + " > " + trvSuggestions.SelectedNode.Parent.Text;
                            if (trvSuggestions.SelectedNode.Index > 0)
                                lblCurrentCategory.Text = trvSuggestions.SelectedNode.Parent.Parent.Text + " > " + trvSuggestions.SelectedNode.Parent.Text + " > " + trvSuggestions.SelectedNode.PrevNode.Text;
                            DeleteNode(trvSuggestions.SelectedNode.Parent.Parent.Text, trvSuggestions.SelectedNode.Parent.Text, trvSuggestions.SelectedNode.Text, 2);
                            break;
                    }
                    SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
                    LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());

                }
                catch { }
            }
        }

        /// <summary>
        /// delete Selected treeNode
        /// </summary>
        /// <param name="category"></param>
        /// <param name="groupName"></param>
        /// <param name="suggestion"></param>
        /// <param name="level"></param>
        private void DeleteNode(string category, string groupName, string suggestion, int level)
        {
            try
            {

                var suc = (from sc in subliCategories.Categories
                           where sc.CategoryName == category
                           select sc).FirstOrDefault();
                if (level >= 1)
                {
                    var grp = (from gp in suc.Groups
                               where gp.GroupName == groupName
                               select gp).FirstOrDefault();
                    if (level == 2)
                    {
                        int sIndex = -1;
                        for (int k = 0; k < grp.SubliSuggestion.Suggestions.Count; k++)
                        {
                            if (grp.SubliSuggestion.Suggestions[k] == suggestion)
                                sIndex = k;
                        }
                        grp.SubliSuggestion.Suggestions.RemoveAt(sIndex);
                        return;
                    }
                    int gIndex = -1;
                    for (int j = 0; j < suc.Groups.Count; j++)
                    {
                        if (suc.Groups[j].GroupName == groupName)
                        {
                            gIndex = j;
                        }
                    }
                    suc.Groups.RemoveAt(gIndex);
                    return;
                }
                int cIndex = -1;
                for (int i = 0; i < subliCategories.Categories.Count; i++)
                {
                    if (subliCategories.Categories[i].CategoryName == category)
                        cIndex = i;
                }
                subliCategories.Categories.RemoveAt(cIndex);
            }
            catch { }
        }

        #endregion
        


        #region Start Splashing And Stop Splashing

        private System.Threading.Timer RefreshTimer;

        //
        // gert added for global font
        //
        public static Font globalFont = new Font(FontFamily.GenericSansSerif, 25);
        public static Color globalFontColor = Color.Silver;
        public static int screenPostion = 0;
        /// <summary>
        /// on Start button click, calls for function to start splashing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMimizeWindow_Click(object sender, EventArgs e)
        {
            //gert added, setting global options
            globalFont = lblSelectedFont.Font;
            globalFontColor = lblSelectedFont.ForeColor;
            screenPostion = Convert.ToInt32(cmbSplashingPosition.SelectedValue.ToString());

            bool_stop = false;
            this.WindowState = FormWindowState.Minimized;
            start_execute();
            this.Show();
            this.Activate();
        }
      
        /// <summary>
        /// Timer instance inintializes here for splashing
        /// </summary>
        private void start_execute()
        {
            RefreshTimer = new System.Threading.Timer(new System.Threading.TimerCallback(splashing), null, Convert.ToInt32(cmbSuggestionsPeriodSeconds.SelectedValue) * 1000, System.Threading.Timeout.Infinite);
            if (bool_stop)
            {
                RefreshTimer.Dispose();
            }
        }
        private void splashing(object sender)
        {//
            // gert modified
            //
            /*
            var suc = (from sc in subliCategories.Categories
                       where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                       select sc).FirstOrDefault();


            var sug = (from gp in suc.Groups
                       where gp.GroupName == lblCurrentCategory.Text.Split('>')[1].Trim()
                       select gp).FirstOrDefault();
            */

            //int catIndex = listBox1.SelectedIndex;
            //if (catIndex == -1) return;
            //if (listviewGroup.SelectedIndices.Count == 0) return;
            //int groupIndex = listviewGroup.SelectedIndices[0];

            //var suc = subliCategories.Categories[catIndex];
            //var sug = suc.Groups[groupIndex];

            try
            {
                RefreshTimer.Dispose();

                //
                // Each Affirmation
                //
                if (lblCurrentCategory.Text.Split('>').Length != 2)
                {
                    if (lblCurrentCategory.Text.Split('>')[2].Trim() == "Images")
                    {
                        foreach (var sug in mySelection)
                        {
                            var sugImgFiles = sug.SubliImage.ImageFiles;
                            if (curVersion == VersionType.PRO)
                            {

                            }
                            else if(curVersion == VersionType.FREE)
                            {
                                sugImgFiles = sugImgFiles.Take(Math.Min(sugImgFiles.Count, FreeAffirmationCount)).ToList();
                            }

                            foreach (var img in sugImgFiles)
                            {
                                if (img.Split(',')[3] == "y" && (!bool_stop))
                                {
                                    SplashScreen ss = new SplashScreen();
                                    Thread splashthread = new Thread(new ParameterizedThreadStart(ss.ShowSplashScreen));
                                    splashthread.IsBackground = true;
                                    splashthread.Start((object)new SubliCurrentImages { CurrentImage = img, SubliImagesEntity = sug.SubliImage });

                                    //
                                    // version 
                                    //
                                    if (curVersion == VersionType.PRO)
                                        Thread.Sleep(sug.SubliImage.SplashDisplayForMilliSeconds);
                                    else if (curVersion == VersionType.FREE)
                                        Thread.Sleep(globalDurationInMillisecondsFree * 1000);

                                    splashthread.Abort();
                                    ss.CloseSplashScreen();
                                    //version
                                    if (curVersion == VersionType.PRO)
                                        Thread.Sleep(sug.SubliImage.SplashingPeriodInSeconds * 1000);
                                    else if (curVersion == VersionType.FREE)
                                        Thread.Sleep(globalSplashingPeriodInSecondsFree * 1000);
                                }
                            }
                        }
                    }
                    else if (lblCurrentCategory.Text.Split('>')[2].Trim() == "Suggestions")
                    {
                        foreach (var sug in mySelection)
                        {
                            var subSugFiles = sug.SubliSuggestion.Suggestions;
                            if (curVersion == VersionType.PRO)
                            {

                            }
                            else if (curVersion == VersionType.FREE)
                            {
                                subSugFiles = subSugFiles.Take(Math.Min(subSugFiles.Count, FreeAffirmationCount)).ToList();
                            }
                            foreach (var sugg in subSugFiles)
                            {
                                if (!bool_stop)
                                {
                                    TextSplashScreen ss = new TextSplashScreen();
                                    Thread splashthread = new Thread(new ParameterizedThreadStart(ss.ShowSplashScreen));
                                    splashthread.IsBackground = true;
                                    splashthread.Start((object)new SubliCurrentSuggestions { CurrentSuggestion = sugg, SubliSuggestionsEntity = sug.SubliSuggestion });
                                    //version
                                    if (curVersion == VersionType.PRO)
                                        Thread.Sleep(sug.SubliSuggestion.SplashDisplayForMilliSeconds);
                                    else if (curVersion == VersionType.FREE)
                                        Thread.Sleep(SubliMasterMain.globalDurationInMillisecondsFree);

                                    splashthread.Abort();
                                    ss.CloseSplashScreen();
                                    //version
                                    if (curVersion == VersionType.PRO)
                                        Thread.Sleep(sug.SubliSuggestion.SplashingPeriodInSeconds * 1000);
                                    else if (curVersion == VersionType.FREE)
                                        Thread.Sleep(SubliMasterMain.globalSplashingPeriodInSecondsFree * 1000);
                                }
                            }
                        }
                    }
                }
                //
                // All Affirmation
                //
                else
                {
                    foreach (var sug in mySelection)
                    {
                        var sugImgFiles = sug.SubliImage.ImageFiles;
                        var subSugFiles = sug.SubliSuggestion.Suggestions;
                        
                        if (curVersion == VersionType.PRO)
                        {

                        }
                        else if (curVersion == VersionType.FREE)
                        {
                            sugImgFiles = sugImgFiles.Take(Math.Min(sugImgFiles.Count, FreeAffirmationCount)).ToList();
                            subSugFiles = subSugFiles.Take(Math.Min(subSugFiles.Count, FreeAffirmationCount)).ToList();
                        }
                       

                        foreach (var img in sugImgFiles)
                        {
                            if (img.Split(',')[3] == "y" && (!bool_stop))
                            {
                                SplashScreen ss = new SplashScreen();
                                Thread splashthread = new Thread(new ParameterizedThreadStart(ss.ShowSplashScreen));
                                splashthread.IsBackground = true;
                                splashthread.Start((object)new SubliCurrentImages { CurrentImage = img, SubliImagesEntity = sug.SubliImage });


                                //
                                // version 
                                //
                                if (curVersion == VersionType.PRO)
                                    Thread.Sleep(sug.SubliImage.SplashDisplayForMilliSeconds);
                                else if (curVersion == VersionType.FREE)
                                    Thread.Sleep(globalDurationInMillisecondsFree);

                                splashthread.Abort();
                                ss.CloseSplashScreen();


                                //version
                                if (curVersion == VersionType.PRO)
                                    Thread.Sleep(sug.SubliImage.SplashingPeriodInSeconds * 1000);
                                else if (curVersion == VersionType.FREE)
                                    Thread.Sleep(globalSplashingPeriodInSecondsFree * 1000);
                            }
                        }
                        foreach (var sugg in subSugFiles)
                        {
                            if (!bool_stop)
                            {
                                TextSplashScreen ss = new TextSplashScreen();
                                Thread splashthread = new Thread(new ParameterizedThreadStart(ss.ShowSplashScreen));
                                splashthread.IsBackground = true;
                                splashthread.Start((object)new SubliCurrentSuggestions { CurrentSuggestion = sugg, SubliSuggestionsEntity = sug.SubliSuggestion });

                                //version
                                if (curVersion == VersionType.PRO)
                                    Thread.Sleep(sug.SubliSuggestion.SplashDisplayForMilliSeconds);
                                else if (curVersion == VersionType.FREE)
                                    Thread.Sleep(globalDurationInMillisecondsFree);

                                splashthread.Abort();
                                ss.CloseSplashScreen();
                                //version
                                if (curVersion == VersionType.PRO)
                                    Thread.Sleep(sug.SubliSuggestion.SplashingPeriodInSeconds * 1000);
                                else if (curVersion == VersionType.FREE)
                                    Thread.Sleep(globalSplashingPeriodInSecondsFree * 1000);
                            }
                        }
                    }
                }

                start_execute();
            }

            catch { }
        }
        
        /// <summary>
        /// on stop button click, splashing stop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStopAll_Click(object sender, EventArgs e)
        {
            RefreshTimer.Dispose();
            bool_stop = true;
            /* foreach (var c in subliCategories.Categories)
                {
                foreach (var g in c.Groups)
                {
                    g.SubliImage.StopImageSplash();
                    g.SubliSuggestion.StopSuggestionSplash();
                }
                }*/
        }

        #endregion
        


        #region Sorting images and suggestions

        /// <summary>
        /// sort images and suggestions by selected index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbSortingOrder_SelectedIndexChanged(object sender, EventArgs e)
        {

            /*Sort By Type*/
            if (cmbSortingOrder.SelectedIndex == 2)
            {

                var imgs = new List<string>();
                foreach (var item in lstViewItems.Items)
                {
                    if (((ListViewItem)item).Checked == true)
                    {
                        string str = ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[0].Text + "," + ((ListViewItem)item).SubItems[2].Text + ",y";
                        imgs.Add(str);
                    }
                    else if (((ListViewItem)item).Checked == false)
                    {
                        try
                        {
                            string str = ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[0].Text + "," + ((ListViewItem)item).SubItems[2].Text + ",n";
                            imgs.Add(str);
                        }
                        catch { }
                    }
                }
                lstViewItems.Items.Clear();
                imgs.Sort();
                foreach (var img in imgs)
                {
                    var item =
                        new ListViewItem(new[]
                                             {
                                                 img.Split(',')[1], img.Split(',')[0], img.Split(',')[2]
                                             });
                    if (img.Split(',')[3] == "y")
                    {
                        item.Checked = true;
                    }
                    lstViewItems.Items.Add(item);
                }
            }

            /*Sort By File Size*/

            else if (cmbSortingOrder.SelectedIndex == 3)
            {

                var imgs = new List<string>();
                foreach (var item in lstViewItems.Items)
                {
                    if (((ListViewItem)item).Checked == true)
                    {
                        string str = ((ListViewItem)item).SubItems[2].Text + "," + ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[0].Text + ",y";
                        imgs.Add(str);
                    }
                    else if (((ListViewItem)item).Checked == false)
                    {
                        try
                        {
                            string str = ((ListViewItem)item).SubItems[2].Text + "," + ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[0].Text + ",n";
                            imgs.Add(str);
                        }
                        catch { }
                    }
                }
                lstViewItems.Items.Clear();
                imgs.Sort();
                foreach (var img in imgs)
                {
                    var item =
                        new ListViewItem(new[]
                                             {
                                                 img.Split(',')[2], img.Split(',')[1], img.Split(',')[0]
                                             });
                    if (img.Split(',')[3] == "y")
                    {
                        item.Checked = true;
                    }
                    lstViewItems.Items.Add(item);
                }
            }


            /*Sort By Name*/

            else
            {
                var imgs = new List<string>();
                foreach (var item in lstViewItems.Items)
                {
                    if (((ListViewItem)item).Checked == true)
                    {
                        string str = ((ListViewItem)item).SubItems[0].Text + "," + ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[2].Text + ",y";
                        imgs.Add(str);
                    }
                    else if (((ListViewItem)item).Checked == false)
                    {
                        try
                        {
                            string str = ((ListViewItem)item).SubItems[0].Text + "," + ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[2].Text + ",n";
                            imgs.Add(str);
                        }
                        catch { }
                    }
                }
                lstViewItems.Items.Clear();
                imgs.Sort();
                foreach (var img in imgs)
                {
                    var item =
                        new ListViewItem(new[]
                                             {
                                                 img.Split(',')[0], img.Split(',')[1], img.Split(',')[2]
                                             });
                    if (img.Split(',')[3] == "y")
                    {
                        item.Checked = true;
                    }
                    lstViewItems.Items.Add(item);
                }

                var suggs = new List<String>();
                foreach (var sugg in rtxtSuggestions.Text.Split('\n'))
                {
                    if (sugg.Trim().Length > 0)
                        suggs.Add(sugg);
                }
                suggs.Sort();
                rtxtSuggestions.Text = "";

                foreach (var s in suggs)
                {
                    rtxtSuggestions.Text += s + "\n";
                }
            }

        }

        #endregion



        #region categories TreeNode Drag Drop section [ not using currently]

        /// <summary>
        /// categories tree node drag enter allowed effect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trvSuggestions_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
        }

        /// <summary>
        /// categories tree node item dragged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trvSuggestions_ItemDrag(object sender, System.Windows.Forms.ItemDragEventArgs e)
        {

        }

        /// <summary>
        /// categories tree node item droped event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trvSuggestions_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            try
            {
                Point targetPoint = trvSuggestions.PointToClient(new Point(e.X, e.Y));
                TreeNode targetNode = trvSuggestions.GetNodeAt(targetPoint);
                TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                if (!draggedNode.Equals(targetNode) && !ContainsNode(draggedNode, targetNode))
                {
                    if (e.Effect == DragDropEffects.Move)
                    {
                        draggedNode.Remove();
                        targetNode.Nodes.Add(draggedNode);
                    }
                    else if (e.Effect == DragDropEffects.Copy)
                    {
                        TreeNode parentNode = (TreeNode)draggedNode.Parent;
                        TreeNode child = targetNode.Parent;
                        TreeNode parent = child.Parent;

                        var suc = (from sc in subliCategories.Categories
                                   where sc.CategoryName == parent.Text.Trim()
                                   select sc).FirstOrDefault();
                        var sug = (from gp in suc.Groups
                                   where gp.GroupName == child.Text.Trim()
                                   select gp).FirstOrDefault();


                        if (parentNode.Text == "Images" && targetNode.Text == "Images")
                        {

                            lstViewItems.Items.Clear();
                            foreach (var img in sug.SubliImage.ImageFiles)
                            {
                                var item =
                                        new ListViewItem(new[]
                                                 {
                                                     img.Split(',')[0], img.Split(',')[1], img.Split(',')[2]
                                                 });
                                if (img.Split(',')[3] == "y")
                                {
                                    item.Checked = true;
                                }
                                lstViewItems.Items.Add(item);
                            }

                            var itemw =
                                new ListViewItem(new[]
                                            {
                                                srt.Split(',')[0], srt.Split(',')[1], srt.Split(',')[2]
                                            });
                            if (srt.Split(',')[3] == "y")
                            {
                                itemw.Checked = true;
                            }
                            lstViewItems.Items.Add(itemw);

                            draggedNode.Text = srt.Split(',')[0].Trim();
                            targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
                        }
                        else if (parentNode.Text == "Suggestions" && targetNode.Text == "Suggestions")
                        {
                            rtxtSuggestions.Text = "";
                            foreach (var sugg in sug.SubliSuggestion.Suggestions)
                            {
                                rtxtSuggestions.Text += sugg + "\n";
                            }
                            rtxtSuggestions.Text += srt + "\n";
                            draggedNode.Text = srt.Split(',')[0].Trim();
                            targetNode.Nodes.Add((TreeNode)draggedNode.Clone());
                            tabControls.SelectedIndex = 1;
                        }
                        else
                        {
                        }
                    }
                    targetNode.Expand();
                }
            }
            catch { }
        }

        /// <summary>
        /// check if node have parent
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns>true false</returns>
        private bool ContainsNode(TreeNode node1, TreeNode node2)
        {
            // Check the parent node of the second node. 
            if (node2.Parent == null) return false;
            if (node2.Parent.Equals(node1)) return true;

            // If the parent node is not null or equal to the first node,  
            // call the ContainsNode method recursively using the parent of  
            // the second node. 
            return ContainsNode(node1, node2.Parent);
        }

        /// <summary>
        /// categories tree node hover event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trvSuggestions_DragOver(object sender, DragEventArgs e)
        {

            Point targetPoint = trvSuggestions.PointToClient(new Point(e.X, e.Y));
            trvSuggestions.SelectedNode = trvSuggestions.GetNodeAt(targetPoint);
        }


        #endregion



        /// <summary>
        /// on Start Click, splashing display in Test Area
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTest_Click(object sender, EventArgs e)
        {
            //gert added, setting global position
            globalFont = lblSelectedFont.Font;
            globalFontColor = lblSelectedFont.ForeColor;
            screenPostion = Convert.ToInt32(cmbSplashingPosition.SelectedValue.ToString());


            this.Cursor = Cursors.WaitCursor;
            var suc = (from sc in subliCategories.Categories
                       where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                       select sc).FirstOrDefault();


            var sug = (from gp in suc.Groups
                       where gp.GroupName == lblCurrentCategory.Text.Split('>')[1].Trim()
                       select gp).FirstOrDefault();
            var seconds = Convert.ToInt32(cmbSuggestionsPeriodSeconds.SelectedValue) * 1000;
            var milliseconds = Convert.ToInt32(cmbSuggestionDurationInMilliseconds.SelectedValue);
            if (lblCurrentCategory.Text.Split('>').Length != 2)
            {
                if (lblCurrentCategory.Text.Split('>')[2].Trim() == "Images")
                {
                    foreach (var img in sug.SubliImage.ImageFiles)
                    {
                        if (img.Split(',')[3] == "y")
                        {
                            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                            System.Drawing.Image image = System.Drawing.Image.FromFile(path + "\\" + img.Split(',')[0]);
                            int opacity = (255 - trkOpacity.Value);
                            Bitmap bmp = (Bitmap)image.Clone();
                            PixelFormat pxf = PixelFormat.Format32bppArgb;
                            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
                            IntPtr ptr = bmpData.Scan0;
                            int numBytes = bmp.Width * bmp.Height * bytesPerPixel;
                            byte[] argbValues = new byte[numBytes];
                            System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, numBytes);
                            if (sug.SubliImage.Transparency == 1)
                            {
                                opacity = 100;
                            }
                            for (int counter = 0; counter < argbValues.Length; counter += bytesPerPixel)
                            {
                                if (argbValues[counter + bytesPerPixel - 1] == 0)
                                    continue;

                                int pos = 0;
                                pos++;
                                pos++;
                                pos++;

                                if (opacity == 0) { opacity = 1; }
                                argbValues[counter + pos] = (byte)(argbValues[counter + pos] * opacity);
                            }
                            System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, numBytes);
                            bmp.UnlockBits(bmpData);
                            image = bmp;
                            Thread.Sleep(seconds);

                            /* gert commented
                            test_pictureBox.Height = 80;
                            test_pictureBox.Width = 320;

                            if (image.Height <= 80)
                            {
                                test_pictureBox.Height = 80;
                            }

                            if (image.Width <= 320)
                            {
                                test_pictureBox.Width = image.Width;
                            }
                            */
                            if (sug.SubliImage.Transparency == 1)
                            {

                            }
                            test_pictureBox.Image = image;
                            Application.DoEvents();
                            Thread.Sleep(milliseconds);
                            test_pictureBox.Image = null;
                            Application.DoEvents();
                        }
                    }
                }
                else if (lblCurrentCategory.Text.Split('>')[2].Trim() == "Suggestions")
                {
                    
                    foreach (var sugg in sug.SubliSuggestion.Suggestions)
                    {
                        //gert
                        Application.DoEvents();

                        System.Drawing.Image image = TextToBitmap(sugg, new FontConverter().ConvertFromString(sug.SubliSuggestion.SelectedFont.ToString()) as Font, Color.FromName(sug.SubliSuggestion.FontColor), Color.FromName(sug.SubliSuggestion.BackgroundColor));
                        int opacity = (255 - trkOpacity.Value);
                        Bitmap bmp = (Bitmap)image.Clone();
                        PixelFormat pxf = PixelFormat.Format32bppArgb;
                        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                        BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
                        IntPtr ptr = bmpData.Scan0;
                        int numBytes = bmp.Width * bmp.Height * bytesPerPixel;
                        byte[] argbValues = new byte[numBytes];
                        System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, numBytes);
                        if (sug.SubliImage.Transparency == 1)
                        {
                            opacity = 100;
                        }
                        for (int counter = 0; counter < argbValues.Length; counter += bytesPerPixel)
                        {
                            if (argbValues[counter + bytesPerPixel - 1] == 0)
                                continue;

                            int pos = 0;
                            pos++;
                            pos++;
                            pos++;

                            if (opacity == 0) { opacity = 1; }
                            argbValues[counter + pos] = (byte)(argbValues[counter + pos] * opacity);
                        }
                        System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, numBytes);
                        bmp.UnlockBits(bmpData);
                        image = bmp;
                        Thread.Sleep(seconds);

                        // gert commented
                        /*
                        test_pictureBox.Height = 80;
                        test_pictureBox.Width = 320;
                        if (image.Height <= 80)
                        {
                            test_pictureBox.Height = 80;
                        }

                        if (image.Width <= 320)
                        {
                            test_pictureBox.Width = image.Width;
                        }
                        */
                        test_pictureBox.Image = image;
                        Application.DoEvents();
                        Thread.Sleep(milliseconds);
                        test_pictureBox.Image = null;
                        Application.DoEvents();
                    }
                }
            }
            else
            {
                foreach (var img in sug.SubliImage.ImageFiles)
                {
                    if (img.Split(',')[3] == "y")
                    {
                        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        System.Drawing.Image image = System.Drawing.Image.FromFile(path + "\\" + img.Split(',')[0]);
                        int opacity = (255 - trkOpacity.Value);
                        Bitmap bmp = (Bitmap)image.Clone();
                        PixelFormat pxf = PixelFormat.Format32bppArgb;
                        Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                        BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
                        IntPtr ptr = bmpData.Scan0;
                        int numBytes = bmp.Width * bmp.Height * bytesPerPixel;
                        byte[] argbValues = new byte[numBytes];
                        System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, numBytes);
                        if (sug.SubliImage.Transparency == 1)
                        {
                            opacity = 100;
                        }
                        for (int counter = 0; counter < argbValues.Length; counter += bytesPerPixel)
                        {
                            if (argbValues[counter + bytesPerPixel - 1] == 0)
                                continue;

                            int pos = 0;
                            pos++;
                            pos++;
                            pos++;

                            if (opacity == 0) { opacity = 1; }
                            argbValues[counter + pos] = (byte)(argbValues[counter + pos] * opacity);
                        }
                        System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, numBytes);
                        bmp.UnlockBits(bmpData);
                        image = bmp;
                        Thread.Sleep(seconds);
                        test_pictureBox.Height = 80;
                        test_pictureBox.Width = 320;
                        if (image.Height <= 80)
                        {
                            test_pictureBox.Height = 80;
                        }

                        if (image.Width <= 320)
                        {
                            test_pictureBox.Width = image.Width;
                        }
                        if (sug.SubliImage.Transparency == 1)
                        {

                        }
                        test_pictureBox.Image = image;
                        Application.DoEvents();
                        Thread.Sleep(milliseconds);
                        test_pictureBox.Image = null;
                        Application.DoEvents();
                    }
                }
                foreach (var sugg in sug.SubliSuggestion.Suggestions)
                {
                    System.Drawing.Image image = TextToBitmap(sugg, new FontConverter().ConvertFromString(sug.SubliSuggestion.SelectedFont.ToString()) as Font, Color.FromName(sug.SubliSuggestion.FontColor), Color.FromName(sug.SubliSuggestion.BackgroundColor));
                    int opacity = (255 - trkOpacity.Value);
                    Bitmap bmp = (Bitmap)image.Clone();
                    PixelFormat pxf = PixelFormat.Format32bppArgb;
                    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                    BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);
                    IntPtr ptr = bmpData.Scan0;
                    int numBytes = bmp.Width * bmp.Height * bytesPerPixel;
                    byte[] argbValues = new byte[numBytes];
                    System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, numBytes);
                    if (sug.SubliImage.Transparency == 1)
                    {
                        opacity = 100;
                    }
                    for (int counter = 0; counter < argbValues.Length; counter += bytesPerPixel)
                    {
                        if (argbValues[counter + bytesPerPixel - 1] == 0)
                            continue;

                        int pos = 0;
                        pos++;
                        pos++;
                        pos++;

                        if (opacity == 0) { opacity = 1; }
                        argbValues[counter + pos] = (byte)(argbValues[counter + pos] * opacity);
                    }
                    System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, numBytes);
                    bmp.UnlockBits(bmpData);
                    image = bmp;
                    Thread.Sleep(seconds);
                    test_pictureBox.Height = 80;
                    test_pictureBox.Width = 320;
                    if (image.Height <= 80)
                    {
                        test_pictureBox.Height = 80;
                    }

                    if (image.Width <= 320)
                    {
                        test_pictureBox.Width = image.Width;
                    }
                    test_pictureBox.Image = image;
                    Application.DoEvents();
                    Thread.Sleep(milliseconds);
                    test_pictureBox.Image = null;
                    Application.DoEvents();

                }

            }
            this.Cursor = Cursors.Default;
        }


        /// <summary>
        /// system tray icon click event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIconSubli_DoubleClick(object sender, EventArgs e)
        {
            Show();
            this.Activate();
            this.BringToFront();
            WindowState = FormWindowState.Maximized;
        }



        /// <summary>
        /// Window Load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubliMasterMain_Load(object sender, EventArgs e)
        {
            if (rkApp.GetValue("SubliDesk") != null)
            {
                trayMenuStartAutomatically.Checked = true;
                cmbStartUpMode.SelectedValue = 1;
            }
            LoadSuggestions();
            fontDialogSubli.Font = lblSelectedFont.Font;
            fontDialogSubli.Color = lblSelectedFont.ForeColor;
            if (sublicCategoryForCopy == null)
            {
                treeViewToolStripMenuItemCopy.Enabled = true;
                treeViewToolStripMenuItemPasteAppend.Enabled = false;
                treeViewToolStripMenuItemPasteOverwrite.Enabled = false;
            }
        }

        /// <summary>
        /// Treenode Load on category change and save 
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="categoryToSelect"></param>
        public void LoadSuggestionsTree(SubliCategories sc, string categoryToSelect)
        {
            trvSuggestions.Nodes.Clear();
            //
            //categories, gert
            //
            listBox1.Items.Clear();
            if (curVersion == VersionType.PRO)
            {
                listBox1.Items.AddRange(sc.Categories.Select(x => x.CategoryName).ToArray());
            }
            else if (curVersion == VersionType.FREE)
            {
                int curGroupCount = sc.Categories.Count;
                listBox1.Items.AddRange(sc.Categories.GetRange(0, Math.Min(curGroupCount, FreeCategoryCount)).Select(x => x.CategoryName).ToArray());
            }

            
            if (listBox1.Items.Count > 0)
            {
                listBox1.SelectedIndex = 0;
            }

            foreach (var subli in sc.Categories)
            {
                var rootNode = new TreeNode(subli.CategoryName);
                //
                // groups
                //
                foreach (var g in subli.Groups)
                {
                    if (g.GroupName == null) g.GroupName = "";
                    if (g.GroupName.Length > 0)
                    {
                        var groupNode = new TreeNode(g.GroupName);
                        var sugest = new TreeNode("Suggestions");
                        groupNode.Nodes.Add(sugest);
                        var images = new TreeNode("Images");
                        groupNode.Nodes.Add(images);
                        foreach (var sug in g.SubliSuggestion.Suggestions)
                        {
                            if (sug.Trim().Length > 1)
                            {
                                var suggNode = new TreeNode(sug);
                                sugest.Nodes.Add(suggNode);
                            }
                        }
                        foreach (var imgg in g.SubliImage.ImageFiles)
                        {
                            if (imgg.Trim().Length > 1)
                            {
                                string img_name = imgg.Split(',')[0].Trim();
                                var suggNode = new TreeNode(img_name);
                                images.Nodes.Add(suggNode);
                            }
                        }
                        rootNode.Nodes.Add(groupNode);
                    }
                }
                trvSuggestions.Nodes.Add(rootNode);

                trvSuggestions.ExpandAll();
            }

            foreach (TreeNode tr in trvSuggestions.Nodes)
            {
                if (tr.Text == categoryToSelect)
                {
                    foreach (TreeNode chldNode in tr.Nodes)
                    {
                        //if (chldNode.Name == lblCurrentCategory.Text.Split('>')[1].Trim())
                        {
                        //    trvSuggestions.SelectedNode = chldNode;
                        //    trvSuggestions.Focus();
                        }
                    }
                }
            }

        }

        /// <summary>
        /// form Window resize event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubliMasterMain_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                notifyIconSubli.Visible = true;
                notifyIconSubli.ShowBalloonTip(500);
                this.Hide();
            }
            else if (this.WindowState == FormWindowState.Normal)
            {
                notifyIconSubli.Visible = false;
            }

            listViewAffImages.Columns[0].Width = this.Width - 800;
            browser.Location = new Point(20, 20);
            browser.Size = new Size(gboxIntroAndNews.Width-40, gboxIntroAndNews.Height-40);

        }

        private void ctxTrayMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == trayMenuSettings)
            {
                Show();
                this.Activate();
                this.BringToFront();
                WindowState = FormWindowState.Normal;

            }
            else if (e.ClickedItem == TrayMenuExit)
            {
                Application.Exit();
            }

        }

        /// <summary>
        /// Binding data to controls
        /// </summary>
        private void FillAndBindLists()
        {
            switch (curVersion)
            {
                case VersionType.FREE:
                    FillAndBindListsFree();
                    break;
                case VersionType.PRO:
                    FillAndBindListsPro();
                    break;
                default:
                    FillAndBindListsFree();
                    break;
            }
        }

        private void FillAndBindListsPro()
        {
            var suggPeriod = new List<SplashingPeriod>();

            for (int i = 0; i <= 120; i++)
            {
                suggPeriod.Add(new SplashingPeriod { DisplayMember = i.ToString(), ValueMember = i });
            }

            cmbSuggestionsPeriodSeconds.ValueMember = "ValueMember";
            cmbSuggestionsPeriodSeconds.DisplayMember = "DisplayMember";
            cmbSuggestionsPeriodSeconds.DataSource = suggPeriod;

            var sugDuration = new List<SplashingPeriod>();

            for (int i = 10; i <= 1000; i++)
            {
                sugDuration.Add(new SplashingPeriod { DisplayMember = i.ToString(), ValueMember = i });
            }
            cmbSuggestionDurationInMilliseconds.ValueMember = "ValueMember";
            cmbSuggestionDurationInMilliseconds.DisplayMember = "DisplayMember";
            cmbSuggestionDurationInMilliseconds.DataSource = sugDuration;

            var splashingPosition = new List<SplashingPosition>
                                        {
                                            new SplashingPosition {DisplayMember = "Centered at screen", ValueMember = 1},
                                            new SplashingPosition {DisplayMember = "Random at screen", ValueMember = 2}
                                        };
            cmbSplashingPosition.ValueMember = "ValueMember";
            cmbSplashingPosition.DisplayMember = "DisplayMember";
            cmbSplashingPosition.DataSource = splashingPosition;

            var splashSortOrder = new List<SplashSortOrder>
                                        {
                                            new SplashSortOrder {DisplayMember = "Random Order", ValueMember = 1},
                                            new SplashSortOrder {DisplayMember = "Sort By Name", ValueMember = 2},
                                            new SplashSortOrder {DisplayMember = "Sort By Type", ValueMember = 3},
                                            new SplashSortOrder {DisplayMember = "Sort By File Size", ValueMember = 4}
                                        };
            cmbSortingOrder.ValueMember = "ValueMember";
            cmbSortingOrder.DisplayMember = "DisplayMember";
            cmbSortingOrder.DataSource = splashSortOrder;

            var startUpMode = new List<StartUpMode>
                                        {
                                            new StartUpMode {DisplayMember = "Autostart", ValueMember = 1},
                                            new StartUpMode {DisplayMember = "Manual", ValueMember = 2}
                                        };
            cmbStartUpMode.ValueMember = "ValueMember";
            cmbStartUpMode.DisplayMember = "DisplayMember";
            cmbStartUpMode.DataSource = startUpMode;

            ArrayList colorList = new ArrayList();
            Type colorType = typeof(System.Drawing.Color);
            PropertyInfo[] propInfoList = colorType.GetProperties(BindingFlags.Static |
                                          BindingFlags.DeclaredOnly | BindingFlags.Public);
            var bgColour = new List<GenericList>();

            foreach (PropertyInfo c in propInfoList)
            {
                bgColour.Add(new GenericList
                {
                    DisplayMember = c.Name,
                    ValueMember = c.Name
                });
                //this.cmbBackgroundColour.Items.Add(c.Name);
            }
            cmbBackgroundColour.ValueMember = "ValueMember";
            cmbBackgroundColour.DisplayMember = "DisplayMember";
            cmbBackgroundColour.DataSource = bgColour;

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
            doc.Save(path + "/languages.xml");


            cmbLanguage.ValueMember = "ValueMember";
            cmbLanguage.DisplayMember = "DisplayMember";
            cmbLanguage.DataSource = langList;
        }

        private void FillAndBindListsFree()
        {
            var suggPeriod = new List<SplashingPeriod>();

            {
                int i = 5;
                suggPeriod.Add(new SplashingPeriod { DisplayMember = i.ToString(), ValueMember = i });
            }

            cmbSuggestionsPeriodSeconds.ValueMember = "ValueMember";
            cmbSuggestionsPeriodSeconds.DisplayMember = "DisplayMember";
            cmbSuggestionsPeriodSeconds.DataSource = suggPeriod;

            var sugDuration = new List<SplashingPeriod>();

            {
                int i = 271;
                sugDuration.Add(new SplashingPeriod { DisplayMember = i.ToString(), ValueMember = i });
            }

            cmbSuggestionDurationInMilliseconds.ValueMember = "ValueMember";
            cmbSuggestionDurationInMilliseconds.DisplayMember = "DisplayMember";
            cmbSuggestionDurationInMilliseconds.DataSource = sugDuration;

            var splashingPosition = new List<SplashingPosition>
                                        {
                                            new SplashingPosition {DisplayMember = "Centered at screen", ValueMember = 1}
                                        };

            cmbSplashingPosition.ValueMember = "ValueMember";
            cmbSplashingPosition.DisplayMember = "DisplayMember";
            cmbSplashingPosition.DataSource = splashingPosition;

            var splashSortOrder = new List<SplashSortOrder>
                                        {
                                            new SplashSortOrder {DisplayMember = "Random Order", ValueMember = 1},
                                            new SplashSortOrder {DisplayMember = "Sort By Name", ValueMember = 2},
                                            new SplashSortOrder {DisplayMember = "Sort By Type", ValueMember = 3},
                                            new SplashSortOrder {DisplayMember = "Sort By File Size", ValueMember = 4}
                                        };
            cmbSortingOrder.ValueMember = "ValueMember";
            cmbSortingOrder.DisplayMember = "DisplayMember";
            cmbSortingOrder.DataSource = splashSortOrder;

            var startUpMode = new List<StartUpMode>
                                        {
                                            new StartUpMode {DisplayMember = "Autostart", ValueMember = 1},
                                            new StartUpMode {DisplayMember = "Manual", ValueMember = 2}
                                        };
            cmbStartUpMode.ValueMember = "ValueMember";
            cmbStartUpMode.DisplayMember = "DisplayMember";
            cmbStartUpMode.DataSource = startUpMode;

            ArrayList colorList = new ArrayList();
            Type colorType = typeof(System.Drawing.Color);
            PropertyInfo[] propInfoList = colorType.GetProperties(
                BindingFlags.Static |
                BindingFlags.DeclaredOnly | 
                BindingFlags.Public);
            var bgColour = new List<GenericList>();
            /*
            foreach (PropertyInfo c in propInfoList)
            {
                bgColour.Add(new GenericList
                {
                    DisplayMember = c.Name,
                    ValueMember = c.Name
                });
                //this.cmbBackgroundColour.Items.Add(c.Name);
            }
            */
            bgColour.Add(new GenericList
            {
                DisplayMember = "Transparent",
                ValueMember = "Transparent"
            });

            cmbBackgroundColour.ValueMember = "ValueMember";
            cmbBackgroundColour.DisplayMember = "DisplayMember";
            cmbBackgroundColour.DataSource = bgColour;

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
            doc.Save(path + "/languages.xml");


            cmbLanguage.ValueMember = "ValueMember";
            cmbLanguage.DisplayMember = "DisplayMember";
            cmbLanguage.DataSource = langList;
        }

        private void txtSplasingTime_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }


        /// <summary>
        /// Startup Dropdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trayMenuStartAutomatically_CheckedChanged(object sender, EventArgs e)
        {
            if (trayMenuStartAutomatically.Checked)
            {
                rkApp.SetValue("SubliDesk", Application.ExecutablePath.ToString());
            }
            else
            {
                rkApp.DeleteValue("SubliDesk", false);
            }
        }

        private void cmbBackgroundColour_DrawItem(object sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            Rectangle rect = e.Bounds;
            if (e.Index >= 0)
            {
                string n = ((ComboBox)sender).Items[e.Index].ToString();
                Font f = new Font("Arial", 9, FontStyle.Regular);
                Color c = Color.FromName(n);
                Brush b = new SolidBrush(c);
                g.DrawString(n, f, Brushes.Black, rect.X, rect.Top);
                g.FillRectangle(b, rect.X + 110, rect.Y + 5,
                                rect.Width - 10, rect.Height - 10);
            }
        }

        /// <summary>
        /// Change Font 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChangeFont_Click(object sender, EventArgs e)
        {
            try
            {
                fontDialogSubli.Font = lblSelectedFont.Font;
                fontDialogSubli.Color = lblSelectedFont.ForeColor;

                fontDialogSubli.AllowScriptChange = false;
                fontDialogSubli.AllowVectorFonts = false;
                fontDialogSubli.AllowVerticalFonts = false;
                if (fontDialogSubli.ShowDialog() == DialogResult.OK)
                {
                    lblSelectedFont.Font = fontDialogSubli.Font;
                    lblSelectedFont.ForeColor = fontDialogSubli.Color;

                    rtxtSuggestions.Font = fontDialogSubli.Font;
                    rtxtSuggestions.ForeColor = fontDialogSubli.Color;

                    lblChangedMark.Visible = true;
                }
            }
            catch (Exception ex) { }
        }

        /// <summary>
        /// load default values from xml
        /// </summary>
        private void LoadFormDefaults()
        {

            cmbSuggestionsPeriodSeconds.SelectedValue = 1;
            cmbSuggestionDurationInMilliseconds.SelectedValue = 1000;


            cmbSplashingPosition.SelectedValue = 1;
            trkOpacity.Value = 0;
            cmbSortingOrder.SelectedValue = 1;

            lblSelectedFont.Font = rtxtSuggestions.Font = new Label().Font;

            lblSelectedFont.ForeColor =
                rtxtSuggestions.ForeColor = new Label().ForeColor;
            lstViewItems.Items.Clear();
            /*
            txtCategory.Text = "";
            txtGroup.Text = "";
            */

            rtxtSuggestions.Text = "";
        }

        /// <summary>
        /// onChange of treenode call loadsuggestions function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trvSuggestions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                //btnNewGroup.Enabled = true; //gert commented
                if (e.Node.Text == "Default")
                    btnDeleteNode.Enabled = false;
                else
                    btnDeleteNode.Enabled = true;
                lblCurrentCategory.Text = e.Node.Text;
                if (e.Node.FirstNode != null)
                    LoadSuggestions(e.Node.Text, e.Node.FirstNode.Text, "Images", 0);
            }
            else if (e.Node.Level == 1)
            {
                //btnNewGroup.Enabled = false; //gert commented
                if (e.Node.Text == "Default" && e.Node.Parent.Text == "Default")
                    btnDeleteNode.Enabled = false;
                else
                    btnDeleteNode.Enabled = true;
                lblCurrentCategory.Text = e.Node.Parent.Text + " > " + e.Node.Text;
                LoadSuggestions(e.Node.Parent.Text, e.Node.Text, "Images", 0);
            }
            else if (e.Node.Level == 2)
            {
                //btnNewGroup.Enabled = false; //gert commented
                if (e.Node.Parent.Parent.Text == "Default" && e.Node.Parent.Text == "Default")
                    btnDeleteNode.Enabled = false;
                else
                    btnDeleteNode.Enabled = true;
                lblCurrentCategory.Text = e.Node.Parent.Parent.Text + " > " + e.Node.Parent.Text + " > " + e.Node.Text;
                LoadSuggestions(e.Node.Parent.Parent.Text, e.Node.Parent.Text, e.Node.Text, 1);
                // MessageBox.Show(e.Node.Text);
            }
            else
            {
            }
        }


        /// <summary>
        /// Save Button Click, Save current Selected Group Images and Suggestion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveSubliEntity_Click(object sender, EventArgs e)
        {
            saveSubliEntity2();
            lblChangedMark.Visible = false;
        }
        private void btnApplyAllSubliEntity_Click(object sender, EventArgs e)
        {
            saveSubliEntity3();
            lblChangedMark.Visible = false;
        }

        private void saveSubliEntity()
        {
            var imgs = new List<string>();
            foreach (var item in lstViewItems.Items)
            {
                if (((ListViewItem)item).Checked == true)
                {
                    string str = ((ListViewItem)item).SubItems[0].Text + "," + ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[2].Text + ",y";
                    imgs.Add(str);
                }
                else if (((ListViewItem)item).Checked == false)
                {
                    string str = ((ListViewItem)item).SubItems[0].Text + "," + ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[2].Text + ",n";
                    imgs.Add(str);
                }
            }
            var suggs = new List<String>();
            foreach (var sugg in rtxtSuggestions.Text.Split('\n'))
            {
                if (sugg.Trim().Length > 0)
                    suggs.Add(sugg);
            }

            var suc = (from sc in subliCategories.Categories
                       where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                       select sc).FirstOrDefault();

            suc.CategoryName = lblCurrentCategory.Text.Split('>')[0].Trim();
            suc.Language = cmbLanguage.SelectedValue.ToString();
            suc.StartupMode = cmbStartUpMode.SelectedValue.ToString();

            int back_transparent;
            if (chkTransparency.Checked == true)
            {
                back_transparent = 1;
            }
            else
            {
                back_transparent = 0;
            }

            var sug = (from gp in suc.Groups
                       where gp.GroupName == lblCurrentCategory.Text.Split('>')[1].Trim()
                       select gp).FirstOrDefault();
            try
            {
                sug = new SubliGroup
                {
                    GroupName = lblCurrentCategory.Text.Split('>')[1].Trim(),
                    SubliImage = new SubliImageEntity
                    {
                        ImageFiles = imgs,
                        SortingOrder = Convert.ToInt32(cmbSortingOrder.SelectedValue.ToString()),
                        SplashingPeriodInSeconds = Convert.ToInt32(cmbSuggestionsPeriodSeconds
                                                                   .SelectedValue.ToString()),
                        SplashDisplayForMilliSeconds = Convert.ToInt32(cmbSuggestionDurationInMilliseconds
                                         .SelectedValue.ToString()),
                        SplashPosition = Convert.ToInt32(cmbSplashingPosition
                                                        .SelectedValue.ToString()),
                        Opacity = trkOpacity.Value,
                        Transparency = back_transparent
                    }
                    ,
                    SubliSuggestion = new SubliSuggestionsEntity
                    {
                        SplashingPeriodInSeconds = Convert.ToInt32(cmbSuggestionsPeriodSeconds
                                                                   .SelectedValue.ToString
                                                                       ()),
                        SplashDisplayForMilliSeconds = Convert.ToInt32(cmbSuggestionDurationInMilliseconds
                                                                        .SelectedValue
                                                                        .ToString()),
                        BackgroundColor = cmbBackgroundColour.SelectedValue.ToString(),
                        SelectedFont = new FontConverter().ConvertToString(fontDialogSubli.Font),
                        FontColor = fontDialogSubli.Color.Name,
                        Language = cmbLanguage.SelectedValue.ToString(),
                        Suggestions = suggs,
                        SplashPosition = Convert.ToInt32(cmbSplashingPosition.SelectedValue.ToString()),
                        Opacity = trkOpacity.Value,
                        Transparency = back_transparent
                    }
                };

            }
            catch { }

            for (int i = 0; i < subliCategories.Categories.Count; i++)
            {
                if (subliCategories.Categories[i].CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim())
                {
                    subliCategories.Categories[i].Language = suc.Language;
                    subliCategories.Categories[i].StartupMode = suc.StartupMode;
                    //foreach (var sg in sc.Groups)
                    for (int j = 0; j < subliCategories.Categories[i].Groups.Count; j++)
                    {
                        if (subliCategories.Categories[i].Groups[j].GroupName == lblCurrentCategory.Text.Split('>')[1].Trim())
                        {
                            subliCategories.Categories[i].Groups[j].SubliImage.ImageFiles = sug.SubliImage.ImageFiles;
                            subliCategories.Categories[i].Groups[j].SubliImage.SplashingPeriodInSeconds = sug.SubliImage.SplashingPeriodInSeconds;
                            subliCategories.Categories[i].Groups[j].SubliImage.SplashDisplayForMilliSeconds = sug.SubliImage.SplashDisplayForMilliSeconds;
                            subliCategories.Categories[i].Groups[j].SubliImage.SplashPosition = sug.SubliImage.SplashPosition;
                            subliCategories.Categories[i].Groups[j].SubliImage.Opacity = sug.SubliImage.Opacity;
                            subliCategories.Categories[i].Groups[j].SubliImage.SortingOrder = sug.SubliImage.SortingOrder;
                            subliCategories.Categories[i].Groups[j].SubliImage.Transparency = sug.SubliImage.Transparency;

                            subliCategories.Categories[i].Groups[j].SubliSuggestion.Suggestions = sug.SubliSuggestion.Suggestions;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SplashingPeriodInSeconds = sug.SubliSuggestion.SplashingPeriodInSeconds;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SplashDisplayForMilliSeconds = sug.SubliSuggestion.SplashDisplayForMilliSeconds;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SplashPosition = sug.SubliSuggestion.SplashPosition;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.Opacity = sug.SubliSuggestion.Opacity;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.FontColor = sug.SubliSuggestion.FontColor;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SelectedFont = sug.SubliSuggestion.SelectedFont;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.BackgroundColor = sug.SubliSuggestion.BackgroundColor;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.Transparency = sug.SubliSuggestion.Transparency;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.Language = sug.SubliSuggestion.Language;
                        }
                    }
                }
            }
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
            var startUpMode = Convert.ToInt32(cmbStartUpMode.SelectedValue.ToString());
            if (startUpMode == 1)
            {
                rkApp.SetValue("SubliMaster", Application.ExecutablePath.ToString());
            }
            else
            {
                rkApp.DeleteValue("SubliMaster", false);
            }

            string stri = "";
            string settings = "";
            string record = "";
            var xml_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var subli1 = SubliDataManager.Read(xml_path + "\\Subli.xml");
            subliCategories = subli1;
            if (subli1.Categories != null)
            {
                var subli = subli1.Categories[0];
                stri = subli.Language;
            }

            XmlDocument doc = new XmlDocument();
            string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            doc.Load(pathqw + "/languages.xml");
            XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
            foreach (XmlNode chldNode in node.ChildNodes)
            {
                string astring = chldNode.Attributes["name"].InnerXml.Trim();
                //
                if (astring == stri)
                {
                    record = chldNode["record_saved"].InnerText;
                    settings = chldNode["setting_saved"].InnerText;
                }
            }

            LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());
            MessageBox.Show(settings + " " + lblCurrentCategory.Text, record, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        }
        private void saveSubliEntity2()
        {
            var imgs = new List<string>();
            foreach (var item in listViewAffImages.Items)
            {
                if (((ListViewItem)item).Checked == true)
                {
                    string str = ((ListViewItem)item).SubItems[0].Text + "," + ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[2].Text + ",y";
                    imgs.Add(str);
                }
                else if (((ListViewItem)item).Checked == false)
                {
                    string str = ((ListViewItem)item).SubItems[0].Text + "," + ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[2].Text + ",n";
                    imgs.Add(str);
                }
            }

            var suggs = new List<String>();
            foreach (ListViewItem sugg in listViewAffSuggestions.Items)
            {
                if (sugg.Text.Trim().Length > 0)
                    suggs.Add(sugg.Text.Trim());
            }
            //
            // commented for bug, gert
            //
            /*
            var suc = (from sc in subliCategories.Categories
                       where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                       select sc).FirstOrDefault();
            */
            if (listBox1.Items.Count == 0) return;
            int catSelIndex = listBox1.SelectedIndex;
            if (catSelIndex == -1)
            {
                catSelIndex = 0;
            }
            var suc = subliCategories.Categories[catSelIndex];

            //suc.CategoryName = lblCurrentCategory.Text.Split('>')[0].Trim();
            suc.Language = cmbLanguage.SelectedValue.ToString();
            suc.StartupMode = cmbStartUpMode.SelectedValue.ToString();

            int back_transparent;
            if (chkTransparency.Checked == true)
            {
                back_transparent = 1;
            }
            else
            {
                back_transparent = 0;
            }
            
            //var sug = (from gp in suc.Groups
            //           where gp.GroupName == lblCurrentCategory.Text.Split('>')[1].Trim()
            //           select gp).FirstOrDefault();
            var sug = new SubliGroup();
            if (listviewGroup.SelectedIndices.Count > 0)
            {
                sug = suc.Groups[listviewGroup.SelectedIndices[0]];
            }
            else
            {
                return;
            }
            
            try
            {
                sug = new SubliGroup
                {
                    GroupName = lblCurrentCategory.Text.Split('>')[1].Trim(),
                    SubliImage = new SubliImageEntity
                    {
                        ImageFiles = imgs,
                        SortingOrder = Convert.ToInt32(cmbSortingOrder.SelectedValue.ToString()),
                        SplashingPeriodInSeconds = Convert.ToInt32(cmbSuggestionsPeriodSeconds
                                                                   .SelectedValue.ToString()),
                        SplashDisplayForMilliSeconds = Convert.ToInt32(cmbSuggestionDurationInMilliseconds
                                         .SelectedValue.ToString()),
                        SplashPosition = Convert.ToInt32(cmbSplashingPosition
                                                        .SelectedValue.ToString()),
                        Opacity = trkOpacity.Value,
                        Transparency = back_transparent
                    }
                    ,
                    SubliSuggestion = new SubliSuggestionsEntity
                    {
                        SplashingPeriodInSeconds = Convert.ToInt32(cmbSuggestionsPeriodSeconds
                                                                   .SelectedValue.ToString
                                                                       ()),
                        SplashDisplayForMilliSeconds = Convert.ToInt32(cmbSuggestionDurationInMilliseconds
                                                                        .SelectedValue
                                                                        .ToString()),
                        BackgroundColor = cmbBackgroundColour.SelectedValue.ToString(),
                        SelectedFont = new FontConverter().ConvertToString(fontDialogSubli.Font),
                        FontColor = fontDialogSubli.Color.Name,
                        Language = cmbLanguage.SelectedValue.ToString(),
                        Suggestions = suggs,
                        SplashPosition = Convert.ToInt32(cmbSplashingPosition.SelectedValue.ToString()),
                        Opacity = trkOpacity.Value,
                        Transparency = back_transparent
                    }
                };

            }
            catch { }
            

            for (int i = 0; i < subliCategories.Categories.Count; i++)
            {
                if (subliCategories.Categories[i].CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim())
                {
                    subliCategories.Categories[i].Language = suc.Language;
                    subliCategories.Categories[i].StartupMode = suc.StartupMode;
                    //foreach (var sg in sc.Groups)
                    for (int j = 0; j < subliCategories.Categories[i].Groups.Count; j++)
                    {
                        if (subliCategories.Categories[i].Groups[j].GroupName == lblCurrentCategory.Text.Split('>')[1].Trim())
                        {
                            subliCategories.Categories[i].Groups[j].SubliImage.ImageFiles = sug.SubliImage.ImageFiles;
                            subliCategories.Categories[i].Groups[j].SubliImage.SplashingPeriodInSeconds = sug.SubliImage.SplashingPeriodInSeconds;
                            subliCategories.Categories[i].Groups[j].SubliImage.SplashDisplayForMilliSeconds = sug.SubliImage.SplashDisplayForMilliSeconds;
                            subliCategories.Categories[i].Groups[j].SubliImage.SplashPosition = sug.SubliImage.SplashPosition;
                            subliCategories.Categories[i].Groups[j].SubliImage.Opacity = sug.SubliImage.Opacity;
                            subliCategories.Categories[i].Groups[j].SubliImage.SortingOrder = sug.SubliImage.SortingOrder;
                            subliCategories.Categories[i].Groups[j].SubliImage.Transparency = sug.SubliImage.Transparency;

                            subliCategories.Categories[i].Groups[j].SubliSuggestion.Suggestions = sug.SubliSuggestion.Suggestions;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SplashingPeriodInSeconds = sug.SubliSuggestion.SplashingPeriodInSeconds;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SplashDisplayForMilliSeconds = sug.SubliSuggestion.SplashDisplayForMilliSeconds;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SplashPosition = sug.SubliSuggestion.SplashPosition;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.Opacity = sug.SubliSuggestion.Opacity;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.FontColor = sug.SubliSuggestion.FontColor;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SelectedFont = sug.SubliSuggestion.SelectedFont;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.BackgroundColor = sug.SubliSuggestion.BackgroundColor;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.Transparency = sug.SubliSuggestion.Transparency;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.Language = sug.SubliSuggestion.Language;
                        }
                    }
                }
            }

            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
            var startUpMode = Convert.ToInt32(cmbStartUpMode.SelectedValue.ToString());
            if (startUpMode == 1)
            {
                rkApp.SetValue("SubliMaster", Application.ExecutablePath.ToString());
            }
            else
            {
                rkApp.DeleteValue("SubliMaster", false);
            }

            
            string stri = "";
            string settings = "";
            string record = "";

            /*
            var xml_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var subli1 = SubliDataManager.Read(xml_path + "\\Subli.xml");
            subliCategories = subli1;
            if (subli1.Categories != null)
            {
                var subli = subli1.Categories[0];
                stri = subli.Language;
            }
            */
            XmlDocument doc = new XmlDocument();
            string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            doc.Load(pathqw + "/languages.xml");
            XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
            foreach (XmlNode chldNode in node.ChildNodes)
            {
                string astring = chldNode.Attributes["name"].InnerXml.Trim();
                //
                if (astring == stri)
                {
                    record = chldNode["record_saved"].InnerText;
                    settings = chldNode["setting_saved"].InnerText;
                }
            }

            // gert commented
            //LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());
            

            MessageBox.Show(settings + " " + lblCurrentCategory.Text, record, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

        }
        private void saveSubliEntity3()
        {
            var imgs = new List<string>();
            foreach (var item in listViewAffImages.Items)
            {
                if (((ListViewItem)item).Checked == true)
                {
                    string str = ((ListViewItem)item).SubItems[0].Text + "," + ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[2].Text + ",y";
                    imgs.Add(str);
                }
                else if (((ListViewItem)item).Checked == false)
                {
                    string str = ((ListViewItem)item).SubItems[0].Text + "," + ((ListViewItem)item).SubItems[1].Text + "," + ((ListViewItem)item).SubItems[2].Text + ",n";
                    imgs.Add(str);
                }
            }

            var suggs = new List<String>();
            foreach (ListViewItem sugg in listViewAffSuggestions.Items)
            {
                if (sugg.Text.Trim().Length > 0)
                    suggs.Add(sugg.Text.Trim());
            }

            /*
            var suc = (from sc in subliCategories.Categories
                       where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                       select sc).FirstOrDefault();
            */
            if(listBox1.Items.Count ==0) return;
            int catSelIndex = listBox1.SelectedIndex;
            if (catSelIndex == -1)
            {
                catSelIndex = 0;
            }
            var suc = subliCategories.Categories[catSelIndex];
            //suc.CategoryName = lblCurrentCategory.Text.Split('>')[0].Trim();
            suc.Language = cmbLanguage.SelectedValue.ToString();
            suc.StartupMode = cmbStartUpMode.SelectedValue.ToString();

            int back_transparent;
            if (chkTransparency.Checked == true)
            {
                back_transparent = 1;
            }
            else
            {
                back_transparent = 0;
            }

            /*
            var sug = (from gp in suc.Groups
                       where gp.GroupName == lblCurrentCategory.Text.Split('>')[1].Trim()
                       select gp).FirstOrDefault();
            
            var sug = new SubliGroup();
            try
            {
                sug = new SubliGroup
                {
                    GroupName = lblCurrentCategory.Text.Split('>')[1].Trim(),
                    SubliImage = new SubliImageEntity
                    {
                        ImageFiles = imgs,
                        SortingOrder = Convert.ToInt32(cmbSortingOrder.SelectedValue.ToString()),
                        SplashingPeriodInSeconds = Convert.ToInt32(cmbSuggestionsPeriodSeconds
                                                                   .SelectedValue.ToString()),
                        SplashDisplayForMilliSeconds = Convert.ToInt32(cmbSuggestionDurationInMilliseconds
                                         .SelectedValue.ToString()),
                        SplashPosition = Convert.ToInt32(cmbSplashingPosition
                                                        .SelectedValue.ToString()),
                        Opacity = trkOpacity.Value,
                        Transparency = back_transparent
                    }
                    ,
                    SubliSuggestion = new SubliSuggestionsEntity
                    {
                        SplashingPeriodInSeconds = Convert.ToInt32(cmbSuggestionsPeriodSeconds
                                                                   .SelectedValue.ToString
                                                                       ()),
                        SplashDisplayForMilliSeconds = Convert.ToInt32(cmbSuggestionDurationInMilliseconds
                                                                        .SelectedValue
                                                                        .ToString()),
                        BackgroundColor = cmbBackgroundColour.SelectedValue.ToString(),
                        SelectedFont = new FontConverter().ConvertToString(fontDialogSubli.Font),
                        FontColor = fontDialogSubli.Color.Name,
                        Language = cmbLanguage.SelectedValue.ToString(),
                        Suggestions = suggs,
                        SplashPosition = Convert.ToInt32(cmbSplashingPosition.SelectedValue.ToString()),
                        Opacity = trkOpacity.Value,
                        Transparency = back_transparent
                    }
                };

            }
            catch { }
            */
            for (int i = 0; i < subliCategories.Categories.Count; i++)
            {
                {
                    if(listBox1.SelectedIndex != -1)
                        //gert modiffeid
                    //if (subliCategories.Categories[i].CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim())
                        if (subliCategories.Categories[i] == suc)
                        {
                            //subliCategories.Categories[i].Language = suc.Language;
                            //subliCategories.Categories[i].StartupMode = suc.StartupMode;
                            subliCategories.Categories[i].Language = cmbLanguage.SelectedValue.ToString();
                            subliCategories.Categories[i].StartupMode = cmbStartUpMode.SelectedValue.ToString();
                        }
                    //foreach (var sg in sc.Groups)
                    for (int j = 0; j < subliCategories.Categories[i].Groups.Count; j++)
                    {
                        if (listviewGroup.SelectedIndices.Count > 0)
                            //if (subliCategories.Categories[i].Groups[j].GroupName == lblCurrentCategory.Text.Split('>')[1].Trim())
                            if (subliCategories.Categories[i].Groups[j].GroupName == listviewGroup.Items[listviewGroup.SelectedIndices[0]].Text)
                            {
                                subliCategories.Categories[i].Groups[j].SubliImage.ImageFiles = imgs; // sug.SubliImage.ImageFiles;
                                subliCategories.Categories[i].Groups[j].SubliSuggestion.Suggestions = suggs;// sug.SubliSuggestion.Suggestions;
                                subliCategories.Categories[i].Groups[j].SubliSuggestion.Language = cmbLanguage.SelectedValue.ToString(); // sug.SubliSuggestion.Language;
                            }

                        {
                            subliCategories.Categories[i].Groups[j].SubliImage.SplashingPeriodInSeconds = Convert.ToInt32(cmbSuggestionsPeriodSeconds.SelectedValue.ToString());
                            subliCategories.Categories[i].Groups[j].SubliImage.SplashDisplayForMilliSeconds = Convert.ToInt32(cmbSuggestionDurationInMilliseconds.SelectedValue.ToString());
                            subliCategories.Categories[i].Groups[j].SubliImage.SplashPosition = Convert.ToInt32(cmbSplashingPosition.SelectedValue.ToString());
                            subliCategories.Categories[i].Groups[j].SubliImage.Opacity = trkOpacity.Value;
                            subliCategories.Categories[i].Groups[j].SubliImage.SortingOrder = Convert.ToInt32(cmbSortingOrder.SelectedValue.ToString());
                            subliCategories.Categories[i].Groups[j].SubliImage.Transparency = back_transparent;



                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SplashingPeriodInSeconds = Convert.ToInt32(cmbSuggestionsPeriodSeconds.SelectedValue.ToString());
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SplashDisplayForMilliSeconds = Convert.ToInt32(cmbSuggestionDurationInMilliseconds.SelectedValue.ToString());
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SplashPosition = Convert.ToInt32(cmbSplashingPosition.SelectedValue.ToString());
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.Opacity = trkOpacity.Value;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.FontColor = lblSelectedFont.ForeColor.Name;
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.SelectedFont = new FontConverter().ConvertToString(lblSelectedFont.Font);
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.BackgroundColor = cmbBackgroundColour.SelectedValue.ToString();
                            subliCategories.Categories[i].Groups[j].SubliSuggestion.Transparency = back_transparent;


                        }
                    }
                }
            }
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
            var startUpMode = Convert.ToInt32(cmbStartUpMode.SelectedValue.ToString());
            if (startUpMode == 1)
            {
                rkApp.SetValue("SubliMaster", Application.ExecutablePath.ToString());
            }
            else
            {
                rkApp.DeleteValue("SubliMaster", false);
            }


            string stri = "";
            string settings = "";
            string record = "";
            /*
            var xml_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var subli1 = SubliDataManager.Read(xml_path + "\\Subli.xml");
            subliCategories = subli1;
            if (subli1.Categories != null)
            {
                var subli = subli1.Categories[0];
                stri = subli.Language;
            }
            */

            XmlDocument doc = new XmlDocument();
            string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            doc.Load(pathqw + "/languages.xml");
            XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
            foreach (XmlNode chldNode in node.ChildNodes)
            {
                string astring = chldNode.Attributes["name"].InnerXml.Trim();
                //
                if (astring == stri)
                {
                    record = chldNode["record_saved"].InnerText;
                    settings = chldNode["setting_saved"].InnerText;
                }
            }

            // gert commented
            //LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());


            MessageBox.Show(settings + " " + lblCurrentCategory.Text, record, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

        }
        /// <summary>
        /// Opacity % display onchange
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trkOpacity_Scroll(object sender, EventArgs e)
        {
            /*
            double xyz = trkOpacity.Value;
            int z = (int)((xyz / 255) * 100);
            lbl_percent_opacity.Text = z.ToString() + " %";
            */
            lbl_percent_opacity.Text = trkOpacity.Value.ToString() + "%";
        }

        #region language related

        /// <summary>
        /// language change here and calling language function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbLanguage_SelectedValueChanged(object sender, EventArgs e)
        {
            language_data(2);
        }

       

        /// <summary>
        /// language function, content load from xml by language
        /// </summary>
        /// <param name="type_id">1 > onload, 2> when language change</param>
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
            else
            {
                bstre = Convert.ToString(cmbLanguage.SelectedValue).Split(':')[0];
            }

            //
            // get set current language
            //
            Language.setCurLang(bstre);

            switch (curVersion)
            {
                case VersionType.FREE:
                    language_dataFree();
                    break;
                case VersionType.PRO:
                    language_dataPro();
                    break;
                default:
                    language_dataFree();
                    break;
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
                    this.Text = chldNode["title"].InnerText;

                    //label6.Text = chldNode["slogan_1"].InnerText;
                    //label7.Text = chldNode["slogan_2"].InnerText;
                    label1.Text = chldNode["slogan_1"].InnerText + " " + chldNode["slogan_2"].InnerText;


                    groupBox4.Text = chldNode["timing_heading"].InnerText;
                    label11.Text = chldNode["every"].InnerText;
                    label9.Text = chldNode["seconds"].InnerText;
                    label10.Text = chldNode["display_for"].InnerText;
                    label8.Text = chldNode["millisecond"].InnerText;
                    grpSplashingPosition.Text = chldNode["splashing_position_heading"].InnerText;

                    /*
                    var splashingPosition = new List<SplashingPosition>
                                        {
                                            new SplashingPosition {DisplayMember = chldNode["screen_center"].InnerText, ValueMember = 1},
                                            new SplashingPosition {DisplayMember = chldNode["screen_random"].InnerText, ValueMember = 2}
                                        };
                    cmbSplashingPosition.ValueMember = "ValueMember";
                    cmbSplashingPosition.DisplayMember = "DisplayMember";
                    cmbSplashingPosition.DataSource = splashingPosition;
                    */

                    grpOpacity.Text = chldNode["opacity_heading"].InnerText;
                    grpSortingOrder.Text = chldNode["order_heading"].InnerText;

                    var splashSortOrder = new List<SplashSortOrder>
                                                {
                                                    new SplashSortOrder {DisplayMember = chldNode["random_ordered"].InnerText, ValueMember = 1},
                                                    new SplashSortOrder {DisplayMember = chldNode["sort_by_name"].InnerText, ValueMember = 2},
                                                    new SplashSortOrder {DisplayMember = chldNode["sort_by_type"].InnerText, ValueMember = 3},
                                                    new SplashSortOrder {DisplayMember = chldNode["sort_by_size"].InnerText, ValueMember = 4}
                                                };
                    cmbSortingOrder.ValueMember = "ValueMember";
                    cmbSortingOrder.DisplayMember = "DisplayMember";
                    cmbSortingOrder.DataSource = splashSortOrder;
                     

                    grpAppearance.Text = chldNode["appearance_heading"].InnerText;
                    btnChangeFont.Text = chldNode["change_font"].InnerText;
                    lblBGColor.Text = chldNode["background_color"].InnerText;
                    grpStartupMode.Text = chldNode["startup_heading"].InnerText;
                   

                    var startUpMode = new List<StartUpMode>
                                                {
                                                    new StartUpMode {DisplayMember = chldNode["autostart"].InnerText, ValueMember = 1},
                                                    new StartUpMode {DisplayMember = chldNode["manual"].InnerText, ValueMember = 2}
                                                };
                    cmbStartUpMode.ValueMember = "ValueMember";
                    cmbStartUpMode.DisplayMember = "DisplayMember";
                    cmbStartUpMode.DataSource = startUpMode;
                    


                    label4.Text = chldNode["language_heading"].InnerText;
                    //gert commented
                    //groupBox1.Text = chldNode["testarea_heading"].InnerText;
                    
                    //commented by gert
                    //btnNewCategory.Text = chldNode["new_category"].InnerText;
                    ToolTip toolTip1 = new System.Windows.Forms.ToolTip();
                    toolTip1.SetToolTip(btnNewCategory, chldNode["new_category"].InnerText);
                    //btnNewGroup.Text = chldNode["new_group"].InnerText;
                    toolTip1 = new System.Windows.Forms.ToolTip();
                    toolTip1.SetToolTip(btnNewGroup, chldNode["new_group"].InnerText);
                    //btnDeleteNode.Text = chldNode["delete_node"].InnerText;
                    toolTip1 = new System.Windows.Forms.ToolTip();
                    toolTip1.SetToolTip(btnDeleteNode, chldNode["delete_node"].InnerText);

                    btnPlayAll.Text = chldNode["start"].InnerText;
                    btnStopAll.Text = chldNode["stop"].InnerText;

                    button12.Text = chldNode["stop"].InnerText;
                    button13.Text = chldNode["start"].InnerText;
                    //btnUpgrade.Text = chldNode["upgrade"].InnerText;
                    btnSaveSubliEntity.Text = chldNode["save"].InnerText;
                    btnTest.Text = chldNode["test"].InnerText;
                    tabImages.Text = chldNode["images"].InnerText;
                    tabSuggestions.Text = chldNode["suggestions"].InnerText;

                    //gert commented
                    //groupBox3.Text = chldNode["categories"].InnerText;

                    //
                    // tabs
                    //

                    label13.Text = Language.getString("My Selection");
                    label3.Text = Language.getString("Introduction & News");

                    label21.Text = Language.getString("Category"); ;
                    label22.Text = Language.getString("Group");
                    lblImageAffirmation.Text = Language.getString("Images");
                    lblTextAffirmation.Text = Language.getString("Suggestions");

                    label23.Text = Language.getString("Options");
                    button11.Text = Language.getString("Apply All");

                    label25.Text = Language.getString("Opacity");
                    label26.Text = Language.getString("Sort by");

                    btnDeleteNode.Text = Language.getString("Delete");
                    button5.Text = Language.getString("Delete");
                    button6.Text = Language.getString("Delete");

                    button15.Text = Language.getString("Get Pro Version");

                    label32.Text = Language.getString("This is a Pro version feature.");
                    linkLabel1.Text = Language.getString("Get your license here.");

                    cmbLanguage.SelectedValue = bstre;
                }
            }
            doc.Save(path + "/languages.xml");
        }

        private void language_dataPro()
        {
            
        }

        private void language_dataFree()
        {
            
        }

        #endregion

        /// <summary>
        /// converts sting into bitmap image
        /// </summary>
        /// <param name="TheText">input</param>
        /// <param name="f">font</param>
        /// <param name="c">color</param>
        /// <param name="b">back</param>
        /// <returns>bitmap image</returns>
        public Bitmap TextToBitmap(String TheText, Font f, Color c, Color b)
        {
            Font DrawFont = null;
            SolidBrush DrawBrush = null;
            Graphics DrawGraphics = null;
            Bitmap TextBitmap = null;
            try
            {
                // start with empty bitmap, get it's graphic's object
                // and choose a font
                TextBitmap = new Bitmap(1, 1);
                DrawGraphics = Graphics.FromImage(TextBitmap);
                DrawFont = f;// new Font("Arial", 26);

                // see how big the text will be
                int Width = (int)DrawGraphics.MeasureString(TheText, DrawFont).Width;
                int Height = (int)DrawGraphics.MeasureString(TheText, DrawFont).Height;


                // recreate the bitmap and graphic object with the new size
                TextBitmap = new Bitmap(TextBitmap, Width, Height);
                DrawGraphics = Graphics.FromImage(TextBitmap);


                // get the drawing brush and where we're going to draw
                DrawBrush = new SolidBrush(c);
                PointF DrawPoint = new PointF(0, 0);


                // clear the graphic white and draw the string
                DrawGraphics.Clear(b);
                DrawGraphics.DrawString(TheText, DrawFont, DrawBrush, DrawPoint);


                return TextBitmap;
            }
            finally
            {
                // don't dispose the bitmap, the caller needs it.
                DrawFont.Dispose();
                DrawBrush.Dispose();
                DrawGraphics.Dispose();
            }
        }


        private SubliCategory sublicCategoryForCopy = null;
        private SubliGroup subliGroupForCopy = null;
        private SubliImageEntity subliImageEntiryForCopy = null;
        private SubliSuggestionsEntity subliSuggestionsEntityForCopy = null;

        private void trvSuggestions_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                trvSuggestions.SelectedNode = trvSuggestions.GetNodeAt(e.X, e.Y);
                if (trvSuggestions.SelectedNode.Level == 0)
                    trvSuggestions.SelectedNode.ContextMenuStrip = ctxTreeViewSuggestionsCategoryCopy;
                else if (trvSuggestions.SelectedNode.Level == 1)
                    trvSuggestions.SelectedNode.ContextMenuStrip = ctxTreeViewSuggestionsGroupCopy;
                else if (trvSuggestions.SelectedNode.Level == 2)
                {
                    if (trvSuggestions.SelectedNode.Text == "Suggestions")
                        trvSuggestions.SelectedNode.ContextMenuStrip = ctxTreeViewSuggestionsCopy;
                    else
                        trvSuggestions.SelectedNode.ContextMenuStrip = ctxTreeViewSuggestionsImageCopy;
                }
            }
        }

        private void ctxTreeViewSuggestionsCategoryCopy_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == treeViewToolStripMenuItemCopy)
            {
                treeViewToolStripMenuItemPasteAppend.Enabled = true;
                treeViewToolStripMenuItemPasteOverwrite.Enabled = true;
                sublicCategoryForCopy = (from sc in subliCategories.Categories
                                         //where sc.CategoryName == trvSuggestions.SelectedNode.Text
                                         where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                                         select sc).FirstOrDefault();
            }
            else if (e.ClickedItem == treeViewToolStripMenuItemPasteOverwrite && sublicCategoryForCopy != null)
            {
                for (int i = 0; i < subliCategories.Categories.Count(); i++)
                {
                    if (subliCategories.Categories[i].CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim())
                    {
                        subliCategories.Categories[i].Groups = new List<SubliGroup>();
                        for (int j = 0; j < sublicCategoryForCopy.Groups.Count(); j++)
                        {
                           try
                           {
                                subliCategories.Categories[i].Groups.Add(new SubliGroup
                                {
                                    GroupName = sublicCategoryForCopy.Groups[j].GroupName,
                                    SubliImage = sublicCategoryForCopy.Groups[j].SubliImage,
                                    SubliSuggestion = sublicCategoryForCopy.Groups[j].SubliSuggestion
                                });
                            }
                            catch (Exception ex) { /*MessageBox.Show(ex.Message);*/ }

                        }
                    }
                }
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
                LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());
            }
            else
            {
                try
                {
                var msg = "";
                //for (int i = 0; i < subliCategories.Categories.Count; i++)
                var suc = (from sc in subliCategories.Categories
                           where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                           select sc).FirstOrDefault();
                if (suc.Groups.Count > 0)
                {
                    //if (suc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim())
                    {
                        //subliCategories.Categories[i].Groups = new List<SubliGroup>();
                        //foreach (var sg in sc.Groups)
                        for (int j = 0; j < sublicCategoryForCopy.Groups.Count(); j++)
                        {
                                try
                                {
                                    var sug = (from gp in suc.Groups
                                               where gp.GroupName == sublicCategoryForCopy.Groups[j].GroupName
                                               select gp).FirstOrDefault();
                                    if (sug != null)
                                    {
                                        msg += sug.GroupName + "\n";
                                    }
                                    else
                                    {
                                        SubliImageEntity sie = new SubliImageEntity
                                        {
                                            ImageFiles = new List<string>(),
                                            SortingOrder = sublicCategoryForCopy.Groups[j].SubliImage.SortingOrder,
                                            SplashDisplayForMilliSeconds = sublicCategoryForCopy.Groups[j].SubliImage.SplashDisplayForMilliSeconds,
                                            SplashingPeriodInSeconds = sublicCategoryForCopy.Groups[j].SubliImage.SplashingPeriodInSeconds,
                                            SplashPosition = sublicCategoryForCopy.Groups[j].SubliImage.SplashPosition,
                                            Opacity = sublicCategoryForCopy.Groups[j].SubliImage.Opacity
                                        };
                                        foreach (var str in sublicCategoryForCopy.Groups[j].SubliImage.ImageFiles)
                                        {
                                            //bool clearToAdd = true;
                                            //foreach (var im in sug.SubliImage.ImageFiles)
                                            //{
                                            //    if (im == str)
                                            //        clearToAdd = false;
                                            //}
                                            //if (clearToAdd)
                                            sie.ImageFiles.Add(str);
                                        }
                                        suc.Groups.Add(new SubliGroup
                                        {
                                            GroupName = sublicCategoryForCopy.Groups[j].GroupName,
                                            SubliImage = sie,
                                            SubliSuggestion = sublicCategoryForCopy.Groups[j].SubliSuggestion
                                        });
                                    }
                                }
                                catch (Exception ex) { /*MessageBox.Show(ex.Message);*/ }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < subliCategories.Categories.Count(); i++)
                    {
                        if (subliCategories.Categories[i].CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim())
                        {
                            subliCategories.Categories[i].Groups = new List<SubliGroup>();
                            for (int j = 0; j < sublicCategoryForCopy.Groups.Count(); j++)
                            {
                                try
                                {
                                    subliCategories.Categories[i].Groups.Add(new SubliGroup
                                    {
                                        GroupName = sublicCategoryForCopy.Groups[j].GroupName,
                                        SubliImage = sublicCategoryForCopy.Groups[j].SubliImage,
                                        SubliSuggestion = sublicCategoryForCopy.Groups[j].SubliSuggestion
                                    });
                                }
                                catch (Exception ex) { /*MessageBox.Show(ex.Message);*/ }

                            }
                        }
                    }
                }
                if (msg.Length > 0)
                {

                    string stri = "";
                    string text1 = "";
                    string text2 = "";
                    var xml_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    var subli1 = SubliDataManager.Read(xml_path + "\\Subli.xml");
                    subliCategories = subli1;
                    if (subli1.Categories != null)
                    {
                        var subli = subli1.Categories[0];
                        stri = subli.Language;
                    }

                    XmlDocument doc = new XmlDocument();
                    string pathqw = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
                    doc.Load(pathqw + "/languages.xml");
                    XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
                    foreach (XmlNode chldNode in node.ChildNodes)
                    {
                        string astring = chldNode.Attributes["name"].InnerXml.Trim();
                        //
                        if (astring == stri)
                        {
                            text1 = chldNode["duplicate_found"].InnerText;
                            text2 = chldNode["following_group_names_already_exist"].InnerText;
                        }
                    }
                    MessageBox.Show(text2 + " \n" + msg, text1, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
                LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());
                     
                }
                   catch(Exception ex){}
            }

        }

        /// <summary>
        /// if user have no rights then this function calls
        /// </summary>
        /// <returns>Upgrade version message</returns>
        private string show_upgrade_Message()
        {
            string returning = "";
            string stri = "";
            var xml_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var subli1 = SubliDataManager.Read(xml_path + "\\Subli.xml");
            subliCategories = subli1;
            if (subli1.Categories != null)
            {
                var subli = subli1.Categories[0];
                stri = subli.Language;
            }

            XmlDocument doc = new XmlDocument();
            string path = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            doc.Load(path + "/languages.xml");
            XmlNode node = doc.DocumentElement.SelectSingleNode("//Root");
            foreach (XmlNode chldNode in node.ChildNodes)
            {
                string astring = chldNode.Attributes["name"].InnerXml.Trim();
                //
                if (astring == stri)
                {
                    returning = chldNode["title"].InnerText;
                    returning = returning + "%" + chldNode["upgrade_message"].InnerText;
                }
            }
            doc.Save(path + "/languages.xml");
            return returning;
        }

        

        private void ctxTreeViewSuggestionsGroupCopy_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == treeViewGroupCopy)
            {
                treeViewGroupAppend.Enabled = true;
                treeViewGroupOverwrite.Enabled = true;
                var cat = (from sc in subliCategories.Categories
                           //where sc.CategoryName == trvSuggestions.SelectedNode.Text
                           where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                           select sc).FirstOrDefault();
                subliGroupForCopy = (from sg in cat.Groups
                                     where sg.GroupName == lblCurrentCategory.Text.Split('>')[1].Trim()
                                     select sg).FirstOrDefault();
            }
            else if (e.ClickedItem == treeViewGroupOverwrite && subliGroupForCopy != null)
            {
                for (int i = 0; i < subliCategories.Categories.Count(); i++)
                {
                    if (subliCategories.Categories[i].CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim())
                    {
                        //subliCategories.Categories[i].Groups = new List<SubliGroup>();
                        for (int j = 0; j < subliCategories.Categories[i].Groups.Count(); j++)
                        {
                            try
                            {
                                if (subliCategories.Categories[i].Groups[j].GroupName == lblCurrentCategory.Text.Split('>')[1].Trim())
                                {
                                    subliCategories.Categories[i].Groups[j].SubliImage = subliGroupForCopy.SubliImage;
                                    subliCategories.Categories[i].Groups[j].SubliSuggestion = subliGroupForCopy.SubliSuggestion;
                                }

                            }
                            catch (Exception ex) { /*MessageBox.Show(ex.Message);*/ }

                        }
                    }
                }
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
                LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());
            }
            else
            {
                try
                {
                    var msg = "";
                    //for (int i = 0; i < subliCategories.Categories.Count; i++)
                    var suc = (from sc in subliCategories.Categories
                               where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                               select sc).FirstOrDefault();
                    var g = (from gr in suc.Groups
                             where gr.GroupName == lblCurrentCategory.Text.Split('>')[1].Trim()
                             select gr).FirstOrDefault();

                    for (int i = 0; i < subliGroupForCopy.SubliSuggestion.Suggestions.Count; i++)
                    {
                        if (!g.SubliSuggestion.Suggestions.Contains(subliGroupForCopy.SubliSuggestion.Suggestions[i]))
                            g.SubliSuggestion.Suggestions.Add(subliGroupForCopy.SubliSuggestion.Suggestions[i]);
                    }
                    for (int j = 0; j < subliGroupForCopy.SubliImage.ImageFiles.Count; j++)
                    {
                        if (!g.SubliImage.ImageFiles.Contains(subliGroupForCopy.SubliImage.ImageFiles[j]))
                            g.SubliImage.ImageFiles.Add(subliGroupForCopy.SubliImage.ImageFiles[j]);
                    }

                    var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
                    LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());

                }
                catch (Exception ex) { }
            }
        }
        private void ctxTreeViewSuggestionsImageCopy_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == treeViewImagesCopy)
            {
                treeViewImageAppend.Enabled = true;
                treeViewImageOverwrite.Enabled = true;
                var cat = (from sc in subliCategories.Categories
                           //where sc.CategoryName == trvSuggestions.SelectedNode.Text
                           where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                           select sc).FirstOrDefault();
                var g = (from sg in cat.Groups
                                     where sg.GroupName == lblCurrentCategory.Text.Split('>')[1].Trim()
                                     select sg).FirstOrDefault();
                subliImageEntiryForCopy = g.SubliImage;
            }
            else if (e.ClickedItem == treeViewImageOverwrite && subliImageEntiryForCopy != null)
            {
                for (int i = 0; i < subliCategories.Categories.Count(); i++)
                {
                    if (subliCategories.Categories[i].CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim())
                    {
                        //subliCategories.Categories[i].Groups = new List<SubliGroup>();
                        for (int j = 0; j < subliCategories.Categories[i].Groups.Count(); j++)
                        {
                            try
                            {
                                if (subliCategories.Categories[i].Groups[j].GroupName == lblCurrentCategory.Text.Split('>')[1].Trim())
                                {
                                    subliCategories.Categories[i].Groups[j].SubliImage = subliImageEntiryForCopy;
                                    //subliCategories.Categories[i].Groups[j].SubliSuggestion = subliImageEntiryForCopy.SubliSuggestion;
                                }

                            }
                            catch (Exception ex) { /*MessageBox.Show(ex.Message);*/ }

                        }
                    }
                }
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
                LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());
            }
            else
            {
                try
                {
                    var msg = "";
                    //for (int i = 0; i < subliCategories.Categories.Count; i++)
                    var suc = (from sc in subliCategories.Categories
                               where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                               select sc).FirstOrDefault();
                    var g = (from gr in suc.Groups
                             where gr.GroupName == lblCurrentCategory.Text.Split('>')[1].Trim()
                             select gr).FirstOrDefault();

                    /*
                    for (int i = 0; i < subliGroupForCopy.SubliSuggestion.Suggestions.Count; i++)
                    {
                        if (!g.SubliSuggestion.Suggestions.Contains(subliGroupForCopy.SubliSuggestion.Suggestions[i]))
                            g.SubliSuggestion.Suggestions.Add(subliGroupForCopy.SubliSuggestion.Suggestions[i]);
                    }
                    */
                    for (int j = 0; j < subliImageEntiryForCopy.ImageFiles.Count; j++)
                    {
                        if (!g.SubliImage.ImageFiles.Contains(subliImageEntiryForCopy.ImageFiles[j]))
                            g.SubliImage.ImageFiles.Add(subliImageEntiryForCopy.ImageFiles[j]);
                    }

                    var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
                    LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());

                }
                catch (Exception ex) { }
            }
        }

        private void ctxTreeViewSuggestionsCopy_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == treeViewSuggestionsCopy)
            {
                treeViewSuggestionsAppend.Enabled = true;
                treeViewSuggestionsOverwrite.Enabled = true;
                var cat = (from sc in subliCategories.Categories
                           //where sc.CategoryName == trvSuggestions.SelectedNode.Text
                           where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                           select sc).FirstOrDefault();
                var g = (from sg in cat.Groups
                         where sg.GroupName == lblCurrentCategory.Text.Split('>')[1].Trim()
                         select sg).FirstOrDefault();
                subliSuggestionsEntityForCopy = g.SubliSuggestion;
            }
            else if (e.ClickedItem == treeViewSuggestionsOverwrite && subliSuggestionsEntityForCopy != null)
            {
                for (int i = 0; i < subliCategories.Categories.Count(); i++)
                {
                    if (subliCategories.Categories[i].CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim())
                    {
                        //subliCategories.Categories[i].Groups = new List<SubliGroup>();
                        for (int j = 0; j < subliCategories.Categories[i].Groups.Count(); j++)
                        {
                            try
                            {
                                if (subliCategories.Categories[i].Groups[j].GroupName == lblCurrentCategory.Text.Split('>')[1].Trim())
                                {
                                    //subliCategories.Categories[i].Groups[j].SubliImage = subliImageEntiryForCopy;
                                    subliCategories.Categories[i].Groups[j].SubliSuggestion = subliSuggestionsEntityForCopy;
                                }

                            }
                            catch (Exception ex) { /*MessageBox.Show(ex.Message);*/ }

                        }
                    }
                }
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
                LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());
            }
            else
            {
                try
                {
                    var msg = "";
                    //for (int i = 0; i < subliCategories.Categories.Count; i++)
                    var suc = (from sc in subliCategories.Categories
                               where sc.CategoryName == lblCurrentCategory.Text.Split('>')[0].Trim()
                               select sc).FirstOrDefault();
                    var g = (from gr in suc.Groups
                             where gr.GroupName == lblCurrentCategory.Text.Split('>')[1].Trim()
                             select gr).FirstOrDefault();

                    /*
                    for (int i = 0; i < subliGroupForCopy.SubliSuggestion.Suggestions.Count; i++)
                    {
                        if (!g.SubliSuggestion.Suggestions.Contains(subliGroupForCopy.SubliSuggestion.Suggestions[i]))
                            g.SubliSuggestion.Suggestions.Add(subliGroupForCopy.SubliSuggestion.Suggestions[i]);
                    }
                    */
                    for (int j = 0; j < subliSuggestionsEntityForCopy.Suggestions.Count; j++)
                    {
                        if (!g.SubliSuggestion.Suggestions.Contains(subliSuggestionsEntityForCopy.Suggestions[j]))
                            g.SubliSuggestion.Suggestions.Add(subliSuggestionsEntityForCopy.Suggestions[j]);
                    }

                    var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    SubliDataManager.Write(subliCategories, path + "\\Subli.xml");
                    LoadSuggestionsTree(subliCategories, lblCurrentCategory.Text.Split('>')[0].Trim());

                }
                catch (Exception ex) { }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Maximized;
                button3.Text = "◱";
            }
            else
            {
                this.WindowState = FormWindowState.Normal;
                button3.Text = "⬜";
            }
        }

        private void SubliMasterMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            button3_Click(null, null);
        }

        private void Section_MouseEnter(object sender, EventArgs e)
        {
            Label label = sender as Label;
            hoverOnSection(label);
        }

        private void Section_MouseLeave(object sender, EventArgs e)
        {
            Label label = sender as Label;
            hoverOffSection(label);

        }

        private void sectionLabel_Click(object sender, EventArgs e)
        {
            //clearAllLabelTags();
            secLabels[(int)curSection].Tag = "";
            Label label = sender as Label;
            label.Tag = "1";

            selectSection((Section) Array.IndexOf(secLabels, sender));

            
        }

        Font fontSectionSelected = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        Font fontSectionDefault = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        Font fontSectionHover = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

        Color colorSectionSelected = System.Drawing.SystemColors.WindowText;
        Color colorSectionDefault = System.Drawing.SystemColors.ControlDark;
        Color colorSectionHover = System.Drawing.SystemColors.ControlDark;

        void selectSection(Section section)
        {
            secUnderLabels[(int)section].Visible = true;
            secLabels[(int)section].ForeColor = colorSectionSelected;

            /*
            var t = new Transition(new TransitionType_EaseInEaseOut(200));
            t.add(this, "Left", this.Left+3);
            t.add(this, "Left", this.Left);
          
            t.run();
            */

            //gboxIntroAndNews.Visible = (section == Section.INTRODUCTION);

            if (section == Section.INTRODUCTION)
                gboxIntroAndNews.BringToFront();
            else
                gboxIntroAndNews.SendToBack();

            gboxTestArea.Visible = (section == Section.TEST_AREA);
            gboxHome.Visible = (section == Section.HOME);
            gboxSelection.Visible = (section == Section.SELECTION);
            

            if (curSection == section) return;

            //secLabels[(int)curSection].Font = fontSectionDefault;
            secLabels[(int)curSection].ForeColor = colorSectionDefault;
            Util.Animate(secUnderLabels[(int)curSection], Util.Effect.Slide, 100, 0);

            curSection = section;

            //secLabels[(int)curSection].Font = fontSectionSelected;
        }

        void hoverOffSection(Label label)
        {
            if (label == secLabels[(int)curSection]) return;
            
            //label.Font = fontSectionDefault;
            //label.ForeColor = colorSectionDefault;

            Util.Animate(secUnderLabels[Array.IndexOf(secLabels, label)], Util.Effect.Slide, 100, 0);
        }

        void hoverOnSection(Label label)
        {
            
            if (label == secLabels[(int)curSection]) return;
            
            //label.Font = fontSectionHover;
            //label.ForeColor = colorSectionHover;

            Util.Animate(secUnderLabels[Array.IndexOf(secLabels, label)], Util.Effect.Slide, 100, 0);

        }

        private void clearAllLabelTags()
        {
            label2.Tag = "";
            label3.Tag = "";

            label2.Font = fontSectionDefault;
            label2.ForeColor = System.Drawing.SystemColors.ControlDark;

            label3.Font = new System.Drawing.Font("Century Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label3.ForeColor = System.Drawing.SystemColors.ControlDark;
        }

        class Util
        {
            public enum Effect { Roll, Slide, Center, Blend }

            public static void Animate(Control ctl, Effect effect, int msec, int angle)
            {
                int flags = effmap[(int)effect];
                if (ctl.Visible) { flags |= 0x10000; angle += 180; }
                else
                {
                    if (ctl.TopLevelControl == ctl) flags |= 0x20000;
                    else if (effect == Effect.Blend) throw new ArgumentException();
                }
                flags |= dirmap[(angle % 360) / 45];
                bool ok = AnimateWindow(ctl.Handle, msec, flags);
                if (!ok) throw new Exception("Animation failed");
                ctl.Visible = !ctl.Visible;
            }

            private static int[] dirmap = { 1, 5, 4, 6, 2, 10, 8, 9 };
            private static int[] effmap = { 0, 0x40000, 0x10, 0x80000 };

            [DllImport("user32.dll")]
            private static extern bool AnimateWindow(IntPtr handle, int msec, int flags);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //
            //gert added
            //
            if (listBox1.SelectedIndex == -1) return;
            listviewGroup.Items.Clear();


            List<SubliGroup> groupValues = null;
            if (curVersion == VersionType.PRO)
            {
                groupValues = subliCategories.Categories[listBox1.SelectedIndex].Groups;//.Select(x => x.GroupName).ToArray();
            }
            else if (curVersion == VersionType.FREE)
            {
                int curGroupCount = subliCategories.Categories[listBox1.SelectedIndex].Groups.Count;
                if (listBox1.SelectedIndex == 0) //default category
                {
                        groupValues = subliCategories.Categories[listBox1.SelectedIndex].Groups.GetRange(0, Math.Min(curGroupCount, FreeDefaultGroupCount));//.Select(x => x.GroupName).ToArray();
                    
                }
                else if (listBox1.SelectedIndex >= 1) //custome category
                {
                        groupValues = subliCategories.Categories[listBox1.SelectedIndex].Groups.GetRange(0, Math.Min(curGroupCount, FreeCustomeGroupCount));//.Select(x => x.GroupName).ToArray();
                    
                }
                else
                {
                    
                    return;
                }
                
            }

            
            if (groupValues == null) return;

            foreach (var item in groupValues)
            {
                ListViewItem lvi = new ListViewItem(item.GroupName);

                lvi.Checked = mySelection.Contains(item);
                listviewGroup.Items.Add(lvi);
            }

            lblCurrentCategory.Text = subliCategories.Categories[listBox1.SelectedIndex].CategoryName;
            /*
            if (listviewGroup.Items.Count > 0)
            {
                //listviewGroup.Items[0].Selected = true;
                //listviewGroup.Select();
            }
            else
            {
                textBox2.Text = "";
                textBox3.Text = "";
                listView1.Items.Clear();
                listView2.Items.Clear();
            }
            */

            textBox2.Text = "";
            textBox3.Text = "";
            listViewAffImages.Items.Clear();
            listViewAffSuggestions.Items.Clear();
            pictureBox2.Image = null;

            if (listBox1.SelectedItem == null) return;
            textBox1.Text = listBox1.SelectedItem.ToString();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

         //Constants
        const int AW_SLIDE = 0X40000;
        const int AW_TO_RIGHT = 0X1;
        const int AW_TO_LEFT = 0X2;
        const int AW_BLEND = 0X80000;
 
        [DllImport("user32")]
        static extern bool AnimateWindow(IntPtr hwnd, int time, int flags);

        private void lblAffirmation_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1) return;
            if (listviewGroup.SelectedItems.Count == 0) return;

            lblCurrentCategory.Text = listBox1.Items[listBox1.SelectedIndex].ToString() + " > " + listviewGroup.SelectedItems[0].Text;

            if (sender == lblImageAffirmation)
            {
                lblCurrentCategory.Text += " > Images";
                curAffirmationType = AffirmationType.Image;
                if (listBox1.SelectedItem !=null && listviewGroup.SelectedItems.Count > 0)
                    LoadSuggestions2(listBox1.SelectedItem.ToString(), listviewGroup.SelectedItems[0].Text, lblImageAffirmation.Text, 1);
            }
            else if (sender == lblTextAffirmation)
            {
                lblCurrentCategory.Text += " > Suggestions";
                curAffirmationType = AffirmationType.Text;
                if (listBox1.SelectedItem != null && listviewGroup.SelectedItems.Count > 0)
                    LoadSuggestions2(listBox1.SelectedItem.ToString(), listviewGroup.SelectedItems[0].Text, lblTextAffirmation.Text, 1);
            }
        }

        private void lblAffirmation_MouseEnter(object sender, EventArgs e)
        {
            Label label = sender as Label;
            if (label == lblImageAffirmation && lblImageAffUnderline.Visible) return;
            if (label == lblTextAffirmation && lblTextAffUnderline.Visible) return;

            int flag1 = AW_SLIDE;
            int flag2 = AW_SLIDE;

            if (lblImageAffUnderline.Visible)
                flag1 |= 0x10000 | AW_TO_RIGHT;
            else
                flag1 |= AW_TO_LEFT;

            if (lblTextAffUnderline.Visible)
                flag2 |= 0x10000 | AW_TO_LEFT;
            else
                flag2 |= AW_TO_RIGHT;

            lblImageAffirmation.ForeColor = lblImageAffUnderline.Visible ? colorSectionHover : colorSectionSelected;
            lblTextAffirmation.ForeColor = lblTextAffUnderline.Visible ? colorSectionHover : colorSectionSelected;

            AnimateWindow(lblImageAffUnderline.Handle, 100, flag1);
            AnimateWindow(lblTextAffUnderline.Handle, 100, flag2);

            lblImageAffUnderline.Visible = !lblImageAffUnderline.Visible;
            lblTextAffUnderline.Visible = !lblTextAffUnderline.Visible;

            listViewAffImages.Visible = lblImageAffUnderline.Visible;
            listViewAffSuggestions.Visible = lblTextAffUnderline.Visible;

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        enum AffirmationType { Image, Text };
        AffirmationType curAffirmationType = AffirmationType.Image;
        //
        // gert
        //
        private void ImageSugg_MouseLeave(object sender, EventArgs e)
        {
            if (curAffirmationType == AffirmationType.Image)
            {
                Console.WriteLine("image");
                lblAffirmation_MouseEnter(lblImageAffirmation, null);
            }
            else if (curAffirmationType == AffirmationType.Text)
            {
                Console.WriteLine("suggestion");
                lblAffirmation_MouseEnter(lblTextAffirmation, null);
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewAffImages.SelectedItems.Count == 0)
            {
                lblCurrentCategory.Text = listBox1.Items[listBox1.SelectedIndex].ToString() + " > " + listviewGroup.SelectedItems[0].Text;
                //listView1.Items.Clear();
                //listView2.Items.Clear();
                pictureBox2.Image = null;

                textBox3.Text = "";
                return;
            }

            if (listBox1.SelectedIndex == -1) return;
            if (listviewGroup.SelectedItems.Count == 0) return;

            lblCurrentCategory.Text = listBox1.Items[listBox1.SelectedIndex].ToString() + " > " + listviewGroup.SelectedItems[0].Text + " > Images"  ;

            textBox3.Text = listViewAffImages.SelectedItems[0].Text;

            //Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + fi.Name;

            int catSelIndex = listBox1.SelectedIndex;
            int groupSelIndex = -1;
            if (listviewGroup.SelectedIndices.Count > 0)
                groupSelIndex = listviewGroup.SelectedIndices[0];
            int oldSelIndex = -1;
            if (listViewAffImages.SelectedIndices.Count > 0)
                oldSelIndex = listViewAffImages.SelectedIndices[0];

            if (groupSelIndex == -1 || catSelIndex == -1 || oldSelIndex == -1) return;

            try
            {
                string imgFile = subliCategories.Categories[catSelIndex].Groups[groupSelIndex].SubliImage.ImageFiles[oldSelIndex].Split(',')[0];
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                System.Drawing.Image image = System.Drawing.Image.FromFile(path + "\\" + imgFile);
                pictureBox2.Image = image;
                return;
            }
            catch
            {
            }

        }

        private void grpStartupMode_Enter(object sender, EventArgs e)
        {

        }
        
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            var listBox2 = sender as ListBox;
            if (listBox2.SelectedItem == null) return;
            textBox2.Text = listBox2.SelectedItem.ToString();

            if (listBox1.SelectedItem == null) return;
            
            LoadSuggestions2(listBox1.SelectedItem.ToString(), textBox2.Text, "Images", 0);

            if(curAffirmationType == AffirmationType.Image)
                lblAffirmation_Click(lblImageAffirmation, null);
            else if (curAffirmationType == AffirmationType.Text)
                lblAffirmation_Click(lblTextAffirmation, null);
        }
        
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewAffSuggestions.SelectedItems.Count == 0)
            {
                lblCurrentCategory.Text = listBox1.Items[listBox1.SelectedIndex].ToString() + " > " + listviewGroup.SelectedItems[0].Text;
                //listView1.Items.Clear();
                //listView2.Items.Clear();
                pictureBox2.Image = null;

                textBox3.Text = "";
                return;
            }

            lblCurrentCategory.Text = listBox1.Items[listBox1.SelectedIndex].ToString() + " > " + listviewGroup.SelectedItems[0].Text + " > Suggestions";
            textBox3.Text = listViewAffSuggestions.SelectedItems[0].Text;
        }

        bool isDetailView = true;
        private void button10_Click(object sender, EventArgs e)
        {
            isDetailView = !isDetailView;
            if (isDetailView)
            {
                listViewAffImages.View = View.Details;
                listViewAffSuggestions.View = View.Tile;
                listViewSelection.View = View.Tile;
            }
            else
            {
                listViewAffImages.View = View.LargeIcon;
                listViewAffSuggestions.View = View.List;
                listViewSelection.View = View.List;
            }

        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (curAffirmationType == AffirmationType.Image)
            {
                addImage2();
            }
            else
            {
                addSuggestion();
            }
        }

        private void addSuggestion()
        {
            if (!access)
            {
                string returned_str = show_upgrade_Message();
                MessageBox.Show(returned_str.Split('%')[1].Trim(), returned_str.Split('%')[0].Trim(), MessageBoxButtons.OK);
            }
            else
            {

                if (curVersion == VersionType.PRO)
                {

                }
                else if (curVersion == VersionType.FREE)
                {
                    if (listBox1.SelectedIndex == 0)
                    {
                        MessageBox.Show("Can't add more affirmations to default group in FREE version.", "SubliDesk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else if (listBox1.SelectedIndex >= 1)
                    {
                        if (listViewAffSuggestions.Items.Count >= FreeAffirmationCount)
                        {
                            MessageBox.Show("Can't add affirmations more than " + FreeAffirmationCount + " in FREE version.", "SubliDesk", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                string newValue = textBox3.Text.Trim();
                if (newValue == "") return;
                if (listViewAffSuggestions.Items.Cast<ListViewItem>().Any(x => x.Text == newValue))
                    return;
                listViewAffSuggestions.Items.Add(new ListViewItem(newValue));
            }
        }
        //
        //Gert
        //
        private void btnDelCategory_Click(object sender, EventArgs e)
        {
            int oldSelIndex = listBox1.SelectedIndex;
            if (oldSelIndex == -1) return;

            String newValue = listBox1.SelectedItem.ToString();
            listBox1.Items.Remove(listBox1.SelectedItem);
            //
            // Change Database Value
            //
            subliCategories.Categories.RemoveAt(oldSelIndex);
            //
            // Restore old selected index
            //
            if (oldSelIndex > listBox1.Items.Count - 1)
            {
                listBox1.SelectedIndex = listBox1.Items.Count - 1;
            }
            else
            {
                listBox1.SelectedIndex = oldSelIndex;
            }
            //
            // 
            //
            lblChangedMark.Visible = true;
        }

        private void btnDelGroup_Click(object sender, EventArgs e)
        {
            int catIndex = listBox1.SelectedIndex;
            int oldSelIndex = -1;
            if (listviewGroup.SelectedIndices.Count > 0)
                oldSelIndex = listviewGroup.SelectedIndices[0];
            if (oldSelIndex == -1 || catIndex == -1) return;

            String newValue = listBox1.SelectedItem.ToString();
            listviewGroup.Items.RemoveAt(oldSelIndex);
            //
            // Change Database Value
            //
            subliCategories.Categories[catIndex].Groups.RemoveAt(oldSelIndex);
            //
            // Restore old selected index
            //
            if (listviewGroup.Items.Count == 0)
            {
            }
            else if (oldSelIndex > listviewGroup.Items.Count - 1)
            {
                //listBox2.SelectedIndex = listBox2.Items.Count - 1;
                listviewGroup.Items[listviewGroup.Items.Count - 1].Selected = true;
            }
            else
            {
                //listBox2.SelectedIndex = oldSelIndex;
                listviewGroup.Items[oldSelIndex].Selected = true;
            }
            listviewGroup.Select();
            //
            // 
            //
            lblChangedMark.Visible = true;
        }

        private void btnDelItem_Click(object sender, EventArgs e)
        {
            int catSelIndex = listBox1.SelectedIndex;
            int groupSelIndex = -1;
            if (listviewGroup.SelectedIndices.Count > 0)
                groupSelIndex = listviewGroup.SelectedIndices[0];
            int oldSelIndex = -1;

            if (curAffirmationType == AffirmationType.Image)
            {
                if (listViewAffImages.SelectedIndices.Count > 0)
                    oldSelIndex = listViewAffImages.SelectedIndices[0];

                if (groupSelIndex == -1 || catSelIndex == -1 || oldSelIndex == -1) return;

                listViewAffImages.Items.RemoveAt(oldSelIndex);
                subliCategories.Categories[catSelIndex].Groups[groupSelIndex].SubliImage.ImageFiles.RemoveAt(oldSelIndex);
            }
            else
            {

                if (listViewAffSuggestions.SelectedIndices.Count > 0)
                    oldSelIndex = listViewAffSuggestions.SelectedIndices[0];

                if (groupSelIndex == -1 || catSelIndex == -1 || oldSelIndex == -1) return;
                //
                // DEL Value (Add new value and remove old value)
                //
                listViewAffSuggestions.Items.RemoveAt(oldSelIndex);
                //
                // Change Database Value
                //
                subliCategories.Categories[catSelIndex].Groups[groupSelIndex].SubliSuggestion.Suggestions.RemoveAt(oldSelIndex);
                //
                // Restore old selected index
                //
                //listBox2.SelectedIndex = oldSelIndex;
                //
                //
                //
            }
            lblChangedMark.Visible = true;
        }
        //
        // Gert
        //
        private void btnChangeCategory_Click(object sender, EventArgs e)
        {
            int oldSelIndex = listBox1.SelectedIndex;
            if (oldSelIndex == -1) return;
            //
            // Check if exist same value
            //
            String newValue = textBox1.Text;
            if (listBox1.Items.Cast<Object>().Any(x => x.ToString() == newValue))
            {
                return;
            }
            //
            // EDIT Value (Add new value and remove old value)
            //
            listBox1.Items.Insert(oldSelIndex, newValue);
            listBox1.Items.Remove(listBox1.SelectedItem);
            //
            // Change Database Value
            //
            subliCategories.Categories[oldSelIndex].CategoryName = newValue;
            //
            // Restore old selected index
            //
            listBox1.SelectedIndex = oldSelIndex;
            //
            // 
            //
            lblChangedMark.Visible = true;
        }

        private void btnEditGroup_Click(object sender, EventArgs e)
        {
            int oldSelIndex = -1;
            if (listviewGroup.SelectedIndices.Count > 0)
                oldSelIndex = listviewGroup.SelectedIndices[0];
            int catSelIndex = listBox1.SelectedIndex;

            if (oldSelIndex == -1 || catSelIndex == -1) return;
            //
            // Check if exist same value
            //
            String newValue = textBox2.Text;
            if (listviewGroup.Items.Cast<Object>().Any(x => x.ToString() == newValue))
            {
                return;
            }
            //
            // EDIT Value (Add new value and remove old value)
            //
            listviewGroup.Items[oldSelIndex].Text =  newValue;
            //
            // Change Database Value
            //
            subliCategories.Categories[catSelIndex].Groups[oldSelIndex].GroupName = newValue;
            //
            // Restore old selected index
            //
            //listBox2.SelectedIndex = oldSelIndex;
            listviewGroup.Items[oldSelIndex].Selected = true;
            listviewGroup.Select();
            listviewGroup_SelectedIndexChanged(listviewGroup, null);
            //
            //
            //
            lblChangedMark.Visible = true;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                btnChangeCategory_Click(null, null);
            }
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                btnEditGroup_Click(null, null);
            }
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                btnEditSuggestion_Click(null, null);
            }
        }

        private void btnEditSuggestion_Click(object sender, EventArgs e)
        {
            int catSelIndex = listBox1.SelectedIndex;
            int groupSelIndex = -1;
            if (listviewGroup.SelectedIndices.Count > 0)
                groupSelIndex = listviewGroup.SelectedIndices[0];
            int oldSelIndex = -1;
            if (listViewAffSuggestions.SelectedIndices.Count > 0)
                oldSelIndex = listViewAffSuggestions.SelectedIndices[0];

            if (groupSelIndex == -1 || catSelIndex == -1 || oldSelIndex == -1) return;
            //
            // Check if exist same value
            //
            String newValue = textBox3.Text;
            if (listViewAffSuggestions.Items.Cast<Object>().Any(x => x.ToString() == newValue))
            {
                return;
            }
            //
            // EDIT Value (Add new value and remove old value)
            //
            listViewAffSuggestions.Items[oldSelIndex].Text = newValue;
            //
            // Change Database Value
            //
            subliCategories.Categories[catSelIndex].Groups[groupSelIndex].SubliSuggestion.Suggestions[oldSelIndex] = newValue;
            //
            // Restore old selected index
            //
            //listBox2.SelectedIndex = oldSelIndex;
            //
            //
            //
            lblChangedMark.Visible = true;
        }

        private void listviewGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listviewGroup.SelectedIndices.Count == 0)
            {
                lblCurrentCategory.Text = listBox1.Items[listBox1.SelectedIndex].ToString();
                listViewAffImages.Items.Clear();
                listViewAffSuggestions.Items.Clear();
                pictureBox2.Image = null;

                textBox2.Text = "";
                textBox3.Text = "";
                return;
            }

            textBox2.Text = listviewGroup.SelectedItems[0].Text;

            if (listBox1.SelectedItem == null) return;

            LoadSuggestions2(listBox1.SelectedItem.ToString(), textBox2.Text, "Images", 0);

            lblCurrentCategory.Text = listBox1.Items[listBox1.SelectedIndex].ToString() + " > " + listviewGroup.SelectedItems[0].Text;

            /*
            if (curAffirmationType == AffirmationType.Image)
                lblAffirmation_Click(lblImageAffirmation, null);
            else if (curAffirmationType == AffirmationType.Text)
                lblAffirmation_Click(lblTextAffirmation, null);
            */
        }

        List<SubliGroup> mySelection = new List<SubliGroup>();
        private void listviewGroup_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            int catSelIndex = listBox1.SelectedIndex;
            if (catSelIndex == -1) return;

            int selIndex = e.Item.Index;
            


            string newValue = e.Item.Text;
            //
            // exist value
            //
            if (listViewSelection.Items.Cast<ListViewItem>().Any(x => x.Text == newValue))
            {
                if (e.Item.Checked == false)
                {
                    var items = listViewSelection.Items.Cast<ListViewItem>().First(x => x.Text == newValue);
                    if (items != null)
                    {
                        listViewSelection.Items.Remove((ListViewItem)items);
                        var sug = mySelection.First(x => x.GroupName == e.Item.Text);
                        if(sug != null)
                            mySelection.Remove(sug);
                    }
                }
            }
            //
            // no value
            //
            else
            {
                if (e.Item.Checked == true)
                {
                    listViewSelection.Items.Add(new ListViewItem(e.Item.Text));
                    mySelection.Add(subliCategories.Categories[catSelIndex].Groups[e.Item.Index]);
                    
                    //foreach(int sel in listView3.SelectedIndices) {
                        
                    //}
                    
                }
            }
            if (listViewSelection.Items.Count > 0)
            {
                lblSelectionGap.Visible = true;
                lblSelectionGap.Text = "(" + listViewSelection.Items.Count + ")";
            }
            else
            {
                lblSelectionGap.Visible = false;
                lblSelectionGap.Text = "";
            }
        }

        private void listviewGroup_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            int oldSelIndex = e.Item;
            int catSelIndex = listBox1.SelectedIndex;

            if (oldSelIndex == -1 || catSelIndex == -1) return;
            //
            // Check if exist same value
            //
            String newValue = e.Label;
            if (newValue == null) return;

            if (listviewGroup.Items.Cast<ListViewItem>().Any(x => x.Text == newValue))
            {
                e.CancelEdit = true;
                return;
            }
            //
            // Change Database Value
            //
            subliCategories.Categories[catSelIndex].Groups[oldSelIndex].GroupName = newValue;
            //
            //
            //
            lblChangedMark.Visible = true;
            return;
        }

        private void listviewGroup_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            return;
        }

        private void btnAppTest_Click(object sender, EventArgs e)
        {
            //gert added, setting global options
            globalFont = lblSelectedFont.Font;
            globalFontColor = lblSelectedFont.ForeColor;
            screenPostion = Convert.ToInt32(cmbSplashingPosition.SelectedValue.ToString());

            bool_stop = false;
            start_execute();
            this.Show();
            this.Activate();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start(HOME_URL);
            browser.ViewSource();
            Console.WriteLine(browser.Text);
        }

        private string HOME_URL = "www.sublidesk.com";

        public const int FreeCategoryCount = 2;
        public const int FreeDefaultGroupCount = 3;
        public const int FreeCustomeGroupCount = 1;
        public const int FreeAffirmationCount = 5;

        public const int globalDurationInMillisecondsFree = 271;
        public const int globalSplashingPeriodInSecondsFree = 5;
        public const int globalOpacityFree = 100;
        public const int globalPositionFree = 1;

        public const int globalFontSizeFree = 16;
        public static Font globalFontFree = new Font(FontFamily.GenericSansSerif, globalFontSizeFree);
        public static Color globalFontColorFree = Color.Red;
        

        private void button15_MouseEnter(object sender, EventArgs e)
        {
            this.button15.BackgroundImage = global::SubliMaster.Properties.Resources.ProButton_hover;
        }

        private void button15_MouseLeave(object sender, EventArgs e)
        {
            this.button15.BackgroundImage = global::SubliMaster.Properties.Resources.ProButton;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(HOME_URL);
        }
    }
}