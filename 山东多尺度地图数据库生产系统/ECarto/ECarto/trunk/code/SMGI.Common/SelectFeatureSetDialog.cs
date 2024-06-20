using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SMGI.Common
{
    public partial class SelectFeatureSetDialog : Form
    {
        SelectFeatureSetControl sfsc;
        public SelectFeatureSetDialog(GApplication app)
        {
            InitializeComponent();
            sfsc = new SelectFeatureSetControl(app);
            this.Controls.Add(sfsc);
            sfsc.Dock = DockStyle.Fill;
        }

        public bool AreaLayer { get { return sfsc.AreaLayer; } set { sfsc.AreaLayer = value; } }

        public bool LineLayer { get { return sfsc.LineLayer; } set { sfsc.LineLayer = value; } }

        public bool PointLayer { get { return sfsc.PointLayer; } set { sfsc.PointLayer = value; } }

        public List<SelectFeatureSet> FeatureSets { get { return sfsc.FeatureSets; } }
    }
}
