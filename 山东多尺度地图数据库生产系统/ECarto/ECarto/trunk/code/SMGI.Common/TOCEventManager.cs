using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.DisplayUI;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.ADF.CATIDs;

namespace SMGI.Common
{
    public enum ContextMenuType
    {
        None = 0,
        TOCLayer,
        TOCLegend,
        TOCMap,
    }


    internal class TOCEventManager
    {
        AxTOCControl esriTOC;

        GApplication application;
        internal TOCEventManager(AxTOCControl toc, GApplication app)
        {
            esriTOC = toc;

            application = app;

            if (esriTOC != null)
            {
                //双击
                esriTOC.OnDoubleClick += new ITOCControlEvents_Ax_OnDoubleClickEventHandler(esriTOC_OnDoubleClick);
                //单击
                //esriTOC.OnMouseUp += new ITOCControlEvents_Ax_OnMouseUpEventHandler(esriTOC_OnMouseUp);
                //编辑图层名称
                esriTOC.OnEndLabelEdit += new ITOCControlEvents_Ax_OnEndLabelEditEventHandler(esriTOC_OnEndLabelEdit);
            }
        }

        

        void esriTOC_OnEndLabelEdit(object sender, ITOCControlEvents_OnEndLabelEditEvent e)
        {

        }

        //双击
        void esriTOC_OnDoubleClick(object sender, ITOCControlEvents_OnDoubleClickEvent e)
        {
            try
            {
                if (e.button == 1)
                {
                    esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
                    IBasicMap basicmap = new MapClass();
                    ILayer lyr = null;
                    object unk = Type.Missing;
                    object obj = Type.Missing;
                    ILegendGroup group = new LegendGroupClass();
                    object idx = 1;
                    object grp = group;
                    (sender as AxTOCControl).HitTest(e.x, e.y, ref item, ref basicmap, ref lyr, ref grp, ref idx);
                   
                    if (e.button == 1)
                    {
                        if (item == esriTOCControlItem.esriTOCControlItemLegendClass)
                        {
                            #region //LZ，直接跳出符号选择器(非制图表达符号）
                            ILegendClass cls = (grp as ILegendGroup).get_Class(System.Convert.ToInt32(idx));
                            if (cls is IRepresentationLegendClass)//制图表达
                                return;

                            var curContextItem = new TOCSeleteItem(item, basicmap, lyr, grp as ILegendGroup, cls);
                            if (curContextItem.Group != null && curContextItem.Class != null)
                            {
                                if (lyr is IRasterLayer && (lyr as IRasterLayer).Renderer is IRasterStretchColorRampRenderer)//色带符号
                                {
                                    IRasterStretchColorRampRenderer stretchRenderer = (lyr as IRasterLayer).Renderer as IRasterStretchColorRampRenderer;
                                    ColorRampSetForm frm = new ColorRampSetForm(stretchRenderer.ColorRamp, (stretchRenderer as IRasterStretch).Invert, e.x, e.y);
                                    if (frm.ShowDialog() == DialogResult.OK)
                                    {
                                        
                                        stretchRenderer.ColorRamp = frm.ColorRamp; 
                                        (stretchRenderer as IRasterStretch).Invert = frm.Invert;

                                        (sender as AxTOCControl).Refresh();
                                        (application.ActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, (application.ActiveView).Extent);
                                    }
                                }
                                else//普通符号
                                {
                                    ISymbolSelector symbolSelector = new SymbolSelectorClass();
                                    symbolSelector.AddSymbol(curContextItem.Class.Symbol);
                                    symbolSelector.SelectSymbol(0);
                                    ISymbol sym = symbolSelector.GetSymbolAt(0);

                                    curContextItem.Class.Symbol = sym;

                                    (sender as AxTOCControl).Refresh();
                                    (application.ActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, (application.ActiveView).Extent);
                                }
                            }
                            #endregion
                        }
                        else if (item == esriTOCControlItem.esriTOCControlItemLayer)
                        {
                            if (lyr is IFeatureLayer)
                            {
                                Helper.SetupFeatureLayerPropertySheet(lyr);

                                //更新图例
                                IMapSurround pMapSurround = null;
                                ILegend pLegend = null;
                                for (int i = 0; i < (application.ActiveView as IMap).MapSurroundCount; i++)
                                {
                                    pMapSurround = (application.ActiveView as IMap).get_MapSurround(i);
                                    if (pMapSurround is ILegend)
                                    {
                                        pLegend = pMapSurround as ILegend;
                                        pLegend.AutoVisibility = true;
                                        pLegend.Refresh();

                                    }
                                }

                                application.TOCControl.Update();
                                application.TOCControl.Refresh();

                                (application.ActiveView).PartialRefresh(esriViewDrawPhase.esriViewGeography, null, (application.ActiveView).Extent);
                            }
                            else if (lyr is IRasterLayer)
                            {
                                IRasterRenderer rasterRender = (lyr as IRasterLayer).Renderer;
                                if (!(rasterRender is IRasterRGBRenderer) && !(rasterRender is IRasterStretchColorRampRenderer))
                                {
                                    MessageBox.Show("栅格图层当前的渲染设置不支持！");
                                    return;//暂不支持其它渲染方法的属性修改
                                }

                                RasterLayerPropertyForm renderFrm = new RasterLayerPropertyForm(lyr as IRasterLayer);
                                renderFrm.ShowDialog();
                            }
                        }
                    }

                }
            }
            catch
            { }
        }

        
    }
}
