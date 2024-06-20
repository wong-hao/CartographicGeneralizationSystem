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
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using SMGI.Common;
namespace SMGI.Plugin.MapGeneralization
{
    public partial class POISelectionForm : Form
    {
        private GApplication _app;
        private Dictionary<string, ILayer> _layerDic = new Dictionary<string, ILayer>();

        private Dictionary<string, List<string>> _layerRules = new Dictionary<string, List<string>>();
        public List<ILayer> _selectedLayers = new List<ILayer>();
        public Dictionary<string, List<string>> _selectedRuleNames = new Dictionary<string, List<string>>();
        public double _selectRatio;
        public ILayer _selectLayer;
        private bool _check;

        public bool Check
        {
            get { return _check; }
            set { _check = value; }
        }
        
        public POISelectionForm(GApplication app)
        {
            InitializeComponent();
            _app = app;
            _selectRatio = 0.3;
        }

        private void GetLayers(ILayer layer, esriGeometryType geoType)
        {
            if (layer is IGroupLayer)
            {
                ICompositeLayer pGroupLayer = (ICompositeLayer)layer;
                for (int i = 0; i < pGroupLayer.Count; i++)
                {
                    ILayer SubLayer = pGroupLayer.get_Layer(i);
                    GetLayers(SubLayer, geoType);
                }
            }
            else
            {
                if (layer is IFeatureLayer)
                {
                    IFeatureLayer lyr = (IFeatureLayer)layer;
                    IFeatureClass fc = lyr.FeatureClass;
                    if (fc.ShapeType == geoType)
                    {
                        _layerRules.Add(layer.Name, GetRuleNames(fc));
                        _layerDic.Add(layer.Name, layer);
                    }
                }
            }
        }

        
        private List<string> GetRuleNames(IFeatureClass fc)
        {
           List<string> ruleNames = new List<string>();
          
           IRepresentationClass rpc = OpenRepClass(fc);
           if (rpc == null)
           {
                ruleNames.Add("全部要素");
                return ruleNames;
           }
           var rules = rpc.RepresentationRules;
           rules.Reset();
           int id;
           IRepresentationRule rule; 
           rules.Next(out id, out rule);
           while (rule != null)
           {
               if (!rules.get_Name(id).Contains("不显示要素"))
               {
                   ruleNames.Add(rules.get_Name(id));
               }                
               rules.Next(out id, out rule);
            }
           return ruleNames;
        }

        public  IRepresentationClass OpenRepClass(IFeatureClass fc)
        {
            if (fc == null) 
                return null;
            try
            {
                IDataset pDs = fc as IDataset;
                IWorkspace pWorkspace = pDs.Workspace;
                IRepresentationWorkspaceExtension pRepWSExt;
                IWorkspaceExtensionManager pExtManager = pWorkspace as IWorkspaceExtensionManager;
                UID pUID = new UID();
                pUID.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
                IWorkspaceExtension pWksExt = pExtManager.FindExtension(pUID);
                pRepWSExt = pWksExt as IRepresentationWorkspaceExtension;
                string featClsName = pDs.Name;
                string strRepClassName = featClsName;
                
                bool bHasRepClass = pRepWSExt.get_FeatureClassHasRepresentations(fc);
                IRepresentationClass pRepClass = null;
                if (bHasRepClass)
                {
                     pRepClass = pRepWSExt.OpenRepresentationClass(strRepClassName);
                }
                return pRepClass;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void POISelectionForm_Load(object sender, EventArgs e)
        {
            esriGeometryType geoType = esriGeometryType.esriGeometryPoint;
            var acv = _app.ActiveView;
            var map = acv.FocusMap;

            for (int i = 0; i < map.LayerCount; i++)
            {
                ILayer pLayer = map.get_Layer(i);
                GetLayers(pLayer, geoType);
            }

            foreach (var layerName in _layerDic.Keys)
            {
                this.clbLayerList.Items.Add(layerName);
            }
            if (this.clbLayerList.Items.Count>1)
            {
                this.clbLayerList.SelectedIndex = 1;
            }
            
            tbSelectRatio.Text = _selectRatio.ToString();
        }

        private void btnOK_Click(object sender, EventArgs e)
        { 
            _selectRatio = Convert.ToDouble(tbSelectRatio.Text);
            List<string> ruleNames=new List<string>();
            for(int i=0;i<checkedRuleNames.CheckedItems.Count;i++)
            {
                var item = checkedRuleNames.CheckedItems[i];
                ruleNames.Add(item.ToString());
            }

            if (ruleNames.Count == 0)
            {
                MessageBox.Show("请选择需要抽稀的要素类别！");
                return;
            }
            _selectedRuleNames.Add(clbLayerList.SelectedItem.ToString(), ruleNames);
            _selectLayer = _layerDic[clbLayerList.SelectedItem.ToString()];
            this.Close();
             
        }
        //图层变化
        private void clbLayerList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string layerName=clbLayerList.SelectedItem.ToString();
            checkedRuleNames.Items.Clear();
            checkedRuleNames.Items.AddRange(_layerRules[layerName].ToArray());
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedRuleNames.Items.Count; i++)
            {
                checkedRuleNames.SetItemChecked(i, true);
            }
        }

        private void btnClearSelect_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedRuleNames.Items.Count; i++)
            {
                if (checkedRuleNames.GetItemChecked(i))
                {
                    checkedRuleNames.SetItemChecked(i, false);
                }
                else
                {
                    checkedRuleNames.SetItemChecked(i, true);
                }
            }
        }

        private void checkedRuleNames_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            int index=e.Index;
            if (!checkedRuleNames.GetItemChecked(index))
            {
                for (int i = 0; i < checkedRuleNames.Items.Count; i++)
                {
                    if (i != index)
                    {
                        checkedRuleNames.SetItemChecked(i, false);
                    }
                }
            }
        }
    }
}
