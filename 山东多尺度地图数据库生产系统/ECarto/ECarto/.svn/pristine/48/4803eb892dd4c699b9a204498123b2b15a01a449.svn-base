using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Windows.Forms;
using SMGI.Common;
using ESRI.ArcGIS.Controls;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.IO;
using ESRI.ArcGIS.DataSourcesGDB;

namespace SMGI.Plugin.EmergencyMap
{
    public class MaskProcessHelper
    {
        public MaskProcessHelper()
        {
        }

        /// <summary>
        ///在被掩膜的图层中查找要素，该要素与参照图层（满足条件的指定要素）相交，并在制图表达层面将其掩膜 
        /// </summary>
        /// <param name="repWS">制图表达工作空间</param>
        /// <param name="MaskedFC">被掩膜的图层</param>
        /// <param name="refFC">参照图层</param>
        /// <param name="qf">属性查询</param>
        /// <param name="whereClause">过滤条件</param>
        /// <param name="sf">空间查询</param>
        public static void MaskProcess(IWorkspace workspace, IFeatureClass MaskedFC, IFeatureClass refFC, IQueryFilter qf, ISpatialFilter sf)
        {

            double bufferDis = 5;
            XElement expertiseContent = ExpertiseDatabase.getContentElement(GApplication.Application);
            XElement bridgeMask = expertiseContent.Element("BridgeMask");
            if (bridgeMask != null)
            {
                string val = bridgeMask.Value;
                double.TryParse(val, out bufferDis);
                if(bufferDis==0)
                    bufferDis = 5;
            }

            IRepresentationWorkspaceExtension repWS = GetRepersentationWorkspace(workspace);
            if (repWS.get_FeatureClassHasRepresentations(MaskedFC) && repWS.get_FeatureClassHasRepresentations(refFC))   //被掩膜的图层要有制图表达
            {
                IEnumDatasetName enumRepName = repWS.get_FeatureClassRepresentationNames(MaskedFC);
                enumRepName.Reset();
                IRepresentationClass maskedRPC = repWS.OpenRepresentationClass(enumRepName.Next().Name);  //获取被掩膜图层的要素类

                enumRepName = repWS.get_FeatureClassRepresentationNames(refFC);
                enumRepName.Reset();
                IRepresentationClass refRPC = repWS.OpenRepresentationClass(enumRepName.Next().Name);  //获取参照图层的要素类

                //初始化一个几何效果的地图环境
                IMapContext mctx = new MapContextClass();
                mctx.Init((MaskedFC as IGeoDataset).SpatialReference, 10000, (MaskedFC as IGeoDataset).Extent);
                if (maskedRPC != null && refRPC != null)
                {
                    Dictionary<IRepresentation, IRepresentation> maskedReps = new Dictionary<IRepresentation, IRepresentation>();
                    IFeature refFeture = null;
                    IFeatureCursor refCursor = refFC.Search(qf, false);

                    while ((refFeture = refCursor.NextFeature()) != null)
                    {
                        sf.Geometry = refFeture.Shape;
                        int nCount = MaskedFC.FeatureCount(sf);

                        if (nCount > 0)    //参照图层与被掩膜的图层有相交
                        {
                            var refRep = refRPC.GetRepresentation(refFeture, mctx);

                            IFeature maskedFeature = null;
                            IFeatureCursor maskedCursor = MaskedFC.Search(sf, false);
                            while ((maskedFeature = maskedCursor.NextFeature()) != null)
                            {
                                List<string> targetFeature = new List<string>();
                                var maskedRep = maskedRPC.GetRepresentation(maskedFeature, mctx);

                                var filter = new SpatialFilterClass { Geometry = maskedRep.Shape, SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin };
                                IFeatureCursor feCu = refFC.Search(filter, false);
                                IFeature fe;
                                while ((fe = feCu.NextFeature()) != null)
                                {
                                    targetFeature.Add(fe.OID.ToString());
                                }
                                Marshal.ReleaseComObject(feCu);

                                if (!targetFeature.Contains(refFeture.OID.ToString()))
                                {
                                    maskedReps.Add(maskedRep, refRep);
                                }
                            }
                            Marshal.ReleaseComObject(maskedCursor);
                        }
                        foreach (var maskedRep in maskedReps.Keys)
                        {
                            IGeometry bufGeo = (maskedReps[maskedRep].Shape as ITopologicalOperator).Buffer(bufferDis);
                            try
                            {
                                maskedRep.Shape = (maskedRep.Shape as ITopologicalOperator).Difference(bufGeo);
                            }
                            catch (Exception)
                            {
                                maskedRep.Shape = maskedReps[maskedRep].Shape;
                            }

                            maskedRep.UpdateFeature();
                            maskedRep.Feature.Store();
                        }
                    }
                    Marshal.ReleaseComObject(refCursor);

                   
                }
                Marshal.ReleaseComObject(enumRepName);
            }
        }

        public static void MaskProcess(StreamWriter outputSW, IFeatureClass targetFC, IFeatureClass lfclFC, IQueryFilter qf, ISpatialFilter sf, string mdbName)
        {
            var whereClause = qf.WhereClause;
            IFeature fe = null;
            IFeatureCursor cursor = lfclFC.Search(qf, true);
            while ((fe = cursor.NextFeature()) != null)
            {
                sf.Geometry = fe.Shape;
                int nCount = targetFC.FeatureCount(sf);
                if (nCount > 0)
                {
                    outputSW.WriteLine("【" + mdbName + "】LFCL(" + whereClause.Substring(whereClause.LastIndexOf("=") + 1) + "):" + fe.OID.ToString());
                }
            }
            Marshal.ReleaseComObject(cursor);
        }

        public static IRepresentationWorkspaceExtension GetRepersentationWorkspace(IWorkspace workspace)
        {
            IWorkspaceExtensionManager wem = workspace as IWorkspaceExtensionManager;
            UID uid = new UIDClass();
            uid.Value = "{FD05270A-8E0B-4823-9DEE-F149347C32B6}";
            IRepresentationWorkspaceExtension rwe = wem.FindExtension(uid) as IRepresentationWorkspaceExtension;
            return rwe;
        }
    }
}