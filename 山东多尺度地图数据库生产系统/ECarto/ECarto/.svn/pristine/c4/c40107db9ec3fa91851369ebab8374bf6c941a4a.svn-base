using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
//**********************
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;

namespace SMGI.Plugin.EmergencyMap
{
    internal class FieldPropertyDescriptor : PropertyDescriptor
    {
        IRepresentationRules rules;
        //private varibles
        private int wrappedFieldIndex;//PropertyDescriptor描述的字段索引
        private Type netType;//存储字段值.Net类型
        private Type actualType;//存储字段的域.Net类型（当useCVDomain为false时使用）
        private esriFieldType esriType;//存储字段值esri类型
        bool isEditable = true;//
        private IWorkspaceEdit wkspcEdit;
        private ICodedValueDomain cvDomain;//coded value domain
        private bool useCVDomain;//当使用字符串值时为true，使用数值时为false
        private TypeConverter actualValueConverter;//显示域的值
        private TypeConverter cvDomainValDescriptionConverter;//显示域的名称

        //Construction/Destruction
        public FieldPropertyDescriptor(ITable wrappedTable, string fieldName, int fieldIndex) : base(fieldName, null)
        {
            wrappedFieldIndex = fieldIndex;
            IField wrappedField = wrappedTable.Fields.get_Field(fieldIndex);
            esriType = wrappedField.Type;
            isEditable = wrappedField.Editable &&
              (esriType != esriFieldType.esriFieldTypeBlob) &&
              (esriType != esriFieldType.esriFieldTypeRaster) &&
              (esriType != esriFieldType.esriFieldTypeGeometry);
            netType = actualType = EsriFieldTypeToSystemType(wrappedField);
            wkspcEdit = ((IDataset)wrappedTable).Workspace as IWorkspaceEdit;
            var ws = wkspcEdit as IWorkspace;

            if (wrappedTable is IGeoFeatureLayer)
            {
                var gl = wrappedTable as IGeoFeatureLayer;
                if (gl.Renderer is IRepresentationRenderer)
                {
                    var rpc = (gl.Renderer as IRepresentationRenderer).RepresentationClass;
                    if (rpc.RuleIDFieldIndex == fieldIndex)
                    {
                        netType = actualType = typeof(string);
                        rules = rpc.RepresentationRules;
                    }
                }
            }
        }

        //*******************
        /// <summary>
        /// Gets a value indicating whether the field represented by this property 
        /// has a CV domain.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has a CV domain; otherwise, <c>false</c>.
        /// </value>
        public bool HasCVDomain
        {
            get
            {
                return null != cvDomain;
            }
        }

        /// <summary>
        /// Sets a value indicating whether [use CV domain].
        /// </summary>
        /// <value><c>true</c> if [use CV domain]; otherwise, <c>false</c>.</value>
        public bool UseCVDomain
        {
            set
            {
                useCVDomain = value;
                if (value)
                {
                    // We want the property type for this field to be string
                    netType = typeof(string);
                }
                else
                {
                    // Restore the original type
                    netType = actualType;
                }
            }
        }

        #region Public Overrides
        /// <summary>
        /// 为当前property获取typeConverter.
        /// </summary>
        /// <remarks>
        /// 如果字段有CVD(coded value domain)，我们不使用值而改用名称，反之亦然。
        /// </remarks>
        /// <returns>
        /// A <see cref="T:System.ComponentModel.TypeConverter"></see> 
        /// that is used to convert the <see cref="T:System.Type"></see> of this 
        /// property.
        /// </returns>
        /// <PermissionSet>
        /// <IPermission class="System.Security.Permissions.SecurityPermission, 
        /// mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" 
        /// version="1" Flags="UnmanagedCode"/>
        /// </PermissionSet>
        
