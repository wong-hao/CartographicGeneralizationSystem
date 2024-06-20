using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using System.Xml.Linq;
namespace SMGI.Plugin.EmergencyMap
{
    public partial class RegionNameLocationForm : Form
    {
        GApplication _app;
        List<IFeature> _result;

        string _sdeAddress = "172.16.1.50";
        string _userName = "sde";
        string _password = "p@ssw0rd";

        string _regionNameDatabase = "dcdmeta";
        string _regionNameLayerName = "AGNP";
        string _regionNameFieldName = "NAME";

        List<string> BOUAList = new List<string>();
        string _regionNameProvince = "";
        string _regionNameCity = "";
        string _regionNameCountry = "";
        string _regionNameTown = "";
        IFeatureClass FclBOUA2 = null;
        IFeatureClass FclBOUA4 = null;
        IFeatureClass FclBOUA5 = null;
        IFeatureClass FclBOUA6 = null;

        public double Dx;//偏移
        public double Dy;//偏移
        IGroupElement _oldClipGroupElement = null;

        /// <summary>
        /// 裁切中心点
        /// </summary>
        private IPolygon m_GeoPlygon;
        public IPolygon GeoPlygon
        {
            get
            {
                return m_GeoPlygon;
            }
            set
            {
                m_GeoPlygon = value;

              
            }
        }

        /// <summary>
        /// 比例尺
        /// </summary>
        public double RefScale
        {
            get
            {
                return double.Parse(cbRefSclae.Text);
            }
        }

        /// <summary>
        /// 纸张尺寸
        /// </summary>
        public PaperSize ClipPaperSize
        {
            get
            {
                return new PaperSize { PaperWidth = double.Parse(tbPaperWidth.Text), PaperHeight = double.Parse(tbPaperHeight.Text) };

            }
        }

        bool _scaleChangedState;//由于比例尺的变化会触发纸张大小的更新，通过这个变量来保证该事件触发纸张的变换不会导致重新更新比例尺


        public RegionNameLocationForm(GApplication app)
        {
            InitializeComponent();

            _app = app;

            _result = new List<IFeature>();
            m_GeoPlygon = null;

            //根据环境设置，更新控件值
            var content = EnvironmentSettings.getContentElement(_app);
           // var service = content.Element("Server");
            var pagesize = content.Element("PageSize");
            var mapScale = content.Element("MapScale");

            cbRefSclae.Text = mapScale.Value;
            tbPaperWidth.Text = pagesize.Element("Width").Value;
            tbPaperHeight.Text = pagesize.Element("Height").Value;

            #region 服务器参数
            var setFileName = "LocationSearch.xml";
            string fileName = app.Template.Root + @"\专家库\数据定位\" + setFileName;
            
          
            {
                XDocument doc = XDocument.Load(fileName);

                var eles = doc.Element("Template").Element("Content").Elements("Server");
                foreach (var ele in eles)
                {
                    if (ele.Attribute("name").Value == "行政区定位")
                    {
                        var service = ele;
                        _sdeAddress = service.Element("IPAddress").Value;
                        _userName = service.Element("UserName").Value;
                        _password = service.Element("Password").Value;
                        _regionNameDatabase = service.Element("DataBase").Value;
                        XElement[] Bouax=service.Elements("Lyr").ToArray();
                        foreach (var x in Bouax)
                        {
                            BOUAList.Add(x.Value);
                        }
                        _regionNameProvince = BOUAList[0];
                        _regionNameCity = BOUAList[1];
                        _regionNameCountry = BOUAList[2];
                        _regionNameTown = BOUAList[3];
                        _regionNameFieldName = service.Element("Field").Value;
                        break;
                    }
                }
            }
            #endregion


            double width = Convert.ToDouble(tbPaperWidth.Text);
            double height = Convert.ToDouble(tbPaperHeight.Text);

            //纸张尺寸
            List<PaperInfo> pis = PageSizeRules.getPageSizeInfos(app);
            int index = -1;
            for (int i = 0; i < pis.Count; ++i)
            {
                cbPaperSize.Items.Add(pis[i]);
                if (pis[i].Width == width && pis[i].Height == height)
                {
                    index = i;
                }
            }

            cbPaperSize.SelectedIndex = index;
            if (-1 == index)
            {
                cbPaperSize.Text = "自定义尺寸";
            }

            
            _oldClipGroupElement = ClipElement.getClipGroupElement(_app.MapControl.ActiveView.GraphicsContainer);
        }

        private void RegionNameLocationFormJL_Load(object sender, EventArgs e)
        {
            if (_app.Template.Root.Contains("江苏"))
            {
                if (dgResult.Columns.Contains("City"))
                {
                    dgResult.Columns["City"].HeaderText = "所属设市区";
                }
            }
        }

