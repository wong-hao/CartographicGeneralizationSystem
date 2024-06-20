using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BuildingGen {
    public partial class CreateWorkspaceDlg : Form {
        private AddDataDlg dlg;
        private CreateWSDlg wsDlg;
        public CreateWorkspaceDlg() {
            InitializeComponent();
            dlg = new AddDataDlg();
            wsDlg = new CreateWSDlg();
        }

        private void btPath_Click(object sender, EventArgs e) {
            if (wsDlg.ShowDialog() == DialogResult.OK) {
                tbPath.Text = wsDlg.CurrentDirectory + "\\" + wsDlg.LayerName + ".gdb";
            }
        }
        private void ViewData(object sender, EventArgs e) {
            Button bt = sender as Button;
            TextBox tb = null;
            dlg.LayerName = bt.Tag as string;
            switch (bt.Tag as string) {
            case "铁路线":
                dlg.LayerType = GCityLayerType.铁路;
                tb = tbRailway;
                break;
            case "道路面":
                dlg.LayerType = GCityLayerType.道路;
                tb = tbRoadA;
                break;
            case "道路线":
                dlg.LayerType = GCityLayerType.道路;
                tb = tbRoadL;
                break;
            case "高架面":
                dlg.LayerType = GCityLayerType.高架;
                tb = tbRoadHA;
                break;
            case "高架线":
                dlg.LayerType = GCityLayerType.高架;
                tb = tbRoadHL;
                break;
            case "房屋面":
                dlg.LayerType = GCityLayerType.建筑物;
                tb = tbBuilding;
                break;
            case "工矿面":
                dlg.LayerType = GCityLayerType.工矿;
                tb = tbFactory;
                break;
            case "植被面":
                dlg.LayerType = GCityLayerType.植被;
                tb = tbPlant;
                break;
            case "禁测面":
                dlg.LayerType = GCityLayerType.禁测;
                tb = tbForbid;
                break;
            case "水系面":
                dlg.LayerType = GCityLayerType.水系;
                tb = tbWater;
                break;
            case "BRT交通面":
                dlg.LayerType = GCityLayerType.BRT交通面;
                tb = tbBRT;
                break;
            case "绿化岛":
                dlg.LayerType = GCityLayerType.绿化岛;
                tb = tbIsland;
                break;
            case "POI":
                dlg.LayerType = GCityLayerType.POI;
                tb = tbPOI;
                break;
            }
            if (dlg.ShowDialog() == DialogResult.OK) {
                tb.Text = dlg.LayerPath;
                string wn;
                string fn;
                if (!dlg.LayerPath.Contains("|")) {
                    wn = System.IO.Path.GetDirectoryName(dlg.LayerPath);
                    fn = System.IO.Path.GetFileNameWithoutExtension(dlg.LayerPath);
                }
                else {
                    string[] paths = dlg.LayerPath.Split(new char[] { '|' });
                    wn = paths[0];
                    fn = paths[paths.Length - 1];
                }
                tb.Tag = new CreateLayerInfo(wn, fn, dlg.LayerName, dlg.LayerType);
            }
        }


        public double OrgScale {
            get {
                try {
                    string text = cbOrgScale.Text;
                    string[] v = text.Split(new char[] { ':', '：' });
                    string v2 = v[1];
                    return Convert.ToDouble(v2);
                }
                catch {
                    return 2000;
                }
            }
        }
        public double GenScale {
            get {
                try {
                    string text = cbGenScale.Text;
                    string[] v = text.Split(new char[] { ':', '：' });
                    string v2 = v[1];
                    return Convert.ToDouble(v2);
                }
                catch {
                    return 2000;
                }
            }
        }
        public string WorkspacePath {
            get { return tbPath.Text; }
        }
        public CreateLayerInfo RoadA {
            get { return tbRoadA.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo RoadL {
            get { return tbRoadL.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo RoadHA {
            get { return tbRoadHA.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo RoadHL {
            get { return tbRoadHL.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo Railway {
            get { return tbRailway.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo Building {
            get { return tbBuilding.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo Factory {
            get { return tbFactory.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo Forbid {
            get { return tbForbid.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo Plant {
            get { return tbPlant.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo Water {
            get { return tbWater.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo BRT {
            get { return tbBRT.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo Island {
            get { return tbIsland.Tag as CreateLayerInfo; }
        }
        public CreateLayerInfo POI {
            get { return tbPOI.Tag as CreateLayerInfo; }
        }
        private void create(object sender, EventArgs e) {
            DialogResult = DialogResult.None;
            if (WorkspacePath == "") {
                MessageBox.Show("选择有效的路径");
                return;
            }
            DialogResult = DialogResult.OK;
        }
    }

    public class CreateLayerInfo {
        public string WorkspacePath;
        public string Name;
        public string FeatureName;
        public GCityLayerType Type;
        public CreateLayerInfo(string wspath, string featureName, string name, GCityLayerType type) {
            this.WorkspacePath = wspath;
            FeatureName = featureName;
            this.Name = name;
            this.Type = type;
        }
    }
}
