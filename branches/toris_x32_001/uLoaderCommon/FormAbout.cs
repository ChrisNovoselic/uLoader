using ASUTP.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

////using HClassLibrary;

namespace uLoaderCommon
{
    public partial class FormAbout : Form
    {
        public FormAbout()
        {
            InitializeComponent();
        }

        private void m_btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormAbout_Load(object sender, EventArgs e)
        {
            this.m_pictureBoxIconMain.Image = uLoaderCommon.Properties.Resources.IconMainULoader.ToBitmap ();
            this.m_labelProductVersion.Text = ProgramBase.AppProductVersion;
        }
    }
}
