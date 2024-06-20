using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using System.Security.Cryptography;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
namespace SMGI.Plugin.EmergencyMap.LabelSym
{
    public partial class FrmAnnoMap : Form
    {
     
        public FrmAnnoMap(ILayer lyr,int oid_)
        {
            InitializeComponent();
            axMapControl1.AddLayer(lyr);
            IGeoFeatureLayer geoLyr = lyr as IGeoFeatureLayer;
            if (geoLyr != null)
            {
                IFeatureClass pFeatureClass = geoLyr.FeatureClass;
                string oid = pFeatureClass.OIDFieldName;
                (lyr as IFeatureSelection).SelectFeatures(new QueryFilterClass { WhereClause = oid + "=" + oid_ }, esriSelectionResultEnum.esriSelectionResultNew, false);
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, axMapControl1.ActiveView.FullExtent);
            }
        }
        public void ConvertLabelsToAnnotationSingleLayerMapAnno(int layerIndex, string expression = "")
        {
            
            IMap pMap = axMapControl1.Map;
            pMap.ReferenceScale = 1;
            ILayer pLayer = pMap.get_Layer(layerIndex);
            IGeoFeatureLayer geoLyr = pLayer as IGeoFeatureLayer;
            if (geoLyr != null)
            {
                IFeatureClass pFeatureClass = geoLyr.FeatureClass;
                IConvertLabelsToAnnotation pConvertLabelsToAnnotation = new
                    ConvertLabelsToAnnotationClass();
                ITrackCancel pTrackCancel = new CancelTrackerClass();
                //Change global level options for the conversion by sending in different parameters to the next line.

                ILabelEngineLayerProperties leLyrProp = new LabelEngineLayerPropertiesClass();
                if (expression == "")
                {
                    expression = "[" + pFeatureClass.OIDFieldName + "]";
                }
                else
                {
                    if (expression.ToUpper().Contains("FUNCTION"))
                    {
                        leLyrProp.IsExpressionSimple = false;
                        IAnnotationExpressionEngine pAee = new AnnotationVBScriptEngineClass();
                        leLyrProp.ExpressionParser = pAee;
                    }
                  
                }
                leLyrProp.Expression = expression;
          
                IBasicOverposterLayerProperties lyrProp = new BasicOverposterLayerPropertiesClass();
                lyrProp.NumLabelsOption = esriBasicNumLabelsOption.esriOneLabelPerShape;
                leLyrProp.BasicOverposterLayerProperties = lyrProp;
                (leLyrProp as IAnnotateLayerProperties).CreateUnplacedElements = true;
                (lyrProp as IOverposterLayerProperties).PlaceLabels = true;

                IAnnotateLayerPropertiesCollection annoPropColl = (geoLyr.AnnotationProperties as IClone).Clone() as IAnnotateLayerPropertiesCollection;
                geoLyr.AnnotationProperties.Clear();
                bool bDispAnno = geoLyr.DisplayAnnotation;
                geoLyr.AnnotationProperties.Add(leLyrProp as IAnnotateLayerProperties);
                geoLyr.DisplayAnnotation = true;
                try
                {
                    pConvertLabelsToAnnotation.Initialize(pMap,
                        esriAnnotationStorageType.esriMapAnnotation,
                        esriLabelWhichFeatures.esriSelectedFeatures, true, pTrackCancel, null);


                    //Add the layer information to the converter object. Specify the parameters of the output annotation feature class here as well.
                    pConvertLabelsToAnnotation.AddFeatureLayer(geoLyr,
                            geoLyr.Name + "_Anno", null, null, false, false, false, false,
                            false, "");
                    //Do the conversion.
                    pConvertLabelsToAnnotation.ConvertLabels();
                    //Turn off labeling for the layer converted.
                    geoLyr.DisplayAnnotation = false;
                    //Refresh the map to update the display.
                    IActiveView pActiveView = pMap as IActiveView;
                    pActiveView.Refresh();
                }
                catch
                {
                }
            }
            GC.Collect();
        }
        public string MapLabels
        {
            get
            {
                string info = string.Empty;
                
                IGraphicsLayer gl = axMapControl1.ActiveView.FocusMap.BasicGraphicsLayer;
                ICompositeGraphicsLayer cgl = gl as ICompositeGraphicsLayer;
                IGraphicsLayer sublayer;
                ILayer pLayer = axMapControl1.ActiveView.FocusMap.get_Layer(0);
                IGeoFeatureLayer geoLyr = pLayer as IGeoFeatureLayer;
                sublayer = cgl.FindLayer(geoLyr.Name + "_Anno");//查找Com
                IGraphicsContainer gc = sublayer as IGraphicsContainer;
                gc.Reset();
                IElement ele = null;

                while ((ele = gc.Next()) != null)
                {
                    string val = (ele as ITextElement).Text;
                    IElementProperties ep = ele as IElementProperties;
                    info+= val;
                }
                return info;
            }
        }
        private void FrmAnnoMap_Load(object sender, EventArgs e)
        {
            ConvertLabelsToAnnotationSingleLayerMapAnno(0);
           
        }
    }
}
