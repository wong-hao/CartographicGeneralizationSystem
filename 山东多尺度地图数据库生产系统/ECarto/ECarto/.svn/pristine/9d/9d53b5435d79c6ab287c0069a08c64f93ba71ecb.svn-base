using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;

namespace SMGI.Plugin.CollaborativeWork
{
    public class MeasureAreaTool : SMGITool
    {
        /// <summary>
        /// 多边形
        /// </summary>
        INewPolygonFeedback _polygonFeedBack;
        /// <summary>
        /// 点集
        /// </summary>
        IPointCollection _areaPointCol;
        /// <summary>
        /// 测量结果显示框
        /// </summary>
        FrmMeasureResult _frmMeasureResult;

        /// <summary>
        /// 等面积投影
        /// </summary>
        IProjectedCoordinateSystem _equalAreaProjCS;

        public MeasureAreaTool()
        {
            m_caption = "测量面积";       
            m_cursor = new System.Windows.Forms.Cursor(GetType().Assembly.GetManifestResourceStream(GetType(), "修线.cur"));

            ISpatialReferenceFactory pSpatialRefFactory = new SpatialReferenceEnvironmentClass();
            _equalAreaProjCS = pSpatialRefFactory.CreateProjectedCoordinateSystem((int)esriSRProjCS4Type.esriSRProjCS_WorldCylindricalEqualArea);
        }

        public override void OnClick()
        {
            _polygonFeedBack = null;
            _areaPointCol = new MultipointClass();
            _frmMeasureResult = null;

            //打开提示框
            showResultForm();
        }

        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
            {
                return;
            }
            //打开提示框
            showResultForm();

            if (null == _polygonFeedBack)
            {
                var dis = m_Application.ActiveView.ScreenDisplay;
                var lineSymbol = new SimpleLineSymbolClass 
                { 
                    Color = new RgbColorClass { Red = 85, Green = 255, Blue = 0 }, 
                    Style = esriSimpleLineStyle.esriSLSSolid, 
                    ROP2 = esriRasterOpCode.esriROPNotXOrPen,
                    Width = 2.0
                };

                _polygonFeedBack = new NewPolygonFeedbackClass { Display = dis, Symbol = lineSymbol as ISymbol };

                _areaPointCol.RemovePoints(0, _areaPointCol.PointCount);

                IPoint pt = ToSnapedMapPoint(x, y);
                _polygonFeedBack.Start(pt);
                _areaPointCol.AddPoint(pt);
            }
            else
            {
                IPoint pt = ToSnapedMapPoint(x, y);
                _polygonFeedBack.AddPoint(pt);
                _areaPointCol.AddPoint(pt);
            }
        }

        public override void OnMouseMove(int button, int shift, int x, int y)
        {
            if (_polygonFeedBack != null)
            {
                IPoint pt = ToSnapedMapPoint(x, y);
                _polygonFeedBack.MoveTo(pt);

                //更新面积信息
                IPointCollection pPoints = new Polygon();
                IPolygon pPolygon = new PolygonClass();
                IGeometry pGeo = null;

                ITopologicalOperator pTopo = null;
                for (int i = 0; i < _areaPointCol.PointCount; ++i)
                {
                    pPoints.AddPoint(_areaPointCol.get_Point(i));
                }
                pPoints.AddPoint(pt);

                if (pPoints.PointCount < 3) return;

                pPolygon = pPoints as IPolygon;
                if (pPolygon != null)
                {
                    pPolygon.Close();

                    pGeo = pPolygon as IGeometry;
                    pTopo = pGeo as ITopologicalOperator;

                    //使几何图形的拓扑正确
                    pTopo.Simplify();
                    pGeo.Project(m_Application.MapControl.SpatialReference);


                    if (m_Application.MapControl.SpatialReference is IGeographicCoordinateSystem)//地理坐标
                    {
                        if (_equalAreaProjCS != null)
                        {
                            pGeo.Project(_equalAreaProjCS);//等面积投影
                            IArea pProjArea = pGeo as IArea;

                            _frmMeasureResult.ResultText = string.Format("{0:.####}平方米", pProjArea.Area);
                        }
                        else
                        {
                            IAreaGeodetic pArea = pGeo as IAreaGeodetic;

                            _frmMeasureResult.ResultText = string.Format("{0:.####}平方米", pArea.get_AreaGeodetic(esriGeodeticType.esriGeodeticTypeGeodesic, null));
                        }
                        
                    }
                    else
                    {
                        IArea pArea = pGeo as IArea;

                        _frmMeasureResult.ResultText = string.Format("{0:.####}平方{1}", pArea.Area, getMapUnti(m_Application.MapControl.Map.MapUnits));
                    }
                    

                    pPolygon = null;
                }

            }
        }

        public override void OnDblClick()
        {
            clear();
        }

        public override bool Deactivate()
        {
            //关闭提示框
            _frmMeasureResult.Close();

            return base.Deactivate();
        }

        public override bool Enabled
        {
            get
            {
                return m_Application != null
                    && m_Application.Workspace != null
                    && m_Application.LayoutState == Common.LayoutState.MapControl;
            }
        }

        /// <summary>
        /// 清空成员
        /// </summary>
        private void clear()
        {
            if (_polygonFeedBack != null)
            {
                _polygonFeedBack.Stop();
                _polygonFeedBack = null;

                _areaPointCol.RemovePoints(0, _areaPointCol.PointCount);

                m_Application.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
            }
            
        }

        /// <summary>
        /// 显示结果框
        /// </summary>
        private void showResultForm()
        {
            if (null == _frmMeasureResult || _frmMeasureResult.IsDisposed)
            {
                _frmMeasureResult = new FrmMeasureResult();
                _frmMeasureResult.frmClosed += new FrmMeasureResult.FrmClosedEventHandler(clear);
                _frmMeasureResult.ResultText = "0";

                _frmMeasureResult.Show();
            }
            else
            {
                _frmMeasureResult.Activate();
            }
        }

        private string getMapUnti(esriUnits unit)
        {
            string result = string.Empty;

            switch (unit)
            {
                case esriUnits.esriCentimeters:
                    {
                        result = "厘米";
                        break;
                    }
                case esriUnits.esriDecimalDegrees:
                    {
                        result = "十进制";
                        break;
                    }
                case esriUnits.esriDecimeters:
                    {
                        result = "分米";
                        break;
                    }
                case esriUnits.esriFeet:
                    {
                        result = "尺";
                        break;
                    }
                case esriUnits.esriInches:
                    {
                        result = "英寸";
                        break;
                    }
                case esriUnits.esriKilometers:
                    {
                        result = "千米";
                        break;
                    }
                case esriUnits.esriMeters:
                    {
                        result = "米";
                        break;
                    }
                case esriUnits.esriMiles:
                    {
                        result = "英里";
                        break;
                    }
                case esriUnits.esriMillimeters:
                    {
                        result = "毫米";
                        break;
                    }
                case esriUnits.esriNauticalMiles:
                    {
                        result = "海里";
                        break;
                    }
                case esriUnits.esriPoints:
                    {
                        result = "点";
                        break;
                    }
                case esriUnits.esriUnitsLast:
                    {
                        result = "UnitsLast";
                        break;
                    }
                case esriUnits.esriUnknownUnits:
                    {
                        result = "未知单位";
                        break;
                    }
                case esriUnits.esriYards:
                    {
                        result = "码";
                        break;
                    }
                default:
                    break;
            }

            return result;
        }
    }
}
