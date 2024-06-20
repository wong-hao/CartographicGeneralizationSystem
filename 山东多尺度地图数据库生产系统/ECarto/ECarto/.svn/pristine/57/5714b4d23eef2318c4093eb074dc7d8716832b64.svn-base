using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ESRI.ArcGIS.Controls;
using System.IO;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Xml.Linq;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using ESRI.ArcGIS.Maplex;
using ESRI.ArcGIS.Display;
using System.Runtime.InteropServices;
using SMGI.Plugin.EmergencyMap.LabelSym;
using ESRI.ArcGIS.esriSystem;
using System.Drawing;

namespace SMGI.Plugin.EmergencyMap
{
  
    public class LabelAttSymTool : SMGI.Common.SMGITool
    {
        public LabelAttSymTool()
        {
            m_caption = "标注符号标注";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
                     
            }
        }
        IActiveView act;
      

        bool isCheck = false;
        bool isDraw = false;

        string lyrName = string.Empty;
        string fieldName = string.Empty;
        ICmykColor textColor = null;
        Font txtFont = new Font("宋体", 10);
        ISimpleFillSymbol sfs = null;
        string lableGeometry = "锥形";
        string expressionStr = string.Empty;
        bool isExpression = false;
        string sql = string.Empty;
        IGraphicsContainer gc = null;
       
        public override void OnClick()
        {
           

            act = m_Application.ActiveView;
            gc =  LabelClass.Instance.GraphicsLayer as IGraphicsContainer;
            //设置窗体
            var frm = new FrmSymTool(lyrName, fieldName, sql, txtFont);
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            isCheck = true;
            lyrName = frm.LyrName;
            fieldName = frm.FieldName;
            textColor = frm.FontColor;
            sql = frm.SQL;
            sfs = frm.SFS;
            txtFont = frm.FontText;
            lableGeometry = frm.LableType;
            isExpression = frm.IsExpress;
            expressionStr = frm.ExpressStr;
        }
        public override bool Deactivate()
        {
            isCheck = false;
            isDraw = false;
            LabelClass.Instance.ActiveDefaultLayer();
            return base.Deactivate();
        }

        public override void OnKeyUp(int keyCode, int shift)
        {
            if (keyCode == 32)
            {
                isCheck = false;
                var frm = new FrmSymTool(lyrName, fieldName, sql, txtFont);
                if (frm.ShowDialog() != DialogResult.OK)
                    return;
                isCheck = true;
                lyrName = frm.LyrName;
                fieldName = frm.FieldName;
                textColor = frm.FontColor;

                sql = frm.SQL;
                sfs = frm.SFS;
                txtFont = frm.FontText;
                lableGeometry = frm.LableType;
                isExpression = frm.IsExpress;
                expressionStr = frm.ExpressStr;
            }
        }
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (!isCheck)
                return;
            if (Button == 2)//右键取消
            {
                return;

            }
            if (Button != 1)
            {
                return;
            }
            
