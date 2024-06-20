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
namespace SMGI.Plugin.EmergencyMap.LabelSym
{
    public partial class FrmSymTool : Form
    {
        public FrmSymTool()
        {
            InitializeComponent();
        }
        public FrmSymTool(string lyr, string field, string sql, Font FontText_)
        {
            InitializeComponent();
            LyrName = lyr;
            FieldName = field;
            txtSql.Text = sql;
            FontText = FontText_;

        }
        public string LyrName=string.Empty;
        public string FieldName = string.Empty;
        public Font FontText=new Font("宋体",10);
        public ICmykColor FontColor;
        Color textColor = Color.Black;
        public ISimpleFillSymbol SFS;
        public string LableType
        {
            get
            {
                return cmbFillType.SelectedItem.ToString();
            }
        }
        public bool IsExpress
        {
            get
            {
                return cbVbScript.Checked;
            }
        }
        public string ExpressStr = string.Empty;
        
        private void FrmSymTool_Load(object sender, EventArgs e)
        {
            this.cmbFields.Enabled = !cbVbScript.Checked;
            this.btExpress.Enabled = cbVbScript.Checked;
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return l.Visible&&(l is IFeatureLayer)&&(l as IFeatureLayer).FeatureClass.ShapeType== ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint;
            }));
            foreach (var lyr in lyrs)
            {
                if ((lyr as IFeatureLayer).FeatureClass != null)
                    cmbLyr.Items.Add((lyr as IFeatureLayer).FeatureClass.AliasName);
            }
            if (cmbLyr.Items.IndexOf(LyrName) != -1)
            {
                cmbLyr.SelectedIndex = cmbLyr.Items.IndexOf(LyrName);
            }
            foreach (var kv in LabelClass.Instance.LineStyle)
            {
                
                cmbFillLineStyle.Items.Add(kv.Value);
               
            }
            cmbFillLineStyle.SelectedIndex = 0;
            cmbFillStlye.SelectedIndex = 0;
            cmbFillType.Items.AddRange(new string[] { "圆形", "锥形" });
            cmbFillType.SelectedIndex = 0;

            lbFont.Font = FontText;
            lbFont.Text = FontText.Name + "," + FontText.Size + "pt";
            lbColor.ForeColor = textColor;
            lbColor.Text = textColor.Name;

            CmykColorClass cmy = new CmykColorClass();
            cmy.Cyan = 80;
            cmy.Magenta = 40;
            cmy.Yellow = 80;
            cmy.Black = 0;
            Color bg = ColorHelper.ConvertICMYKColorToColor(cmy);
            btColorFill.BackColor = bg;
            btFillLineColor.BackColor = bg;

        }
        private List<string> GetFields(IFeatureClass fc)
        {
            List<string> ruleNames = new List<string>();
            for (int i = 0; i < fc.Fields.FieldCount; i++)
            {
                var field = fc.Fields.get_Field(i);
                if (field.Type != esriFieldType.esriFieldTypeGeometry && field.Type != esriFieldType.esriFieldTypeBlob && !field.Name.ToLower().Contains("shape") && field.Type != esriFieldType.esriFieldTypeOID)
                {
                    ruleNames.Add(field.Name.ToUpper());
                }
            }
            return ruleNames;
        }
        private void cmbLyr_SelectedIndexChanged(object sender, EventArgs e)
        {
            IFeatureLayer feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
           ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == cmbLyr.SelectedItem.ToString().ToUpper())).FirstOrDefault() as IFeatureLayer;
            if (feLayer != null)
            {
                cmbFields.Items.Clear();
                foreach (var kv in GetFields(feLayer.FeatureClass))
                {
                    cmbFields.Items.Add(kv);
                    
                }
                cmbFields.SelectedIndex = cmbFields.Items.IndexOf("Name");
            }
            if (cmbFields.Items.IndexOf(FieldName) != -1)
            {
                cmbFields.SelectedIndex = cmbFields.Items.IndexOf(FieldName);
            }
        }

        private void cmbFields_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (cmbLyr.SelectedItem == null)
            {
                MessageBox.Show("请设置图层和字段！");
                return;
            }
            if (IsExpress && ExpressStr == string.Empty)
            {
                MessageBox.Show("请设置注记表达式！");
                return;
            }
            if(!IsExpress &&cmbFields.SelectedItem == null)
            {
                MessageBox.Show("请设置字段！");
                return;
            }
            LyrName = cmbLyr.SelectedItem.ToString();
            FieldName = cmbFields.SelectedItem.ToString();
            FontColor = ColorHelper.ConvertColorToCMYK(textColor);
            SFS = new SimpleFillSymbolClass();
            SFS.Color = ColorHelper.ConvertColorToCMYK(btColorFill.BackColor);
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
                Color =  ColorHelper.ConvertColorToCMYK(btFillLineColor.BackColor)
            };
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (cmbLyr.SelectedItem == null)
                return;
            IFeatureLayer feLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) &&
                    ((l as IGeoFeatureLayer).FeatureClass.AliasName.Trim().ToUpper() == cmbLyr.SelectedItem.ToString().ToUpper())).FirstOrDefault() as IFeatureLayer;
            if (feLayer== null)
            {
                return;
            }
            SelectBySQLDialog dlg = new SelectBySQLDialog(GApplication.Application.ActiveView as IMap);
            dlg.OnlyOneLayer = true;
            dlg.LayerSelected = feLayer;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                txtSql.Text = dlg.SQLCondition;
            }
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
            ColorDialog pColorDialog = new ColorDialog();
            pColorDialog.Color = textColor;
            if (pColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                lbColor.ForeColor = pColorDialog.Color;
                lbColor.Text = pColorDialog.Color.Name;
                textColor = pColorDialog.Color;
            }

        }

        private void btExpress_Click(object sender, EventArgs e)
        {
            FrmLbExpress frm = new FrmLbExpress();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                ExpressStr = frm.ExpressionStr;
            }

        }

        private void cbVbScript_CheckedChanged(object sender, EventArgs e)
        {
            this.cmbFields.Enabled = !cbVbScript.Checked;
            this.btExpress.Enabled = cbVbScript.Checked;
             
        }

        public string SQL
        {
            get
            {
                return txtSql.Text.Trim();
            }
        }
    }
}
