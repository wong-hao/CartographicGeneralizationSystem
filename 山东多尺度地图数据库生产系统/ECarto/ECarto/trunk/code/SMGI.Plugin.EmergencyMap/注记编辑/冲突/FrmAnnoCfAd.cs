using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmAnnoCfAd : Form
    {
        public FrmAnnoCfAd()
        {
            InitializeComponent();
        }
        public AnnoConflictType ConflictType = AnnoConflictType.Envelop;
        private void btOk_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void rgEn_CheckedChanged(object sender, EventArgs e)
        {
            if (rgEn.Checked)
            {
                ConflictType = AnnoConflictType.Envelop;
            }
        
        }

        private void rbGeo_CheckedChanged(object sender, EventArgs e)
        {
            if (rbGeo.Checked)
            {
                ConflictType = AnnoConflictType.Geometry;
            }
        
        }
    }
    public enum AnnoConflictType
    {
        Envelop=0,
        Geometry=1
    }

}
