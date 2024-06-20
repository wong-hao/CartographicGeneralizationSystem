using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Display;

namespace SMGI.Plugin.EmergencyMap.OneKey
{
    public partial class FrmNameSet : Form
    {
        public MapNameInfo MapNameInfos = null;
        private Color textColor = Color.FromName("Black");
        private System.Drawing.Font textFontName = new System.Drawing.Font("方正宋黑简体", 35);
        public FrmNameSet()
        {
            InitializeComponent();
            string rgb = ESRI.ArcGIS.ADF.Connection.Local.Converter.ToRGBColor(textColor).RGB.ToString();
            MapNameInfos = new MapNameInfo()
            {
                MapNameSize = textFontName.Size,
                MapNameFont = textFontName.Name,
                MapNameColor = rgb,
                IsArtStyle = cbShadow.Checked,
                MapNameSpace = double.Parse(txtWordSpace.Text),
                MapNameWidth = double.Parse(txtWordWidth.Text),
                MapNameDis = double.Parse(txtTopDis.Text)
            };
        }

        private void cmdFont_Click(object sender, EventArgs e)
        {
            FontDialog pFontDialog = new FontDialog();
            pFontDialog.Font = textFontName;
            if (pFontDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbfont.Text = "字体：" + pFontDialog.Font.Name + "," + pFontDialog.Font.Size + "pt";             
                //lbfont.Font = pFontDialog.Font;
                textFontName = pFontDialog.Font;
            }
        }

        private void cmdColor_Click(object sender, EventArgs e)
        {
            ColorDialog pColorDialog = new ColorDialog();
            pColorDialog.Color = textColor;
            if (pColorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                lbcolor.ForeColor = pColorDialog.Color;
                lbcolor.Text = pColorDialog.Color.Name;
                textColor = pColorDialog.Color;
            }
           
        }

        private void btn_nameSetOK_Click(object sender, EventArgs e)
        {
            string rgb = ESRI.ArcGIS.ADF.Connection.Local.Converter.ToRGBColor(textColor).RGB.ToString();
            MapNameInfos = new MapNameInfo() 
            { 
                MapNameSize = textFontName.Size,
                MapNameFont = textFontName.Name,
                MapNameColor = rgb,
                IsArtStyle =cbShadow.Checked,
                MapNameSpace = double.Parse(txtWordSpace.Text),
                MapNameWidth = double.Parse(txtWordWidth.Text),
                MapNameDis = double.Parse(txtTopDis.Text) };
            DialogResult = DialogResult.OK;
        }

        private void FrmNameSet_Load(object sender, EventArgs e)
        {

        }
    }

    public class MapNameInfo
    {
        public float MapNameSize;//图名大小
        public string MapNameFont;//图名字体类型
        public string MapNameColor;//图名颜色
        public bool IsArtStyle;//是否艺术字
        public double MapNameSpace;//字符间距
        public double MapNameWidth;//字符宽度
        public double MapNameDis;//外图阔间距  
    }
}