        public override TypeConverter Converter
        {
            get
            {
                TypeConverter retVal = null;

                if (null != cvDomain)
                {
                    if (useCVDomain)
                    {
                        if (null == cvDomainValDescriptionConverter)
                        {
                            // We want a string converter
                            cvDomainValDescriptionConverter = TypeDescriptor.GetConverter(typeof(string));
                        }

                        retVal = cvDomainValDescriptionConverter;
                    }
                    else
                    {
                        if (null == actualValueConverter)
                        {
                            // We want a converter for the type of this field's actual value
                            actualValueConverter = TypeDescriptor.GetConverter(actualType);
                        }

                        retVal = actualValueConverter;
                    }
                }
                else
                {
                    // This field doesn't have a coded value domain, the base implementation
                    // works fine.
                    retVal = base.Converter;
                }

                return retVal;
            }
        }

       /// <summary>
        /// 是否重置字段值
       /// </summary>
       /// <param name="component">IRow</param>
       /// <returns></returns>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// 获取当前属性的成员类型
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Type"></see> that represents the type of 
        /// component this property is bound to. When the 
        /// <see cref="M:System.ComponentModel.PropertyDescriptor.GetValue(System.Object)"></see> 
        /// or <see cref="M:System.ComponentModel.PropertyDescriptor.SetValue(System.Object,System.Object)"></see> 
        /// methods are invoked, the object specified might be an instance of this type.</returns>
        public override Type ComponentType
        {
            get { return typeof(IRow); }
        }

        /// <summary>
        /// 获取属性row当前成员index的值value
        /// </summary>
        /// <param name="component">IRow</param>
        /// <remarks>
        /// This will return the field value for all fields apart from geometry, raster and Blobs.
        /// These fields will return the string equivalent of the geometry type.
        /// </remarks>
        /// <returns>
        /// The value of a property for a given component. This will be the value of
        /// the field this class instance represents in the IRow passed in the component
        /// parameter.
        /// </returns>
        public override object GetValue(object component)
        {
            object retVal = null;

            IRow givenRow = (IRow)component;
            try
            {
                // Get value
                object value = givenRow.get_Value(wrappedFieldIndex);

                if (rules != null)
                {
                    if (value == null || value == DBNull.Value)
                    {
                        return "未设置";
                    }

                    var id = (int)value;
                    if (id == -1)
                    {
                        return "自由制图表达";
                    }

                    return rules.get_Name((int)value);
                }

                if (value == null || value == DBNull.Value)
                {
                    //if (esriType == esriFieldType.esriFieldTypeDouble ||
                    //    esriType == esriFieldType.esriFieldTypeInteger ||
                    //    esriType == esriFieldType.esriFieldTypeSmallInteger ||
                    //    esriType == esriFieldType.esriFieldTypeSingle ||
                    //    esriType == esriFieldType.esriFieldTypeOID)
                    //{

                    //    return 0;
                    //}
                    //else
                    //{
                    //    return string.Empty;
                    //}

                    return "<空>";
                    
                }

                if ((null != cvDomain) && useCVDomain)
                {
                    value = cvDomain.get_Name(Convert.ToInt32(value));
                }

                switch (esriType)
                {
                    case esriFieldType.esriFieldTypeBlob:
                        retVal = "Blob";
                        break;

                    case esriFieldType.esriFieldTypeGeometry:
                        retVal = GetGeometryTypeAsString(value);
                        break;

                    case esriFieldType.esriFieldTypeRaster:
                        retVal = "Raster";
                        break;

                    default:
                        retVal = value;
                        break;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return retVal;
        }

        /// <summary>
        /// 当前属性是否只读
        /// </summary>
        /// <value></value>
        /// <returns>true if the property is read-only; otherwise, false.</returns>
        public override bool IsReadOnly
        {
            get { return !isEditable; }
        }

        /// <summary>
        /// 获取属性类型
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Type"></see> that represents the type 
        /// of the property.</returns>
        public override Type PropertyType
        {
            get { return netType; }
        }

        /// <summary>
        /// 重置value
        /// </summary>
        /// <param name="component">The component (an IRow) with the property value 
        /// that is to be reset to the default value.</param>
        public override void ResetValue(object component)
        {

        }

        /// <summary>
        /// 设置value
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="component">The component (an IRow) with the property value 
        /// that is to be set.</param>
        /// <param name="value">The new value.</param>
        public override void SetValue(object component, object value)
        {
            IRow givenRow = (IRow)component;

            if (null != cvDomain)
            {
                // This field has a coded value domain
                if (!useCVDomain)
                {
                    // Check value is valid member of the domain
                    if (!((IDomain)cvDomain).MemberOf(value))
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format(
                          "Value {0} is not valid for coded value domain {1}", value.ToString(), ((IDomain)cvDomain).Name));
                        return;
                    }
                }
                else
                {
                    // We need to convert the string value to one of the cv domain values
                    // 循环至找到匹配条件为止
                    bool foundMatch = false;
                    for (int valueCount = 0; valueCount < cvDomain.CodeCount; valueCount++)
                    {
                        if (value.ToString() == cvDomain.get_Name(valueCount))
                        {
                            foundMatch = true;
                            value = valueCount;
                            break;
                        }
                    }

                    // 是否匹配
                    if (!foundMatch)
                    {
                        System.Windows.Forms.MessageBox.Show(string.Format(
                          "Value {0} is not valid for coded value domain {1}", value.ToString(), ((IDomain)cvDomain).Name));
                        return;
                    }
                }
            }
            givenRow.set_Value(wrappedFieldIndex, value);

            //// Start editing if we aren't already editing
            //bool weStartedEditing = false;
            //if (!wkspcEdit.IsBeingEdited())
            //{
            //    //wkspcEdit.StartEditing(false);
            //    weStartedEditing = false;
            //}

            // Store change in an edit operation
            wkspcEdit.StartEditOperation();
            givenRow.Store();
            wkspcEdit.StopEditOperation();

            //// Stop editing if we started here
            //if (weStartedEditing)
            //{
            //    wkspcEdit.StopEditing(true);
            //}

        }

