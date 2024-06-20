using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
//****************************
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;

using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using System.Linq;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class AttributeTableForm : Form
    {
        GApplication _app;
        //private varibles
        private TableWrapper tableWrapper;
        private TableWrapper selectedTableWrapper;
        //private IMap map;
        private string filterString;
        private IFeatureLayer featureLayer;
        private IFeatureLayerSelectionEvents_Event featureLayerSelectionEvent;
        private IActiveView activeView;
        private IActiveViewEvents_Event activeViewEvents_Event;
        private string OIDString;
        //挂起源数据，防止dataGridView的行数为0时产生错误
        private CurrencyManager cm;
        //
        private Hashtable hashTable;
        /*******************************/
        //标识按下的按钮，显示所有记录或只显示选中要素的记录
        private bool IsShowingAll = true;
        //选中要素的ID
        private List<int> SelectedFeaturesIDs;
        //dataGridView显示的位置
        private int FirstDisplayerRowIndex = 0;
        //是否升序排列
        private bool IsAscending = false;

        private string _selStateFN;

        public AttributeTableForm(GApplication app,IMap Map, ILayer Layer, string FilterString, string selStateFN)
        {
            InitializeComponent();
            filterString = FilterString;
            featureLayer = Layer as IFeatureLayer;
            featureLayerSelectionEvent = featureLayer as IFeatureLayerSelectionEvents_Event;
            featureLayerSelectionEvent.FeatureLayerSelectionChanged += (featureLayerSelectionEvent_FeatureLayerSelectionChanged);
            activeView = Map as IActiveView;
            activeViewEvents_Event = activeView as IActiveViewEvents_Event;
            activeViewEvents_Event.AfterDraw += new IActiveViewEvents_AfterDrawEventHandler(activeViewEvents_Event_AfterDraw);
            OIDString = featureLayer.FeatureClass.OIDFieldName;

            _app = app;
            _selStateFN = selStateFN;

            this.Text = string.Format("{0}[未选取要素]", Layer.Name);
        }

       
        //改变选择的记录
        void featureLayerSelectionEvent_FeatureLayerSelectionChanged()
        {
            dataGridView.SelectionChanged -= dataGridView_SelectionChanged;
            //Label选中要素个数
            this.selectedFeaturesCount.Text = (featureLayer as IFeatureSelection).SelectionSet.Count.ToString() + "个要素选中";
            
            if (IsShowingAll)
            {
                //选中dataGridView中相应行
                selectRows();
            }
            else
            {
                selectedTableWrapper = new TableWrapper(featureLayer.FeatureClass as ITable, filterString, false, ref hashTable);
                bindingSource1.DataSource = selectedTableWrapper;
                dataGridView.DataSource = bindingSource1;
                SuspendBingToDataGridview(selectedTableWrapper.Count, cm);
            }
            activeView.PartialRefresh(esriViewDrawPhase.esriViewForeground, featureLayer, activeView.Extent);
            dataGridView.SelectionChanged += (dataGridView_SelectionChanged);
        }


        //绘制前景色
        void activeViewEvents_Event_AfterDraw(IDisplay Display, esriViewDrawPhase phase)
        {
            if (!IsShowingAll && dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected) > 0 && phase == esriViewDrawPhase.esriViewForeground)
            {
                IFeatureClass featureClass = featureLayer.FeatureClass;
                IRgbColor drawColor = new RgbColorClass();
                drawColor.Red = 255;
                drawColor.Green = 255;
                drawColor.Blue = 0;
                //判断featureClass的类型
                if (featureClass.ShapeType == esriGeometryType.esriGeometryPolygon)
                {
                    IFillSymbol fillSymbol = new SimpleFillSymbolClass();
                    fillSymbol.Color = drawColor as IColor;
                    activeView.ScreenDisplay.SetSymbol(fillSymbol as ISymbol);
                    foreach (int OID in SelectedFeaturesIDs)
                    {
                        activeView.ScreenDisplay.DrawPolygon(featureClass.GetFeature(OID).ShapeCopy);
                    }
                }
                else if (featureClass.ShapeType == esriGeometryType.esriGeometryPolyline)
                {
                    ILineSymbol lineFillSymbol = new SimpleLineSymbolClass();
                    lineFillSymbol.Color = drawColor as IColor;
                    lineFillSymbol.Width = 3;
                    activeView.ScreenDisplay.SetSymbol(lineFillSymbol as ISymbol);
                    foreach (int OID in SelectedFeaturesIDs)
                    {
                        activeView.ScreenDisplay.DrawPolyline(featureClass.GetFeature(OID).ShapeCopy);
                    }
                }
                else if (featureClass.ShapeType == esriGeometryType.esriGeometryPoint)
                {
                    IMarkerSymbol markerFillSymbol = new SimpleMarkerSymbol();
                    markerFillSymbol.Color = drawColor as IColor;
                    activeView.ScreenDisplay.SetSymbol(markerFillSymbol as ISymbol);
                    foreach (int OID in SelectedFeaturesIDs)
                    {
                        activeView.ScreenDisplay.DrawPoint(featureClass.GetFeature(OID).ShapeCopy);
                    }
                }
            }
        }

        //当dataGridView选择行更改时，更新SelectedFeaturesIDs
        //该过程用于显示选中要素之后，高亮再次选中的要素
        void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            
            featureLayerSelectionEvent.FeatureLayerSelectionChanged -= featureLayerSelectionEvent_FeatureLayerSelectionChanged;

            if (IsShowingAll)
            {
                this.selectedFeaturesCount.Text = dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected).ToString() + "个要素选中";
                IFeatureSelection featureSelection = featureLayer as IFeatureSelection;
                //featureSelection.Clear();
                featureSelection.CombinationMethod = esriSelectionResultEnum.esriSelectionResultNew;
                for (int i = 0; i < dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected); i++)
                {
                    int OID = Convert.ToInt32(dataGridView.SelectedRows[i].Cells[OIDString].Value);
                    //featureSelection.Add(featureLayer.FeatureClass.GetFeature(OID));
                    (activeView as IMap).SelectFeature(featureLayer, featureLayer.FeatureClass.GetFeature(OID));
                    //SelectedFeaturesIDs.Add();
                }
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, featureLayer, activeView.Extent);
            }
            else
            {
                this.selectedFeaturesCount.Text = dataGridView.Rows.Count.ToString() + "个要素选中";
                SelectedFeaturesIDs = new List<int>();
                for (int i = 0; i < dataGridView.Rows.GetRowCount(DataGridViewElementStates.Selected); i++)
                {
                    int OID = Convert.ToInt32(dataGridView.SelectedRows[i].Cells[OIDString].Value);
                    SelectedFeaturesIDs.Add(OID);
                }
                activeView.PartialRefresh(esriViewDrawPhase.esriViewForeground, featureLayer, activeView.Extent);
            }
            featureLayerSelectionEvent.FeatureLayerSelectionChanged += (featureLayerSelectionEvent_FeatureLayerSelectionChanged);

            btnSelectFeature.Enabled = (dataGridView.SelectedRows.Count > 0);
        }


        //***************************************************************
        //*******************Private Events******************************
        private void AttributeTableForm_Load(object sender, EventArgs e)
        {
            if (featureLayer != null)
            {
                // Bind dataset to the binding source
                tableWrapper = new TableWrapper(featureLayer.FeatureClass as ITable, filterString, true, ref hashTable);
                bindingSource1.DataSource = tableWrapper;
                dataGridView.DataSource = bindingSource1;

                
                
                IQueryFilter qf = new QueryFilterClass(); 
                qf.WhereClause = filterString;
                this.totalFeaturesCount.Text = (featureLayer.FeatureClass as ITable).RowCount(qf).ToString() + "个要素中有";
                this.selectedFeaturesCount.Text = (featureLayer as IFeatureSelection).SelectionSet.Count.ToString() + "个要素选中";
                
                //当记录条数为0时，挂起绑定，否则重新绑定
                cm = BindingContext[dataGridView.DataSource] as CurrencyManager;
                SuspendBingToDataGridview(tableWrapper.Count, cm);
                //选中相应行
                //dataGridView.MultiSelectChanged
                selectRows();
                bringSpecifiedRowsToFront();

                //注册SelectionChanged事件
                dataGridView.SelectionChanged += (dataGridView_SelectionChanged);

                //txtBoxBinding = new Binding("Text", bindingSource1, "FULL_NAME");
                //textBox1.DataBindings.Add(txtBoxBinding);
                //for (int i = 0; i < dataGridView.Columns.Count; i++)
                //{ dataGridView.Columns[i].SortMode = DataGridViewColumnSortMode.Automatic; }

                btnSelectFeature.Enabled = (dataGridView.SelectedRows.Count > 0);
            }
        }

        //定位到选中要素
        private void dataGridView_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            
            int OID = Convert.ToInt32(dataGridView.Rows[e.RowIndex].Cells[OIDString].Value);
            IFeature feature = featureLayer.FeatureClass.GetFeature(OID);

            IGeometry shape = feature.Shape;
            if (feature.Shape.SpatialReference != null && activeView.FocusMap.SpatialReference != null && feature.Shape.SpatialReference.Name != activeView.FocusMap.SpatialReference.Name)
            {
                shape.Project(activeView.FocusMap.SpatialReference);//投影变换
            }
            var env = shape.Envelope;

            if ((env as IArea).Area < 1)//modified by YJ 支持所有几何定位
            {
                var envelope = featureLayer.AreaOfInterest;
                env.Width = envelope.Width*0.05;
                env.Height = envelope.Height*0.05;
                if (env.Width < 1e-6)
                {
                    env.Height = activeView.Extent.Height * 0.05; 
                    env.Width = activeView.Extent.Width * 0.05;
                   
                }
                env.CenterAt((feature.Shape.Envelope as IArea).Centroid);
            }
            else
            {
                env.Expand(2,2,true);
            }

            GApplication.Application.MapControl.Extent=env;
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography, featureLayer, env);
        }


        private void AttributeTableForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //activeViewEvents_Event.AfterDraw -= activeViewEvents_Event_AfterDraw;
            activeView.PartialRefresh(esriViewDrawPhase.esriViewForeground, featureLayer, activeView.Extent);
        }

        //***************************************************************
        //*******************Private Method******************************
        /// <summary>
        /// 挂起绑定
        /// </summary>
        /// <param name="count"></param>
        /// <param name="m_CM"></param>
        private void SuspendBingToDataGridview(int count, CurrencyManager m_CM)
        {
            if (count == 0)
            {
                m_CM.SuspendBinding();
            }
        }

        private void selectRows()
        {
            dataGridView.ClearSelection();
            //List<int> selectedOID = new List<int>();
            ISelectionSet selectionSet = (featureLayer as IFeatureSelection).SelectionSet;
            IEnumIDs enumIDs = selectionSet.IDs;
            int ID;
            if (dataGridView.RowCount != hashTable.Count)
            {
                return;
            } 
            while ((ID = enumIDs.Next()) != -1)
            {
                if (hashTable.ContainsKey(ID))
                {
                    dataGridView.Rows[Convert.ToInt32(hashTable[ID])].Selected = true;
                }
                //selectedOID.Add(ID);
            }
            //foreach (DataGridViewRow row in m_DataGridView.Rows)
            //{
            //    if (selectedOID.Contains(Convert.ToInt32(row.Cells[OIDString].Value)))
            //    {
            //        row.Selected = true;
            //    }
            //    else
            //    {
            //        row.Selected = false;
            //    }
            //}
        }

        /// <summary>
        /// 双击选中定位到该点
        /// </summary>
        /// <param name="ActiveView">先前的视窗</param>
        /// <param name="Point">选中的点</param>
        /// <param name="layer">选中的点</param>
        /// <returns></returns>
        private IEnvelope locateToSelectedPoint(IActiveView ActiveView, IFeature Point, IFeatureLayer layer)
        {
            IEnvelope envelope = layer.AreaOfInterest;
            envelope.Width = envelope.Width * 0.05;
            envelope.Height = envelope.Height * 0.05;
            if(envelope.Width<1e-6)
            {
                envelope.Width = ActiveView.Extent.Width;
            }
            envelope.CenterAt(Point.Shape as IPoint);
            return envelope;
        }

        /// <summary>
        /// 双击选中定位到该点
        /// </summary>
        /// <param name="ActiveView">先前的视窗</param>
        /// <param name="Point">选中的点</param>
        /// <returns></returns>
        private IEnvelope locateToSelectedPointEx(IActiveView ActiveView, IFeature line, IFeatureLayer layer)
        {
            IEnvelope envelope = layer.AreaOfInterest;
            envelope.Width = envelope.Width * 0.05;
            envelope.Height = envelope.Height * 0.05;
            envelope.CenterAt((line.Shape as IPointCollection).get_Point(0) as IPoint);
            return envelope;
        }

        /// <summary>
        /// 将重要列靠前显示
        /// </summary>
        private void bringSpecifiedRowsToFront()
        {
            string[] specialColumns = new string[7] { OIDString, "GB", "CLASS", "NAME", "TEXTSTRING", "RULEID", _selStateFN };
            int j = 0;
            for (int i = 0; i < specialColumns.Length; i++)
            {
                if (dataGridView.Columns.Contains(specialColumns[i].ToUpper()))
                {
                    foreach (DataGridViewColumn column in dataGridView.Columns)
                    {
                        if (column.Name.ToUpper() == specialColumns[i].ToUpper())
                        {
                            column.DisplayIndex = j;
                            j++;
                        }
                    }
                }
            }
        }

        private void btnSelectFeature_Click(object sender, EventArgs e)
        {
            IEngineEditLayers editLayer = _app.EngineEditor as IEngineEditLayers;
            if (!editLayer.IsEditable(featureLayer))
            {
                MessageBox.Show("图层不可编辑，请先开启编辑！");
                return;
            }

            _app.EngineEditor.StartOperation();
            try
            {
                List<int> oidList = new List<int>();
                for (int i = 0; i < dataGridView.SelectedRows.Count; i++)
                {
                    int OID = Convert.ToInt32(dataGridView.SelectedRows[i].Cells[OIDString].Value);
                    IFeature feature = featureLayer.FeatureClass.GetFeature(OID);
                    int index;
                    if (feature != null && (index = feature.Fields.FindField(_selStateFN)) != -1)
                    {
                        feature.set_Value(index, DBNull.Value);
                        feature.Store();

                        oidList.Add(OID);
                    }
                }

                if (!(featureLayer is IFDOGraphicsLayer) && oidList.Count > 0)
                {
                    selectAnnoByConnFe(featureLayer.FeatureClass, oidList, _selStateFN);
                }
                
                this.Refresh();
                tableWrapper = new TableWrapper(featureLayer.FeatureClass as ITable, filterString, true, ref hashTable);
                bindingSource1.DataSource = tableWrapper;
                dataGridView.DataSource = bindingSource1;

                dataGridView.ClearSelection();
                this.Cursor = Cursors.Default;
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGeography, featureLayer, activeView.Extent);

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = filterString;
                this.totalFeaturesCount.Text = (featureLayer.FeatureClass as ITable).RowCount(qf).ToString() + "个要素中有";
                this.selectedFeaturesCount.Text = (featureLayer as IFeatureSelection).SelectionSet.Count.ToString() + "个要素选中";

                _app.EngineEditor.StopOperation("要素恢复");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);

                _app.EngineEditor.AbortOperation();
            }
            
        }


        private void selectAnnoByConnFe(IFeatureClass fc, List<int> selectOIDList, string selStateFN)
        {
            var annoLayers = GApplication.Application.Workspace.LayerManager.GetLayer(l => l is IFDOGraphicsLayer).ToArray();
            for (int i = 0; i < annoLayers.Length; i++)
            {
                IFeatureClass annoFC = (annoLayers[i] as IFeatureLayer).FeatureClass;

                int selStateIndex = annoFC.FindField(selStateFN);
                if (selStateIndex == -1)
                {
                    continue;
                }
                string annoClassIDFN = GApplication.Application.TemplateManager.getFieldAliasName("AnnotationClassID", annoFC.AliasName);
                int annoClassIDIndex = annoFC.FindField(annoClassIDFN);
                if (annoClassIDIndex == -1)
                {
                    continue;
                }

                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = string.Format("{0} = {1} and {2} is not null", annoClassIDFN, fc.ObjectClassID, selStateFN);
                IFeatureCursor fCursor = annoFC.Update(qf, true);
                IFeature f = null;
                while ((f = fCursor.NextFeature()) != null)
                {
                    IAnnotationFeature2 annoFe = f as IAnnotationFeature2;
                    if (selectOIDList.Contains(annoFe.LinkedFeatureID))
                    {
                        f.set_Value(selStateIndex, DBNull.Value);
                        fCursor.UpdateFeature(f);
                    }
                }
                Marshal.ReleaseComObject(fCursor);

            }
        }

    }
}
