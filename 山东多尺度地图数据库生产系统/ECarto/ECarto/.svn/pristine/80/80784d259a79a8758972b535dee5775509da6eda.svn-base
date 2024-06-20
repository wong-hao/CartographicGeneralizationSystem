using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using SMGI.Common;
using ESRI.ArcGIS.esriSystem;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using ESRI.ArcGIS.Geodatabase;
namespace SMGI.Plugin.EmergencyMap
{

    public sealed class LabelBallCalloutTool : SMGITool
    {
        
        public LabelBallCalloutTool()
        {
            
        }
        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null;
            }
        }  

        

        #region Overridden Class Methods
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);
           
        }
        public override void Refresh(int hdc)
        {
           
        }
        
        IGraphicsContainerSelect gs = null;
        IActiveView act = null;
        IGraphicsContainer gc = null;
        Dictionary<string, esriBalloonCalloutStyle> ballStyleDic = new Dictionary<string, esriBalloonCalloutStyle>();
        Dictionary<string, FontStyle> fontStyleDic = new Dictionary<string, FontStyle>();
        bool IsSet = false;
        FrmLableBall frm = null;
        public override void OnClick()
        {

            act = m_Application.ActiveView;
            gc = LabelClass.Instance.GraphicsLayer as IGraphicsContainer;
            gs = gc as IGraphicsContainerSelect;
            ballStyleDic["矩形框"] = esriBalloonCalloutStyle.esriBCSRectangle;
            ballStyleDic["圆角矩形"] = esriBalloonCalloutStyle.esriBCSRoundedRectangle;
            ballStyleDic["无边框"] = esriBalloonCalloutStyle.esriBCSOval;

            fontStyleDic["常规"] = FontStyle.Regular;
            fontStyleDic["加粗"] = FontStyle.Bold;
            fontStyleDic["倾斜"] = FontStyle.Italic;
            frm = new FrmLableBall();
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            IsSet = true;
        }
        public override void OnKeyUp(int keyCode, int shift)
        {
            if (keyCode == 32)
            {
                IsSet = false;
                frm = new FrmLableBall();
                if (frm.ShowDialog() != DialogResult.OK)
                    return;
                IsSet = true;
            }
        }
        public override bool Deactivate()
        {
            LabelClass.Instance.ActiveDefaultLayer();
            IsSet = false;
            return base.Deactivate();
        }
        public override void OnMouseDown(int Button, int Shift, int x, int y)
        {
            if (Button != 1)
                return;
            if (!IsSet)
            {
                return;
            }
            IPoint pPoint1 = act.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            if (frm.POI)
            {
                double disbuffer = act.ScreenDisplay.DisplayTransformation.FromPoints(3);
                IGeometry geobuffer = (pPoint1 as ITopologicalOperator).Buffer(disbuffer);
                IFeatureLayer feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => l.Visible && (l is IGeoFeatureLayer) &&
                ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == "POI")).FirstOrDefault() as IFeatureLayer;
                if (feLayer == null)
                {
                    feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => l.Visible&&(l is IGeoFeatureLayer) &&
                        ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == "AGNP")).FirstOrDefault() as IFeatureLayer;
                }
                if (feLayer != null)
                {
                    ISpatialFilter sf = new SpatialFilterClass();
                    sf.Geometry = geobuffer;
                    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                    IFeatureCursor cursor= feLayer.Search(sf, false);
                    IFeature fe = cursor.NextFeature();
                    if (fe != null)
                    {
                        string val = fe.get_Value(feLayer.FeatureClass.FindField("Name")).ToString();
                        if(val.Trim()!=string.Empty)
                          frm.txtContent.Text = val;

                    }
                    Marshal.ReleaseComObject(cursor);
                }
            }
            {
                Color ballRgb = frm.btColorFill.BackColor;
                Color outlineRgb = frm.btColorOutLine.BackColor;
                Color textRgb = frm.btColorText.BackColor;
                ISimpleFillSymbol pSmpleFill = new SimpleFillSymbol();
                pSmpleFill.Style = esriSimpleFillStyle.esriSFSSolid;
                pSmpleFill.Color =ColorHelper.ConvertColorToIColor(ballRgb);
                ISimpleLineSymbol lineSymbol = new SimpleLineSymbol();
                lineSymbol.Width = double.Parse(frm.txtOutLineWidth.Text);
                lineSymbol.Color = ColorHelper.ConvertColorToIColor(outlineRgb);
                pSmpleFill.Outline = lineSymbol;

                IBalloonCallout pBllCallout = new BalloonCalloutClass();
                pBllCallout.Style = ballStyleDic[frm.cmbBallStyle.SelectedItem.ToString()];

                IPoint pPoint2 = new PointClass();
                pPoint2.X = pPoint1.X + 5*act.FocusMap.ReferenceScale*0.001;
                pPoint2.Y = pPoint1.Y + 5 * act.FocusMap.ReferenceScale * 0.001;
                pBllCallout.Symbol = pSmpleFill;
                pBllCallout.LeaderTolerance = 5;
                pBllCallout.AnchorPoint = pPoint1;

                //文本
                IFormattedTextSymbol pTextSymbol = new TextSymbolClass();
                pTextSymbol.Direction = esriTextDirection.esriTDAngle;
                pTextSymbol.Angle = double.Parse(frm.txtAngle.Text);
                pTextSymbol.Background = pBllCallout as ITextBackground;
                pTextSymbol.Color = ColorHelper.ConvertColorToIColor(frm.btColorText.BackColor);
                pTextSymbol.Size = double.Parse(frm.cmbFontSize.SelectedItem.ToString()); 

                FontStyle fontStyle = new FontStyle();
                fontStyle =fontStyleDic[frm.cmbFontShp.SelectedItem.ToString()];
                Font font = new Font(frm.cmbFont.SelectedItem.ToString(), Convert.ToSingle(frm.cmbFontSize.SelectedItem.ToString()), fontStyle);
                pTextSymbol.Font = ESRI.ArcGIS.ADF.COMSupport.OLE.GetIFontDispFromFont(font) as stdole.IFontDisp;
                pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                switch (frm.cmbFontLocation.Text)
                {
                    case "靠左":
                        pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;
                        break;
                    case "居中":
                        pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                        break;
                    case "靠右":
                        pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHARight;
                        break;
                }
                ITextSymbol textSymbol = pTextSymbol as ITextSymbol;

                ITextElement textElement = new TextElementClass();
                textElement.Text =frm.txtContent.Text;
                textElement.Symbol = textSymbol;
                textElement.ScaleText = true;

                IElement element = (IElement)textElement;
                element.Geometry = pPoint2;
                string elementParameter = frm.txtContent.Text + "," + pPoint1.X + "," + pPoint1.Y + "," + ballRgb.ToArgb()
                     + "," + frm.cmbBallStyle.SelectedItem.ToString() + "," + frm.txtOutLineWidth.Text + "," + outlineRgb.ToArgb() + "," + frm.txtAngle.Text
                     + "," + textRgb.ToArgb() + "," + frm.cmbFontSize.SelectedItem.ToString() + "," + frm.cmbFontLocation.Text + "," + frm.cmbFont.SelectedItem.ToString() + "," + frm.cmbFontShp.SelectedItem.ToString();
                (element as IElementProperties3).Name = elementParameter;//原参数
                (element as IElementProperties3).Type = LabelType.BallCallout.ToString();//类型
                gc.AddElement(element, 0);
                act.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
           
        }
        
        
        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
           
           
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            

            
        }
        IPoint center = new PointClass();
         
        public override void OnDblClick()
        {
           
        }
        #endregion
    }
}
