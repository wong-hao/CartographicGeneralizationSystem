using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SMGI.Common;
using System.IO;
using System.Xml.Linq;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.EmergencyMap
{
    public partial class AdministrativeRegeionLocationForm : Form
    {
        private GApplication _app;
        private string _serverIPAddress;
        private string _userName;
        private string _password;
        private string _serverDatabase;//行政区划列表中几何对应的服务器数据库名
        private string _boua2FCName;
        private string _boua4FCName;
        private string _boua5FCName;
        private string _boua6FCName;
        private Dictionary<string, KeyValuePair<string, IPolygon>> _pac2NameGeo2;
        private Dictionary<string, KeyValuePair<string, IPolygon>> _pac2NameGeo4;
        private Dictionary<string, KeyValuePair<string, IPolygon>> _pac2NameGeo5;
        private Dictionary<string, KeyValuePair<string, IPolygon>> _pac2NameGeo6;
        private IPolygon _ClipGeoPlygon;
        private string _curServerDatabase;//当前裁切几何面对应的服务器数据库名
        List<string> _clipPACList;
        bool _scaleChangedState;//由于比例尺的变化会触发纸张大小的更新，通过这个变量来保证该事件触发的纸张大小更新不会再更改比例尺
        public AdministrativeRegeionLocationForm(GApplication app)
        {
            InitializeComponent();

            _app = app;
            _ClipGeoPlygon = null;
            _scaleChangedState = false;
        }

        private void AdministrativeRegeionLocationForm_Load(object sender, EventArgs e)
        {
            using (var wo = _app.SetBusy())
            {
                #region 初始化行政区划列表
                wo.SetText("正在读取配置信息......");
                string configFileName = _app.Template.Root + @"\专家库\数据定位\LocationSearch.xml";
                if (!File.Exists(configFileName))
                    return;

                XDocument doc = XDocument.Load(configFileName);
                var serverEles = doc.Element("Template").Element("Content").Elements("Server");
                XElement regionEle = null;
                foreach (var ele in serverEles)
                {
                    if (ele.Attribute("name").Value == "行政区定位")
                    {
                        regionEle = ele;
                        break;
                    }
                }

                if (regionEle == null)
                    return;

                _serverIPAddress = regionEle.Element("IPAddress").Value;
                _userName = regionEle.Element("UserName").Value;
                _password = regionEle.Element("Password").Value;
                _serverDatabase = regionEle.Element("DataBase").Value;
                _boua2FCName = regionEle.Element("BOUA").Element("BOUA2").Value;
                _boua4FCName = regionEle.Element("BOUA").Element("BOUA4").Value;
                _boua5FCName = regionEle.Element("BOUA").Element("BOUA5").Value;
                _boua6FCName = regionEle.Element("BOUA").Element("BOUA6").Value;
                string provPAC = regionEle.Element("ProvincePAC").Value.Trim().Substring(0, 2);

                wo.SetText("正在连接服务器数据库......");
                IFeatureWorkspace sdeWorkspace = _app.GetWorkspacWithSDEConnection(_serverIPAddress, _userName, _password, _serverDatabase) as IFeatureWorkspace;
                if (null == sdeWorkspace)
                {
                    MessageBox.Show("无法访问服务器！");
                    return;
                }

                if (!(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _boua2FCName))
                {
                    MessageBox.Show(string.Format("无法访问服务器中的要素类【{0}】！", _boua2FCName));
                    return;
                }
                IFeatureClass boua2FC = sdeWorkspace.OpenFeatureClass(_boua2FCName);
                if (!(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _boua4FCName))
                {
                    MessageBox.Show(string.Format("无法访问服务器中的要素类【{0}】！", _boua4FCName));
                    return;
                }
                IFeatureClass boua4FC = sdeWorkspace.OpenFeatureClass(_boua4FCName);
                if (!(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _boua5FCName))
                {
                    MessageBox.Show(string.Format("无法访问服务器中的要素类【{0}】！", _boua5FCName));
                    return;
                }
                IFeatureClass boua5FC = sdeWorkspace.OpenFeatureClass(_boua5FCName);
                if (!(sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _boua6FCName))
                {
                    MessageBox.Show(string.Format("无法访问服务器中的要素类【{0}】！", _boua6FCName));
                    return;
                }
                IFeatureClass boua6FC = sdeWorkspace.OpenFeatureClass(_boua6FCName);

                _pac2NameGeo2 = new Dictionary<string, KeyValuePair<string, IPolygon>>();
                _pac2NameGeo4 = new Dictionary<string, KeyValuePair<string, IPolygon>>();
                _pac2NameGeo5 = new Dictionary<string, KeyValuePair<string, IPolygon>>();
                _pac2NameGeo6 = new Dictionary<string, KeyValuePair<string, IPolygon>>();

                #region 获取各级行政区划信息
                wo.SetText("正在从服务器数据库中获取行政区几何......");
                int pacIndex = boua2FC.FindField("pac");
                int nameIndex = boua2FC.FindField("name");
                if (pacIndex != -1 && nameIndex != -1)
                {
                    IFeatureCursor feCursor = boua2FC.Search(null, true);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        string name = fe.get_Value(nameIndex).ToString();
                        string pac = fe.get_Value(pacIndex).ToString().Trim();
                        if (fe.Shape == null || fe.Shape.IsEmpty)
                            continue;

                        if (!pac.StartsWith(provPAC))
                            continue;

                        int fdIndex = fe.Fields.FindField("fd");
                        if (fdIndex != -1)
                        {
                            var fdValue = fe.get_Value(fdIndex);
                            if(!Convert.IsDBNull(fdValue) && fdValue.ToString().Trim() != "")
                                continue;//飞地：fd字段不为空，且也不为空字符串
                        }

                        if (!_pac2NameGeo2.ContainsKey(pac))
                        {
                            _pac2NameGeo2.Add(pac, new KeyValuePair<string, IPolygon>(name, fe.ShapeCopy as IPolygon));
                        }
                        else
                        {
                            IPolygon newShape = (_pac2NameGeo2[pac].Value as ITopologicalOperator).Union(fe.ShapeCopy) as IPolygon;
                            _pac2NameGeo2[pac] = new KeyValuePair<string, IPolygon>(_pac2NameGeo2[pac].Key, newShape);
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);
                }

                pacIndex = boua4FC.FindField("pac");
                nameIndex = boua4FC.FindField("name");
                if (pacIndex != -1 && nameIndex != -1)
                {
                    IFeatureCursor feCursor = boua4FC.Search(null, true);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        string name = fe.get_Value(nameIndex).ToString();
                        string pac = fe.get_Value(pacIndex).ToString().Trim();
                        if (fe.Shape == null || fe.Shape.IsEmpty)
                            continue;

                        if (pac.Length < 4)
                            continue;

                        bool isValidPac = false;
                        foreach (var kv in _pac2NameGeo2)
                        {
                            if (pac.StartsWith(kv.Key.Substring(0, 2)))
                            {
                                isValidPac = true;
                                break;
                            }
                        }
                        if (!isValidPac)
                            continue;

                        int fdIndex = fe.Fields.FindField("fd");
                        if (fdIndex != -1)
                        {
                            var fdValue = fe.get_Value(fdIndex);
                            if (!Convert.IsDBNull(fdValue) && fdValue.ToString().Trim() != "")
                                continue;//飞地：fd字段不为空，且也不为空字符串
                        }

                        if (!_pac2NameGeo4.ContainsKey(pac))
                        {
                            _pac2NameGeo4.Add(pac, new KeyValuePair<string, IPolygon>(name, fe.ShapeCopy as IPolygon));
                        }
                        else
                        {
                            IPolygon newShape = (_pac2NameGeo4[pac].Value as ITopologicalOperator).Union(fe.ShapeCopy) as IPolygon;
                            _pac2NameGeo4[pac] = new KeyValuePair<string, IPolygon>(_pac2NameGeo4[pac].Key, newShape);
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);
                }


                pacIndex = boua5FC.FindField("pac");
                nameIndex = boua5FC.FindField("name");
                if (pacIndex != -1 && nameIndex != -1)
                {
                    IFeatureCursor feCursor = boua5FC.Search(null, true);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        string name = fe.get_Value(nameIndex).ToString();
                        string pac = fe.get_Value(pacIndex).ToString().Trim();
                        if (fe.Shape == null || fe.Shape.IsEmpty)
                            continue;

                        if (pac.Length < 6)
                            continue;

                        bool isValidPac = false;
                        foreach (var kv in _pac2NameGeo4)
                        {
                            if (pac.StartsWith(kv.Key.Substring(0, 4)))
                            {
                                isValidPac = true;
                                break;
                            }
                        }
                        if (!isValidPac)
                            continue;

                        int fdIndex = fe.Fields.FindField("fd");
                        if (fdIndex != -1)
                        {
                            var fdValue = fe.get_Value(fdIndex);
                            if (!Convert.IsDBNull(fdValue) && fdValue.ToString().Trim() != "")
                                continue;//飞地：fd字段不为空，且也不为空字符串
                        }

                        if (!_pac2NameGeo5.ContainsKey(pac))
                        {
                            _pac2NameGeo5.Add(pac, new KeyValuePair<string, IPolygon>(name, fe.ShapeCopy as IPolygon));
                        }
                        else
                        {
                            IPolygon newShape = (_pac2NameGeo5[pac].Value as ITopologicalOperator).Union(fe.ShapeCopy) as IPolygon;
                            _pac2NameGeo5[pac] = new KeyValuePair<string, IPolygon>(_pac2NameGeo5[pac].Key, newShape);
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);
                }

                pacIndex = boua6FC.FindField("pac");
                nameIndex = boua6FC.FindField("name");
                if (pacIndex != -1 && nameIndex != -1)
                {
                    IFeatureCursor feCursor = boua6FC.Search(null, true);
                    IFeature fe = null;
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        string name = fe.get_Value(nameIndex).ToString();
                        string pac = fe.get_Value(pacIndex).ToString().Trim();
                        if (fe.Shape == null || fe.Shape.IsEmpty)
                            continue;

                        if (pac.Length < 9)
                            continue;

                        bool isValidPac = false;
                        foreach (var kv in _pac2NameGeo5)
                        {
                            if (pac.StartsWith(kv.Key.Substring(0, 6)))
                            {
                                isValidPac = true;
                                break;
                            }
                        }
                        if (!isValidPac)
                            continue;

                        int fdIndex = fe.Fields.FindField("fd");
                        if (fdIndex != -1)
                        {
                            var fdValue = fe.get_Value(fdIndex);
                            if (!Convert.IsDBNull(fdValue) && fdValue.ToString().Trim() != "")
                                continue;//飞地：fd字段不为空，且也不为空字符串
                        }

                        if (!_pac2NameGeo6.ContainsKey(pac))
                        {
                            _pac2NameGeo6.Add(pac, new KeyValuePair<string, IPolygon>(name, fe.ShapeCopy as IPolygon));
                        }
                        else
                        {
                            IPolygon newShape = (_pac2NameGeo6[pac].Value as ITopologicalOperator).Union(fe.ShapeCopy) as IPolygon;
                            _pac2NameGeo6[pac] = new KeyValuePair<string, IPolygon>(_pac2NameGeo6[pac].Key, newShape);
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);
                }
                #endregion

                #region 初始化行政区划列表
                wo.SetText("正在初始化行政区划列表......");
                var topNode = RegionTreeView.Nodes.Add("行政区划列表");//根节点
                foreach (var item2 in _pac2NameGeo2)
                {
                    var node2 = topNode.Nodes.Add(item2.Key, item2.Value.Key);
                    foreach (var item4 in _pac2NameGeo4)
                    {
                        if (item4.Key.Substring(0, 2) != item2.Key.Substring(0, 2))
                            continue;

                        var node4 = node2.Nodes.Add(item4.Key, item4.Value.Key);
                        foreach (var item5 in _pac2NameGeo5)
                        {
                            if (item5.Key.Substring(0, 4) != item4.Key.Substring(0, 4))
                                continue;

                            var node5 = node4.Nodes.Add(item5.Key, item5.Value.Key);
                            foreach (var item6 in _pac2NameGeo6)
                            {
                                if (item6.Key.Substring(0, 6) != item5.Key.Substring(0, 6))
                                    continue;

                                node5.Nodes.Add(item6.Key, item6.Value.Key);
                            }
                        }
                    }
                    node2.Expand();
                }
                topNode.Expand();
                RegionTreeView.CheckBoxes = true;
                #endregion

                #endregion


                #region 根据环境设置，初始化其它控件值
                wo.SetText("正在初始化窗体......");
                var content = EnvironmentSettings.getContentElement(_app);
                var pagesize = content.Element("PageSize");
                var mapScale = content.Element("MapScale");

                cbRefSclae.Text = mapScale.Value;
                tbPaperWidth.Text = pagesize.Element("Width").Value;
                tbPaperHeight.Text = pagesize.Element("Height").Value;


                //纸张尺寸
                double width = Convert.ToDouble(tbPaperWidth.Text);
                double height = Convert.ToDouble(tbPaperHeight.Text);
                List<PaperInfo> pi = PageSizeRules.getPageSizeInfos(_app);
                int index = -1;
                for (int i = 0; i < pi.Count; ++i)
                {
                    cbPaperSize.Items.Add(pi[i]);
                    if (pi[i].Width == width && pi[i].Height == height)
                    {
                        index = i;
                    }
                }
                cbPaperSize.SelectedIndex = index;
                if (-1 == index)
                {
                    cbPaperSize.Text = "自定义尺寸";
                }
                #endregion
            }
        }

        private void cbPaperSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            PaperInfo pi = cbPaperSize.SelectedItem as PaperInfo;
            if (pi != null)
            {
                tbPaperWidth.Text = pi.Width.ToString();
                tbPaperHeight.Text = pi.Height.ToString();
            }

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

            #region 更新成图尺寸、内图廓尺寸、内外图廓间距等控件
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

            //更新比例尺
            if (_ClipGeoPlygon != null && !_scaleChangedState)
            {
                if (_ClipGeoPlygon.SpatialReference.Name != targetSpatialReference.Name)
                {
                    (_ClipGeoPlygon as IGeometry).Project(targetSpatialReference);//对定位几何进行投影变换
                }
                double scale = CommonLocationMethods.CaluateMapScale(InlineWidth, InlineHeight, _ClipGeoPlygon);
                cbRefSclae.Text = scale.ToString();
            }

            //重置
            _scaleChangedState = false;
        }

        private void cbRefSclae_SelectedIndexChanged(object sender, EventArgs e)
        {
            double scale;
            bool res = double.TryParse(cbRefSclae.Text, out scale);
            if (!res || scale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");
                return;
            }

            //更新相关控件
            ModifyPageByScale(scale);
        }

        private void cbRefSclae_KeyPress(object sender, KeyPressEventArgs e)
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

            double scale;
            bool res = double.TryParse(cbRefSclae.Text, out scale);
            if (!res || scale <= 0)
            {
                e.Handled = true;
            }
        }

        private void cbRefSclae_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                double scale;
                bool res = double.TryParse(cbRefSclae.Text, out scale);

                ModifyPageByScale(scale);
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
                cbPaperSize.Text = "自定义尺寸";
        }

        private void RegionTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            _ClipGeoPlygon = null;
            _clipPACList = new List<string>();

            for (int i = 0; i < RegionTreeView.Nodes.Count; ++i)
            {
                var provNode = RegionTreeView.Nodes[i];
                if (provNode.Checked)
                {
                    string pac = provNode.Name.Trim();
                    IPolygon geo = null;
                    if (_pac2NameGeo2.ContainsKey(pac))
                        geo = _pac2NameGeo2[pac].Value;
                    if (geo == null)
                        continue;

                    if (_ClipGeoPlygon == null)
                    {
                        _ClipGeoPlygon = (geo as IClone).Clone() as IPolygon;
                    }
                    else
                    {
                        ITopologicalOperator to = _ClipGeoPlygon as ITopologicalOperator;
                        _ClipGeoPlygon = to.Union(geo) as IPolygon;
                    }

                    if (!_clipPACList.Contains(pac))
                        _clipPACList.Add(pac);
                }
                else
                {
                    //递归子节点
                    updateClipGeoByCheckedNode(provNode);
                }
            }

            if (_ClipGeoPlygon == null)
            {
                btn_Preview.Enabled = false;
                btOK.Enabled = false;

                _curServerDatabase = "";//更新当前裁切几何面对应的数据库名

                //清空临时元素
                _app.MapControl.ActiveView.GraphicsContainer.DeleteAllElements();
                _app.MapControl.ActiveView.Refresh();
            }
            else
            {
                btn_Preview.Enabled = true;
                btOK.Enabled = true;

                _curServerDatabase = _serverDatabase;//更新当前裁切几何面对应的数据库名

                //设置目标空间坐标系
                ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
                if (null == targetSpatialReference)
                    targetSpatialReference = _app.MapControl.Map.SpatialReference;
                if (_ClipGeoPlygon.SpatialReference.Name != targetSpatialReference.Name)
                {
                    (_ClipGeoPlygon as IGeometry).Project(targetSpatialReference);//对定位几何进行投影变换
                }
                //更新
                updateMapPaperInfo(true);
            }
        }

        private void btn_Preview_Click(object sender, EventArgs e)
        {
            if (!updateMapPaperInfo())
                return;

            DialogResult = DialogResult.OK;
        }

        private void btOK_Click(object sender, EventArgs e)
        {
            if (!updateMapPaperInfo())
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void AdministrativeRegeionLocationForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (DialogResult != DialogResult.OK)
            {
                _app.MapControl.ActiveView.GraphicsContainer.DeleteAllElements();
                _app.MapControl.ActiveView.Refresh();
            }
        }


        //根据比例尺更新纸张尺寸
        private void ModifyPageByScale(double scale)
        {
            if (_ClipGeoPlygon == null)
                return;

            ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
            if (_ClipGeoPlygon.SpatialReference.Name != targetSpatialReference.Name)
            {
                (_ClipGeoPlygon as IGeometry).Project(targetSpatialReference);//对定位几何进行投影变换
            }

            double mapwidth = _ClipGeoPlygon.Envelope.Width * 1.1 / scale * 1000;//毫米,裁切面扩展1.1倍
            double mapheight = _ClipGeoPlygon.Envelope.Height * 1.1 / scale * 1000;//毫米，裁切面扩展1.1倍
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
                        if ((inlineWidth - mapwidth > 0 && inlineWidth1 - mapwidth < 0) || (inlineHeight - mapheight > 0 && inlineHeight1 - mapheight < 0))
                        {
                            _scaleChangedState = true;
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
                        if ((inlineHeight - mapheight > 0 && inlineHeight1 - mapheight < 0) || (inlineWidth - mapwidth > 0 && inlineWidth1 - mapwidth < 0))
                        {
                            _scaleChangedState = true;
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
                        if ((p.Height - mapheight > 0 && p1.Height - mapheight < 0) || (p.Width - mapwidth > 0 && p1.Width - mapwidth < 0))
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

        //遍历节点的子节点，更新裁切几何面
        private void updateClipGeoByCheckedNode(TreeNode node)
        {
            for (int i = 0; i < node.Nodes.Count; ++i)
            {
                var childNode = node.Nodes[i];
                if (childNode.Checked)
                {
                    string pac = childNode.Name.Trim();
                    IPolygon geo = null;
                    if (_pac2NameGeo2.ContainsKey(pac))
                        geo = _pac2NameGeo2[pac].Value;
                    else if (_pac2NameGeo4.ContainsKey(pac))
                        geo = _pac2NameGeo4[pac].Value;
                    else if (_pac2NameGeo5.ContainsKey(pac))
                        geo = _pac2NameGeo5[pac].Value;
                    else if (_pac2NameGeo6.ContainsKey(pac))
                        geo = _pac2NameGeo6[pac].Value;

                    if (geo == null)
                        continue;

                    if (_ClipGeoPlygon == null)
                    {
                        _ClipGeoPlygon = (geo as IClone).Clone() as IPolygon;
                    }
                    else
                    {
                        ITopologicalOperator to = _ClipGeoPlygon as ITopologicalOperator;
                        _ClipGeoPlygon = to.Union(geo) as IPolygon;
                    }

                    if (!_clipPACList.Contains(pac))
                        _clipPACList.Add(pac);
                }
                else
                {
                    //递归子节点
                    updateClipGeoByCheckedNode(childNode);
                }
            }
        }

        //根据当前比例尺，重新获取当前比例尺下的裁切面
        private void updateClipGeoByScale(double scale)
        {
            XElement expertiseContent = ExpertiseDatabase.getContentElement(GApplication.Application);
            var mapScaleRule = expertiseContent.Element("MapScaleRule");
            var scaleItems = mapScaleRule.Elements("Item");
            string targetBDName = "";
            foreach (XElement ele in scaleItems)
            {
                string database = (string)ele.Element("DatabaseName");
                double min = double.Parse(ele.Element("Min").Value);
                double max = double.Parse(ele.Element("Max").Value);
                if (scale >= min && scale <= max)
                {
                    targetBDName = database;
                    break;
                }
            }
            if (targetBDName == "")
            {
                MessageBox.Show("无法访问当前尺度的服务器数据库！");
                return;
            }

            if (targetBDName != _curServerDatabase)
            {
                _ClipGeoPlygon = null;//重新初始化为空

                IWorkspace sdeWorkspace = _app.GetWorkspacWithSDEConnection(_serverIPAddress, _userName, _password, targetBDName);
                IFeatureWorkspace sdeFeatureWorkspace = sdeWorkspace as IFeatureWorkspace;
                Dictionary<string, IFeatureClass> bouaFCList = new Dictionary<string, IFeatureClass>();
                if ((sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _boua2FCName))
                {
                    if (!bouaFCList.ContainsKey(_boua2FCName))
                    {
                        IFeatureClass fc = sdeFeatureWorkspace.OpenFeatureClass(_boua2FCName);
                        bouaFCList.Add(_boua2FCName, fc);
                    }
                }
                if ((sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _boua4FCName))
                {
                    if (!bouaFCList.ContainsKey(_boua4FCName))
                    {
                        IFeatureClass fc = sdeFeatureWorkspace.OpenFeatureClass(_boua4FCName);
                        bouaFCList.Add(_boua4FCName, fc);
                    }
                }
                if ((sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _boua5FCName))
                {
                    if (!bouaFCList.ContainsKey(_boua5FCName))
                    {
                        IFeatureClass fc = sdeFeatureWorkspace.OpenFeatureClass(_boua5FCName);
                        bouaFCList.Add(_boua5FCName, fc);
                    }
                }
                if ((sdeWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass, _boua6FCName))
                {
                    if (!bouaFCList.ContainsKey(_boua6FCName))
                    {
                        IFeatureClass fc = sdeFeatureWorkspace.OpenFeatureClass(_boua6FCName);
                        bouaFCList.Add(_boua6FCName, fc);
                    }
                }

                IFeature fe = null;
                foreach (var kv in bouaFCList)
                {
                    int pacIndex = kv.Value.FindField("pac");
                    if (pacIndex == -1)
                        continue;

                    IFeatureCursor feCursor = kv.Value.Search(null, true);
                    while ((fe = feCursor.NextFeature()) != null)
                    {
                        string pac = fe.get_Value(pacIndex).ToString().Trim();
                        if (fe.Shape == null || fe.Shape.IsEmpty)
                            continue;

                        if (!_clipPACList.Contains(pac))
                            continue;

                        int fdIndex = fe.Fields.FindField("fd");
                        if (fdIndex != -1)
                        {
                            var fdValue = fe.get_Value(fdIndex);
                            if (!Convert.IsDBNull(fdValue) && fdValue.ToString().Trim() != "")
                                continue;//飞地：fd字段不为空，且也不为空字符串
                        }

                        if (_ClipGeoPlygon == null)
                        {
                            _ClipGeoPlygon = fe.ShapeCopy as IPolygon;
                        }
                        else
                        {
                            _ClipGeoPlygon = (_ClipGeoPlygon as ITopologicalOperator).Union(fe.ShapeCopy) as IPolygon;
                        }
                    }
                    Marshal.ReleaseComObject(feCursor);
                }

                if (_ClipGeoPlygon == null)
                {
                    MessageBox.Show("在相应尺度数据库中该地区的几何为空！", "提示");
                    return;
                }


                //设置目标空间坐标系
                ISpatialReference targetSpatialReference = CommonMethods.getClipSpatialRef();
                if (null == targetSpatialReference)
                    targetSpatialReference = _app.MapControl.Map.SpatialReference;
                if (_ClipGeoPlygon.SpatialReference.Name != targetSpatialReference.Name)
                {
                    (_ClipGeoPlygon as IGeometry).Project(targetSpatialReference);//对定位几何进行投影变换
                }

                _curServerDatabase = targetBDName;
            }
        }                       

        //更新出图纸张信息
        private bool updateMapPaperInfo(bool bUpdateScale = false)
        {
            #region 判断当前输入值是否合理
            //裁切几何
            if (_ClipGeoPlygon == null)
            {
                MessageBox.Show("请指定裁切几何面！");
                return false;
            }

            //比例尺
            double refScale;
            bool res = double.TryParse(cbRefSclae.Text, out refScale);
            if (!res || refScale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");
                return false;
            }

            //宽度
            double paperWidth;
            res = double.TryParse(tbPaperWidth.Text, out paperWidth);
            if (!res || paperWidth <= 0)
            {
                MessageBox.Show("输入的纸张宽度不合法！");
                return false;
            }

            //高度
            double paperHeight;
            res = double.TryParse(tbPaperHeight.Text, out paperHeight);
            if (!res || paperHeight <= 0)
            {
                MessageBox.Show("输入的纸张高度不合法！");
                return false;
            }


            //成图尺寸宽度
            double MapSizeWidth;
            res = double.TryParse(tbMapSizeWidth.Text, out MapSizeWidth);
            if (!res || MapSizeWidth <= 0)
            {
                MessageBox.Show("输入的成图尺寸宽度不合法！");
                return false;
            }

            //成图尺寸高度
            double MapSizeHeight;
            res = double.TryParse(tbMapSizeHeight.Text, out MapSizeHeight);
            if (!res || MapSizeHeight <= 0)
            {
                MessageBox.Show("输入的成图尺寸高度不合法！");
                return false;
            }

            //内图廓宽度
            double InlineWidth;
            res = double.TryParse(tbInlineWidth.Text, out InlineWidth);
            if (!res || InlineWidth <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸宽度不合法！");
                return false;
            }

            //内图廓高度
            double InlineHeight;
            res = double.TryParse(tbInlineHeight.Text, out InlineHeight);
            if (!res || InlineHeight <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return false;
            }

            //内外图廓间距
            double InOutLineWidth;
            res = double.TryParse(tbInOutWidth.Text, out InOutLineWidth);
            if (!res || InOutLineWidth < 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return false;
            }

            if (paperWidth < MapSizeWidth || paperHeight < MapSizeHeight)
            {
                MessageBox.Show("纸张尺寸不能小于成图尺寸！");
                return false;
            }

            if (InlineWidth + InOutLineWidth > MapSizeWidth || InlineHeight + InOutLineWidth > MapSizeHeight)
            {
                MessageBox.Show("成图尺寸不能小于外图廓尺寸！");
                return false;
            }
            #endregion

            using (var wo = _app.SetBusy())
            {
                if (bUpdateScale)
                {
                    //根据内图廓的尺寸更新裁切几何面
                    wo.SetText("正在根据内图廓的尺寸更新参考比例尺......");
                    refScale = CommonLocationMethods.CaluateMapScale(InlineWidth, InlineHeight, _ClipGeoPlygon);
                    cbRefSclae.Text = refScale.ToString();
                }


                //根据当前比例尺，重新获取当前比例尺下的裁切面
                wo.SetText("正在根据参考比例尺,获取当前比例尺下的裁切几何面......");
                updateClipGeoByScale(refScale);
                if (_ClipGeoPlygon == null)
                    return false;

                //存储地图各尺寸
                CommonMethods.SetMapPar(paperWidth, paperHeight, MapSizeWidth, MapSizeHeight, InlineWidth, InlineHeight, InOutLineWidth, refScale);

                var ps = new PaperSize { PaperWidth = double.Parse(tbPaperWidth.Text), PaperHeight = double.Parse(tbPaperHeight.Text) };

                //更新配置表
                EnvironmentSettings.updateMapScale(_app, refScale);
                EnvironmentSettings.updatePageSize(_app, paperWidth, paperHeight);
                CommonMethods.CalculateSptialRef(_ClipGeoPlygon.Envelope);
                double dx = 0;
                double dy = 0;
                IGroupElement clipELe = ClipElement.createClipGroupElementEx2(_app, _ClipGeoPlygon, InlineWidth, InlineHeight, refScale, dx, dy);
                ClipElement.SetClipGroupElement(_app, clipELe);
                CommonMethods.clipEx = true;
            }

            return true;
        }

        private void btnImportParams_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "定位参数|*.xml";
            dlg.FileName = "定位参数";
            dlg.InitialDirectory = GApplication.Application.Template.Root + @"\专家库\数据定位";
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            double scale = 0, paperWidth = 0, paperHeight = 0, mapWidth = 0, mapHeight = 0, innerBorderWidth = 0, innerBorderHeight = 0, borderSpacing = 0;
            CommonLocationMethods.ImportLocationParamsXML(dlg.FileName, ref scale, ref paperWidth, ref paperHeight, ref mapWidth, ref mapHeight, ref innerBorderWidth, ref innerBorderHeight, ref borderSpacing);

            cbPaperSize.Text = "预定义尺寸";
            cbRefSclae.Text = scale.ToString();
            tbPaperWidth.Text = paperWidth.ToString();
            tbPaperHeight.Text = paperHeight.ToString();
            tbMapSizeWidth.Text = mapWidth.ToString();
            tbMapSizeHeight.Text = mapHeight.ToString();
            tbInlineWidth.Text = innerBorderWidth.ToString();
            tbInlineHeight.Text = innerBorderHeight.ToString();
            tbInOutWidth.Text = borderSpacing.ToString();
        }

        private void btnExportParams_Click(object sender, EventArgs e)
        {
            //比例尺
            double scale;
            bool res = double.TryParse(cbRefSclae.Text, out scale);
            if (!res || scale <= 0)
            {
                MessageBox.Show("输入的比例尺不合法！");
                return ;
            }

            //宽度
            double paperWidth;
            res = double.TryParse(tbPaperWidth.Text, out paperWidth);
            if (!res || paperWidth <= 0)
            {
                MessageBox.Show("输入的纸张宽度不合法！");
                return ;
            }

            //高度
            double paperHeight;
            res = double.TryParse(tbPaperHeight.Text, out paperHeight);
            if (!res || paperHeight <= 0)
            {
                MessageBox.Show("输入的纸张高度不合法！");
                return ;
            }


            //成图尺寸宽度
            double mapWidth;
            res = double.TryParse(tbMapSizeWidth.Text, out mapWidth);
            if (!res || mapWidth <= 0)
            {
                MessageBox.Show("输入的成图尺寸宽度不合法！");
                return ;
            }

            //成图尺寸高度
            double mapHeight;
            res = double.TryParse(tbMapSizeHeight.Text, out mapHeight);
            if (!res || mapHeight <= 0)
            {
                MessageBox.Show("输入的成图尺寸高度不合法！");
                return ;
            }

            //内图廓宽度
            double innerBorderWidth;
            res = double.TryParse(tbInlineWidth.Text, out innerBorderWidth);
            if (!res || innerBorderWidth <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸宽度不合法！");
                return ;
            }

            //内图廓高度
            double innerBorderHeight;
            res = double.TryParse(tbInlineHeight.Text, out innerBorderHeight);
            if (!res || innerBorderHeight <= 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return ;
            }

            //内外图廓间距
            double borderSpacing;
            res = double.TryParse(tbInOutWidth.Text, out borderSpacing);
            if (!res || borderSpacing < 0)
            {
                MessageBox.Show("输入的内图廓尺寸高度不合法！");
                return ;
            }

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "定位参数|*.xml";
            dlg.FileName = "定位参数";
            dlg.InitialDirectory = GApplication.Application.Template.Root + @"\专家库\数据定位";
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            res = CommonLocationMethods.ExportLocationParamsXML(dlg.FileName, scale, paperWidth, paperHeight, mapWidth, mapHeight, innerBorderWidth, innerBorderHeight,borderSpacing);
            if (res)
            {
                MessageBox.Show("导出成功!", "提示");
            }
            else
            {
                MessageBox.Show("导出失败!", "提示");
            }
        }

        
  
    }
}
