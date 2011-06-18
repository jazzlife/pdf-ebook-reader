namespace PdfBookReader.UI
{
    partial class MainForm
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
            this.pReading = new PdfBookReader.UI.ReadingPanel();
            this.pLibrary = new PdfBookReader.UI.LibraryPanel();
            this.SuspendLayout();
            // 
            // pReading
            // 
            this.pReading.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pReading.Location = new System.Drawing.Point(467, 0);
            this.pReading.Name = "pReading";
            this.pReading.Size = new System.Drawing.Size(318, 597);
            this.pReading.TabIndex = 1;
            this.pReading.GoToLibrary += new System.EventHandler(this.pReading_GoToLibrary);
            // 
            // pLibrary
            // 
            this.pLibrary.AllowDrop = true;
            this.pLibrary.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pLibrary.Location = new System.Drawing.Point(0, 0);
            this.pLibrary.Name = "pLibrary";
            this.pLibrary.Size = new System.Drawing.Size(461, 597);
            this.pLibrary.TabIndex = 0;
            this.pLibrary.OpenBook += new System.EventHandler<PdfBookReader.UI.OpenBookEventArgs>(this.pLibrary_OpenBook);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(783, 635);
            this.Controls.Add(this.pReading);
            this.Controls.Add(this.pLibrary);
            this.Name = "MainForm";
            this.Text = "PDF eBook Reader";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private LibraryPanel pLibrary;
        private PdfBookReader.UI.ReadingPanel pReading;
    }
}