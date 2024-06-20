using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.SystemUI;
using SMGI.Common;
using System.Runtime.InteropServices;
namespace SMGI.Plugin.GeneralEdit
{
    public partial class FrmSelectionDo : Form
    {
        GApplication app;
        public FrmSelectionDo()
        {
            InitializeComponent();
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }
        private int getIndexImg(IFeature fe)
        {
            int img = 1;
            if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
            {
                img = 1;
                
            }
            if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                img = 2;
            }
            if (fe.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
            {
                img = 3;
            }
            if (fe.FeatureType == esriFeatureType.esriFTAnnotation)
            {
                img = 4;
            }
            return img;
        }
        public void LoadFeatures()
        {
            treeView1.Nodes.Clear();
            
 
            IFeature fe;
            Dictionary<string, List<int>> fesDic = new Dictionary<string, List<int>>();
            Dictionary<string, int> imgDic = new Dictionary<string, int>();
            #region
            int ct = 0;
           
            ISelection selection = app.MapControl.Map.FeatureSelection;
            IEnumFeature mapEnumFeature = selection as IEnumFeature;
            IEnumFeatureSetup pEnumsetup = mapEnumFeature as IEnumFeatureSetup;
            pEnumsetup.AllFields = true;
            mapEnumFeature.Reset();
            while ((fe = mapEnumFeature.Next()) != null)
            {
                ct++;
                List<int> lists = null;
                string fclName = fe.Class.AliasName;
                if (fesDic.ContainsKey(fclName))
                {
                    lists = fesDic[fclName];
                    lists.Add(fe.OID);
                    fesDic[fclName] = lists;
                }
                else
                {
                    lists = new List<int>();
                    lists.Add(fe.OID);
                    fesDic[fclName] = lists;
                }
                imgDic[fclName] = getIndexImg(fe);
            }
            label1.Text = "一共：" + ct + " 要素";
            #endregion
            foreach (var kv in fesDic)
            {
                TreeNode layer = new TreeNode(kv.Key);
                layer.ImageIndex = imgDic[kv.Key];
                layer.SelectedImageIndex = imgDic[kv.Key];
                foreach (var id in kv.Value)
                {
                    TreeNode feid = new TreeNode(id.ToString());
                    feid.ImageIndex = 0;
                    feid.SelectedImageIndex =0;
                    layer.Nodes.Add(feid);
                }

                treeView1.Nodes.Add(layer);
            }
        }
        private void FrmSelectionDo_Load(object sender, EventArgs e)
        {
            app = GApplication.Application;
           
            IFeature fe;
            Dictionary<string, List<int>> fesDic = new Dictionary<string, List<int>>();
            Dictionary<string, int> imgDic = new Dictionary<string, int>();
            #region
            int ct = 0;
            ISelection selection = app.MapControl.Map.FeatureSelection;
            IEnumFeature mapEnumFeature = selection as IEnumFeature;
            IEnumFeatureSetup pEnumsetup = mapEnumFeature as IEnumFeatureSetup;
            pEnumsetup.AllFields = true;
            mapEnumFeature.Reset();
            while ((fe = mapEnumFeature.Next()) != null)
            {
                ct++;
                List<int> lists = null;
                string fclName = fe.Class.AliasName;
                if (fesDic.ContainsKey(fclName))
                {
                    lists = fesDic[fclName];
                    lists.Add(fe.OID);
                    fesDic[fclName] = lists;
                }
                else
                {
                    lists = new List<int>();
                    lists.Add(fe.OID);
                    fesDic[fclName] = lists;
                }
                imgDic[fclName] = getIndexImg(fe);
            }
        
           label1.Text = "一共：" + ct + " 要素";
            #endregion
            foreach(var kv in fesDic)
            {
                TreeNode layer = new TreeNode(kv.Key);
                layer.ImageIndex = imgDic[kv.Key];
                layer.SelectedImageIndex = imgDic[kv.Key];
                foreach (var id in kv.Value)
                {
                    TreeNode feid = new TreeNode(id.ToString());
                    feid.ImageIndex = 0;
                    feid.SelectedImageIndex = 0;
                    layer.Nodes.Add(feid);
                }

                treeView1.Nodes.Add(layer);
            }
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Drawing.Point clickp = new System.Drawing.Point(e.X, e.Y);
                TreeNode node = treeView1.GetNodeAt(clickp);
                if (node != null)
                {
                    if (node.Level == 0)
                    {
                        node.ContextMenuStrip = contextMenuStrip1;
                        contextMenuStrip1.Show();
                    }
                    if (node.Level == 1)
                    {
                        node.ContextMenuStrip = contextMenuStrip2;
                        contextMenuStrip2.Show();
                    }
                    treeView1.SelectedNode = node;
                }
            }
        }

