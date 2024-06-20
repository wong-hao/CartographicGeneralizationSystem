using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using SMGI.Common;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Controls;

namespace SMGI.Plugin.CollaborativeWork
{
    public partial class FrmDataDetection : Form
    {
        private GApplication _app;
        private int _localBaseVersion;
        private string _serverIPAddress;
        private string _sdeUsername;
        private string _sdePassword;
        private string _sdeDataBaseName;

        Dictionary<IFeature, IFeature> _conflictFeatures;

        public FrmDataDetection(GApplication app, string serverIPAddress, string sdeUsername, string sdePassword, string sdeDataBaseName, int localBaseVersion)
        {
            InitializeComponent();

            _app = app;
            _localBaseVersion = localBaseVersion;
            _serverIPAddress = serverIPAddress;
            _sdeUsername = sdeUsername;
            _sdePassword = sdePassword;
            _sdeDataBaseName = sdeDataBaseName;
        }

        private void FrmDataDetection_Load(object sender, EventArgs e)
        {
            var map = _app.ActiveView.FocusMap;
            var selection = map.FeatureSelection;


            IEnumFeature selectEnumFeature = (selection as MapSelection) as IEnumFeature;
            selectEnumFeature.Reset();
            IFeature selectFeature = null;

            _conflictFeatures = new Dictionary<IFeature, IFeature>();
            SDEDataServer ds = new SDEDataServer(_app, _serverIPAddress, _sdeUsername, _sdePassword, _sdeDataBaseName);
            while ((selectFeature = selectEnumFeature.Next()) != null)
            {
                var guid = selectFeature.get_Value(selectFeature.Fields.FindField(ServerDataInitializeCommand.CollabGUID)).ToString();
                IFeature serverConflictFeature = ds.getLatestFeature(selectFeature.Class.AliasName, guid, _localBaseVersion);

                if (serverConflictFeature != null)
                {
                    _conflictFeatures.Add(selectFeature, serverConflictFeature);
                }

            }

            var pWorkspaceEdit = _app.Workspace.EsriWorkspace as IWorkspaceEdit;
            if (pWorkspaceEdit.IsBeingEdited())
                pWorkspaceEdit.StartEditOperation();
            LocalDataBase db = new LocalDataBase(_app, _app.Workspace.EsriWorkspace);
            if (db.insertFeatures(_conflictFeatures.Values.ToList()))
            {
                upateTable(_conflictFeatures);
            }
            if (pWorkspaceEdit.IsBeingEdited())
                pWorkspaceEdit.StopEditOperation();

            _app.ActiveView.Refresh();
        }

        private void dgConflictResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridViewButtonCell btCell = dgConflictResult.Rows[e.RowIndex].Cells[e.ColumnIndex] as DataGridViewButtonCell;
            if (btCell != null)//缩放至要素
            {
                IFeatureLayer pLayer = null;
                IFeature f = null;
                IMap pMap = GApplication.Application.ActiveView as IMap;
                string guid = dgConflictResult.Rows[e.RowIndex].Cells["GUID"].Value.ToString();
                if ("LocalFeature" == dgConflictResult.Columns[e.ColumnIndex].Name)
                {
                    foreach (var item in _conflictFeatures.Keys)
                    {
                        if (item.get_Value(item.Fields.FindField(ServerDataInitializeCommand.CollabGUID)).ToString() == guid)
                        {
                            f = item;
                            break;
                        }
                    }

                    string layerName = dgConflictResult.Rows[e.RowIndex].Cells["localFeatureLN"].Value.ToString();

                    pLayer = getLayerByLayerName(pMap, layerName) as IFeatureLayer;
                }
                else if ("serverFeature" == dgConflictResult.Columns[e.ColumnIndex].Name)
                {
                    if (dgConflictResult.Rows[e.RowIndex].Cells["serverFeatureDelState"].Value.ToString() == "删除")
                    {
                        MessageBox.Show("该要素在服务器中已被删除！");
                    }
                    else
                    {
                        string layerName = dgConflictResult.Rows[e.RowIndex].Cells["serverFeatureLN"].Value.ToString();
                        pLayer = getLayerByLayerName(pMap, layerName) as IFeatureLayer;
                        if (pLayer != null)
                        {
                            string version = dgConflictResult.Rows[e.RowIndex].Cells["serverFeatureVer"].Value.ToString();

                            IQueryFilter qf = new QueryFilterClass();
                            qf.WhereClause = string.Format("{0} = '{1}' and {2} = {3}",
                            ServerDataInitializeCommand.CollabGUID, guid, ServerDataInitializeCommand.CollabVERSION, version);
                            IFeatureCursor fCursor = pLayer.Search(qf, false);
                            f = fCursor.NextFeature();

                            System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                        }
                        
                    }

                }

                ZoomToFeature(pLayer, f);
            }
        }

