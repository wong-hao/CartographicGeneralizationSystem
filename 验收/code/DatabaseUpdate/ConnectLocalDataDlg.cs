using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DatabaseUpdate
{
    public partial class ConnectLocalDataDlg : Form
    {
        public ConnectLocalDataDlg()
        {
            InitializeComponent();
            clFeature.Items.Clear();
            clFeature.Items.AddRange(Enum.GetNames(typeof(ULayerInfos.LayerType)));
        }

        private void btPath_Click(object sender, EventArgs e)
        {
          
          OpenWSDlg2 dlg = new OpenWSDlg2();

          if (dlg.ShowDialog() != DialogResult.OK)
          {
            return;
          }

          string path = dlg.LayerPath;

          tbPath.Text = path;
          //app.OpenWorkspace(dlg.LayerPath);
          //app.AppConfig["最后工作区"] = dlg.CurrentDirectory;
        }

        public string GDBPath {
          get {
            return tbPath.Text;
          }
        }

        public int[] Scales {
          get {
            List<int> s = new List<int>();
            for (int i = 0; i < clScale.CheckedItems.Count; i++)
            {
              string item = clScale.CheckedItems[i].ToString();
              s.Add(Convert.ToInt32(item));
            }
            return s.ToArray();
          }
        }
        public ULayerInfos.LayerType[] FeatureClassName {
          get {
            List<ULayerInfos.LayerType> s = new List<ULayerInfos.LayerType>();
            for (int i = 0; i < clFeature.CheckedItems.Count; i++)
            {
              string ss = clFeature.CheckedItems[i].ToString();
              s.Add((ULayerInfos.LayerType)Enum.Parse(typeof(ULayerInfos.LayerType), ss));
            }
            return s.ToArray();
          }
        }

    }
}
