using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoDatabaseUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
namespace SMGI.Common
{
    public partial class TableForm : Form, ITableViewCallback, ITableViewContextMenus,ICalculatorCallback,ITableDataCallback,ITableDataCallback2
    {
        enum CellMoveAction
        {
            None,
            PanTo,
            ZoomTo
        }
        CellMoveAction cellMoveAction = CellMoveAction.None;
        TableViewClass allView, selectView;
        IFeatureLayer layer;
        GApplication app;
        public TableForm(GApplication app, IFeatureLayer layer)
        {
            this.layer = layer;           
            this.app = app;
            InitializeComponent();
            this.Text = layer.Name;            
        }

        #region ITableViewCallback
        void ITableViewCallback.AbortEditOperation()
        {
            app.EngineEditor.AbortOperation();
            System.Diagnostics.Trace.WriteLine("AbortEditOperation");
        }

        void ITableViewCallback.EnableEditUndoRedo(bool Enable)
        {
            System.Diagnostics.Trace.WriteLine("EnableEditUndoRedo:" + Enable);
        }

        void ITableViewCallback.RedrawFeatureLayer(IEnvelope pRedrawArea)
        {
            System.Diagnostics.Trace.WriteLine("RedrawFeatureLayer");
        }

        void ITableViewCallback.RefreshDisplay(IEnvelope pRedrawArea)
        {
            System.Diagnostics.Trace.WriteLine("RefreshDisplay");
        }

        void ITableViewCallback.RefreshSelection(IEnvelope pRedrawArea)
        {
            System.Diagnostics.Trace.WriteLine("RefreshSelection");
            if (!ShowSelected)
                app.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, pRedrawArea);
            else
            {
                app.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
            }
        }

        ICursor ITableViewCallback.Search(IQueryFilter pQueryFilter, bool recycling)
        {
            System.Diagnostics.Trace.WriteLine("Search");
           
            return layer.Search(pQueryFilter, recycling) as ICursor;
        }

        void ITableViewCallback.SelectionChange(ISelectionSet pSelection, bool newSelectionSet)
        {
            
            System.Diagnostics.Trace.WriteLine("SelectionChange");
            System.Diagnostics.Trace.WriteLine("newSelectionSet:" + newSelectionSet);
            
            if (!ShowSelected)
            {
                if (newSelectionSet)
                {
                    (layer as IFeatureSelection).SelectionSet = pSelection;
                    (layer as IFeatureSelection).SelectionChanged();
                    if (pSelection != null)
                    {
                        System.Diagnostics.Trace.WriteLine("newSelectionSetCount:" + pSelection.Count);
                        Helper.RefreshAttributeWindow(layer);
                    }
                    else
                    {
                        Helper.RefreshAttributeWindow(layer);
                    }
                }
                else
                {
                    var sset = (layer as IFeatureSelection).SelectionSet;
                    pSelection.Combine(sset, esriSetOperation.esriSetUnion, out sset);
                    (layer as IFeatureSelection).SelectionSet = sset;
                    (layer as IFeatureSelection).SelectionChanged();
                    if (pSelection != null)
                    {
                        System.Diagnostics.Trace.WriteLine("SelectionSetCount:" + pSelection.Count);
                        Helper.RefreshAttributeWindow(layer);
                    }
                    else
                    {
                        Helper.RefreshAttributeWindow(layer); 
                    }
            
                }
            }
        }

        void ITableViewCallback.ShowRelationshipTable(IRelationshipClass pRelationshipClass, bool showSource, ISelectionSet pOriginSelectionSet)
        {
            System.Diagnostics.Trace.WriteLine("ShowRelationshipTable");
        }

        ESRI.ArcGIS.Geometry.ISpatialReference ITableViewCallback.SpatialReference
        {
            get { return (layer.FeatureClass as IGeoDataset).SpatialReference; }
        }

        void ITableViewCallback.StartEditOperation()
        {
            app.EngineEditor.StartOperation();
            System.Diagnostics.Trace.WriteLine("StartEditOperation");
        }

        void ITableViewCallback.StopEditOperation(string operationName)
        {
            app.EngineEditor.StopOperation(operationName);
            System.Diagnostics.Trace.WriteLine("StopEditOperation:" + operationName);
        }
        #endregion

        #region ITableViewContextMenus
        bool ITableViewContextMenus.CellMenu()
        {
            System.Diagnostics.Trace.WriteLine("CellMenu");
            return true;
        }

