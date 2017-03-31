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
    public partial class ImageSplash : Form
    {
        private const int bytesPerPixel = 4;
        delegate void SplashShowCloseDelegate();
        private SubliCurrentImages subliImagesEntity = null;

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, uint windowStyle);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);


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
            
            //this.Show();
            ShowWindow(this.Handle, 1u);
            SetWindowPos(this.Handle, new IntPtr(-1), 0, 0, 0, 0, 3u);

            this.Activate();
            Application.Run(this);
            
        }
        public ImageSplash(SubliCurrentImages img)
        {
            
            InitializeComponent();
            //this.Opacity = img.SubliImagesEntity.Opacity;
            
            this.Hide();
            subliImagesEntity = img;
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            System.Drawing.Image image = System.Drawing.Image.FromFile(path + "\\" + img.CurrentImage.Split(',')[0]);
            this.Width = this.splashImage.Width = image.Width;
            this.Height = this.splashImage.Height = image.Height;
            Bitmap bm = new Bitmap(path + "\\" + img.CurrentImage.Split(',')[0]);
            //double opacity = (subliImagesEntity.SubliImagesEntity.Opacity);
            //double percent = (opacity / 255) * 100;
            //this.Opacity = (percent / 100);
            splashImage.Image = bm;

            if (SubliMasterMain.curVersion == SubliMasterMain.VersionType.PRO)
            {
                this.Opacity = double.Parse(subliImagesEntity.SubliImagesEntity.Opacity.ToString()) * 0.01;
            }
            else if (SubliMasterMain.curVersion == SubliMasterMain.VersionType.FREE)
            {
                this.Opacity = SubliMasterMain.globalOpacityFree * 0.01;
            }
            //ChangeImageOpacity(bm, this.Opacity);


            this.Update();
            int transparency = (subliImagesEntity.SubliImagesEntity.Transparency);
            /*
            if (transparency == 1)
            {
                this.Opacity = 0.3;
            }
            */
            // gert, modified for global position
            //int position = (subliImagesEntity.SubliImagesEntity.SplashPosition);
            int position = SubliMasterMain.screenPostion;
            if (SubliMasterMain.curVersion == SubliMasterMain.VersionType.PRO)
            {
                if (position == 1)
                {
                    this.StartPosition = FormStartPosition.CenterScreen;
                }
                else
                {
                    // gert, modified for correct random position
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
            }else if(SubliMasterMain.curVersion == SubliMasterMain.VersionType.FREE)
            {
                this.StartPosition = FormStartPosition.CenterScreen;
            }
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.TransparencyKey = Color.FromKnownColor(KnownColor.Control);
            this.TopMost = true;
            this.TopLevel = true;

            this.Update();
            
        }

        /// <summary>
        /// Change the opacity of an image
        /// </summary>
        /// <param name="originalImage">The original image</param>
        /// <param name="opacity">Opacity, where 1.0 is no opacity, 0.0 is full transparency</param>
        /// <returns>The changed image</returns>
        public  Image ChangeImageOpacity(Image originalImage, double opacity)
        {
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

                if (opacity == 0) { opacity = 1; }
                argbValues[counter + pos] = (byte)(argbValues[counter + pos] * opacity);
            }

            // Copy the ARGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(argbValues, 0, ptr, numBytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);
            return bmp;
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
        /// window load event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImageSplash_Load(object sender, EventArgs e)
        {
           
        }
    }
}
