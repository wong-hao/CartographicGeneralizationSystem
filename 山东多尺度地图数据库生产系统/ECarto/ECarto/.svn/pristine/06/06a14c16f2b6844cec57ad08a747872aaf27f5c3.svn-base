using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Windows.Forms;
using System.Drawing;
using SMGI.Common;
using System.Data;
using System.Runtime.InteropServices;
using stdole;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using System.Xml.Linq;
namespace SMGI.Plugin.EmergencyMap
{

    public class LaceParam
    {
        public double Length;
        public string Name;
        public int ImgIndex;
        public int CornerImgIndex;
        public int Angle;//必须正方位角
        public bool Flip;
    }

    /// <summary>
    /// 图廓整饰类：负责生成0.图廓线,1.图名，2.格网线，3.图例，4指北针，5比例尺
    /// </summary>
    public class MapLayoutHelperLH
    {
        private const double MM2PIXEL = 2.8346456692913385826771653543307;
     


        public IGeometry pageGeo = null;//页面矩形
        IGeometry clipGeo = null;//裁切面几何
        IEnvelope clipGeoIn = null;//内图廓矩形
        public IEnvelope clipGeoOut = null;//外图廓矩形
        IFeatureClass pointfcl = null;//图廓点图层
        IFeatureClass linefcl = null;//图廓线图层
        IFeatureClass gridfcl = null;//格网线线图层
        IFeatureClass polygonfcl = null;//图廓面图层：
        GApplication m_Application = GApplication.Application;
        double mapScale = 0;

        IPoint orginPoint = null;
        IFeatureClass pFclAnno = null;//图廓注记图层
        DataTable ruleDt = null;//图廓规则
        DataTable ruleDtCOM = null;//图廓规则 通用
        Dictionary<string, int> ruleIDcopass = new Dictionary<string, int>();//图廓规则指北针
        Dictionary<string, int> ruleIDlace = new Dictionary<string, int>();//图廓规则花边
        Dictionary<string, double> ruleRatlace = new Dictionary<string, double>();//图廓规则花边长宽比
        Dictionary<string, int> ruleIDPolygon = new Dictionary<string, int>();//图廓规则
        Dictionary<string, int> ruleIDLines = new Dictionary<string, int>();//图廓规则格网线
        Dictionary<string, int> ruleIDlegend = new Dictionary<string, int>();//图廓规则图例
        Dictionary<string, int> ruleIDline = new Dictionary<string, int>();//图廓规则图廓线
        Dictionary<string, int> ruleIDlpoly = new Dictionary<string, int>();//图廓规则图廓面
        IQueryFilter qf = new QueryFilterClass();

        private IRepresentationClass _mRepClass = null;//自由表达：指北针，比例尺，图例
        private string templatefcl = EnvironmentSettings.getTemplateFclName();//整饰模板名称            
        private IWorkspace repWscp = null;//改指北针

        double fontunit = 88.0 / 5.0 / 50000.0;//水平字体尺寸1的字体所占的高度和宽度
        double fontunitV = 490 / 25.0 / 50000.0;//垂直字体，尺寸1的字体所占的高度
        public MapLayoutHelperLH(string NAME = "")
        {
            if (NAME == string.Empty) { NAME = "LACE"; }
            IntialData(NAME);
        }
        private void IntialData(string NAME)
        {

            string templatecp = m_Application.Template.Root + @"\整饰\DRESS.gdb";
            IWorkspaceFactory wsFactorycp = new FileGDBWorkspaceFactoryClass();
            IWorkspace wscp = wsFactorycp.OpenFromFile(templatecp, 0);
            repWscp = wscp;

            #region

            var content = EnvironmentSettings.getContentElement(m_Application);
            string tempmapScale = content.Element("MapScale").Value;
            double.TryParse(tempmapScale, out mapScale);

            mapScale = m_Application.ActiveView.FocusMap.ReferenceScale;

            string mdbpath = m_Application.Template.Root + @"\整饰\整饰规则库.mdb";
            string mdbname = NAME;
            ruleDtCOM = CommonMethods.ReadToDataTable(mdbpath, "COMMON");//通用RuleID
            DataRow[] drs1 = ruleDtCOM.Select();
            foreach (DataRow dr in drs1)//COMPASS
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDPolygon[keyname] = val;//!!
            }

