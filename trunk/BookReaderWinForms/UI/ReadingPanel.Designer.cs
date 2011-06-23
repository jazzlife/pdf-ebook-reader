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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReadingPanel));
            this.pbContent = new System.Windows.Forms.PictureBox();
            this.pMargins = new System.Windows.Forms.Panel();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.bLibrary = new System.Windows.Forms.ToolStripButton();
            this.bPrevPage = new System.Windows.Forms.ToolStripButton();
            this.bNextPage = new System.Windows.Forms.ToolStripButton();
            this.lbPageNum = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.bWidthPlus = new System.Windows.Forms.ToolStripButton();
            this.bWidthMinus = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.bPaperWhite = new System.Windows.Forms.ToolStripButton();
            this.bPaperBlack = new System.Windows.Forms.ToolStripButton();
            this.bPaperSepia = new System.Windows.Forms.ToolStripButton();
            this.timerResize = new System.Windows.Forms.Timer(this.components);
            this.timerCacheDisplay = new System.Windows.Forms.Timer(this.components);
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.pbBrigthness = new PdfBookReader.UI.BookProgressBar();
            this.bookProgressBar = new PdfBookReader.UI.BookProgressBar();
            this.bStartPage = new System.Windows.Forms.ToolStripButton();
            ((System.ComponentModel.ISupportInitialize)(this.pbContent)).BeginInit();
            this.pMargins.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbContent
            // 
            this.pbContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbContent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            this.pbContent.Location = new System.Drawing.Point(70, 20);
            this.pbContent.Name = "pbContent";
            this.pbContent.Size = new System.Drawing.Size(560, 550);
            this.pbContent.TabIndex = 1;
            this.pbContent.TabStop = false;
            this.pbContent.Resize += new System.EventHandler(this.pbContent_Resize);
            // 
            // pMargins
            // 
            this.pMargins.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.pMargins.BackColor = System.Drawing.Color.White;
            this.pMargins.Controls.Add(this.pbContent);
            this.pMargins.Location = new System.Drawing.Point(128, 0);
            this.pMargins.Name = "pMargins";
            this.pMargins.Size = new System.Drawing.Size(700, 590);
            this.pMargins.TabIndex = 2;
            // 
            // toolStrip
            // 
            this.toolStrip.AutoSize = false;
            this.toolStrip.BackColor = System.Drawing.SystemColors.ControlDark;
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.Left;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bLibrary,
            this.bStartPage,
            this.bPrevPage,
            this.bNextPage,
            this.lbPageNum,
            this.toolStripLabel2,
            this.toolStripSeparator1,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.bWidthPlus,
            this.bWidthMinus,
            this.toolStripSeparator3,
            this.toolStripLabel3,
            this.bPaperWhite,
            this.bPaperBlack,
            this.bPaperSepia});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(60, 600);
            this.toolStrip.TabIndex = 3;
            this.toolStrip.Text = "toolBar";
            // 
            // bLibrary
            // 
            this.bLibrary.Image = ((System.Drawing.Image)(resources.GetObject("bLibrary.Image")));
            this.bLibrary.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bLibrary.Name = "bLibrary";
            this.bLibrary.Size = new System.Drawing.Size(58, 35);
            this.bLibrary.Text = "Library";
            this.bLibrary.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.bLibrary.Click += new System.EventHandler(this.bLibrary_Click);
            // 
            // bPrevPage
            // 
            this.bPrevPage.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.bPrevPage.Image = ((System.Drawing.Image)(resources.GetObject("bPrevPage.Image")));
            this.bPrevPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bPrevPage.Name = "bPrevPage";
            this.bPrevPage.Size = new System.Drawing.Size(58, 35);
            this.bPrevPage.Text = "Previous";
            this.bPrevPage.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.bPrevPage.Click += new System.EventHandler(this.bPrevPage_Click);
            // 
            // bNextPage
            // 
            this.bNextPage.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.bNextPage.Image = ((System.Drawing.Image)(resources.GetObject("bNextPage.Image")));
            this.bNextPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bNextPage.Name = "bNextPage";
            this.bNextPage.Size = new System.Drawing.Size(58, 35);
            this.bNextPage.Text = "Next";
            this.bNextPage.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.bNextPage.Click += new System.EventHandler(this.bNextPage_Click);
            // 
            // lbPageNum
            // 
            this.lbPageNum.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.lbPageNum.ForeColor = System.Drawing.Color.White;
            this.lbPageNum.Name = "lbPageNum";
            this.lbPageNum.Size = new System.Drawing.Size(58, 15);
            this.lbPageNum.Text = "1/100";
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel2.ForeColor = System.Drawing.Color.White;
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(58, 15);
            this.toolStripLabel2.Text = "Page";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(58, 6);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(58, 6);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.ForeColor = System.Drawing.Color.White;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(58, 15);
            this.toolStripLabel1.Text = "Margin";
            // 
            // bWidthPlus
            // 
            this.bWidthPlus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bWidthPlus.Image = ((System.Drawing.Image)(resources.GetObject("bWidthPlus.Image")));
            this.bWidthPlus.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bWidthPlus.Name = "bWidthPlus";
            this.bWidthPlus.Size = new System.Drawing.Size(58, 19);
            this.bWidthPlus.Text = "(+)";
            this.bWidthPlus.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.bWidthPlus.Click += new System.EventHandler(this.bWidthPlus_Click);
            // 
            // bWidthMinus
            // 
            this.bWidthMinus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bWidthMinus.Image = ((System.Drawing.Image)(resources.GetObject("bWidthMinus.Image")));
            this.bWidthMinus.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bWidthMinus.Name = "bWidthMinus";
            this.bWidthMinus.Size = new System.Drawing.Size(58, 19);
            this.bWidthMinus.Text = "(-)";
            this.bWidthMinus.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.bWidthMinus.Click += new System.EventHandler(this.bWidthMinus_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(58, 6);
            // 
            // bPaperWhite
            // 
            this.bPaperWhite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.bPaperWhite.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bPaperWhite.ForeColor = System.Drawing.Color.Black;
            this.bPaperWhite.Image = ((System.Drawing.Image)(resources.GetObject("bPaperWhite.Image")));
            this.bPaperWhite.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bPaperWhite.Name = "bPaperWhite";
            this.bPaperWhite.Size = new System.Drawing.Size(58, 19);
            this.bPaperWhite.Text = "White";
            this.bPaperWhite.Click += new System.EventHandler(this.bPaperWhite_Click);
            // 
            // bPaperBlack
            // 
            this.bPaperBlack.BackColor = System.Drawing.Color.Black;
            this.bPaperBlack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bPaperBlack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.bPaperBlack.Image = ((System.Drawing.Image)(resources.GetObject("bPaperBlack.Image")));
            this.bPaperBlack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bPaperBlack.Name = "bPaperBlack";
            this.bPaperBlack.Size = new System.Drawing.Size(58, 19);
            this.bPaperBlack.Text = "Black";
            this.bPaperBlack.Click += new System.EventHandler(this.bPaperBlack_Click);
            // 
            // bPaperSepia
            // 
            this.bPaperSepia.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(217)))), ((int)(((byte)(207)))), ((int)(((byte)(187)))));
            this.bPaperSepia.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bPaperSepia.Image = ((System.Drawing.Image)(resources.GetObject("bPaperSepia.Image")));
            this.bPaperSepia.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bPaperSepia.Name = "bPaperSepia";
            this.bPaperSepia.Size = new System.Drawing.Size(58, 19);
            this.bPaperSepia.Text = "Sepia";
            this.bPaperSepia.Click += new System.EventHandler(this.bPaperSepia_Click);
            // 
            // timerResize
            // 
            this.timerResize.Tick += new System.EventHandler(this.timerDelayedRender_Tick);
            // 
            // timerCacheDisplay
            // 
            this.timerCacheDisplay.Enabled = true;
            this.timerCacheDisplay.Interval = 1000;
            this.timerCacheDisplay.Tick += new System.EventHandler(this.timerCacheDisplay_Tick);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(58, 15);
            this.toolStripLabel3.Text = "Color";
            // 
            // pbBrigthness
            // 
            this.pbBrigthness.Location = new System.Drawing.Point(0, 197);
            this.pbBrigthness.Name = "pbBrigthness";
            this.pbBrigthness.PageIncrementSize = 0F;
            this.pbBrigthness.Size = new System.Drawing.Size(60, 14);
            this.pbBrigthness.TabIndex = 5;
            this.pbBrigthness.Text = "pbPaperBrightness";
            this.pbBrigthness.Value = 0.5F;
            this.pbBrigthness.ValueChanged += new System.EventHandler(this.pbBrigthness_ValueChanged);
            // 
            // bookProgressBar
            // 
            this.bookProgressBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bookProgressBar.Location = new System.Drawing.Point(60, 590);
            this.bookProgressBar.Name = "bookProgressBar";
            this.bookProgressBar.PageIncrementSize = 0F;
            this.bookProgressBar.Size = new System.Drawing.Size(835, 10);
            this.bookProgressBar.TabIndex = 4;
            this.bookProgressBar.Text = "bookProgressBar";
            this.bookProgressBar.Value = 0F;
            this.bookProgressBar.ValueChanged += new System.EventHandler(this.bookProgressBar_ValueChanged);
            // 
            // bStartPage
            // 
            this.bStartPage.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.bStartPage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bStartPage.Image = ((System.Drawing.Image)(resources.GetObject("bStartPage.Image")));
            this.bStartPage.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bStartPage.Name = "bStartPage";
            this.bStartPage.Size = new System.Drawing.Size(58, 19);
            this.bStartPage.Text = "|<< Start";
            this.bStartPage.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.bStartPage.Click += new System.EventHandler(this.bStartPage_Click);
            // 
            // ReadingPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.Controls.Add(this.pbBrigthness);
            this.Controls.Add(this.bookProgressBar);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.pMargins);
            this.Name = "ReadingPanel";
            this.Size = new System.Drawing.Size(895, 600);
            ((System.ComponentModel.ISupportInitialize)(this.pbContent)).EndInit();
            this.pMargins.ResumeLayout(false);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbContent;
        private System.Windows.Forms.Panel pMargins;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton bWidthMinus;
        private System.Windows.Forms.ToolStripButton bNextPage;
        private System.Windows.Forms.ToolStripButton bPrevPage;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Timer timerResize;
        private System.Windows.Forms.ToolStripButton bLibrary;
        private System.Windows.Forms.ToolStripLabel lbPageNum;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton bWidthPlus;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private BookProgressBar bookProgressBar;
        private System.Windows.Forms.Timer timerCacheDisplay;
        private System.Windows.Forms.ToolStripButton bPaperWhite;
        private System.Windows.Forms.ToolStripButton bPaperBlack;
        private System.Windows.Forms.ToolStripButton bPaperSepia;
        private BookProgressBar pbBrigthness;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripButton bStartPage;
    }
}
