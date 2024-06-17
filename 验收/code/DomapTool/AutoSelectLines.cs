using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;

namespace DomapTool
{
    /// <summary>
    /// Summary description for AutoSelectLines.
    /// </summary>
    [Guid("60fe9062-5d3b-4a51-8cdf-2e92ff4a84cf")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DomapTool.AutoSelectLines")]
    public sealed class AutoSelectLines : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);

        }

        #endregion
        #endregion
        INewPolygonFeedback fb;
        private IApplication m_application;
        public AutoSelectLines()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "隔条选取"; //localizable text 
            base.m_caption = "隔条选取";  //localizable text 
            base.m_message = "隔条选取";  //localizable text
            base.m_toolTip = "隔条选取";  //localizable text
            base.m_name = "DomapTool.AutoSelectLines";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
            try
            {
                //
                // TODO: change resource name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overriden Class Methods
        IFeatureClass fc = null;
        IFeatureLayer waterLayer = null;
        IEditor editor;
        IActiveView ac;
        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            m_application = hook as IApplication;

            //Disable if it is not ArcMap
            if (hook is IMxApplication)
                base.m_enabled = true;
            else
                base.m_enabled = false;
            editor = m_application.FindExtensionByName("ESRI Object Editor") as IEditor;
            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        double mindis;
        public override void OnClick()
        {
            ac = (m_application.Document as IMxDocument).ActiveView;
            LayersPara d = new LayersPara(ac.FocusMap);
            d.Text = "隔条选取";
            d.textBox1.Text = Convert.ToString(180);
            if (d.ShowDialog() == DialogResult.Cancel)
                return;
            waterLayer = d.SelectLayer;
            mindis = d.dis;
            fc = waterLayer.FeatureClass;
            if (waterLayer == null)
            {
                MessageBox.Show("请确保操作图层不为空!");
                return;
            }
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && editor.EditState == esriEditState.esriStateEditing;
            }
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            IPoint p = (m_application.Document as IMxDocument).ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
            if (fb == null)
            {
                fb = new NewPolygonFeedbackClass();
                fb.Display = (m_application.Document as IMxDocument).ActiveView.ScreenDisplay;
                fb.Start(p);
            }
            else
            {
                fb.AddPoint(p);
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            if (fb != null)
            {
                IPoint p = (m_application.Document as IMxDocument).ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                fb.MoveTo(p);
            }
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add AutoSelectLines.OnMouseUp implementation
        }

        public override void OnDblClick()
        {
            if (fb == null)
                return;
            IPolygon p = fb.Stop();
            fb = null;
            autoSelectDitchLines(p);
        }

        public bool isOutsideTri(ITinTriangle tri)
        {
            int tag = -1;
            tag = (tri.get_Node(0)).TagValue;
            if (tag == (tri.get_Node(1)).TagValue && tag == (tri.get_Node(2)).TagValue)
            {
                return false;
            }
            if (tag != (tri.get_Node(1)).TagValue && tag != (tri.get_Node(2)).TagValue && (tri.get_Node(1)).TagValue != (tri.get_Node(2)).TagValue)
            {
                return false;
            }
            return true;

        }
        public void addNodeOIDtoList(ITinTriangle tri, List<int> toList)
        {
            bool isExisted = false;
            for (int i = 0; i < 3; i++)
            {
                ITinNode node = tri.get_Node(i);

                foreach (int j in toList)
                {
                    if (j == node.TagValue)
                    {
                        isExisted = true;
                    }
                }
                if (!isExisted)
                {
                    toList.Add(node.TagValue);
                }
            }
        }
        Dictionary<int, bool> hasDeletedFeats = new Dictionary<int, bool>();
        Dictionary<int, bool> hasSavedFeats = new Dictionary<int, bool>();
        Dictionary<int, List<int>> Feat_neighberFeats = new Dictionary<int, List<int>>();
        void select(IFeature startFeat, bool priviousFeatHasDeleted)
        {
            if (!Feat_neighberFeats.ContainsKey(startFeat.OID) || hasDeletedFeats.ContainsKey(startFeat.OID) || hasSavedFeats.ContainsKey(startFeat.OID))
            {
                return;
            }
            List<int> nextList = Feat_neighberFeats[startFeat.OID];
            if (!priviousFeatHasDeleted)
            {
                if (!hasDeletedFeats.ContainsKey(startFeat.OID))
                {
                    hasDeletedFeats.Add(startFeat.OID, true);
                }
                //startFeat.set_Value(_GenUsed2, -1);
                //startFeat.Store();
                //System.Diagnostics.Debug.WriteLine(startFeat.OID);
                //startFeat.Delete();
            }
            else
            {
                if (!hasSavedFeats.ContainsKey(startFeat.OID))
                {
                    hasSavedFeats.Add(startFeat.OID, true);
                }
                //startFeat.set_Value(_GenUsed2, 1);
                //startFeat.Store();
            }

            if (!priviousFeatHasDeleted)
            {
                foreach (int nextIDs in nextList)
                {
                    if (hasDeletedFeats.ContainsKey(nextIDs))
                    {
                        continue;
                    }
                    IFeature nextFeat = ID_Feat[nextIDs];
                    select(nextFeat, true);
                }
            }
            else
            {
                foreach (int nextIDs in nextList)
                {
                    if (hasSavedFeats.ContainsKey(nextIDs))
                    {
                        continue;
                    }
                    IFeature nextFeat = ID_Feat[nextIDs];
                    select(nextFeat, false);
                }
            }

        }
        Dictionary<int, IFeature> ID_Feat = new Dictionary<int, IFeature>();
        TinClass tin;
        void autoSelectDitchLines(IPolygon polygon2)
        {
            IFeatureClass lineFC = fc;
            ID_Feat.Clear();
            Feat_neighberFeats.Clear();
            hasSavedFeats.Clear();
            hasDeletedFeats.Clear();

            IStatusBar sBar = m_application.StatusBar;
            IStepProgressor sPro = sBar.ProgressBar;
            sPro.Position = 0;

            editor.StartOperation();
            //WaitOperation wo = m_application.SetBusy(true);
            //wo.SetText("正在进行分析准备……");
            tin = new TinClass();
            tin.InitNew(ac.FullExtent);
            tin.StartInMemoryEditing();

            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            sf.Geometry = polygon2;
            IFeatureCursor fCursor = fc.Search(sf, false);
            int featureCount = fc.FeatureCount(sf);
            IFeature feature = null;
            ITinNode node = new TinNodeClass();
            object z = 0;
            try
            {
                sBar.ShowProgressBar("正在进行分析准备...", 0, featureCount, 1, true);
                while ((feature = fCursor.NextFeature()) != null)
                {
                    sBar.StepProgressBar();
                    IPolyline line = feature.ShapeCopy as IPolyline;
                    if (line.Length < mindis)
                    {
                        //feature.set_Value(_GenUsed2, -2);
                        //feature.Store();
                        feature.Delete();
                        continue;
                    }
                    ITopologicalOperator lineto = line as ITopologicalOperator;
                    lineto.Simplify();
                    line.Densify(4, 0);
                    IPointCollection pc = line as IPointCollection;
                    for (int i = 0; i < pc.PointCount; i++)
                    {
                        tin.AddShape(pc.get_Point(i), esriTinSurfaceType.esriTinMassPoint, feature.OID, ref z);
                    }
                    ID_Feat.Add(feature.OID, feature);
                }
                ITinEdit tinEdit = tin as ITinEdit;
                ITinAdvanced tinAd = tin as ITinAdvanced;
                ITinValueFilter filter = new TinValueFilterClass();
                ITinFeatureSeed seed = new TinTriangleClass();
                filter.ActiveBound = esriTinBoundType.esriTinUniqueValue;
                seed.UseTagValue = true;
                sPro.Position = 0;
                sBar.ShowProgressBar("还是在分析...", 0, tin.TriangleCount, 1, true);
                for (int i = 1; i < tin.TriangleCount + 1; i++)
                {
                    sBar.StepProgressBar();
                    ITinTriangle tinTriangle = tin.GetTriangle(i);
                    bool istoolong = false;
                    if (!tinTriangle.IsInsideDataArea || !isOutsideTri(tinTriangle))
                    {
                        tinEdit.SetTriangleTagValue(i, -2);
                        continue;
                    }
                    else
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            ITinEdge tinEdge = tinTriangle.get_Edge(k);
                            if (tinEdge.Length > mindis)
                            {
                                istoolong = true;
                                tinEdit.SetTriangleTagValue(i, -2);
                                break;
                            }
                        }
                    }
                    int oid1 = -1;
                    int oid2 = -1;
                    if (!istoolong)
                    {
                        oid1 = (tinTriangle.get_Node(0)).TagValue;
                        if ((tinTriangle.get_Node(1)).TagValue != oid1)
                        {
                            oid2 = (tinTriangle.get_Node(1)).TagValue;
                        }
                        else
                        {
                            oid2 = (tinTriangle.get_Node(2)).TagValue;
                        }
                        int lasttagvalue = 0;
                        if (oid1 < oid2)
                        {
                            lasttagvalue = oid1 * 2 + oid2;
                        }
                        else
                        {
                            lasttagvalue = oid2 * 2 + oid1;
                        }
                        tinEdit.SetTriangleTagValue(i, lasttagvalue);
                    }
                }
                sBar.ShowProgressBar("存储分析结果...", 0, tin.TriangleCount, 1, true);
                for (int i = 1; i < tin.TriangleCount + 1; i++)
                {
                    sBar.StepProgressBar();
                    int tpValue = (tin.GetTriangle(i)).TagValue;
                    if (tpValue != -2 && tpValue != 0)
                    {
                        filter.UniqueValue = (tin.GetTriangle(i)).TagValue;
                        seed = tin.GetTriangle(i) as ITinFeatureSeed;
                        ITinPolygon tinPoly = tinAd.ExtractPolygon(seed as ITinElement, filter as ITinFilter, false);
                        IPolygon polygon = tinPoly.AsPolygon(null, false);
                        IEnumTinTriangle enumTri = tinPoly.AsTriangles();
                        int triCount = 0;
                        enumTri.Reset();
                        ITinTriangle tpTri = null;
                        List<int> touchOIDList = new List<int>();
                        while ((tpTri = enumTri.Next()) != null)
                        {
                            addNodeOIDtoList(tpTri, touchOIDList);
                            tinEdit.SetTriangleTagValue(tpTri.Index, -2);
                            triCount++;
                        }
                        if (triCount < 15)
                        {
                            continue;
                        }
                        int[] touchArray = touchOIDList.ToArray();
                        if (touchArray.Length < 2)
                        {
                            continue;
                        }
                        IFeature oriFeat = ID_Feat[touchArray[0]];;
                        IPolyline oriLine = oriFeat.Shape as IPolyline;
                        IFeature nextFeat = ID_Feat[touchArray[1]];
                        IPolyline nextLine = nextFeat.Shape as IPolyline;
                        double oriAngle = (oriLine.FromPoint.Y - oriLine.ToPoint.Y) / (oriLine.FromPoint.X - oriLine.ToPoint.X);
                        double nextAngle = (nextLine.FromPoint.Y - nextLine.ToPoint.Y) / (nextLine.FromPoint.X - nextLine.ToPoint.X);
                        double difference = Math.Abs(oriAngle - nextAngle);
                        if (difference > 0.5)
                        {
                            continue;
                        }

                        if (Feat_neighberFeats.ContainsKey(touchArray[0]))
                        {
                            List<int> neighberFeats = Feat_neighberFeats[touchArray[0]];
                            if (!neighberFeats.Contains(touchArray[1]))
                            {
                                neighberFeats.Add(touchArray[1]);
                            }
                        }
                        else
                        {
                            List<int> neighberFeats = new List<int>();
                            neighberFeats.Add(touchArray[1]);
                            Feat_neighberFeats.Add(touchArray[0], neighberFeats);
                        }

                        if (Feat_neighberFeats.ContainsKey(touchArray[1]))
                        {
                            List<int> neighberFeats = Feat_neighberFeats[touchArray[1]];
                            if (!neighberFeats.Contains(touchArray[0]))
                            {
                                neighberFeats.Add(touchArray[0]);
                            }
                        }
                        else
                        {
                            List<int> neighberFeats = new List<int>();
                            neighberFeats.Add(touchArray[0]);
                            Feat_neighberFeats.Add(touchArray[1], neighberFeats);
                        }
                    }
                }
                //wo.Step(0);
                int co = fc.FeatureCount(sf);
                IFeatureCursor fcuu;
                fcuu = fc.Update(sf, true);
                IFeature f = null;
                sBar.ShowProgressBar("整理数据...", 0, co, 1, true);
                //editor.StartOperation();
                while ((f = fcuu.NextFeature()) != null)
                {
                    sBar.StepProgressBar();
                    select(f, false);
                }
                foreach (int dID in hasDeletedFeats.Keys)
                {
                    IFeature dF = ID_Feat[dID];
                    dF.Delete();
                }
                editor.StopOperation("隔条选取");
                sBar.HideProgressBar();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                ac.Refresh();
            }
            catch (Exception ex)
            {
                editor.StopOperation("隔条选取");
                sBar.HideProgressBar();
            } 
        }

        #endregion
    }
}
