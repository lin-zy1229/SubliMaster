
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SubliMaster
{
    public partial class TextSplash : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, uint windowStyle);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        private const int bytesPerPixel = 4;
        delegate void SplashShowCloseDelegate();
        private SubliCurrentSuggestions subliSuggestionsEntity = null;
        private SubliCategories subliCategories;

        /// <summary>
        /// To ensure splash screen is closed using the API and not by keyboard or any other things
        /// </summary>
        bool CloseSplashScreenFlag = false;
        /// <summary>
        /// Displays the splashscreen
        /// </summary>
        public void ShowSplashScreen()
        {            
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new SplashShowCloseDelegate(ShowSplashScreen)); 
                return;
            }
            
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var subli1 = SubliDataManager.Read(path + "\\Subli.xml");
            subliCategories = subli1;
            if (subli1.Categories != null)
            {
                var subli = subli1.Categories[0];
                int position = subli.Groups[0].SubliImage.SplashPosition;
                if (position == 1)
                {
                    this.StartPosition=System.Windows.Forms.FormStartPosition.CenterScreen;
                }
            }
            this.TopMost = true;
            //this.Show();

            ShowWindow(this.Handle, 1u);
            SetWindowPos(this.Handle, new IntPtr(-1), 0, 0, 0, 0, 3u);


            this.Activate();
            Application.Run(this);
            
        }
        public TextSplash(SubliCurrentSuggestions sse)
        {
            InitializeComponent();
            subliSuggestionsEntity = sse;

            lblText.Text = sse.CurrentSuggestion;

            //modified by gert for font

            /*
            lblText.Font = new FontConverter().ConvertFromString(sse.SubliSuggestionsEntity.SelectedFont) as Font;
            lblText.ForeColor = Color.FromName(sse.SubliSuggestionsEntity.FontColor);
            Bitmap bmp = TextToBitmap(subliSuggestionsEntity.CurrentSuggestion, new FontConverter().ConvertFromString(subliSuggestionsEntity.SubliSuggestionsEntity.SelectedFont) as Font);
            */

            if (SubliMasterMain.curVersion == SubliMasterMain.VersionType.PRO)
            {
                lblText.Font = new Font(SubliMasterMain.globalFont.FontFamily, SubliMasterMain.globalFont.Size, SubliMasterMain.globalFont.Style);
                lblText.ForeColor = SubliMasterMain.globalFontColor;
            }
            else if (SubliMasterMain.curVersion == SubliMasterMain.VersionType.FREE)
            {
                lblText.Font = new Font(SubliMasterMain.globalFontFree.FontFamily, SubliMasterMain.globalFontFree.Size, SubliMasterMain.globalFontFree.Style);
                lblText.ForeColor = SubliMasterMain.globalFontColorFree;
            }


                Bitmap bmp = TextToBitmap(subliSuggestionsEntity.CurrentSuggestion, new Font(lblText.Font.FontFamily, lblText.Font.Size, lblText.Font.Style));


            this.Width = bmp.Width + 15;
            this.Height = bmp.Height + 5;
            //double opacity = (subliSuggestionsEntity.SubliSuggestionsEntity.Opacity);
            //double percent = (opacity / 255) * 100;

            //this.Opacity = (percent / 100);
            if (SubliMasterMain.curVersion == SubliMasterMain.VersionType.PRO)
            {
                this.Opacity = double.Parse(subliSuggestionsEntity.SubliSuggestionsEntity.Opacity.ToString()) * 0.01;

                if (subliSuggestionsEntity.SubliSuggestionsEntity.BackgroundColor == "Transparent")
                {
                    this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                    this.TransparencyKey = Color.FromKnownColor(KnownColor.Control);
                    this.Update();
                }
                else
                {
                    this.BackColor = Color.FromName(subliSuggestionsEntity.SubliSuggestionsEntity.BackgroundColor);
                    this.Update();
                }
            }
            else if (SubliMasterMain.curVersion == SubliMasterMain.VersionType.FREE)
            {
                this.Opacity = SubliMasterMain.globalOpacityFree * 0.01;

                
                    this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                    this.TransparencyKey = Color.FromKnownColor(KnownColor.Control);
                    this.Update();
                
            }

            
            /*
            int transparency = (subliSuggestionsEntity.SubliSuggestionsEntity.Transparency);
            
            if (transparency == 1)
            {
                this.Opacity = 0.3;

            }
            */
            //gert, modified for global position
            //int position = (subliSuggestionsEntity.SubliSuggestionsEntity.SplashPosition);

            int position = SubliMasterMain.screenPostion;
            if (SubliMasterMain.curVersion == SubliMasterMain.VersionType.PRO)
            {
                if (position == 1)
                {
                    this.StartPosition = FormStartPosition.CenterScreen;
                }
                else
                {
                    // gert modified for random screen.
                    //this.StartPosition = FormStartPosition.CenterParent;

                    this.StartPosition = FormStartPosition.Manual;
                    Random rand = new Random();
                    int width = 0;
                    foreach (var screen in Screen.AllScreens)
                    {
                        width += screen.Bounds.Width;
                    }
                    int x = rand.Next(width - this.Width);
                    int y = rand.Next(Screen.PrimaryScreen.Bounds.Height - this.Height);

                    this.Location = new Point(x, y);
                }
            }
            else if (SubliMasterMain.curVersion == SubliMasterMain.VersionType.FREE)
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        /// <summary>
        /// converts string to image
        /// </summary>
        /// <param name="TheText"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public Bitmap TextToBitmap(String TheText,Font f)
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
                //DrawFont = new Font("Arial", 16);
                DrawFont = f;

                // see how big the text will be
                int Width = (int)DrawGraphics.MeasureString(TheText, DrawFont).Width;
                int Height = (int)DrawGraphics.MeasureString(TheText, DrawFont).Height;


                // recreate the bitmap and graphic object with the new size
                TextBitmap = new Bitmap(TextBitmap, Width, Height);
                DrawGraphics = Graphics.FromImage(TextBitmap);


                // get the drawing brush and where we're going to draw
                DrawBrush = new SolidBrush(Color.Black);
                PointF DrawPoint = new PointF(0, 0);


                // clear the graphic white and draw the string
                DrawGraphics.Clear(Color.White);
                DrawGraphics.DrawString(TheText, DrawFont, DrawBrush, DrawPoint);


                return TextBitmap;
            }
            finally
            {
                // don't dispose the bitmap, the caller needs it.
                DrawFont.Dispose();
                    if(DrawBrush != null)
                        DrawBrush.Dispose();
                    if (DrawGraphics != null)
                DrawGraphics.Dispose();
            }
        }
        /// <summary>
        /// Closes the SplashScreen
        /// </summary>
        public void CloseSplashScreen()
        {
            if (InvokeRequired)
            {
                // We're not in the UI thread, so we need to call BeginInvoke
                BeginInvoke(new SplashShowCloseDelegate(CloseSplashScreen));
                return;
            }
            CloseSplashScreenFlag = true;
            this.Close();
        }




        /// <summary>
        /// not using yet
        /// </summary>
        /// <param name="originalImage"></param>
        /// <param name="transparency"></param>
        /// <returns></returns>
        public Bitmap ChangeImageTransparency(Bitmap originalImage, int transparency)
        {
            Bitmap bmp = originalImage; 
            for(int x=1;x<bmp.Width;x++){
                for (int y = 1; y < bmp.Height;y++ )
                {
                    Color c = bmp.GetPixel(x, y);
                    Color n = Color.FromArgb(20, c.R, c.G, c.B);
                    bmp.SetPixel(x, y, n);
                }
            }
         /*   
            if ((originalImage.PixelFormat & PixelFormat.Indexed) == PixelFormat.Indexed)
            {
                // Cannot modify an image with indexed colors
                return originalImage;
            }

            Bitmap bmp = (Bitmap)originalImage.Clone();

            // Specify a pixel format.
            PixelFormat pxf = PixelFormat.Format32bppArgb;

            // Lock the bitmap's bits.
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, pxf);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            // This code is specific to a bitmap with 32 bits per pixels 
            // (32 bits = 4 bytes, 3 for RGB and 1 byte for alpha).
            int numBytes = bmp.Width * bmp.Height * bytesPerPixel;
            byte[] argbValues = new byte[numBytes];

            // Copy the ARGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, argbValues, 0, numBytes);

            // Manipulate the bitmap, such as changing the
            // RGB values for all pixels in the the bitmap.
            for (int counter = 0; counter < argbValues.Length; counter += bytesPerPixel)
            {
                // argbValues is in format BGRA (Blue, Green, Red, Alpha)

                // If 100% transparent, skip pixel
                if (argbValues[counter + bytesPerPixel - 1] == 0)
                    continue;

                int pos = 0;
                pos++; // B value
                pos++; // G value
                pos++; // R value

                if (transparency == 0) { transparency = 1; }
                argbValues[counter + pos] = (byte)(argbValues[counter + pos] * transparency);
            }

            // Copy the ARGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, numBytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);*/

            return bmp;
        }
        
    }
}
