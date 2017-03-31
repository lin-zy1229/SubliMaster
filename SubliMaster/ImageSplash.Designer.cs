namespace SubliMaster
{
    partial class ImageSplash
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splashImage = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.splashImage)).BeginInit();
            this.SuspendLayout();
            // 
            // splashImage
            // 
            this.splashImage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.splashImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splashImage.InitialImage = null;
            this.splashImage.Location = new System.Drawing.Point(0, 0);
            this.splashImage.Name = "splashImage";
            this.splashImage.Size = new System.Drawing.Size(695, 454);
            this.splashImage.TabIndex = 0;
            this.splashImage.TabStop = false;
            // 
            // ImageSplash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(695, 454);
            this.Controls.Add(this.splashImage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImageSplash";
            this.Opacity = 0.3D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ImageSplash";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ImageSplash_Load);
            ((System.ComponentModel.ISupportInitialize)(this.splashImage)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox splashImage;
    }
}