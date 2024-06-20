using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Common
{
    class LabelFeatureLayer
    {
        public static void LabelFeatureLayerInfo()
        {


        }

        public void ConvertLabelsToGDBAnnotationSingleLayer(IMap pMap, IGroupLayer targetgrouplyr,
           IFeatureLayer pLayer, string field_, string expression_, ISymbol symbol_, bool featureLinked, bool issimple)
        {

            IConvertLabelsToAnnotation pConvertLabelsToAnnotation = new
              ConvertLabelsToAnnotationClass();
            ITrackCancel pTrackCancel = new CancelTrackerClass();
            //Change global level options for the conversion by sending in different parameters to the next line.
            pConvertLabelsToAnnotation.Initialize(pMap,
              esriAnnotationStorageType.esriDatabaseAnnotation,
              esriLabelWhichFeatures.esriAllFeatures, true, pTrackCancel, null);

            IGeoFeatureLayer pGeoFeatureLayer = pLayer as IGeoFeatureLayer;
            if (pGeoFeatureLayer != null)
            {
                pGeoFeatureLayer.DisplayAnnotation = true;
                pGeoFeatureLayer.DisplayField = field_;

                IAnnotateLayerPropertiesCollection pAnnProperties = pGeoFeatureLayer.AnnotationProperties;

                IAnnotateLayerProperties pAnnProperty = null;
                IElementCollection outEle1 = null; IElementCollection outEle2 = null;
                pAnnProperties.QueryItem(0, out pAnnProperty, out outEle1, out outEle2);
                ILabelEngineLayerProperties2 pLabelEngine = null;
                pLabelEngine = pAnnProperty as ILabelEngineLayerProperties2;

                //IMaplexOverposterLayerProperties pMapLex = new MaplexOverposterLayerPropertiesClass();
                //生成的注记和原始几何图形的相对位置关系
                //pMapLex.PointPlacementMethod = esriMaplexPointPlacementMethod.esriMaplexEastOfPoint;
                //pMapLex.PolygonPlacementMethod = esriMaplexPolygonPlacementMethod.esriMaplexHorizontalInPolygon;
                IBasicOverposterLayerProperties bolp = new BasicOverposterLayerPropertiesClass();
                (bolp as IBasicOverposterLayerProperties2).PointPlacementMethod = esriOverposterPointPlacementMethod.esriAroundPoint;
                (bolp as IBasicOverposterLayerProperties2).NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerPart;
                IPointPlacementPriorities ppp = new PointPlacementPrioritiesClass();
                ppp.CenterRight = 1;
                ppp.AboveRight = 2;
                ppp.BelowRight = 3;
                ppp.AboveCenter = 4;
                ppp.BelowCenter = 5;
                bolp.PointPlacementPriorities = ppp;

                pLabelEngine.OverposterLayerProperties = bolp as IOverposterLayerProperties;

                pLabelEngine.IsExpressionSimple = issimple;
                pLabelEngine.Expression = expression_;
                if (symbol_ != null)
                {
                    pLabelEngine.Symbol = (ITextSymbol)symbol_;
                }
                /**
                pLabelEngine.Expression = "fix([BSGC])"
                 * 
                 *
                 * Function FindLabel ( [GDPDJ], [GDLX] )
                    if [GDPDJ]=1 then FindLabel =  "I"
                    if [GDPDJ]=2 then FindLabel = "II"
                    if [GDPDJ]=3 then FindLabel =  "III"
                    if [GDPDJ]=4 then FindLabel =  "IV"
                    if [GDPDJ]=5 then FindLabel =  "V"
                    if [GDLX]="T" then FindLabel = FindLabel+ "-T"
                    End Function
                 */

                IFeatureClass pFeatureClass = pGeoFeatureLayer.FeatureClass;
                IDataset pDataset = pFeatureClass as IDataset;

                IFeatureWorkspace pFeatureWorkspace = pDataset.Workspace as
                  IFeatureWorkspace;
                if ((pFeatureWorkspace as IWorkspace2).get_NameExists(esriDatasetType.esriDTFeatureClass,
                    pGeoFeatureLayer.Name + "_Anno"))
                {
                    IDataset ds = (IDataset)pFeatureWorkspace.OpenFeatureClass(pGeoFeatureLayer.Name + "_Anno");
                    ds.Delete();
                }
                //Add the layer information to the converter object. Specify the parameters of the output annotation feature class here as well.
                pConvertLabelsToAnnotation.AddFeatureLayer(
                    pGeoFeatureLayer,
                    pGeoFeatureLayer.Name + "_Anno",
                    pFeatureWorkspace,
                    pFeatureClass.FeatureDataset,
                    featureLinked,
                    false, false, true, true, "");

                //Do the conversion.???
                pConvertLabelsToAnnotation.ConvertLabels();

                IEnumLayer pEnumLayer = pConvertLabelsToAnnotation.AnnoLayers;
                if (pEnumLayer == null)
                {
                    return;
                }
                //Turn off labeling for the layer converted.
                pGeoFeatureLayer.DisplayAnnotation = false;

                //Add the result annotation layer to the map.
                if (targetgrouplyr == null)
                {
                    pMap.AddLayers(pEnumLayer, true);
                }
                else
                {
                    try
                    {
                        pEnumLayer.Reset();
                        ILayer lyr = null;
                        while ((lyr = pEnumLayer.Next()) != null)
                        {
                            targetgrouplyr.Add(lyr);
                        }
                    }
                    catch
                    {
                        pMap.AddLayers(pEnumLayer, true);
                    }
                }

                //Refresh the map to update the display.
                IActiveView pActiveView = pMap as IActiveView;
                pActiveView.Refresh();
            }
        }


    }
}
