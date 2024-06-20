using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Plugin.MapGeneralization
{
    class CreateAdjacencyRelation
    {
        Process progWindow = null;
        // GeoDataRecordFile errorGeoDataFile = null;
        TxtRecordFile errorTxtFile = null;
        Dictionary<string, string> Grid = new Dictionary<string, string>();
        Dictionary<string, string> Grid2 = new Dictionary<string, string>();
        List<string> tempName = new List<string>();
        List<string> tempName2 = new List<string>();
        public CreateAdjacencyRelation(Process prog)
        {
            this.progWindow = prog;
            //   this.errorTxtFile = txtRecord;
            //   this.errorGeoDataFile = geoRecord;
        }
        public Dictionary<string, string> CreateAdjacencyRelation1(IFeatureClass checkFeatureClass)
        {

            //创建要素类，并将其加入DLG要素数据集           

            IFeatureCursor checkFeatureCursor = checkFeatureClass.Search(null, true);
            IFeature CheckFeature = null;
            while ((CheckFeature = checkFeatureCursor.NextFeature()) != null)
            {               
                progWindow.label1.Text = "正在分析要素【" + CheckFeature.OID.ToString() + "】......";
                System.Windows.Forms.Application.DoEvents();
                if (CheckFeature.Shape == null || CheckFeature.Shape.IsEmpty)
                    continue;

                IPolyline CheckLine = CheckFeature.Shape as IPolyline;
                //起点
                double x = ((int)(CheckLine.FromPoint.X * 10)) / 10;
                double y = ((int)(CheckLine.FromPoint.Y * 10)) / 10;
                string curitem1 = x + "_" + y;

                double x2 = ((int)(CheckLine.ToPoint.X * 10)) / 10;
                double y2 = ((int)(CheckLine.ToPoint.Y * 10)) / 10;
                string curitem2 = x2 + "_" + y2;

                if (Grid.ContainsKey(curitem1))
                {
                    string FiledIsSelected = Grid[curitem1];
                    Grid[curitem1] = FiledIsSelected + "," + CheckFeature.OID.ToString();
                    string str = Grid[curitem1];
                }
                else
                {
                    string FiledIsSelected = CheckFeature.OID.ToString();
                    Grid.Add(curitem1, FiledIsSelected);
                }
                if (Grid.ContainsKey(curitem2))
                {
                    string FiledIsSelected = Grid[curitem2];
                    Grid[curitem2] = FiledIsSelected + "," + CheckFeature.OID.ToString();
                    string str = Grid[curitem2];
                }
                else
                {
                    string FiledIsSelected = CheckFeature.OID.ToString();
                    Grid.Add(curitem2, FiledIsSelected);
                }
            }
            progWindow.Close();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(checkFeatureCursor);
            return Grid;
        }

    }
}

