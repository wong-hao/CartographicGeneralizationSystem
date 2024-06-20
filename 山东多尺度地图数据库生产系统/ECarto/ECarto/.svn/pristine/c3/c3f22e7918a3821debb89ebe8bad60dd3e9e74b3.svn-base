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

namespace SMGI.Common.AttributeTable
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

        public AttributeTableForm(GApplication app,IMap Map, ILayer Layer, string FilterString)
        {
            InitializeComponent();
            if (Layer.Name != "XZQ" && Layer.Name != "XZQJX")
            {
                filterString = FilterString;
            }
            else
            {
                filterString = "";
            }
            featureLayer = Layer as IFeatureLayer;
            featureLayerSelectionEvent = featureLayer as IFeatureLayerSelectionEvents_Event;
            featureLayerSelectionEvent.FeatureLayerSelectionChanged += (featureLayerSelectionEvent_FeatureLayerSelectionChanged);
            activeView = Map as IActiveView;
            activeViewEvents_Event = activeView as IActiveViewEvents_Event;
            activeViewEvents_Event.AfterDraw += new IActiveViewEvents_AfterDrawEventHandler(activeViewEvents_Event_AfterDraw);
            OIDString = featureLayer.FeatureClass.OIDFieldName;

            _app = app;
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
                selectedTableWrapper = new TableWrapper(featureLayer as ITable, filterString, false, ref hashTable);
                bindingSource1.DataSource = selectedTableWrapper;
                dataGridView.DataSource = bindingSource1;
                SuspendBingToDataGridview(selectedTableWrapper.Count, cm);
            }
            activeView.PartialRefresh(esriViewDrawPhase.esriViewForeground, featureLayer, activeView.Extent);
            dataGridView.SelectionChanged += (dataGridView_SelectionChanged);
        }

        //当可是区域发生变化时
        void DefinitionExpressionEvent_DefinitionExpressionChanged(object pSource)
        {
            //注销事件
            dataGridView.SelectionChanged -= dataGridView_SelectionChanged;

            filterString = (featureLayer as IFeatureLayerDefinition).DefinitionExpression;
            this.totalFeaturesCount.Text = (featureLayer as ITable).RowCount(null).ToString() + "个要素中有";
            this.selectedFeaturesCount.Text = (featureLayer as IFeatureSelection).SelectionSet.Count.ToString() + "个要素选中";
            if (IsShowingAll)
            {
                tableWrapper = new TableWrapper(featureLayer as ITable, filterString, true, ref hashTable);
                bindingSource1.DataSource = tableWrapper;
                dataGridView.DataSource = bindingSource1;
                selectRows();
                bringSpecifiedRowsToFront();
            }
            else
            {
                selectedTableWrapper = new TableWrapper(featureLayer as ITable, filterString, false, ref hashTable);
                bindingSource1.DataSource = selectedTableWrapper;
                dataGridView.DataSource = bindingSource1;
                SuspendBingToDataGridview(selectedTableWrapper.Count, cm);
            }
            //重新注册
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
        }


        //***************************************************************
        //*******************Private Events******************************
        private void AttributeTableForm_Load(object sender, EventArgs e)
        {
            this.Text = featureLayer.Name;
            if (featureLayer != null)
            {
                // Bind dataset to the binding source
                tableWrapper = new TableWrapper(featureLayer as ITable, filterString, true, ref hashTable);
                bindingSource1.DataSource = tableWrapper;
                dataGridView.DataSource = bindingSource1;

                
                
                if ((featureLayer as IFeatureLayerDefinition).DefinitionExpression != "")
                {
                    //注册DefinitionExpression事件
                    IDefinitionExpressionEvents_Event DefinitionExpressionEvent = featureLayer as IDefinitionExpressionEvents_Event;
                    DefinitionExpressionEvent.DefinitionExpressionChanged += new IDefinitionExpressionEvents_DefinitionExpressionChangedEventHandler(DefinitionExpressionEvent_DefinitionExpressionChanged);
                }

                this.totalFeaturesCount.Text = (featureLayer as ITable).RowCount(null).ToString() + "个要素中有";
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

            }
        }
      

        private void buttonAll_Click(object sender, EventArgs e)
        {
            //光标样式
            this.Cursor = Cursors.WaitCursor;
            //基本参数设定
            IsShowingAll = true;
            buttonAll.FlatStyle = FlatStyle.Flat;
            buttonSelected.FlatStyle = FlatStyle.Standard;
            dataGridView.RowsDefaultCellStyle.BackColor = Color.FromKnownColor(KnownColor.Window);
            dataGridView.RowsDefaultCellStyle.SelectionBackColor = Color.Cyan;
            //注销事件，防止单击后清除已选择的要素
            dataGridView.SelectionChanged -= dataGridView_SelectionChanged;
            bindingSource1.DataSource = tableWrapper;
            dataGridView.DataSource = bindingSource1;
            //选中相应行
            selectRows();
            bringSpecifiedRowsToFront();
            //行显示位置
            if (tableWrapper.Count > 0)
            {
                dataGridView.FirstDisplayedScrollingRowIndex = FirstDisplayerRowIndex;
            }
            dataGridView.SelectionChanged += (dataGridView_SelectionChanged);
            this.Cursor = Cursors.Default;
        }

        private void buttonSelected_Click(object sender, EventArgs e)
        {
            //光标样式
            this.Cursor = Cursors.WaitCursor;
            //记录显示“所有”要素时dataGridView所在的RowIndex
            FirstDisplayerRowIndex = dataGridView.FirstDisplayedScrollingRowIndex;
            //基本参数设定
            IsShowingAll = false;
            buttonAll.FlatStyle = FlatStyle.Standard;
            buttonSelected.FlatStyle = FlatStyle.Flat;
            dataGridView.RowsDefaultCellStyle.BackColor = Color.Cyan;
            dataGridView.RowsDefaultCellStyle.SelectionBackColor = Color.Yellow;

            selectedTableWrapper = new TableWrapper(featureLayer as ITable, filterString, false, ref hashTable);
            bindingSource1.DataSource = selectedTableWrapper;
            dataGridView.DataSource = bindingSource1;

            bringSpecifiedRowsToFront();
            SuspendBingToDataGridview(selectedTableWrapper.Count, cm);
            
            this.Cursor = Cursors.Default;
        }

        private void buttonShowSmallPolygon_Click(object sender, EventArgs e)
        {
            if (featureLayer.Name == "DLTB")
            {
                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.WhereClause = "LandGenXTBFlag = 1";
                IFeatureSelection featureSelection = featureLayer as IFeatureSelection;
                featureSelection.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
                activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, featureLayer, activeView.Extent);
            }
        }

        //如果全选
        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }
      

        //定位到选中要素
        private void dataGridView_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            
            int OID = Convert.ToInt32(dataGridView.Rows[e.RowIndex].Cells[OIDString].Value);
            IFeature feature = featureLayer.FeatureClass.GetFeature(OID);

            IGeometry shape = feature.Shape;
            if (feature.Shape.SpatialReference.Name != activeView.FocusMap.SpatialReference.Name)
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
            string[] specialColumns = new string[9] { OIDString, "DLBM", "DLMC", "QSXZ", "QSDWDM", "QSDWMC", "ZLDWDM", "ZLDWMC", "LandGenXTBFlag" };
            int j = 0;
            for (int i = 0; i < 9; i++)
            {
                if (dataGridView.Columns.Contains(specialColumns[i]))
                {
                    foreach (DataGridViewColumn column in dataGridView.Columns)
                    {
                        if (column.Name == specialColumns[i])
                        {
                            column.DisplayIndex = j;
                            j++;
                        }
                    }
                }
            }
        }

        private void btSelectByAtt_Click(object sender, EventArgs e)
        {
            SelectByAttributesDialog dlg = new SelectByAttributesDialog(activeView as IMap);
            dlg.OnlyOneLayer = true;
            dlg.LayerSelected = this.featureLayer;
            dlg.ShowDialog(this);            
        }

        private void btnAttributeModify_Click(object sender, EventArgs e)
        {
            IEngineEditLayers editLayer = _app.EngineEditor as IEngineEditLayers;
            if (!editLayer.IsEditable(featureLayer))
            {
                MessageBox.Show("图层不可编辑，请先开启编辑！");
                return;
            }

            AttributeProcessForm attr = new AttributeProcessForm(_app,dataGridView, activeView.FocusMap, featureLayer);
            attr.ShowDialog();

            //光标样式
            this.Cursor = Cursors.WaitCursor;
            //记录显示“所有”要素时dataGridView所在的RowIndex
            FirstDisplayerRowIndex = dataGridView.FirstDisplayedScrollingRowIndex;
            //基本参数设定
            IsShowingAll = false;
            buttonAll.FlatStyle = FlatStyle.Standard;
            buttonSelected.FlatStyle = FlatStyle.Flat;
            dataGridView.RowsDefaultCellStyle.BackColor = Color.Cyan;
            dataGridView.RowsDefaultCellStyle.SelectionBackColor = Color.Yellow;

            selectedTableWrapper = new TableWrapper(featureLayer as ITable, filterString, false, ref hashTable);
            bindingSource1.DataSource = selectedTableWrapper;
            dataGridView.DataSource = bindingSource1;

            bringSpecifiedRowsToFront();
            SuspendBingToDataGridview(selectedTableWrapper.Count, cm);

            this.Cursor = Cursors.Default;
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, featureLayer, activeView.Extent);

        }

    }
}
