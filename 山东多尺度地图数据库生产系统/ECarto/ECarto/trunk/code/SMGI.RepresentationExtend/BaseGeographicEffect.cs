using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using System.Reflection;
using ESRI.ArcGIS.ADF.CATIDs;

namespace SMGI.RepresentationExtend
{
    internal enum GraphicAttributeTypeEnum
    {
        GraphicAttributeAngleType,
        GraphicAttributeBooleanType,
        GraphicAttributeColorType,
        GraphicAttributeDashType,
        GraphicAttributeDoubleType,
        //GraphicAttributeEnumType, 枚举类型请用第二个构造函数
        GraphicAttributeIntegerType,
        GraphicAttributeMarkerType,
        GraphicAttributeSizeType,
        GraphicAttributeTextType
    }
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class SMGIGraphicAttribute : Attribute
    {
        // This is a positional argument
        public SMGIGraphicAttribute(string name, int index, GraphicAttributeTypeEnum graphicAttributeType)
        {
            Name = name;
            Index = index;
            switch (graphicAttributeType)
            {
                case GraphicAttributeTypeEnum.GraphicAttributeAngleType:
                    GraphicAttributeType = new GraphicAttributeAngleTypeClass();
                    break;
                case GraphicAttributeTypeEnum.GraphicAttributeBooleanType:
                    GraphicAttributeType = new GraphicAttributeBooleanTypeClass();
                    break;
                case GraphicAttributeTypeEnum.GraphicAttributeColorType:
                    GraphicAttributeType = new GraphicAttributeColorTypeClass();
                    break;
                case GraphicAttributeTypeEnum.GraphicAttributeDashType:
                    GraphicAttributeType = new GraphicAttributeDashTypeClass();
                    break;
                case GraphicAttributeTypeEnum.GraphicAttributeDoubleType:
                    GraphicAttributeType = new GraphicAttributeDoubleTypeClass();
                    break;               
                case GraphicAttributeTypeEnum.GraphicAttributeIntegerType:
                    GraphicAttributeType = new GraphicAttributeIntegerTypeClass();
                    break;
                case GraphicAttributeTypeEnum.GraphicAttributeMarkerType:
                    GraphicAttributeType = new GraphicAttributeMarkerTypeClass();
                    break;
                case GraphicAttributeTypeEnum.GraphicAttributeSizeType:
                    GraphicAttributeType = new GraphicAttributeSizeTypeClass(); 
                    break;
                case GraphicAttributeTypeEnum.GraphicAttributeTextType:
                    GraphicAttributeType = new GraphicAttributeTextTypeClass();
                    break;
                default:
                    break;
            }
           
            
        }
        public SMGIGraphicAttribute(string name, int index, Type enumType)
        {            
            Name = name;
            Index = index;
            var gt = new GraphicAttributeEnumTypeClass();
            foreach (var item in Enum.GetValues(enumType))
            {
                gt.AddValue(Convert.ToInt32(item), Enum.GetName(enumType, item));
            }
            GraphicAttributeType = gt;            
        }

        public string Name { get; set; }
        public int Index { get; set; }
        public IGraphicAttributeType GraphicAttributeType { get; set; }
    }
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class SMGIClassNameAttribute : Attribute
    {
        // This is a positional argument
        public SMGIClassNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
    public abstract class BaseGeographicEffect: IGraphicAttributes, IPersistVariant
    {
        #region private
        class Info
        {
            internal PropertyInfo PropertyInfo { get; set; }
            internal SMGIGraphicAttribute Attribute { get; set; }
            internal int Index { get; set; }
        }
        static Dictionary<Type, Info[]> Reflections;
        Info[] GetGraphicAttributeProperty()
        {
            if (Reflections == null)
                Reflections = new Dictionary<Type, Info[]>();

            var t = this.GetType();
            if (Reflections.ContainsKey(t))
            {
                return Reflections[t];
            }
            var ppInfos = t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            List<Info> pis = new List<Info>();
            foreach (var info in ppInfos)
            {
                var paInfos = info.GetCustomAttributes(typeof(SMGIGraphicAttribute), false);
                if (paInfos != null && paInfos.Length > 0)
                {
                    pis.Add(new Info
                    {
                        PropertyInfo = info,
                        Attribute = paInfos[0] as SMGIGraphicAttribute,
                        Index = (paInfos[0] as SMGIGraphicAttribute).Index
                    });
                }
            }
            pis.Sort((item1,item2) =>
            {
                return item1.Attribute.Index.CompareTo(item2.Attribute.Index);
            });
            for (int i = 0; i < pis.Count; i++)
            {
                pis[i].Index = i;
            }
            var v = pis.ToArray();
            Reflections.Add(t, v);
            return v;
        }
        #endregion

        #region Registration Helper

        public static void GeometricEffectRegistration(Type registerType)
        {
            string regKey = String.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            GeometricEffect.Register(regKey);
        }
        public static void GeometricEffectUnregistration(Type registerType)
        {
            string regKey = String.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            GeometricEffect.Unregister(regKey);
        }

        public static void MarkerPlacementRegistration(Type registerType)
        {
            string regKey = String.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MarkerPlacement.Register(regKey);
        }
        public static void MarkerPlacementUnregistration(Type registerType)
        {
            string regKey = String.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MarkerPlacement.Unregister(regKey);
        }
        #endregion

        #region IGraphicAttributes
        public string ClassName
        {
            get {
                var t = this.GetType();
                var atts = t.GetCustomAttributes(typeof(SMGIClassNameAttribute), false);
                if (atts != null && atts.Length > 0)
                {
                    return "国三制图院 " + (atts[0] as SMGIClassNameAttribute).Name;
                }
                else
                {
                    return t.FullName;
                }
            }
        }

        public int GraphicAttributeCount
        {
            get {
                return GetGraphicAttributeProperty().Length;
            }
        }

 
        public int get_ID(int attrIndex)
        {
            return GetGraphicAttributeProperty()[attrIndex].Attribute.Index;
        }

        public int get_IDByName(string Name)
        {
            var pas = GetGraphicAttributeProperty();
            foreach (var item in pas)
            {
                if (item.Attribute.Name == Name)
                {
                    return item.Attribute.Index;
                }
            }
            return -1;
        }

        public string get_Name(int attrId)
        {
            var pas = GetGraphicAttributeProperty();
            foreach (var item in pas)
            {
                if (item.Attribute.Index == attrId)
                {
                    return item.Attribute.Name;
                }
            }
            return null;
        }

        public IGraphicAttributeType get_Type(int attrId)
        {
            var pas = GetGraphicAttributeProperty();
            foreach (var item in pas)
            {
                if (item.Attribute.Index == attrId)
                {
                    return item.Attribute.GraphicAttributeType;
                }
            }
            return null;
        }

        public object get_Value(int attrId)
        {
            var pas = GetGraphicAttributeProperty();
            foreach (var item in pas)
            {
                if (item.Attribute.Index == attrId)
                {
                    return item.PropertyInfo.GetValue(this, null);
                }
            }
            return null;
        }

        public void set_Value(int attrId, object val)
        {
            var pas = GetGraphicAttributeProperty();

            foreach (var item in pas)
            {
                if (item.Attribute.Index == attrId)
                {
                    item.PropertyInfo.SetValue(this, val, null);
                }
            }            
        }
        #endregion

        #region IPersistVariant
        public UID ID
        {
            get {
                UID pUID;
                pUID = new UID();
                pUID.Value = "{" + this.GetType().GUID.ToString() + "}";
                return pUID;
            }
        }

        public void Load(IVariantStream Stream)
        {
            var aps = GetGraphicAttributeProperty();
            foreach (var item in aps)
            {
                var p = item.PropertyInfo;
                var v = Stream.Read();
                p.SetValue(this, v, null);
            }
        }

        public void Save(IVariantStream Stream)
        {
            var aps = GetGraphicAttributeProperty();
            foreach (var item in aps)
            {
                var p = item.PropertyInfo;
                var v = p.GetValue(this, null);
                Stream.Write(v);
            }
        }
        #endregion
    }
}
