using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geoprocessor;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Data;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.SpatialAnalyst;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.Geometry;
using System.Xml.Linq;
using System.IO;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.AnnotationEngine
{
    public class CommonMethods
    {

        /// <summary>
        /// 读取mdb数据库表
        /// </summary>
        /// <param name="mdbFilePath"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
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

            if (pTable != null && pTable.RowCount(null) >0)
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

        /// <summary>
        /// 设置pMap中所有注记要素类的的参考比例尺
        /// </summary>
        /// <param name="pMap"></param>
        /// <param name="referenceScale"></param>
        public static void UpdateAnnoRefScale(IMap pMap, double referenceScale)
        {
            for (int i = 0; i < pMap.LayerCount; i++)
            {
                var l = pMap.get_Layer(i);

                if (l is IFDOGraphicsLayer)
                {
                    IFeatureClass pfcl = (l as IFeatureLayer).FeatureClass;
                    IAnnoClass pAnno = pfcl.Extension as IAnnoClass;
                    IAnnoClassAdmin3 pAnnoAdmin = pAnno as IAnnoClassAdmin3;
                    if (pAnno.ReferenceScale != referenceScale)
                    {
                        pAnnoAdmin.AllowSymbolOverrides = true;
                        pAnnoAdmin.ReferenceScale = referenceScale;
                        pAnnoAdmin.UpdateProperties();
                    }
                }
            }
        }

        /// <summary>
        /// 获取注记的长度：注记要素的面积 /（字高*行数）
        /// </summary>
        /// <param name="fe"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static double GetAnnoFeatureLen(IFeature fe, double scale)
        {
            IAnnotationFeature annoFe = fe as IAnnotationFeature;
            ITextElement textElement = annoFe.Annotation as ITextElement;
            if (annoFe == null || textElement == null)
                return 0;

            string text = textElement.Text;
            string[] lineTexts = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            double fontSize = textElement.Symbol.Size / 2.8345;//mm

            double characterHeight = fontSize * 1e-3 * scale;
            double annoLen = (fe.Shape as IArea).Area / (characterHeight * lineTexts.Count());

            return annoLen;
        }

        /// <summary>
        /// 缩放线
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static IPolyline ExpandPolyline(IPolyline pl, double scale)
        {
            IPolyline newPL = null;

            if (pl == null || pl.IsEmpty)
                return pl;

            IGeometryCollection gc = pl as IGeometryCollection;
            for (var i = 0; i < gc.GeometryCount; i++)
            {
                IPointCollection subPC = gc.Geometry[i] as IPointCollection;
                
                //扩展
                IPointCollection newSubPL = new PolylineClass();
                newSubPL.AddPointCollection((subPC as IClone).Clone() as IPointCollection);
                for (int j = 1; j < subPC.PointCount; ++j)
                {
                    var line = new PolylineClass() { FromPoint = subPC.get_Point(j - 1), ToPoint = subPC.get_Point(j) };
                    IPoint pt = new PointClass();
                    if (j > 2)
                    {
                        IPoint tempPt = new ESRI.ArcGIS.Geometry.Point();
                        double distanceAlongCurve = 0;
                        double distanceFromCurve = 0;
                        bool bRightSide = false;
                        line.QueryPointAndDistance(esriSegmentExtension.esriNoExtension, newSubPL.get_Point(j - 1), false, tempPt,
                            ref distanceAlongCurve, ref distanceFromCurve, ref bRightSide);
                        if (!bRightSide)
                            distanceFromCurve = -distanceFromCurve;

                        //做平行线
                        PolylineClass offsetLine = new PolylineClass();
                        offsetLine.ConstructOffset(line, distanceFromCurve);//右为正值，左为负值

                        offsetLine.QueryPoint(esriSegmentExtension.esriExtendTangentAtTo, scale, true, pt);//延长
                    }
                    else//j=1
                    {
                        line.QueryPoint(esriSegmentExtension.esriExtendTangentAtTo, scale, true, pt);//延长
                    }

                    newSubPL.UpdatePoint(j, pt);
                }

                if (newPL == null)
                {
                    newPL = newSubPL as IPolyline;
                }
                else
                {
                    newPL = (newPL as ITopologicalOperator).Union(newSubPL as IPolyline) as IPolyline;
                    (newPL as ITopologicalOperator).Simplify();
                }
            }

            return newPL;

        }

    }
}
