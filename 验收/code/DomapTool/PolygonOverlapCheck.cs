using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Editor;
using System.Windows.Forms;

namespace DomapTool
{
    /// <summary>
    /// Summary description for PolygonOverlapCheck.
    /// </summary>
    [Guid("1fedabcd-bf2e-4662-bf56-630a5f962c2d")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("DomapTool.PolygonOverlapCheck")]
    public sealed class PolygonOverlapCheck : BaseTool
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

        SimpleFillSymbolClass sfs;
        private IApplication m_application;
        public PolygonOverlapCheck()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = "Domap"; //localizable text 
            base.m_caption = "重叠检查";  //localizable text 
            base.m_message = "重叠检查";  //localizable text
            base.m_toolTip = "面状要素的重叠检查";  //localizable text
            base.m_name = "DomapTool.PolygonOverlapCheck";   //unique id, non-localizable (e.g. "MyCategory_ArcMapTool")
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

        public override bool Deactivate()
        {
            IActiveViewEvents_Event events = ac as IActiveViewEvents_Event;
            events.AfterDraw -= new IActiveViewEvents_AfterDrawEventHandler(events_AfterDraw);
            return true;
        }

        public override bool Enabled
        {
            get
            {
                return base.Enabled && editor.EditState == esriEditState.esriStateEditing;
                //return base.Enabled;
            }
        }
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

            sfs = new SimpleFillSymbolClass();
            SimpleLineSymbolClass sls = new SimpleLineSymbolClass();
            RgbColorClass rgb = new RgbColorClass();
            rgb.Red = 0;
            rgb.Green = 0;
            rgb.Blue = 255;
            sls.Color = rgb;
            sls.Width = 2;
            sfs.Outline = sls;
            sfs.Style = esriSimpleFillStyle.esriSFSNull;
            editor = m_application.FindExtensionByName("ESRI Object Editor") as IEditor;
            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        IFeatureClass fc = null;
        IActiveView ac = null;
        IFeatureLayer areaLayer = null;
        public override void OnClick()
        {
            ac = (m_application.Document as IMxDocument).ActiveView;
            IActiveViewEvents_Event events = ac as IActiveViewEvents_Event;
            events.AfterDraw += new IActiveViewEvents_AfterDrawEventHandler(events_AfterDraw);

            if (gb != null && gb.Count > 0)
            {
                if (System.Windows.Forms.MessageBox.Show(
                        "已经计算过重叠关系，是否重新计算？",
                        "提示",
                        System.Windows.Forms.MessageBoxButtons.YesNo)
                        == System.Windows.Forms.DialogResult.No)
                {
                    (m_application.Document as IMxDocument).ActiveView.Refresh();
                    return;
                }
            }

            IMap map = ac.FocusMap;
            GenerateUpdateAreaWin dlg = new GenerateUpdateAreaWin(map);
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            IFeatureLayer buildingLayer = dlg.SelectUpdateLayer;
            areaLayer = dlg.SelectUpdateAreaLayer;
            fc = buildingLayer.FeatureClass;

            Analysis();
            ac.Refresh();
        }
        void events_AfterDraw(ESRI.ArcGIS.Display.IDisplay dis, esriViewDrawPhase phase)
        {
            if (gb != null)
            {
                bool drawEnv = dis.DisplayTransformation.FromPoints(15) < 8;
                if (true)
                {
                    dis.SetSymbol(sfs);
                    for (int i = 0; i < gb.GeometryCount; i++)
                    {
                        IEnvelope env = gb.get_Geometry(i).Envelope;
                        dis.DrawRectangle(env);
                    }
                }
            }
        }

        List<Dictionary<int, IFeature>> overlapGroups;
        GeometryBagClass gb;
        //void Analysis()
        //{
        //    int remainCount = 0;
        //    IFeature feature = null;
        //    Dictionary<int, int> fid_gid_dic = new Dictionary<int, int>();
        //    List<Dictionary<int, IFeature>> groups = new List<Dictionary<int, IFeature>>();

