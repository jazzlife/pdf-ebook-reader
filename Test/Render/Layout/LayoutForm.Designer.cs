namespace BookReaderTest.Render.Layout
{
    partial class LayoutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayoutForm));
            this.bPass = new System.Windows.Forms.ToolStripButton();
            this.bIgnore = new System.Windows.Forms.ToolStripButton();
            this.bClearData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.bPassAcceptable = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.tbComment = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.bFail = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.bHaltTests = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.lbStatus = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.lbFilename = new System.Windows.Forms.ToolStripLabel();
            this.lbPageNum = new System.Windows.Forms.ToolStripLabel();
            this.pbPage = new System.Windows.Forms.PictureBox();
            this.toolStrip1.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPage)).BeginInit();
            this.SuspendLayout();
            // 
            // bPass
            // 
            this.bPass.BackColor = System.Drawing.Color.Lime;
            this.bPass.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bPass.Image = ((System.Drawing.Image)(resources.GetObject("bPass.Image")));
            this.bPass.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bPass.Name = "bPass";
            this.bPass.Size = new System.Drawing.Size(110, 28);
            this.bPass.Text = "  Good  ";
            this.bPass.Click += new System.EventHandler(this.bCorrect_Click);
            // 
            // bIgnore
            // 
            this.bIgnore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.bIgnore.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bIgnore.Image = ((System.Drawing.Image)(resources.GetObject("bIgnore.Image")));
            this.bIgnore.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bIgnore.Name = "bIgnore";
            this.bIgnore.Size = new System.Drawing.Size(110, 28);
            this.bIgnore.Text = " Ignore ";
            this.bIgnore.Click += new System.EventHandler(this.bIgnore_Click);
            // 
            // bClearData
            // 
            this.bClearData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bClearData.Image = ((System.Drawing.Image)(resources.GetObject("bClearData.Image")));
            this.bClearData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bClearData.Name = "bClearData";
            this.bClearData.Size = new System.Drawing.Size(122, 28);
            this.bClearData.Text = "  Clear  ";
            this.bClearData.Click += new System.EventHandler(this.bClearData_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip1.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bPass,
            this.toolStripSeparator1,
            this.bPassAcceptable,
            this.toolStripSeparator7,
            this.tbComment,
            this.toolStripSeparator6,
            this.bFail,
            this.toolStripSeparator3,
            this.bIgnore,
            this.bHaltTests,
            this.toolStripSeparator5,
            this.bClearData,
            this.toolStripSeparator4});
            this.toolStrip1.Location = new System.Drawing.Point(0, 790);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(930, 31);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // bPassAcceptable
            // 
            this.bPassAcceptable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.bPassAcceptable.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bPassAcceptable.Image = ((System.Drawing.Image)(resources.GetObject("bPassAcceptable.Image")));
            this.bPassAcceptable.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bPassAcceptable.Name = "bPassAcceptable";
            this.bPassAcceptable.Size = new System.Drawing.Size(110, 28);
            this.bPassAcceptable.Text = "   OK   ";
            this.bPassAcceptable.Click += new System.EventHandler(this.bPassAcceptable_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 31);
            // 
            // tbComment
            // 
            this.tbComment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbComment.Name = "tbComment";
            this.tbComment.Size = new System.Drawing.Size(170, 31);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 31);
            // 
            // bFail
            // 
            this.bFail.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.bFail.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bFail.Image = ((System.Drawing.Image)(resources.GetObject("bFail.Image")));
            this.bFail.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bFail.Name = "bFail";
            this.bFail.Size = new System.Drawing.Size(110, 28);
            this.bFail.Text = "  Fail  ";
            this.bFail.Click += new System.EventHandler(this.bFail_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 31);
            // 
            // bHaltTests
            // 
            this.bHaltTests.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.bHaltTests.BackColor = System.Drawing.Color.Red;
            this.bHaltTests.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.bHaltTests.ForeColor = System.Drawing.Color.White;
            this.bHaltTests.Image = ((System.Drawing.Image)(resources.GetObject("bHaltTests.Image")));
            this.bHaltTests.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bHaltTests.Name = "bHaltTests";
            this.bHaltTests.Size = new System.Drawing.Size(62, 28);
            this.bHaltTests.Text = "Halt";
            this.bHaltTests.Click += new System.EventHandler(this.bHaltTests_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 31);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 31);
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lbStatus,
            this.toolStripSeparator2,
            this.lbFilename,
            this.lbPageNum});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(930, 25);
            this.toolStrip2.TabIndex = 2;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // lbStatus
            // 
            this.lbStatus.Name = "lbStatus";
            this.lbStatus.Size = new System.Drawing.Size(38, 22);
            this.lbStatus.Text = "status";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // lbFilename
            // 
            this.lbFilename.Name = "lbFilename";
            this.lbFilename.Size = new System.Drawing.Size(53, 22);
            this.lbFilename.Text = "filename";
            // 
            // lbPageNum
            // 
            this.lbPageNum.Name = "lbPageNum";
            this.lbPageNum.Size = new System.Drawing.Size(13, 22);
            this.lbPageNum.Text = "1";
            // 
            // pbPage
            // 
            this.pbPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(212)))), ((int)(((byte)(212)))), ((int)(((byte)(255)))));
            this.pbPage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbPage.Location = new System.Drawing.Point(0, 25);
            this.pbPage.Name = "pbPage";
            this.pbPage.Size = new System.Drawing.Size(930, 765);
            this.pbPage.TabIndex = 3;
            this.pbPage.TabStop = false;
            // 
            // LayoutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(930, 821);
            this.Controls.Add(this.pbPage);
            this.Controls.Add(this.toolStrip2);
            this.Controls.Add(this.toolStrip1);
            this.Name = "LayoutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LayoutForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbPage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripButton bPass;
        private System.Windows.Forms.ToolStripButton bIgnore;
        private System.Windows.Forms.ToolStripButton bClearData;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripLabel lbStatus;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel lbFilename;
        private System.Windows.Forms.ToolStripLabel lbPageNum;
        private System.Windows.Forms.ToolStripButton bFail;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripTextBox tbComment;
        private System.Windows.Forms.ToolStripButton bHaltTests;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.PictureBox pbPage;
        private System.Windows.Forms.ToolStripButton bPassAcceptable;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
    }
}