        private void tbPlaceName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnResearch.PerformClick();
            }
        }

        
        private void btnResearch_Click(object sender, EventArgs e)              //
        {
            if (tbPlaceName.Text.Trim() == "")
            {
                MessageBox.Show("请输入搜索关键字！");
                return;
            }
            btnResearch.Enabled = false;

            _result = new List<IFeature>();
            m_GeoPlygon = null;

            IWorkspace sdeWorkspace = _app.GetWorkspacWithSDEConnection(_sdeAddress, _userName, _password, _regionNameDatabase);
            if (null == sdeWorkspace)
            {
                MessageBox.Show("无法访问服务器！");
                return;
            }

            if (!(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _regionNameProvince) && !(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _regionNameCity) && !(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _regionNameCountry) && !(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _regionNameTown))
            {
                MessageBox.Show("无法访问服务器中的行政区库！");
                return;
            }
            IFeatureWorkspace sdeFeatureWorkspace = sdeWorkspace as IFeatureWorkspace;
            FclBOUA2 = sdeFeatureWorkspace.OpenFeatureClass(_regionNameProvince);
            if (FclBOUA2 == null)
            {
                return;
            }
            search(FclBOUA2, _regionNameFieldName, tbPlaceName.Text);

            FclBOUA4 = sdeFeatureWorkspace.OpenFeatureClass(_regionNameCity);
            if (FclBOUA4 == null)
            {
                return;
            }
            search(FclBOUA4, _regionNameFieldName, tbPlaceName.Text);

            FclBOUA5 = sdeFeatureWorkspace.OpenFeatureClass(_regionNameCountry);
            if (FclBOUA5 == null)
            {
                return;
            }
            search(FclBOUA5, _regionNameFieldName, tbPlaceName.Text);

            FclBOUA6 = sdeFeatureWorkspace.OpenFeatureClass(_regionNameTown);
            if (FclBOUA6 == null)
            {
                return;
            }
            search(FclBOUA6, _regionNameFieldName, tbPlaceName.Text);


            updateResultTable();

            btnResearch.Enabled = true;
        }

        private void dgResult_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            DataGridViewLinkColumn placeNameColumn = dgResult.Columns[e.ColumnIndex] as DataGridViewLinkColumn;
            if (null == placeNameColumn || placeNameColumn.Name != "PlaceName")
                return;
            int index = int.Parse(dgResult.Rows[e.RowIndex].HeaderCell.Value.ToString());
            IFeature f = _result[e.RowIndex];
            //设置目标空间坐标系
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (null == targetSpatialReference)
                targetSpatialReference = _app.MapControl.Map.SpatialReference;
            m_GeoPlygon = f.ShapeCopy as IPolygon;
            if (m_GeoPlygon.SpatialReference.Name != targetSpatialReference.Name)
            {
                (m_GeoPlygon as IGeometry).Project(targetSpatialReference);//对定位几何进行投影变换
            }
            objName = dgResult.Rows[e.RowIndex].Cells["PlaceName"].Value.ToString();
            objPac = f.get_Value(f.Fields.FindField(pacFN)).ToString();
            m_Timer.Stop();
            //内图廓宽度
            double InlineWidth;
            bool res;
            res = double.TryParse(tbInlineWidth.Text, out InlineWidth);
            if (!res || InlineWidth <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸宽度不合法！");
                return;
            }

            //内图廓高度
            double InlineHeight;
            res = double.TryParse(tbInlineHeight.Text, out InlineHeight);
            if (!res || InlineHeight <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }

            double scale = CommonLocationMethods.CaluateMapScale(InlineWidth, InlineHeight, m_GeoPlygon);
            cbRefSclae.Text = scale.ToString();
            double dx = Dx;
            double dy = Dy;
            CommonMethods.CalculateSptialRef(m_GeoPlygon.Envelope);
            //比例尺
            double refScale;
            res = double.TryParse(cbRefSclae.Text, out refScale);
            if (!res || refScale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");
                return;
            }
            //宽度
            double paperWidth;
            res = double.TryParse(tbPaperWidth.Text, out paperWidth);
            if (!res || paperWidth <= 0)
            {
                MessageBox.Show("输入的纸张宽度不合法！");
                return;
            }

            //高度
            double paperHeight;
            res = double.TryParse(tbPaperHeight.Text, out paperHeight);
            if (!res || paperHeight <= 0)
            {
                MessageBox.Show("输入的纸张高度不合法！");
                return;
            }
            
            //成图尺寸宽度
            double MapSizeWidth;
            res = double.TryParse(tbMapSizeWidth.Text, out MapSizeWidth);
            if (!res || MapSizeWidth <= 0)
            {
                MessageBox.Show("输入的成图尺寸宽度不合法！");
                return;
            }
            //成图尺寸高度
            double MapSizeHeight;
            res = double.TryParse(tbMapSizeHeight.Text, out MapSizeHeight);
            if (!res || MapSizeHeight <= 0)
            {
                MessageBox.Show("输入的成图尺寸高度不合法！");
                return;
            }

            //内图廓宽度            
            res = double.TryParse(tbInlineWidth.Text, out InlineWidth);
            if (!res || InlineWidth <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸宽度不合法！");
                return;
            }

            //内图廓高度
          
            res = double.TryParse(tbInlineHeight.Text, out InlineHeight);
            if (!res || InlineHeight <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }

            //内外图廓间距
            double InOutLineWidth;
            res = double.TryParse(tbInOutWidth.Text, out InOutLineWidth);
            if (!res || InOutLineWidth < 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }
            if (paperWidth < MapSizeWidth || paperHeight < MapSizeHeight)
            {
                MessageBox.Show("纸张尺寸不能小于成图尺寸！");
                return;
            }
            if (InlineWidth + InOutLineWidth > MapSizeWidth || InlineHeight + InOutLineWidth > MapSizeHeight)
            {
                MessageBox.Show("成图尺寸不能小于外图廓尺寸！");
                return;
            }
            //存储地图各尺寸
            CommonMethods.SetMapPar(paperWidth, paperHeight, MapSizeWidth, MapSizeHeight, InlineWidth, InlineHeight, InOutLineWidth, refScale);
            IGroupElement clipELe = ClipElement.createClipGroupElementEx2(_app, m_GeoPlygon, InlineWidth, InlineHeight, scale, dx, dy);
            ClipElement.SetClipGroupElement(_app, clipELe);
            CommonMethods.clipEx = true;
        }

        private XElement getMapInnerElement()
        {

            string fileName = _app.Template.Root + @"\专家库\MapMargin.xml";
            

           
            {
                XDocument doc = XDocument.Load(fileName);
                return doc.Element("MapMargin").Element("Content").Element("MapInner");
            }
        }
        bool PaperMosueClick=false;
        private void cbPaperSize_SelectedIndexChanged(object sender, EventArgs e)
        {

            PaperInfo pi = cbPaperSize.SelectedItem as PaperInfo;
            tbPaperWidth.Text = pi.Width.ToString();
            tbPaperHeight.Text = pi.Height.ToString();

            //宽度
            double paperWidth;
            bool res = double.TryParse(tbPaperWidth.Text, out paperWidth);
            if (!res || paperWidth <= 0)
            {
                MessageBox.Show("纸张宽度不合法!");
            }
            //高度
            double paperHeight;
            res = double.TryParse(tbPaperHeight.Text, out paperHeight);
            if (!res || paperHeight <= 0)
            {
                MessageBox.Show("纸张高度不合法!");
            }

            DataRow itemSel = CommonLocationMethods.CaluateMapAndInlineSize(paperWidth, paperHeight);
            #region 成图尺寸、内图廓尺寸、内外图廓间距写入textbox
            if (itemSel != null)
            {
                //横版
                if (paperWidth > paperHeight)
                {
                    tbMapSizeWidth.Text = itemSel["成图尺寸一边"].ToString();
                    tbMapSizeHeight.Text = itemSel["成图尺寸另一边"].ToString();
                    tbInlineWidth.Text = itemSel["横版内图廓尺寸宽"].ToString();
                    tbInlineHeight.Text = itemSel["横版内图廓尺寸高"].ToString();

                }
                //竖版
                else
                {
                    tbMapSizeHeight.Text = itemSel["成图尺寸一边"].ToString();
                    tbMapSizeWidth.Text = itemSel["成图尺寸另一边"].ToString();
                    tbInlineWidth.Text = itemSel["竖版内图廓尺寸宽"].ToString();
                    tbInlineHeight.Text = itemSel["竖版内图廓尺寸高"].ToString();

                }
                tbInOutWidth.Text = itemSel["内外图廓间距"].ToString();

            }
            #endregion
            if (!PaperMosueClick) return;         
            if (m_GeoPlygon == null)
            {
              
                return;
            }
            
            //内图廓宽度
            double InlineWidth;
            res = double.TryParse(tbInlineWidth.Text, out InlineWidth);
            if (!res || InlineWidth <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸宽度不合法！");
                return;
            }

            //内图廓高度
            double InlineHeight;
            res = double.TryParse(tbInlineHeight.Text, out InlineHeight);
            if (!res || InlineHeight <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }
            //设置目标空间坐标系
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (null == targetSpatialReference)
                targetSpatialReference = _app.MapControl.Map.SpatialReference;           
            if (m_GeoPlygon.SpatialReference.Name != targetSpatialReference.Name)
            {
                (m_GeoPlygon as IGeometry).Project(targetSpatialReference);//对定位几何进行投影变换
            }
            if (!_scaleChangedState)//非因手动修改比例尺而导致的纸张变化
            {
                double scale = CommonLocationMethods.CaluateMapScale(InlineWidth, InlineHeight, m_GeoPlygon);
                cbRefSclae.Text = scale.ToString();
            }
            //重置
            _scaleChangedState = false;
            PaperMosueClick = false;


        }

        private void tbWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            //判断按键是不是要输入的类型。
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;

            //小数点的处理。
            if ((int)e.KeyChar == 46) //小数点
            {
                if ((sender as TextBox).Text.Length <= 0)
                {
                    e.Handled = true;   //小数点不能在第一位
                }
                else
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse((sender as TextBox).Text, out oldf);
                    b2 = float.TryParse((sender as TextBox).Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }

            if (!e.Handled)
                cbPaperSize.Text = "自定义尺寸";
        }

        private void tbHeight_KeyPress(object sender, KeyPressEventArgs e)
        {
            //判断按键是不是要输入的类型。
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;

            //小数点的处理。
            if ((int)e.KeyChar == 46) //小数点
            {
                if ((sender as TextBox).Text.Length <= 0)
                {
                    e.Handled = true; //小数点不能在第一位
                }
                else
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse((sender as TextBox).Text, out oldf);
                    b2 = float.TryParse((sender as TextBox).Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }

            if (!e.Handled)
                cbPaperSize.Text = "自定义尺寸";
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            bool res;

            m_Timer.Stop();
            if (_result == null || _result.Count == 0)
            {
                MessageBox.Show("请指定出图目标！");

                return;
            }

            //比例尺
            double refScale;
            res = double.TryParse(cbRefSclae.Text, out refScale);
            if (!res || refScale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");

                return;
            }

            //宽度
            double paperWidth;
            res = double.TryParse(tbPaperWidth.Text, out paperWidth);
            if (!res || paperWidth <= 0)
            {
                MessageBox.Show("输入的纸张宽度不合法！");

                return;
            }

            //高度
            double paperHeight;
            res = double.TryParse(tbPaperHeight.Text, out paperHeight);
            if (!res || paperHeight <= 0)
            {
                MessageBox.Show("输入的纸张高度不合法！");

                return;
            }


            //成图尺寸宽度
            double MapSizeWidth;
            res = double.TryParse(tbMapSizeWidth.Text, out MapSizeWidth);
            if (!res || MapSizeWidth <= 0)
            {
                MessageBox.Show("输入的成图尺寸宽度不合法！");
                return;
            }
            //成图尺寸高度
            double MapSizeHeight;
            res = double.TryParse(tbMapSizeHeight.Text, out MapSizeHeight);
            if (!res || MapSizeHeight <= 0)
            {
                MessageBox.Show("输入的成图尺寸高度不合法！");
                return;
            }

            //内图廓宽度
            double InlineWidth;
            res = double.TryParse(tbInlineWidth.Text, out InlineWidth);
            if (!res || InlineWidth <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸宽度不合法！");
                return;
            }

            //内图廓高度
            double InlineHeight;
            res = double.TryParse(tbInlineHeight.Text, out InlineHeight);
            if (!res || InlineHeight <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }

            //内外图廓间距
            double InOutLineWidth;
            res = double.TryParse(tbInOutWidth.Text, out InOutLineWidth);
            if (!res || InOutLineWidth < 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }
            if (paperWidth < MapSizeWidth || paperHeight < MapSizeHeight)
            {
                MessageBox.Show("纸张尺寸不能小于成图尺寸！");
                return;
            }
            if (InlineWidth + InOutLineWidth > MapSizeWidth || InlineHeight + InOutLineWidth > MapSizeHeight)
            {
                MessageBox.Show("成图尺寸不能小于外图廓尺寸！");
                return;
            }
            //存储地图各尺寸
            CommonMethods.SetMapPar(paperWidth, paperHeight, MapSizeWidth, MapSizeHeight, InlineWidth, InlineHeight, InOutLineWidth, refScale);
            //重新获取相应尺度的几何
            GetCurrentGeo(refScale);
            //中心点
            if (m_GeoPlygon == null)
            {
                MessageBox.Show("请指定裁切中心点！");

                return;
            }


            double dx = Dx;
            double dy = Dy;
            var ps = new PaperSize { PaperWidth = double.Parse(tbPaperWidth.Text), PaperHeight = double.Parse(tbPaperHeight.Text) };
            //更新配置表
            EnvironmentSettings.updateMapScale(_app, refScale);
            EnvironmentSettings.updatePageSize(_app, paperWidth, paperHeight);
            CommonMethods.CalculateSptialRef(m_GeoPlygon.Envelope);
            IGroupElement clipELe = ClipElement.createClipGroupElementEx2(_app, m_GeoPlygon, InlineWidth, InlineHeight, double.Parse(cbRefSclae.Text), dx, dy);
            ClipElement.SetClipGroupElement(_app, clipELe);
            CommonMethods.clipEx = true;
            DialogResult = DialogResult.OK;
            Close();
        }
        //获取相应尺度的行政范围面！！！
        private void GetCurrentGeo(double scale)
        {

            XElement expertiseContent = ExpertiseDatabase.getContentElement(GApplication.Application);

            var mapScaleRule = expertiseContent.Element("MapScaleRule");
            var scaleItems = mapScaleRule.Elements("Item");
            IWorkspace sdeWorkspace = null;
            foreach (XElement ele in scaleItems)
            {
                #region
                string database = (string)ele.Element("DatabaseName");
                double min = double.Parse(ele.Element("Min").Value);
                double max = double.Parse(ele.Element("Max").Value);
                if (scale >= min && scale <= max)
                {
                    sdeWorkspace = _app.GetWorkspacWithSDEConnection(_sdeAddress, _userName, _password, database);
                    break;
                }
                else if (scale < 50000)
                {
                    sdeWorkspace = _app.GetWorkspacWithSDEConnection(_sdeAddress, _userName, _password, _regionNameDatabase);
                    break;
                }
                #endregion
            }
            if (null == sdeWorkspace)
            {
                MessageBox.Show("无法访问服务器！");
                return;
            }
            bool dcdDataBaseNoExistsBOUA = (!(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _regionNameProvince) || !(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _regionNameCity) || !(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _regionNameCountry) || !(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _regionNameTown));//多尺度境界面是否存在
            bool tdtDataBaseExistsNoBOUA = !(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, "BOUA");//天地图境界面是否存在
            if (dcdDataBaseNoExistsBOUA && tdtDataBaseExistsNoBOUA)
            {
                MessageBox.Show("无法访问服务器中的境界库！");
                return;
            }
            IFeatureWorkspace sdeFeatureWorkspace = sdeWorkspace as IFeatureWorkspace;
            List<IFeatureClass> fclboua = new List<IFeatureClass>();
            //省、市州、区县、乡镇街道
            if (!dcdDataBaseNoExistsBOUA)
            {
                IFeatureClass fclboua2 = sdeFeatureWorkspace.OpenFeatureClass(_regionNameProvince);
                IFeatureClass fclboua4 = sdeFeatureWorkspace.OpenFeatureClass(_regionNameCity);
                IFeatureClass fclboua5 = sdeFeatureWorkspace.OpenFeatureClass(_regionNameCountry);
                IFeatureClass fclboua6 = sdeFeatureWorkspace.OpenFeatureClass(_regionNameTown);
                fclboua.Add(fclboua2);
                fclboua.Add(fclboua4);
                fclboua.Add(fclboua5);
                fclboua.Add(fclboua6);
            }
            else if (!tdtDataBaseExistsNoBOUA)
            {
                IFeatureClass boua = sdeFeatureWorkspace.OpenFeatureClass("BOUA");
                fclboua.Add(boua);
            }
            

            IFeature f=null;
            foreach (var fcl in fclboua)
            {
                f = searchGeometry(fcl);
                if (f != null)
                {
                    break;
                }
            }
                        
            if (f == null)
            {
                MessageBox.Show("没有相应范围比例尺的数据", "提示");
                m_GeoPlygon = null;
                return;
            }




            m_GeoPlygon = f.ShapeCopy as IPolygon;
            //设置目标空间坐标系
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (null == targetSpatialReference)
                targetSpatialReference = _app.MapControl.Map.SpatialReference;
            m_GeoPlygon = f.ShapeCopy as IPolygon;
            if (m_GeoPlygon.SpatialReference.Name != targetSpatialReference.Name)
            {
                (m_GeoPlygon as IGeometry).Project(targetSpatialReference);//对定位几何进行投影变换
            }  
        }                       
        private void btCancel_Click(object sender, EventArgs e)
        {
            _app.MapControl.ActiveView.GraphicsContainer.DeleteAllElements();
            _app.MapControl.ActiveView.Refresh();
            Close();
        }

        private void search(IFeatureClass placeNameFeatureClass, string fieldName, string keyVal)
        {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = string.Format("{0} LIKE '%{1}%'", fieldName, keyVal);
            if (placeNameFeatureClass.FindField("st_area(shape)") >= 0)
            {
                IQueryFilterDefinition qfDef = qf as IQueryFilterDefinition;
                qfDef.PostfixClause = "ORDER BY st_area(shape) DESC";
            }         
            IFeatureCursor fCursor = placeNameFeatureClass.Search(qf, false);
            IFeature f = fCursor.NextFeature();
            while (f != null)
            {
                _result.Add(f);
                f = fCursor.NextFeature();
            }

            System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
        }
        string pacFN = "PAC";
        string objName = "";
        string objPac = "";
        private void updateResultTable()
        {
            dgResult.Rows.Clear();

            if (_result != null)
            {
                IQueryFilter qf = new QueryFilterClass();
                for (int i = 0; i < _result.Count; ++i)
                {
                    IFeature f = _result[i];
                    if (f == null || f.Shape == null || f.Shape.GeometryType != esriGeometryType.esriGeometryPolygon)
                    {
                        continue;
                    }
                    int indexOfName = f.Fields.FindField(_regionNameFieldName);
                    if (indexOfName == -1)
                    {
                        continue;
                    }
                    string name = f.get_Value(indexOfName).ToString();

                    int rowIndex = dgResult.Rows.Add();
                    dgResult.Rows[rowIndex].Cells["PlaceName"].Value = name;
                    //设置所属县，市，省
                    int pacIndex = f.Fields.FindField(pacFN);
                    if (pacIndex != -1)
                    {
                        IField field = f.Fields.get_Field(pacIndex);
                        string pacVal = f.get_Value(pacIndex).ToString();

                        string country = pacVal.Substring(0, 6);
                        qf.WhereClause = string.Format("PAC LIKE '{0}%'", country);
                        IFeatureCursor fCursor = FclBOUA5.Search(qf, false);
                        IFeature fe = fCursor.NextFeature();
                        if (fe != null)
                        {
                            dgResult.Rows[rowIndex].Cells["Country"].Value = fe.get_Value(indexOfName).ToString();
                        }

                        string city = pacVal.Substring(0, 4);
                        qf.WhereClause = string.Format("PAC LIKE '{0}%'", city);
                        fCursor = FclBOUA4.Search(qf, false);
                        fe = fCursor.NextFeature();
                        if (fe != null)
                        {
                            dgResult.Rows[rowIndex].Cells["City"].Value = fe.get_Value(indexOfName).ToString();
                        }

                        string province = pacVal.Substring(0, 2);
                        qf.WhereClause = string.Format("PAC LIKE '{0}%'", province);
                        fCursor = FclBOUA2.Search(qf, false);
                        fe = fCursor.NextFeature();
                        if (fe != null)
                        {
                            dgResult.Rows[rowIndex].Cells["Province"].Value = fe.get_Value(indexOfName).ToString();
                        }
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);

                    }
                    dgResult.Rows[rowIndex].HeaderCell.Value = i.ToString();
                }
            }

            if (dgResult.CurrentRow != null)
            {
                int index = dgResult.CurrentRow.Index;
                objName = dgResult.CurrentRow.Cells["PlaceName"].Value.ToString();
                IFeature f = _result[index];
                m_GeoPlygon = f.ShapeCopy as IPolygon;
                objPac = f.get_Value(f.Fields.FindField(pacFN)).ToString();
            }
        }

       

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void btView_Click(object sender, EventArgs e)
        {
            bool res;

            m_Timer.Stop();
            if (_result == null || _result.Count == 0)
            {
                MessageBox.Show("请指定出图目标！");

                return;
            }

            //比例尺
            double refScale;
            res = double.TryParse(cbRefSclae.Text, out refScale);
            if (!res || refScale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");

                return;
            }

            //宽度
            double paperWidth;
            res = double.TryParse(tbPaperWidth.Text, out paperWidth);
            if (!res || paperWidth <= 0)
            {
                MessageBox.Show("输入的纸张宽度不合法！");

                return;
            }

            //高度
            double paperHeight;
            res = double.TryParse(tbPaperHeight.Text, out paperHeight);
            if (!res || paperHeight <= 0)
            {
                MessageBox.Show("输入的纸张高度不合法！");

                return;
            }


            //成图尺寸宽度
            double MapSizeWidth;
            res = double.TryParse(tbMapSizeWidth.Text, out MapSizeWidth);
            if (!res || MapSizeWidth <= 0)
            {
                MessageBox.Show("输入的成图尺寸宽度不合法！");
                return;
            }
            //成图尺寸高度
            double MapSizeHeight;
            res = double.TryParse(tbMapSizeHeight.Text, out MapSizeHeight);
            if (!res || MapSizeHeight <= 0)
            {
                MessageBox.Show("输入的成图尺寸高度不合法！");
                return;
            }

            //内图廓宽度
            double InlineWidth;
            res = double.TryParse(tbInlineWidth.Text, out InlineWidth);
            if (!res || InlineWidth <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸宽度不合法！");
                return;
            }

            //内图廓高度
            double InlineHeight;
            res = double.TryParse(tbInlineHeight.Text, out InlineHeight);
            if (!res || InlineHeight <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }

            //内外图廓间距
            double InOutLineWidth;
            res = double.TryParse(tbInOutWidth.Text, out InOutLineWidth);
            if (!res || InOutLineWidth < 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return;
            }
            if (paperWidth < MapSizeWidth || paperHeight < MapSizeHeight)
            {
                MessageBox.Show("纸张尺寸不能小于成图尺寸！");
                return;
            }
            if (InlineWidth + InOutLineWidth > MapSizeWidth || InlineHeight + InOutLineWidth > MapSizeHeight)
            {
                MessageBox.Show("成图尺寸不能小于外图廓尺寸！");
                return;
            }
            //存储地图各尺寸
            CommonMethods.SetMapPar(paperWidth, paperHeight, MapSizeWidth, MapSizeHeight, InlineWidth, InlineHeight, InOutLineWidth, refScale);
            if (_result.Count == 0)
                return;
            //重新获取相应尺度的几何
            GetCurrentGeo(refScale);
            //中心点
            if (m_GeoPlygon == null)
            {
                MessageBox.Show("请指定裁切中心点！");

                return;
            }


            double dx = Dx;
            double dy = Dy;
            var ps = new PaperSize { PaperWidth = double.Parse(tbPaperWidth.Text), PaperHeight = double.Parse(tbPaperHeight.Text) };
            //更新配置表
            EnvironmentSettings.updateMapScale(_app, refScale);
            EnvironmentSettings.updatePageSize(_app, paperWidth, paperHeight);
            CommonMethods.CalculateSptialRef(m_GeoPlygon.Envelope);
            IGroupElement clipELe = ClipElement.createClipGroupElementEx2(_app, m_GeoPlygon, InlineWidth, InlineHeight, double.Parse(cbRefSclae.Text), dx, dy);
            ClipElement.SetClipGroupElement(_app, clipELe);
            CommonMethods.clipEx = true;
            DialogResult = DialogResult.OK;

        }

        private void RegionNameLocationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {            
                _app.MapControl.ActiveView.Refresh();               
            }
        }
        private IFeature searchGeometry(IFeatureClass fcl)
        {
            IArea shapeArea = _result[dgResult.CurrentCell.RowIndex].ShapeCopy as IArea;
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = string.Format("{0} = '{1}' and {2} = '{3}'", _regionNameFieldName, objName, pacFN, objPac);
            IFeatureCursor fCursor = fcl.Search(qf, false);
            IFeature corretFe = null;
            IFeature fe = null;
            double minError = double.MaxValue;
            while ((fe = fCursor.NextFeature()) != null)
            {
                double error = Math.Abs((fe.ShapeCopy as IArea).Area - shapeArea.Area);
                if (error < minError)
                {
                    minError = error;
                    corretFe = fe;
                }
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
            return corretFe;
        }

        //根据比例尺修改纸张
        private void ModifyPageByScale()
        {
            if (m_GeoPlygon == null)
                return;
            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (m_GeoPlygon.SpatialReference.Name != targetSpatialReference.Name)
            {
                (m_GeoPlygon as IGeometry).Project(targetSpatialReference);//对定位几何进行投影变换
            }
            IGeometry polygon = m_GeoPlygon;
            double width = polygon.Envelope.Width;
            double height = polygon.Envelope.Height;
            bool res;
            double scale;
            res = double.TryParse(cbRefSclae.Text, out scale);
            if (!res || scale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");

                return;
            }

            double mapwidth = width * 1.1 / scale * 1000;//毫米,裁切面扩展1.1倍
            double mapheight = height * 1.1 / scale * 1000;//毫米，裁切面扩展1.1倍
            Dictionary<int, PaperInfo> infoDic = new Dictionary<int, PaperInfo>();
            string keyword = "横";
            if (mapwidth < mapheight) //纵向
            {
                keyword = "纵";
            }
            for (int i = 0; i < cbPaperSize.Items.Count; i++)
            {
                PaperInfo p = cbPaperSize.Items[i] as PaperInfo;
                if (p.ToString().IndexOf(keyword) != -1)
                {
                    infoDic[i] = p;
                }
            }
            var list = infoDic.ToList();
            for (int i = 0; i < list.Count; i++)
            {
                PaperInfo p = list[i].Value as PaperInfo;
                int inext = i < list.Count - 1 ? i + 1 : list.Count - 1;
                PaperInfo p1 = list[inext].Value as PaperInfo;
                if (mapwidth > mapheight) //横版
                {
                    DataRow itemSel = CommonLocationMethods.CaluateMapAndInlineSize(p.Width, p.Height);
                    DataRow itemSel1 = CommonLocationMethods.CaluateMapAndInlineSize(p1.Width, p1.Height);
                    double inlineWidth = 0, inlineHeight = 0, inlineWidth1 = 0, inlineHeight1 = 0;
                    #region 计算内图廓尺寸
                    if (itemSel != null)
                    {
                        //横版
                        if (p.Width > p.Height)
                        {

                            inlineWidth = double.Parse(itemSel["横版内图廓尺寸宽"].ToString());
                            inlineHeight = double.Parse(itemSel["横版内图廓尺寸高"].ToString());

                        }
                        //竖版
                        else
                        {
                            inlineWidth = double.Parse(itemSel["竖版内图廓尺寸宽"].ToString());
                            inlineHeight = double.Parse(itemSel["竖版内图廓尺寸高"].ToString());
                        }
                        //横版
                        if (p1.Width > p1.Height)
                        {

                            inlineWidth1 = double.Parse(itemSel1["横版内图廓尺寸宽"].ToString());
                            inlineHeight1 = double.Parse(itemSel1["横版内图廓尺寸高"].ToString());

                        }
                        //竖版
                        else
                        {
                            inlineWidth1 = double.Parse(itemSel1["竖版内图廓尺寸宽"].ToString());
                            inlineHeight1 = double.Parse(itemSel1["竖版内图廓尺寸高"].ToString());
                        }
                    }
                    #endregion
                    //匹配到内图廓尺寸则按内图廓计算，否则按纸张计算
                    if (itemSel != null && itemSel1 != null)
                    {
                        if ((inlineWidth - mapwidth > 0 && inlineWidth1 - mapwidth < 0)||(inlineHeight - mapheight > 0 && inlineHeight1 - mapheight < 0))
                        {
                            _scaleChangedState = true;
                            //double a = p.Height - mapheight; double b = p1.Height - mapheight;
                            cbPaperSize.SelectedIndex = list[i].Key;
                            break;
                        }
                        //最后一个：最小出图尺寸
                        if (i == list.Count - 1)
                        {
                            if (inlineWidth - mapwidth > 0)
                            {
                                _scaleChangedState = true;
                                cbPaperSize.SelectedIndex = list[i].Key;
                                break;
                            }
                        }

                    }
                    else
                    {
                        if ((p.Width - mapwidth > 0 && p1.Width - mapwidth < 0) || (p.Height - mapheight > 0 && p1.Height - mapheight < 0))
                        {
                            _scaleChangedState = true;
                            //double a = p.Height - mapheight; double b = p1.Height - mapheight;
                            cbPaperSize.SelectedIndex = list[i].Key;
                            break;
                        }
                        //最后一个：最小出图尺寸
                        if (i == list.Count - 1)
                        {
                            if (p.Width - mapwidth > 0)
                            {
                                _scaleChangedState = true;
                                cbPaperSize.SelectedIndex = list[i].Key;
                                break;
                            }
                        }
                    }
                }
                else//纵版
                {
                    DataRow itemSel = CommonLocationMethods.CaluateMapAndInlineSize(p.Width, p.Height);
                    DataRow itemSel1 = CommonLocationMethods.CaluateMapAndInlineSize(p1.Width, p1.Height);
                    double inlineWidth = 0, inlineHeight = 0, inlineWidth1 = 0, inlineHeight1 = 0;
                    #region 计算内图廓尺寸
                    if (itemSel != null)
                    {
                        //横版
                        if (p.Width > p.Height)
                        {

                            inlineWidth = double.Parse(itemSel["横版内图廓尺寸宽"].ToString());
                            inlineHeight = double.Parse(itemSel["横版内图廓尺寸高"].ToString());

                        }
                        //竖版
                        else
                        {
                            inlineWidth = double.Parse(itemSel["竖版内图廓尺寸宽"].ToString());
                            inlineHeight = double.Parse(itemSel["竖版内图廓尺寸高"].ToString());
                        }
                        //横版
                        if (p1.Width > p1.Height)
                        {

                            inlineWidth1 = double.Parse(itemSel1["横版内图廓尺寸宽"].ToString());
                            inlineHeight1 = double.Parse(itemSel1["横版内图廓尺寸高"].ToString());

                        }
                        //竖版
                        else
                        {
                            inlineWidth1 = double.Parse(itemSel1["竖版内图廓尺寸宽"].ToString());
                            inlineHeight1 = double.Parse(itemSel1["竖版内图廓尺寸高"].ToString());
                        }
                    }
                    #endregion
                    //匹配到内图廓尺寸则按内图廓计算，否则按纸张计算
                    if (itemSel != null && itemSel1 != null)
                    {
                        if ((inlineHeight - mapheight > 0 && inlineHeight1 - mapheight < 0)||(inlineWidth - mapwidth > 0 && inlineWidth1 - mapwidth < 0))
                        {
                            _scaleChangedState = true;
                            //double a = p.Height - mapheight; double b = p1.Height - mapheight;
                            cbPaperSize.SelectedIndex = list[i].Key;
                            break;
                        }
                        //最后一个：最小出图尺寸
                        if (i == list.Count - 1)
                        {
                            if (inlineHeight - mapheight > 0)
                            {
                                _scaleChangedState = true;
                                cbPaperSize.SelectedIndex = list[i].Key;
                                break;
                            }
                        }

                    }
                    else
                    {
                        if ((p.Height - mapheight > 0 && p1.Height - mapheight < 0)||(p.Width - mapwidth > 0 && p1.Width - mapwidth < 0))
                        {
                            _scaleChangedState = true;
                            cbPaperSize.SelectedIndex = list[i].Key;
                            break;
                        }
                        //最后一个：最小出图尺寸
                        if (i == list.Count - 1)
                        {
                            if (p.Height - mapheight > 0)
                            {
                                _scaleChangedState = true;
                                cbPaperSize.SelectedIndex = list[i].Key;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void cbPaperSize_MouseClick(object sender, MouseEventArgs e)
        {
            PaperMosueClick = true;
        }
        Timer m_Timer = new Timer();

        private void cbRefSclae_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool res;
            double scale;
            res = double.TryParse(cbRefSclae.Text, out scale);
            if (!res || scale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");

                return;
            }

            ModifyPageByScale();
        }
        private void cbRefSclae_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                m_Timer.Stop();
                ModifyPageByScale();
            }
            //判断按键是不是要输入的类型。
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;

            //小数点的处理。
            if ((int)e.KeyChar == 46) //小数点
            {
                if ((sender as TextBox).Text.Length <= 0)
                {
                    e.Handled = true; //小数点不能在第一位
                }
                else
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse((sender as TextBox).Text, out oldf);
                    b2 = float.TryParse((sender as TextBox).Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }
        }

        private void cbRefSclae_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ModifyPageByScale();
            }
        }


        private void tbPaperWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            //判断按键是不是要输入的类型。
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;

            //小数点的处理。
            if ((int)e.KeyChar == 46) //小数点
            {
                if ((sender as TextBox).Text.Length <= 0)
                {
                    e.Handled = true; //小数点不能在第一位
                }
                else
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse((sender as TextBox).Text, out oldf);
                    b2 = float.TryParse((sender as TextBox).Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }

            if (!e.Handled)
            {
                cbPaperSize.Text = "自定义尺寸";
                return;

            }
        }

        private void tbPaperHeight_KeyPress(object sender, KeyPressEventArgs e)
        {
            //判断按键是不是要输入的类型。
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;

            //小数点的处理。
            if ((int)e.KeyChar == 46) //小数点
            {
                if ((sender as TextBox).Text.Length <= 0)
                {
                    e.Handled = true; //小数点不能在第一位
                }
                else
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse((sender as TextBox).Text, out oldf);
                    b2 = float.TryParse((sender as TextBox).Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }

            if (!e.Handled)
            {
                cbPaperSize.Text = "自定义尺寸";
                return;

            }
        }

        

        

        
        
    }
}