        //    ISpatialFilter sf = new SpatialFilterClass();
        //    sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;

        //    IStatusBar sBar = m_application.StatusBar;
        //    IStepProgressor sPro = sBar.ProgressBar;
        //    sPro.Position = 0;
        //    sBar.ShowProgressBar("道路重叠分析中...", 0, fc.FeatureCount(null), 1, true);
        //    IFeatureCursor fCursor2;
        //    fCursor2 = fc.Search(null, false);
        //    while ((feature = fCursor2.NextFeature()) != null)
        //    {
        //        try
        //        {
        //            sBar.StepProgressBar();
        //            Dictionary<int, IFeature> g = null;
        //            int groupid = -1;
        //            if (fid_gid_dic.ContainsKey(feature.OID))
        //            {
        //                groupid = fid_gid_dic[feature.OID];
        //                g = groups[groupid];
        //            }
        //            else
        //            {
        //                g = new Dictionary<int, IFeature>();
        //                g.Add(feature.OID, feature);
        //                groupid = groups.Count;
        //                groups.Add(g);
        //                fid_gid_dic.Add(feature.OID, groupid);
        //            }

        //            IGeometry buffered = (feature.Shape as ITopologicalOperator).Buffer(bufferDis);
        //            sf.Geometry = buffered;
        //            IFeatureCursor overlapCursor = fc.Search(sf, false);
        //            //IFeatureCursor overlapCursor = fc.Update(sf, false);
        //            IFeature overlapFeature = null;
        //            while ((overlapFeature = overlapCursor.NextFeature()) != null)
        //            {
        //                if (overlapFeature.OID == feature.OID)
        //                    continue;

        //                if (fid_gid_dic.ContainsKey(overlapFeature.OID))
        //                {
        //                    int find_groupID = fid_gid_dic[overlapFeature.OID];
        //                    if (find_groupID != groupid)
        //                    {
        //                        Dictionary<int, IFeature> fg = groups[find_groupID];
        //                        foreach (var item in fg.Values)
        //                        {
        //                            g.Add(item.OID, item);
        //                            fid_gid_dic[item.OID] = groupid;
        //                        }
        //                        fg.Clear();
        //                    }
        //                    continue;
        //                }

        //                g.Add(overlapFeature.OID, overlapFeature);
        //                fid_gid_dic.Add(overlapFeature.OID, groupid);

        //            }
        //            overlapCursor.Flush();
        //            System.Runtime.InteropServices.Marshal.ReleaseComObject(overlapCursor);
        //        }
        //        catch (Exception ex)
        //        {
        //            //continue;
        //        }
        //    }
        //    sBar.HideProgressBar();
        //    fCursor2.Flush();
        //    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor2);
        //    overlapGroups = new List<Dictionary<int, IFeature>>();
        //    foreach (var item in groups)
        //    {
        //        if (item.Count < 2)
        //        {
        //            continue;
        //        }
        //        overlapGroups.Add(item);
        //    }

        //    object miss = Type.Missing;
        //    gb = new GeometryBagClass();
        //    foreach (var item in overlapGroups)
        //    {
        //        remainCount++;
        //        IEnvelope env = null;
        //        foreach (var fe in item.Values)
        //        {
        //            if (env == null)
        //            {
        //                env = fe.Shape.Envelope;
        //            }
        //            else
        //            {
        //                env.Union(fe.Shape.Envelope);
        //            }
        //        }
        //        gb.AddGeometry(env, ref miss, ref miss);
        //    }
        //    string message = string.Format("剩余{0}个需要手工修复。", remainCount);
        //    System.Windows.Forms.MessageBox.Show(message);
        //}

