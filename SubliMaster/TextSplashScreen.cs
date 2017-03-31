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
    public class TextSplashScreen
    {
        private TextSplash txtSplash = null;
        
        /// <summary>
        /// Displays the splashscreen
        /// </summary>
        public void ShowSplashScreen(object scg)
        {
            if (txtSplash == null)
            {
                txtSplash = new TextSplash((SubliCurrentSuggestions)scg);
                txtSplash.TopMost = true;
                txtSplash.TopLevel = true;
                txtSplash.ShowSplashScreen();
            }
        }

        /// <summary>
        /// Closes the SplashScreen
        /// </summary>
        public void CloseSplashScreen()
        {
            if (txtSplash != null)
            {
                txtSplash.CloseSplashScreen();
                txtSplash = null;
            }
        }
    }
}

