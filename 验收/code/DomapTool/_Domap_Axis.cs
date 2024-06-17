using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using GENERALIZERLib;
using System.IO;
using System.Reflection;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.DataManagementTools;
using Ress.Carto506.LandGen.CenterLineCut;

namespace GPFunctionTool_DomapSimplification
{
    public class _Domap_Axis:IGPFunction
    {
        private string toolname = "domap_HydroAxis";
        private IGPUtilities2 gputilities;
        private IArray m_Parameters;
        private string para_filepath;

        public _Domap_Axis()
        {
            gputilities = new GPUtilitiesClass();

            string dllLcation = Assembly.GetCallingAssembly().Location;
            string dllPath = System.IO.Path.GetDirectoryName(dllLcation);
            if (!dllPath.EndsWith(@"\"))
            {
                dllPath += @"\";
            }
            para_filepath = dllPath + "GeneRules.inf";
        }

        public UID DialogCLSID
        {
            get { return null; }
        }

        public string DisplayName
        {
            get { return "HydroAxis_Domap"; }
        }

        public void Execute(IArray paramvalues, ITrackCancel trackCancel, IGPEnvironmentManager envMgr, IGPMessages message)
        {
            // Get the first Input Parameter
            IGPParameter parameter = (IGPParameter)paramvalues.get_Element(0);
            // UnPackGPValue. This ensures you get the value either form the dataelement or GpVariable (modelbuilder)
            IGPValue parameterValue = gputilities.UnpackGPValue(parameter);
            // Open Input Dataset
            IFeatureClass inputFeatureClass = null;
            IQueryFilter qf = null;
            IGPRecordSet gprs = null;
            IRecordSet2 rs2 = null;

            //IName name_ = gputilities.CreateFeatureClassName(parameterValue.GetAsText());
            //IFeatureClass fc = name_.Open() as IFeatureClass;
            //gputilities.DecodeFeatureLayer(parameterValue, out outputFeatureclass, out qf);
            //gputilities.CreateDataElement(parameter.c

            IFeatureClass outPutFeatureclass = null;
            ICursor SearchCursor = null;
            int sumCount = 0;
            if (parameterValue.DataType is IDEGeoDatasetType)
            {
                gputilities.DecodeFeatureLayer(parameterValue, out inputFeatureClass, out qf);
                if (inputFeatureClass == null)
                {
                    message.AddError(2, "Could not open input dataset.");
                    return;
                }
                SearchCursor = inputFeatureClass.Search(null, false) as ICursor;
                sumCount = inputFeatureClass.FeatureCount(null);
            }
            else if (parameterValue.DataType is IGPFeatureRecordSetLayerType)
            {
                gprs = parameterValue as IGPRecordSet;
                rs2 = gprs.RecordSet as IRecordSet2;
                SearchCursor = rs2.get_Cursor(false);
                sumCount = rs2.Table.RowCount(null);
            }
            /*
             * *Create FeatureClass  输出参数
             * 
             */
            parameter = (IGPParameter)paramvalues.get_Element(1);
            
            parameterValue = gputilities.UnpackGPValue(parameter);
            Geoprocessor gp = new Geoprocessor();
            // Create the new Output Polygon Feature Class
            CreateFeatureclass cfc = new CreateFeatureclass();
            IName name = gputilities.CreateFeatureClassName(parameterValue.GetAsText());
            IDatasetName dsName = name as IDatasetName;
            IFeatureClassName fcName = dsName as IFeatureClassName;
            IFeatureDatasetName fdsName = fcName.FeatureDatasetName as IFeatureDatasetName;

            // Check if output is in a FeatureDataset or not. Set the output path parameter for CreateFeatureClass tool.
            if (fdsName != null)
            {
                cfc.out_path = fdsName;
            }
            else
            {
                cfc.out_path = dsName.WorkspaceName.PathName;
            }
            // Set the output Coordinate System for CreateFeatureClass tool.
            IGPEnvironment env = envMgr.FindEnvironment("outputCoordinateSystem");
            // Same as Input
            if (env.Value.IsEmpty())
            {
                //IGeoDataset ds = inputFeatureClass as IGeoDataset;
                //cfc.spatial_reference = ds.SpatialReference as ISpatialReference3;
            }
            // Use the evnviroment setting
            else
            {
                IGPCoordinateSystem cs = env.Value as IGPCoordinateSystem;
                cfc.spatial_reference = cs.SpatialReference as ISpatialReference3;
            }
            // Remaing properties for Create Feature Class Tool
            cfc.out_name = dsName.Name;
            cfc.geometry_type = "POLYLINE";
            gp.Execute(cfc, null);
            /******创建完毕***************/

            outPutFeatureclass = gputilities.OpenFeatureClassFromString(parameterValue.GetAsText());
            IFeatureCursor newCursor = outPutFeatureclass.Insert(true);
            IFeatureBuffer rowbuffer = null;
            //Set the properties of the Step Progressor
            IStepProgressor pStepPro = (IStepProgressor)trackCancel;
            pStepPro.MinRange = 0;
            pStepPro.MaxRange = sumCount;
            pStepPro.StepValue = (1);
            pStepPro.Message = "数据读取处理中...";
            pStepPro.Position = 0;
            pStepPro.Show();

            IGeometry geometry;    
            IRow newRow = null;
            CenterLineFactory clf = new CenterLineFactory();
            
            while ((newRow = SearchCursor.NextRow()) != null)
            {
                try
                {
                    geometry = (newRow as IFeature).ShapeCopy;
                    CenterLine cl = clf.Create2((IPolygon)geometry);
                    rowbuffer = outPutFeatureclass.CreateFeatureBuffer();
                    rowbuffer.Shape = cl.Line as IGeometry;
                    newCursor.InsertFeature(rowbuffer);
                    pStepPro.Step();
                }
                catch(Exception e)
                {
                    message.AddError(000, e.Message);
                    continue;
                }
            }
            newCursor.Flush();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(newCursor);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(outPutFeatureclass);
            pStepPro.Hide();
            // Release the update cursor to remove the lock on the input data.
            System.Runtime.InteropServices.Marshal.ReleaseComObject(SearchCursor);

        }

        public IName FullName
        {
            get
            {
                IGPFunctionFactory gpfunctionfactory = new Domap_SimplificationFactory();
                return (IName)gpfunctionfactory.GetFunctionName(toolname);
            }
        }

        public object GetRenderer(IGPParameter pParam)
        {
            return null;
        }

        public int HelpContext
        {
            get { return 0; }
        }

        public string HelpFile
        {
            get { return ""; }
        }

        public bool IsLicensed()
        {
            return true;
        }

        public string MetadataFile
        {
            get { return "a.xml"; }
        }

        public string Name
        {
            get { return toolname; }
        }

        public IArray ParameterInfo
        {
            get
            {
                //Array to the hold the parameters	
                IArray parameters = new ArrayClass();

                //Input DataType is GPFeatureLayerType
                IGPParameterEdit3 inputParameter = new GPParameterClass();
                IGPFeatureSchema gpSchema = new GPFeatureSchemaClass();
                inputParameter.DataType = new GPFeatureLayerTypeClass();
                inputParameter.Value = new GPFeatureLayerClass();
                gpSchema.GeometryType = esriGeometryType.esriGeometryPoint;
                gpSchema.GeometryTypeRule = esriGPSchemaGeometryType.esriGPSchemaGeometryAsSpecified;
                gpSchema.FeatureType = esriFeatureType.esriFTSimple;
                gpSchema.FieldsRule = esriGPSchemaFieldsType.esriGPSchemaFieldsAllFIDsOnly;

                // Set Input Parameter properties
                inputParameter.Direction = esriGPParameterDirection.esriGPParameterDirectionInput;
                inputParameter.DisplayName = "Input Features"; //空格，首字母大写
                inputParameter.Name = "input_features";        //小写，连接线
                inputParameter.ParameterType = esriGPParameterType.esriGPParameterTypeRequired;
                //parameter  0
                parameters.Add(inputParameter);

                // Output parameter (Derived) and data type is DEFeatureClass
                IGPParameterEdit3 outputParameter = new GPParameterClass();
                outputParameter.DataType = new DEFeatureClassTypeClass();
                // Value object is DEFeatureClass
                outputParameter.Value = new DEFeatureClassClass();
                // Set output parameter properties
                outputParameter.Direction = esriGPParameterDirection.esriGPParameterDirectionOutput;
                outputParameter.DisplayName = "Output FeatureClass";
                outputParameter.Name = "out_featureclass";
                outputParameter.ParameterType = esriGPParameterType.esriGPParameterTypeRequired;
                // Create a new schema object - schema means the structure or design of the feature class (field information, geometry information, extent)
                IGPFeatureSchema outputSchema = new GPFeatureSchemaClass();
                outputSchema.GeometryType = esriGeometryType.esriGeometryPolyline;
                outputSchema.GeometryTypeRule = esriGPSchemaGeometryType.esriGPSchemaGeometryAsSpecified;
                IGPSchema schema = (IGPSchema)outputSchema;
                // Clone the schema from the dependency. 
                //This means update the output with the same schema as the input feature class (the dependency).                                
                schema.CloneDependency = true;
                schema.GenerateOutputCatalogPath = true;
                // Set the schema on the output because this tool will add an additional field.
                outputParameter.Schema = outputSchema as IGPSchema;
                outputParameter.AddDependency("input_features");
                parameters.Add(outputParameter);

                return parameters;
            }
        }

        public IGPMessages Validate(IArray paramvalues, bool updateValues, IGPEnvironmentManager envMgr)
        {
            if (m_Parameters == null)
                m_Parameters = this.ParameterInfo;

            // Call UpdateParameters(). 
            // Only Call if updatevalues is true.
            if (updateValues == true)
            {
                //UpdateParameters(paramvalues, envMgr);
            }

            // Call InternalValidate (Basic Validation). Are all the required parameters supplied?
            // Are the Values to the parameters the correct data type?
            IGPMessages validateMsgs = gputilities.InternalValidate(m_Parameters, paramvalues, updateValues, true, envMgr);

            // Call UpdateMessages();
            //UpdateMessages(paramvalues, envMgr, validateMsgs);

            // Return the messages
            return validateMsgs;
        }


    }
}