        /// <summary>
        /// 当重写子类时, 指示当前属性值是否保留
        /// </summary>
        /// <param name="component">an IRow with the property to be examined for persistence.
        /// </param>
        /// <returns>
        /// true if the property should be persisted; otherwise, false.
        /// </returns>
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
        #endregion Public Overrides

        #region Private Methods
        /// <summary>
        /// 将Esri类型转为.Net类型
        /// </summary>
        /// <param name="esriType">The ESRI field type to be converted.</param>
        /// <returns>The appropriate .NET type.</returns>
        private Type EsriFieldTypeToSystemType(IField field)
        {
            esriFieldType esriType = field.Type;

            // Does this field have a domain?
            cvDomain = field.Domain as ICodedValueDomain;
            if ((null != cvDomain) && useCVDomain)
            {
                return typeof(string);
            }

            try
            {
                switch (esriType)
                {
                    case esriFieldType.esriFieldTypeBlob:
                        //beyond scope of sample to deal with blob fields
                        return typeof(string);
                    case esriFieldType.esriFieldTypeDate:
                        return typeof(DateTime);
                    case esriFieldType.esriFieldTypeDouble:
                        return typeof(double);
                    case esriFieldType.esriFieldTypeGeometry:
                        return typeof(string);
                    case esriFieldType.esriFieldTypeGlobalID:
                        return typeof(string);
                    case esriFieldType.esriFieldTypeGUID:
                        return typeof(Guid);
                    case esriFieldType.esriFieldTypeInteger:
                        return typeof(Int32);
                    case esriFieldType.esriFieldTypeOID:
                        return typeof(Int32);
                    case esriFieldType.esriFieldTypeRaster:
                        //beyond scope of sample to correctly display rasters
                        return typeof(string);
                    case esriFieldType.esriFieldTypeSingle:
                        return typeof(Single);
                    case esriFieldType.esriFieldTypeSmallInteger:
                        return typeof(Int16);
                    case esriFieldType.esriFieldTypeString:
                        return typeof(string);
                    default:
                        return typeof(string);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return typeof(string);
            }
        }

        /// <summary>
        /// 获取标识GeometryType的字符串
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string equivalent of the geometry type</returns>
        private string GetGeometryTypeAsString(object value)
        {
            string retVal = "";
            IGeometry geometry = value as IGeometry;
            if (geometry != null)
            {
                retVal = geometry.GeometryType.ToString().Substring(12);
            }
            return retVal;
        }
        #endregion Private Methods


    }
}
