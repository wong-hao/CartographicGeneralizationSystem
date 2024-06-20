using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using System.Windows.Forms;

namespace SMGI.Plugin.EmergencyMap
{
    /// <summary>
    /// 注记打散,字向朝北,只处理河流等不规则注记
    /// </summary>
    public class AnnoHorVerConvert
    {
        private GApplication _app;//应用程序
        private string AnnoLyrName = "";
        public AnnoHorVerConvert(GApplication app, string annoName = "ANNO")
        {
            _app = app;
            AnnoLyrName = annoName;
        }    
        public string ConvertHorVer(WaitOperation wo = null)
        {
            string msg = "";

            try
            {
                var lyrs = _app.Workspace.LayerManager.GetLayer(new SMGI.Common.LayerManager.LayerChecker(l =>
                { return ((l is IFDOGraphicsLayer) && (l as IFeatureLayer).FeatureClass.AliasName == AnnoLyrName); })).ToArray();

                if (lyrs.Length > 0)
                {                  
                        IFeatureLayer annoFeatureLayer = lyrs[0] as IFeatureLayer;
                        //if (wo != null)
                        //    wo.SetText("正在处理" + annoFeatureLayer.Name + "……");
                        IFeatureClass annoFeatureClass = annoFeatureLayer.FeatureClass;
                        var pEnumFeature = _app.Workspace.Map.FeatureSelection as IEnumFeature;
                    IFeature fea;
                    while((fea=pEnumFeature.Next())!=null)
                    {
                       // wo.SetText("正在处理" + annoFeatureLayer.Name +"-"+ fea.OID+"……");
                        IAnnotationFeature annofea = fea as IAnnotationFeature;
                        if (annofea == null) continue;
                        if (annofea.Annotation == null) continue;
                        IFormattedTextSymbol pTextSymbol = ((annofea.Annotation as ITextElement).Symbol) as IFormattedTextSymbol;  
                        bool cjkrotation=((annofea.Annotation as ITextElement).Symbol as ICharacterOrientation).CJKCharactersRotation;
                        double oldCenterx = (fea.Shape.Envelope.XMax + fea.Shape.Envelope.XMin) / 2;
                        double oldCentery = (fea.Shape.Envelope.YMax + fea.Shape.Envelope.YMin) / 2; 
                        //原来是竖排，改为横排
                        if ((pTextSymbol.Angle == 270||pTextSymbol.Angle==-90) && cjkrotation)
                        {
                            VerToHor(fea);
                            double newCenterx = (fea.Shape.Envelope.XMax + fea.Shape.Envelope.XMin) / 2;
                            double newCentery = (fea.Shape.Envelope.YMax + fea.Shape.Envelope.YMin) / 2;
                            AnnoMove(fea, oldCenterx - newCenterx, oldCentery - newCentery);
                        }
                            //原来是横排，改为竖排
                        else
                        {
                            HorToVer(fea);
                            double newCenterx = (fea.Shape.Envelope.XMax + fea.Shape.Envelope.XMin) / 2;
                            double newCentery = (fea.Shape.Envelope.YMax + fea.Shape.Envelope.YMin) / 2;
                            AnnoMove(fea, oldCenterx - newCenterx, oldCentery - newCentery);
                        }

                    }
                        
                    }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                msg = ex.Message;
            }

            return msg;
        }
     
        private void AnnoMove(IFeature fea,double dx,double dy)
        {
             IAnnotationFeature annofea = fea as IAnnotationFeature;
             ITextElement pTextElement = annofea.Annotation as ITextElement;
             ITransform2D ptrans = annofea.Annotation.Geometry as ITransform2D;
             ptrans.Move(dx, dy);
             IElement pElement = pTextElement as IElement;
             pElement.Geometry = ptrans as IGeometry;
             annofea.Annotation = pElement;
             fea.Store();
        }
        //原来是竖排，改为横排
        private void VerToHor(IFeature fea)
        {
            IAnnotationFeature annofea = fea as IAnnotationFeature;
            if (annofea.Annotation == null)return;
            IFormattedTextSymbol pTextSymbol = ((annofea.Annotation as ITextElement).Symbol) as IFormattedTextSymbol;    
            pTextSymbol.Angle = 0;
            ITextElement pTextElement = annofea.Annotation as ITextElement;
            pTextElement.Symbol = pTextSymbol;
            ICharacterOrientation pCharacterOrientation = null;
            pCharacterOrientation = pTextElement.Symbol as ICharacterOrientation;
            pCharacterOrientation.CJKCharactersRotation = false;
            pTextElement.Symbol = pCharacterOrientation as ITextSymbol;
            IElement pElement = pTextElement as IElement;
            annofea.Annotation = pElement;
            fea.Store();          
                      
        }
        private void HorToVer(IFeature fea)
        {
            IAnnotationFeature annofea = fea as IAnnotationFeature;
            if (annofea.Annotation == null) return;
            IFormattedTextSymbol pTextSymbol = ((annofea.Annotation as ITextElement).Symbol) as IFormattedTextSymbol;       
            pTextSymbol.Angle = 270;
            ITextElement pTextElement = annofea.Annotation as ITextElement;
            pTextElement.Symbol = pTextSymbol;
            ICharacterOrientation pCharacterOrientation = null;
            pCharacterOrientation = pTextElement.Symbol as ICharacterOrientation;
            pCharacterOrientation.CJKCharactersRotation = true;
            pTextElement.Symbol = pCharacterOrientation as ITextSymbol;
            IElement pElement = pTextElement as IElement;
            annofea.Annotation = pElement;
            fea.Store();
        }
    }
}
