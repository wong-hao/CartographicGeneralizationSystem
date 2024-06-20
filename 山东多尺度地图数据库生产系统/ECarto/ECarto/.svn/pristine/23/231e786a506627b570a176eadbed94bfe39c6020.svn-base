using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Common
{

    public partial class LayerSelection : Form
    {
        GWorkspace workspace;
        ImageList imgList;
        int thumbWidth = 24;
        int thumbheight = 19;
        int gap = 3;

        public LayerManager.LayerChecker targetChecker { get; set; }
        public LayerManager.LayerChecker constraintChecker { get; set; }
        public string Title
        {
            set
            {
                this.Text = value;
            }
        }

        private OperationLayersInfo LayersInfo;

        public LayerSelection()
        {
            InitializeComponent();
            treeView1.CheckBoxes = true;

            imgList = new ImageList();
            imgList.ImageSize = new Size(thumbWidth, thumbheight);
            this.treeView1.ImageList = imgList;
            treeView1.ShowPlusMinus = true;
            treeView1.AfterCheck += new TreeViewEventHandler(treeView1_AfterCheck);
            treeView1.AfterSelect += new TreeViewEventHandler(treeView1_AfterSelect);
            treeView1.BeforeSelect += new TreeViewCancelEventHandler(treeView1_BeforeSelect);

            treeView2.ImageList = imgList;
            treeView2.ShowPlusMinus = true;
            treeView2.AfterCheck += new TreeViewEventHandler(treeView2_AfterCheck);

            targetChecker = null;
            constraintChecker = null;



        }

        public OperationLayerItem[] GetTargetLayerItems()
        {
            return LayersInfo.GetLayerAndCorrespondingConstraint();
        }
        /// <summary>
        /// 得到目标图层游标，必须支持
        /// </summary>
        /// <returns></returns>
        /*
        public IFeatureCursor[] GetTargetFeatureCursors(bool cursorRecyling = true)
        {
            List<IFeatureCursor> curs = new List<IFeatureCursor>();
            OperationLayerItem[] oli = LayersInfo.GetLayerAndCorrespondingConstraint();
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SearchOrder = esriSearchOrder.esriSearchOrderAttribute;
            foreach (var item in oli)
            {
                if (item.lyr is IGeoFeatureLayer)
                {
                    sf.WhereClause = GetAllClause(item);
                    curs.Add((item.lyr as IGeoFeatureLayer).FeatureClass.Search(sf, cursorRecyling));
                }
            }
            return curs.ToArray();
        }
        */


        public LayerItem[] GetLConstraintLayerItems()
        {
            return LayersInfo.GetShareConstratinLayers();
        }

        public List<string> GetTargetLayerNames()
        {
            return LayersInfo.GetTargetLayerNames();

        }

        public bool IsShowLayerContent { get { return checkBox_showfeature.Checked; } }

        public bool IsShowConstraintLayer { get { return checkBox_constraint.Checked; } }

        void treeView2_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse)
            {
                treeView1_AfterCheck(sender, e);
                RefreshNodes2OperationLayersInfo(treeView1, treeView2, !checkBox_separately.Checked);
            }
        }

        private void RefreshNodes2OperationLayersInfo(TreeView srcTree, TreeView tree, bool share)
        {
            if (share)
            {
                LayersInfo.ClearConstraintItem();
                foreach (TreeNode tn in tree.Nodes)
                {
                    if (tn.Checked && tn.Tag != null && tn.Tag is ILayer)
                    {
                        LayerItem li = CreateLayerItemFromNode(tn);
                        LayersInfo.AddConstraintItem(li);
                    }
                }
            }
            else
            {
                TreeNode tnode = srcTree.SelectedNode;
                if (tnode == null || tnode.Tag == null || !(tnode.Tag is ILayer)) return;

                LayersInfo.CrearConstraintItemForLayer(tnode.Tag as ILayer);

                foreach (TreeNode tn in tree.Nodes)
                {
                    if (tn.Checked && tn.Tag != null && tn.Tag is ILayer)
                    {
                        LayerItem li = CreateLayerItemFromNode(tn);

                        LayersInfo.AddConstraintItem2Layer(tnode, li);
                    }
                }

            }
        }

        private LayerItem CreateLayerItemFromNode(TreeNode tn)
        {
            LayerItem li = new LayerItem(tn.Tag as ILayer, tn);
            return li;
        }


        #region 重写Treeview的文字部分
        void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            //e.DrawDefault = true;

            Font nodeFont = e.Node.NodeFont;
            if (nodeFont == null) nodeFont = ((TreeView)sender).Font;
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Near;
            sf.LineAlignment = StringAlignment.Center;
            Rectangle r = Rectangle.Inflate(e.Bounds, 3, 0);
            r.X += 2;
            if ((e.State & TreeNodeStates.Selected) != 0)
            {
                e.Graphics.FillRectangle(Brushes.Blue, e.Node.Bounds);

                e.Graphics.DrawString(e.Node.Text, nodeFont, Brushes.White, r, sf);
            }
            else
            {
                e.Graphics.DrawString(e.Node.Text, nodeFont, Brushes.Black, r, sf);
            }



            //if ((e.State & TreeNodeStates.Focused) != 0)
            //{
            //    using (Pen focusPen = new Pen(Color.Black))
            //    {
            //        focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
            //        Rectangle focusBounds = e.Node.Bounds;
            //        focusBounds.Size = new Size(focusBounds.Width - 1,
            //        focusBounds.Height - 1);
            //        e.Graphics.DrawRectangle(focusPen, focusBounds);
            //    }
            //}
        }
        #endregion

        void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Action != TreeViewAction.ByMouse) { e.Cancel = true; }
            if (!(e.Node.Tag is ILayer)) { e.Cancel = true; }

        }
        void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            foreach (TreeNode tn in (sender as TreeView).Nodes)
            {
                tn.ForeColor = Color.Black;
            }
            e.Node.ForeColor = Color.Red;
            UpdateConstraintChecked(checkBox_separately.Checked);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            treeView1.Nodes.Clear();
            treeView2.Nodes.Clear();

            FillTree(treeView1, targetChecker);

            //CloneTrees(treeView1, treeView2);
            FillTree(treeView2, targetChecker, true);
            if (checkBox_showfeature.Checked)
            {
                FillLayerChildren(treeView1);
                FillLayerChildren(treeView2);
            }

            LayersInfo = new OperationLayersInfo(treeView1);
        }

        private void CloneTrees(TreeView trsSrc, TreeView treeTgt)
        {
            treeTgt.Nodes.Clear();
            foreach (TreeNode item in trsSrc.Nodes)
            {
                TreeNode newNode = (TreeNode)item.Clone();
                treeTgt.Nodes.Add(newNode);
            }
        }

        void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.ByMouse)
            {
                return;
            }

            TreeView tree = sender as TreeView;
            if (e.Node == null) return;
            TreeNode node = e.Node;
            bool ischecked = node.Checked;

            //被勾选
            if (ischecked)
            {
                if (node.Parent != null)
                {
                    node.Parent.Checked = true;
                }
                if (node.Nodes.Count > 0)
                {
                    foreach (TreeNode tn in node.Nodes)
                    {
                        tn.Checked = true;
                    }
                }
            }//没有勾选
            else
            {
                if (node.Parent != null)
                {
                    bool hasAchild = false;
                    foreach (TreeNode tnn in node.Parent.Nodes)
                    {
                        if (tnn.Checked) { hasAchild = true; break; }
                    }
                    if (!hasAchild) { node.Parent.Checked = false; }
                }
                if (node.Nodes.Count > 0)
                {
                    foreach (TreeNode tn in node.Nodes)
                    {
                        tn.Checked = false;
                    }
                }
            }

            LayersInfo.UpdateTargetLayer();

        }

        private void LayerSelection_Load(object sender, EventArgs e)
        {

        }



        private void FillLayerChildren(TreeView tree)
        {
            //if (tree == null || !tree.Enabled) { return; }
            if (tree.Nodes.Count > 0)
            {
                foreach (TreeNode tn in tree.Nodes)
                {
                    if (tn.Tag != null)
                    {
                        //只读取GeoFeatureLayer
                        if (tn.Tag is IGeoFeatureLayer)
                        {
                            tn.Nodes.AddRange(GetNodeFromLayer(tn.Tag as IGeoFeatureLayer));
                        }
                    }
                }
            }
        }

        private void RemoveLayerChildren(TreeView tree)
        {
            //if (tree == null || !tree.Enabled) { return; }

            if (tree.Nodes.Count > 0)
            {
                foreach (TreeNode tn in tree.Nodes)
                {
                    if (tn.Tag != null)
                    {
                        tn.Nodes.Clear();
                    }
                }
            }
        }

        private void FillTree(TreeView tree, LayerManager.LayerChecker checker, bool append = false)
        {
            tree.Nodes.Clear();
            if (workspace != null)
            {
                LayerInfo[] lyrs = workspace.LayerManager.GetLayer(checker);

                FillTheImage(lyrs, append);

                foreach (LayerInfo l in lyrs)
                {
                    tree.Nodes.Add(CreateLayerNode(l));
                }
            }
            tree.Refresh();
        }

        private TreeNode CreateLayerNode(LayerInfo l)
        {
            TreeNode tn = new TreeNode(l.LayerName);
            tn.Tag = l.Layer;
            if (l.Layer is IGeoFeatureLayer)
            {
                IGeoFeatureLayer gf = l.GetGeoFeatureLayer();
                switch (gf.FeatureClass.ShapeType)
                {
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                        tn.ImageKey = "Polyline";
                        tn.SelectedImageKey = "Polyline";
                        break;
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                        tn.ImageKey = "Polygon";
                        tn.SelectedImageKey = "Polygon";
                        break;
                    case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                        tn.ImageKey = "Marker";
                        tn.SelectedImageKey = "Marker";
                        break;
                    default:
                        break;
                }
            }
            else if (l.Layer is IRasterLayer)
            {

            }
            else if (l.Layer is IFDOGraphicsLayer)
            {
                tn.ImageKey = "Label";
                tn.SelectedImageKey = "Label";
            }

            return tn;
        }


        /// <summary>
        /// 如果要支持多种Renderer,则在此扩展
        /// </summary>
        /// <param name="geoFl"></param>
        /// <returns></returns>
        private TreeNode[] GetNodeFromLayer(IGeoFeatureLayer geoFl)
        {

            List<TreeNode> nodes = new List<TreeNode>();
            if (geoFl == null) return nodes.ToArray();

            if (geoFl.Renderer is IUniqueValueRenderer)
            {
                for (var i = 0; i < (geoFl.Renderer as IUniqueValueRenderer).ValueCount; i++)
                {
                    string val = (geoFl.Renderer as IUniqueValueRenderer).get_Value(i);
                    string label = (geoFl.Renderer as IUniqueValueRenderer).get_Label(val);
                    ISymbol symbol = (geoFl.Renderer as IUniqueValueRenderer).get_Symbol(val);
                    symbol = (symbol as IClone).Clone() as ISymbol;
                    TreeNode node = new TreeNode(label);

                    node.Tag = CreateFcitem(val, label, symbol, geoFl.Renderer as IRendererClasses, geoFl);

                    node.ImageKey = geoFl.Name + label;
                    node.SelectedImageKey = geoFl.Name + label;

                    nodes.Add(node);
                }
            }
            else
            {
                //********其他的Renderer
            }
            return nodes.ToArray();
        }

        private FeatureClassItem CreateFcitem(
            string value,
            string label,
            ISymbol sym,
            IRendererClasses legclss,
            ILayer pLayer)
        {
            string whereClause = string.Empty;
            for (int i = 0; i < legclss.ClassCount; i++)
            {
                string s = legclss.get_Class(i);
                if (s == label)
                {
                    whereClause = legclss.get_WhereClause(i, (pLayer as ITable));
                    break;
                }
            }
            FeatureClassItem fcitem = new FeatureClassItem();
            fcitem.Value = value;
            fcitem.Label = label;
            fcitem.WhereClause = whereClause;
            fcitem.Symbol = sym;
            fcitem.FeatureLayer = pLayer as IGeoFeatureLayer;

            return fcitem;
        }

        #region 缩略图相关
        private void FillTheDefaultImage()
        {
            ISymbol markerSymbol = RenderFeatureLayer.createDefaultSimpleSymbol(
                 ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint);
            Image img = ViewImage(markerSymbol);
            imgList.Images.Add("Marker", img);

            markerSymbol = RenderFeatureLayer.createDefaultSimpleSymbol(
                 ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon);
            img = ViewImage(markerSymbol);
            imgList.Images.Add("Polygon", img);

            markerSymbol = RenderFeatureLayer.createDefaultSimpleSymbol(
                 ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline);
            img = ViewImage(markerSymbol);
            imgList.Images.Add("Polyline", img);

            ITextSymbol ts = new TextSymbolClass();
            ts.Size = 10; ts.Color = RenderFeatureLayer.createCMYKColor(0, 0, 0, 100);
            ts.Text = "注记";
            img = ViewImage(ts as ISymbol);
            imgList.Images.Add("Label", img);


        }

        private void FillTheImage(LayerInfo[] lyrs, bool append = false)
        {
            if (!append)
            {
                imgList.Images.Clear();

                FillTheDefaultImage();
            }
            lyrs.Reverse();
            foreach (LayerInfo l in lyrs)
            {
                try
                {
                    if (l.Layer is IFeatureLayer)
                    {
                        if (l.Layer is FDOGraphicsLayerClass)
                        {

                        }
                        else if (l.Layer is IGeoFeatureLayer)
                        {
                            ReadGeoFeatureLayer(l.GetGeoFeatureLayer());
                        }
                    }
                }
                catch
                {

                }
            }
        }

        private void ReadGeoFeatureLayer(IGeoFeatureLayer geoFl)
        {
            if (geoFl == null) return;
            if (geoFl.Renderer is ISimpleRenderer)
            {
                if (!imgList.Images.ContainsKey(geoFl.Name))
                {
                    Image img = ViewImage((geoFl.Renderer as ISimpleRenderer).Symbol);
                    if (img != null)
                    {
                        imgList.Images.Add(geoFl.Name, img);
                    }
                }
            }
            else if (geoFl.Renderer is IUniqueValueRenderer)
            {
                for (var i = 0; i < (geoFl.Renderer as IUniqueValueRenderer).ValueCount; i++)
                {
                    string val = (geoFl.Renderer as IUniqueValueRenderer).get_Value(i);
                    string label = geoFl.Name + (geoFl.Renderer as IUniqueValueRenderer).get_Label(val);

                    if (!imgList.Images.ContainsKey(label))
                    {
                        ISymbol sym = (geoFl.Renderer as IUniqueValueRenderer).get_Symbol(val);
                        Image image = ViewImage(sym);
                        if (image != null) { imgList.Images.Add(label, image); }
                    }
                }
            }
            else
            {

            }
        }

        private Image ViewImage(ISymbol sym)
        {
            esriSymbologyStyleClass cls = GetSymbolType(sym);
            if (cls != esriSymbologyStyleClass.esriStyleClassVectorizationSettings)
            {
                System.Drawing.Image image = SMGI.Common.SymbolForm.ImageFromSymbol(sym, imgList.ImageSize.Width,
                    imgList.ImageSize.Height, gap);
                return image;
            }
            else
            {
                return null;
            }
        }

        private esriSymbologyStyleClass GetSymbolType(ISymbol sym)
        {
            esriSymbologyStyleClass symStyleCls;
            if (sym is IMarkerSymbol)
            {
                symStyleCls = esriSymbologyStyleClass.esriStyleClassMarkerSymbols;
            }
            else if (sym is IFillSymbol)
            {
                symStyleCls = esriSymbologyStyleClass.esriStyleClassFillSymbols;
            }
            else if (sym is ILineSymbol)
            {
                symStyleCls = esriSymbologyStyleClass.esriStyleClassLineSymbols;
            }
            else if (sym is ITextSymbol)
            {
                symStyleCls = esriSymbologyStyleClass.esriStyleClassTextSymbols;
            }
            else
            {
                symStyleCls = esriSymbologyStyleClass.esriStyleClassVectorizationSettings;
            }
            return symStyleCls;
        }
        #endregion


        public DialogResult SelectLayer(GWorkspace ws)
        {
            workspace = ws;
            if (workspace == null)
                return System.Windows.Forms.DialogResult.Cancel;
            return ShowDialog();
        }


        private void checkBox_constraint_CheckedChanged(object sender, EventArgs e)
        {
            groupBox_constrained.Enabled = ((CheckBox)sender).Checked;
            if (groupBox_constrained.Enabled)
            {
                treeView2.Nodes.Clear();
                //CloneTrees(treeView1, treeView2);
                FillTree(treeView2, constraintChecker, true);
                if (checkBox_showfeature.Checked) { FillLayerChildren(treeView2); }
            }
        }

        private void checkBox_showfeature_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                FillLayerChildren(treeView1);
                FillLayerChildren(treeView2);

            }
            else
            {
                RemoveLayerChildren(treeView1);
                RemoveLayerChildren(treeView2);
            }
        }

        public void AllChecked(TreeNode tn, bool ch)
        {
            if (tn != null)
            {
                tn.Checked = ch;
                foreach (TreeNode t in tn.Nodes)
                {
                    AllChecked(t, ch);
                }
            }
        }
        public void SwitchChecked(TreeNode tn)
        {
            if (tn != null)
            {
                tn.Checked = !tn.Checked;
                foreach (TreeNode t in tn.Nodes)
                {
                    t.Checked = t.Parent.Checked;
                }
            }
        }

        private void checkBox_separately_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox check = sender as CheckBox;
            UpdateConstraintChecked(check.Checked);
        }

        private void UpdateConstraintChecked(bool sep)
        {
            foreach (TreeNode item in treeView2.Nodes)
            {
                AllChecked(item, false);
            }
            if (sep)
            {
                if (treeView1.SelectedNode != null)
                {
                    LayersInfo.SynChecked(treeView1.SelectedNode.Text, treeView2);
                }
            }
            else
            {
                LayersInfo.SynChecked(string.Empty, treeView2);
            }
        }

        private void linkLabel_all_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (TreeNode tn in treeView1.Nodes)
            {
                AllChecked(tn, true);
            }
        }

        private void linkLabel_No_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (TreeNode tn in treeView1.Nodes)
            {
                AllChecked(tn, false);
            }
        }

        private void linkLabel_switch_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (TreeNode tn in treeView1.Nodes)
            {
                SwitchChecked(tn);
            }
        }

    }


    public class OperationLayersInfo
    {
        bool shareConstraintLayer = true;

        private Dictionary<string, OperationLayerItem> items;
        private Dictionary<string, LayerItem> shareItems;


        internal OperationLayersInfo(TreeView t)
        {
            items = new Dictionary<string, OperationLayerItem>();
            shareItems = new Dictionary<string, LayerItem>();
            InitOperationLayersFromTree(t);
        }

        public void InitOperationLayersFromTree(TreeView tree)
        {
            items.Clear();
            foreach (TreeNode item in tree.Nodes)
            {
                if (item.Tag != null && item.Tag is ILayer)
                {
                    OperationLayerItem oi = new OperationLayerItem(item.Tag as ILayer, item);
                    items.Add(item.Text, oi);
                }
            }
        }

        internal void ClearConstraintItem()
        {
            shareItems.Clear();
        }

        internal void CrearConstraintItemForLayer(ILayer l)
        {
            if (items.ContainsKey(l.Name))
            {
                items[l.Name].ClearConstraint();
            }
        }

        internal void AddConstraintItem(LayerItem it)
        {
            if (shareItems.ContainsKey(it.lyr.Name))
            {
                shareItems[it.lyr.Name] = it;
            }
            else
            {
                shareItems.Add(it.lyr.Name, it);
            }
        }

        internal void AddConstraintItem2Layer(TreeNode tn, LayerItem it)
        {
            ILayer l = tn.Tag as ILayer;
            if (items.ContainsKey(l.Name))
            {
                items[l.Name].AddConstraintLayerItem(it);

            }
            else
            {
                OperationLayerItem oi = new OperationLayerItem(l, tn);
                oi.AddConstraintLayerItem(it);
                items.Add(l.Name, oi);
            }
        }

        internal void SynChecked(string lyrName, TreeView tree)
        {
            Dictionary<string, LayerItem> targetItems = null;

            if (lyrName == string.Empty)
            {
                targetItems = shareItems;
            }
            else
            {
                if (items.ContainsKey(lyrName))
                {
                    targetItems = items[lyrName].ConstratintLayerItems;
                }
            }

            if (targetItems == null) { return; }


            foreach (TreeNode t in tree.Nodes)
            {
                if (targetItems.ContainsKey(t.Text))
                {
                    t.Checked = true;
                    SynChildrenChecked(t, targetItems[t.Text]);
                }
            }
        }

        internal void SynChildrenChecked(TreeNode t, LayerItem l)
        {
            List<string> strs = l.getLabelString();
            foreach (TreeNode tn in t.Nodes)
            {
                if (strs.Contains(tn.Text)) { tn.Checked = true; }
            }
        }

        internal void UpdateTargetLayer()
        {
            foreach (OperationLayerItem item in items.Values)
            {
                item.UpdateSubItemsByNode();
            }
        }

        internal LayerItem[] GetShareConstratinLayers()
        {
            return shareItems.Values.ToArray();
        }

        internal OperationLayerItem[] GetLayerAndCorrespondingConstraint()
        {
            List<OperationLayerItem> its = new List<OperationLayerItem>();
            foreach (OperationLayerItem item in items.Values)
            {
                if (item.Tag != null && item.Tag is TreeNode && (item.Tag as TreeNode).Checked)
                {
                    its.Add(item);
                }
            }
            return its.ToArray();
        }

        internal List<string> GetTargetLayerNames()
        {
            List<string> its = new List<string>();

            foreach (OperationLayerItem item in items.Values)
            {
                if (item.Tag != null && item.Tag is TreeNode && (item.Tag as TreeNode).Checked)
                {
                    its.Add(item.lyr.Name);
                }
            }

            return its;
        }


    }

    public class OperationLayerItem : LayerItem
    {
        private Dictionary<string, LayerItem> constraintLyrs { get; set; }

        internal OperationLayerItem(ILayer l, object t)
            : base(l, t)
        {
            constraintLyrs = new Dictionary<string, LayerItem>();
        }

        internal void AddConstraintLayerItem(LayerItem it)
        {
            if (constraintLyrs.ContainsKey(it.lyr.Name))
            {
                constraintLyrs[it.lyr.Name] = it;
            }
            else
            {
                constraintLyrs.Add(it.lyr.Name, it);
            }
        }

        internal Dictionary<string, LayerItem> ConstratintLayerItems
        {
            get { return constraintLyrs; }
        }
        public LayerItem[] ConstratintLayers
        {
            get { return constraintLyrs.Values.ToArray(); }
        }
        internal void ClearConstraint()
        {
            constraintLyrs.Clear();
        }

    }

    public class LayerItem
    {
        public ILayer lyr { get; set; }
        public IFeatureCursor FeatureCursor
        {
            get
            {
                if (lyr is IGeoFeatureLayer)
                {
                    IQueryFilter sf = new QueryFilterClass();
                    sf.WhereClause = GetAllClause();
                    return (lyr as IGeoFeatureLayer).FeatureClass.Search(sf, true);
                }
                else
                {
                    return null;
                }
            }
        }
        internal List<FeatureClassItem> Items { get; set; }
        internal object Tag { get; set; }
        internal bool Checked { get { return (Tag as TreeNode).Checked; } }
        private string GetAllClause()
        {
            string cls = string.Empty;
            foreach (var item in Items)
            {
                cls += item.WhereClause + " AND ";
            }
            return cls;
        }
        internal LayerItem(ILayer l, object t)
        {
            lyr = l;
            Tag = t;
            Items = new List<FeatureClassItem>();
            UpdateSubItemsByNode();
        }
        private void AddSubItems(FeatureClassItem fcitem)
        {
            if (!Items.Exists(new Predicate<FeatureClassItem>(x => { return x.Label == fcitem.Label; })))
            {
                Items.Add(fcitem);
            }
        }
        internal void UpdateSubItemsByNode()
        {
            Items.Clear();
            foreach (TreeNode t in (Tag as TreeNode).Nodes)
            {
                if (t.Checked && t.Tag != null && t.Tag is FeatureClassItem)
                {
                    this.AddSubItems(t.Tag as FeatureClassItem);
                }
            }
        }

        internal List<string> getLabelString()
        {
            List<string> ss = new List<string>();
            foreach (FeatureClassItem s in Items)
            {
                ss.Add(s.Label);
            }
            return ss;
        }

        public FeatureClassItem[] QueryItems
        {
            get { return Items.ToArray(); }
        }


    }

    public class FeatureClassItem
    {
        public string Value { get; set; }
        public string Label { get; set; }
        public string WhereClause { get; set; }
        public ISymbol Symbol { get; set; }
        public IGeoFeatureLayer FeatureLayer { get; internal set; }
        public IFeatureCursor FeatureCursor
        {
            get
            {
                IFeatureCursor fc = null;
                try
                {
                    if (FeatureLayer != null)
                    {
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = WhereClause;
                        fc = FeatureLayer.Search(qf, true);
                    }
                }
                catch
                {
                }
                return fc;
            }
        }
        public FeatureClassItem() { }
    }

}
