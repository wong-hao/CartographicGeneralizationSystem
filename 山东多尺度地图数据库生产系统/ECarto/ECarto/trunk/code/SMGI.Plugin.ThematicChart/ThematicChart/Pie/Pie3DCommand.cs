﻿using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.ADF.BaseClasses;
using System.Windows.Forms;
using SMGI.Common;
using System.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
namespace SMGI.Plugin.ThematicChart.ThematicChart
{
    //多系列3D饼图
    public class Pie3DCommand :SMGITool
    {
        private IActiveView pAc;
        private IFeatureLayer pTableLayer=null;//添加自由表达图层
        private double mapScale;
        public Pie3DCommand()
        {
        }

        public override bool Enabled
        {
            get
            {

                return m_Application != null && m_Application.EngineEditor.EditState != esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            pAc = m_Application.ActiveView;
            mapScale = (m_Application.ActiveView as IMap).ReferenceScale;
           
        }
        public override void OnMouseDown(int button, int shift, int x, int y)
        {
            if (button != 1)
                return;
            if (mapScale == 0)
            {
                MessageBox.Show("请先设置参考比例尺！");
                return;

            }
            //锚点
            IPoint anchorPoint = m_Application.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            FrmPieChartsSet frm = new FrmPieChartsSet(anchorPoint,"三维饼图", "饼图");
            frm.Show();
        }
    }
}