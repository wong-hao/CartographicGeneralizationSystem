using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using System.IO;
using ESRI.ArcGIS.DataSourcesRaster;
using SMGI.Common;
namespace SMGI.Plugin.MapGeneralization
{
    public partial class FrmTopolgy : Form
    {
        IActiveView pac=null;
        IMap pmap = null;
        //图层名称->数据集名称
        private Dictionary<string, IFeatureClass> fclsDic = new Dictionary<string, IFeatureClass>();
        private Dictionary<string, string> layersName = new Dictionary<string, string>();
        private Dictionary<string, IFeatureDataset> fDsDic = new Dictionary<string, IFeatureDataset>();
        public FrmTopolgy(IMap map)
        {
            InitializeComponent();
            pmap = map;
            pac = map as IActiveView;
        }
        
        private void btOK_Click(object sender, EventArgs e)
        {
            TopologyHelper helper = new TopologyHelper(pac);
            if (cmbTop.Items.Count == 0)
            {
                MessageBox.Show("拓扑必须创建于数据集中,请先创建数据集！");
                return;
            }
            if (cmbTop.SelectedItem == null)
            {
                MessageBox.Show("请先选择数据集！");
                return;
            }
            
            WaitOperation wo=  GApplication.Application.SetBusy();
            bool flag = true;
            try
            {
                wo.SetText("正在创建拓扑");
                IFeatureDataset pfds = fDsDic[cmbTop.SelectedItem.ToString()];
                string topolgyName = txtTopName.Text;
                ITopology topgy = helper.CreateTopology(topolgyName, pfds);
                helper.RemoveAllClass(topgy);
                for (int i = 0; i < listLayer.Items.Count; i++)
                {
                    string checkboxname = listLayer.Items[i].ToString();
                    if (listLayer.GetItemChecked(i))
                    {
                        IFeatureClass fc = fclsDic[checkboxname];
                        helper.AddClass(topgy, fc);
                    }

                }
                //创建成功
                TopologyApplication.TopName = topolgyName;
                TopologyApplication.Topology = topgy;
                
            }
            catch(Exception ex)
            {
                MessageBox.Show("拓扑构建错误！"+ex.Message);
                flag = false;
            }
            finally
            {
                wo.Dispose();
            }
            if(flag)
            MessageBox.Show("创建成功");
            DialogResult = DialogResult.OK;
        }
        private void LoadTopLayer(string fdName)
        {
            listLayer.Items.Clear();
            
            IFeatureDataset featureDs = fDsDic[fdName];
            ITopologyContainer2 topologyContainer = (ITopologyContainer2)featureDs;
            string topolgyName=fdName+"_Topology";
             
            ITopology pTopology = GetTop(topologyContainer,topolgyName);
            List<string> list = new List<string>();//拓扑中的要素
            if (pTopology != null)//拓扑存在
            {
                IFeatureClassContainer fcContainer = pTopology as IFeatureClassContainer;
                IEnumFeatureClass enumfc = fcContainer.Classes;
                enumfc.Reset();
                IFeatureClass fc = null;
                while ((fc = enumfc.Next()) != null)
                {
                    list.Add(fc.AliasName);
                }
                Marshal.ReleaseComObject(enumfc);

            }
            int flag = 0;
            foreach (var kv in layersName)
            {
                if (kv.Value == fdName)
                {
                    listLayer.Items.Add(kv.Key);
                    listLayer.SetItemChecked(flag, list.Contains(kv.Key));
                    flag++;
                }
            }
          

        }
        private ITopology GetTop(ITopologyContainer2 topologyContainer, string topolgyName)
        {
            ITopology top = null;
            try
            {
                
                top = topologyContainer.get_TopologyByName(topolgyName);
                return top;
            }
            catch (Exception ex)
            {
                return top;
            }
        }
        private void FrmTopolgy_Load(object sender, EventArgs e)
        {
            List<string> fdnames = new List<string>();
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l is IFeatureLayer;
            })).ToArray();
            for (int i = 0; i < lyrs.Length; i++)
            {
                ILayer player = lyrs[i];
                if (player is IFeatureLayer && (player as IFeatureLayer).FeatureClass!=null)
                {
                    if ((player as IFeatureLayer).FeatureClass != null)
                    {
                        IFeatureDataset pfds = (player as IFeatureLayer).FeatureClass.FeatureDataset;
                        if (pfds != null)
                        {

                            fDsDic[pfds.Name] = pfds;
                            string name = (player as IFeatureLayer).FeatureClass.AliasName;
                            fclsDic[name] = (player as IFeatureLayer).FeatureClass;
                            layersName[name] = pfds.Name;
                            if (!fdnames.Contains(pfds.Name))
                            {
                                fdnames.Add(pfds.Name);
                            }

                        }
                    }
                }
            }
            //添加到top
            if (fdnames.Count > 0)
            {
                cmbTop.Items.AddRange(fdnames.ToArray());
                cmbTop.SelectedIndex = 0;
            }

        }

        private void cmbTop_SelectedIndexChanged(object sender, EventArgs e)
        {
            string name=cmbTop.SelectedItem.ToString();
            txtTopName.Text = name + "_Topology";
            LoadTopLayer(name);
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
