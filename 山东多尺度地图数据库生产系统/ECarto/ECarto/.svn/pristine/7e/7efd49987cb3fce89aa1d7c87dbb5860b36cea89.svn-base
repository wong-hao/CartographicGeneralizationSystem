using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Data;

namespace SMGI.Plugin.EmergencyMap
{
    [SMGIAutomaticCommand]
    public class ZoneColorCmd : SMGI.Common.SMGICommand
    {

        public ZoneColorCmd()
        {

            m_caption = "行政区普色";
            m_category = "行政区普色";
            m_toolTip = "行政区普色";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null && m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing;
            }
        }
        Dictionary<int, IColor> ColorDic = new Dictionary<int, IColor>();//ruleID->Icolor
        public override void OnClick()
        {
            ZoneColorForm frm = new ZoneColorForm();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            ColorDic.Clear();
            string sqlText = "ATTACH is NULL";
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = sqlText;
            if (frm.Layer == null)
            {
                MessageBox.Show("普色要素为空！");
                return;
            }
            int featureCount = (frm.Layer as IFeatureLayer).FeatureClass.FeatureCount(qf);
            if (featureCount == 0)
            {
                MessageBox.Show("普色要素为空！");
                return;
            }
            using (WaitOperation wo = m_Application.SetBusy())
            {
                wo.SetText("正在着色...");
                ColorToBOUA(frm.CMYKColors,frm.Layer,frm.ProvPAC,frm.AttachColors);
            }

        }

