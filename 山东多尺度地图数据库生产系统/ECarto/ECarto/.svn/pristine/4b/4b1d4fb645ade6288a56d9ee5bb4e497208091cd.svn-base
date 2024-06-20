using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.CollaborativeWork
{
    public partial class FrmConflictResult : DevExpress.XtraEditors.XtraForm
    {
        private struct ConflictFeatures
        {
            public IFeature localFeature;
            public IFeature serverFeature;
        }

        private int _nConflict;
        private int _nUntreatedConflict;
        private GApplication _app;

        List<string> _autoProcessFeaGUID;//程序自动处理的冲突要素的guid集合

        private const double _toleranceValue = 0.01;//(米)

        public FrmConflictResult(GApplication app)
        {
            InitializeComponent();

            _app = app;

            _nUntreatedConflict = 0;
            _nConflict = 0;
            _autoProcessFeaGUID = new List<string>();

            cbFilter.Items.Add("显示所有冲突");
            cbFilter.Items.Add("仅显示未处理冲突");
            cbFilter.Items.Add("仅显示程序自动处理的伪冲突");
            cbFilter.SelectedIndex = 0;

            dgConflictResult.ForeColor = Color.FromArgb(0, 0, 0);
        }

        #region 事件

        private void cbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            upateTable();
            dgConflictResult.Focus();
        }

        //筛选出冲突要素中属性相同（除oid和collabversion外），几何体相同的冲突
        private void btnAutoProcess_Click(object sender, EventArgs e)
        {
            if (DialogResult.No == MessageBox.Show("程序将会根据冲突要素的属性和几何拓扑关系进行伪冲突的自动处理,是否继续？", "提示", MessageBoxButtons.YesNo))
            {
                return;
            }

            _autoProcessFeaGUID = new List<string>();

            string machineName = System.Environment.MachineName;

            using (var wo = GApplication.Application.SetBusy())
            {
                IMap pMap = GApplication.Application.ActiveView as IMap;

                //第一步，获取明显可忽略的冲突
                wo.SetText(string.Format("正在进行初步筛选..."));
                Dictionary<string, Dictionary<string,int>> conflicts = new Dictionary<string, Dictionary<string,int>>();//Dictionary<layerName, Dictionary<guid,服务器冲突要素版本号>>
                for (int i = 0; i < dgConflictResult.RowCount; ++i)
                {
                    string guid = dgConflictResult.Rows[i].Cells["GUID"].Value.ToString();

                    int localVer, serverVer;
                    int.TryParse(dgConflictResult.Rows[i].Cells["localFetureVer"].Value.ToString(), out localVer);
                    int.TryParse(dgConflictResult.Rows[i].Cells["serverFeatureVer"].Value.ToString(), out serverVer);

                    string localDelState = dgConflictResult.Rows[i].Cells["localFetureDelState"].Value.ToString();
                    string serverDelState = dgConflictResult.Rows[i].Cells["serverFeatureDelState"].Value.ToString();
                    if (localDelState == ServerDataInitializeCommand.DelStateText && serverDelState == ServerDataInitializeCommand.DelStateText)//都为删除状态，可忽略该冲突，自动判定为已处理
                    {
                        _autoProcessFeaGUID.Add(guid);//标识为程序自动处理的伪冲突
                    }

                    //初步过滤
                    string opuser = dgConflictResult.Rows[i].Cells["ServerFeatureOPUser"].Value.ToString();
                    /*
                    if (opuser != machineName)
                        continue;
                     */ 

                    //获取需进一步判断的冲突相关信息
                    string layerName = dgConflictResult.Rows[i].Cells["localFeatureLN"].Value.ToString();
                    if (!conflicts.ContainsKey(layerName))//第一次遇到改图层
                    {
                        Dictionary<string, int> guid2ver = new Dictionary<string, int>();
                        guid2ver.Add(guid, serverVer);

                        conflicts.Add(layerName, guid2ver);
                    }
                    else//该图层已存在
                    {
                        var dics = conflicts[layerName];
                        if (!dics.ContainsKey(guid))
                        {
                            dics.Add(guid, serverVer);
                        }
                    }
                    
                }

                //第二步，获取需进一步判断的冲突要素
                wo.SetText(string.Format("正在获取冲突要素..."));
                Dictionary<string, ConflictFeatures> confictFeas = new Dictionary<string, ConflictFeatures>();//Dictionary<GUID, ConflictFeatures>
                foreach (var kv in conflicts)
                {
                    string layerName = kv.Key;

                    IFeatureLayer pLayer = getLayerByLayerName(pMap, layerName) as IFeatureLayer;
                    if (null == pLayer)
                        continue;

                    string guidSet = "";
                    foreach (var item in conflicts[layerName])
                    {
                        if (guidSet != "")
                            guidSet += string.Format(",'{0}'", item.Key);
                        else
                            guidSet = string.Format("'{0}'", item.Key);
                    }

                    if (guidSet != "")
                    {
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = string.Format("{0} in ({1})", ServerDataInitializeCommand.CollabGUID, guidSet);
                        IFeatureCursor fCursor = pLayer.Search(qf, false);
                        IFeature fe;
                        while ((fe = fCursor.NextFeature()) != null)
                        {
                            string guid = fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabGUID)).ToString();
                            if (!confictFeas.ContainsKey(guid))//第一次遇到
                            {
                                ConflictFeatures feas = new ConflictFeatures { localFeature = null, serverFeature = null };

                                int ver;
                                int.TryParse(fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString(), out ver);
                                if (ver > GApplication.Application.Workspace.LocalBaseVersion)
                                {
                                    feas.serverFeature = fe;
                                }
                                else
                                {
                                    feas.localFeature = fe;
                                }

                                confictFeas.Add(guid, feas);
                            }
                            else//非第一次遇到
                            {
                                ConflictFeatures feas = confictFeas[guid];

                                int ver;
                                int.TryParse(fe.get_Value(fe.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString(), out ver);
                                if (ver > GApplication.Application.Workspace.LocalBaseVersion)
                                {
                                    feas.serverFeature = fe;
                                }
                                else
                                {
                                    feas.localFeature = fe;
                                }

                                confictFeas[guid] = feas;
                            }
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                    }
                }
                conflicts.Clear();

                //第三步，根据冲突要素的属性和几何拓扑判断该冲突是否可忽略
                wo.SetText(string.Format("正在依据属性及几何拓扑关系进行预处理..."));
                foreach (var kv in confictFeas)
                {
                    IFeature localFea = kv.Value.localFeature;
                    IFeature serverFea = kv.Value.serverFeature;
                    if (serverFea == null || localFea == null)//（解决任意一处为空的问题）（f2671c2b=2022.8.8）
                        continue;                    

                    //属性过滤
                    bool bPro = true;
                    for (int i = 0; i < localFea.Fields.FieldCount; i++)
                    {
                        IField field = localFea.Fields.get_Field(i);
                        if (field.Type == esriFieldType.esriFieldTypeGeometry || field.Type == esriFieldType.esriFieldTypeOID)
                            continue;

                        if (field.Name.ToUpper() == ServerDataInitializeCommand.CollabVERSION)
                            continue;
                        if (field.Name.ToUpper() == ServerDataInitializeCommand.CollabOPUSER)
                            continue;

                        string localVal = localFea.get_Value(i).ToString().Trim();
                        string serverVal = serverFea.get_Value(i).ToString().Trim();
                        if (localVal != serverVal)
                        {
                            bPro = false;
                            break;
                        }
                    }

                    if (bPro == false)
                        continue;//属性不一致

                    //几何拓扑过滤
                    IRelationalOperator relOperator = localFea.Shape as IRelationalOperator;
                    bool relEqual = relOperator.Equals(serverFea.Shape);
                    if (relEqual == false)
                        continue;

                    //程序认定为可忽略的冲突
                    string guid = localFea.get_Value(localFea.Fields.FindField(ServerDataInitializeCommand.CollabGUID)).ToString();
                    _autoProcessFeaGUID.Add(guid);//标识为程序自动处理的伪冲突

                }
                wo.SetText(string.Format("处理完成！"));
            }


            cbFilter.SelectedIndex = 2;
        }

        private void dgConflictResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridViewCheckBoxCell checkboxCell = dgConflictResult.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewCheckBoxCell;
            if (checkboxCell != null)//修改冲突确认状态
            {
                string guid = dgConflictResult.Rows[e.RowIndex].Cells["GUID"].Value.ToString();

                if (CollaborativeTask.Instance.LocalConflictFeaturesState.ContainsKey(guid))
                {
                    if (Convert.ToBoolean(checkboxCell.EditedFormattedValue))
                    {
                        CollaborativeTask.Instance.LocalConflictFeaturesState[guid] = true;
                        _nUntreatedConflict--;

                    }
                    else
                    {
                        CollaborativeTask.Instance.LocalConflictFeaturesState[guid] = false;
                        _nUntreatedConflict++;
                    }

                    if (_autoProcessFeaGUID != null && _autoProcessFeaGUID.Contains(guid))
                    {
                        _autoProcessFeaGUID.Remove(guid);
                        dgConflictResult.Rows[e.RowIndex].Cells["remark"].Value = "";
                    }
                }

                lbInfo.Text = string.Format("冲突未处理的要素({0})/冲突要素({1})", _nUntreatedConflict, _nConflict);

                return;
            }

            if ("PropertyProcess" == dgConflictResult.Columns[e.ColumnIndex].Name)//属性冲突处理
            {
                if (_app.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing)
                {
                    MessageBox.Show("请先开启编辑！");
                    return;
                }

                #region 属性冲突处理
                IFeature localFe = null, serverFe = null;
                #region 获取本地要素、服务器要素
                IMap pMap = GApplication.Application.ActiveView as IMap;
                string guid = dgConflictResult.Rows[e.RowIndex].Cells["GUID"].Value.ToString();
                //本地要素
                {
                    string layerName = dgConflictResult.Rows[e.RowIndex].Cells["localFeatureLN"].Value.ToString();

                    IFeatureLayer pLayer = getLayerByLayerName(pMap, layerName) as IFeatureLayer;
                    if (pLayer != null)
                    {
                        string version = dgConflictResult.Rows[e.RowIndex].Cells["localFetureVer"].Value.ToString();

                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = string.Format("{0} = '{1}' and {2} = {3}",
                            ServerDataInitializeCommand.CollabGUID, guid, ServerDataInitializeCommand.CollabVERSION, version);
                        IFeatureCursor fCursor = pLayer.FeatureClass.Search(qf, false);
                        localFe = fCursor.NextFeature();
                    }
                }

                //服务器要素
                {
                    if (dgConflictResult.Rows[e.RowIndex].Cells["serverFeatureDelState"].Value.ToString() != "删除")
                    {
                        string layerName = dgConflictResult.Rows[e.RowIndex].Cells["serverFeatureLN"].Value.ToString();

                        IFeatureLayer pLayer = getLayerByLayerName(pMap, layerName) as IFeatureLayer;
                        if (pLayer != null)
                        {
                            string version = dgConflictResult.Rows[e.RowIndex].Cells["serverFeatureVer"].Value.ToString();

                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = string.Format("{0} = '{1}' and {2} = {3}",
                            ServerDataInitializeCommand.CollabGUID, guid, ServerDataInitializeCommand.CollabVERSION, version);
                            IFeatureCursor fCursor = pLayer.FeatureClass.Search(qf, false);
                            serverFe = fCursor.NextFeature();

                            System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                        }


                    }

                }
                #endregion

                if (localFe == null || serverFe == null)
                {
                    MessageBox.Show("冲突要素不存在！");
                    return;
                }
                else
                {
                    Dictionary<string, KeyValuePair<object, object>> conflictPropertys = getConflictPropertys(localFe, serverFe);
                    if (conflictPropertys == null || conflictPropertys.Count == 0)
                    {
                        MessageBox.Show("不存在属性冲突！");
                        return;
                    }

                    PropertyConflictProcessForm frm = new PropertyConflictProcessForm(conflictPropertys);
                    if (frm.ShowDialog() == System.Windows.Forms.DialogResult.OK && frm.NeedUpdatePropertyList.Count > 0)
                    {
                        //更新本地要素属性
                        _app.EngineEditor.StartOperation();
                        try
                        {
                            foreach (var kv in frm.NeedUpdatePropertyList)
                            {
                                int index = localFe.Fields.FindField(kv.Key);
                                localFe.set_Value(index, kv.Value);
                            }
                            localFe.Store();
                            _app.EngineEditor.StopOperation("本地要素属性更新");
                        }
                        catch(Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine(ex.Message);
                            System.Diagnostics.Trace.WriteLine(ex.Source);
                            System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                            MessageBox.Show(ex.Message);

                            _app.EngineEditor.AbortOperation();
                        }

                    }
                }
                #endregion
            }

            DataGridViewButtonCell btCell = dgConflictResult.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewButtonCell;
            if (btCell != null)//缩放至要素
            {
                #region 缩放至要素
                IFeatureLayer pLayer = null;
                IFeature f = null;
                IMap pMap = GApplication.Application.ActiveView as IMap;
                string guid = dgConflictResult.Rows[e.RowIndex].Cells["GUID"].Value.ToString();
                if ("LocalFeature" == dgConflictResult.Columns[e.ColumnIndex].Name)
                {
                    string layerName = dgConflictResult.Rows[e.RowIndex].Cells["localFeatureLN"].Value.ToString();

                    pLayer = getLayerByLayerName(pMap, layerName) as IFeatureLayer;
                    if (pLayer != null)
                    {
                        string version = dgConflictResult.Rows[e.RowIndex].Cells["localFetureVer"].Value.ToString();

                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = string.Format("{0} = '{1}' and {2} = {3}",
                            ServerDataInitializeCommand.CollabGUID, guid, ServerDataInitializeCommand.CollabVERSION, version);
                        IFeatureCursor fCursor = pLayer.FeatureClass.Search(qf, false);
                        f = fCursor.NextFeature();

                        System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);

                        if (null == f)
                        {
                            MessageBox.Show("该要素不存在！");
                            return;
                        }
                        ZoomToFeature(pLayer, f);
                    }
                }
                else if ("serverFeature" == dgConflictResult.Columns[e.ColumnIndex].Name)
                {
                    string delState = dgConflictResult.Rows[e.RowIndex].Cells["serverFeatureDelState"].Value.ToString();
                    if (delState != "删除")
                    {
                        string layerName = dgConflictResult.Rows[e.RowIndex].Cells["serverFeatureLN"].Value.ToString();

                        pLayer = getLayerByLayerName(pMap, layerName) as IFeatureLayer;
                        if (pLayer != null)
                        {
                            string version = dgConflictResult.Rows[e.RowIndex].Cells["serverFeatureVer"].Value.ToString();

                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = string.Format("{0} = '{1}' and {2} = {3}",
                            ServerDataInitializeCommand.CollabGUID, guid, ServerDataInitializeCommand.CollabVERSION, version);
                            IFeatureCursor fCursor = pLayer.FeatureClass.Search(qf, false);
                            f = fCursor.NextFeature();

                            System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);

                            if (null == f)
                            {
                                MessageBox.Show("该要素不存在！");
                                return;
                            }
                            ZoomToFeature(pLayer, f);
                        }

                        
                    }
                    else
                    {
                        MessageBox.Show("该要素在服务器中已被删除！");
                    }

                }
                #endregion
            }
            
        }


        private void dgConflictResult_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            int ri = dgConflictResult.CurrentCell.RowIndex;
            int ci = dgConflictResult.CurrentCell.ColumnIndex;
            if (ri != -1 && dgConflictResult.Columns[ci].Name == "GeoProcess")
            {
                ((ComboBox)e.Control).SelectedIndexChanged -= new EventHandler(FrmConflictResult_SelectedIndexChanged);
                ((ComboBox)e.Control).SelectedIndexChanged += new EventHandler(FrmConflictResult_SelectedIndexChanged);
            }
        }

        /// <summary>
        /// 自定义事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmConflictResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            int ri = dgConflictResult.CurrentCell.RowIndex;
            int ci = dgConflictResult.CurrentCell.ColumnIndex;

            //if(false)
            if (cb.Text.Contains("合并几何"))
            {
                IFeature localFeature = null;
                IFeature serverFeature = null;
                IPolygon pl = CollaborativeTask.Instance.ExtentPolygon;

                #region 获取本地要素、服务器要素
                IFeatureLayer pLayer = null;
                IFeature f = null;
                IMap pMap = GApplication.Application.ActiveView as IMap;
                string guid = dgConflictResult.Rows[ri].Cells["GUID"].Value.ToString();
                //本地要素
                {
                    string layerName = dgConflictResult.Rows[ri].Cells["localFeatureLN"].Value.ToString();

                    pLayer = getLayerByLayerName(pMap, layerName) as IFeatureLayer;
                    if (pLayer != null)
                    {
                        string version = dgConflictResult.Rows[ri].Cells["localFetureVer"].Value.ToString();

                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = string.Format("{0} = '{1}' and {2} = {3}",
                            ServerDataInitializeCommand.CollabGUID, guid, ServerDataInitializeCommand.CollabVERSION, version);
                        IFeatureCursor fCursor = pLayer.FeatureClass.Search(qf, false);
                        localFeature = fCursor.NextFeature();
                    }
                }

                //服务器要素
                {
                    if (dgConflictResult.Rows[ri].Cells["serverFeatureDelState"].Value.ToString() != "删除")
                    {
                        string layerName = dgConflictResult.Rows[ri].Cells["serverFeatureLN"].Value.ToString();

                        pLayer = getLayerByLayerName(pMap, layerName) as IFeatureLayer;
                        if (pLayer != null)
                        {
                            string version = dgConflictResult.Rows[ri].Cells["serverFeatureVer"].Value.ToString();

                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = string.Format("{0} = '{1}' and {2} = {3}",
                            ServerDataInitializeCommand.CollabGUID, guid, ServerDataInitializeCommand.CollabVERSION, version);
                            IFeatureCursor fCursor = pLayer.FeatureClass.Search(qf, false);
                            serverFeature = fCursor.NextFeature();

                            System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                        }


                    }

                }
                #endregion

                //根据处理方式的选择方法处理要素冲突
                if (localFeature == null || serverFeature == null)
                {
                    return;
                }


                if (_app.EngineEditor.EditState != esriEngineEditState.esriEngineStateEditing)
                {
                    MessageBox.Show("请先开启编辑！");
                    return;
                }

                try
                {
                    #region 合并几何，更新本地要素几何及属性
                    IGeometry newShape = null;
                    if (localFeature.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
                    {
                        //几何不做处理
                    }
                    else if (localFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
                    {
                        if (localFeature.Shape.SpatialReference.Name != pl.SpatialReference.Name)
                            pl.Project(localFeature.Shape.SpatialReference);//投影变换
                        ITopologicalOperator op = pl as ITopologicalOperator;
                        op.Simplify();


                        //求服务器要素与作业范围面求差的几何部分
                        IPolyline interGeo = op.Intersect(serverFeature.Shape, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;
                        var pOutline = (serverFeature.ShapeCopy as ITopologicalOperator).Difference(interGeo);
                        var outGC = pOutline as IGeometryCollection;
                        if (outGC != null && outGC.GeometryCount > 0)
                        {
                            //求本地要素与作业范围面相交的几何部分
                            var inLine = op.Intersect(localFeature.Shape, esriGeometryDimension.esriGeometry1Dimension) as IPolyline;

                            //判断本地要素与服务器要素在作业范围面的交点是否重合
                            bool bEnableUnion = false;
                            IPointCollection serverInterPoints = op.Intersect(serverFeature.Shape, esriGeometryDimension.esriGeometry0Dimension) as IPointCollection;
                            IPointCollection localInterPoints = op.Intersect(localFeature.Shape as IGeometry, esriGeometryDimension.esriGeometry0Dimension) as IPointCollection;
                            if (serverInterPoints != null && localInterPoints != null && serverInterPoints.PointCount == localInterPoints.PointCount)
                            {
                                int i = 0;
                                for (; i < serverInterPoints.PointCount; ++i)
                                {
                                    IPoint p1 = serverInterPoints.get_Point(i);
                                    bool bExistSame = false;
                                    for (int j = 0; j < localInterPoints.PointCount; ++j)
                                    {
                                        IPoint p2 = localInterPoints.get_Point(i);

                                        if ((p1 as IProximityOperator).ReturnDistance(p2) < _toleranceValue)
                                        {
                                            bExistSame = true;
                                            break;
                                        }
                                    }
                                    if (!bExistSame)//没有找到对应的点
                                    {
                                        break;
                                    }
                                }
                                if (i == serverInterPoints.PointCount)//全部有对应的点
                                {
                                    bEnableUnion = true;
                                }
                            }

                            if (bEnableUnion)
                            {
                                //合并
                                (inLine as ITopologicalOperator).Simplify();
                                (pOutline as ITopologicalOperator).Simplify();
                                newShape = (inLine as ITopologicalOperator).Union(pOutline);
                            }
                            else
                            {
                                MessageBox.Show("本地要素与服务器要素在作业范围面的交点不一致，无法进行几何的拼接，请先进行人工处理！", "提示");
                                newShape = null;
                            }

                        }
                        else
                        {
                            //服务器要素在作业范围外没有几何，故本地要素的几何不做改变
                        }

                    }
                    else if (localFeature.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
                    {

                        if (localFeature.Shape.SpatialReference.Name != pl.SpatialReference.Name)
                            pl.Project(localFeature.Shape.SpatialReference);//投影变换
                        ITopologicalOperator op = pl as ITopologicalOperator;
                        op.Simplify();


                        //求服务器要素与作业范围面求差的几何部分
                        IPolygon interGeo = op.Intersect(serverFeature.ShapeCopy as IGeometry, esriGeometryDimension.esriGeometry2Dimension) as IPolygon;
                        var pOutPolygon = (serverFeature.ShapeCopy as ITopologicalOperator).Difference(interGeo);
                        var gc = (pOutPolygon as IPolygon4).ConnectedComponentBag as IGeometryCollection;
                        if (gc.GeometryCount > 0)
                        {
                            //求本地要素与作业范围面相交的几何部分
                            IPolygon pInPolygon = op.Intersect(localFeature.ShapeCopy as IGeometry, esriGeometryDimension.esriGeometry2Dimension) as IPolygon;
                            newShape = pInPolygon;

                            //几何拼接
                            IProximityOperator ProxiOP = pOutPolygon as IProximityOperator;
                            if (ProxiOP.ReturnDistance(newShape) < _toleranceValue * 0.0001)
                            {
                                (newShape as ITopologicalOperator).Simplify();
                                (pOutPolygon as ITopologicalOperator).Simplify();

                                //更新几何
                                newShape = (newShape as ITopologicalOperator).Union(pOutPolygon);
                            }
                            else
                            {
                                MessageBox.Show("本地要素与服务器要素在作业范围边界处的位置有差异，无法进行几何的拼接，请先进行人工处理！", "提示");
                                newShape = null;
                            }
                        }
                        else
                        {
                            //服务器要素在作业范围外没有几何，故本地要素的几何不做改变
                        }
                    }

                    //更新本地要素几何和属性
                    if (newShape != null)
                    {
                        //更新几何
                        try
                        {
                            _app.EngineEditor.StartOperation();

                            (newShape as ITopologicalOperator).Simplify();
                            localFeature.Shape = newShape;

                            localFeature.Store();

                            _app.EngineEditor.StopOperation(string.Format("更新本地要素类【{0}】中的要素【{1}】!", localFeature.Class.AliasName, localFeature.OID));


                            //预处理只能运行一次
                            MessageBox.Show("处理完成！");
                        }
                        catch (Exception ex1)
                        {
                            _app.EngineEditor.AbortOperation();

                            throw ex1;
                        }

                    }
                    #endregion
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine(ex.Message);
                    System.Diagnostics.Trace.WriteLine(ex.Source);
                    System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                    MessageBox.Show(ex.Message);
                }
                

            }
        }

        #endregion

        #region 方法
        /// <summary>
        /// 更新表格
        /// </summary>
        public void upateTable()
        {
            dgConflictResult.Rows.Clear();

            Dictionary<string, bool> conflictFeaturesState = CollaborativeTask.Instance.LocalConflictFeaturesState;
            if (null == conflictFeaturesState)
            {
                lbInfo.Text = string.Format("冲突未处理的要素(0)/冲突要素(0)");

                return;
            }

            if (_autoProcessFeaGUID != null)//程序自动处理的
            {
                foreach (var item in _autoProcessFeaGUID)
                {
                    CollaborativeTask.Instance.LocalConflictFeaturesState[item] = true;
                }
                
            }

            _nUntreatedConflict = 0;
            foreach (KeyValuePair<string, bool> kv in conflictFeaturesState)//统计未处理冲突数
            {
                if (!kv.Value)
                    ++_nUntreatedConflict;
            }
            _nConflict = conflictFeaturesState.Count;
            lbInfo.Text = string.Format("冲突未处理的要素({0})/冲突要素({1})", _nUntreatedConflict, _nConflict);

  
            IMap pMap = GApplication.Application.ActiveView as IMap;
            string localFCName = "";
            int localFeatureVersion = int.MaxValue;
            string localFeatureDelState = "";
            string severFCName = "";
            int serverFeatureVersion = int.MaxValue;
            string serverFeatureDelState = "";
            string serverFeatureOpUser = "";
            foreach (KeyValuePair<string, bool> kv in conflictFeaturesState)
            {

                foreach (var item in CollaborativeTask.Instance.LocalConflictFeatures)//获取本地编辑要素的相关信息
                {
                    if (item.Value.ContainsKey(kv.Key))
                    {
                        localFCName = item.Key;
                        localFeatureVersion = item.Value[kv.Key].collabVer;
                        localFeatureDelState = item.Value[kv.Key].collabDel;

                        break;
                    }
                }

                foreach (var item in CollaborativeTask.Instance.ServerConflictFeatures)//获取服务器中冲突要素的相关信息
                {
                    if (item.Value.ContainsKey(kv.Key))
                    {
                        severFCName = item.Key.Split('.').Last();
                        serverFeatureVersion = item.Value[kv.Key].collabVer;
                        serverFeatureDelState = item.Value[kv.Key].collabDel;
                        serverFeatureOpUser = item.Value[kv.Key].opuser;
                        

                        break;
                    }
                }

                if (1 == cbFilter.SelectedIndex && kv.Value)//仅显示未处理的
                    continue;//不显示
                else if (2 == cbFilter.SelectedIndex && !_autoProcessFeaGUID.Contains(kv.Key))//仅显示程序预处理的
                    continue;//不显示

                int rowIndex = dgConflictResult.Rows.Add();
                dgConflictResult.Rows[rowIndex].HeaderCell.Value = dgConflictResult.RowCount.ToString();

                dgConflictResult.Rows[rowIndex].Cells["GUID"].Value = kv.Key;
                dgConflictResult.Rows[rowIndex].Cells["localFeatureLN"].Value = localFCName;
                dgConflictResult.Rows[rowIndex].Cells["localFetureVer"].Value = localFeatureVersion;
                if (localFeatureDelState == ServerDataInitializeCommand.DelStateText)
                {
                    dgConflictResult.Rows[rowIndex].Cells["localFetureDelState"].Value = "删除";
                }
                else
                {
                    dgConflictResult.Rows[rowIndex].Cells["localFetureDelState"].Value = "";
                }

                dgConflictResult.Rows[rowIndex].Cells["ServerFeatureOPUser"].Value = serverFeatureOpUser;
                dgConflictResult.Rows[rowIndex].Cells["serverFeatureLN"].Value = severFCName;
                dgConflictResult.Rows[rowIndex].Cells["serverFeatureVer"].Value = serverFeatureVersion.ToString();
                if (serverFeatureDelState == ServerDataInitializeCommand.DelStateText)
                {
                    dgConflictResult.Rows[rowIndex].Cells["serverFeatureDelState"].Value = "删除";
                }
                else
                {
                    dgConflictResult.Rows[rowIndex].Cells["serverFeatureDelState"].Value = "";
                }

                if (_autoProcessFeaGUID != null && _autoProcessFeaGUID.Contains(kv.Key))
                {
                    dgConflictResult.Rows[rowIndex].Cells["remark"].Value = "自动处理的伪冲突";
                }
                
                ((DataGridViewCheckBoxCell)dgConflictResult.Rows[rowIndex].Cells["Processed"]).Value = kv.Value;  
            }
            
        }

        /// <summary>
        /// 缩放至要素
        /// </summary>
        /// <param layerName="f"></param>
        /// <param name="f"></param>
        private void ZoomToFeature(IFeatureLayer pLayer, IFeature f)
        {
            IMap pMap = GApplication.Application.ActiveView as IMap;
            pMap.ClearSelection();

            if (null == pLayer || null == f)
                return;

            pMap.SelectFeature(pLayer, f);

            if (esriGeometryType.esriGeometryPoint == f.Shape.GeometryType)
            {
                GApplication.Application.MapControl.CenterAt(f.Shape as IPoint);
                
            }
            else 
            {
                GApplication.Application.MapControl.ActiveView.Extent = f.Extent;
            }

            GApplication.Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, null);
        }

        /// <summary>
        /// 通过图层名返回图层
        /// </summary>
        /// <param name="map"></param>
        /// <param name="layerName"></param>
        /// <returns></returns>
        private ILayer getLayerByLayerName(IMap map, string layerName)
        {
            ILayer pLayer = null;

            try
            {
                var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IGeoFeatureLayer) && (l.Name == layerName);
                })).ToArray();
                if (lyrs != null && lyrs.Length > 0)
                    pLayer = lyrs[0];
                //for (int i = 0; i < map.LayerCount; ++i)
                //{
                //    if (layerName.ToUpper() == map.get_Layer(i).Name.ToUpper())
                //    {
                //        pLayer = map.get_Layer(i);

                //        break;
                //    }
                //}
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return null;
            }

            return pLayer;
        }

        private Dictionary<string, KeyValuePair<object, object>> getConflictPropertys(IFeature localFe, IFeature serverFe, List<string> fieldNameList = null)
        {
            Dictionary<string, KeyValuePair<object, object>> result = new Dictionary<string, KeyValuePair<object, object>>();
            if (fieldNameList == null || fieldNameList.Count == 0)//监控属性未设置
            {
                for (int j = 0; j < localFe.Fields.FieldCount; j++)
                {
                    IField field = localFe.Fields.get_Field(j);
                    if (!field.Editable || field.Type == esriFieldType.esriFieldTypeGeometry || field.Type == esriFieldType.esriFieldTypeOID)
                    {
                        continue;
                    }

                    if (field.Name.ToUpper() == ServerDataInitializeCommand.CollabVERSION || 
                        field.Name.ToUpper() == ServerDataInitializeCommand.CollabOPUSER)
                    {
                        continue;
                    }

                    int index = serverFe.Fields.FindField(field.Name);
                    if (index == -1)
                    {
                        continue;
                    }

                    object localVal = localFe.get_Value(j);
                    object serverVal = serverFe.get_Value(index);
                    if ((localVal == null && serverVal == null) || (localVal != null && serverVal != null && localVal.Equals(serverVal)))
                    {
                        continue;
                    }

                    result.Add(field.Name, new KeyValuePair<object, object>(localVal, serverVal));
                }
            }
            else
            {
                foreach(var fn in fieldNameList)
                {
                    int localIndex = localFe.Fields.FindField(fn);
                    int serverIndex = serverFe.Fields.FindField(fn);
                    if (localIndex == -1 || serverIndex == -1)
                    {
                        continue;
                    }

                    IField field = localFe.Fields.get_Field(localIndex);
                    if (!field.Editable || field.Type == esriFieldType.esriFieldTypeGeometry || field.Type == esriFieldType.esriFieldTypeOID)
                    {
                        continue;
                    }

                    if (field.Name.ToUpper() == ServerDataInitializeCommand.CollabVERSION ||
                        field.Name.ToUpper() == ServerDataInitializeCommand.CollabOPUSER)
                    {
                        continue;
                    }

                    object localVal = localFe.get_Value(localIndex);
                    object serverVal = serverFe.get_Value(serverIndex);
                    if ((localVal == null && serverVal == null) || (localVal != null && serverVal != null && localFe.Equals(serverVal)))
                    {
                        continue;
                    }

                    result.Add(field.Name, new KeyValuePair<object, object>(localVal, serverVal));
                }
            }

            return result;
        }


        #endregion
 
    }
}
