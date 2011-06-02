namespace PdfBookReader.UI
{
    partial class ReadingPanel
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.bLibrary = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bLibrary
            // 
            this.bLibrary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.bLibrary.Location = new System.Drawing.Point(657, 478);
            this.bLibrary.Name = "bLibrary";
            this.bLibrary.Size = new System.Drawing.Size(76, 36);
            this.bLibrary.TabIndex = 0;
            this.bLibrary.Text = "Library";
            this.bLibrary.UseVisualStyleBackColor = true;
            this.bLibrary.Click += new System.EventHandler(this.bLibrary_Click);
            // 
            // ReadingPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this.bLibrary);
            this.Name = "ReadingPanel";
            this.Size = new System.Drawing.Size(733, 514);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bLibrary;
    }
}
