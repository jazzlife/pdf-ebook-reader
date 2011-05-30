namespace PDFViewer.Reader
{
    partial class LibraryPanel
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
            this.lbBooks = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // lbBooks
            // 
            this.lbBooks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbBooks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lbBooks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lbBooks.Location = new System.Drawing.Point(3, 27);
            this.lbBooks.Name = "lbBooks";
            this.lbBooks.Size = new System.Drawing.Size(506, 583);
            this.lbBooks.TabIndex = 0;
            this.lbBooks.UseCompatibleStateImageBehavior = false;
            this.lbBooks.View = System.Windows.Forms.View.List;
            this.lbBooks.DoubleClick += new System.EventHandler(this.lbBooks_DoubleClick);
            this.lbBooks.KeyUp += new System.Windows.Forms.KeyEventHandler(this.lbBooks_KeyUp);
            // 
            // LibraryPanel
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this.lbBooks);
            this.Name = "LibraryPanel";
            this.Size = new System.Drawing.Size(512, 613);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.LibraryPanel_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.LibraryPanel_DragEnter);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lbBooks;
    }
}
