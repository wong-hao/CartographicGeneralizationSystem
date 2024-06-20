using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
namespace SMGI.Plugin.EmergencyMap.LabelSym
{
    public partial class FrmSymLabelProperty : Form
    {
        public FrmSymLabelProperty()
        {
            InitializeComponent();
        }
        string lbGeometry = string.Empty;
        IGroupElement groupEle = null;
        IGraphicsContainer gc = null;
        public FrmSymLabelProperty(LabelJson json, IGroupElement groupEle_ ,IGraphicsContainer gc_)
        {
            InitializeComponent();
            lbGeometry = json.TextType;
            groupEle = groupEle_;
            ConnectLineProperty();
            txtSym=  txtEle.Symbol as ITextSymbol;
            SFS = polyEle.Symbol as ISimpleFillSymbol;
            gc = gc_;
        }
        public string LyrName=string.Empty;
        public string FieldName = string.Empty;
        public Font FontText=new Font("宋体",10);
        public ICmykColor FontColor;
        Color textColor = Color.Black;
        public ISimpleFillSymbol SFS;
        ICmykColor fillColor = null;
        ICmykColor outLineColor = null;
        public string LableType
        {
            get
            {
                return cmbFillType.SelectedItem.ToString();
            }
        }
        ITextElement txtEle = null;
        ITextSymbol txtSym = null;
        IFillShapeElement polyEle = null;
        private void ConnectLineProperty()
        {
            //引线标注
            for (int i = 0; i < groupEle.ElementCount; i++)
            {
               
                IElement ee = groupEle.get_Element(i);
                if (ee is ITextElement)
                {
                    txtEle = ee as ITextElement;
                }
                if (ee is IFillShapeElement)
                {
                    polyEle = ee as IFillShapeElement;
                }
               
            }
        }
        private void FrmSymTool_Load(object sender, EventArgs e)
        {
           
             
            foreach (var kv in LabelClass.Instance.LineStyle)
            {
                
                cmbFillLineStyle.Items.Add(kv.Value);
               
            }
            cmbFillLineStyle.SelectedIndex = 0;
            cmbFillStlye.SelectedIndex = 0;
            cmbFillType.Items.AddRange(new string[] { "圆形", "锥形" });
            cmbFillType.SelectedIndex = cmbFillType.Items.IndexOf(lbGeometry);


            var fs = LabelClass.Instance.GetFontStlye(txtEle);
            FontText = new Font(txtSym.Font.Name, (float)(txtSym.Size), fs);
            FontColor = new CmykColorClass { CMYK = txtSym.Color.CMYK };
            lbFont.Font = FontText;
            lbFont.Text = FontText.Name + "," + FontText.Size + "pt";
            textColor = ColorHelper.ConvertIColorToColor(FontColor);
            lbColor.ForeColor = textColor;
            lbColor.Text = textColor.Name;


            Color bg = ColorHelper.ConvertIColorToColor((polyEle.Symbol as ISimpleFillSymbol).Color);
            btColorFill.BackColor = bg;
            fillColor = new CmykColorClass { CMYK = (polyEle.Symbol as ISimpleFillSymbol).Color.CMYK };

            if (SFS.Style == esriSimpleFillStyle.esriSFSNull)
            {
                cmbFillStlye.SelectedIndex = cmbFillStlye.Items.IndexOf("空心");
                
            }
            else
            {
                cmbFillStlye.SelectedIndex = cmbFillStlye.Items.IndexOf("实心");
            }
            var dic = LabelClass.Instance.LineStyle.Where(t => t.Key ==(SFS.Outline as ISimpleLineSymbol).Style.ToString()).ToDictionary(p => p.Key, p => p.Value);
            cmbFillLineStyle.SelectedIndex = cmbFillLineStyle.Items.IndexOf(dic.First().Value);
            txtFillLineWidth.Text=((SFS.Outline as ISimpleLineSymbol).Width/2.83).ToString();
            btFillLineColor.BackColor=  ColorHelper.ConvertIColorToColor((SFS.Outline as ISimpleLineSymbol).Color);

            outLineColor = new CmykColorClass { CMYK = (SFS.Outline as ISimpleLineSymbol).Color.CMYK };
            this.txtContent.Text = txtEle.Text;
        }



