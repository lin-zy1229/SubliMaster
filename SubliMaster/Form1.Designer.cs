namespace SubliMaster
{
    partial class Form1
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
            this.lbl_text_1 = new System.Windows.Forms.Label();
            this.btn_register = new System.Windows.Forms.Button();
            this.btn_signup = new System.Windows.Forms.Button();
            this.lbl_text_2 = new System.Windows.Forms.Label();
            this.lbl_Language = new System.Windows.Forms.Label();
            this.cmb_languages = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // lbl_text_1
            // 
            this.lbl_text_1.AutoSize = true;
            this.lbl_text_1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_text_1.Location = new System.Drawing.Point(90, 25);
            this.lbl_text_1.Name = "lbl_text_1";
            this.lbl_text_1.Size = new System.Drawing.Size(341, 13);
            this.lbl_text_1.TabIndex = 0;
            this.lbl_text_1.Text = "YOU HAVE TO REGISTER TO UTILIZE THE SOFTWARE. ";
            // 
            // btn_register
            // 
            this.btn_register.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_register.Location = new System.Drawing.Point(75, 115);
            this.btn_register.Name = "btn_register";
            this.btn_register.Size = new System.Drawing.Size(111, 23);
            this.btn_register.TabIndex = 1;
            this.btn_register.Text = "REGISTER";
            this.btn_register.UseVisualStyleBackColor = true;
            // 
            // btn_signup
            // 
            this.btn_signup.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btn_signup.Location = new System.Drawing.Point(231, 115);
            this.btn_signup.Name = "btn_signup";
            this.btn_signup.Size = new System.Drawing.Size(184, 23);
            this.btn_signup.TabIndex = 2;
            this.btn_signup.Text = "SIGNUP WITH REGISTER INFO\'S";
            this.btn_signup.UseVisualStyleBackColor = true;
            // 
            // lbl_text_2
            // 
            this.lbl_text_2.AutoSize = true;
            this.lbl_text_2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_text_2.Location = new System.Drawing.Point(81, 47);
            this.lbl_text_2.Name = "lbl_text_2";
            this.lbl_text_2.Size = new System.Drawing.Size(350, 13);
            this.lbl_text_2.TabIndex = 3;
            this.lbl_text_2.Text = " CLICK ON THE RIGHT BUTTON TO GET THE SERIAL KEY";
            // 
            // lbl_Language
            // 
            this.lbl_Language.AutoSize = true;
            this.lbl_Language.Location = new System.Drawing.Point(112, 84);
            this.lbl_Language.Name = "lbl_Language";
            this.lbl_Language.Size = new System.Drawing.Size(55, 13);
            this.lbl_Language.TabIndex = 4;
            this.lbl_Language.Text = "Language";
            // 
            // cmb_languages
            // 
            this.cmb_languages.FormattingEnabled = true;
            this.cmb_languages.Location = new System.Drawing.Point(220, 81);
            this.cmb_languages.Name = "cmb_languages";
            this.cmb_languages.Size = new System.Drawing.Size(121, 21);
            this.cmb_languages.TabIndex = 5;
            this.cmb_languages.SelectedValueChanged += new System.EventHandler(this.cmb_languages_SelectedValueChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(481, 166);
            this.Controls.Add(this.cmb_languages);
            this.Controls.Add(this.lbl_Language);
            this.Controls.Add(this.lbl_text_2);
            this.Controls.Add(this.btn_signup);
            this.Controls.Add(this.btn_register);
            this.Controls.Add(this.lbl_text_1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SubliDesk";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_text_1;
        private System.Windows.Forms.Button btn_register;
        private System.Windows.Forms.Button btn_signup;
        private System.Windows.Forms.Label lbl_text_2;
        private System.Windows.Forms.Label lbl_Language;
        private System.Windows.Forms.ComboBox cmb_languages;
    }
}