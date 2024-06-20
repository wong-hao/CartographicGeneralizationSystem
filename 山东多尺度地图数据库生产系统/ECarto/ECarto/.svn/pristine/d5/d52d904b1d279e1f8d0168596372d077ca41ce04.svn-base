using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using SMGI.Common;
using System.IO;
using System.Xml.Linq;
using ESRI.ArcGIS.Geodatabase;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class FrmFootBorder : Form
    {
         
        readonly GApplication _app;
        List<string> _imgname;
        //外部 花边
        List<LaceParam> laceList = new List<LaceParam>();

        public List<LaceParam> LaceList
        {
            get { return laceList; }
            set {
                laceList = value;
                foreach (ListViewItem lv in listView1.Items)
                {
                    lv.Selected = false;
                    if (lv.Text == laceList[0].Name)
                    {
                        lv.Selected = true;
                    }
                    
                }

            }
        }
        //外部 花边角
        LaceParam cornerLace = null;

        public LaceParam CornerLace
        {
            get { return cornerLace; }
            set {
                cornerLace = value;
                foreach (ListViewItem lv in listView1.Items)
                {
                    if (lv.Text == cornerLace.Name)
                    {
                        lv.Selected = true;
                    }

                }
            }
        }
        private const int PicSize = 52;

        private int _curElemIdx = 0;//-1为Corner
        //花边宽度 外部
        private double borderWidth;
        public double BorderWidth
        {
            get 
            {  
                double r = 0;
                double.TryParse(inWidth.Text, out r);
                borderWidth = r;
                return borderWidth;
            } 
            set {
                borderWidth = value;
                inWidth.Text = borderWidth.ToString();
            }
        }
        private double borderStep;
        //间距 外部
        public double BorderStep
        {
            get
            {
                double r = 0;
                double.TryParse(txtInterval.Text, out r);
                borderStep = r;
                return borderStep;
            }
            set { 
                borderStep = value;
                txtInterval.Text = borderStep.ToString();
            }
        }
        bool oneKey = false;//是否一键图
        public FrmFootBorder(bool flag = false)
        {
            InitializeComponent();
            _app = GApplication.Application;
            oneKey = flag;
            Init();
            btn_Preview.Visible = !flag;
        }
        private void Init()
        {
            picCornerLace.Click += new EventHandler(picCornerLace_Click);
            btn_del.Enabled = false;
            imageList1.ImageSize = new Size(PicSize, PicSize);


            var firstPic = (PictureBox)pnl_lace.Controls[0].Controls[0];
            firstPic.Click += picbox_Click;

            string mdbPath = _app.Template.Root + "\\整饰\\花边";
            var di = new DirectoryInfo(mdbPath);
            _imgname = new List<string>();
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Name.ToUpper().EndsWith(".BMP") || fi.Name.ToUpper().EndsWith(".PNG"))
                {
                    imageList1.Images.Add(Image.FromFile(fi.FullName));
                    string[] temp = fi.Name.Split('.');
                    _imgname.Add(temp[0]);
                }
            }

            for (int i = 0; i < _imgname.Count; i++)
            {
                var lvi = new ListViewItem(_imgname[i]) { ImageIndex = i };
                listView1.Items.Add(lvi);
            }


            firstPic.Image = (Image)imageList1.Images[0].Clone();

            cornerLace = new LaceParam { Angle = 0, Flip = false, Name = _imgname[0] + "角" };
            laceList = new List<LaceParam> { new LaceParam { Angle = 0, Flip = false, Name = _imgname[0] } };
            picCornerLace.Image = (Image)imageList1.Images[0].Clone();


            listView1.Items[0].Selected = true;
            cmb_Ang.SelectedIndex = 0;

            var maplaceItem = GetNearestSize();
            if (maplaceItem != null)
            {

                inWidth.Text = maplaceItem["花边宽度"].ToString();
                if (maplaceItem["花边与外图廓间距"] != DBNull.Value)
                    txtInterval.Text = maplaceItem["花边与外图廓间距"].ToString();

            }
            isClipLineOut();
        }
        double MapLineHeight = 0;
        //判断是否生成内外图廓
        private bool isClipLineOut()
        {
            MapLineHeight = 0;
            if (oneKey)
            {
                MapLineHeight = CommonMethods.InOutLineWidth;
                lbMsg.Text = "内外图廓间距：" + Math.Round(MapLineHeight, 2) + " mm";
            }

            IQueryFilter qf = new QueryFilterClass();

            try
            {
                var lyr = _app.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LLINE").FirstOrDefault();
                IFeature fe;
                IFeatureCursor cursor = null;
                qf.WhereClause = "TYPE = '外图廓'";
                cursor = (lyr as IFeatureLayer).Search(qf, false);
                fe = cursor.NextFeature();
                ESRI.ArcGIS.Geometry.IEnvelope enOut = fe.Shape.Envelope;
                //图名
                qf.WhereClause = "TYPE = '内图廓'";
                cursor = (lyr as IFeatureLayer).Search(qf, false);
                fe = cursor.NextFeature();
                ESRI.ArcGIS.Geometry.IEnvelope enIn = fe.Shape.Envelope;
                Marshal.ReleaseComObject(cursor);
                MapLineHeight = (enOut.Height - enIn.Height) / GApplication.Application.MapControl.Map.ReferenceScale * 1000;
                MapLineHeight = MapLineHeight * 0.5;
                lbMsg.Text = "内外图廓间距：" + Math.Round(MapLineHeight, 2) + " mm,";

                double width = 0;
                double.TryParse(inWidth.Text, out width);
                double step = 0;//花边与外图廓间距
                double.TryParse(txtInterval.Text, out step);
                if (width + step > MapLineHeight)
                {
                    //重新修改参数
                    inWidth.Text = Math.Round(MapLineHeight * 0.8, 1).ToString();
                    txtInterval.Text = Math.Round(MapLineHeight * 0.1, 1).ToString();
                }

                if (fe != null)
                {

                    Marshal.ReleaseComObject(fe);
                    return true;
                }

            }
            catch
            {
                return false;
            }
            return false;
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
            if (!oneKey)
            {
                if (!isClipGeoOut())
                {
                    MessageBox.Show("请先生成图廓线！");
                    return;
                }
            }
            
            double lenth = Convert.ToDouble(inWidth.Text);
            double len = 0;//花边与外图廓间距
            double.TryParse(txtInterval.Text, out len);
            if (lenth + len > MapLineHeight)
            {
                MessageBox.Show("花边宽度和花边与外图廓间距之和不能大于内外图廓间距!");
                return;
            }
            Apply();
            SaveParms();
            DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void btn_Preview_Click(object sender, EventArgs e)
        {
            if (!isClipGeoOut() && !oneKey)
            {
                MessageBox.Show("请先生成图廓线！");
                return;
            }
            Apply();
            SaveParms();
        }
        //保存参数
        private void SaveParms()
        {

            var LaceBorder = new XElement("LaceBorder");
            LaceBorder.Add(new XElement("Width",inWidth.Text));
            LaceBorder.Add(new XElement("Interval", txtInterval.Text));

            var border = new XElement("Border");
            foreach(var lace in laceList)
            {
                var item = new XElement("Item");
                item.Add(new XElement("Length", lace.Length));
                item.Add(new XElement("Angle", lace.Angle));
                item.Add( new XElement("Flip", lace.Flip));
                item.Add(new XElement("Name", lace.Name));
                border.Add(item);
            }
            LaceBorder.Add(border);
            var corner = new XElement("Corner");
            var item1 = new XElement("Item");
            item1.Add(new XElement("Length", cornerLace.Length));
            item1.Add(new XElement("Angle", cornerLace.Angle));
            item1.Add(new XElement("Flip", cornerLace.Flip));
            item1.Add(new XElement("Name", cornerLace.Name));
            corner.Add(item1);
            LaceBorder.Add(corner);

            ExpertiseParamsClass.UpdateLaceBorder(_app, LaceBorder);

        }

        private void Apply()
        {
            //花边宽度可以为0
            double lenth = Convert.ToDouble(inWidth.Text);
            double len = 0;//花边与外图廓间距
            double.TryParse(txtInterval.Text, out len);
            if (lenth + len > MapLineHeight)
            {
                MessageBox.Show("花边宽度和花边与外图廓间距之和不能大于内外图廓间距!");
                return;
            }
            if (!oneKey && lenth > 0)
            {
                var wo = _app.SetBusy();
                _app.EngineEditor.StartOperation();


                if (listView1.SelectedItems.Count == 0)
                    return;
                //花边
                var GDBname = "LACE";
                var mh = new MapLayoutHelperLH("LACE");

                mh.CreateFootBorder2(laceList, cornerLace, lenth, len, GDBname);
                _app.EngineEditor.StopOperation("花边设置");
                wo.SetText("花边生成完成");
                wo.Dispose();
                GC.Collect();
                _app.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            }
        }

        //初始化
        private void FrmFootBorder_Load(object sender, EventArgs e)
        {
           
          
        }
        //获取最近的【纸张开本】
        ESRI.ArcGIS.Geometry.IPolygon pageGeo = null;
        DataTable ruleDatatable = null;
        private DataRow GetNearestSize()
        {
            double width = 0;
            double height = 0;
            DataRow itemSel = null;
            try
            {
                if (pageGeo == null)
                {
                    if (_app.Workspace != null)
                    {
                        var lyr = _app.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l => { return (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToLower() == "clipboundary"); })).First();
                        if (lyr != null)
                        {
                            #region
                            IFeatureClass clipfcl = (lyr as IFeatureLayer).FeatureClass;
                            if (clipfcl.FeatureCount(null) != 0)
                            {
                                IFeature fe = null;
                                IQueryFilter qf = new QueryFilterClass();
                                qf.WhereClause = "TYPE = '页面'";
                                IFeatureCursor cursor = clipfcl.Search(qf, false);
                                fe = cursor.NextFeature();
                                pageGeo = fe.ShapeCopy as ESRI.ArcGIS.Geometry.IPolygon;
                                Marshal.ReleaseComObject(cursor);
                                Marshal.ReleaseComObject(qf);
                                Marshal.ReleaseComObject(fe);
                                double ms = _app.ActiveView.FocusMap.ReferenceScale;
                                width = pageGeo.Envelope.Width / ms * 1000; //毫米
                                height = pageGeo.Envelope.Height / ms * 1000;//毫米
                            }
                            #endregion
                        }
                    }
                }
                if (width == 0)
                {
                    var paramContent = EnvironmentSettings.getContentElement(GApplication.Application);
                    var pagesize = paramContent.Element("PageSize");//页面大小
                    width = double.Parse(pagesize.Element("Width").Value);
                    height = double.Parse(pagesize.Element("Height").Value);
                }

                double min = Math.Min(width, height);
                double max = Math.Max(width, height);
                string filemdb = _app.Template.Root + @"\专家库\纸张整饰参数\整饰参数经验值.mdb";
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
        private void LoadLacesOutParams(string fileName)
        {
            //string planStr = @"专家库\经验方案\经验方案.xml";
            //string fileName = _app.Template.Root + @"\" + planStr;
          
            XDocument doc;
           
            {
                doc = XDocument.Load(fileName);

                var content = doc.Element("Template").Element("Content");
                var mapborder = content.Element("LaceBorder");
                inWidth.Text = mapborder.Element("Width").Value;
                txtInterval.Text = mapborder.Element("Interval").Value;
                var borders = mapborder.Element("Border").Elements("Item");
                laceList.Clear();
                pnl_lace.Controls.RemoveAt(0);
                foreach (var item in borders)
                {
                   int angle = int.Parse(item.Element("Angle").Value);
                   bool flip = bool.Parse(item.Element("Flip").Value);
                   string name = item.Element("Name").Value;
                   var param = new LaceParam { Angle = angle, Flip = flip, Name = name };
                   laceList.Add(param);
                   #region
                   for (int i = 0; i < _imgname.Count; i++)
                   {
                       if (_imgname[i] != name)
                       {
                           continue;
                       }
                       var picbox = new PictureBox { Image =  (Image)imageList1.Images[i].Clone()};
                       picbox.Width = PicSize;
                       picbox.Height = PicSize;
                       var n = pnl_lace.Controls.Count;

                       picbox.Cursor = Cursors.Hand;
                       picbox.Location = new Point(0, 4);
                       picbox.Click += (picbox_Click);
                       var panl = new Panel();
                       panl.Controls.Add(picbox);
                       panl.Location = new Point(n * PicSize + 3, 0);
                       panl.Height = PicSize + 10;
                       panl.Width = PicSize;
                       panl.BackColor = SystemColors.MenuHighlight;
                       pnl_lace.Controls.Add(panl);

                       if (_curElemIdx == -1)
                           picCornerLace.Parent.BackColor = SystemColors.Control;
                       else
                           pnl_lace.Controls[_curElemIdx].BackColor = SystemColors.Control;

                       _curElemIdx = n;
                       if (pnl_lace.Controls.Count > 1)
                           btn_del.Enabled = true;
                   }
                   #endregion
                }
                var corners = mapborder.Element("Corner").Elements("Item");
                foreach (var item in corners)
                {
                    int angle = int.Parse(item.Element("Angle").Value);
                    bool flip = bool.Parse(item.Element("Flip").Value);
                    string name = item.Element("Name").Value;
                    cornerLace = new LaceParam { Angle = angle, Flip = flip, Name = name };
                    #region
                    for (int i = 0; i < _imgname.Count; i++)
                    {
                        if (_imgname[i] != name)
                        {
                            continue;
                        }
                        picCornerLace.Image = (Image)imageList1.Images[i].Clone();
                    }
                    #endregion
                }
            }
        }
        //初始化花边参数
        private void InitsMapLinePars()
        {
            string fileName = _app.Template.Root + @"\专家库\MapMargin.xml";
            

             
            {
                XDocument doc = XDocument.Load(fileName);

                XElement xelement = doc.Element("MapMargin").Element("Content").Element("MapLine");
   

                xelement = doc.Element("MapMargin").Element("Content").Element("FootBorder");
                inWidth.Text = (xelement.Element("FootBorderWidth").Value);
                txtInterval.Text = (xelement.Element("FootBorderSpacing").Value);//FootBorderWidth   FootBorderSpacing


                //laceList[0].Angle = int.Parse(xelement.Element("Angle").Value);
                //laceList[0].Flip = int.Parse(xelement.Element("Flip").Value) == 0;
                //laceList[0].Name = xelement.Element("ruleName").Value;

                //cornerLace.Angle = int.Parse(xelement.Element("CornerAngle").Value);
                //cornerLace.Flip = int.Parse(xelement.Element("CornerFlip").Value) == 0;
                //cornerLace.Name = xelement.Element("CornerName").Value;

            }
        }



        #region 其它事件
        //添加组合花边
        private void button1_Click(object sender, EventArgs e)
        {
            var img = imageList1.Images[0];
            var picbox = new PictureBox { Image = (Image)img.Clone() };
            picbox.Width = PicSize;
            picbox.Height = PicSize;
            var n = pnl_lace.Controls.Count;

            picbox.Cursor = Cursors.Hand;
            picbox.Location = new Point(0, 4);
            picbox.Click += (picbox_Click);
            var panl = new Panel();
            panl.Controls.Add(picbox);
            panl.Location = new Point(n * PicSize + 3, 0);
            panl.Height = PicSize+10;
            panl.Width = PicSize;
            panl.BackColor = SystemColors.MenuHighlight;
            pnl_lace.Controls.Add(panl);

            if (_curElemIdx == -1)
                picCornerLace.Parent.BackColor = SystemColors.Control;
            else
                pnl_lace.Controls[_curElemIdx].BackColor = SystemColors.Control;

            _curElemIdx = n;
            if (pnl_lace.Controls.Count > 1)
                btn_del.Enabled = true;

            var lace = new LaceParam() { Angle = 0, Flip = false, Length = 0, Name = listView1.Items[0].Text };
            laceList.Add(lace);

            listView1.Items[0].Selected = true;
            chk_Filp.Checked = false;
            cmb_Ang.SelectedIndex = 0;
            txtinLen.Text = "0";

        }
        //删除组合花边
        private void button2_Click(object sender, EventArgs e)
        {

            if (pnl_lace.Controls.Count == 1) return;

            int i = 0;
            if (_curElemIdx != -1)
                i = _curElemIdx;
            else
                return;

            //if (i == pnl_lace.Controls.Count)//没有选择就最后一个
            //    i = pnl_lace.Controls.Count - 1;

            var pic = (PictureBox)pnl_lace.Controls[i].Controls[0];

            pic.Click -= picbox_Click;
            pnl_lace.Controls.RemoveAt(i);
            laceList.RemoveAt(i);

            if (i == pnl_lace.Controls.Count)
                _curElemIdx = i - 1;
            else _curElemIdx = i;
            for (var j = i; j < pnl_lace.Controls.Count; j++)
            {
                var pt = pnl_lace.Controls[j].Location;
                pt.Offset(-PicSize, 0);
                pnl_lace.Controls[j].Location = pt;
            }


            pnl_lace.Controls[_curElemIdx].BackColor = SystemColors.MenuHighlight;


            if (pnl_lace.Controls.Count == 1)//不能删
            {
                btn_del.Enabled = false;
            }
        }


        //花边角 点击事件
        private void picCornerLace_Click(object obj, EventArgs args)
        {
            if (_curElemIdx == -1) return;

            pnl_lace.Controls[_curElemIdx].BackColor = SystemColors.Control;
            picCornerLace.Parent.BackColor = SystemColors.MenuHighlight;

            _curElemIdx = -1;

            var idx = GetImgIdx(cornerLace.Name);
            listView1.Items[idx].Selected = true;

            chk_Filp.Checked = cornerLace.Flip;
            txtinLen.Text = cornerLace.Length.ToString();
            cmb_Ang.SelectedIndex = cornerLace.Angle / 90;

            
        }
        //点击图片
        private void picbox_Click(object obj, EventArgs args)
        {
            var pic = (PictureBox)obj;
            var ix = pic.Parent.Left / PicSize;

            if (ix == _curElemIdx) return;
            if (_curElemIdx == -1)
                picCornerLace.Parent.BackColor = SystemColors.Control;
            else
                pnl_lace.Controls[_curElemIdx].BackColor = SystemColors.Control;
            pic.Parent.BackColor = SystemColors.MenuHighlight;
            _curElemIdx = ix;

            var name = laceList[_curElemIdx].Name;
            //库索引
            var idx = GetImgIdx(name);
            listView1.Items[idx].Selected = true;

            
            var lace = laceList[_curElemIdx];
            chk_Filp.Checked = lace.Flip;
            txtinLen.Text = lace.Length.ToString();
            cmb_Ang.SelectedIndex = lace.Angle / 90;



        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

            foreach (ListViewItem lv in listView1.Items)
            {
                lv.BackColor = SystemColors.Window;
                lv.ForeColor = Color.Black;
            }
            foreach (ListViewItem lv in listView1.SelectedItems)
            {
                lv.BackColor = SystemColors.MenuHighlight;
                lv.ForeColor = Color.White;
            }
            if (listView1.SelectedItems.Count == 0)
                return;
            // MessageBox.Show(listView1.SelectedItems.Count.ToString( ));
            if (_curElemIdx == -1)
                cornerLace.Name = listView1.SelectedItems[0].Text;
            else
            {
                laceList[_curElemIdx].Name = listView1.SelectedItems[0].Text;

                var idx = GetImgIdx(laceList[_curElemIdx].Name+"角");
                if (idx != -1)
                {
                    cornerLace.Name = laceList[_curElemIdx].Name + "角";
                    cornerLace.Angle = 0;
                    cornerLace.Flip = false;
                     
                    var img = (Image)imageList1.Images[idx].Clone();
                    picCornerLace.Image = img;
                }
            }
            //修改符号

            RotatePicture();

        }


        private void label13_MouseEnter(object sender, EventArgs e)
        {
            lbl_tips.Visible = true;
        }

        private void label13_MouseLeave(object sender, EventArgs e)
        {
            lbl_tips.Visible = false;
        }


        private void txtinLen_TextChanged(object sender, EventArgs e)
        {

            var val = txtinLen.Text.Trim();
            if (val == "" || val == "0")
            {
                if (_curElemIdx == -1)
                    cornerLace.Length = 0;
                else
                    laceList[_curElemIdx].Length = 0;

                return;
            }
            if (_curElemIdx == -1)
                cornerLace.Length = double.Parse(val);
            else
                laceList[_curElemIdx].Length = double.Parse(val);

        }

        private void cmb_Ang_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (curLace == null) return;
            if (_curElemIdx == -1)
                cornerLace.Angle = int.Parse(cmb_Ang.SelectedItem.ToString());
            else
                laceList[_curElemIdx].Angle = int.Parse(cmb_Ang.SelectedItem.ToString());

            //图片也需旋转 curpic
            RotatePicture();
        }

        private void chk_Filp_CheckedChanged(object sender, EventArgs e)
        {
            //if (curLace == null) return;
            if (_curElemIdx == -1)
                cornerLace.Flip = chk_Filp.Checked;
            else
                laceList[_curElemIdx].Flip = chk_Filp.Checked;

            RotatePicture();
        }


        private void btCancel_Click(object sender, EventArgs e)
        {
            Hide();
        }
        #endregion

        #region 功能

        //获取界面图元序号
        private int GetImgIdx(string name)
        {
            for (var i = 0; i < _imgname.Count; i++)
            {
                if (_imgname[i] == name) return i;
            }
            return -1;
        }

        private void RotatePicture()
        {
           //初始化时可能没值
            if (cmb_Ang.SelectedItem == null)
            {
                return;
               
            }
            var ag = cmb_Ang.SelectedItem.ToString();
            ag = ag == "0" ? "RotateNone" : "Rotate" + ag;
            var filp = chk_Filp.Checked ? "FlipX" : "FlipNone";

            var tpy = ag + filp;
            //if (tpy == "RotateNoneFlipNone")

            LaceParam lace;
            PictureBox pic;

            if (_curElemIdx == -1)
            {
                pic = picCornerLace;
                lace = cornerLace;
            }
            else
            {
                lace = laceList[_curElemIdx];
                pic = (PictureBox)pnl_lace.Controls[_curElemIdx].Controls[0];
            }

            var idx = GetImgIdx(lace.Name);
            var img = (Image)imageList1.Images[idx].Clone();

            img.RotateFlip((RotateFlipType)Enum.Parse(typeof(RotateFlipType), tpy));
            pic.Image = img;
        }

        //判断是否生成内外图廓
        private bool isClipGeoOut()
        {
            var lyr = _app.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && (l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LLINE").FirstOrDefault();

            IQueryFilter qf = new QueryFilterClass();

            IFeature fe;
            IFeatureCursor cursor = null;
            //图名
            qf.WhereClause = "TYPE = '内图廓'";
            cursor = (lyr as IFeatureLayer).Search(qf, false);
            fe = cursor.NextFeature();
            Marshal.ReleaseComObject(cursor);
            if (fe != null)
            {
                Marshal.ReleaseComObject(fe);
                return true;
            }
            return false;
        }

        #endregion

        private void btn_LastParas_Click(object sender, EventArgs e)
        {
            string planStr = @"专家库\经验方案\经验方案.xml";
            string fileName = _app.Template.Root + @"\" + planStr;
            LoadLacesOutParams(fileName);
        }

     
      

    }
}