        private void clearLayerItem_Click(object sender, EventArgs e)
        {
            TreeNode node= treeView1.SelectedNode;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l.Name == node.Text);
            })).ToArray();
            ILayer layerLF = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == (node.Text))).FirstOrDefault();
          //  IFeatureLayer fclLayer = lyrs.First() as IFeatureLayer;
            IFeatureLayer fclLayer = layerLF as IFeatureLayer;
            IFeatureSelection fclSel = fclLayer as IFeatureSelection;
            fclSel.Clear();
            treeView1.Nodes.Remove(node);

            #region 要素数量确定
            int count = app.MapControl.Map.SelectionCount;
            if (count == 0)
            {
                this.Close();
            }
            else
            {
                label1.Text = "一共：" + app.MapControl.Map.SelectionCount + " 要素";
                app.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, app.ActiveView.Extent);
            }
            #endregion
            app.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, app.ActiveView.Extent);
           
        }
        //清除当前要素
        private void ClearFeatureItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            var pnode = node.Parent;
                     ILayer layerLF = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == (pnode.Text))).FirstOrDefault();
         
            IFeatureLayer fclLayer = layerLF as IFeatureLayer;
            IFeatureSelection fclSel = fclLayer as IFeatureSelection;
            fclSel.Clear();
          
            for(int i=0;i<pnode.Nodes.Count;i++)
            {
                if (pnode.Nodes[i].Text != node.Text)
                {
                    IFeature fe = fclLayer.FeatureClass.GetFeature(int.Parse(pnode.Nodes[i].Text));
                    GApplication.Application.ActiveView.FocusMap.SelectFeature(fclLayer, fe);
                }
            }
           
            pnode.Nodes.Remove(node);

            if (pnode.GetNodeCount(true) == 0)  //如果子节点数量为零，则清除父节点 dxm
            {
                treeView1.Nodes.Remove(pnode);
            }
      
            #region 要素数量确定
            int count = app.MapControl.Map.SelectionCount;
            if (count == 0)
            {
                this.Close();
            }
            else
            {
                label1.Text = "一共：" + app.MapControl.Map.SelectionCount + " 要素";
                app.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, app.ActiveView.Extent);
            }
            #endregion
           
    
        }

        private void SeCurrentItem_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            var pnode = node.Parent;
            ILayer layerLF = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == (pnode.Text))).FirstOrDefault();
            //  IFeatureLayer fclLayer = lyrs.First() as IFeatureLayer;

            for (int i = 0; i < treeView1.Nodes.Count; i++)
            {
                if (treeView1.Nodes[i].Text != pnode.Text)
                {
                    ILayer layerTemp = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == (treeView1.Nodes[i].Text))).FirstOrDefault();
                    IFeatureSelection fclSelTemp = layerTemp as IFeatureSelection;
                    fclSelTemp.Clear();
                    treeView1.Nodes.Remove(treeView1.Nodes[i]);
                }
            }
            IFeatureLayer fclLayer = layerLF as IFeatureLayer;
            IFeatureSelection fclSel = fclLayer as IFeatureSelection;
            fclSel.Clear();
            IFeature fe = fclLayer.FeatureClass.GetFeature(int.Parse(node.Text));
            GApplication.Application.ActiveView.FocusMap.SelectFeature(fclLayer, fe);

            pnode.Nodes.Clear();
            pnode.Nodes.Add(node);

            #region 要素数量确定
            label1.Text = "一共：" + app.MapControl.Map.SelectionCount + " 要素";
            #endregion
            app.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, app.ActiveView.Extent);

        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 1)
            {
                treeView1.SelectedNode = e.Node;
                TreeNode node = treeView1.SelectedNode;
                var pnode = node.Parent;
                       ILayer layerLF = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName == (pnode.Text))).FirstOrDefault();
                //  IFeatureLayer fclLayer = lyrs.First() as IFeatureLayer;
                IFeatureLayer fclLayer = layerLF as IFeatureLayer;
                IFeature fe = fclLayer.FeatureClass.GetFeature(int.Parse(node.Text));

                app.MapControl.FlashShape(fe.Shape);
                
            }
        }
    }
}