        private void SetParms()
        {
            
            SFS = new SimpleFillSymbolClass();
            SFS.Color = fillColor;
            if (cmbFillStlye.SelectedItem.ToString() != "实心")
            {
                SFS.Style = esriSimpleFillStyle.esriSFSNull;
            }
            else
            {
                SFS.Style = esriSimpleFillStyle.esriSFSSolid;
            }
            var dic = LabelClass.Instance.LineStyle.Where(t => t.Value == cmbFillLineStyle.SelectedItem.ToString()).ToDictionary(p => p.Key, p => p.Value);

            SFS.Outline = new SimpleLineSymbolClass()
            {
                Width = double.Parse(txtFillLineWidth.Text) * 2.83,
                Style = (esriSimpleLineStyle)System.Enum.Parse(typeof(esriSimpleLineStyle), dic.First().Key),
                Color = outLineColor
            };
        }
        Dictionary<string, IGeometry> geoDic = new Dictionary<string, IGeometry>();
        Dictionary<string, IGeometry> txtDic = new Dictionary<string, IGeometry>();
        private void UpdateEles()
        {
            IGeometry geoP = (polyEle as IElement).Geometry as IGeometry;
            IPoint pt = new PointClass { X = geoP.Envelope.XMax * 0.5 + geoP.Envelope.XMin * 0.5, Y = geoP.Envelope.YMin };
            if (lbGeometry == "圆形")
            {
                pt = new PointClass { X = geoP.Envelope.XMax * 0.5 + geoP.Envelope.XMin * 0.5, Y = (geoP.Envelope.YMin + geoP.Envelope.YMax) * 0.5 };
            }
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

            ITextSymbol pTextSymbol = new TextSymbolClass()
            {
                Color = FontColor,
            };
            txtEle.Text = this.txtContent.Text.Trim();
            txtEle.Symbol = pTextSymbol;
            (txtEle as ISymbolCollectionElement).FontName = FontText.Name;
            (txtEle as ISymbolCollectionElement).Size = FontText.Size;
            (txtEle as ISymbolCollectionElement).VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
            (txtEle as ISymbolCollectionElement).HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
            (txtEle as ISymbolCollectionElement).Bold = FontText.Bold;
            (txtEle as ISymbolCollectionElement).Italic = FontText.Italic;
            (txtEle as IElement).Geometry = pt;
            string  lableGeometry = LableType;
            //创建内容形状
            double orginWidth, orginHeight = 0;
            double orginTxtWidth, orginTxtHeight = 0;
            orginWidth = geoDic[lableGeometry].Envelope.Width;
            orginHeight = geoDic[lableGeometry].Envelope.Height;
            orginTxtWidth = txtDic[lableGeometry].Envelope.Width;
            orginTxtHeight = txtDic[lableGeometry].Envelope.Height;
            double dy = (txtDic[lableGeometry].Envelope as IArea).Centroid.Y - geoDic[lableGeometry].Envelope.YMin;
            dy = dy / orginHeight;

            IPolygon outline = new PolygonClass();

            (txtEle as IElement).QueryOutline(GApplication.Application.ActiveView.ScreenDisplay, outline);
            IEnvelope enText = outline.Envelope;
            double geoHeight = enText.Height / orginTxtHeight * orginHeight;
            double geoWidth = enText.Width / orginTxtWidth * orginWidth;
            geoWidth = enText.Height / orginTxtWidth * orginWidth;
            IPolygon geoPoly = null;
            IPolygonElement polygon = new PolygonElementClass();
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
            (geoPoly as ITransform2D).Scale(pt, geoWidth / orginWidth, geoHeight / orginHeight);
            (polygon as IElement).Geometry = geoPoly;
            (polygon as IFillShapeElement).Symbol = SFS;
            //修正几何
            dy = dy * geoHeight;
            if (lableGeometry == "圆形")
            {
            }
            else
            {
                (txtEle as IElement).Geometry = new PointClass { X = pt.X, Y = pt.Y + dy };
            }
            groupEle.ClearElements();
            groupEle.AddElement(polygon as IElement);
            groupEle.AddElement(txtEle as IElement);

           

            #region 标注信息
            LabelJson json = new LabelJson
            {
                TextType = lableGeometry
            };

            #endregion
            string jsonText = LabelClass.GetJsonText(json);
            (groupEle as IElementProperties).Name = jsonText;
            (groupEle as IElementProperties).Type = LabelType.SymbolLine.ToString();

            gc.UpdateElement(groupEle as IElement);
            GApplication.Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        private void btOK_Click(object sender, EventArgs e)
        {

            SetParms();
            UpdateEles();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

      

        private void btCancel_Click(object sender, EventArgs e)
        {
            Close();
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
                if (btn.Name == "btColorFill")
                {
                    fillColor = new CmykColorClass { CMYK = colorPalette.Color.CMYK };
                }
                else
                {
                    outLineColor = new CmykColorClass { CMYK = colorPalette.Color.CMYK };
                }
            }

        }

        private void btFont_Click(object sender, EventArgs e)
        {
            FontDialog pFontDialog = new FontDialog();
            pFontDialog.Font = FontText;
            if (pFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbFont.Font = FontText;
                lbFont.Text = pFontDialog.Font.Name + "," + pFontDialog.Font.Size + "pt";
                FontText = pFontDialog.Font;
            }
        }

        private void btfontColor_Click(object sender, EventArgs e)
        {
            IColorPalette colorPalette;
            colorPalette = new ColorPalette();

            IColor color = FontColor;
            color.NullColor = bool.Parse(btColorFill.Tag.ToString());
            tagRECT tagRect = new tagRECT();
            tagRect.left = (this.Left * 2 + this.Width) / 2 - 100;

            tagRect.bottom = (this.Top * 2 + this.Height) / 2 - 100;
            if (colorPalette.TrackPopupMenu(ref tagRect, color, false, 0))
            {
                lbColor.ForeColor = ColorHelper.ConvertIColorToColor(colorPalette.Color);
                FontColor = new CmykColorClass { CMYK = colorPalette.Color.CMYK };
            }
           

        }

        private void btView_Click(object sender, EventArgs e)
        {
            SetParms();
            UpdateEles();
        }
      
    }
}
