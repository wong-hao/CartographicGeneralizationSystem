using System;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using System.Data;
using System.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using System.Collections;
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesGDB;
using System.Xml.Linq;
using ESRI.ArcGIS.Framework;
using SMGI.Common;
using System.IO;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmLabelPropertySet: Form
    {
        public string TxtContent
        {
            get
            {
                string val = txtContent.Text.Trim();
                string[] strs = val.Split(new char[] {'\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (strs.Length > 1)
                {
                    val = string.Empty;
                    for (int i = 0; i < strs.Length - 1; i++)
                    {
                        val += strs[i] + "\r\n";
                    }
                    val += strs[strs.Length - 1];
                }
                return val;
            }
        }
        LabelJson labelJson = null;

        IElement anchorEle = null;
        IGeometry anchorGeo = null;


        ILineElement line1 = null;
        IGeometry geoLine1 = null;
        ILineElement line2 = null;
        IGeometry geoLine2 = null;
        IPolygonElement polygon = null;
        IGeometry geoPolygon = null;
        ITextElement txtElement = null;
        IPoint txtGeometry = null;
        ILineElement lineSep = null;
        IGeometry geoLineSep = null;


        IGroupElement groupEle = null;
        IGraphicsContainer gc = null;
        IPoint center = new PointClass();
        IActiveView act = null;
        LabelJson labelInfo = null;
        Font txtFont = new Font("宋体", 10);
        Color textColor = Color.Black;
        
        public FrmLabelPropertySet(LabelJson json, IGroupElement groupEle_)
        {
            InitializeComponent();
            labelInfo = json;
            groupEle = groupEle_;
        }
     
     

        private void ConnectLineProperty()
        {
            //引线标注
            for (int i = 0; i < groupEle.ElementCount; i++)
            {
                #region
                IElement ee = groupEle.get_Element(i);
                switch ((ee as IElementProperties).Name)
                {
                    case "锚点":
                        anchorEle = ee;
                        anchorGeo = (ee.Geometry as IClone).Clone() as IGeometry;
                        if (anchorGeo is IPolygon)
                        {
                            center = (anchorGeo as IArea).Centroid as IPoint;
                        }
                        else if (anchorGeo is IPolyline)
                        {
                            var gcs = anchorGeo as IGeometryCollection;
                            var pcs = anchorGeo as IPointCollection;
                            if (pcs.PointCount != 2)
                            {
                                center = pcs.get_Point(1);
                            }
                            else
                            {
                                (anchorGeo as IPolyline).QueryPoint(esriSegmentExtension.esriNoExtension, 0.5, true, center);
                            }

                        }
                        break;
                    case "锚线":
                        line1 = ee as ILineElement;
                        geoLine1 = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "分割线":
                        lineSep = ee as ILineElement;
                        geoLineSep = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "连接线":
                        line2 = ee as ILineElement;
                        geoLine2 = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "内容框":
                        polygon = ee as IPolygonElement;
                        geoPolygon = (ee.Geometry as IClone).Clone() as IGeometry;
                        break;
                    case "文本":
                        txtElement = ee as ITextElement;
                        this.txtContent.Text = txtElement.Text;
                        txtGeometry = (ee.Geometry as IClone).Clone() as IPoint;
                        break;
                    default:
                        break;

                }
                #endregion
            }
        }
        double mapscale = 1;
        private void FrmLabelLine_Load(object sender, EventArgs e)
        {   
            mapscale= GApplication.Application.ActiveView.FocusMap.ReferenceScale;
            this.Height -= (panelLine.Height + panelBackGround.Height + panelAnthor.Height + gbSep.Height);
            ConnectLineProperty();
            LabelClass.Instance.POIState = false;
            try
            {
                foreach(var kv in LabelClass.Instance.AnchorName)
                {
                    CmbAnchor.Items.Add(kv.Value);
                }
                foreach(var kv in LabelClass.Instance.ExtentName)
                {
                    cmbFillType.Items.Add(kv.Value);
                }
                foreach(var kv in LabelClass.Instance.LineStyle)
                {
                    cmbLabelLineType.Items.Add(kv.Value);
                    cmbFillLineStyle.Items.Add(kv.Value);
                    cmbAnchorLineStyle.Items.Add(kv.Value);
                    cmbSepType.Items.Add(kv.Value);
                    cmbSepType.SelectedIndex = 0;
                }

                
                //文字信息
                string fontname = (txtElement as ISymbolCollectionElement).FontName;
                float fontsize = (float)(txtElement as ISymbolCollectionElement).Size;
                var fs = LabelClass.Instance.GetFontStlye(txtElement);
                txtFont = new System.Drawing.Font(fontname, fontsize,fs);
                lbFont.Text = fontname + "," + fontsize + "pt";
                textColor = ColorHelper.ConvertIColorToColor(txtElement.Symbol.Color);
                lbColor.ForeColor = textColor;
                lbColor.Text = textColor.Name;
                      
                //背景框
                cmbFillType.SelectedIndex = LabelClass.Instance.ExtentName.Keys.ToList().IndexOf(labelInfo.TextType);
                btColorFill.BackColor = ColorHelper.ConvertIColorToColor((polygon as IFillShapeElement).Symbol.Color);
                cmbFillStlye.SelectedIndex = LabelClass.Instance.FillStyle.Keys.ToList().IndexOf(((polygon as IFillShapeElement).Symbol as ISimpleFillSymbol).Style.ToString());
                cmbFillLineStyle.SelectedIndex = LabelClass.Instance.LineStyle.Keys.ToList().IndexOf(((polygon as IFillShapeElement).Symbol.Outline as ISimpleLineSymbol).Style.ToString());
                btFillLineColor.BackColor = ColorHelper.ConvertIColorToColor((polygon as IFillShapeElement).Symbol.Outline.Color);
                txtFillLineWidth.Text = ((polygon as IFillShapeElement).Symbol.Outline.Width/2.83).ToString();
                //锚点
                CmbAnchor.SelectedIndex = LabelClass.Instance.AnchorName.Keys.ToList().IndexOf(labelInfo.AnchorType);
                txtAnchorSize.Text = labelInfo.AnchorSize.ToString();
                cmbAnchorFillStyle.SelectedIndex = 0;
                if (anchorGeo is IPolygon)
                {
                    AnchorColor.BackColor = ColorHelper.ConvertIColorToColor((anchorEle as IFillShapeElement).Symbol.Color);
                    cmbAnchorLineStyle.SelectedIndex = LabelClass.Instance.LineStyle.Keys.ToList().IndexOf(((anchorEle as IFillShapeElement).Symbol.Outline as ISimpleLineSymbol).Style.ToString());
                    AnchorLineColor.BackColor = ColorHelper.ConvertIColorToColor((anchorEle as IFillShapeElement).Symbol.Outline.Color);
                    cmbAnchorFillStyle.SelectedIndex = LabelClass.Instance.FillStyle.Keys.ToList().IndexOf(((anchorEle as IFillShapeElement).Symbol as ISimpleFillSymbol).Style.ToString());
                    txtAnchorLineWIdth.Text = ((anchorEle as IFillShapeElement).Symbol.Outline.Width / 2.83).ToString();
                }
                else
                {
                  
                    cmbAnchorLineStyle.SelectedIndex = LabelClass.Instance.LineStyle.Keys.ToList().IndexOf(((anchorEle as ILineElement).Symbol as ISimpleLineSymbol).Style.ToString());
                    AnchorLineColor.BackColor = ColorHelper.ConvertIColorToColor((anchorEle as ILineElement).Symbol.Color);
                    txtAnchorLineWIdth.Text = ((anchorEle as ILineElement).Symbol.Width/2.83).ToString();
                }
                //连接线

                btColorLabelLine.BackColor = ColorHelper.ConvertIColorToColor((line1 as ILineElement).Symbol.Color);
                cmbLabelLineType.SelectedIndex = LabelClass.Instance.LineStyle.Keys.ToList().IndexOf(((line1 as ILineElement).Symbol as ISimpleLineSymbol).Style.ToString());
                txtLabelLineWidth.Text = ((line1 as ILineElement).Symbol.Width/2.83).ToString();
                txtLabelLineLens.Text = Math.Round((geoLine2 as IPolyline).Length / mapscale * 1000, 1).ToString();

                cbSep.Checked = false;
                //分割线
                if (lineSep != null)
                {
                    cbSep.Checked = true;
                    cmbSepType.SelectedIndex = LabelClass.Instance.LineStyle.Keys.ToList().IndexOf(((lineSep as ILineElement).Symbol as ISimpleLineSymbol).Style.ToString());
                    txtSepInterval.Text = Math.Round((geoPolygon.Envelope.Width - (geoLineSep as IPolyline).Length) * 0.5 / mapscale * 1000, 1).ToString();
                    txtSepLineWidth.Text = ((lineSep as ILineElement).Symbol.Width/2.83).ToString();
                    btSepColor.BackColor = ColorHelper.ConvertIColorToColor((lineSep as ILineElement).Symbol.Color);
                }
                       
              
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }

        }
        public string TxtValue
        {
            get
            {
                return txtContent.Text.Trim();
            }
        }
        private void btOK_Click(object sender, EventArgs e)
        {
            if (CmbAnchor.SelectedItem == null)
                return;
            if (cmbLabelLineType.SelectedItem == null)
                return;
            if (cmbFillLineStyle.SelectedItem == null)
                return;
            if (cmbAnchorLineStyle.SelectedItem == null)
                return;
            if (CmbAnchor.SelectedItem == null)
                return;
            if (txtContent.Text.Trim() == "")
                return;

            try
            {
                double.Parse(txtAnchorSize.Text);
                double.Parse(txtLabelLineWidth.Text );
                double.Parse(txtLabelLineLens.Text);

                double.Parse(txtFillLineWidth.Text);
                double.Parse(txtAnchorLineWIdth.Text);
                 
            }
            catch
            {
                MessageBox.Show("请输入正确数值！");
            }
            UpdateParms();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();

        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btFont_Click(object sender, EventArgs e)
        {
            FontDialog pFontDialog = new FontDialog();
            pFontDialog.Font = txtFont;
            if (pFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbFont.Text = pFontDialog.Font.Name + "," + pFontDialog.Font.Size + "pt";
                txtFont = pFontDialog.Font;
            }
        }

        private void btfontColor_Click(object sender, EventArgs e)
        {
            ColorDialog pColorDialog = new ColorDialog();
            pColorDialog.Color = textColor;
            if (pColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                lbColor.ForeColor = pColorDialog.Color;
                lbColor.Text = pColorDialog.Color.Name;
                textColor = pColorDialog.Color;
            }
        }

        private void btColor_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette;
            colorPalette = new ColorPalette();
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            IColor color = ColorHelper.ConvertColorToIColor(btn.BackColor);
            color.NullColor = bool.Parse(btColorFill.Tag.ToString());
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;

            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;


            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
            {
                btn.BackColor = ColorHelper.ConvertIColorToColor(colorPalette.Color);
                btn.Tag = colorPalette.Color.NullColor;
            }

        }


        bool expand = false;
        private void btAdv_Click(object sender, EventArgs e)
        {
           
            if (expand)
            {
                this.Height -= (panelLine.Height + panelBackGround.Height + panelAnthor.Height + gbSep.Height);
            }
            else
            {
                this.Height += (panelLine.Height + panelBackGround.Height + panelAnthor.Height + gbSep.Height);
            }
            expand = !expand;
        }

        private void btSepColor_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette;
            colorPalette = new ColorPalette();
            System.Windows.Forms.Button btn = (System.Windows.Forms.Button)sender;
            IColor color = ColorHelper.ConvertColorToIColor(btn.BackColor);
            color.NullColor = bool.Parse(btColorFill.Tag.ToString());
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;

            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;


            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
            {
                btn.BackColor = ColorHelper.ConvertIColorToColor(colorPalette.Color);
                btn.Tag = colorPalette.Color.NullColor;
            }
        }
        private void UpdateParms()
        {
            string fileName = GApplication.Application.Template.Root + @"\专家库\标注引线\默认配置.xml";
            if (!File.Exists(fileName))
            {
                return;
            }
            XDocument doc = XDocument.Load(fileName);
            var root = doc.Element("Template").Element("LabelText");

            //文字信息
            XElement ele = root.Element("Text");
            ele.Element("Font").Attribute("FontName").SetValue(txtFont.Name);
            ele.Element("Font").Attribute("FontSize").SetValue(txtFont.Size);
            ele.Element("Font").Attribute("FontColor").SetValue(ColorHelper.GetCmykStrByColor(textColor));
            if (ele.Element("Font").Attribute("FontItalic") == null)
            {
                XAttribute newatt = new XAttribute("FontItalic", "");
                ele.Element("Font").Add(newatt);
            }
            if (ele.Element("Font").Attribute("FontBold") == null)
            {
                XAttribute newatt = new XAttribute("FontBold", "");
                ele.Element("Font").Add(newatt);
            }
            ele.Element("Font").Attribute("FontItalic").SetValue(txtFont.Italic);
            ele.Element("Font").Attribute("FontBold").SetValue(txtFont.Bold);

            //背景框
            ele = root.Element("Content");


            ele.Element("TextType").Value = cmbFillType.SelectedItem.ToString();
            ele.Element("FillColor").Value = ColorHelper.GetCmykStrByColor(btColorFill.BackColor);
            ele.Element("FillStlye").Value = cmbFillStlye.SelectedItem.ToString();
            ele.Element("TextLineColor").Value = ColorHelper.GetCmykStrByColor(btFillLineColor.BackColor);
            ele.Element("TextLineType").Value = cmbFillLineStyle.SelectedItem.ToString();
            ele.Element("TextLineWidth").Value = txtFillLineWidth.Text;


            //锚点
            ele = root.Element("Anchor");
            ele.Element("AnchorType").Value = CmbAnchor.SelectedItem.ToString();
            ele.Element("AnchorSize").Value = txtAnchorSize.Text;
            ele.Element("AnchorLineStyle").Value = cmbAnchorLineStyle.SelectedItem.ToString();
            ele.Element("AnchorFillColor").Value = ColorHelper.GetCmykStrByColor(AnchorColor.BackColor);
            ele.Element("AnchorLineColor").Value = ColorHelper.GetCmykStrByColor(AnchorLineColor.BackColor);
            ele.Element("AnchorLineWidth").Value = txtAnchorLineWIdth.Text;
            ele.Element("FillStlye").Value = cmbAnchorFillStyle.SelectedItem.ToString();
            //连接线
            ele = root.Element("ConnectLine");
            ele.Element("LineColor").Value = ColorHelper.GetCmykStrByColor(btColorLabelLine.BackColor);
            ele.Element("LineType").Value = cmbLabelLineType.SelectedItem.ToString();
            ele.Element("LineWidth").Value = txtLabelLineWidth.Text;
            ele.Element("ConnectLens").Value = txtLabelLineLens.Text;

            //分割线
            ele = root.Element("SepLine");
            if (ele.Attribute("check") != null)
            {
                ele.Attribute("check").Value = this.cbSep.Checked.ToString();
                //this.cbSep.Checked = bool.Parse(ele.Attribute("check").Value);
            }
            ele.Element("SepLineColor").Value = ColorHelper.GetCmykStrByColor(btSepColor.BackColor);
            ele.Element("SepLineType").Value = cmbSepType.SelectedItem.ToString();
            ele.Element("SepLineWidth").Value = txtSepLineWidth.Text;
            ele.Element("SepInterval").Value = txtSepInterval.Text;


            doc.Save(fileName);
        }
        private void View()
        {

            if (CmbAnchor.SelectedItem == null)
                return;
            if (cmbLabelLineType.SelectedItem == null)
                return;
            if (cmbFillLineStyle.SelectedItem == null)
                return;
            if (cmbAnchorLineStyle.SelectedItem == null)
                return;
            if (CmbAnchor.SelectedItem == null)
                return;
            if (txtContent.Text.Trim() == "")
                return;

            try
            {
                double.Parse(txtAnchorSize.Text);
                double.Parse(txtLabelLineWidth.Text);
                double.Parse(txtLabelLineLens.Text);

                double.Parse(txtFillLineWidth.Text);
                double.Parse(txtAnchorLineWIdth.Text);

            }
            catch
            {
                MessageBox.Show("请输入正确数值！");
            }
            UpdateParms();
            LabelClass lbHelper = new LabelClass();
            lbHelper.UpdateLabelElement(this.TxtContent, center);
        }
        private void btView_Click(object sender, System.EventArgs e)
        {
            View();
        }
 
    }
}
