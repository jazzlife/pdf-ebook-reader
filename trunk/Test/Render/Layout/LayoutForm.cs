using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BookReader.Utils;
using System.IO;

namespace BookReaderTest.Render.Layout
{
    public partial class LayoutForm : Form
    {
        public LayoutForm()
        {
            InitializeComponent();
        }

        TestCaseStatus _status = TestCaseStatus.Unknown;
        public TestCaseStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                lbStatus.Text = _status.ToString();
                lbStatus.BackColor = GetColor(_status);
            }
        }

        public String Comment
        {
            get { return tbComment.Text; }
            set { tbComment.Text = value; }
        }

        Color GetColor(TestCaseStatus status)
        {
            switch (status)
            {
                case TestCaseStatus.Unknown: return Color.Gray;
                case TestCaseStatus.Fail: return Color.FromArgb(255, 192, 192);
                case TestCaseStatus.Pass_Good: return Color.FromArgb(192, 255, 192);
                case TestCaseStatus.Pass_Acceptable: return Color.FromArgb(64, 255, 64);
                case TestCaseStatus.Ignore: return Color.FromArgb(255, 255, 192);
                default: return Color.DarkGray;
            }
        }

        public TestCaseStatus Show(TestCaseStatus initStatus, LayoutTestCase tcase, DW<Bitmap> bitmap)
        {
            lbFilename.Text = Path.GetFileName(tcase.Filename);
            lbPageNum.Text = tcase.PageNum.ToString();
            Comment = tcase.Comment;

            pbPage.Image = bitmap.o;
            Status = initStatus;

            ShowDialog();

            tcase.Comment = Comment;

            return Status;
        }

        private void bCorrect_Click(object sender, EventArgs e)
        {
            Status = TestCaseStatus.Pass_Good;
            Close();
        }

        private void bPassAcceptable_Click(object sender, EventArgs e)
        {
            Status = TestCaseStatus.Pass_Acceptable;
            Close();
        }

        private void bFail_Click(object sender, EventArgs e)
        {
            Status = TestCaseStatus.Fail;
            Close();
        }

        private void bIgnore_Click(object sender, EventArgs e)
        {
            Status = TestCaseStatus.Ignore;
            Close();
        }

        private void bClearData_Click(object sender, EventArgs e)
        {
            Status = TestCaseStatus.Ignore_Clear;
            Close();
        }

        private void bHaltTests_Click(object sender, EventArgs e)
        {
            Status = TestCaseStatus.HaltTest;
            Close();
        }



    }
}
