using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SubliMaster
{    
    /// <summary>
    /// Initiate instance of SplashScreen
    /// </summary>
    public class SplashScreen
    {
        private ImageSplash imgSplash = null;
        
        /// <summary>
        /// Displays the splashscreen
        /// </summary>
        public  void ShowSplashScreen(object img)
        {
            if (imgSplash == null)
            {
                imgSplash = new ImageSplash((SubliCurrentImages)img);

                imgSplash.TopLevel = true;
                imgSplash.TopMost = true;

                imgSplash.ShowSplashScreen();
                try
                {
                    //imgSplash.TopMost = true;
                }
                catch { }
            }
        }

        /// <summary>
        /// Closes the SplashScreen
        /// </summary>
        public void CloseSplashScreen()
        {
            if (imgSplash != null)
            {
                imgSplash.CloseSplashScreen();
                imgSplash = null;
            }
        }        
    }
}