            IRubberBand pRubberBand = new RubberRectangularPolygonClass();
            var geo = pRubberBand.TrackNew(act.ScreenDisplay, null);
            if (geo == null || geo.IsEmpty)
            {
                geo = act.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y); 
            }
            if (geo == null || geo.IsEmpty)
            {
                return;
            }
         
            double disbuffer = act.ScreenDisplay.DisplayTransformation.FromPoints(3);
            if (geo is IPoint)
            {
                geo = (geo as ITopologicalOperator).Buffer(disbuffer);
            }
            IFeatureLayer feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => l.Visible && (l is IGeoFeatureLayer) &&
            ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == lyrName.ToUpper())).FirstOrDefault() as IFeatureLayer;
            if (feLayer != null)
            {
                ISpatialFilter sf = new SpatialFilterClass();
                sf.Geometry = geo;
                sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                sf.WhereClause = sql;
                IFeatureCursor cursor = feLayer.Search(sf, false);
                IFeature fe = null;
                while((fe=cursor.NextFeature())!=null)
                {

                    string val = fe.get_Value(feLayer.FeatureClass.FindField(fieldName)).ToString();
                    if (isExpression && expressionStr != string.Empty)
                    {
                        val = LabelExpressHelper.MapLableFromLyr(feLayer, fe, expressionStr);
                    }
                    if (val.Trim() == string.Empty)
                        continue;
                    IPoint pt = fe.ShapeCopy as IPoint;
                    CreateSym(pt, val);
                }
                Marshal.ReleaseComObject(cursor);
            }
            
        }
        Dictionary<string, IGeometry> geoDic = new Dictionary<string, IGeometry>();
        Dictionary<string, IGeometry> txtDic = new Dictionary<string, IGeometry>();
        private void CreateSym(IPoint pt, string name)
        {
            if (geoDic.Count == 0)
            {
                IWorkspace ws = GApplication.GDBFactory.OpenFromFile(GApplication.Application.Template.Root + @"\专家库\标注引线\LableRule.gdb", 0);

                IFeature fe;
                IFeatureClass fc = (ws as IFeatureWorkspace).OpenFeatureClass("LableExtent");
                IFeatureCursor cursor = fc.Search(null, false);
                while ((fe = cursor.NextFeature()) != null)
                {
                    string type = fe.get_Value(fc.FindField("TYPE")).ToString();
                    geoDic[type] = fe.ShapeCopy as IPolygon;
                }
                fc = (ws as IFeatureWorkspace).OpenFeatureClass("TextExtent");
                cursor = fc.Search(null, false);
                while ((fe = cursor.NextFeature()) != null)
                {
                    string type = fe.get_Value(fc.FindField("TYPE")).ToString();
                    txtDic[type] = fe.ShapeCopy as IPolygon;
                }
                Marshal.ReleaseComObject(cursor);
            }
          
            //文字
            //创建文字
            ITextSymbol pTextSymbol = new TextSymbolClass()
            {
                Color = textColor,
            };
            ITextElement txtElement = new TextElementClass();
           
             txtElement.Symbol = pTextSymbol;
            (txtElement as ISymbolCollectionElement).FontName = txtFont.Name;
            (txtElement as ISymbolCollectionElement).Size = txtFont.Size;
            (txtElement as ISymbolCollectionElement).Bold = txtFont.Bold;
            (txtElement as ISymbolCollectionElement).Italic = txtFont.Italic;

            (txtElement as IElement).Geometry = pt;
            txtElement.Text = name;
            IPolygon outline = new PolygonClass();
            (txtElement as IElement).QueryOutline(act.ScreenDisplay, outline);

            IEnvelope enText = outline.Envelope;
            (txtElement as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
            (txtElement as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
           

            //创建内容形状
            double orginWidth, orginHeight = 0;
            double orginTxtWidth, orginTxtHeight = 0;
            orginWidth = geoDic[lableGeometry].Envelope.Width;
            orginHeight = geoDic[lableGeometry].Envelope.Height;
            orginTxtWidth = txtDic[lableGeometry].Envelope.Width;
            orginTxtHeight =txtDic[lableGeometry].Envelope.Height;
            double dy = (txtDic[lableGeometry].Envelope as IArea).Centroid.Y - geoDic[lableGeometry].Envelope.YMin;
            dy = dy / orginHeight;

            double  geoHeight = enText.Height / orginTxtHeight * orginHeight;
            double  geoWidth = enText.Width / orginTxtWidth * orginWidth;
            geoWidth = enText.Height / orginTxtWidth * orginWidth;
            IPolygon geoPoly = null;
            IPolygonElement  polygon = new PolygonElementClass();
            geoPoly = (geoDic[lableGeometry] as IClone).Clone() as IPolygon;
            //特殊处理
            if (lableGeometry == "圆形")
            {
                IPoint circelCt = (geoPoly.Envelope as IArea).Centroid;
                (geoPoly as ITransform2D).Move(pt.X - (geoPoly.Envelope as IArea).Centroid.X, pt.Y - circelCt.Y);
            }
            else
            {
                (geoPoly as ITransform2D).Move(pt.X - (geoPoly.Envelope as IArea).Centroid.X, pt.Y - geoPoly.Envelope.YMin);
            }
            (geoPoly as ITransform2D).Scale(pt,geoWidth/orginWidth, geoHeight / orginHeight);
            (polygon as IElement).Geometry = geoPoly;
            (polygon as IFillShapeElement).Symbol = sfs;
            //修正几何
            dy = dy * geoHeight;
            if (lableGeometry == "圆形")
            {
            }
            else
            {
                (txtElement as IElement).Geometry = new PointClass { X = pt.X, Y = pt.Y + dy };
            }
            gc.AddElement(polygon as IElement, 0);
            gc.AddElement(txtElement as IElement, 0);

            IGroupElement gp = new GroupElementClass();
            gc.MoveElementToGroup(polygon as IElement,gp);
            gc.MoveElementToGroup(txtElement as IElement, gp);

            #region 标注信息
            LabelJson json = new LabelJson
            {
                TextType = lableGeometry
            };

            #endregion
            string jsonText = LabelClass.GetJsonText(json);
            (gp as IElementProperties).Name = jsonText;
            (gp as IElementProperties).Type =LabelType.SymbolLine.ToString();

            gc.AddElement(gp as IElement, 0);
            act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
           
        }
        public override void OnDblClick()
        {
            

        }
        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            
        }
       
    }
   
        
}