        private void ColorToBOUA(List<ICmykColor> frmColors, ILayer selectedLayer, string provPAC, Dictionary<string, IRgbColor> attachColors)
        {
            try
            {
                Dictionary<int, int> ColorRules = null;//颜色序号->ruleID；

                IGeoFeatureLayer geoFlyr = selectedLayer as IGeoFeatureLayer;
                IRepresentationRenderer repRender = geoFlyr.Renderer as IRepresentationRenderer;
                if (repRender == null)
                {
                    repRender = SwitchRender(geoFlyr);
                }
                ColorRules = RuleColorSets(selectedLayer.Name, frmColors);
                int colorNum = frmColors.Count;
                {

                    string mdbpath = EnvironmentSettings.getLayerRuleDBFileName(m_Application);

                    DataTable ruleDt = CommonMethods.ReadToDataTable(mdbpath, "图层对照规则");//通用RuleID
                    DataRow[] drs1 = ruleDt.Select("图层='BOUA'");
                    Dictionary<string, int> ruleIDPolygon = new Dictionary<string, int>();
                    foreach (DataRow dr in drs1)//COMPASS
                    {
                        string keyname = dr["RuleName"].ToString();
                        int val = int.Parse(dr["RuleID"].ToString());
                        ruleIDPolygon[keyname] = val;//!!
                    }
                    string sqlText = "ATTACH is NULL";
                    IQueryFilter qf = new QueryFilterClass();
                    qf.WhereClause = sqlText;
                    int featureCount = (selectedLayer as IFeatureLayer).FeatureClass.FeatureCount(qf);
                    if (featureCount == 0)
                    {
                        return;
                    }
                    //获取邻接性
                    Dictionary<int, int> Fedics = null;//序号->OjbectID
                    Dictionary<int, double> areas = null;//序号->面积

                    int[,] connMatrix = ConnectivityMatrixEx(selectedLayer, qf, out Fedics, out areas);
                    int[] color = new int[featureCount];
                    var sort = areas.OrderBy(t => t.Value);
                    color[sort.First().Key] = ColorRules.Last().Key;//面积最小;
                    color[sort.Last().Key] = ColorRules.First().Key;//面积最大
                    {
                        color = new int[featureCount];
                        MapColor(ref color, colorNum, connMatrix, featureCount);
                    }

                    AssignColorFieldEx(color, selectedLayer, qf, "color", ColorRules);

                    selectedLayer.Visible = true;  //当前着色图层可见

                    m_Application.ActiveView.Refresh();

                    //处理附区
                    #region 省内

                    qf.WhereClause = "ATTACH = '1'";
                    IFeatureLayer featureLayer = selectedLayer as IFeatureLayer;
                    var featurecursor = featureLayer.FeatureClass.Search(qf, false);
                    IFeature fe;


                    IMapContext mctx = new MapContextClass();
                    mctx.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, geoFlyr.AreaOfInterest);
                    var attachColor = attachColors["省内"];
                    if (repRender != null)
                    {

                        IRepresentationClass rpc = repRender.RepresentationClass;
                        while ((fe = featurecursor.NextFeature()) != null)
                        {
                            var rep = rpc.GetRepresentation(fe, mctx);
                            string pac = fe.get_Value(fe.Fields.FindField("PAC")).ToString();
                            if (pac == "0")
                            {
                                attachColor = attachColors["国外"];
                            }
                            else if (pac.StartsWith(provPAC))
                            {

                                attachColor = attachColors["省内"];
                            }
                            else
                            {
                                attachColor = attachColors["省外"];
                            }
                            OverrideColorValueSet(rep, attachColor);
                        }
                    }
                    #endregion
                    #region 其他区域[省外，国外]
                    qf.WhereClause = "";
                    var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                    {
                        return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == "BOUA2");
                    })).FirstOrDefault() as IFeatureLayer;

                    if (lyr == null)
                        return;
                    featureLayer = lyr as IFeatureLayer;
                    featurecursor = featureLayer.FeatureClass.Search(qf, false);
                    geoFlyr = featureLayer as IGeoFeatureLayer;
                    mctx.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, geoFlyr.AreaOfInterest);
                    repRender = geoFlyr.Renderer as IRepresentationRenderer;
                    if (repRender == null)
                    {
                        repRender = SwitchRender(geoFlyr);
                    }
                    if (repRender != null)
                    {
                        IRepresentationClass rpc = repRender.RepresentationClass;
                        while ((fe = featurecursor.NextFeature()) != null)
                        {
                            var rep = rpc.GetRepresentation(fe, mctx);
                            string pac = fe.get_Value(fe.Fields.FindField("PAC")).ToString();
                            if (pac.StartsWith("0"))
                            {
                                attachColor = attachColors["国外"];
                            }
                            else if (pac.StartsWith(provPAC))
                            {
                                attachColor = attachColors["省内"];
                                continue;

                            }
                            else
                            {
                                attachColor = attachColors["省外"];
                            }
                            OverrideColorValueSet(rep, attachColor);
                        }
                        lyr.Visible = true;
                    }
                    #endregion

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
            }
        }
        private IRepresentationRenderer SwitchRender(IGeoFeatureLayer geoFlyr)
        {
            IFeatureClass fc = geoFlyr.FeatureClass;
            IRepresentationRenderer repRenderer = new RepresentationRendererClass();
            IRepresentationWorkspaceExtension pRepWksExt = m_Application.Workspace.RepersentationWorkspaceExtension;
            bool hasRep = pRepWksExt.get_FeatureClassHasRepresentations(fc);
            if (hasRep)
            {
                IEnumDatasetName enumDatasetName = pRepWksExt.get_FeatureClassRepresentationNames(fc);
                enumDatasetName.Reset();
                IDatasetName pDatasetName = enumDatasetName.Next();
                if (pDatasetName != null)
                {
                    repRenderer.RepresentationClass = pRepWksExt.OpenRepresentationClass(pDatasetName.Name);
                }
                geoFlyr.Renderer = repRenderer as IFeatureRenderer;
                return repRenderer;
            }
            else
            {


                MessageBox.Show("请先设置图层为制图表达!");
                return null;
            }
        }
        protected override bool DoCommand(System.Xml.Linq.XElement args, Action<string> messageRaisedAction)
        {
            try
            {
                //解析参数
                var colors = args.Element("BOUAColors");//主区颜色
                List<ICmykColor> frmColors = new List<ICmykColor>();
                foreach (var ele in colors.Elements("Color"))
                {
                    string cmyk = ele.Value;
                    ICmykColor icolor = new CmykColorClass { CMYK = int.Parse(cmyk) };
                    frmColors.Add(icolor);

                    messageRaisedAction(string.Format("主区颜色方案:C{0}M{1}Y{2}K{3}",icolor.Cyan,icolor.Magenta, icolor.Yellow, icolor.Black));
                }

                var fcName = args.Element("BOUAFCName").Value;
                messageRaisedAction(string.Format("普色要素类名:{0}", fcName));

                //附区颜色
                Dictionary<string, IRgbColor> attachColor = new Dictionary<string, IRgbColor>();
                foreach (var ele in args.Element("AttachColors").Elements("Color"))
                {
                    string key = ele.Attribute("Name").Value;
                    string rgb = ele.Value;
                    IRgbColor c = new RgbColorClass { RGB = int.Parse(rgb) };
                    attachColor[key] = c;

                    messageRaisedAction(string.Format("邻区颜色方案:R{0}G{1}B{2}", c.Red, c.Green, c.Blue));

                }

                string pacProv = args.Element("ProPAC").Value;
                messageRaisedAction(string.Format("省级PAC:{0}", pacProv));


                ILayer selectedLayer = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
                {
                    return (l is IFeatureLayer) && ((l as IFeatureLayer).FeatureClass.AliasName.ToUpper() == fcName.ToUpper());
                })).FirstOrDefault();

                ColorToBOUA(frmColors, selectedLayer, pacProv, attachColor);
                m_Application.Workspace.Save();

                messageRaisedAction("保存工程");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                return false;
            }
            return true;
        }
        private IColor GetColorByString(string cmyk)
        {
            char[] D = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            StringBuilder sb = new StringBuilder();
            //新建一个CMYK颜色，然后各项值付为0
            ICmykColor CMYK_Color = new CmykColorClass();
            CMYK_Color.Cyan = 0;
            CMYK_Color.Magenta = 0;
            CMYK_Color.Yellow = 0;
            CMYK_Color.Black = 0;
            try
            {
                for (int i = 0; i <= cmyk.Length; i++)
                {
                    if (i == cmyk.Length)
                    {
                        string sbs = sb.ToString();
                        if (sbs.Contains('C'))
                        {
                            CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('M'))
                        {
                            CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('Y'))
                        {
                            CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                        }
                        if (sbs.Contains('K'))
                        {
                            CMYK_Color.Black = int.Parse(sbs.Substring(1));
                        }
                        break;
                    }
                    else
                    {
                        char C = cmyk[i];
                        if (D.Contains(C))
                        {
                            sb.Append(C);
                        }
                        else
                        {
                            string sbs = sb.ToString();
                            if (sbs.Contains('C'))
                            {
                                CMYK_Color.Cyan = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('M'))
                            {
                                CMYK_Color.Magenta = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('Y'))
                            {
                                CMYK_Color.Yellow = int.Parse(sbs.Substring(1));
                            }
                            if (sbs.Contains('K'))
                            {
                                CMYK_Color.Black = int.Parse(sbs.Substring(1));
                            }
                            sb.Clear();
                            sb.Append(C);
                        }
                    }
                }
                return CMYK_Color;
            }
            catch
            {
                return null;
            }

        }

        public void OverrideColorValueSet(IRepresentation rep, IColor pColor)
        {

            var ruleOrg = rep.RepresentationClass.RepresentationRules.get_Rule(rep.RuleID);
            IBasicFillSymbol fillSym = ruleOrg.get_Layer(0) as IBasicFillSymbol;
            IGraphicAttributes ga = fillSym.FillPattern as IGraphicAttributes;
            if (fillSym != null)
            {
                if (ga.ClassName == "SolidColorPattern")
                {
                    int id = ga.get_IDByName("Color");
                    rep.set_Value(ga, id, pColor);
                }
                if (ga.ClassName == "GradientPattern")
                {
                    int id1 = ga.get_IDByName("Color1");
                    rep.set_Value(ga, id1, pColor);
                    int id2 = ga.get_IDByName("Color2");
                    rep.set_Value(ga, id2, pColor);
                }

            }
            rep.RepresentationClass.RepresentationRules.set_Rule(rep.RuleID, ruleOrg);
            rep.UpdateFeature();
            rep.Feature.Store();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(rep);
        }

        //建立邻接矩阵

        private int[,] ConnectivityMatrixEx(ILayer layer, IQueryFilter qf, out Dictionary<int, int> Fedics, out Dictionary<int, double> areas)
        {
            Fedics = new Dictionary<int, int>();
            areas = new Dictionary<int, double>();
            if (layer == null)
            {
                return null;
            }
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            if (featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                return null;
            }

            int num = featureLayer.FeatureClass.FeatureCount(qf);
            int[,] connMatrix = new int[num, num];
            IFeatureCursor featureCursor1 = featureLayer.Search(qf, false);
            IFeature feature1 = null;
            int i = 0;

            while ((feature1 = featureCursor1.NextFeature()) != null)
            {
                if (i >= num)
                {
                    break;
                }
                IPolygon polygon1 = feature1.Shape as IPolygon;
                IRelationalOperator reltOperator = polygon1 as IRelationalOperator;
                IFeatureCursor featureCursor2 = featureLayer.Search(qf, false);
                IFeature feature2 = null;
                int j = 0;
                while ((feature2 = featureCursor2.NextFeature()) != null)
                {
                    #region
                    if (j >= num)
                    {
                        break;
                    }
                    if (j == i)
                    {
                        connMatrix[i, j] = 0;
                        j++;
                        continue;
                    }
                    bool isAdjacent = reltOperator.Touches(feature2.Shape);
                    if (isAdjacent)
                    {
                        connMatrix[i, j] = 1;
                    }
                    else
                    {
                        connMatrix[i, j] = 0;
                    }
                    j++;
                    #endregion
                }
                Marshal.ReleaseComObject(featureCursor2);

                Fedics[i] = feature1.OID;
                areas[i] = (feature1.ShapeCopy as IArea).Area;
                i++;
            }
            Marshal.ReleaseComObject(featureCursor1);
            return connMatrix;
        }
        private int[,] ConnectivityMatrix(ILayer layer, IQueryFilter qf)
        {
            if (layer == null)
            {
                return null;
            }
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            if (featureLayer.FeatureClass.ShapeType != esriGeometryType.esriGeometryPolygon)
            {
                return null;
            }

            int num = featureLayer.FeatureClass.FeatureCount(qf);
            int[,] connMatrix = new int[num, num];
            IFeatureCursor featureCursor1 = featureLayer.Search(qf, false);
            IFeature feature1 = null;
            int i = 0;

            while ((feature1 = featureCursor1.NextFeature()) != null)
            {
                if (i >= num)
                {
                    break;
                }
                IPolygon polygon1 = feature1.Shape as IPolygon;
                IRelationalOperator reltOperator = polygon1 as IRelationalOperator;
                IFeatureCursor featureCursor2 = featureLayer.Search(qf, false);
                IFeature feature2 = null;
                int j = 0;
                while ((feature2 = featureCursor2.NextFeature()) != null)
                {
                    #region
                    if (j >= num)
                    {
                        break;
                    }
                    if (j == i)
                    {
                        connMatrix[i, j] = 0;
                        j++;
                        continue;
                    }
                    bool isAdjacent = reltOperator.Touches(feature2.Shape);
                    if (isAdjacent)
                    {
                        connMatrix[i, j] = 1;
                    }
                    else
                    {
                        connMatrix[i, j] = 0;
                    }
                    j++;
                    #endregion
                }
                Marshal.ReleaseComObject(featureCursor2);
                i++;
            }
            Marshal.ReleaseComObject(featureCursor1);
            return connMatrix;
        }

        //根据邻接矩阵生成可行的着色数组

        private void MapColorEx(int max, int min, ref int[] color, int colorNum, int[,] connMatrix, int count)
        {


            if (max != 0 && min != 0)
                color[0] = 1;
            int i = 1, j = 1;
            while (i < count)
            {

                while (j <= colorNum && i < count)
                {

                    #region
                    int k = 0;
                    for (k = 0; k < i; k++)
                    {
                        if (connMatrix[i, k] == 1 && color[k] == j)//染色有冲突
                        {
                            break;
                        }
                    }
                    if (k < i)//冲突，换新的颜色
                    {
                        if (i == max || i == min)//退回
                        {
                            i--;
                            continue;
                        }

                        j++;
                    }
                    else//当前染色OK
                    {
                        #region
                        //当前第i图斑，颜色为j
                        if (connMatrix[i, max] == 1 && color[max] == j)
                        {
                            j++;
                        }
                        else if (connMatrix[i, min] == 1 && color[min] == j)
                        {
                            j++;
                        }
                        else
                        {
                            if (i == max || i == min)
                            {
                                i++;
                                continue;
                            }
                            color[i] = j;
                            i++;
                            j = 1;
                        }
                        #endregion
                    }
                    #endregion
                }

                if (j > colorNum)//当前超出标记的范围（区域无合适颜色，必须回退）
                {

                    i--;
                    j = color[i] + 1;
                }
            }
        }
        //根据邻接矩阵生成可行的着色数组
        private void MapColor(ref int[] color, int colorNum, int[,] connMatrix, int count)
        {
            color[0] = 1;
            int i = 1, j = 1;
            while (i < count)
            {
                while (j <= colorNum && i < count)
                {

                    int k = 0;
                    for (k = 0; k < i; k++)
                    {
                        if (connMatrix[i, k] == 1 && color[k] == j)//染色有冲突
                        {
                            break;
                        }
                    }
                    if (k < i)//冲突，换新的颜色
                    {
                        j++;
                    }
                    else//当前染色OK
                    {
                        //当前颜色为j
                        color[i] = j;
                        i++;
                        j = 1;
                    }
                }

                if (j > colorNum)//当前超出标记的范围（区域无合适颜色，必须回退）
                {
                    i--;
                    if (i < 0)
                    {
                        throw new Exception("方案颜色数量过少，普色存在冲突！");
                    }
                    j = color[i] + 1;
                }
            }
        }


        //赋值着色
        private void AssignColorFieldEx(int[] color, ILayer layer, IQueryFilter qf, string colorFieldName, Dictionary<int, int> ruleIDPolygon)
        {
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            IGeoFeatureLayer geoFlyr = featureLayer as IGeoFeatureLayer;
            IMapContext mctx = new MapContextClass();
            mctx.Init(m_Application.ActiveView.FocusMap.SpatialReference, m_Application.ActiveView.FocusMap.ReferenceScale, geoFlyr.AreaOfInterest);
            IRepresentationRenderer repRender = geoFlyr.Renderer as IRepresentationRenderer;
            IRepresentationClass rpc = repRender.RepresentationClass;
              

            int indexOfColorField = featureLayer.FeatureClass.FindField(colorFieldName);
            
            //若colorFieldName字段已存在，则直接赋值
            IFeatureCursor featureCursor = featureLayer.Search(qf, false);
            IFeature feature = null;
            int i = 0;
            while ((feature = featureCursor.NextFeature()) != null)
            {

                //feature.set_Value(featureLayer.FeatureClass.FindField(colorFieldName), color[i] + 1);
                //feature.set_Value(featureLayer.FeatureClass.FindField("RuleID"), ruleIDPolygon[color[i]]);
                //featureCursor.UpdateFeature(feature);
                var rep = rpc.GetRepresentation(feature, mctx);
                int ruleID = ruleIDPolygon[color[i]];
                OverrideColorValueSet(rep, ColorDic[ruleID]);
                i++;
            }
            
        }
        private void AssignColorFieldEx0(int[] color, ILayer layer, IQueryFilter qf, string colorFieldName, Dictionary<int, int> ruleIDPolygon)
        {
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            int indexOfColorField = featureLayer.FeatureClass.FindField(colorFieldName);
            ITable table = featureLayer.FeatureClass as ITable;
            {    
                //若colorFieldName字段已存在，则直接赋值
                IFeatureCursor featureCursor = featureLayer.Search(qf, false);
                IFeature feature = null;
                int i = 0;
                while ((feature = featureCursor.NextFeature()) != null)
                {
                    IRow currentRow = table.GetRow(feature.OID);
                    currentRow.set_Value(featureLayer.FeatureClass.FindField(colorFieldName), color[i] + 1);
                    currentRow.set_Value(featureLayer.FeatureClass.FindField("RuleID"), ruleIDPolygon[color[i]]);
                    currentRow.Store();
                    i++;
                }
            }
        }
        private void AddField(IFeatureClass fCls, string fieldName)
        {

            int index = fCls.FindField(fieldName);
            if (index != -1)
                return;
            IFields pFields = fCls.Fields;
            IFieldsEdit pFieldsEdit = pFields as IFieldsEdit;
            IField pField = new FieldClass();
            IFieldEdit pFieldEdit = pField as IFieldEdit;
            pFieldEdit.Name_2 = fieldName;
            pFieldEdit.AliasName_2 = fieldName;
            pFieldEdit.Length_2 = 1;
            pFieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            IClass pTable = fCls as IClass;
            pTable.AddField(pField);
            pFieldsEdit = null;
            pField = null;
        }
        //赋值着色
        private void AssignColorField(int[] color, ILayer layer, IQueryFilter qf, string colorFieldName, Dictionary<string, int> ruleIDPolygon)
        {
            IFeatureLayer featureLayer = layer as IFeatureLayer;
            int indexOfColorField = featureLayer.FeatureClass.FindField(colorFieldName);
            ITable table = featureLayer.FeatureClass as ITable;
            if (indexOfColorField == -1)
            {//若colorFieldName字段不存在，则新建该字段后再赋值
                IField field = new FieldClass();
                IFieldEdit fieldEdit = field as IFieldEdit;
                fieldEdit.Name_2 = colorFieldName;
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
                table.AddField(fieldEdit);

                IFeatureCursor featureCursor = featureLayer.Search(qf, false);
                IFeature feature = null;
                int i = 0;
                while ((feature = featureCursor.NextFeature()) != null)
                {
                    IRow currentRow = table.GetRow(feature.OID);
                    currentRow.set_Value(featureLayer.FeatureClass.FindField(colorFieldName), color[i]);
                    currentRow.set_Value(featureLayer.FeatureClass.FindField("RuleID"), ruleIDPolygon[color[i] + ""]);
                    currentRow.Store();
                    i++;
                }
            }
            else
            {//若colorFieldName字段已存在，则直接赋值
                IFeatureCursor featureCursor = featureLayer.Search(qf, false);
                IFeature feature = null;
                int i = 0;
                while ((feature = featureCursor.NextFeature()) != null)
                {
                    IRow currentRow = table.GetRow(feature.OID);
                    currentRow.set_Value(featureLayer.FeatureClass.FindField(colorFieldName), color[i] + 1);
                    currentRow.set_Value(featureLayer.FeatureClass.FindField("RuleID"), ruleIDPolygon[color[i] + ""]);
                    currentRow.Store();
                    i++;
                }
            }
        }


        private Dictionary<int, int> RuleColorSets(string boua, List<ICmykColor> colors)
        {
            Dictionary<int, int> dicColors = new Dictionary<int, int>();
            var lyrs = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IGeoFeatureLayer) && (l.Name == boua);
            })).ToArray();
            ILayer pRepLayer = lyrs.First();//boua
            var rp = (pRepLayer as IGeoFeatureLayer).Renderer as IRepresentationRenderer;
            IRepresentationClass m_RepClass = rp.RepresentationClass;
            IRepresentationRules rules = m_RepClass.RepresentationRules;
            rules.Reset();
            IRepresentationRule rule = null;
            int ruleID;
            int i = 1;
            while (true)
            {
                rules.Next(out ruleID, out rule);
                if (rule == null) break;
                if (rules.get_Name(ruleID) != "不显示要素" && i <= colors.Count)
                {
                    //修改颜色
                    IBasicFillSymbol fillSym = rule.get_Layer(0) as IBasicFillSymbol;
                    if (fillSym != null)
                    {
                        IFillPattern fillPattern = fillSym.FillPattern;
                        IGraphicAttributes fillAttrs = fillPattern as IGraphicAttributes;
                        if (fillAttrs.ClassName == "SolidColorPattern")
                        {
                            fillAttrs.set_Value(0, colors[i - 1]);
                        }
                        if (fillAttrs.ClassName == "GradientPattern")
                        {
                            int id1 = fillAttrs.get_IDByName("Color1");
                            fillAttrs.set_Value(id1, colors[i - 1]);
                            int id2 = fillAttrs.get_IDByName("Color2");
                            fillAttrs.set_Value(id2, colors[i - 1]);
                        }
                    }
                    dicColors[i] = ruleID;
                    ColorDic[ruleID] = colors[i - 1];
                    i++;
                }
            }
            m_RepClass.RepresentationRules = rules;
            return dicColors;
        }
    }
}
