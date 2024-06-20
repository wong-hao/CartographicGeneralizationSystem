using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.CartographyTools;
using ESRI.ArcGIS.DataManagementTools;
using SMGI.Common;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Plugin.MapGeneralization.Top
{
    public class TopSimplefyClass
    {

        private GApplication app=null;
        public TopSimplefyClass()
        {
            app = GApplication.Application;
        }
        private IFeatureClass CreatePolygonMemoryLayer()
        {
            ISpatialReference sp=app.ActiveView.FocusMap.SpatialReference;
            //设置字段集
            IFields fields = new FieldsClass();
            var fieldsEdit = (IFieldsEdit)fields;
            IField field = new FieldClass();
            var fieldEdit = (IFieldEdit)field;

            //创建主键
            fieldEdit.Name_2 = "FID";//记录线的ID
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            fieldsEdit.AddField(field);

            //创建图形字段
            IGeometryDef geometryDef = new GeometryDefClass();
            var geometryDefEdit = (IGeometryDefEdit)geometryDef;
            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
            geometryDefEdit.SpatialReference_2 = sp;

            field = new FieldClass();
            fieldEdit = (IFieldEdit)field;
            fieldEdit.Name_2 = "Shape";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            fieldEdit.GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(field);

            IWorkspaceFactory wf = new InMemoryWorkspaceFactoryClass();
            var wn = wf.Create("", "MemoryWorkspace", null, 0);
            var na = (IName)wn;
            var fw = (IFeatureWorkspace)(na.Open()); //打开内存空间
            
           // var fw = app.Workspace.EsriWorkspace as IFeatureWorkspace;
            var featureClass = fw.CreateFeatureClass("MW_LineConsPolygon", fields, null, null, esriFeatureType.esriFTSimple, "Shape", "");
            return featureClass;
        }

        private void AddFeToMemoryLayer(IFeatureClass fclLine, IEnumTopologyEdge edges)
        {   
            IFeatureCursor insertcursor = fclLine.Insert(true);
           
            int flag=0;
            edges.Reset();
            ITopologyEdge edge = null;
            while ((edge = edges.Next()) != null)
            {      
                IFeatureBuffer newBuf = fclLine.CreateFeatureBuffer();
                IGeometry pgeo=new PolylineClass();
                edge.QueryGeometry(pgeo);
                IZAware pZAware = (IZAware)pgeo;
                pZAware.ZAware = false;
                newBuf.Shape = pgeo;
                newBuf.set_Value(fclLine.FindField("FID"), flag);
                insertcursor.InsertFeature(newBuf);
                flag++;
            }
           
           
          
                
                  
                
            
        }
        public void SimplyLine(IEnumTopologyEdge edges,double width,double height,WaitOperation wo)
        {
           
            
            try{
                    wo.SetText("创建内存图层");
                    var inFcl = CreatePolygonMemoryLayer();
                   
                    AddFeToMemoryLayer(inFcl,edges);
                 
                    wo.SetText("添加拓扑边内存图层");
                    var gp = new Geoprocessor() { OverwriteOutput = true };
                    gp.SetEnvironmentValue("workspace", app.Workspace.EsriWorkspace.PathName);
                    string outfcl="topline";
                    ESRI.ArcGIS.CartographyTools.SimplifyLine simplifyLineTool = 
                    new ESRI.ArcGIS.CartographyTools.SimplifyLine(inFcl, outfcl, "BEND_SIMPLIFY", width);
                    IGeoProcessorResult geoResult = null;
                    wo.SetText("化简处理...");
                    geoResult = (IGeoProcessorResult)gp.Execute(simplifyLineTool, null);

                    if (geoResult.Status == ESRI.ArcGIS.esriSystem.esriJobStatus.esriJobSucceeded)
                    {
                        wo.SetText("平滑处理...");
                        string outfclSm="toplineSmooth";
                        ESRI.ArcGIS.CartographyTools.SmoothLine smoothLineTool = new ESRI.ArcGIS.CartographyTools.SmoothLine(outfcl, outfclSm, "PAEK", 10);
                        geoResult = (IGeoProcessorResult)gp.Execute(smoothLineTool, null);

                    }
                }
                catch(Exception ex)
                {
                }
             
        }
    }
}
