using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;
using stdole;
using System.IO;
using System.Xml.Linq;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmGridLine : Form
    {
       
        GApplication app;
        string GridName = "索引网";
        public FrmGridLine()
        {
            InitializeComponent();
            app = GApplication.Application;
        }

        private void FrmGridLine_Load(object sender, EventArgs e)
        {
            groupBox3.Enabled = false;
            var mapgridItem = GetNearestSize();
            if (mapgridItem != null)
            {
                if (mapgridItem["格网线文字尺寸"] != DBNull.Value && mapgridItem["格网线文字尺寸"] != "")
                {
                    txtAnnoSize.Text = mapgridItem["格网线文字尺寸"].ToString();
                   
                }
                if (mapgridItem["格网线序号尺寸"] != DBNull.Value && mapgridItem["格网线序号尺寸"] != "" )
                {
                    txtNumSize.Text = mapgridItem["格网线序号尺寸"].ToString();
                }
                if (mapgridItem["格网线文字与内图廓间距"] != DBNull.Value && mapgridItem["格网线文字与内图廓间距"] != "" )
                {
                    txtAnnoInterval.Text = mapgridItem["格网线文字与内图廓间距"].ToString();
                    NumInterval.Text = mapgridItem["格网线文字与内图廓间距"].ToString();
                }
                if (mapgridItem["格网线间距"] != DBNull.Value && mapgridItem["格网线间距"] != "") 
                {
                    double step = double.Parse(mapgridItem["格网线间距"].ToString());
                    double ms = app.ActiveView.FocusMap.ReferenceScale;
                    double width = pageGeo.Envelope.Width / ms * 1000; //毫米
                    double height = pageGeo.Envelope.Height / ms * 1000;//毫米
                    int row = (int)(height / step);
                    int column = (int)(width / step);
                    txtRow.Text = row.ToString();
                    txtColumn.Text = column.ToString();
                }
            }
            this.cmbMapType.SelectedIndex = 0;
            radiaIndex.Checked = true;
        }
        //获取最近的【纸张开本】
        IPolygon pageGeo = null;
        DataTable ruleDatatable = null;
        private DataRow GetNearestSize()
        {
            DataRow itemSel = null;
            try
            {
                if (pageGeo == null)
                {
                    var lyr = app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l => { return (l is IGeoFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName== "ClipBoundary"); })).First() as IFeatureLayer;
                    #region
                    IFeatureClass clipfcl = lyr.FeatureClass;
                    if (clipfcl.FeatureCount(null) != 0)
                    {
                        IFeature fe = null;
                        IQueryFilter qf = new QueryFilterClass();
                        qf.WhereClause = "TYPE = '页面'";
                        IFeatureCursor cursor = clipfcl.Search(qf, false);
                        fe = cursor.NextFeature();
                        pageGeo = fe.ShapeCopy as IPolygon;
                        Marshal.ReleaseComObject(cursor);
                        Marshal.ReleaseComObject(fe);
                        Marshal.ReleaseComObject(qf);
                    }
                    #endregion
                }
                double ms = app.ActiveView.FocusMap.ReferenceScale;
                double width = pageGeo.Envelope.Width / ms * 1000; //毫米
                double height = pageGeo.Envelope.Height / ms * 1000;//毫米

                double min = Math.Min(width, height);
                double max = Math.Max(width, height);
                string filemdb = app.Template.Root + @"\专家库\纸张整饰参数\整饰参数经验值.mdb";
                if (ruleDatatable == null)
                {                    
                    ruleDatatable = CommonMethods.ReadToDataTable(filemdb, "纸张整饰参数经验值");  
                }


                for (int i = 0; i < ruleDatatable.Rows.Count; i++)
                {
                    #region 考虑两边
                    DataRow row = ruleDatatable.Rows[i];
                    double pagewidth = double.Parse(row["纸张宽"].ToString());
                    double pageheight = double.Parse(row["纸张高"].ToString());
                    double pagemin = Math.Min(pagewidth, pageheight);
                    double pagemax = Math.Max(pagewidth, pageheight);
                    if ((Math.Abs(pagemin - min) / min) < 0.2 && Math.Abs(pagemax - max) / max < 0.2)
                    {
                        itemSel = row;
                        break;
                    }
                    #endregion
                }
                if (itemSel == null)
                {
                    #region 考虑单边
                    for (int i = 0; i < ruleDatatable.Rows.Count; i++)
                    {
                        DataRow row = ruleDatatable.Rows[i];
                        double pagewidth = double.Parse(row["纸张宽"].ToString());
                        double pageheight = double.Parse(row["纸张高"].ToString());
                        double pagemin = Math.Min(pagewidth, pageheight);
                        double pagemax = Math.Max(pagewidth, pageheight);
                        if ((Math.Abs(pagemin - min) / min) < 0.2 || Math.Abs(pagemin - max) / max < 0.2)
                        {
                            itemSel = row;
                            break;
                        }
                        if ((Math.Abs(pagemax - min) / min) < 0.2 || Math.Abs(pagemax - max) / max < 0.2)
                        {
                            itemSel = row;
                            break;
                        }
                    }
                    #endregion
                }
                return itemSel;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return null;
            }

        }
        private DataTable LoadDataFromExcel(string filePath)
        {
            try
            {
                string strConn;
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=YES;IMEX=1'";
                System.Data.OleDb.OleDbConnection OleConn = new System.Data.OleDb.OleDbConnection(strConn);
                OleConn.Open();
                String sql = "SELECT * FROM  [Sheet1$]";
                System.Data.OleDb.OleDbDataAdapter OleDaExcel = new System.Data.OleDb.OleDbDataAdapter(sql, OleConn);
                DataSet OleDsExcle = new DataSet();
                OleDaExcel.Fill(OleDsExcle, "Sheet1");
                OleConn.Close();
                return OleDsExcle.Tables[0];
            }
            catch (Exception err)
            {
                System.Diagnostics.Trace.WriteLine(err.Message);
                System.Diagnostics.Trace.WriteLine(err.Source);
                System.Diagnostics.Trace.WriteLine(err.StackTrace);

                MessageBox.Show("加载Excel失败!失败原因：" + err.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }
        //加载外部参数
        private void LoadGridLineOutParams(string fileName)
        {
            
            XDocument doc;
           
            {
                doc = XDocument.Load(fileName);

                var content = doc.Element("Template").Element("Content");
                var gridline = content.Element("GridLine");
                //1
                var meaturegrid= gridline.Element("MeatureGrid");
                if (bool.Parse(meaturegrid.Attribute("checked").Value))
                    radiameature.Checked = true;
                txtMeatureX.Text = meaturegrid.Element("X").Value;
                txtMeatureY.Text = meaturegrid.Element("Y").Value;
                //2
                var graticulegrid = gridline.Element("GraticuleGrid");
                string lo = graticulegrid.Element("Longitude").Value;
                string[] los = lo.Split(new char[] { '°', '′', '″' }, StringSplitOptions.RemoveEmptyEntries);
                txtlongDe.Text = los[0];
                txtlongMin.Text = los[1];
                txtlongSe.Text = los[2];

                string la = graticulegrid.Element("Latitude").Value;
                string[] las = la.Split(new char[] { '°', '′', '″' }, StringSplitOptions.RemoveEmptyEntries);
                txtlatDe.Text = las[0];
                txtlatMin.Text = las[1];
                txtlatSe.Text = las[2];
                if (bool.Parse(graticulegrid.Attribute("checked").Value))
                    radiagraducal.Checked = true;
                //3
                var indexgrid = gridline.Element("IndexGrid");
                txtRow.Text=indexgrid.Element("Row").Value;
                txtColumn.Text=indexgrid.Element("Column").Value;
                if (bool.Parse(indexgrid.Attribute("checked").Value))
                    radiaIndex.Checked = true;
                //anno
                var annoh = gridline.Element("AnnoSize");
                txtAnnoSize.Text = annoh.Value;
                var annov = gridline.Element("AnnoInterval");
                txtAnnoInterval.Text = annov.Value;


                txtNumSize.Text = gridline.Element("NumSize").Value;
                NumInterval.Text = gridline.Element("NumInterval").Value;
            }
        }
        //设置保存参数
        private void SaveParams()
        {
           
            var gridline = new XElement("GridLine");
            //1
            var meaturegrid = new XElement("MeatureGrid");
            meaturegrid.Add(new XElement("X", txtMeatureX.Text));
            meaturegrid.Add(new XElement("Y", txtMeatureY.Text));
            if (GridName == "方里网")
                meaturegrid.SetAttributeValue("checked", true);
            else
                meaturegrid.SetAttributeValue("checked", false);
            gridline.Add(meaturegrid);
            //2
            var graticulegrid = new XElement("GraticuleGrid");
            string lo = txtlongDe.Text + "°" + txtlongMin.Text + "′" + txtlongSe.Text + "″";
            string la = txtlatDe.Text + "°" + txtlatMin.Text + "′" + txtlatSe.Text + "″";
            graticulegrid.Add(new XElement("Longitude", lo));
            graticulegrid.Add(new XElement("Latitude", la));
            if (GridName == "经纬网")
                graticulegrid.SetAttributeValue("checked", true);
            else
                graticulegrid.SetAttributeValue("checked", false);
            gridline.Add(graticulegrid);
            //3
            var indexgrid =  new XElement("IndexGrid");
            indexgrid.Add(new XElement("Row", txtRow.Text));
            indexgrid.Add(new XElement("Column", txtColumn.Text));
            if (GridName == "索引网")
                indexgrid.SetAttributeValue("checked", true);
            else
                indexgrid.SetAttributeValue("checked", false);
            gridline.Add(indexgrid);
            //anno
            var annoh = new XElement("AnnoSize", txtAnnoSize.Text);
            var annov = new XElement("AnnoInterval", txtAnnoInterval.Text);
            gridline.Add(annoh);
            gridline.Add(annov);
            //格网线
            var numsize = new XElement("NumSize", txtAnnoSize.Text);
            var numinterval = new XElement("NumInterval", txtAnnoInterval.Text);
            gridline.Add(numsize);
            gridline.Add(numinterval);
            ExpertiseParamsClass.UpdateGridLine(app, gridline);
        }
        private void btOK_Click(object sender, EventArgs e)
        {
            #region 输入验证
            ValidateUtil valid = new ValidateUtil();
            if (!valid.TraversalTextBox(this.Controls))
            {
                return;
            }
            #endregion    
            double anSize, anInterval;
            double dx, dy;
            anSize = double.Parse(txtAnnoSize.Text);
            anInterval = double.Parse(txtAnnoInterval.Text);
            try
            {
                switch (GridName)
                {
                    case "方里网":
                        dx = double.Parse(txtMeatureX.Text);
                        dy = double.Parse(txtMeatureY.Text);
                        break;
                    case "索引网":
                        int column = int.Parse(txtColumn.Text);
                        column = (column > 10) ? 10 : column;
                        dy = Convert.ToDouble(column);
                        int row = int.Parse(txtRow.Text);
                        row = (row > 10) ? 10 : row;
                        dx = Convert.ToDouble(row);
                        anSize = double.Parse(txtNumSize.Text);
                        anInterval = double.Parse(NumInterval.Text);
                        break;
                    default://经纬网
                        dx = Convert.ToDouble(txtlongDe.Text) + Convert.ToDouble(txtlongMin.Text) / 60 + Convert.ToDouble(txtlongSe.Text) / 3600;
                        dy = Convert.ToDouble(txtlatDe.Text) + Convert.ToDouble(txtlatMin.Text) / 60 + Convert.ToDouble(txtlatSe.Text) / 3600;
                        break;
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
                return;
            }
            string gdb = "GRIDLINE";
            if (!isClipGeoOut())
            {
                MessageBox.Show("请先生成图廓线！");
                return;
            }
            using (WaitOperation wo = app.SetBusy())
            {
                app.EngineEditor.StartOperation();
                MapLayoutHelperLH mh = new MapLayoutHelperLH(gdb);
                mh.CreateGridLine(GridName, dx, dy, anSize, anInterval, gdb);
                app.ActiveView.Refresh();
                SaveParams();
                app.EngineEditor.StopOperation("格网线生成");
                wo.SetText("格网线生成 成功");
            }
            Close();
            Marshal.ReleaseComObject(pageGeo);
            ruleDatatable.Dispose();
            ruleDatatable = null;
        }     
      
        private void btCancel_Click(object sender, EventArgs e)
        {            
            
            Close();
        }
       
        private void radio_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb.Checked)
            {
                GridName = rb.Text;
            }
            switch(rb.Text)
            {
                case "方里网":
                    tabCtrlSet.SelectedIndex = 0;
                    groupBox3.Enabled = true;
                    break;
                case "经纬网":
                    tabCtrlSet.SelectedIndex = 1;
                    groupBox3.Enabled = true;
                    break;
                case "索引网":
                    tabCtrlSet.SelectedIndex = 2;
                    groupBox3.Enabled = false;
                    break;
                default:
                    tabCtrlSet.SelectedIndex = 2;
                    break;
            }
        }

        private bool isClipGeoOut()
        {
            var lyr = app.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LLINE").FirstOrDefault();

            IQueryFilter qf = new QueryFilterClass();

            IFeature fe;
            IFeatureCursor cursor = null;
            //图名
            qf.WhereClause = "TYPE = '内图廓'";
            cursor = (lyr as IFeatureLayer).Search(qf, false);

            fe = cursor.NextFeature();
            if (fe != null)
            {
                return true;
            }
            return false;
        }

        private void btn_LastParas_Click(object sender, EventArgs e)
        {
            string planStr = @"专家库\经验方案\经验方案.xml";
            string fileName = app.Template.Root + @"\" + planStr;
            LoadGridLineOutParams(fileName);
        }

        private void cmbMapType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectItemStr = cmbMapType.SelectedItem.ToString();
            if (selectItemStr == "通用")
            {
                radiameature.Checked = true;
                txtlongDe.Text = "0";
                txtlongMin.Text = "4";
                txtlongSe.Text = "0";
                txtlatDe.Text = "0";
                txtlatMin.Text = "5";
                txtlatSe.Text = "0";
            }
            else
            {
                radiagraducal.Checked = true;
                string fileName = GApplication.Application.Template.Root + @"\专家库\纸张整饰参数\GridLine.xml";
               
                XDocument doc;
             
                {
                    doc = XDocument.Load(fileName);
                    var content = doc.Element("Template").Element("Content");
                    var groups = content.Elements("Group");
                    var group = (from item in groups where item.FirstAttribute.Value == selectItemStr select item).First();
                    if (group != null)
                    {
                        string[] longitude = group.Element("Longitude").Value.Split(new char[] { '°', '′', '″' }, StringSplitOptions.RemoveEmptyEntries);
                        txtlongDe.Text = longitude[0];
                        txtlongMin.Text = longitude[1];
                        txtlongSe.Text = longitude[2];
                        string[] latitude = group.Element("Latitude").Value.Split(new char[] { '°', '′', '″' }, StringSplitOptions.RemoveEmptyEntries);
                        txtlatDe.Text = latitude[0];
                        txtlatMin.Text = latitude[1];
                        txtlatSe.Text = latitude[2];
                    }
                }
            }
        }

     
    }
}