        void Analysis()
        {
            int complexCount = 0;
            int autoCount = 0;
            int remainCount = 0;
            IFeature feature = null;
            ISpatialFilter sourceSF = new SpatialFilterClass();
            sourceSF.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            if (areaLayer.Name == "全图")
            {
                sourceSF = null;
            }
            else
            {
                IFeatureClass areaClass = areaLayer.FeatureClass;
                IFeatureCursor tpCur = areaClass.Search(null, false);
                IFeature tpFeat = tpCur.NextFeature();
                if (tpFeat == null)
                {
                    MessageBox.Show("范围图层中没有要素！");
                    return;
                }
                sourceSF.Geometry = tpFeat.Shape;
            }
            int wCount = fc.FeatureCount(sourceSF);
            IFeatureCursor fCursor = fc.Search(sourceSF, true);
            List<int> complexFeatures = new List<int>();
            List<int> emptyFeatures = new List<int>();
            IStatusBar sBar = m_application.StatusBar;
            IStepProgressor sPro = sBar.ProgressBar;
            sPro.Position = 0;
            sBar.ShowProgressBar("正在检查复杂多边形以及空多边形...", 0, wCount, 1, true);
            //wo.SetText("正在检查复杂多边形以及空多边形。");
            while ((feature = fCursor.NextFeature()) != null)
            {
                sBar.StepProgressBar();
                if (feature.Shape == null || feature.Shape.IsEmpty || (feature.Shape as IArea).Area < 1)
                {
                    emptyFeatures.Add(feature.OID);
                    continue;
                }
                if (((feature.Shape as IPolygon4).ConnectedComponentBag as IGeometryCollection).GeometryCount > 1)
                {
                    complexFeatures.Add(feature.OID);
                }
                else
                {
                    if (!(feature.Shape as ITopologicalOperator).IsSimple)
                    {
                        ITopologicalOperator sgeo = feature.ShapeCopy as ITopologicalOperator;
                        sgeo.Simplify();
                        feature.Shape = sgeo as IGeometry;
                        feature.Store();
                        complexCount++;
                    }
                }
            }
            foreach (var item in emptyFeatures)
            {
                complexCount++;
                feature = fc.GetFeature(item);
                feature.Delete();
            }
            foreach (var item in complexFeatures)
            {
                complexCount++;
                feature = fc.GetFeature(item);
                ComplexFeature(feature, feature.ShapeCopy as IPolygon4, fc);
            }
            sBar.HideProgressBar();
            fCursor = fc.Search(sourceSF, false);
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelEnvelopeIntersects;
            Dictionary<int, int> fid_gid_dic = new Dictionary<int, int>();
            List<Dictionary<int, IFeature>> groups = new List<Dictionary<int, IFeature>>();
            sBar.ShowProgressBar("正在进行叠置分析...", 0, wCount, 1, true);
            while ((feature = fCursor.NextFeature()) != null)
            {
                sBar.StepProgressBar();
                Dictionary<int, IFeature> g = null;
                int groupid = -1;
                if (fid_gid_dic.ContainsKey(feature.OID))
                {
                    groupid = fid_gid_dic[feature.OID];
                    g = groups[groupid];
                }
                else
                {
                    g = new Dictionary<int, IFeature>();
                    g.Add(feature.OID, feature);
                    groupid = groups.Count;
                    groups.Add(g);
                    fid_gid_dic.Add(feature.OID, groupid);
                }

                sf.Geometry = feature.Shape;
                IFeatureCursor overlapCursor = fc.Search(sf, false);
                IFeature overlapFeature = null;
                while ((overlapFeature = overlapCursor.NextFeature()) != null)
                {
                    if (overlapFeature.OID == feature.OID)
                        continue;

                    if (fid_gid_dic.ContainsKey(overlapFeature.OID))
                    {
                        int find_groupID = fid_gid_dic[overlapFeature.OID];
                        if (find_groupID != groupid)
                        {
                            IArea a = (feature.Shape as ITopologicalOperator).Intersect(overlapFeature.Shape, esriGeometryDimension.esriGeometry2Dimension) as IArea;
                            if (a != null && a.Area > 1)
                            {
                                Dictionary<int, IFeature> fg = groups[find_groupID];
                                foreach (var item in fg.Values)
                                {
                                    g.Add(item.OID, item);
                                    fid_gid_dic[item.OID] = groupid;
                                }
                                fg.Clear();
                            }
                        }
                        continue;
                    }
                    IArea area = (feature.Shape as ITopologicalOperator).Intersect(overlapFeature.Shape, esriGeometryDimension.esriGeometry2Dimension) as IArea;
                    if (area != null && area.Area > 1)
                    {
                        g.Add(overlapFeature.OID, overlapFeature);
                        fid_gid_dic.Add(overlapFeature.OID, groupid);
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(overlapCursor);
            }
            sBar.HideProgressBar();
            //wo.SetText("正在进行自动处理");
            overlapGroups = new List<Dictionary<int, IFeature>>();
            foreach (var item in groups)
            {
                if (item.Count < 2)
                {
                    continue;
                }
                else if (item.Count == 2)
                {
                    autoCount++;
                    IFeature[] fe = new IFeature[2];
                    item.Values.CopyTo(fe, 0);
                    IArea a2 = fe[0].Shape as IArea;
                    IArea a3 = fe[1].Shape as IArea;

                    IArea a1 = (fe[0].Shape as ITopologicalOperator).Intersect(fe[1].Shape, esriGeometryDimension.esriGeometry2Dimension) as IArea;

                    //item.Values.CopyTo(fe, 0);
                    if (Math.Abs(a1.Area / a2.Area - 1) < 0.01 && Math.Abs(a1.Area / a3.Area - 1) < 0.01)
                    {
                        fe[1].Delete();
                        continue;
                    }
                }
                overlapGroups.Add(item);
            }

            object miss = Type.Missing;
            gb = new GeometryBagClass();
            foreach (var item in overlapGroups)
            {
                remainCount++;
                IEnvelope env = null;
                foreach (var fe in item.Values)
                {
                    if (env == null)
                    {
                        env = fe.Shape.Envelope;
                    }
                    else
                    {
                        env.Union(fe.Shape.Envelope);
                    }
                }
                gb.AddGeometry(env, ref miss, ref miss);
            }
            //m_application.SetBusy(false);
            string message = string.Format("共修复复杂多边形{0}个；\n自动修复重叠多边形{1}个，剩余{2}个需要手工修复。", complexCount, autoCount, remainCount);
            System.Windows.Forms.MessageBox.Show(message);
        }

        private void ComplexFeature(IFeature feature, IPolygon4 geo, IFeatureClass fc)
        {
            IFeatureCursor insertCursor = fc.Insert(true);
            IGeometryCollection gc = (geo.ConnectedComponentBag as IGeometryCollection);
            for (int i = 0; i < gc.GeometryCount; i++)
            {
                IGeometry g = gc.get_Geometry(i);
                if (g.IsEmpty || (g as IArea).Area < 1)
                    continue;
                feature.Shape = g;
                insertCursor.InsertFeature(feature as IFeatureBuffer);
            }
            insertCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
            feature.Delete();
        }

        INewPolygonFeedback fb;
        IEditor editor;
        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            if (Button == 4)
                return;
            IPoint p = ac.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);


            editor.StartOperation();
            for (int i = 0; i < gb.GeometryCount; i++)
            {
                IEnvelope env = gb.get_Geometry(i) as IEnvelope;
                if ((env as IRelationalOperator).Contains(p))
                {
                    SelectForm sform = new SelectForm();
                    foreach (var item in overlapGroups[i].Values)
                    {
                        sform.Features.Add(item);
                    }
                    if (sform.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        List<IFeature> insertFeatures = new List<IFeature>();
                        for (int j = 0; j < sform.Features.Count; j++)
                        {
                            if (!sform.FeatureIsSelect[j])
                            {
                                sform.Features[j].Delete();
                            }
                            else
                            {
                                insertFeatures.Add(sform.Features[j]);
                            }
                        }
                        if (insertFeatures.Count >= 2)
                        {
                            IGeometry op = null;
                            foreach (var iFe in insertFeatures)
                            {
                                if (op == null)
                                {
                                    op = iFe.ShapeCopy;
                                    continue;
                                }
                                else
                                {
                                    IGeometry geo = (iFe.Shape as ITopologicalOperator).Difference(op);
                                    op = (iFe.Shape as ITopologicalOperator).Union(op);
                                    if (geo == null || geo.IsEmpty || (geo as IArea).Area < 1)
                                    {
                                        iFe.Delete();
                                    }
                                    else
                                    {
                                        ComplexFeature(iFe, geo as IPolygon4, iFe.Table as IFeatureClass);
                                    }
                                }
                            }
                        }
                        gb.RemoveGeometries(i, 1);
                        overlapGroups.RemoveAt(i);
                        ac.Refresh();
                    }
                    break;
                }
            }
            editor.StopOperation("删除重叠");
        }

