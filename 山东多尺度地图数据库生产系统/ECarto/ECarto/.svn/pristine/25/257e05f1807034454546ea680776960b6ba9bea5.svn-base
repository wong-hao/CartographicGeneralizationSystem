using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.CATIDs;
using System.Runtime.InteropServices;

namespace SMGI.RepresentationExtend
{
    [Guid("203e8417-1750-4ace-b485-e7e66ffd8e45")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("SMGI.RepresentationExtend.ElecPowerLineGeoEffect")]
    public class ElecPowerLineGeoEffect : IGraphicAttributes, IPersistVariant, IMarkerPlacement
    {        
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            ArcGISCategoryRegistration(registerType);
        }
        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            ArcGISCategoryUnregistration(registerType);
        }
        #endregion

        #region Component Category Registration
        static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = String.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MarkerPlacement.Register(regKey);
        }
        static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = String.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MarkerPlacement.Unregister(regKey);
        }
        #endregion


        bool IsOtherVertex
        {
            get;
            set;
        }

        bool BeginVertexVisible
        {
            get;
            set;
        }

        bool EndVertexVisible
        {
            get;
            set;
        }

        double Step
        {
            get;
            set;
        }

        public ElecPowerLineGeoEffect()
        {
            IsOtherVertex = true;
            EndVertexVisible = true;
            BeginVertexVisible = true;
            Step = 14*2.83;
        }


        #region zhouqi
        IEnumerator<IAffineTransformation2D> trans;

        IAffineTransformation2D IMarkerPlacement.NextTransformation()
        {
            if (trans!=null && trans.MoveNext())
            {
                return trans.Current;
            }
            return null;
        }

        IEnumerator<IAffineTransformation2D> GetNextTransformation(IPolyline line)
        {
            //line.Densify(Step,0);
            var trans = new AffineTransformation2DClass();
            var segs = line as ISegmentCollection;
            for (var i = 0; i < segs.SegmentCount; i++)
            {
                ISegment seg = segs.get_Segment(i);
                double dx = seg.ToPoint.X - seg.FromPoint.X;
                double dy = seg.ToPoint.Y - seg.FromPoint.Y;
                if ((dx * dx + dy * dy)<130)continue;
                if (i != 0 || BeginVertexVisible)
                {
                    double r = Math.Atan2(dy , dx);
                    trans.Reset();
                    trans.Rotate(r);
                    trans.Move(seg.FromPoint.X,seg.FromPoint.Y);
                    yield return trans;
                }
                if (i != segs.SegmentCount-1 || EndVertexVisible)
                {
                    double r = Math.Atan2(-dy, -dx);
                    trans.Reset();
                    trans.Rotate(r);
                    trans.Move(seg.ToPoint.X, seg.ToPoint.Y);
                    yield return trans;
                }
            }
        }

        void IMarkerPlacement.Reset(IGeometry geom)
        {
            if (geom is IPolyline)
            {
                trans = GetNextTransformation(geom as IPolyline);
            }
            else
            {
                trans = null;
            }
        }

        bool IMarkerPlacement.get_AcceptGeometryType(esriGeometryType inputType)
        {
            if (inputType==esriGeometryType.esriGeometryPolyline)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IGraphicAttributes Members
        string IGraphicAttributes.ClassName
        {
            get { return "国三制图院 电力线"; }
        }

        int IGraphicAttributes.GraphicAttributeCount
        {
            get { return 3; }
        }

        int IGraphicAttributes.get_ID(int attrIndex)
        {
            if (attrIndex >= 0 & attrIndex < 3)
            {
                return attrIndex;
            }
            return -1;
        }

        int IGraphicAttributes.get_IDByName(string Name)
        {
            if (Name == "间隔")
            {
                return 0;
            }

            if (Name == "起点")
            {
                return 1;
            }

            if (Name == "终点")
            {
                return 2;
            }
            return -1;
        }

        string IGraphicAttributes.get_Name(int attrId)
        {
            if (attrId == 0)
                return "间隔";
            if (attrId == 1)
                return "起点";
            if (attrId == 2)
                return "终点";
            return "";
        }

        IGraphicAttributeType IGraphicAttributes.get_Type(int attrId)
        {
            if (attrId == 0)
            {
                GraphicAttributeSizeTypeClass gaSizeType = new GraphicAttributeSizeTypeClass();
                return gaSizeType;
            }
            if (attrId == 1)
            {
                GraphicAttributeBooleanTypeClass gaBooleanType = new GraphicAttributeBooleanTypeClass();
                return gaBooleanType;
            }
            if (attrId == 2)
            {
                GraphicAttributeBooleanTypeClass gaBooleanType = new GraphicAttributeBooleanTypeClass();
                return gaBooleanType;
            }
            return null;
        }

        object IGraphicAttributes.get_Value(int attrId)
        {
            if (attrId == 0)
            {
                return Step;
            }
            if (attrId == 1)
            {
                return BeginVertexVisible;
            }
            if (attrId == 2)
            {
                return EndVertexVisible;
            }
            return 0;
        }

        void IGraphicAttributes.set_Value(int attrId, object val)
        {
            if (attrId == 0)
            {
                Step = (double)val;
            }
            if (attrId == 1)
            {
                BeginVertexVisible = (bool)val;
            }
            if (attrId == 2)
            {
                EndVertexVisible = (bool)val;
            }
        }
        #endregion

        #region IPersistVariant Members

        UID IPersistVariant.ID
        {
            get
            {
                UID pUID;
                pUID = new UID();
                pUID.Value = "{"+this.GetType().GUID.ToString()+"}";
                return pUID;
            }
        }

        void IPersistVariant.Load(IVariantStream Stream)
        {
            int version ;
            version = (int)Stream.Read();
            Step = (double)Stream.Read();
            BeginVertexVisible = (bool)Stream.Read();
            EndVertexVisible = (bool)Stream.Read();
        }

        void IPersistVariant.Save(IVariantStream Stream)
        {
            int version;
            version = 1;
            Stream.Write(version);
            Stream.Write(Step);
            Stream.Write(BeginVertexVisible);
            Stream.Write(EndVertexVisible);
        }
        #endregion
    }
}
