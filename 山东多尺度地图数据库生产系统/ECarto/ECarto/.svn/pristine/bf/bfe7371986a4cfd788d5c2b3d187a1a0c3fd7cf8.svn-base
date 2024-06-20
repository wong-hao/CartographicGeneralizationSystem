using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Data;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Carto;
using System.Drawing;
using ESRI.ArcGIS.Display;

namespace SMGI.Common
{
    public static class Helper
    {
        public static DataTable ReadToDataTable(string mdbFilePath, string tableName)
        {
            DataTable pDataTable = new DataTable();
            IWorkspaceFactory pWorkspaceFactory = new AccessWorkspaceFactory();
            IWorkspace pWorkspace = pWorkspaceFactory.OpenFromFile(mdbFilePath, 0);
            IEnumDataset pEnumDataset = pWorkspace.get_Datasets(esriDatasetType.esriDTTable);
            pEnumDataset.Reset();
            IDataset pDataset = pEnumDataset.Next();
            ITable pTable = null;
            while (pDataset != null)
            {
                if (pDataset.Name == tableName)
                {
                    pTable = pDataset as ITable;
                    break;
                }
                pDataset = pEnumDataset.Next();
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumDataset);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspace);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(pWorkspaceFactory);

            if (pTable != null)
            {
                ICursor pCursor = pTable.Search(null, false);
                IRow pRow = pCursor.NextRow();
                //添加表的字段信息
                for (int i = 0; i < pRow.Fields.FieldCount; i++)
                {
                    pDataTable.Columns.Add(pRow.Fields.Field[i].Name);
                }
                //添加数据
                while (pRow != null)
                {
                    DataRow dr = pDataTable.NewRow();
                    for (int i = 0; i < pRow.Fields.FieldCount; i++)
                    {
                        object obValue = pRow.get_Value(i);
                        if (obValue != null && !Convert.IsDBNull(obValue))
                        {
                            dr[i] = pRow.get_Value(i);
                        }
                        else
                        {
                            dr[i] = "";
                        }
                    }
                    pDataTable.Rows.Add(dr);
                    pRow = pCursor.NextRow();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pCursor);
            }
            
            return pDataTable;
        }

        public static IGeoProcessorResult ExecuteGPTool(Geoprocessor gp, IGPProcess process, ITrackCancel trackCancel)
        {
            try
            {
                IGeoProcessorResult res = (IGeoProcessorResult)gp.Execute(process, trackCancel);

                return res;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                string err = ex.Message + ":";
                if (gp.MessageCount > 0)
                {
                    for (int Count = 0; Count <= gp.MessageCount - 1; Count++)
                    {
                        err += gp.GetMessage(Count);

                        System.Diagnostics.Trace.WriteLine(gp.GetMessage(Count));
                    }

                }

                throw new Exception(err);
            }

            
        }

        public static bool SetupFeatureLayerPropertySheet(ILayer layer)
        {
            if (layer == null) return false;

            ESRI.ArcGIS.Framework.IComPropertySheet pComPropSheet;
            pComPropSheet = new ESRI.ArcGIS.Framework.ComPropertySheet();
            pComPropSheet.Title = layer.Name + " - 属性";

            ESRI.ArcGIS.esriSystem.UID pPPUID = new ESRI.ArcGIS.esriSystem.UIDClass();
            pComPropSheet.AddCategoryID(pPPUID);

            // General....
            ESRI.ArcGIS.Framework.IPropertyPage pGenPage = new ESRI.ArcGIS.CartoUI.GeneralLayerPropPageClass();
            pComPropSheet.AddPage(pGenPage);

            // Source
            ESRI.ArcGIS.Framework.IPropertyPage pSrcPage = new ESRI.ArcGIS.CartoUI.FeatureLayerSourcePropertyPageClass();
            pComPropSheet.AddPage(pSrcPage);

            // Selection...
            ESRI.ArcGIS.Framework.IPropertyPage pSelectPage = new ESRI.ArcGIS.CartoUI.FeatureLayerSelectionPropertyPageClass();
            pComPropSheet.AddPage(pSelectPage);

            if (layer is IFDOGraphicsLayer)
            {
                //Anno Display....
                ESRI.ArcGIS.Framework.IPropertyPage pAnnoPage = new ESRI.ArcGIS.CartoUI.AnnoDisplayPropertyPageClass();
                pComPropSheet.AddPage(pAnnoPage);

                //Anno Symbology....
                ESRI.ArcGIS.Framework.IPropertyPage pAnnoSymPage = new ESRI.ArcGIS.CartoUI.AnnoSymbologyPropertyPageClass();
                pComPropSheet.AddPage(pAnnoSymPage);
            }
            else
            {
                // Display....
                ESRI.ArcGIS.Framework.IPropertyPage pDispPage = new ESRI.ArcGIS.CartoUI.FeatureLayerDisplayPropertyPageClass();
                pComPropSheet.AddPage(pDispPage);

                // Symbology....
                ESRI.ArcGIS.Framework.IPropertyPage pDrawPage = new ESRI.ArcGIS.CartoUI.LayerDrawingPropertyPageClass();
                pComPropSheet.AddPage(pDrawPage);
            }

            // Fields... 
            ESRI.ArcGIS.Framework.IPropertyPage pFieldsPage = new ESRI.ArcGIS.CartoUI.LayerFieldsPropertyPageClass();
            pComPropSheet.AddPage(pFieldsPage);

            // Labels....
            ESRI.ArcGIS.Framework.IPropertyPage pSelPage = new ESRI.ArcGIS.CartoUI.LayerLabelsPropertyPageClass();
            pComPropSheet.AddPage(pSelPage);

            // Definition Query... 
            ESRI.ArcGIS.Framework.IPropertyPage pQueryPage = new ESRI.ArcGIS.CartoUI.LayerDefinitionQueryPropertyPageClass();
            pComPropSheet.AddPage(pQueryPage);

            //// Joins & Relates....
            //ESRI.ArcGIS.Framework.IPropertyPage pJoinPage = new ESRI.ArcGIS.ArcMapUI.JoinRelatePageClass();
            //pComPropSheet.AddPage(pJoinPage);

            // Setup layer link
            ESRI.ArcGIS.esriSystem.ISet pMySet = new ESRI.ArcGIS.esriSystem.SetClass();
            pMySet.Add(layer);
            pMySet.Reset();

            // make the symbology tab active
            pComPropSheet.ActivePage = 0;

            // show the property sheet
            bool bOK = false;
            if (pComPropSheet.CanEdit(pMySet))
            {
                bOK = pComPropSheet.EditProperties(pMySet, 0);
            }

            return (bOK);
        }

