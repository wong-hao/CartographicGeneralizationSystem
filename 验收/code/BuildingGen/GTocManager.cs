using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;

namespace BuildingGen
{
    public class GTOCManager
    {
        GApplication app;
        ContextMenuStrip menu;
        ToolStripMenuItem select;
        public GTOCManager(GApplication app)
        {
            this.app = app;
            app.TocControl.OnMouseDown += new ITOCControlEvents_Ax_OnMouseDownEventHandler(TocControl_OnMouseDown);
            menu = new ContextMenuStrip();
            menu.Opening += new System.ComponentModel.CancelEventHandler(menu_Opening);
            select = new ToolStripMenuItem("可选");
            select.CheckOnClick = true;
            menu.Items.Add(select);
            select.Click += new EventHandler(select_Click);
        }

        void select_Click(object sender, EventArgs e)
        {
            (menu.Tag as IFeatureLayer).Selectable = select.Checked;
        }

        void menu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            select.Checked = (menu.Tag as IFeatureLayer).Selectable;
        }
        void TocControl_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            AxTOCControl toc = app.TocControl;

            //左键
            if (e.button == 1)
            {
                IBasicMap map = null;
                ILayer layer = null;
                object other = null;
                object index = null;
                esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;

                toc.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);
                switch (item)
                {
                    case esriTOCControlItem.esriTOCControlItemLayer:
                        break;
                    case esriTOCControlItem.esriTOCControlItemLegendClass:
                        ILegendGroup lg = other as ILegendGroup;
                        int idx = (int)index;
                        ILegendClass lc = lg.get_Class(idx);
                        ISymbol sb = lc.Symbol;
                        SimpleSymbolDlg dlg = new SimpleSymbolDlg(sb);
                        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            lc.Symbol = dlg.Symbol;
                            app.TocControl.Refresh();
                            app.MapControl.Refresh();
                        }
                        break;
                    default:
                        return;
                }                
            }
            //右键
            else if (e.button == 2)
            {
                IBasicMap map = null;
                ILayer layer = null;
                object other = null;
                object index = null;
                esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;

                toc.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);
                switch (item)
                {
                    case esriTOCControlItem.esriTOCControlItemLayer:
                        if (layer is IFeatureLayer)
                        {
                            menu.Tag = layer;
                            menu.Show(toc, e.x, e.y);
                        }
                        break;
                    case esriTOCControlItem.esriTOCControlItemLegendClass:
                        break;
                    default:
                        return;
                }                

            }
            //中键
            else
            {
            }
        }

    }
}