        /// <summary>
        /// 复制服务器要素的几何和属性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReplace_Click(object sender, EventArgs e)
        {
            if (_conflictFeatures == null || _conflictFeatures.Count == 0)
            {
                return;
            }

            var pWorkspaceEdit = _app.Workspace.EsriWorkspace as IWorkspaceEdit;
            if (pWorkspaceEdit.IsBeingEdited())
                pWorkspaceEdit.StartEditOperation();

            LocalDataBase db = new LocalDataBase(_app, _app.Workspace.EsriWorkspace);
            foreach (var item in _conflictFeatures)
            {
                IFeature localFeature = item.Key;
                IFeature serverFeature = item.Value;

                //复制服务器要素几何
                localFeature.Shape = serverFeature.ShapeCopy;
                //复制服务器要素属性
                for (int j = 0; j < serverFeature.Fields.FieldCount; j++)
                {
                    IField pfield = serverFeature.Fields.get_Field(j);
                    if (pfield.Type == esriFieldType.esriFieldTypeGeometry || pfield.Type == esriFieldType.esriFieldTypeOID)
                    {
                        continue;
                    }

                    if (pfield.Name.ToUpper() == "SHAPE_LENGTH" || pfield.Name.ToUpper() == "SHAPE_AREA")
                    {
                        continue;
                    }

                    if (pfield.Name.ToUpper() == ServerDataInitializeCommand.CollabGUID || 
                        pfield.Name.ToUpper() == ServerDataInitializeCommand.CollabVERSION ||
                        pfield.Name.ToUpper() == ServerDataInitializeCommand.CollabDELSTATE ||
                        pfield.Name.ToUpper() == ServerDataInitializeCommand.CollabOPUSER)
                    {
                        continue;
                    }

                    int index = localFeature.Fields.FindField(pfield.Name);
                    if (index != -1 && pfield.Editable)
                    {
                        localFeature.set_Value(index, serverFeature.get_Value(j));
                    }

                }
                localFeature.Store();
                
            }

            if (pWorkspaceEdit.IsBeingEdited())
                pWorkspaceEdit.StopEditOperation();

            _app.ActiveView.Refresh();

            this.Close();
        }

        private void FrmDataDetection_FormClosed(object sender, FormClosedEventArgs e)
        {
            var pWorkspaceEdit = _app.Workspace.EsriWorkspace as IWorkspaceEdit;
            if (pWorkspaceEdit.IsBeingEdited())
                pWorkspaceEdit.StartEditOperation();

            //删除协调数据
            LocalDataBase db = new LocalDataBase(_app, _app.Workspace.EsriWorkspace);
            db.deleteCollaborateFeatures(_localBaseVersion);

            if (pWorkspaceEdit.IsBeingEdited())
                pWorkspaceEdit.StopEditOperation();

            _app.ActiveView.Refresh();
        }


        /// <summary>
        /// 更新表格
        /// </summary>
        private void upateTable(Dictionary<IFeature, IFeature> conflictFeatures)
        {
            dgConflictResult.Rows.Clear();

            foreach (var kv in conflictFeatures)
            {
                int rowIndex = dgConflictResult.Rows.Add();

                IFeature localFeature = kv.Key;
                string localGUID = localFeature.get_Value(localFeature.Fields.FindField(ServerDataInitializeCommand.CollabGUID)).ToString();
                string LocalVer = localFeature.get_Value(localFeature.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString();
                string localDelState = localFeature.get_Value(localFeature.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE)).ToString();
                dgConflictResult.Rows[rowIndex].Cells["GUID"].Value = localGUID;
                dgConflictResult.Rows[rowIndex].Cells["localFeatureLN"].Value = localFeature.Class.AliasName;
                dgConflictResult.Rows[rowIndex].Cells["localFetureVer"].Value = LocalVer;
                if (localDelState == ServerDataInitializeCommand.DelStateText)
                {
                    dgConflictResult.Rows[rowIndex].Cells["localFetureDelState"].Value = "删除";
                }
                else
                {
                    dgConflictResult.Rows[rowIndex].Cells["localFetureDelState"].Value = "";
                }

                IFeature serverFeature = kv.Value;

                int serverVer = int.Parse(serverFeature.get_Value(serverFeature.Fields.FindField(ServerDataInitializeCommand.CollabVERSION)).ToString());
                string serverDelState = serverFeature.get_Value(serverFeature.Fields.FindField(ServerDataInitializeCommand.CollabDELSTATE)).ToString();
                string serverFeatureOpUser = serverFeature.get_Value(serverFeature.Fields.FindField(ServerDataInitializeCommand.CollabOPUSER)).ToString();

                dgConflictResult.Rows[rowIndex].Cells["ServerFeatureOPUser"].Value = serverFeatureOpUser;
                dgConflictResult.Rows[rowIndex].Cells["serverFeatureLN"].Value = serverFeature.Class.AliasName.Split('.').Last();
                dgConflictResult.Rows[rowIndex].Cells["serverFeatureVer"].Value = serverVer.ToString();
                if (serverDelState == ServerDataInitializeCommand.DelStateText)
                {
                    dgConflictResult.Rows[rowIndex].Cells["serverFeatureDelState"].Value = "删除";
                }
                else
                {
                    dgConflictResult.Rows[rowIndex].Cells["serverFeatureDelState"].Value = "";
                }
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
                for (int i = 0; i < map.LayerCount; ++i)
                {
                    if (layerName.ToUpper() == map.get_Layer(i).Name.ToUpper())
                    {
                        pLayer = map.get_Layer(i);

                        break;
                    }
                }
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

    }
}