        /// <summary>
        /// ESRI颜色转Windows颜色
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color GetColorByEsriColor(IColor color)
        {
            if (color.NullColor)
                return Color.Transparent;

            return ColorTranslator.FromOle(color.RGB);
        }
        /// <summary>
        /// 根据esri颜色对象得到CMYK字符串
        /// </summary>
        /// <param name="color">颜色对象</param>
        /// <returns>CMYK字符串（形如：C100M200Y100K50）</returns>
        public static string GetCMYKTextByEsriColor(IColor color)
        {
            ICmykColor cmykColor = new CmykColorClass { CMYK = color.CMYK };
            string cmykString = string.Empty;
            if (color.NullColor)
                return cmykString;

            if (cmykColor.Cyan != 0)
                cmykString += "C" + cmykColor.Cyan.ToString();
            if (cmykColor.Magenta != 0)
                cmykString += "M" + cmykColor.Magenta.ToString();
            if (cmykColor.Yellow != 0)
                cmykString += "Y" + cmykColor.Yellow.ToString();
            if (cmykColor.Black != 0)
                cmykString += "K" + cmykColor.Black.ToString();

            return cmykString == string.Empty ? "C0M0Y0K0" : cmykString;
        }

        /// <summary>
        /// 根据CMYK字符串得到CMYK颜色值
        /// </summary>
        /// <param name="cmyk">cmyk字符串（形如：C100M200Y100K50）</param>
        /// <returns>CMYK颜色值</returns>
        public static IColor GetEsriColorByCMYKText(string cmyk)
        {
            if (cmyk.Trim() == "")
                return new CmykColorClass() { NullColor = true };

            char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder sb = new StringBuilder();
            //新建一个CMYK颜色，然后各项值付为0
            ICmykColor CMYK_Color = new CmykColorClass();
            CMYK_Color.Cyan = 0;
            CMYK_Color.Magenta = 0;
            CMYK_Color.Yellow = 0;
            CMYK_Color.Black = 0;
            try
            {
                for (int i = 0; i <= cmyk.Length; i++)
                {
                    if (i == cmyk.Length)
                    {
                        string sbs = sb.ToString();
                        if (sbs.Contains('C'))
                        {
                            CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('M'))
                        {
                            CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('Y'))
                        {
                            CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('K'))
                        {
                            CMYK_Color.Black = int.Parse(sbs.Substring(1));
                        }
                        break;
                    }
                    else
                    {
                        char C = cmyk[i];
                        if (D.Contains(C))
                        {
                            sb.Append(C);
                        }
                        else
                        {
                            string sbs = sb.ToString();
                            if (sbs.Contains('C'))
                            {
                                CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('M'))
                            {
                                CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('Y'))
                            {
                                CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('K'))
                            {
                                CMYK_Color.Black = int.Parse(sbs.Substring(1));
                            }
                            sb.Clear();
                            sb.Append(C);
                        }
                    }
                }
                return CMYK_Color;
            }
            catch
            {
                return new CmykColorClass() { NullColor = true };
            }

        }


        /// <summary>
        /// 属性显示对话框--刷新一次
        /// </summary>
        /// <param name="LayerSelected"></param>
        public static void RefreshAttributeWindow(ILayer LayerSelected)
        {
            IFeatureSelection Layerselection = (IFeatureSelection)(LayerSelected as IFeatureLayer);
            Layerselection.SelectionChanged();
            if (Layerselection != null)
            {
                ISelectionSet selectionSet = (LayerSelected as IFeatureSelection).SelectionSet;
                if (selectionSet.Count > 0)
                {
                    IEnumIDs ids = selectionSet.IDs;
                    ids.Reset();
                    IFeature fe;
                    int id = 0;

                    while ((id = ids.Next()) > 0)
                    {
                        fe = (LayerSelected as IFeatureLayer).FeatureClass.GetFeature(id);
                        GApplication.Application.ActiveView.FocusMap.SelectFeature(LayerSelected, fe);                        

                        break;
                    }
                }
                else
                {
                    var geometry = GApplication.Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(-2, -2);
                    GApplication.Application.ActiveView.FocusMap.SelectByShape(geometry, null, false);
                    GApplication.Application.ActiveView.FocusMap.ClearSelection();
                }
                GApplication.Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, GApplication.Application.ActiveView.Extent);
            }
        }
    }
}
