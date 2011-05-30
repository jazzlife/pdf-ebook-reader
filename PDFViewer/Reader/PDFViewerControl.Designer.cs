using System;
namespace PDFViewer.Reader
{
    partial class PDFViewerControl
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

            // Dispose underlying objects
            // TODO: consider moving futher inside
            if (_pdfDoc != null)
            {
                _pdfDoc.Dispose();
                _pdfDoc = null;
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PDFViewerControl));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbPrev = new System.Windows.Forms.ToolStripButton();
            this.txtPage = new System.Windows.Forms.ToolStripTextBox();
            this.tsbNext = new System.Windows.Forms.ToolStripButton();
            this.tsbSearch = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.tsbAntialias = new System.Windows.Forms.ToolStripButton();
            this.tsbVectorAntialias = new System.Windows.Forms.ToolStripButton();
            this.tsbUseMuPDF = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.ttpLink = new System.Windows.Forms.ToolTip(this.components);
            this.pageView = new PDFViewer.PageViewer();
            this.bgLoadPages = new System.ComponentModel.BackgroundWorker();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbPrev,
            this.txtPage,
            this.tsbNext,
            this.tsbSearch,
            this.toolStripButton1,
            this.toolStripButton2,
            this.tsbAntialias,
            this.tsbVectorAntialias,
            this.tsbUseMuPDF,
            this.toolStripSeparator1,
            this.StatusLabel});
            this.toolStrip1.Name = "toolStrip1";
            this.ttpLink.SetToolTip(this.toolStrip1, resources.GetString("toolStrip1.ToolTip"));
            // 
            // tsbPrev
            // 
            resources.ApplyResources(this.tsbPrev, "tsbPrev");
            this.tsbPrev.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbPrev.Image = global::PDFViewer.Properties.Resources.netshell_21611_1_16x16x32;
            this.tsbPrev.Name = "tsbPrev";
            this.tsbPrev.Click += new System.EventHandler(this.OnPreviousPage);
            // 
            // txtPage
            // 
            resources.ApplyResources(this.txtPage, "txtPage");
            this.txtPage.Name = "txtPage";
            this.txtPage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPage_KeyDown);
            // 
            // tsbNext
            // 
            resources.ApplyResources(this.tsbNext, "tsbNext");
            this.tsbNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbNext.Image = global::PDFViewer.Properties.Resources.netshell_1611_1_16x16x32;
            this.tsbNext.Name = "tsbNext";
            this.tsbNext.Click += new System.EventHandler(this.OnNextPage);
            // 
            // tsbSearch
            // 
            resources.ApplyResources(this.tsbSearch, "tsbSearch");
            this.tsbSearch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbSearch.Image = global::PDFViewer.Properties.Resources.SearchFolder_323_3_16x16x32;
            this.tsbSearch.Name = "tsbSearch";
            this.tsbSearch.Click += new System.EventHandler(this.tsbSearch_Click);
            // 
            // toolStripButton1
            // 
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::PDFViewer.Properties.Resources.ZoomIn;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.tsbZoomIn_Click);
            // 
            // toolStripButton2
            // 
            resources.ApplyResources(this.toolStripButton2, "toolStripButton2");
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = global::PDFViewer.Properties.Resources.ZoomOut;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Click += new System.EventHandler(this.tsbZoomOut_Click);
            // 
            // tsbAntialias
            // 
            resources.ApplyResources(this.tsbAntialias, "tsbAntialias");
            this.tsbAntialias.Checked = true;
            this.tsbAntialias.CheckOnClick = true;
            this.tsbAntialias.CheckState = System.Windows.Forms.CheckState.Checked;
            this.tsbAntialias.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbAntialias.Name = "tsbAntialias";
            this.tsbAntialias.Click += new System.EventHandler(this.tsbAntialias_Click);
            // 
            // tsbVectorAntialias
            // 
            resources.ApplyResources(this.tsbVectorAntialias, "tsbVectorAntialias");
            this.tsbVectorAntialias.CheckOnClick = true;
            this.tsbVectorAntialias.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbVectorAntialias.Name = "tsbVectorAntialias";
            this.tsbVectorAntialias.Click += new System.EventHandler(this.tsbVectorAntialias_Click);
            // 
            // tsbUseMuPDF
            // 
            resources.ApplyResources(this.tsbUseMuPDF, "tsbUseMuPDF");
            this.tsbUseMuPDF.CheckOnClick = true;
            this.tsbUseMuPDF.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsbUseMuPDF.Name = "tsbUseMuPDF";
            this.tsbUseMuPDF.Click += new System.EventHandler(this.tsbUseMuPDF_Click);
            // 
            // toolStripSeparator1
            // 
            resources.ApplyResources(this.toolStripSeparator1, "toolStripSeparator1");
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            // 
            // StatusLabel
            // 
            resources.ApplyResources(this.StatusLabel, "StatusLabel");
            this.StatusLabel.Name = "StatusLabel";
            // 
            // pageView
            // 
            resources.ApplyResources(this.pageView, "pageView");
            this.pageView.BackColor = System.Drawing.Color.LightGray;
            this.pageView.BorderColor = System.Drawing.Color.Black;
            this.pageView.DrawBorder = false;
            this.pageView.DrawShadow = true;
            this.pageView.Name = "pageView";
            this.pageView.PageColor = System.Drawing.Color.White;
            this.pageView.PageSize = new System.Drawing.Size(0, 0);
            this.pageView.PaintMethod = PDFViewer.PageViewer.DoubleBufferMethod.BuiltInOptimizedDoubleBuffer;
            this.pageView.ScrollPosition = new System.Drawing.Point(-10, -10);
            this.ttpLink.SetToolTip(this.pageView, resources.GetString("pageView.ToolTip"));
            this.pageView.NextPage += new System.EventHandler(this.OnNextPage);
            this.pageView.PreviousPage += new System.EventHandler(this.OnPreviousPage);
            this.pageView.PaintControl += new PDFViewer.PageViewer.PaintControlHandler(this.doubleBufferControl1_PaintControl);
            // 
            // bgLoadPages
            // 
            this.bgLoadPages.WorkerReportsProgress = true;
            this.bgLoadPages.WorkerSupportsCancellation = true;
            // 
            // PDFViewerControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pageView);
            this.Controls.Add(this.toolStrip1);
            this.DoubleBuffered = true;
            this.Name = "PDFViewerControl";
            this.ttpLink.SetToolTip(this, resources.GetString("$this.ToolTip"));
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbPrev;
        private System.Windows.Forms.ToolStripTextBox txtPage;
        private System.Windows.Forms.ToolStripButton tsbNext;
        private System.Windows.Forms.ToolStripButton tsbSearch;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolTip ttpLink;
        private System.Windows.Forms.ToolStripButton tsbAntialias;
        private System.Windows.Forms.ToolStripButton tsbVectorAntialias;
        private PageViewer pageView;
        private System.ComponentModel.BackgroundWorker bgLoadPages;
        private System.Windows.Forms.ToolStripButton tsbUseMuPDF;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        public System.Windows.Forms.ToolStripStatusLabel StatusLabel;
    }
}