            ruleDt = CommonMethods.ReadToDataTable(mdbpath, mdbname);
            DataRow[] drs = ruleDt.Select("图层='COMPASS'");
            foreach (DataRow dr in drs)//COMPASS
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDcopass[keyname] = val;//!!
            }
            drs = ruleDt.Select("图层='LACE'");
            foreach (DataRow dr in drs)
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDlace[keyname] = val;
                ruleRatlace[keyname] = double.Parse(dr["LenWRatio"].ToString());
            }
            drs = ruleDt.Select("图层='LEGEND'");
            foreach (DataRow dr in drs)
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDlegend[keyname] = val;
            }
            drs = ruleDt.Select("图层='GRIDLINE'");
            foreach (DataRow dr in drs)
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDLines[keyname] = val;
            }
            drs = ruleDt.Select("图层='ClipBoundary'");
            foreach (DataRow dr in drs)
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDPolygon[keyname] = val;
            }
            drs = ruleDt.Select("图层='LLINE'");
            foreach (DataRow dr in drs)
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDline[keyname] = val;
            }
            drs = ruleDt.Select("图层='LPOLY'");
            foreach (DataRow dr in drs)
            {
                string keyname = dr["RuleName"].ToString();
                int val = int.Parse(dr["RuleID"].ToString());
                ruleIDlpoly[keyname] = val;
            }

            IFeatureWorkspace fws = m_Application.Workspace.EsriWorkspace as IFeatureWorkspace;
            IWorkspace2 pws2 = m_Application.Workspace.EsriWorkspace as IWorkspace2;

            if (pws2.get_NameExists(esriDatasetType.esriDTFeatureClass, "LPOINT"))
            {
                pointfcl = fws.OpenFeatureClass("LPOINT");
                (pointfcl as IFeatureClassLoad).LoadOnlyMode = false;
            }
            if (pws2.get_NameExists(esriDatasetType.esriDTFeatureClass, "GRID"))
            {
                gridfcl = fws.OpenFeatureClass("GRID");
                (gridfcl as IFeatureClassLoad).LoadOnlyMode = false;
            }
            if (pws2.get_NameExists(esriDatasetType.esriDTFeatureClass, "LLINE"))
            {
                linefcl = fws.OpenFeatureClass("LLINE");
                (linefcl as IFeatureClassLoad).LoadOnlyMode = false;
                qf.WhereClause = "TYPE like '%图廓%'";

                if (linefcl.FeatureCount(qf) > 0)
                {
                    IFeature fe = null;
                    IFeatureCursor cursor = linefcl.Search(qf, false);
                    while ((fe = cursor.NextFeature()) != null)
                    {
                        string type = fe.get_Value(linefcl.FindField("TYPE")).ToString();
                        if (type == "内图廓")
                        {
                            clipGeoIn = fe.ShapeCopy.Envelope;
                        }
                        else if (type == "外图廓")
                        {
                            clipGeoOut = fe.ShapeCopy.Envelope;
                        }

                    }
                    Marshal.ReleaseComObject(cursor);
                }
            }
            if (pws2.get_NameExists(esriDatasetType.esriDTFeatureClass, "LPOLY"))
            {
                #region
                polygonfcl = fws.OpenFeatureClass("LPOLY");
                (polygonfcl as IFeatureClassLoad).LoadOnlyMode = false;

                #endregion
            }
            if (pws2.get_NameExists(esriDatasetType.esriDTFeatureClass, "LANNO"))
            {
                pFclAnno = fws.OpenFeatureClass("LANNO");
                (pFclAnno as IFeatureClassLoad).LoadOnlyMode = false;
                IAnnoClass annoClass = pFclAnno.Extension as IAnnoClass;
                IAnnoClassAdmin3 admin3 = annoClass as IAnnoClassAdmin3;
                admin3.AllowSymbolOverrides = true;
                admin3.ReferenceScale = m_Application.ActiveView.FocusMap.ReferenceScale;
                admin3.UpdateProperties();

            }
            if (pws2.get_NameExists(esriDatasetType.esriDTFeatureClass, "ClipBoundary"))
            {
                #region
                IFeatureClass clipfcl = fws.OpenFeatureClass("ClipBoundary");
                if (clipfcl.FeatureCount(null) != 0)
                {
                    IFeature fe = null;
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = "TYPE = '页面'";
                    IFeatureCursor cursor = clipfcl.Search(qf, false);
                    fe = cursor.NextFeature();
                    pageGeo = fe.ShapeCopy;
                    //
                    qf.WhereClause = "TYPE = '裁切面'";
                    cursor = clipfcl.Search(qf, false);
                    fe = cursor.NextFeature();
                    clipGeo = fe.ShapeCopy;
                    Marshal.ReleaseComObject(cursor);
                }
                #endregion
            }
            #endregion
        }
        public double MapNameSpace = 0;//图名字符间距
        public double MapNameWidth = 100;//图名宽度
        public double MapNameDis =5;//图名图廓间距
        public double ProducerDis=3;//生产者与外图廓间距
        public double TimeDis = 3;//生产时间与外图廓间距
        /// <summary>
        /// 生成图名
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="font">字体</param>
        /// <param name="MMapNameHorizon">true图名水平排列,false垂直排列</param>
        public void CreateMapName(bool shadow, string name, IFontDisp font, IColor pcolor,double interval=5,bool MapNameHorizon=true)
        {
            double fontsize = Convert.ToDouble(font.Size);
            if (clipGeoOut == null)
            {
                MessageBox.Show("请先生成图廓线！");
                return;
            }

            double dx = 0, dy = 0;
            
            //水平排列
            if (MapNameHorizon)
            {
               
                dx = (clipGeoOut.UpperLeft.X + clipGeoOut.UpperRight.X) / 2;
                dy = (clipGeoOut.UpperLeft.Y + clipGeoOut.UpperRight.Y) / 2 + (fontsize / 2.83 / 2 + interval) * mapScale / 1000;
            }

            //垂直排列
            if (!MapNameHorizon)
            {
                
                dx = clipGeoIn.UpperRight.X - (fontsize / 2.83 / 2 + interval*2) * mapScale / 1000;
                //dy = (clipGeoIn.UpperLeft.Y + clipGeoIn.UpperRight.Y) / 2 - (fontsize * (name.Length+1 )*(1+MapNameSpace*0.01/2) / 2.0 / 2.83 + interval*2) * mapScale / 1000; 
                dy = (clipGeoIn.UpperLeft.Y + clipGeoIn.UpperRight.Y) / 2 - ((name.Length * (fontsize / 2.83)) / 2.0 + +interval * 2.0) * mapScale / 1000;
                if (MapNameSpace != 0)
                {
                    dy -= ((name.Length - 1) * (MapNameSpace * 0.01) * (fontsize / 2.83) / 2.0 + +interval * 2.0) * mapScale / 1000;
                }
                if (MapNameWidth != 100)
                {
                    dy -= (name.Length * ((MapNameWidth - 100) * 0.01 * fontsize / 2.83) / 2.0) * mapScale / 1000;
                }
            }


            IPoint p = new PointClass() { X = dx, Y = dy };
            //清空图名
            qf.WhereClause = "TYPE = '图名'";
            ClearFeatures(pFclAnno, qf);

            qf.WhereClause = "TYPE = '图名内边线'";
            ClearFeatures(polygonfcl, qf);
            if (shadow)//创建艺术字
            {
                string path = m_Application.Template.Root + @"\整饰\图名\MapNameRule.Xml";
               
                XDocument doc;
                TreeNode annoNode = new TreeNode("注记图例");
                List<string> templyrs = new List<string>();
                
                {
                    doc = XDocument.Load(path);
                    var content = doc.Element("Template");
                    var lyrs = content.Elements("layer");
                    foreach (var lyr in lyrs)
                    {
                        templyrs.Add(lyr.Value);
                    }

                }
                Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance
                      (factoryType);
                string temp = m_Application.Template.Root + @"\整饰\图名\图名规则库.gdb";
                IWorkspace wsTemp = workspaceFactory.OpenFromFile(temp, 0);

                Dictionary<string, ITextElement> eles = new Dictionary<string, ITextElement>();
                foreach (var str in templyrs)
                {
                    var anno = (wsTemp as IFeatureWorkspace).OpenFeatureClass(str);
                    var cursor = anno.Search(null, false);
                    var fe = cursor.NextFeature();
                    IAnnotationFeature pAnnoFeature = fe as IAnnotationFeature;
                    eles[str] = pAnnoFeature.Annotation as ITextElement;
                    Marshal.ReleaseComObject(cursor);
                }
                pColor = pcolor as IRgbColor;        
                InsertArtAnnoFea("图名", p, name, font, eles,MapNameHorizon);
            }
            else
            {
                p = new PointClass() { X = dx, Y = dy };
                pColor = pcolor as IRgbColor;
                InsertAnnoFea("图名", p, font, name, Convert.ToDouble(font.Size),esriTextVerticalAlignment.esriTVACenter, esriTextHorizontalAlignment.esriTHACenter, MapNameHorizon);
            }
            if (!MapNameHorizon)
            {
                CreaterMapNamePolygon(interval,fontsize);
            }
            m_Application.ActiveView.Refresh();
        }

        
        private void CreaterMapNamePolygon(double interval,double fontsize)
        {
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == "LPOLY")).FirstOrDefault();  
            IFeatureClass polygonfcl = (lyrs as IFeatureLayer).FeatureClass;
            var Annolyrs = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LANNO")).FirstOrDefault();
            IFeatureClass Annofcl = (Annolyrs as IFeatureLayer).FeatureClass;
            //清空图名外的边线          
            qf.WhereClause = "TYPE = '图名内边线'";
            ClearFeatures(polygonfcl, qf);
            qf.WhereClause = "TYPE = '图名'";
            IFeature mapNameFea= Annofcl.Search(qf, true).NextFeature();
            if(mapNameFea==null) return;
            
            IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;
            IPointCollection pRing = new RingClass();
            //pRing.AddPoint(new PointClass() { X = mapNameFea.Shape.Envelope.XMin - interval * mapScale / 1000, Y = mapNameFea.Shape.Envelope.YMin+fontsize*2/2.83*mapScale/1000 - interval * mapScale / 1000 });//左下角
            //pRing.AddPoint(new PointClass() { X = mapNameFea.Shape.Envelope.XMax + interval * mapScale / 1000, Y = mapNameFea.Shape.Envelope.YMin +fontsize*2 / 2.83 * mapScale / 1000 - interval * mapScale / 1000 });//右下角
            //pRing.AddPoint(new PointClass() { X = mapNameFea.Shape.Envelope.XMax + interval * mapScale / 1000, Y = mapNameFea.Shape.Envelope.YMax - fontsize*2 / 2.83 * mapScale / 1000 + interval * mapScale / 1000 });//右上角
            //pRing.AddPoint(new PointClass() { X = mapNameFea.Shape.Envelope.XMin - interval * mapScale / 1000, Y = mapNameFea.Shape.Envelope.YMax - fontsize*2 / 2.83 * mapScale / 1000 + interval * mapScale / 1000 });//左上角   

            double mapNameMiddleY=(mapNameFea.ShapeCopy.Envelope.YMax+mapNameFea.ShapeCopy.Envelope.YMin)/2;//图名中心位置Y
            double mapNameMiddleX = (mapNameFea.ShapeCopy.Envelope.XMax + mapNameFea.ShapeCopy.Envelope.XMin) / 2;//图名中心位置X

            pRing.AddPoint(new PointClass() { X = clipGeoIn.UpperRight.X - (clipGeoIn.UpperRight.X - mapNameMiddleX) * 2, Y = clipGeoIn.UpperRight.Y - (clipGeoIn.UpperRight.Y - mapNameMiddleY) * 2 - interval * mapScale / 1000 });//左下角
            pRing.AddPoint(new PointClass() { X = clipGeoIn.UpperRight.X, Y = clipGeoIn.UpperRight.Y - (clipGeoIn.UpperRight.Y - mapNameMiddleY) * 2 - interval * mapScale / 1000 });//右下角
            pRing.AddPoint(new PointClass() { X = clipGeoIn.UpperRight.X, Y = clipGeoIn.UpperRight.Y });//右上角
            pRing.AddPoint(new PointClass() { X = clipGeoIn.UpperRight.X - (clipGeoIn.UpperRight.X - mapNameMiddleX) * 2, Y = clipGeoIn.UpperRight.Y });//左上角 


            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            IFeatureCursor editcursor = polygonfcl.Insert(true);
            IFeatureBuffer featurebuffer = null;            
            featurebuffer = polygonfcl.CreateFeatureBuffer();
            featurebuffer.Shape = geoCol as IGeometry;
            int TukuoRuleID=CommonMethods.GetRuleIDByRuleName("LPOLY", "图廓");
            if (TukuoRuleID >= 0)
            {
                featurebuffer.set_Value(polygonfcl.FindField("RuleID"), TukuoRuleID);
                featurebuffer.set_Value(polygonfcl.FindField("TYPE"), "图名内边线");
                editcursor.InsertFeature(featurebuffer);
            }
                
                
           
            
        }


        /// <summary>
        /// 生成生产商名与生产时间
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="font">字体</param>
        public void CreateMapProducer(string name, IFontDisp font, IColor pcolor, string template)
        {
            double fontsize = Convert.ToDouble(font.Size) / 72 * 2.54 * 10;
            if (clipGeoOut == null)
            {
                MessageBox.Show("请先生成图廓线！");
                return;
            }

            double dx = 0, dy = 0, dxt = 0, dyt = 0;
            #region
            double fontlenth = fontsize * (name.Length);
            switch (template)
            {
                case "生产商":

                    dx = clipGeoIn.LowerLeft.X;
                    //dx = clipGeoOut.LowerLeft.X + (fontlenth + 30) * mapScale / 1000 / 2;
                    dy = clipGeoOut.LowerLeft.Y - (fontsize  / 2 +ProducerDis ) * mapScale / 1000; 
                    IPoint p = new PointClass() { X = dx, Y = dy };
                    //清空生产商名
                    qf.WhereClause = "TYPE = '生产商'";
                    ClearFeatures(pFclAnno, qf);
                    pColor = pcolor as IRgbColor;
                    InsertAnnoFea("生产商", p, font, name, Convert.ToDouble(font.Size),esriTextVerticalAlignment.esriTVACenter,esriTextHorizontalAlignment.esriTHALeft);
                    break;
                case "生产时间":

                    dxt = clipGeoIn.LowerRight.X;
                    //dxt = clipGeoOut.LowerRight.X - (fontlenth + 20) * mapScale / 1000 / 2;
                    dyt = clipGeoOut.LowerRight.Y - (fontsize / 2 + TimeDis) * mapScale / 1000;
                    IPoint p2 = new PointClass() { X = dxt, Y = dyt };
                    //清空生产时间
                    qf.WhereClause = "TYPE = '生产时间'";
                    ClearFeatures(pFclAnno, qf);
                    pColor = pcolor as IRgbColor;
                    InsertAnnoFea("生产时间", p2, font, name, Convert.ToDouble(font.Size), esriTextVerticalAlignment.esriTVACenter, esriTextHorizontalAlignment.esriTHARight);
                    break;
            }
            #endregion
        }

        public void CreateMapPublishType(string text, IFontDisp font, IColor pcolor, double dist = 3, bool bHorizontal = true)
        {
            string labelText = text;
            if (labelText.Trim() == "")
                return;

            double fontsize = Convert.ToDouble(font.Size) / 72 * 2.54 * 10;
            if (clipGeoOut == null)
            {
                MessageBox.Show("请先生成图廓线！");
                return;
            }

            double fontlenth = fontsize * (text.Length);
            
            #region
            pColor = pcolor as IRgbColor;

            double dx = 0, dy = 0;
            if (bHorizontal)
            {
                dx = clipGeoIn.UpperRight.X;
                //dx = clipGeoOut.UpperRight.X - (fontlenth + 20) * mapScale / 1000 / 2;
                dy = clipGeoOut.UpperRight.Y + (fontsize / 2 + dist) * mapScale / 1000;
                IPoint pos = new PointClass() { X = dx, Y = dy };

                InsertAnnoFea("出版形式", pos, font, labelText, Convert.ToDouble(font.Size),esriTextVerticalAlignment.esriTVACenter, esriTextHorizontalAlignment.esriTHARight);
            }
            else
            {
                dx = clipGeoOut.UpperRight.X + (fontsize / 2 + dist) * mapScale / 1000;
                dy = clipGeoIn.UpperRight.Y;
                IPoint pos = new PointClass() { X = dx, Y = dy };

                labelText = "";
                for (int i = 0; i < text.Length; i++)
                {
                    labelText += text[i].ToString() + "\r\n";
                }
 
                InsertAnnoFea("出版形式", pos, font, labelText, Convert.ToDouble(font.Size), esriTextVerticalAlignment.esriTVATop);
            }

            
            
            #endregion
        }

        /// <summary>
        /// 内外图廓线
        /// </summary>
        /// <param name="outwidth">外图廓与裁切线上部间距：默认100</param>
        ///  <param name="outwidth">外图廓与裁切线下部间距：默认100</param>
        ///  <param name="outwidth">外图廓与裁切线左部间距：默认100</param>
        ///  <param name="outwidth">外图廓与裁切线右部间距：默认100</param>
        /// <param name="inwidth">内外图廓间距：默认16</param>
        public void CreateMapBorderLine0(string outUpWidth, string outDownWidth, string outLeftWidth, string outRightWidth, string inwidth_)
        {

            qf.WhereClause = "TYPE like '%图廓%'";
            ClearFeatures(linefcl, qf);
            ClearFeatures(polygonfcl, qf);

            qf.WhereClause = "TYPE like '%遮盖%'";
            ClearFeatures(polygonfcl, qf);
            if (mapScale == 0)
            {
                MessageBox.Show("请先设置出图比例尺！");
                return;
            }
            if (pageGeo == null)
            {
                MessageBox.Show("页面几何不存在!");
                return;
            }
            if (polygonfcl == null)
            {
                MessageBox.Show("图廓面图层不存在!");
                return;
            }

            int indexRule = linefcl.Fields.FindField("RULEID");
            int indexType = linefcl.Fields.FindField("TYPE");
            //外图廓
            double widthup = Convert.ToDouble(outUpWidth);//宽度
            widthup = widthup / 1000 * mapScale;
            double widthdown = Convert.ToDouble(outDownWidth);//宽度
            widthdown = widthdown / 1000 * mapScale;
            double widthleft = Convert.ToDouble(outLeftWidth);//宽度
            widthleft = widthleft / 1000 * mapScale;
            double widthright = Convert.ToDouble(outRightWidth);//宽度
            widthright = widthright / 1000 * mapScale;
            //四周平移后新结果
            string outfeaturename = "outline";
            IGeometry pgeo = TransFormGeometry(widthup, widthdown, widthleft, widthright, outfeaturename);
            IFeature fe = linefcl.CreateFeature();

            //内图廓
            double inwidthup = Convert.ToDouble(inwidth_);//宽度
            inwidthup = inwidthup / 1000 * mapScale + widthup;
            double inwidthdown = Convert.ToDouble(inwidth_);//宽度
            inwidthdown = inwidthdown / 1000 * mapScale + widthdown;
            double inwidthleft = Convert.ToDouble(inwidth_);//宽度
            inwidthleft = inwidthleft / 1000 * mapScale + widthleft;
            double inwidthright = Convert.ToDouble(inwidth_);//宽度
            inwidthright = inwidthright / 1000 * mapScale + widthright;
            //四周平移后新结果
            string infeaturename = "inline";
            IGeometry pgeoin = TransFormGeometry(inwidthup, inwidthdown, inwidthleft, inwidthright, infeaturename);

            //图廓纸张
            IEnvelope pExtentpage = ((pageGeo as IClone).Clone() as IGeometry).Envelope;
            //       pExtentpage.Expand(1-(1 / 1000 * mapScale) / pExtentpage.Width, 1-(1 / 1000 * mapScale) / pExtentpage.Height, true);
            IEnvelope pExtent = ((pgeo as IClone).Clone() as IGeometry).Envelope;
            IGeometryCollection geoCol = new PolygonClass();//图廓面
            IGeometryCollection geoColout = new PolylineClass();//生成外图廓线
            IGeometryCollection geoColin = new PolylineClass();//生成内图廓线
            IGeometryCollection geoColoverlap = new PolygonClass();//生成遮盖面
            object _missing = Type.Missing;

            IPointCollection pRingpage = new RingClass();
            pRingpage.AddPoint(pExtentpage.LowerLeft);
            pRingpage.AddPoint(pExtentpage.LowerRight);
            pRingpage.AddPoint(pExtentpage.UpperRight);
            pRingpage.AddPoint(pExtentpage.UpperLeft);
            pRingpage.AddPoint(pExtentpage.LowerLeft);
            (pRingpage as IRing).Close();
            geoColoverlap.AddGeometry(pRingpage as IGeometry, ref _missing, ref _missing);//加入纸张环

            IPointCollection pRing = new RingClass();
            pRing.AddPoint(pExtent.LowerLeft);
            pRing.AddPoint(pExtent.LowerRight);
            pRing.AddPoint(pExtent.UpperRight);
            pRing.AddPoint(pExtent.UpperLeft);
            pRing.AddPoint(pExtent.LowerLeft);
            (pRing as IRing).Close();
            IPointCollection pRingount = new PathClass();
            pRingount.AddPoint(pExtent.LowerLeft);
            pRingount.AddPoint(pExtent.LowerRight);
            pRingount.AddPoint(pExtent.UpperRight);
            pRingount.AddPoint(pExtent.UpperLeft);
            pRingount.AddPoint(pExtent.LowerLeft);
            //  pRing.AddPoint(pExtent.LowerLeft);
            //  (pRing as IPath).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            geoColout.AddGeometry(pRingount as IGeometry, ref _missing, ref _missing);

            IEnvelope pExtent1 = ((pgeoin as IClone).Clone() as IGeometry).Envelope;
            IPointCollection pRing1 = new RingClass();
            pRing1.AddPoint(pExtent1.LowerLeft);
            pRing1.AddPoint(pExtent1.LowerRight);
            pRing1.AddPoint(pExtent1.UpperRight);
            pRing1.AddPoint(pExtent1.UpperLeft);
            pRing1.AddPoint(pExtent1.LowerLeft);
            (pRing1 as IRing).Close();
            IPointCollection pRing1in = new PathClass();
            pRing1in.AddPoint(pExtent1.LowerLeft);
            pRing1in.AddPoint(pExtent1.LowerRight);
            pRing1in.AddPoint(pExtent1.UpperRight);
            pRing1in.AddPoint(pExtent1.UpperLeft);
            pRing1in.AddPoint(pExtent1.LowerLeft);
            //  (pRing1 as IRing).Close();
            geoCol.AddGeometry(pRing1 as IGeometry, ref _missing, ref _missing);
            geoColin.AddGeometry(pRing1in as IGeometry, ref _missing, ref _missing);
            geoColoverlap.AddGeometry(pRing1 as IGeometry, ref _missing, ref _missing);//加入内图廓环

            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            IGeometry polygon = geoCol as IGeometry;
            ITopologicalOperator topologicalOperatorout = geoColout as ITopologicalOperator;
            IGeometry polylineout = geoColout as IGeometry;
            ITopologicalOperator topologicalOperatorin = geoColin as ITopologicalOperator;
            IGeometry polylinein = geoColin as IGeometry;
            ITopologicalOperator topologicalOperatorpage = geoColoverlap as ITopologicalOperator;
            IGeometry polylinepage = geoColoverlap as IGeometry;
            topologicalOperator.Simplify();//生成图廓面
            topologicalOperatorout.Simplify();//生成外图廓线
            topologicalOperatorin.Simplify();//生成内图廓线
            topologicalOperatorpage.Simplify();//生成遮盖面

            fe.Shape = polylineout;
            fe.set_Value(indexRule, ruleIDline["外图廓"]);
            fe.set_Value(indexType, "外图廓");
            fe.Store();

            fe = linefcl.CreateFeature();
            fe.Shape = polylinein;
            fe.set_Value(indexRule, ruleIDline["内图廓"]);
            fe.set_Value(indexType, "内图廓");
            fe.Store();
            int indexRulePAGE = polygonfcl.Fields.FindField("RULEID");
            int indexTypePAGE = polygonfcl.Fields.FindField("TYPE");
            IFeature fePAGE = polygonfcl.CreateFeature();
            fePAGE.Shape = polygon;
            fePAGE.set_Value(indexRulePAGE, ruleIDlpoly["图廓"]);
            fePAGE.set_Value(indexTypePAGE, "图廓纸张");
            fePAGE.Store();

            fePAGE = polygonfcl.CreateFeature();
            fePAGE.Shape = polylinepage;
            fePAGE.set_Value(indexRulePAGE, ruleIDPolygon["遮盖"]);
            fePAGE.set_Value(indexTypePAGE, "遮盖");
            fePAGE.Store();
            m_Application.ActiveView.Refresh();
        }
        public void CreateMapBorderLine(string outUpWidth, string outDownWidth, string outLeftWidth, string outRightWidth, string inwidth_, bool cbClip=false)
        {

            qf.WhereClause = "TYPE like '%图廓%'";
            ClearFeatures(linefcl, qf);
            ClearFeatures(polygonfcl, qf);

            qf.WhereClause = "TYPE like '%遮盖%'";
            ClearFeatures(polygonfcl, qf);
            if (mapScale == 0)
            {
                MessageBox.Show("请先设置出图比例尺！");
                return;
            }
            if (pageGeo == null)
            {
                MessageBox.Show("页面几何不存在!");
                return;
            }
            if (polygonfcl == null)
            {
                MessageBox.Show("图廓面图层不存在!");
                return;
            }

            int indexRule = linefcl.Fields.FindField("RULEID");
            int indexType = linefcl.Fields.FindField("TYPE");
            //外图廓
            double widthup = Convert.ToDouble(outUpWidth);//
            widthup = widthup / 1000 * mapScale;
            double widthdown = Convert.ToDouble(outDownWidth);//
            widthdown = widthdown / 1000 * mapScale;
            double widthleft = Convert.ToDouble(outLeftWidth);//
            widthleft = widthleft / 1000 * mapScale;
            double widthright = Convert.ToDouble(outRightWidth);//
            widthright = widthright / 1000 * mapScale;

            
            double xmin = pageGeo.Envelope.XMin + widthleft;
            double xmax = pageGeo.Envelope.XMax - widthright;
            double ymin = pageGeo.Envelope.YMin + widthdown;
            double ymax = pageGeo.Envelope.YMax - widthup;
            //间隔 内外图廓
            double inwidthup = Convert.ToDouble(inwidth_);//
            double interval=inwidthup / 1000 * mapScale;
            if (cbClip)//以裁切面的范围为内图廓
            {
                IEnvelope envelop = clipGeo.Envelope;
                double torrence = 6.0e-3 * m_Application.ActiveView.FocusMap.ReferenceScale;
                xmin = envelop.XMin - torrence - interval;
                xmax = envelop.XMax + torrence + interval;
                ymin = envelop.YMin - torrence - interval;
                ymax = envelop.YMax + torrence + interval;
            }
            //四周平移后新结果
          
            IEnvelope en = new EnvelopeClass();
            en.PutCoords(xmin, ymin, xmax, ymax);
            IElement ele = new RectangleElementClass();
            ele.Geometry = en;
            IGeometry pgeoout = ele.Geometry;

            //内图廓
            double xminIn = xmin + interval;
            double xmaxIn = xmax-interval;
            double yminIn = ymin+interval;
            double ymaxIn = ymax - interval;
            en = new EnvelopeClass();
            en.PutCoords(xminIn, yminIn, xmaxIn, ymaxIn);
            ele = new RectangleElementClass();
            ele.Geometry = en;
            IGeometry pgeoin = ele.Geometry;
            
            //
            double xminlap=Math.Min(xmin,pageGeo.Envelope.XMin)-100;
            double xmaxlap=Math.Max(xmax,pageGeo.Envelope.XMax)+100;
            double yminlap=Math.Min(ymin,pageGeo.Envelope.YMin)-100;
            double ymaxlap=Math.Max(ymax,pageGeo.Envelope.YMax)+100;
            IGeometryCollection geoCol = new PolygonClass();//生成遮盖面
            #region
            IPointCollection ringout = new RingClass();
            ringout.AddPoint(new PointClass { X = xminlap, Y = ymaxlap });
            ringout.AddPoint(new PointClass { X = xmaxlap, Y = ymaxlap });
            ringout.AddPoint(new PointClass { X = xmaxlap, Y = yminlap });
            ringout.AddPoint(new PointClass { X = xminlap, Y = yminlap });
            ringout.AddPoint(new PointClass { X = xminlap, Y = ymaxlap });
            geoCol.AddGeometry(ringout as IGeometry);

            IPointCollection ringin = new RingClass();
            ringout.AddPoint(new PointClass { X = xminIn, Y = ymaxIn });
            ringout.AddPoint(new PointClass { X = xmaxIn, Y = ymaxIn });
            ringout.AddPoint(new PointClass { X = xmaxIn, Y = yminIn });
            ringout.AddPoint(new PointClass { X = xminIn, Y = yminIn });
            ringout.AddPoint(new PointClass { X = xminIn, Y = ymaxIn });
            geoCol.AddGeometry(ringin as IGeometry);
            (geoCol as ITopologicalOperator).Simplify();
            #endregion
            IPointCollection polylineout = new PolylineClass();//生成外图廓线
            polylineout.AddPointCollection(pgeoout as IPointCollection);
            (polylineout as ITopologicalOperator).Simplify();
            IPointCollection polylinein = new PolylineClass();//生成内图廓线
            polylinein.AddPointCollection(pgeoin as IPointCollection);
            (polylinein as ITopologicalOperator).Simplify();
            IGeometry polygon=(pgeoout as ITopologicalOperator).Difference(pgeoin);//图廓面

          
            var maplineLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                    return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LLINE");
            })).FirstOrDefault() as IFeatureLayer;

            var polygonLyr = m_Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == "LPOLY");
            })).FirstOrDefault() as IFeatureLayer;
          
            //添加要素
            IFeature fe = linefcl.CreateFeature();
            fe.Shape = polylineout as IGeometry;
            fe.set_Value(indexRule, ruleIDline["外图廓"]);
            fe.set_Value(indexType, "外图廓");
            fe.Store();
            m_Application.ActiveView.FocusMap.SelectFeature(maplineLyr,fe);
            fe = linefcl.CreateFeature();
            fe.Shape = polylinein as IGeometry;
            fe.set_Value(indexRule, ruleIDline["内图廓"]);
            fe.set_Value(indexType, "内图廓");
            fe.Store();
            m_Application.ActiveView.FocusMap.SelectFeature(maplineLyr, fe);

            int indexRulePAGE = polygonfcl.Fields.FindField("RULEID");
            int indexTypePAGE = polygonfcl.Fields.FindField("TYPE");
            IFeature fePAGE = polygonfcl.CreateFeature();
            fePAGE.Shape = polygon;
            fePAGE.set_Value(indexRulePAGE, ruleIDlpoly["图廓"]);
            fePAGE.set_Value(indexTypePAGE, "图廓纸张");
            fePAGE.Store();
            m_Application.ActiveView.FocusMap.SelectFeature(polygonLyr, fePAGE);
            
            fePAGE = polygonfcl.CreateFeature();
            fePAGE.Shape = geoCol as IGeometry;
            fePAGE.set_Value(indexRulePAGE, ruleIDPolygon["遮盖"]);
            fePAGE.set_Value(indexTypePAGE, "遮盖");
            fePAGE.Store();
           // m_Application.ActiveView.FocusMap.SelectFeature(polygonLyr, fePAGE);
            //局部刷新模式
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, (polylineout as IGeometry).Envelope);
            m_Application.ActiveView.FocusMap.ClearSelection();
        }

        /// <summary>
        /// 格网线生成
        /// </summary>
        /// <param name="gridname">格网类型</param>
        /// <param name="dx">格网行距</param>
        /// <param name="dy">格网列距</param>
        /// <param name="annodx">注记、序号尺寸</param>
        /// <param name="annody">注记、序号与内图廓间距：默认2 毫米</param>
        public void CreateGridLine(string gridname, double dx_, double dy_, double ansize, double aninterval, string gdb)
        {

            if (clipGeoIn == null)
            {

                MessageBox.Show("请先生成图廓线！");
                return;
            }
            if (mapScale == 0)
            {
                MessageBox.Show("请先设置出图比例尺！");
                return;

            }
            qf.WhereClause = "TYPE like '%格网线%'";
            ClearFeatures(pointfcl, qf);
            ClearFeatures(gridfcl, qf);
            ClearFeatures(pFclAnno, qf);
            switch (gridname)
            {
                case "方里网":
                    CreateMeatureGrid(dx_, dy_, ansize, aninterval);
                    break;
                case "经纬网":
                    CreateGraticuleGrid(dx_, dy_, ansize, aninterval);
                    break;
                default://索引网
                    CreateIndexGrid(dx_, dy_, ansize, aninterval, gdb);
                    break;
            }
        }

        #region 图例相关参数

        private IRepresentationRule lengendRule = null;
        private IRepresentationRule themeRule = null;

        private double offsetX = 14;//专题图例  x 间距
        private double offsetY = 4;//专题图例  y 间距
        private double lengendHeight = 145;//图例边框高
        public double lengendWidth = 145;//图例边框宽
        #endregion
        /// <summary>
        /// 图例
        /// </summary>
        /// <param name="themename">专题名称</param>
        public void CreateLengend(string themename, string basethemename, string gdbname, string templatename = "")
        {
            pColor = new RgbColorClass() { Red = 0, Blue = 0, Green = 0 };//默认颜色
            qf.WhereClause = "TYPE like '%图例%'";
            ClearFeatures(pFclAnno, qf);
            ClearFeatures(pointfcl, qf);
            ClearFeatures(polygonfcl, qf);
            GetLengendRepRule(themename, gdbname, basethemename);
            //计算图例高度
            var pAtrs = lengendRule.get_Layer(0) as IGraphicAttributes;
            var marker = pAtrs.get_Value(0) as IRepresentationMarker;
            IPoint orginPoint = null;
            IPoint point = null;
            switch (templatename)
            {
                case "103":
                case "104":
                case "101":
                    //图例注记
                    orginPoint = clipGeoIn.LowerRight;
                    point = new PointClass() { X = orginPoint.X - (lengendWidth / 2) * mapScale / 1000, Y = orginPoint.Y + (lengendHeight - 12) * mapScale / 1000 };
                    AddLendgendAnnotion(point, "图   例");
                    //专题图例
                    if (themeRule != null)
                    {
                        point = new PointClass() { X = orginPoint.X - (lengendWidth) * mapScale / 1000 + offsetX * mapScale / 1000, Y = orginPoint.Y + (lengendHeight - 19) * mapScale / 1000 };
                        CreateLengend(point, themeRule);
                    }
                    //基本图例
                    point = new PointClass() { X = orginPoint.X - (lengendWidth) * mapScale / 1000, Y = orginPoint.Y + (lengendHeight - 19 - offsetY) * mapScale / 1000 };
                    CreateLengend(point, lengendRule);
                    //图例边框
                    CreateLengendLineRight();
                    break;
                default:
                    orginPoint = clipGeoIn.LowerLeft;
                    //图例注记
                    point = new PointClass() { X = orginPoint.X + (lengendWidth / 2) * mapScale / 1000, Y = orginPoint.Y + (lengendHeight - 12) * mapScale / 1000 };
                    AddLendgendAnnotion(point, "图   例");
                    //专题图例
                    if (themeRule != null)
                    {
                        point = new PointClass() { X = orginPoint.X + offsetX * mapScale / 1000, Y = orginPoint.Y + (lengendHeight - 19) * mapScale / 1000 };
                        CreateLengend(point, themeRule);
                    }
                    //基本图例
                    point = new PointClass() { X = orginPoint.X, Y = orginPoint.Y + (lengendHeight - 19 - offsetY) * mapScale / 1000 };
                    CreateLengend(point, lengendRule);
                    //图例边框
                    CreateLengendLine();
                    break;
            }
        }
        public void CreateLengendEx(string themename, string basethemename, string gdbname, string location)
        {
            pColor = new RgbColorClass() { Red = 0, Blue = 0, Green = 0 };//默认颜色
            qf.WhereClause = "TYPE like '%图例%'";
            ClearFeatures(pFclAnno, qf);
            ClearFeatures(pointfcl, qf);
            ClearFeatures(polygonfcl, qf);
            GetLengendRepRule(themename, gdbname, basethemename);
            //计算图例高度,宽度不变！
            var pAtrs = lengendRule.get_Layer(0) as IGraphicAttributes;
            var marker = pAtrs.get_Value(1) as IRepresentationMarker;
            double legSize = Math.Max(marker.Width, marker.Height);
            double markersize = (lengendWidth - 15 * lengendWidth / 145) * 2.83;
            if (marker.Width > marker.Height)
            {
                lengendHeight = marker.Height / legSize * lengendWidth + 40 * lengendWidth / 145;
                pAtrs.set_Value(2, markersize);
            }
            else//高度高
            {
                lengendHeight = marker.Height / marker.Width * lengendWidth + 40 * lengendWidth / 145;
                //修改marker值
                markersize = marker.Height / marker.Width * markersize;
                pAtrs.set_Value(2, markersize);
            }
            IPoint orginPoint = null;
            IPoint point = null;
            switch (location)
            {
                case "左上":
                    orginPoint = clipGeoIn.UpperLeft;
                    orginPoint.Y -= lengendHeight * mapScale * 1e-3;
                    break;
                case "右上":
                    orginPoint = clipGeoIn.UpperRight;
                    orginPoint.X -= lengendWidth * mapScale * 1e-3;
                    orginPoint.Y -= lengendHeight * mapScale * 1e-3;
                    break;
                case "左下":
                    orginPoint = clipGeoIn.LowerLeft;
                    break;
                case "右下":
                    orginPoint = clipGeoIn.LowerRight;
                    orginPoint.X -= lengendWidth * mapScale * 1e-3;
                    break;
                default:
                    break;

            }
            //orginPoint = clipGeoIn.LowerLeft;
            //图例注记
            point = new PointClass() { X = orginPoint.X + (lengendWidth / 2) * mapScale / 1000, Y = orginPoint.Y + (lengendHeight - 12) * mapScale / 1000 };
            AddLendgendAnnotion(point, "图   例");
            //专题图例
            if (themeRule != null)
            {
                point = new PointClass() { X = orginPoint.X + offsetX * mapScale / 1000, Y = orginPoint.Y + (lengendHeight - 19) * mapScale / 1000 };
                CreateLengend(point, themeRule);
            }
            //基本图例
            point = new PointClass() { X = orginPoint.X, Y = orginPoint.Y + (lengendHeight - 19 - offsetY) * mapScale / 1000 };
            CreateLengend(point, lengendRule);
            //图例边框
            CreateLengendLine(orginPoint, location);
        }


        #region 指北针参数
        private IRepresentationRule compassRule = null;
        private string compassName = "";
        #endregion
        /// <summary>
        /// 生成指北针
        /// </summary>
        public void CreateCompass(string compassName_, string gdbname, string size,string location="右上")
        {
            compassName = compassName_;
            IntialCompassRepRule(gdbname);
            qf.WhereClause = "TYPE like '%指北针%'";
            ClearFeatures(pointfcl, qf);
            IPoint uprightp = clipGeoIn.UpperRight;
            double xtemp = double.Parse(size);
            xtemp += 10;
            IPoint point = new PointClass() { X = uprightp.X - xtemp / 2 * mapScale / 1000, Y = uprightp.Y - xtemp / 2 * mapScale / 1000 };
            switch (location)
            {
                case "左上":
                    point = new PointClass() { X = clipGeoIn.UpperLeft.X + xtemp / 2 * mapScale / 1000, Y = clipGeoIn.UpperLeft.Y - xtemp / 2 * mapScale / 1000 };
                    break;
                case "左下":
                    point = new PointClass() { X = clipGeoIn.LowerLeft.X + xtemp / 2 * mapScale / 1000, Y = clipGeoIn.LowerLeft.Y + xtemp / 2 * mapScale / 1000 };
                    break;
                case "右上":
                    point = new PointClass() { X = uprightp.X - xtemp / 2* mapScale / 1000, Y = uprightp.Y - xtemp / 2 * mapScale / 1000 };
                    break;
                case "右下":
                    point = new PointClass() { X = clipGeoIn.LowerRight.X - xtemp / 2 * mapScale / 1000, Y = clipGeoIn.LowerRight.Y + xtemp / 2 * mapScale / 1000 };
                    break;
                default:
                    break;
            }
            xtemp = double.Parse(size);
            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = point;
            fe.set_Value(pointfcl.FindField("TYPE"), "指北针");
            fe.Store();
            //          (pointfcl as IFeatureClassLoad).LoadOnlyMode = false;
            //获取当前要素的制图表达
            var mc = new MapContextClass();
            mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            // IRepresentationWorkspaceExtension pRepWksExt = GetRepersentationWorkspace(m_Application.Workspace.EsriWorkspace);
            IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
            IRepresentationClass g_RepClass = pRepWksExt.OpenRepresentationClass(pointfcl.AliasName);
            IRepresentation rep = g_RepClass.GetRepresentation(fe, mc);
            var ruletemp = (compassRule as IClone).Clone() as IRepresentationRule;
            var bs = ruletemp.get_Layer(0);
            (bs as IGraphicAttributes).set_Value(2, xtemp*2.83);
            //如果地图旋转，则需设置指北针旋转相应的角度
            if (m_Application.Workspace.MapConfig["MapAngle"]!=null)
            {
                double angle = (double)m_Application.Workspace.MapConfig["MapAngle"];
                (bs as IGraphicAttributes).set_Value(3, angle);
            }
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(point, ruletemp);

            rep.Graphics = repGraphics;
            rep.UpdateFeature();
            rep.Feature.Store();
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, fe, null);
        }

        #region 比例尺参数
        private IRepresentationRule scaleRule = null;
        private string scaleName = "";
        public string ScaleLoaction = "图例内";
        public string ScaleUnit = "km";
        #endregion
        /// <summary>
        /// 创建比例尺
        /// </summary>
        public void CreateScaleBar(string scaleName_, string gdbname,string templatename = "")
        {
            pColor = new RgbColorClass() { Red = 0, Blue = 0, Green = 0 };//默认颜色
            scaleName = scaleName_;
            double dx = 50;
            double dy = 10;
            if (clipGeoIn == null)
            {
                MessageBox.Show("请先生成图廓线!");
                return;
            }
            if (mapScale == 0)
            {
                MessageBox.Show("请先设置出图比例尺！");
                return;

            }
            qf.WhereClause = "TYPE='图名'";
            IFeatureCursor cursor = pFclAnno.Search(qf, false);
            IFeature fename = cursor.NextFeature();
            

            IQueryFilter qfle = new QueryFilterClass();
            qfle.WhereClause = "TYPE='图例内边线'";
            IFeatureCursor cursorle = polygonfcl.Search(qfle, false);
            IFeature linele = cursorle.NextFeature();
            double xlocation = 0;
            double ylocation = 0;
            if (linele != null)
            {
                IEnvelope lineenvelope = linele.ShapeCopy.Envelope;
                double linexMax = lineenvelope.XMax;
                double linexMin = lineenvelope.XMin;
                xlocation = lineenvelope.LowerLeft.X + (linexMax - linexMin - dx * mapScale / 1000) / 2;
                ylocation = lineenvelope.YMin;
            }
            double x = 0, y = 0;
            switch (templatename)
            {

                case "102":
                    x = clipGeoIn.LowerRight.X - (50 + dx) * mapScale / 1000;
                    y = clipGeoIn.LowerRight.Y + dy * mapScale / 1000;
                    break;
                case "103":
                    if (fename != null)
                    {
                        IAnnotationFeature annofe = fename as IAnnotationFeature;
                        double fontsize = (annofe.Annotation as ITextElement).Symbol.Size;
                        dy = fontsize * fontunit * mapScale + 5 * mapScale / 1000;
                        y = clipGeoIn.UpperLeft.Y - 15 * mapScale / 1000 - dy;
                        x = clipGeoIn.UpperLeft.X + 5 * mapScale / 1000;
                    }
                    else
                    {
                        y = clipGeoIn.UpperLeft.Y - 25 * mapScale / 1000;
                        x = clipGeoIn.UpperLeft.X + 5 * mapScale / 1000;
                    }
                    break;
                default://100,104,101
                    if (xlocation != 0 && ScaleLoaction=="图例内")
                    {
                        x = xlocation;
                        y = ylocation + 3 * mapScale / 1000;
                    }
                    else
                    {
                        switch (ScaleLoaction)
                        {
                            case "左下":
                                x = clipGeoIn.LowerLeft.X + dx * mapScale / 1000;
                                y = clipGeoIn.LowerLeft.Y + dy * mapScale / 1000;
                                break;
                            case "左上":
                                x = clipGeoIn.UpperLeft.X + dx * mapScale / 1000;
                                y = clipGeoIn.UpperLeft.Y - dy * mapScale / 1000;
                                break;
                            case "右上":
                                x = clipGeoIn.UpperRight.X - 2*dx * mapScale / 1000;//减去比例尺的实际长度
                                y = clipGeoIn.UpperRight.Y - dy * mapScale / 1000;
                                break;
                            case "右下":
                            default:
                                x = clipGeoIn.LowerRight.X - 2*dx * mapScale / 1000;//减去比例尺的实际长度
                                y = clipGeoIn.LowerRight.Y + dy * mapScale / 1000;
                                break;
                        }
                    }
                    break;
            }
            IntialScaleBarRepRule(scaleName, gdbname);
            qf.WhereClause = "TYPE='比例尺'";
            ClearFeatures(pointfcl, qf);
            ClearFeatures(pFclAnno, qf);

            IPoint point = new PointClass() { X = x, Y = y };
            orginPoint = point;
            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = point;
            fe.set_Value(pointfcl.FindField("TYPE"), "比例尺");
            fe.Store();
            (pointfcl as IFeatureClassLoad).LoadOnlyMode = false;
            //获取当前要素的制图表达
            var mc = new MapContextClass();
            mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            // IRepresentationWorkspaceExtension pRepWksExt = GetRepersentationWorkspace(m_Application.Workspace.EsriWorkspace);
            IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
            IRepresentationClass g_RepClass = pRepWksExt.OpenRepresentationClass(pointfcl.AliasName);
            IRepresentation rep = g_RepClass.GetRepresentation(fe, mc);

            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(point, scaleRule);
            rep.Graphics = repGraphics;
            rep.UpdateFeature();
            rep.Feature.Store();

            bool bDigitScale = false;
            if (scaleName.Contains("数字"))
            {
                bDigitScale = true;
            }

            AddScaleAnnotion(fe, bDigitScale);
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewBackground, null, clipGeoIn);
            Marshal.ReleaseComObject(cursor);
            Marshal.ReleaseComObject(cursorle);
            Marshal.ReleaseComObject(qf);
            Marshal.ReleaseComObject(qfle);
        }
        //添加图例说明注记
        public void CreateScaleBar(string scaleName_, string gdbname, List<string> notes,double fontsize=3)
        {
            pColor = new RgbColorClass() { Red = 0, Blue = 0, Green = 0 };//默认颜色
            scaleName = scaleName_;
            double dx = 50;
            double dy = 10;
            if (clipGeoIn == null)
            {
                MessageBox.Show("请先生成图廓线!");
                return;
            }
            if (mapScale == 0)
            {
                MessageBox.Show("请先设置出图比例尺！");
                return;

            }
            qf.WhereClause = "TYPE='图名'";
            IFeatureCursor cursor = pFclAnno.Search(qf, false);
            IFeature fename = cursor.NextFeature();


            IQueryFilter qfle = new QueryFilterClass();
            qfle.WhereClause = "TYPE='图例内边线'";
            IFeatureCursor cursorle = polygonfcl.Search(qfle, false);
            IFeature linele = cursorle.NextFeature();
            double xlocation = 0;
            double ylocation = 0;
            double txtPointX = 0;
            if (linele != null)
            {
                IEnvelope lineenvelope = linele.ShapeCopy.Envelope;
                double linexMax = lineenvelope.XMax;
                double linexMin = lineenvelope.XMin;
                xlocation = lineenvelope.LowerLeft.X + (linexMax - linexMin - dx * mapScale / 1000) / 2;
                txtPointX = (lineenvelope.LowerLeft.X + lineenvelope.LowerRight.X) / 2;
                ylocation = lineenvelope.YMin;
            }
            double x = 0, y = 0;
            
            if (xlocation != 0 && ScaleLoaction == "图例内")
            {
                x = xlocation;
                y = ylocation + 3 * mapScale / 1000;
            }
            else
            {
                switch (ScaleLoaction)
                {
                    case "左下":
                        x = clipGeoIn.LowerLeft.X + dx * mapScale / 1000;
                        y = clipGeoIn.LowerLeft.Y + dy * mapScale / 1000;
                        break;
                    case "左上":
                        x = clipGeoIn.UpperLeft.X + dx * mapScale / 1000;
                        y = clipGeoIn.UpperLeft.Y - dy * mapScale / 1000;
                        break;
                    case "右上":
                        x = clipGeoIn.UpperRight.X - 2 * dx * mapScale / 1000;//减去比例尺的实际长度
                        y = clipGeoIn.UpperRight.Y - dy * mapScale / 1000;
                        break;
                    case "右下":
                    default:
                        x = clipGeoIn.LowerRight.X - 2 * dx * mapScale / 1000;//减去比例尺的实际长度
                        y = clipGeoIn.LowerRight.Y + dy * mapScale / 1000;
                        break;
                }
            }
           
            IntialScaleBarRepRule(scaleName, gdbname);
            qf.WhereClause = "TYPE='比例尺'";
            ClearFeatures(pointfcl, qf);
            ClearFeatures(pFclAnno, qf);

            IPoint point = new PointClass() { X = x, Y = y };
            if (notes.Count > 0)
            {
                point.Y += (fontsize + 0.2) * notes.Count * 0.001 * mapScale;//行距默认字体大小的0.2
            }

            orginPoint = point;
            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = point;
            fe.set_Value(pointfcl.FindField("TYPE"), "比例尺");
            fe.Store();
            (pointfcl as IFeatureClassLoad).LoadOnlyMode = false;
            //获取当前要素的制图表达
            var mc = new MapContextClass();
            mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            // IRepresentationWorkspaceExtension pRepWksExt = GetRepersentationWorkspace(m_Application.Workspace.EsriWorkspace);
            IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
            IRepresentationClass g_RepClass = pRepWksExt.OpenRepresentationClass(pointfcl.AliasName);
            IRepresentation rep = g_RepClass.GetRepresentation(fe, mc);

            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(point, scaleRule);
            rep.Graphics = repGraphics;
            rep.UpdateFeature();
            rep.Feature.Store();
             
            bool bDigitScale = false;
            if (scaleName.Contains("数字"))
            {
                bDigitScale = true;
            }
            
            AddScaleAnnotion(fe, bDigitScale);
            double noteY = point.Y - (2.0 + fontsize * 0.5) * mapScale / 1000;//第一行附注说明文本的位置
            for (int i = 0; i < notes.Count; i++)
            {
                IFontDisp pFont = new StdFont()
                {
                    Name = "宋体"
                } as IFontDisp;
                var noteGeometry = new PointClass() { X = txtPointX, Y = noteY - i * (fontsize + 0.2) * mapScale / 1000};
                InsertScaleAnnoFea(fe, "比例尺", noteGeometry, pFont, notes[i], fontsize * 2.83);
            }
            m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewBackground, null, clipGeoIn);
            Marshal.ReleaseComObject(cursor);
            Marshal.ReleaseComObject(cursorle);
            Marshal.ReleaseComObject(qf);
            Marshal.ReleaseComObject(qfle);
        }
        //批量读取花边图元 提高效率
        private List<IRepresentationGraphics> GetRepresentationGraphicsList(List<LaceParam> laces, string gdbname)
        {
            IFeatureClass fc = (repWscp as IFeatureWorkspace).OpenFeatureClass(gdbname);
            IWorkspace ws = repWscp;

            //根据数据库获取制图表达要素类
            IRepresentationWorkspaceExtension repWS = GetRepersentationWorkspace(ws);

            if (!repWS.get_FeatureClassHasRepresentations(fc))
            {

                return null;
            }

            var enumDatasetName = repWS.get_FeatureClassRepresentationNames(fc);
            enumDatasetName.Reset();
            var pDatasetName = enumDatasetName.Next();
            var repClass = repWS.OpenRepresentationClass(pDatasetName.Name);
            var rules = repClass.RepresentationRules;

            var rglist = new List<IRepresentationGraphics>();
            IRepresentationRule rule = null;

            //不能用foreach!
            for (var i = 0; i < laces.Count; i++)
            {
                rules.Reset();
                int ruleID;
                while (true)
                {
                    rules.Next(out ruleID, out rule);
                    //必须找到，否则 异常
                    if (rule == null)
                    {

                        return null;
                    }
                    if (rules.Name[ruleID] == laces[i].Name)
                    {
                        break;
                    }
                }

                var attrs = rule.Layer[0] as IGraphicAttributes;
                var graphics = attrs.Value[1] as IRepresentationGraphics;

                rglist.Add(graphics);
            }

            return rglist;
        }

        /// <summary>
        ///  创建花边
        /// </summary>
        /// <param name="laces">花边符号参数</param>
        /// <param name="cornerLace">花边角</param>
        /// <param name="laceWidth">花边宽度</param>
        /// <param name="len">花边与外边框间距</param>
        public void CreateFootBorder2(List<LaceParam> laces, LaceParam cornerLace, double laceW, double len, string gdbname)
        {
            try
            {
                //初始化
                ILayer repLayer = GApplication.Application.Workspace.LayerManager.GetLayer(l => (l is IGeoFeatureLayer) && ((l as IGeoFeatureLayer).FeatureClass.AliasName.ToUpper() == ("LPOINT"))).FirstOrDefault();

                IFeatureClass pointfcl = (repLayer as IFeatureLayer).FeatureClass;
                var filter = new QueryFilterClass { WhereClause = "TYPE like '%花边%'" };
                ClearFeatures(pointfcl, filter);
                IMapContext mc = new MapContextClass();
                mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);


                IQueryFilter qfle = new QueryFilterClass();
                qfle.WhereClause = "TYPE='外图廓'";
                IFeatureCursor cursorle = linefcl.Search(qfle, false);
                IFeature clip = cursorle.NextFeature();

                IEnvelope clipenvelope = null;
                if (clip != null)
                {
                    clipenvelope = clip.Shape.Envelope;
                }
                else { MessageBox.Show("请先生成图廓线！"); return; }


                double pageWidth = clipenvelope.Width - 2 * len * 1.0e-3 * mapScale;//纸张宽度-花边与外图廓间距
                double pageHeight = clipenvelope.Height - 2 * len * 1.0e-3 * mapScale;//纸张高度-花边与外图廓间距


                var laceWidth = laceW * mapScale * 1.0e-3;
                IPoint ctp = (clipenvelope as IArea).Centroid;
                IPoint upleft = new PointClass() { X = ctp.X - pageWidth / 2, Y = ctp.Y + pageHeight / 2 };
                IPoint downright = new PointClass { X = ctp.X + pageWidth / 2, Y = ctp.Y - pageHeight / 2 };
                double cornerLen = 0;

                if (cornerLace.Length < 1e-3)
                    cornerLen = laceWidth * ruleRatlace[cornerLace.Name];
                else
                    cornerLen = cornerLace.Length * mapScale * 1e-3;



                var corner = GetRepresentationGraphicsList(new List<LaceParam> { cornerLace }, gdbname);
                double cornerSize;

                double cornerRat;
                if (cornerLace.Length < 1e-3)
                {
                    cornerSize = laceW * ruleRatlace[cornerLace.Name] * MM2PIXEL;
                    cornerRat = ruleRatlace[cornerLace.Name];
                }
                else
                {
                    cornerRat = cornerLace.Length / laceW;
                    cornerSize = cornerLace.Length * MM2PIXEL;
                }



                double laceLen = 0;
                var lacelens = new List<double>();
                var laceRatis = new List<double>();
                foreach (var lace in laces)
                {
                    double ll;
                    if (lace.Length < 1e-3)
                    {
                        laceRatis.Add(ruleRatlace[lace.Name]);
                        ll = laceWidth * ruleRatlace[lace.Name];
                    }
                    else
                    {
                        laceRatis.Add(lace.Length / laceW);
                        ll = lace.Length * mapScale * 1e-3;
                    }

                    lacelens.Add(ll);
                    laceLen += ll;
                }




                //创建要素
                IFeatureCursor editcursor = pointfcl.Insert(true);

                var rglist = GetRepresentationGraphicsList(laces, gdbname);

                Func<int, double> markSizeFunc = (idx) =>
                {
                    if (laces[idx].Length < 1e-3)
                        return laceW * ruleRatlace[laces[idx].Name] * MM2PIXEL;
                    return laces[idx].Length * MM2PIXEL;
                };
                var tmplen = pageWidth - cornerLen * 2;
                //上边下边 

                double cnt = Math.Round(tmplen / laceLen);//组数
                double ratX = tmplen / cnt / laceLen;

                var grpX = (int)cnt;

                //上

                #region 上--------------------------------------------------
                var rules = new List<IRepresentationRule>();
                for (var i = 0; i < laces.Count; i++)
                {
                    double markSize = markSizeFunc(i);
                    rules.Add(ObtainFootRepRule(laceRatis[i], laces[i].Angle, laces[i].Flip, ratX, 1, markSize * ratX, rglist[i]));//Math.Max(ratX, 1) * justi
                }

                double tmp = upleft.X + cornerLen;
                //从左到右 上
                for (var i = 0; i < grpX; i++)
                {
                    for (var j = 0; j < rules.Count; j++)//不能foreach
                    {
                        StorePoint(new PointClass() { X = tmp, Y = upleft.Y }, rules[j], editcursor, mc);
                        tmp += lacelens[j] * ratX;
                    }
                }
                #endregion 上

                //下  旋转180
                rules.Clear();
                #region 下 ----------------------------------------------
                for (var i = 0; i < laces.Count; i++)
                {
                    double markSize = markSizeFunc(i);
                    rules.Add(ObtainFootRepRule(laceRatis[i], laces[i].Angle + 180, laces[i].Flip, ratX, 1, markSize * ratX, rglist[i]));//* Math.Max(ratX, 1) * justi
                }


                tmp = upleft.X + cornerLen;
                for (var i = grpX; i > 0; i--)
                {
                    for (var j = rules.Count - 1; j >= 0; j--)//不能foreach
                    {
                        StorePoint(new PointClass() { X = tmp, Y = downright.Y + laceWidth }, rules[j], editcursor, mc);
                        tmp += lacelens[j] * ratX;
                    }
                }
                #endregion 下

                tmplen = pageHeight - cornerLen * 2;
                cnt = Math.Round(tmplen / laceLen);//组数
                double ratY = tmplen / cnt / laceLen;
                var grpY = (int)cnt;

                //右 旋转-90 
                rules.Clear();
                #region 右 ---------------------------------------
                for (var i = 0; i < laces.Count; i++)
                {
                    double markSize = markSizeFunc(i);
                    rules.Add(ObtainFootRepRule(laceRatis[i], laces[i].Flip ? laces[i].Angle + 90 : laces[i].Angle - 90, laces[i].Flip, 1, ratY, markSize * ratY, rglist[i]));//* Math.Max(ratY, 1)
                }

                tmp = upleft.Y - cornerLen;
                //从上到下
                for (var i = 0; i < grpY; i++)
                {
                    for (var j = 0; j < rules.Count; j++)//不能foreach
                    {
                        StorePoint(new PointClass() { X = -laceWidth + downright.X, Y = tmp }, rules[j], editcursor, mc);
                        tmp -= lacelens[j] * ratY;
                    }
                }
                #endregion 右
                //左  旋转90
                rules.Clear();
                #region 左-------------------------------------------
                tmp = downright.Y + cornerLen;
                for (var i = 0; i < laces.Count; i++)
                {
                    double markSize = markSizeFunc(i);
                    rules.Add(ObtainFootRepRule(laceRatis[i], laces[i].Flip ? laces[i].Angle - 90 : laces[i].Angle + 90, laces[i].Flip, 1, ratY, markSize * ratY, rglist[i]));
                }

                //从下到上
                for (var i = 0; i < grpY; i++)
                {
                    for (var j = 0; j < rules.Count; j++)//不能foreach
                    {
                        tmp += lacelens[j] * ratY;
                        StorePoint(new PointClass() { X = upleft.X, Y = tmp }, rules[j], editcursor, mc);

                    }
                }
                #endregion 左


                #region 转角处理

                //left-up
                StorePoint(new PointClass() { X = upleft.X, Y = upleft.Y },
                    ObtainFootRepRule(cornerRat, cornerLace.Angle, cornerLace.Flip, 1, 1, cornerSize, corner[0]),
                    editcursor,
                    mc);
                //right-up  左右翻转
                StorePoint(new PointClass() { X = -cornerLen + downright.X, Y = upleft.Y },// 位置补偿
                  ObtainFootRepRule(cornerRat, cornerLace.Angle, !cornerLace.Flip, 1, 1, cornerSize, corner[0]),
                  editcursor,
                  mc);

                //left-down 上下翻转 转180度
                StorePoint(new PointClass() { X = upleft.X, Y = downright.Y + cornerLen },
                    ObtainFootRepRule(cornerRat, cornerLace.Angle + 180, !cornerLace.Flip, 1, 1, cornerSize, corner[0]),
                    editcursor,
                    mc);
                //right-down  中心旋转180度
                StorePoint(new PointClass() { X = -cornerLen + downright.X, Y = downright.Y + cornerLen },
                  ObtainFootRepRule(cornerRat, cornerLace.Angle + 180, cornerLace.Flip, 1, 1, cornerSize, corner[0]),
                  editcursor,
                  mc);
                #endregion

                editcursor.Flush();
                Marshal.ReleaseComObject(editcursor);
            }
            catch
            {

            }
        }

        private void StorePoint(IPoint pt, IRepresentationRule rule, IFeatureCursor editcursor, IMapContext mc)
        {

            var featurebuffer = pointfcl.CreateFeatureBuffer();
            featurebuffer.Shape = pt as IGeometry;
            featurebuffer.set_Value(pointfcl.FindField("TYPE"), "花边");
            object id = editcursor.InsertFeature(featurebuffer); editcursor.Flush();
            var fe = pointfcl.GetFeature(int.Parse(id.ToString())) as IFeature;
            //自由
            //       (pointfcl as IFeatureClassLoad).LoadOnlyMode = false;
            IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
            IRepresentationClass g_RepClass = pRepWksExt.OpenRepresentationClass(pointfcl.AliasName);
            IRepresentation rep = g_RepClass.GetRepresentation(fe, mc);
            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(pt, rule);
            rep.Graphics = repGraphics;
            rep.UpdateFeature();
            rep.Feature.Store();
        }


        /// <summary>
        ///  支持镜像 flipType:[0:无,1:左右镜像,2:上下]，镜像线为几何外接矩形的水平或垂直边线
        /// </summary>
        /// <param name="rat">长宽比，暂没用（自定义缩放时用）</param>
        /// <param name="rotAngle">旋转角(逆时针)</param>
        /// <param name="flipType">翻转类型</param>
        /// <param name="dx">缩放比例x</param>
        /// <param name="dy">缩放比例y</param>
        /// <param name="size">图元尺寸</param>
        /// <param name="graphics"></param>
        /// <returns></returns>
        private IRepresentationRule ObtainFootRepRule(double rat, int rotAngle, bool flipType, double dx, double dy, double size, IRepresentationGraphics graphics)
        {

            if (rotAngle > 360)
                rotAngle %= 360;
            if (rotAngle < 0)
            {
                rotAngle %= 360;
                rotAngle += 360;
            }
            double angle = Math.PI * rotAngle / 180;

            ILine flipLine = null;
            if (flipType)
            {
                flipLine = new LineClass();
                flipLine.PutCoords(new PointClass() { X = 0, Y = 0 }, new PointClass() { X = 0, Y = 1 });
            }

            //创建rule 
            IRepresentationGraphics footg = new RepresentationMarkerClass();

            graphics.Reset();
            IEnvelope env = null;
            #region 准备
            //计算外接矩形
            while (true)
            {
                IRepresentationRule r;
                IGeometry geo;
                graphics.Next(out geo, out r);
                if (geo == null || r == null)
                    break;
                if (env == null)
                    env = geo.Envelope;
                else
                    env.Union(geo.Envelope);
            }
            //需要先平移到左上角0 0 
            if (Math.Abs(env.XMin) > 1e-3 || Math.Abs(env.YMax) > 1e-3)
            {
                graphics.Reset();
                while (true)
                {
                    IRepresentationRule r;
                    IGeometry geo;
                    graphics.Next(out geo, out r);
                    if (geo == null || r == null)
                        break;
                    var trans = geo as ITransform2D;
                    trans.Move(-env.XMin, -env.YMax);
                }
            }
            #endregion


            graphics.Reset();

            IEnvelope env2 = null;
            while (true)
            {
                IRepresentationRule r;
                IGeometry geo;
                graphics.Next(out geo, out r);
                if (geo == null || r == null)
                    break;

                var rclone = (r as IClone).Clone() as IRepresentationRule;


                var geoClone = (geo as IClone).Clone() as IGeometry;
                var orgin = new PointClass() { X = 0, Y = 0 };
                var trans = geoClone as ITransform2D;

                if (angle > 1e-1)
                {
                    trans.Rotate(orgin, angle);
                    if (Math.Abs(angle - Math.PI) < 1e-5) //中心旋转
                    {
                        trans.Move(env.Width, -env.Height);
                    }
                    else if (Math.Abs(angle - Math.PI / 2) < 1e-5)//逆90
                    {
                        trans.Move(0, -env.Width);
                    }
                    else//顺90
                    {
                        trans.Move(env.Height, 0);
                    }
                }
                if (flipType)
                {
                    var af2D = new AffineTransformation2DClass();
                    af2D.DefineReflection(flipLine);
                    trans.Transform(esriTransformDirection.esriTransformForward, af2D);
                    trans.Move(env.Width, 0);
                }


                trans.Scale(orgin, dx, dy);

                if (env2 == null)
                {
                    env2 = geoClone.Envelope;
                }
                else
                {
                    env2.Union(geoClone.Envelope);
                }
                footg.Add(geoClone, rclone);
            }

            double rat2 = Math.Max(env2.Height, env2.Width) / (footg as IRepresentationMarker).Size;

            var bs = new BasicMarkerSymbolClass();
            bs.set_Value(1, footg); //marker
            bs.set_Value(2, size / rat2); //size 
            bs.set_Value(3, 0);//angle
            bs.set_Value(4, false);

            var footrule = new RepresentationRuleClass();
            footrule.InsertLayer(0, bs);
            return footrule;
        }


        #region 辅助函数

        public void ClearFeatures(IFeatureClass fc, IQueryFilter qf_)
        {
            IFeature fe;
            try
            {
                IFeatureCursor cursor = fc.Update(qf_, false);
                while ((fe = cursor.NextFeature()) != null)
                {
                    cursor.DeleteFeature();
                }
                Marshal.ReleaseComObject(cursor);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);
            }
        }
        /// <summary>
        /// 平移
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        private IGeometry TransFormGeometry(double outUpWidth, double outDownWidth, double outLeftWidth, double outRightWidth, string AliasName)
        {
            IGeometry ptransGeo = null;
            try
            {
                IGeometry pGeoup = (pageGeo as IClone).Clone() as IGeometry;
                IGeometry pGeodown = (pageGeo as IClone).Clone() as IGeometry;
                IGeometry pGeoleft = (pageGeo as IClone).Clone() as IGeometry;
                IGeometry pGeoright = (pageGeo as IClone).Clone() as IGeometry;
                ITransform2D ptfUp = pGeoup as ITransform2D;
                ptfUp.Move(0, outUpWidth);

                ITransform2D ptfdown = pGeodown as ITransform2D;
                ptfdown.Move(0, -outDownWidth);

                IGeometry pinserV = (ptfUp as ITopologicalOperator).Intersect(ptfdown as IGeometry, esriGeometryDimension.esriGeometry2Dimension);

                ITransform2D ptfleft = pGeoleft as ITransform2D;
                ptfleft.Move(-outLeftWidth, 0);

                ITransform2D ptfright = pGeoright as ITransform2D;
                ptfright.Move(outRightWidth, 0);

                IGeometry pinserH = (ptfleft as ITopologicalOperator).Intersect(ptfright as IGeometry, esriGeometryDimension.esriGeometry2Dimension);

                ptransGeo = (pinserH as ITopologicalOperator).Intersect(pinserV, esriGeometryDimension.esriGeometry2Dimension);

                //IWorkspace ws = (clipfcl as IDataset).Workspace;
                //IFeatureWorkspace feaWS = m_Application.Workspace.EsriWorkspace as IFeatureWorkspace;
                //string workspacePath = ws.PathName;
                //string simplifyOutFeature = workspacePath + @"\" + AliasName ;
                //Geoprocessor GP_Tool = new Geoprocessor();
                //PolygonToLine GP_PolygonToLine = new PolygonToLine();
                //GP_PolygonToLine.in_features = ptransGeo as I,Polygon;
                //GP_PolygonToLine.out_feature_class = simplifyOutFeature;
                //GP_Tool.Execute(GP_PolygonToLine,null);
                //IFeatureClass Temp = feaWS.OpenFeatureClass(simplifyOutFeature);
                //IFeature IFeature = Temp.GetFeature(0);

                return ptransGeo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("图廓线生成报错：" + ex.Message);
                return null;
            }
        }
        private IGeometry TransFormGeometry(double width)
        {
            IGeometry ptransGeo = null;
            try
            {
                IGeometry pGeoup = (pageGeo as IClone).Clone() as IGeometry;
                IGeometry pGeodown = (pageGeo as IClone).Clone() as IGeometry;
                IGeometry pGeoleft = (pageGeo as IClone).Clone() as IGeometry;
                IGeometry pGeoright = (pageGeo as IClone).Clone() as IGeometry;
                ITransform2D ptfUp = pGeoup as ITransform2D;
                ptfUp.Move(0, width);

                ITransform2D ptfdown = pGeodown as ITransform2D;
                ptfdown.Move(0, -width);

                IGeometry pinserV = (ptfUp as ITopologicalOperator).Intersect(ptfdown as IGeometry, esriGeometryDimension.esriGeometry2Dimension);

                ITransform2D ptfleft = pGeoleft as ITransform2D;
                ptfleft.Move(-width, 0);

                ITransform2D ptfright = pGeoright as ITransform2D;
                ptfright.Move(width, 0);

                IGeometry pinserH = (ptfleft as ITopologicalOperator).Intersect(ptfright as IGeometry, esriGeometryDimension.esriGeometry2Dimension);

                ptransGeo = (pinserH as ITopologicalOperator).Intersect(pinserV, esriGeometryDimension.esriGeometry2Dimension);
                return ptransGeo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show("图廓线生成报错：" + ex.Message);
                return null;
            }
        }

        //创建索引网
        private void CreateIndexGrid(double dx, double dy, double annosize, double annointerval, string gdb)
        {

            string[] zm = new string[] { "", "A", "B", "C", "D", "E", "F", "J", "H", "I", "J" };
            //double width = 8.0 / 1000.0 * mapScale;
            IEnvelope pExent = clipGeoIn as IEnvelope;//内图廓线


            int indexRule = gridfcl.Fields.FindField("RULEID");
            int indexType = gridfcl.Fields.FindField("TYPE");

            int indexRule1 = pointfcl.Fields.FindField("RULEID");
            int indexType1 = pointfcl.Fields.FindField("TYPE");
            double distance = (annosize/2+annointerval) * mapScale / 1000;
            //width = distance;
            #region 添加竖线，沿X轴
            int column = int.Parse(dy.ToString());
           // column = (column > 10) ? 10 : column;
            IPolyline plineH = null;
            IGeometryCollection plineGeo = new PolylineClass();
            IPointCollection outpath = new PathClass();
            outpath.AddPoint(pExent.LowerLeft);
            outpath.AddPoint(pExent.LowerRight);

            plineGeo.AddGeometry(outpath as IGeometry);
            (plineGeo as ITopologicalOperator).Simplify();
            plineH = plineGeo as IPolyline;
            double ct = double.Parse(column.ToString());
            for (int i = 1; i < column; i++)
            {
                IPoint point = new PointClass();
                double fz = double.Parse(i.ToString());
                double ratio = fz / ct;
                plineH.QueryPoint(esriSegmentExtension.esriNoExtension, ratio, true, point);


                IPoint pointup = new PointClass() { X = point.X, Y = pExent.YMax };

                IPointCollection pc = new PolylineClass();
                pc.AddPoint(point);
                pc.AddPoint(pointup);
                (pc as ITopologicalOperator).Simplify();
                IFeature fe = gridfcl.CreateFeature();
                fe.Shape = pc as IPolyline;
                fe.set_Value(indexRule, 2);
                fe.set_Value(indexType, "垂直格网线");
                fe.Store();
            }
            // 索引格网序号

            for (int i = 1; i < column * 2; )
            {
                double fz = double.Parse(i.ToString());
                double ratio = fz / ct / 2;
                IPoint point = new PointClass();//注记点

                plineH.QueryPoint(esriSegmentExtension.esriNoExtension, ratio, true, point);
                IPoint point1 = new PointClass() { X = point.X, Y = point.Y - distance };

                IFeature fe = pointfcl.CreateFeature();
                fe.Shape = point1 as IGeometry;
                //    fe.set_Value(indexRule1, ruleIDLines[zm[(i + 1) / 2]]);
                fe.set_Value(indexType1, "格网线");
                fe.Store();
                IPoint pointup = new PointClass() { X = point1.X, Y = pExent.YMax + distance };
                IFeature fe1 = pointfcl.CreateFeature();
                fe1.Shape = pointup as IGeometry;
                //   fe.set_Value(indexRule1, ruleIDLines[zm[(i + 1) / 2]]);
                fe1.set_Value(indexType1, "格网线");
                fe1.Store();
                (pointfcl as IFeatureClassLoad).LoadOnlyMode = false;
                //获取当前要素的制图表达
                var mc = new MapContextClass();
                mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
                // IRepresentationWorkspaceExtension pRepWksExt = GetRepersentationWorkspace(m_Application.Workspace.EsriWorkspace);
                IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
                IRepresentationClass g_RepClass = pRepWksExt.OpenRepresentationClass(pointfcl.AliasName);
                IRepresentation rep = g_RepClass.GetRepresentation(fe, mc);
                //设置格网序号
                IRepresentationRule gridrule = IntialGridRepRule(gdb, zm[(i + 1) / 2]);
                var gas = (gridrule.get_Layer(0) as IBasicMarkerSymbol) as IGraphicAttributes;
                gas.set_Value(gas.get_IDByName("Size"), annosize * 2.83);

                IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
                repGraphics.Add(point1, gridrule);

                rep.Graphics = repGraphics;
                rep.UpdateFeature();
                rep.Feature.Store();

                IRepresentation rep1 = g_RepClass.GetRepresentation(fe1, mc);
               
                IRepresentationGraphics repGraphics1 = new RepresentationGraphicsClass();
                repGraphics1.Add(pointup, gridrule);
                rep1.Graphics = repGraphics1;
                rep1.UpdateFeature();
                rep1.Feature.Store();

                m_Application.ActiveView.Refresh();
                i += 2;
            }
            #endregion
             
            #region  添加横线，沿Y轴
            int row = int.Parse(dx.ToString());
           // row = (row > 10) ? 10 : row;
            IPolyline plineV = null;
            plineGeo = new PolylineClass();
            outpath = new PathClass();
            outpath.AddPoint(pExent.LowerLeft);
            outpath.AddPoint(pExent.UpperLeft);

            plineGeo.AddGeometry(outpath as IGeometry);
            (plineGeo as ITopologicalOperator).Simplify();
            plineV = plineGeo as IPolyline;
            ct = double.Parse(row.ToString());
            for (int i = 1; i < row; i++)
            {
                IPoint point = new PointClass();
                double fz = double.Parse(i.ToString());
                double ratio = fz / ct;
                plineV.QueryPoint(esriSegmentExtension.esriNoExtension, ratio, true, point);


                IPoint pointup = new PointClass() { X = pExent.XMax, Y = point.Y };

                IPointCollection pc = new PolylineClass();
                pc.AddPoint(point);
                pc.AddPoint(pointup);
                (pc as ITopologicalOperator).Simplify();
                IFeature fe = gridfcl.CreateFeature();
                fe.Shape = pc as IPolyline;
                fe.set_Value(indexRule, 2);
                fe.set_Value(indexType, "水平格网线");
                fe.Store();
            }
            for (int i = 1; i < row * 2; )
            {
                double fz = double.Parse(i.ToString());
                double ratio = fz / ct / 2;
                IPoint point = new PointClass();//注记点
                plineV.QueryPoint(esriSegmentExtension.esriNoExtension, ratio, true, point);

                IPoint point1 = new PointClass() { X = point.X - distance, Y = point.Y };
                IFeature fe = pointfcl.CreateFeature();
                fe.Shape = point1 as IGeometry;
                //  fe.set_Value(indexRule1, ruleIDLines[((i + 1) / 2).ToString()]);
                fe.set_Value(indexType1, "格网线");
                fe.Store();
                IPoint pointright = new PointClass() { X = pExent.XMax + distance, Y = point.Y };
                IFeature fe1 = pointfcl.CreateFeature();
                fe1.Shape = pointright as IGeometry;
                //  fe1.set_Value(indexRule1, ruleIDLines[((i + 1) / 2).ToString()]);
                fe1.set_Value(indexType1, "格网线");
                fe1.Store();
                (pointfcl as IFeatureClassLoad).LoadOnlyMode = false;
                //获取当前要素的制图表达
                var mc = new MapContextClass();
                mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
                // IRepresentationWorkspaceExtension pRepWksExt = GetRepersentationWorkspace(m_Application.Workspace.EsriWorkspace);
                IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
                IRepresentationClass g_RepClass = pRepWksExt.OpenRepresentationClass(pointfcl.AliasName);
                IRepresentation rep = g_RepClass.GetRepresentation(fe, mc);
                //设置格网序号尺寸
                IRepresentationRule gridrule = IntialGridRepRule(gdb, ((i + 1) / 2).ToString());
                var gas = (gridrule.get_Layer(0) as IBasicMarkerSymbol) as IGraphicAttributes;
                gas.set_Value(gas.get_IDByName("Size"), annosize * 2.83);
               

                IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
                repGraphics.Add(point1, gridrule);
                rep.Graphics = repGraphics;
                rep.UpdateFeature();
                rep.Feature.Store();

                IRepresentation rep1 = g_RepClass.GetRepresentation(fe1, mc);
                //   IRepresentationRule gridrule = IntialGridRepRule(gdb, ((i + 1) / 2).ToString());
                IRepresentationGraphics repGraphics1 = new RepresentationGraphicsClass();
                repGraphics1.Add(pointright, gridrule);
                rep1.Graphics = repGraphics1;
                rep1.UpdateFeature();
                rep1.Feature.Store();
                m_Application.ActiveView.Refresh();
                i += 2;
            }
            #endregion
        }
        //创建经纬网
        private void CreateGraticuleGrid(double dx, double dy, double annosize, double annointerval)
        {
            IColor fontColor = new CmykColorClass() { Cyan = 100, Magenta = 0, Yellow = 0, Black = 0 };
            double fontsize = annosize;
            ISpatialReference psr2000 = CreateCGCSys2000();
            ISpatialReference psr = m_Application.MapControl.Map.SpatialReference;
            IEnvelope pExent = (clipGeoIn as IClone).Clone() as IEnvelope;
            pExent.Project(psr2000);
            int indexRule = gridfcl.Fields.FindField("RULEID");
            int indexType = gridfcl.Fields.FindField("TYPE");
            double minx = pExent.XMin;
            double maxx = pExent.XMax;
            double maxy = pExent.YMax;
            double miny = pExent.YMin;
            double distance = (annointerval) * mapScale / 1000;
            double annoheight = annosize * mapScale / 1000;//文字高度
            #region 沿X轴，创建垂直线


            double widthx = dx;
            int fp = (int)(Math.Truncate(minx / widthx));
            int tp = (int)(Math.Truncate(maxx / widthx));
            for (int i = fp + 1; i < tp + 1; i++)
            {
                IPointCollection pc = new PolylineClass();
                pc.AddPoint(new PointClass { X = i * widthx, Y = maxy });
                pc.AddPoint(new PointClass { X = i * widthx, Y = miny });
                (pc as IGeometry).SpatialReference = psr2000;
                (pc as IGeometry).Project(psr);
                (pc as ITopologicalOperator).Simplify();
                
                
                //构建新的几何
                IPolyline pline = pc as IPolyline;
                double x = pline.FromPoint.X;

                //判断是否相交
                if (x < clipGeoIn.Envelope.XMin || x > clipGeoIn.Envelope.XMax)
                    continue;
                IPointCollection geometry = new PolylineClass();
                geometry.AddPoint(new PointClass { X = x, Y = clipGeoIn.Envelope.YMin });
                geometry.AddPoint(new PointClass { X = x, Y = clipGeoIn.Envelope.YMax });
                (geometry as IGeometry).SpatialReference = psr;
                (geometry as ITopologicalOperator).Simplify();
                IFeature fe = gridfcl.CreateFeature();
                fe.Shape = geometry as IPolyline;
                fe.set_Value(indexRule, 2);
                fe.set_Value(indexType, "垂直格网线");
                fe.Store();

                IPoint pPoint = new PointClass() { X = x, Y = clipGeoIn.Envelope.YMin -distance - annoheight * 0.85 };

                IFontDisp pFont = new StdFont()
                {
                    Name = "宋体",
                    Size = 2
                } as IFontDisp;
                double val = i * widthx;
                int degree = Convert.ToInt16(Math.Truncate(val));
                val = val - degree;
                int min = Convert.ToInt16(Math.Truncate(val * 60));
                int se = Convert.ToInt16(Math.Round((val * 60 - min) * 60));
                if (se == 60)
                {
                    min += 1;
                    se = 0;
                }
                if (min == 60)
                {
                    min = 0;
                    degree += 1;
                }
                //string annoname = degree.ToString() + "°" + min.ToString() + "′" + se.ToString() + "″E";
                string minstr = (min < 10) ? "0" + min.ToString() : min.ToString();
                string annoname = degree.ToString() + "°" + minstr + "′";//制图中心要求，去掉经纬网注记的秒及标识(20180718)
                InsertGridAnnoFea("格网线", pPoint, pFont, annoname, fontsize * 2.83, fontColor, esriTextVerticalAlignment.esriTVABottom);
                pPoint = new PointClass() { X = x, Y = clipGeoIn.Envelope.YMax + distance - annoheight * 0.15 };

                InsertGridAnnoFea("格网线", pPoint, pFont, annoname, fontsize * 2.83, fontColor, esriTextVerticalAlignment.esriTVABottom);
            }


            #endregion

            #region 沿Y轴，创建水平线

          
            double widthy = dy;
            fp = (int)(Math.Truncate(miny / widthy));
            tp = (int)(Math.Truncate(maxy / widthy));
            for (int i = fp + 1; i < tp + 1; i++)
            {
                IPointCollection pc = new PolylineClass();
                pc.AddPoint(new PointClass { X = minx, Y = widthy * i });
                pc.AddPoint(new PointClass { X = maxx, Y = widthy * i });
                (pc as IGeometry).SpatialReference = psr2000;
                (pc as IGeometry).Project(psr);
                (pc as ITopologicalOperator).Simplify();
                //构建新的几何
                IPolyline pline = pc as IPolyline;
                double y = pline.ToPoint.Y;

                //判断是否相交
                if (y < clipGeoIn.Envelope.YMin || y > clipGeoIn.Envelope.YMax)
                    continue;
                IPointCollection geometry = new PolylineClass();
                geometry.AddPoint(new PointClass { X = clipGeoIn.Envelope.XMin, Y = y });
                geometry.AddPoint(new PointClass { X = clipGeoIn.Envelope.XMax, Y = y });
                (geometry as IGeometry).SpatialReference = psr;
                (geometry as ITopologicalOperator).Simplify();
                IFeature fe = gridfcl.CreateFeature();
                fe.Shape = geometry as IPolyline;
                fe.set_Value(indexRule, 2);
                fe.set_Value(indexType, "水平格网线");
                fe.Store();


                IPoint pPoint = new PointClass() { X = clipGeoIn.Envelope.XMin+0.10*annoheight-distance, Y = y };
                IFontDisp pFont = new StdFont()
                {
                    Name = "宋体",
                    Size = 2
                } as IFontDisp;
                double val = i * widthy;
                int degree = Convert.ToInt16(Math.Truncate(val));
                val = val - degree;
                int min = Convert.ToInt16(Math.Truncate(val * 60));
                int se = Convert.ToInt16(Math.Round((val * 60 - min) * 60));
                if (se == 60)
                {
                    min += 1;
                    se = 0;
                }
                if (min == 60)
                {
                    min = 0;
                    degree += 1;
                }
                //string annoname = degree.ToString() + "°" + min.ToString() + "′" + se.ToString() + "″N";
                string minstr = (min < 10) ? "0" + min.ToString() : min.ToString();
                string annoname = degree.ToString() + "°" + minstr + "′";//制图中心要求，去掉经纬网注记的秒及标识(20180718)
                //左边字体
                InsertGridAnnoFeaVertical("格网线", pPoint, pFont, annoname, fontsize * 2.83, fontColor);
                //右边字体
                double fontdx = fontsize * 0.8 / 2.83 * mapScale * 1.0e-3;
                //pPoint = new PointClass() { X = clipGeoIn.Envelope.XMax + distance +0.7*annoheight, Y = y };
                pPoint = new PointClass() { X = clipGeoIn.Envelope.XMax + distance + 0.9 * annoheight, Y = y };
                InsertGridAnnoFeaVertical("格网线", pPoint, pFont, annoname, fontsize * 2.83, fontColor);
            }


            #endregion
        }
        //创建方里网
        private void CreateMeatureGrid(double dx, double dy, double annosize, double annointerval)
        {
            IColor fontColor = new CmykColorClass() { Cyan = 100, Magenta = 0, Yellow = 0, Black = 0 };
            double fontsize = annosize*2.83;
            IEnvelope pExent = clipGeoIn as IEnvelope;
            double minx = pExent.XMin;
            double maxx = pExent.XMax;
            double maxy = pExent.YMax;
            double miny = pExent.YMin;
            int indexRule = gridfcl.Fields.FindField("RULEID");
            int indexType = gridfcl.Fields.FindField("TYPE");
            //水平间距 y不变
            double distanceH = annointerval * mapScale / 1000;
            int widthx = Convert.ToInt32(dx) * 1000;
            double annoheight = annosize * mapScale / 1000;//文字高度
            for (int i = Convert.ToInt32(minx) / widthx + 1; i < Convert.ToInt32(maxx) / widthx + 1; i++)
            {
                double x = i * widthx;
                //判断是否相交
                if (x < clipGeoIn.Envelope.XMin || x > clipGeoIn.Envelope.XMax)
                    continue;
                IPointCollection pc = new PolylineClass();
                pc.AddPoint(new PointClass { X = i * widthx, Y = maxy });
                pc.AddPoint(new PointClass { X = i * widthx, Y = miny });
                (pc as ITopologicalOperator).Simplify();
                IFeature fe = gridfcl.CreateFeature();
                fe.Shape = pc as IPolyline;
                fe.set_Value(indexRule, ruleIDPolygon["格网线"]);
                fe.set_Value(indexType, "垂直格网线");
                fe.Store();
                //注记
                IPoint pPoint = new PointClass() { X = i * widthx, Y = maxy + distanceH - annoheight *0.15};
                IFontDisp pFont = new StdFont()
                {
                    Name = "宋体",
                    Size = 2
                } as IFontDisp;
                string annoname = (i * widthx).ToString();
                InsertGridAnnoFea("格网线", pPoint, pFont, annoname, fontsize, fontColor, esriTextVerticalAlignment.esriTVABottom);
                pPoint = new PointClass() { X = i * widthx, Y = miny - distanceH- annoheight *0.85};
                InsertGridAnnoFea("格网线", pPoint, pFont, annoname, fontsize, fontColor, esriTextVerticalAlignment.esriTVABottom);
            }
            //垂直间距
            double annowidth = annosize * mapScale / 1000*0.5*0.85;//文字宽度
            double distanceV = annointerval * mapScale / 1000;
            int widthy = Convert.ToInt32(dy) * 1000;
            for (int i = Convert.ToInt32(miny) / widthy + 1; i < Convert.ToInt32(maxy) / widthy + 1; i++)
            {
                double y = i * widthy;

                //判断是否相交
                if (y < clipGeoIn.Envelope.YMin || y > clipGeoIn.Envelope.YMax)
                    continue;
                IPointCollection pc = new PolylineClass();
                pc.AddPoint(new PointClass { X = minx, Y = i * widthy });
                pc.AddPoint(new PointClass { X = maxx, Y = i * widthy });
                (pc as ITopologicalOperator).Simplify();
                IFeature fe = gridfcl.CreateFeature();
                fe.Shape = pc as IPolyline;
                fe.set_Value(indexRule, ruleIDPolygon["格网线"]);
                fe.set_Value(indexType, "水平格网线");
                fe.Store();
                //注记：左边
                IPoint pPoint = new PointClass() { X = minx - distanceV - annowidth/2, Y = i * widthy };
                IFontDisp pFont = new StdFont()
                {
                    Name = "宋体",
                    Size = 3
                } as IFontDisp;
                string annoname0 = (i * widthy).ToString();
                string annoname = "";
                for (int c = 0; c < annoname0.Length; c++)
                {
                    annoname += annoname0[c].ToString() + "\r\n";
                }
                InsertGridAnnoFea("格网线", pPoint, pFont, annoname, fontsize, fontColor);
                //右边
                pPoint = new PointClass() { X = maxx + distanceV + annowidth / 2, Y = i * widthy };
                InsertGridAnnoFea("格网线", pPoint, pFont, annoname, fontsize, fontColor);
            }
        }
        //CGCS2000
        private ISpatialReference CreateCGCSys2000()
        {
            ISpatialReferenceFactory spatialReferenceFactory = new SpatialReferenceEnvironmentClass();
            ISpatialReference spatialReference = spatialReferenceFactory.CreateGeographicCoordinateSystem(4490);//CGCS2000
            ISpatialReferenceResolution spatialReferenceResolution = (ISpatialReferenceResolution)spatialReference;
            spatialReferenceResolution.ConstructFromHorizon();
            ISpatialReferenceTolerance spatialReferenceTolerance = (ISpatialReferenceTolerance)spatialReference;
            spatialReferenceTolerance.SetDefaultXYTolerance();
            return spatialReference;
        }
        //创建艺术字注记
        private bool InsertArtAnnoFea(string type, IGeometry pGeometry,string annoName, IFontDisp pFont, Dictionary<string, ITextElement> eles,bool mapNameHorizon=true)
        {
            (pFclAnno as IFeatureClassLoad).LoadOnlyMode = true;
            IFeatureClass annocls = pFclAnno;
            var list = eles.ToList();
            list.Reverse();
            IGeometry polygon = null;
            #region
            {
                ITextElement pTextElement = CreateTextElement(pGeometry, annoName, pFont, Convert.ToDouble(pFont.Size));
                ISymbolCollectionElement se = pTextElement as ISymbolCollectionElement;
                se.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                se.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                se.CharacterSpacing = MapNameSpace;
                se.CharacterWidth = MapNameWidth;
                IFeature pFeature = annocls.CreateFeature();
                IAnnotationFeature pAnnoFeature = pFeature as IAnnotationFeature;
                pAnnoFeature.Annotation = pTextElement  as IElement;
                polygon = pFeature.ShapeCopy;
            }
            #endregion
            foreach (var kv in list)
            {
                IElement pElement = (kv.Value as IClone).Clone() as IElement;
                //创建纵向注记
                if (!mapNameHorizon)
                {
                    ICharacterOrientation pCharacterOrientation = null;
                    pCharacterOrientation = (pElement as ITextElement).Symbol as ICharacterOrientation;
                    pCharacterOrientation.CJKCharactersRotation = true;
                    (pElement as ITextElement).Symbol = pCharacterOrientation as ITextSymbol;
                }
                //修改颜色
                IFormattedTextSymbol symbolText = ((pElement as ITextElement).Symbol as IClone).Clone() as IFormattedTextSymbol;
                IFillSymbol fillsym = symbolText.FillSymbol;
                ILineSymbol linesym = (fillsym.Outline as IClone).Clone() as ILineSymbol;
                switch (kv.Key)
                {
                    case "cheng2"://不处理 留白
                    case "cheng4":
                        break;
                    case "cheng3"://处理边线
                        linesym.Color = pColor;
                        fillsym.Outline = linesym;
                        break;
                    case "cheng1"://处理边线,填充
                    case "cheng5":
                    case "cheng6":
                        fillsym.Color = pColor;
                        linesym.Color = pColor;
                        fillsym.Outline = linesym;
                        break;
                    default:
                        break;
                }

                symbolText.FillSymbol = fillsym;
                //纵向注记270°
                if (!mapNameHorizon)
                {
                    symbolText.Angle = 270;
                }
                (pElement as ITextElement).Symbol = symbolText;

                ISymbolCollectionElement se = pElement as ISymbolCollectionElement;
                se.Size = (double)pFont.Size;
                se.FontName = pFont.Name;
                se.Geometry = pGeometry;
                se.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
                se.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
                se.Text = annoName;
                se.CharacterSpacing = MapNameSpace;
                se.CharacterWidth = MapNameWidth;


                IFeature pFeature = annocls.CreateFeature();
                IAnnotationFeature pAnnoFeature = pFeature as IAnnotationFeature;
                pAnnoFeature.Annotation = pElement;
                pFeature.set_Value(annocls.FindField("TYPE"), type);
                pFeature.Shape = polygon;
                pFeature.Store();

            }
            (pFclAnno as IFeatureClassLoad).LoadOnlyMode = false;

            return true;
        }

        private bool InsertAnnoFea(string type, IGeometry pGeometry, IFontDisp pFont, string annoName, double fontSize, esriTextVerticalAlignment esriv= esriTextVerticalAlignment.esriTVACenter,esriTextHorizontalAlignment esrih = esriTextHorizontalAlignment.esriTHACenter, bool mapNameHorizon=true)
        {
            (pFclAnno as IFeatureClassLoad).LoadOnlyMode = true;
            pGeometry.Project((GApplication.Application.ActiveView as IMap).SpatialReference);
            IFeatureClass annocls = pFclAnno;
            ITextElement pTextElement = CreateTextElement(pGeometry, annoName, pFont, fontSize);
           
            IElement pElement = pTextElement as IElement;

            IFormattedTextSymbol symbolText = ((pElement as ITextElement).Symbol as IClone).Clone() as IFormattedTextSymbol;
            //纵向注记270°
            if (!mapNameHorizon)
            {
                symbolText.Angle = 270;
            }
            (pElement as ITextElement).Symbol = symbolText;
            //创建纵向注记
            if (!mapNameHorizon)
            {
                ICharacterOrientation pCharacterOrientation = null;
                pCharacterOrientation = (pElement as ITextElement).Symbol as ICharacterOrientation;
                pCharacterOrientation.CJKCharactersRotation = true;
                (pElement as ITextElement).Symbol = pCharacterOrientation as ITextSymbol;
                (pElement as ITextElement).Symbol.Angle = 270;
            }


            ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pTextElement;
            pSymbolCollEle.CharacterWidth = 100;
            pSymbolCollEle.VerticalAlignment = esriv;
            pSymbolCollEle.HorizontalAlignment = esrih;
            if (type == "图名")
            {
                pSymbolCollEle.CharacterSpacing = MapNameSpace;
                pSymbolCollEle.CharacterWidth = MapNameWidth;
            }
            IFeature pFeature = annocls.CreateFeature();
            pFeature.set_Value(annocls.FindField("TYPE"), type);
            IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
            pAnnoFeature.Annotation = pElement;
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = string.Format("TYPE = '{0}'", type);
            IFeatureCursor cursor = pointfcl.Search(qf, false);
            IFeature fe;
            if ((fe = cursor.NextFeature()) != null)
            {
                pAnnoFeature.AnnotationClassID = fe.Class.ObjectClassID;
                pAnnoFeature.LinkedFeatureID = fe.OID;
            }
            pFeature.Store();
            (pFclAnno as IFeatureClassLoad).LoadOnlyMode = false;
            Marshal.ReleaseComObject(cursor);
            Marshal.ReleaseComObject(pFeature);
            Marshal.ReleaseComObject(qf);
            GC.Collect();
            return true;
        }
        //创建比例尺注记
        private bool InsertScaleAnnoFea(IFeature fe,string type, IGeometry pGeometry, IFontDisp pFont, string annoName, double fontSize, double width = 100)
        {
            (pFclAnno as IFeatureClassLoad).LoadOnlyMode = true;
            try
            {
                pGeometry.Project((GApplication.Application.ActiveView as IMap).SpatialReference);
                IFeatureClass annocls = pFclAnno;
                ITextElement pTextElement = CreateTextElement(pGeometry, annoName, pFont, fontSize);
                IElement pElement = pTextElement as IElement;

                ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pTextElement;
                pSymbolCollEle.CharacterWidth = width;

                IFeature pFeature = annocls.CreateFeature();
                pFeature.set_Value(annocls.FindField("TYPE"), type);
                IAnnotationFeature2 pAnnoFeature = pFeature as IAnnotationFeature2;
                pAnnoFeature.Annotation = pElement;
                pAnnoFeature.AnnotationClassID = fe.Class.ObjectClassID;
                pAnnoFeature.LinkedFeatureID = fe.OID;
                pFeature.Store();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                (pFclAnno as IFeatureClassLoad).LoadOnlyMode = false;
            }

            return true;
        }
        //创建注记 竖
        private bool InsertAnnoFeaVertical(string type, IGeometry pGeometry, IFontDisp pFont, string annoName, double fontSize, double width = 100)
        {
            IFeatureClass annocls = pFclAnno;
            ITextElement pTextElement = CreateTextElementVertival(pGeometry, annoName, pFont, fontSize, 90);
            IElement pElement = pTextElement as IElement;

            ISymbolCollectionElement pSymbolCollEle = (ISymbolCollectionElement)pTextElement;
            pSymbolCollEle.CharacterWidth = width;

            IFeature pFeature = annocls.CreateFeature();
            pFeature.set_Value(annocls.FindField("TYPE"), type);
            IAnnotationFeature pAnnoFeature = pFeature as IAnnotationFeature;
            pAnnoFeature.Annotation = pElement;
            pFeature.Store();


            return true;
        }
        /// 创建字体符号   
        IRgbColor pColor = new RgbColorClass() { Red = 0, Blue = 0, Green = 0 };//默认颜色
        private ITextElement CreateTextElement(IGeometry pGeoTxt, string txt, IFontDisp pFont, double size)
        {
            ITextSymbol pTextSymbol = new TextSymbolClass()
            {
                Color = pColor,
                Font = pFont,
                Size = size

            };
            pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVACenter;
            pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
            ITextElement pTextElment = null;
            IElement pEle = null;
            pTextElment = new TextElementClass()
            {
                Symbol = pTextSymbol,
                ScaleText = true,
                Text = txt
            };
            pEle = pTextElment as IElement;

            pEle.Geometry = pGeoTxt;
            return pTextElment;
        }
        private ITextElement CreateTextElementVertival(IGeometry pGeoTxt, string txt, IFontDisp pFont, double size, double angle)
        {
            ITextSymbol pTextSymbol = new TextSymbolClass()
            {
                Color = pColor,
                Font = pFont,
                Size = size,
                Angle = angle

            };
            pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVABottom;
            ITextElement pTextElment = null;
            IElement pEle = null;
            pTextElment = new TextElementClass()
            {
                Symbol = pTextSymbol,
                ScaleText = true,
                Text = txt
            };
            pEle = pTextElment as IElement;

            pEle.Geometry = pGeoTxt;
            return pTextElment;
        }
        //根据模板数据库获取制图对应专题的表达要素类
        private void GetLengendRepRule(string mapTheme, string gdbname, string basethemename)
        {

            IFeatureClass fc = (repWscp as IFeatureWorkspace).OpenFeatureClass(gdbname);
            IWorkspace ws = repWscp;
            //根据数据库获取制图表达要素类
            IRepresentationWorkspaceExtension repWS = GetRepersentationWorkspace(ws);
            if (repWS.get_FeatureClassHasRepresentations(fc))
            {
                IEnumDatasetName enumDatasetName = repWS.get_FeatureClassRepresentationNames(fc);
                enumDatasetName.Reset();
                IDatasetName pDatasetName = enumDatasetName.Next();
                _mRepClass = repWS.OpenRepresentationClass(pDatasetName.Name);
                IRepresentationRules rules = _mRepClass.RepresentationRules;
                rules.Reset();
                IRepresentationRule rule = null;
                int ruleID;
                while (true)
                {
                    rules.Next(out ruleID, out rule);
                    if (rule == null) break;
                    if (rules.get_Name(ruleID) == basethemename)
                    {
                        lengendRule = rule;

                    }
                    if (rules.get_Name(ruleID) == mapTheme)
                    {
                        themeRule = rule;
                    }
                }
            }

        }

        private IRepresentationWorkspaceExtension GetRepersentationWorkspace(IWorkspace workspace)
        {
            IWorkspaceExtensionManager wem = workspace as IWorkspaceExtensionManager;
            UID uid = new UIDClass();
            uid.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            IRepresentationWorkspaceExtension rwe = wem.FindExtension(uid) as IRepresentationWorkspaceExtension;
            return rwe;
        }

        private void CreateLengend(IPoint point, IRepresentationRule rule)
        {

            (pointfcl as IFeatureClassLoad).LoadOnlyMode = true;
            IFeature fe = pointfcl.CreateFeature();
            fe.Shape = point;
            fe.set_Value(pointfcl.FindField("TYPE"), "图例");
            fe.Store();
            (pointfcl as IFeatureClassLoad).LoadOnlyMode = false;
            //获取当前要素的制图表达
            var mc = new MapContextClass();
            mc.InitFromDisplay(m_Application.ActiveView.ScreenDisplay.DisplayTransformation);
            IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
            IRepresentationClass g_RepClass = pRepWksExt.OpenRepresentationClass(pointfcl.AliasName);
            IRepresentation rep = g_RepClass.GetRepresentation(fe, mc);

            IRepresentationGraphics repGraphics = new RepresentationGraphicsClass();
            repGraphics.Add(point, rule);

            rep.Graphics = repGraphics;
            rep.UpdateFeature();
            rep.Feature.Store();
        }
        private void CreateLengendLine()
        {

            int ruleID = ruleIDPolygon["纸张页面"];
            IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;
            IPoint orginPoint = clipGeoIn.LowerRight;
            IPointCollection pRing = new RingClass();

            pRing.AddPoint(orginPoint);
            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y + lengendHeight * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y + lengendHeight * mapScale / 1000 });
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();

            IFeature fe = polygonfcl.CreateFeature();
            fe.Shape = geoCol as IGeometry;
            fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
            fe.set_Value(polygonfcl.FindField("TYPE"), "图例外边线");
            fe.Store();
            //内边线
            geoCol = new PolygonClass();
            _missing = Type.Missing;

            pRing = new RingClass();

            pRing.AddPoint(orginPoint);
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth - 2) * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth - 2) * mapScale / 1000, Y = orginPoint.Y + (lengendHeight - 2) * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y + (lengendHeight - 2) * mapScale / 1000 });
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            fe = polygonfcl.CreateFeature();
            fe.Shape = geoCol as IGeometry;
            fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
            fe.set_Value(polygonfcl.FindField("TYPE"), "图例内边线");
            fe.Store();
        }
        private void CreateLengendLine(IPoint orginPoint, string location = "左下")
        {

            int ruleID = ruleIDPolygon["图廓"];
            IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;

            IPointCollection pRing = new RingClass();

            pRing.AddPoint(orginPoint);
            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + lengendWidth * mapScale / 1000, Y = orginPoint.Y + lengendHeight * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y + lengendHeight * mapScale / 1000 });
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();

            IFeature fe = polygonfcl.CreateFeature();
            fe.Shape = geoCol as IGeometry;
            fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
            fe.set_Value(polygonfcl.FindField("TYPE"), "图例外边线");
            fe.Store();
            //内边线
            geoCol = new PolygonClass();
            _missing = Type.Missing;

            pRing = new RingClass();

            pRing.AddPoint(orginPoint);
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth - 2) * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X + (lengendWidth - 2) * mapScale / 1000, Y = orginPoint.Y + (lengendHeight - 2) * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y + (lengendHeight - 2) * mapScale / 1000 });
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            ITransform2D pTrans = geoCol as ITransform2D;
            switch (location)
            {
                case "左上":
                    pTrans.Move(0, 2.0e-3 * mapScale);
                    break;
                case "右上":
                    pTrans.Move(2.0e-3 * mapScale, 2.0e-3 * mapScale);
                    break;
                case "右下":
                    pTrans.Move(2.0e-3 * mapScale, 0);
                    break;
                default://"左下"
                    break;

            }
            fe = polygonfcl.CreateFeature();
            fe.Shape = geoCol as IGeometry;
            fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
            fe.set_Value(polygonfcl.FindField("TYPE"), "图例内边线");
            fe.Store();
        }
        private void CreateLengendLineRight()
        {

            int ruleID = ruleIDPolygon["纸张页面"];
            IGeometryCollection geoCol = new PolygonClass();
            object _missing = Type.Missing;

            IPointCollection pRing = new RingClass();
            IPoint orginPoint = clipGeoIn.LowerRight;
            pRing.AddPoint(orginPoint);
            pRing.AddPoint(new PointClass() { X = orginPoint.X - lengendWidth * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X - lengendWidth * mapScale / 1000, Y = orginPoint.Y + lengendHeight * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y + lengendHeight * mapScale / 1000 });
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            ITopologicalOperator topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();

            IFeature fe = polygonfcl.CreateFeature();
            fe.Shape = geoCol as IGeometry;
            fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
            fe.set_Value(polygonfcl.FindField("TYPE"), "图例外边线");
            fe.Store();
            //内边线
            geoCol = new PolygonClass();
            _missing = Type.Missing;

            pRing = new RingClass();

            pRing.AddPoint(orginPoint);
            pRing.AddPoint(new PointClass() { X = orginPoint.X - (lengendWidth - 2) * mapScale / 1000, Y = orginPoint.Y });
            pRing.AddPoint(new PointClass() { X = orginPoint.X - (lengendWidth - 2) * mapScale / 1000, Y = orginPoint.Y + (lengendHeight - 2) * mapScale / 1000 });
            pRing.AddPoint(new PointClass() { X = orginPoint.X, Y = orginPoint.Y + (lengendHeight - 2) * mapScale / 1000 });
            (pRing as IRing).Close();
            geoCol.AddGeometry(pRing as IGeometry, ref _missing, ref _missing);
            topologicalOperator = geoCol as ITopologicalOperator;
            topologicalOperator.Simplify();
            fe = polygonfcl.CreateFeature();
            fe.Shape = geoCol as IGeometry;
            fe.set_Value(polygonfcl.FindField("RuleID"), ruleID);
            fe.set_Value(polygonfcl.FindField("TYPE"), "图例内边线");
            fe.Store();
        }
        private void AddLendgendAnnotion(IPoint p, string annoName)
        {

            IFontDisp pFont = new StdFont()
            {
                Name = "宋体",
                Size = 23
            } as IFontDisp;

            InsertAnnoFea("图例", p, pFont, annoName, 30);

        }

        private void IntialCompassRepRule(string gdbname)
        {
            IFeatureClass fc = (repWscp as IFeatureWorkspace).OpenFeatureClass(gdbname);
            IWorkspace ws = repWscp;
            //根据数据库获取制图表达要素类
            IRepresentationWorkspaceExtension repWS = GetRepersentationWorkspace(ws);
            if (repWS.get_FeatureClassHasRepresentations(fc))
            {
                IEnumDatasetName enumDatasetName = repWS.get_FeatureClassRepresentationNames(fc);
                enumDatasetName.Reset();
                IDatasetName pDatasetName = enumDatasetName.Next();
                _mRepClass = repWS.OpenRepresentationClass(pDatasetName.Name);
                IRepresentationRules rules = _mRepClass.RepresentationRules;
                rules.Reset();
                IRepresentationRule rule = null;
                int ruleID;
                while (true)
                {
                    rules.Next(out ruleID, out rule);
                    if (rule == null) break;
                    if (rules.get_Name(ruleID) == compassName)
                    {
                        compassRule = rule;
                        break;
                    }

                }

            }
        }
        private IRepresentationRule IntialGridRepRule(string gdbname, string grid)
        {
            IFeatureClass fc = (repWscp as IFeatureWorkspace).OpenFeatureClass(gdbname);
            IWorkspace ws = repWscp;
            //根据数据库获取制图表达要素类
            IRepresentationRule resultrule = null;
            IRepresentationWorkspaceExtension repWS = GetRepersentationWorkspace(ws);
            if (repWS.get_FeatureClassHasRepresentations(fc))
            {
                IEnumDatasetName enumDatasetName = repWS.get_FeatureClassRepresentationNames(fc);
                enumDatasetName.Reset();
                IDatasetName pDatasetName = enumDatasetName.Next();
                _mRepClass = repWS.OpenRepresentationClass(pDatasetName.Name);
                IRepresentationRules rules = _mRepClass.RepresentationRules;
                rules.Reset();
                IRepresentationRule rule = null;
                int ruleID;
                while (true)
                {
                    rules.Next(out ruleID, out rule);
                    if (rule == null) break;
                    if (rules.get_Name(ruleID) == grid)
                    {
                        resultrule = rule;
                        break;
                    }

                }

            }
            return (resultrule as IClone).Clone() as IRepresentationRule;
        }

        private void IntialScaleBarRepRule(string scaleName, string gdbname)
        {

            IFeatureClass fc = (repWscp as IFeatureWorkspace).OpenFeatureClass(gdbname);
            IWorkspace ws = repWscp;
            //根据数据库获取制图表达要素类
            IRepresentationWorkspaceExtension repWS = GetRepersentationWorkspace(ws);
            if (repWS.get_FeatureClassHasRepresentations(fc))
            {
                IEnumDatasetName enumDatasetName = repWS.get_FeatureClassRepresentationNames(fc);
                enumDatasetName.Reset();
                IDatasetName pDatasetName = enumDatasetName.Next();
                _mRepClass = repWS.OpenRepresentationClass(pDatasetName.Name);
                IRepresentationRules rules = _mRepClass.RepresentationRules;
                rules.Reset();
                IRepresentationRule rule = null;
                int ruleID;
                while (true)
                {
                    rules.Next(out ruleID, out rule);
                    if (rule == null) break;
                    if (rules.get_Name(ruleID) == scaleName)
                    {
                        scaleRule = rule;
                        break;
                    }

                }

            }
        }
        //生成比例尺注记
        private void AddScaleAnnotion(IFeature fe, bool bDigitScale = false)
        {
            double x, y;
            x = orginPoint.X;
            y = orginPoint.Y + 2.5e-3 * mapScale;
            IFontDisp pFont = new StdFont()
            {
                Name = "宋体",
                Size = 2
            } as IFontDisp;
            string annoname = "";
            IPoint p = null;
            //1:1000
            int strLength = mapScale.ToString().Length;
            annoname = "1:" + mapScale.ToString().Insert(strLength-3, " ");
            if(m_Application.Template.Root.Contains("江苏"))
                annoname = "1∶" + mapScale.ToString().Insert(strLength - 3, " ");
            p = new PointClass() { X = x + 25 * mapScale / 1000, Y = y + 5 * mapScale / 1000 };
            InsertScaleAnnoFea(fe,"比例尺", p, pFont, annoname, 12);

            if (!bDigitScale)
            {
                //1000
                annoname = (10 * mapScale / 1000).ToString() + " m";
                p = new PointClass() { X = x, Y = y };

                InsertScaleAnnoFea(fe, "比例尺", p, pFont, annoname, 7);
                //0
                x += 10 * mapScale / 1000;
                annoname = "0";
                p = new PointClass() { X = x, Y = y };
                InsertScaleAnnoFea(fe, "比例尺", p, pFont, annoname, 7);
                //1
                double coe = ScaleUnit == "km" ? 100000 : 100;
                x += 10 * mapScale / 1000;
                annoname = (mapScale / coe).ToString();
                p = new PointClass() { X = x, Y = y };
                InsertScaleAnnoFea(fe, "比例尺", p, pFont, annoname, 7);
                //2
                x += 10 * mapScale / 1000;
                annoname = (2 * mapScale / coe).ToString();
                p = new PointClass() { X = x, Y = y };
                InsertScaleAnnoFea(fe, "比例尺", p, pFont, annoname, 7);
                //3
                x += 10 * mapScale / 1000;
                annoname = (3 * mapScale / coe).ToString();
                p = new PointClass() { X = x, Y = y };
                InsertScaleAnnoFea(fe, "比例尺", p, pFont, annoname, 7);
                //4km
                x += 10 * mapScale / 1000;
                annoname = (4 * mapScale / coe).ToString();
                annoname = annoname + " " + ScaleUnit;
                p = new PointClass() { X = x + 2 * mapScale / 1000, Y = y };
                InsertScaleAnnoFea(fe, "比例尺", p, pFont, annoname, 7);
            }
            //说明：本图界境不作实地化界依据
        }

        // 中文字符长度     
        private double GetStrLen(string str)
        {
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if ((int)str[i] > 127)
                    count++;
            }
            double ct = str.Length - count + count * 2;
            return ct / 2;
        }
        #endregion

        private bool InsertGridAnnoFea(string type, IGeometry pGeometry, IFontDisp pFont, string annoName, double fontSize, IColor fontClr = null, esriTextVerticalAlignment esriv = esriTextVerticalAlignment.esriTVACenter)
        {
            pGeometry.Project((GApplication.Application.ActiveView as IMap).SpatialReference);

            ITextSymbol pTextSymbol = new TextSymbolClass();
            pTextSymbol.Font = pFont;
            pTextSymbol.Size = fontSize;
            if (fontClr == null)
            {
                pTextSymbol.Color = new RgbColorClass() { Red = 0, Blue = 0, Green = 0 };//默认颜色
            }
            else
            {
                pTextSymbol.Color = fontClr;
            }
            pTextSymbol.VerticalAlignment = esriv;
            pTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHACenter;
            ITextElement pTextElement = new TextElementClass()
            {
                Symbol = pTextSymbol,
                ScaleText = true,
                Geometry = pGeometry,
                Text = annoName
            };

            IFeatureClass annocls = pFclAnno;
            IFeature newFe = annocls.CreateFeature();
            newFe.set_Value(annocls.FindField("TYPE"), type);
            (newFe as IAnnotationFeature2).Annotation = pTextElement as IElement;
            newFe.Store();

            return true;
        }
        private bool InsertGridAnnoFeaVertical(string type, IGeometry pGeometry, IFontDisp pFont, string annoName, double fontSize, IColor fontClr = null, double width = 100)
        {
            ITextSymbol pTextSymbol = new TextSymbolClass();
            pTextSymbol.Font = pFont;
            pTextSymbol.Size = fontSize;
            if (fontClr == null)
            {
                pTextSymbol.Color = new RgbColorClass() { Red = 0, Blue = 0, Green = 0 };//默认颜色
            }
            else
            {
                pTextSymbol.Color = fontClr;
            }
            pTextSymbol.Angle = 90;
            pTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVABottom;
            ITextElement pTextElement = new TextElementClass()
            {
                Symbol = pTextSymbol,
                ScaleText = true,
                Geometry = pGeometry,
                Text = annoName
            };
            (pTextElement as ISymbolCollectionElement).CharacterWidth = width;

            IFeatureClass annocls = pFclAnno;
            IFeature newFe = annocls.CreateFeature();
            newFe.set_Value(annocls.FindField("TYPE"), type);
            (newFe as IAnnotationFeature2).Annotation = pTextElement as IElement;
            newFe.Store();


            return true;
        }
    }
}
