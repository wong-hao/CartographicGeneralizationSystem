using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;

using SMGI.Common;

namespace SMGI.Plugin.ThematicChart.ThematicChart.ThematicMinMap
{
    public  class DrawMinMapCmd:SMGICommand
    {
        private IActiveView pAc = null;
        private double mapScale;
        public DrawMinMapCmd()
        {
 
        }
        private string layName;
        public override bool Enabled
        {
            get
            {
                return m_Application !=null  && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            pAc = m_Application.ActiveView;
            mapScale = (m_Application.ActiveView as IMap).ReferenceScale;
            var lyrs = m_Application.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
            { return l is IFDOGraphicsLayer; })).ToArray();
            List<string> listNames = new List<string>();
            for (int i = 0; i < lyrs.Length; i++)
            {
                listNames.Add(lyrs[i].Name);
            }
            FrmMinMapSet frm = new FrmMinMapSet(listNames.ToArray());
            DialogResult dr = frm.ShowDialog();
            if (dr != DialogResult.OK)
                return;
            string name = frm.minMapName;
            layName = frm.layerName;
            IPoint cpt = AtlasApplication.CurrentPage.MapCenter;
            double pageW;
            double pageH;
            XMLHelper.ObtainBookInfos(out pageH, out pageW);
            if (AtlasApplication.CurrentPage.MapPageType == pageType.PageDouble)
            {
                pageW *= 2;
            }
            pageW *= mapScale * 1e-3;
            pageH *= mapScale * 1e-3;
            IPoint lowerRight = new PointClass() { X = cpt.X + pageW / 2, Y = cpt.Y - pageH };

            
            IEnvelope pen = new EnvelopeClass();

            if (pageW >= pageH)
            {
                double xmin = lowerRight.X - pageW / 3;
                double ymin = lowerRight.Y + pageH / 2;
                double xmax = lowerRight.X;
                double ymax = lowerRight.Y + pageH ;
                pen.PutCoords(xmin, ymin, xmax, ymax);
            }
            else
            {
                double xmin = lowerRight.X - pageW / 2;
                double ymin = lowerRight.Y + pageH*2 / 3;
                double xmax = lowerRight.X;
                double ymax = lowerRight.Y + pageH;
                pen.PutCoords(xmin, ymin, xmax, ymax);
            }
            IGeometry pGeo = pen as IGeometry;
            getMinMap(pGeo, name);
            //(pAc as IGraphicsContainer).AddElement(ele, 0);
            pAc.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, pGeo.Envelope);
        }

        private void  getMinMap(IGeometry geo, string mapName)
        {
            IPictureElement picEl = new PngPictureElementClass();
            picEl.ImportPictureFromFile(mapName);
            picEl.MaintainAspectRatio = false;
            IElement pel = picEl as IElement;
            pel.Geometry = geo;
            testPicAnno(pel);
        }

        private void testPicAnno( IElement ele)
        {
            var lyr = GApplication.Application.Workspace.LayerManager.GetLayer(new LayerManager.LayerChecker(l =>
            {
                return (l is IFDOGraphicsLayer) && ((l as IFeatureLayer).FeatureClass.AliasName == layName);
            })).ToArray();
            ILayer pRepLayer = lyr.First();
            IFeatureLayer flayer = pRepLayer as IFeatureLayer;
            IFeatureClass fClass = flayer.FeatureClass;
            IFeature feature = fClass.CreateFeature();
            IAnnotationFeature2 annoFeature = feature as IAnnotationFeature2;
            annoFeature.Annotation = ele;
            annoFeature.Status = esriAnnotationStatus.esriAnnoStatusPlaced;
            feature.Store();
        }

    }
}