        bool ITableViewContextMenus.ColumnMenu()
        {
            System.Diagnostics.Trace.WriteLine("ColumnMenu");

            ContextMenu menu = new ContextMenu();
            var it = new MenuItem("字段计算");
            it.Click += (it_Click);
            menu.MenuItems.Add(it);

            menu.Show(this.tablePanel, this.tablePanel.PointToClient(MousePosition));
            return true;
        }

        void it_Click(object sender, EventArgs e)
        {

            IEngineEditLayers editLayer = app.EngineEditor as IEngineEditLayers;
            if (!editLayer.IsEditable(layer))
            {
                MessageBox.Show("图层不可编辑，请先开启编辑！");
                return;
            }
            IField field = layer.FeatureClass.Fields.get_Field(CurrentView.GetCurrentCol());
            if (!field.Editable)
            {
                MessageBox.Show("字段不可编辑");
                return;
            }

            #region

            CalculatorUIClass ui = new CalculatorUIClass();
            ui.Field = layer.FeatureClass.Fields.get_Field(CurrentView.GetCurrentCol()).Name;
            ui.Table = layer.FeatureClass as ITable;
            ui.SelectionSet = CurrentView.SelectionSet;
            ui.Callback = this;
            ui.DoModal(this.Handle.ToInt32());

            #endregion

            //SingleInputValueForm f = new SingleInputValueForm();
            //if (f.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            //{
            //    return;
            //}

            try
            {
                app.EngineEditor.StartOperation();
                var cal = new CalculatorClass();
                ICursor cur = null;
                if (!ShowSelected)
                {
                    if (CurrentView.SelectionSet.Count > 0)
                        CurrentView.SelectionSet.Search(null, false, out cur);
                    else
                        cur = layer.Search(null, false) as ICursor;
                }
                else
                {
                    CurrentView.SelectionSet.Search(null, false, out cur);
                }
                
                cal.Cursor = cur;
                cal.Field = field.Name;
                cal.Expression = ui.Expression;
                cal.PreExpression = ui.PreExpression;
                cal.Callback = this;
                var env = cal.Calculate();
                app.EngineEditor.StopOperation("字段计算");
                allView.Redraw();
                selectView.Redraw();
                app.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, env);
            }
            catch
            {
                app.EngineEditor.AbortOperation();
            }
        }

        MenuItem CreateOptionMenuItem(string caption,esriTableViewOptions action)
        {
            return new MenuItem(caption,
                    (o, e) =>
                    {
                        if (action == esriTableViewOptions.esriTVOptionSelectAll)
                        {
                            CurrentView.ReadToEndOfTable();
                            CurrentView.ExecuteOptionCommand(action);
                            CurrentView.Redraw();
                        }
                        else if (action == esriTableViewOptions.esriTVOptionUnselectAll)
                        {

                            CurrentView.GetTopRow();
                            CurrentView.ExecuteOptionCommand(action);
                            CurrentView.Redraw();
                        }
                        else
                        {
                            CurrentView.ExecuteOptionCommand(action);
                            CurrentView.Redraw();
                        }
                    });
        }

        MenuItem CreateSelectToggleMenuItem()
        {

            return new MenuItem(this.ShowSelected ? "显示所有" : "显示选中",
                        (o, e) =>
                        {
                            this.ShowSelected = !this.ShowSelected;

                            FeatureLayerSelectionChanged();
                        });
        }

        MenuItem CreateCellMoveActionMenuItem()
        {
            MenuItem it = new MenuItem("选择动作");

            var itNone = new MenuItem ("无操作");
            var itPan = new MenuItem("平移至");
            var itZoom = new MenuItem("缩放至");
            Action<CellMoveAction> act = (action) =>
            {
                itNone.Checked = false;
                itZoom.Checked = false;
                itPan.Checked = false;
                switch (this.cellMoveAction)
                {
                    case CellMoveAction.None:
                        itNone.Checked = true;
                        break;
                    case CellMoveAction.PanTo:
                        itPan.Checked = true;
                        break;
                    case CellMoveAction.ZoomTo:
                        itZoom.Checked = true;
                        break;
                    default:
                        break;
                }
            };
            Func<CellMoveAction,EventHandler> eh_func = (a)=> (o, e) => { this.cellMoveAction = a; act(a); };
            itNone.Click += eh_func(CellMoveAction.None);
            itPan.Click += eh_func(CellMoveAction.PanTo);
            itZoom.Click += eh_func(CellMoveAction.ZoomTo);
            it.MenuItems.Add(itNone);
            it.MenuItems.Add(itZoom);
            it.MenuItems.Add(itPan);

            act(this.cellMoveAction);
            return it;
        }
        MenuItem CreateSelectByAttributeMenuItem()
        {
            return new MenuItem("根据属性选择...", (o, e) =>
            {
                SelectByAttributesDialog dlg = new SelectByAttributesDialog(app.ActiveView as IMap);
                dlg.OnlyOneLayer = true;
                dlg.LayerSelected = this.layer;
                dlg.ShowDialog(this);
            });
        }
        bool ITableViewContextMenus.OptionMenu()
        {
            System.Diagnostics.Trace.WriteLine("OptionMenu");
            ContextMenu menu = new System.Windows.Forms.ContextMenu();
            menu.MenuItems.Add(CreateSelectToggleMenuItem());
            menu.MenuItems.Add(CreateCellMoveActionMenuItem());
            menu.MenuItems.Add("-");
            foreach (esriTableViewOptions item in Enum.GetValues(typeof(esriTableViewOptions)))
            {
                string caption = Enum.GetName(typeof(esriTableViewOptions),item);
                caption = caption.Substring(12);
                
                switch (item)
                {
                    case esriTableViewOptions.esriTVOptionAliasNameToggle:
                        caption = "显示原/别名";
                        break;
                    case esriTableViewOptions.esriTVOptionAutoResizeColumns:
                        caption = "自动调整列宽";
                        break;
                    case esriTableViewOptions.esriTVOptionPrintTable:
                        caption = "打印...";
                        break;
                    case esriTableViewOptions.esriTVOptionSelectAll:
                        if (ShowSelected)
                            continue;
                        caption = "全选";
                        break;
                    case esriTableViewOptions.esriTVOptionShowAddFieldWindow:
                        if (CurrentView.IsEditing)
                            continue;
                        caption = "添加字段...";
                        break;
                    case esriTableViewOptions.esriTVOptionShowExportTableWindow:
                        caption = "导出...";
                        break;
                    case esriTableViewOptions.esriTVOptionShowFindReplaceWindow:
                        caption = "查找...";
                        break;
                    case esriTableViewOptions.esriTVOptionSwitchSelection:
                        if (ShowSelected)
                            continue;
                        caption = "反选";
                        break;
                    case esriTableViewOptions.esriTVOptionUnHideAllColumns:
                        caption = "显示所有列";
                        break;
                    case esriTableViewOptions.esriTVOptionUnselectAll:
                        if (ShowSelected)
                            continue;
                        caption = "清空";
                        break;
                    default:
                        continue;
                }
                menu.MenuItems.Add(CreateOptionMenuItem(caption, item));
            }

            menu.MenuItems.Add("-");
            menu.MenuItems.Add(CreateSelectByAttributeMenuItem());
            menu.Show(this.tablePanel, this.tablePanel.PointToClient(MousePosition));
            return true;
        }


     
        public bool RowMenu()
        {
            System.Diagnostics.Trace.WriteLine("RowMenu");
            return true;
        }
        #endregion

        bool ShowSelected
        {
            get { return selectPanel.Visible; }
            set
            {
                selectPanel.Visible = value;
                if (value)
                    selectPanel.Focus();
                else
                    tablePanel.Focus();
            }
        }

        #region 初始化及销毁
        Panel CurrentPanel
        {
            get
            {
                return ShowSelected ? tablePanel : selectPanel;
            }
        }
        TableViewClass CurrentView
        {
            get { return ShowSelected ? selectView : allView; }
        }
        TableViewClass CreateTableView(bool isSelectView)
        {
            var view = new TableViewClass();
            //设置过滤条件
            var fd = layer as ESRI.ArcGIS.Carto.IFeatureLayerDefinition;
            string finitionExpression = fd.DefinitionExpression;
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = finitionExpression;
            view.QueryFilter = qf;
            view.ShowSelected = isSelectView;
            
            if (isSelectView)
                view.TableSelectionAction = esriTableSelectionActions.esriDrawFeatures;
            else
                view.TableSelectionAction = esriTableSelectionActions.esriSelectFeatures;
            //view.SelectToggleAlwaysEnabled = true;
            view.HideOptionsButton = false;
            view.HideViewToggleButtons = true;
            view.Callback = this;
            view.Table = layer.FeatureClass as ITable;
            
            if(!isSelectView)
                view.SelectionSet = ((layer as IFeatureSelection).SelectionSet);
            else
                view.SelectionSet = (layer as ITable).Select(null, esriSelectionType.esriSelectionTypeIDSet, esriSelectionOption.esriSelectionOptionEmpty, (layer.FeatureClass as IDataset).Workspace);
            view.AllowAddRow = true;
            view.AllowDeleteRow = true;
            view.AllowEditing = true;
            view.OnSelectToggle += view_OnSelectToggle;
            view.OnCellMove += (view_OnCellMove);
            //this.ShowSelected = true;
            view.TableFields = layer as ITableFields;
            return view;
        }
        void DistroyTableView(TableViewClass view)
        {
            view.Table = null;
            view.SelectionSet = null;
            view.OnSelectToggle -= view_OnSelectToggle;
            System.Runtime.InteropServices.Marshal.ReleaseComObject(view);
        }

        private void TableForm_Load(object sender, EventArgs e)
        {
            allView = CreateTableView(false);
            tagRECT rect = new tagRECT
            {
                left = 0,
                top = 0,
                right = tablePanel.ClientSize.Width,
                bottom = tablePanel.ClientSize.Height
            };
            allView.Show(tablePanel.Handle.ToInt32(), ref rect, true);
            selectView = CreateTableView(true);
            rect = new tagRECT
            {
                left = 0,
                top = 0,
                right = selectPanel.ClientSize.Width,
                bottom = selectPanel.ClientSize.Height
            };
            selectView.Show(selectPanel.Handle.ToInt32(), ref rect, true);

            var layerEvent = layer as IFeatureLayerSelectionEvents_Event;
            layerEvent.FeatureLayerSelectionChanged += FeatureLayerSelectionChanged;
            var editorEvent = app.EngineEditor as IEngineEditEvents_Event;
            editorEvent.OnStartEditing += (editorEvent_OnStartEditing);
            editorEvent.OnStopEditing += (editorEvent_OnStopEditing);
            
            app.MapControl.OnAfterDraw += (MapControl_OnAfterDraw);

            //修改默认动作为平移至
            cellMoveAction = CellMoveAction.PanTo;
        }

        void MapControl_OnAfterDraw(object sender, IMapControlEvents2_OnAfterDrawEvent e)
        {
            if (!this.ShowSelected)
                return;

            if ((esriViewDrawPhase)e.viewDrawPhase != esriViewDrawPhase.esriViewForeground)
                return;

            IDisplay dis = e.display as IDisplay;

            var z = dis.DisplayTransformation.ReferenceScale;
            dis.DisplayTransformation.ReferenceScale = 0;
            CurrentView.DrawSelectedShapes(dis);
            dis.DisplayTransformation.ReferenceScale = z;
        }

        void editorEvent_OnStopEditing(bool saveChanges)
        {
            allView.EditChanged();
            allView.RemoveAndReloadCache();
            selectView.EditChanged();
            selectView.RemoveAndReloadCache();
        }

        void editorEvent_OnStartEditing()
        {
            //allView. = true;
            allView.ITableControl3_EditChanged();
            selectView.EditChanged();
        }

        private void TableForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            var layerEvent = layer as IFeatureLayerSelectionEvents_Event;
            layerEvent.FeatureLayerSelectionChanged -= FeatureLayerSelectionChanged;

            app.MapControl.OnAfterDraw -= MapControl_OnAfterDraw;

            var editorEvent = app.EngineEditor as IEngineEditEvents_Event;
            editorEvent.OnStartEditing -= (editorEvent_OnStartEditing);
            editorEvent.OnStopEditing -= (editorEvent_OnStopEditing);

            DistroyTableView(allView);
            DistroyTableView(selectView);
            this.Dispose();
        }

        void DelayFlashGeometry(IGeometry geo)
        {
            Timer t = new Timer { Interval = 100 };
            t.Tick += (o, e) => {
                t.Stop();
                app.MapControl.FlashShape(geo);
                t.Dispose();
            };
            t.Start();
        }

        void view_OnCellMove(IRow pRow, int fieldPos)
        {
           
            switch (this.cellMoveAction)
            {
                case CellMoveAction.None:
                    break;
                case CellMoveAction.PanTo:
                    {
                        IFeature fe = layer.FeatureClass.GetFeature(pRow.OID);
                        if (fe.Shape == null || fe.Shape.IsEmpty)
                        {
                            MessageBox.Show("要素几何为空！");
                            break;
                        }

                        IPoint cp = new PointClass
                        {
                            X = (fe.Extent.XMin + fe.Extent.XMax) / 2,
                            Y = (fe.Extent.YMin + fe.Extent.YMax) / 2
                        };
                        if (fe.Shape.SpatialReference != null && app.MapControl.ActiveView.FocusMap.SpatialReference != null
                            && fe.Shape.SpatialReference.Name != app.MapControl.ActiveView.FocusMap.SpatialReference.Name)
                        {
                            cp.Project(app.MapControl.ActiveView.FocusMap.SpatialReference);//投影变换
                        }

                        var extent = app.MapControl.Extent;
                        extent.CenterAt(cp);
                        app.MapControl.Extent = extent;
                        DelayFlashGeometry(fe.Shape);
                    }
                    break;
                case CellMoveAction.ZoomTo:
                    {
                        IFeature fe = layer.FeatureClass.GetFeature(pRow.OID);
                        if (fe.Shape == null || fe.Shape.IsEmpty)
                        {
                            MessageBox.Show("要素几何为空！");
                            break;
                        }

                        var env = fe.Shape.Envelope;
                        if (fe.Shape.SpatialReference != null && app.MapControl.ActiveView.FocusMap.SpatialReference != null 
                            && fe.Shape.SpatialReference.Name != app.MapControl.ActiveView.FocusMap.SpatialReference.Name)
                        {
                            env.Project(app.MapControl.ActiveView.FocusMap.SpatialReference);//投影变换
                        }

                        if ((env as IArea).Area > 1.0)
                        {
                            env.Expand(2, 2, true);
                        }
                        else
                        {
                            var envelope = layer.AreaOfInterest;
                            env.Width = envelope.Width * 0.05;
                            env.Height = envelope.Height * 0.05;
                            if (env.Width < 1e-6)
                            {
                                env.Height = app.MapControl.ActiveView.Extent.Height * 0.05;
                                env.Width = app.MapControl.ActiveView.Extent.Width * 0.05;

                            }
                            env.CenterAt((fe.Shape.Envelope as IArea).Centroid);
                        }

                        app.MapControl.Extent = env;
                        DelayFlashGeometry(fe.Shape);
                    }
                    break;
                default:
                    break;
            }
            
            System.Diagnostics.Trace.WriteLine("view_OnCellMove");
        }

        void view_OnSelectToggle()
        {
            //view.ShowSelected = true;
            System.Diagnostics.Trace.WriteLine("view_OnSelectToggle");
        }

        private void OnResize(object sender, EventArgs e)
        {
            if(selectView != null)
                selectView.SetPosition(0, 0, tablePanel.ClientSize.Width, tablePanel.ClientSize.Height);
            if(allView != null)
                allView.SetPosition(0, 0, tablePanel.ClientSize.Width, tablePanel.ClientSize.Height);
        }

        void FeatureLayerSelectionChanged()
        {
            if (this.ShowSelected)
            {
                CurrentView.RereadFIDs((layer as IFeatureSelection).SelectionSet);
            }
            else
            {
                CurrentView.UpdateSelection((layer as IFeatureSelection).SelectionSet);
            }
            //CurrentView.DrawSelectedShapes(app.ActiveView.ScreenDisplay);
            CurrentView.Redraw();
        }
        #endregion

        
        #region ICalculatorCallback
        bool ICalculatorCallback.CalculatorError(int rowID, bool bHasOID, esriCalculatorErrorType errorType, bool bShowPrompt, string errorMsg)
        {
            System.Diagnostics.Trace.WriteLine(
                string.Format("oid:{0},type:{1},msg:{2}",rowID,errorType,errorMsg));
            return false;
        }

        void ICalculatorCallback.CalculatorWarning(int rowID, bool bHasOID, esriCalculatorErrorType errorType, string errorMsg)
        {
            System.Diagnostics.Trace.WriteLine(
                string.Format("oid:{0},type:{1},msg:{2}", rowID, errorType, errorMsg));
        }

        bool ICalculatorCallback.Status(int rowsWritten, bool lastStatus)
        {
            return false;
        }
        #endregion

        #region 没弄透
        public ICursor TableSearch(IQueryFilter pQueryFilter, bool recycling)
        {
            return layer.Search(pQueryFilter, recycling) as ICursor;
        }

        public ICursor TableUpdate(IQueryFilter pQueryFilter, bool recycling)
        {
            return layer.FeatureClass.Update(pQueryFilter, recycling) as ICursor;
        }        
        #endregion
    }
}