        void ProscessAll(bool commit)
        {
            if (true)
            {
                try
                {
                    //editor = m_application.FindExtensionByName("ESRI Object Editor") as IEditor;
                    editor.StartOperation();
                    IStatusBar sBar = m_application.StatusBar;
                    IStepProgressor sPro = sBar.ProgressBar;
                    sPro.Position = 0;
                    sBar.ShowProgressBar("一次性处理中...", 0, gb.GeometryCount, 1, true);
                    for (int i = gb.GeometryCount - 1; i >= 0; i--)
                    {
                        sBar.StepProgressBar();
                        List<IFeature> insertFeatures = new List<IFeature>();
                        foreach (var item in overlapGroups[i].Values)
                        {
                            insertFeatures.Add(item);
                        }

                        if (insertFeatures.Count >= 2)
                        {
                            IGeometry op = null;
                            int opID = -1;
                            foreach (IFeature iFe in insertFeatures)
                            {
                                if (op == null)
                                {
                                    op = iFe.ShapeCopy;
                                    opID = iFe.OID;
                                    continue;
                                }
                                else
                                  {
                                    IGeometry geo = (iFe.Shape as ITopologicalOperator).Difference(op);
                                    op = (iFe.Shape as ITopologicalOperator).Union(op);
                                    if (geo == null || geo.IsEmpty || (geo as IArea).Area < 1) {
                                        iFe.Delete();
                                    }
                                    else {
                                        ComplexFeature(iFe, geo as IPolygon4, iFe.Table as IFeatureClass);
                                    }
                                }
                            }
                        }
                    }
                    sBar.HideProgressBar();
                    gb.RemoveGeometries(0, gb.GeometryCount);
                    overlapGroups.Clear();
                    //ac.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                    ac.Refresh();
                    editor.StopOperation("自动删除所有重叠");
                }
                catch
                {
                }
            }
        }
        int currentErr = 0;
        void Next()
        {
            if (overlapGroups == null || overlapGroups.Count == 0)
                return;
            currentErr--;
            if (currentErr < 0)
            {
                currentErr = gb.Count - 1;
            }
            if (currentErr > gb.Count - 1)
            {
                currentErr = 0;
            }
            IEnvelope env = gb.get_Geometry(currentErr).Envelope;
            env.Expand(8, 8, false);
            ac.Extent = env;
            //ac.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            ac.Refresh();
        }

        public override void OnKeyUp(int keyCode, int Shift)
        {
            switch (keyCode)
            {
                case (int)System.Windows.Forms.Keys.Space:
                    ProscessAll(true);
                    //ac.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                    ac.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.Enter:
                    //ProscessAll(false);
                    //m_application.MapControl.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.R:
                    Analysis();
                    //ac.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                    ac.Refresh();
                    break;
                case (int)System.Windows.Forms.Keys.N:
                    Next();
                    break;
            }
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add CheckOverlapForRoad.OnMouseMove implementation
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add CheckOverlapForRoad.OnMouseUp implementation
        }
        #endregion
    }
}
