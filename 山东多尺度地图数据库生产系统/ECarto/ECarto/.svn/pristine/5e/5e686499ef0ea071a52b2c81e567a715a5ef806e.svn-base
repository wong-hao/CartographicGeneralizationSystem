using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.GeoAnalyst;
using System.Xml.Linq;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
namespace SMGI.Plugin.EmergencyMap
{
    public class AddHillShadingCmd: SMGICommand
    {
        public AddHillShadingCmd()
        {
            m_caption = "山体阴影";
        }

        public override bool Enabled
        {
            get
            {
                return m_Application.Workspace != null && m_Application.EngineEditor.EditState == esriEngineEditState.esriEngineStateNotEditing;
            }
        }

        public override void OnClick()
        {
            AddHillShadingForm frm = new AddHillShadingForm();
            frm.StartPosition = FormStartPosition.CenterParent;
            if (frm.ShowDialog() != DialogResult.OK)
                return;
            IRasterLayer demRasterLayer = frm.DEMRasterLayer;

            try
            {
                //1.生成山体阴影失败
                HillShading hs = new HillShading();
                IRaster hillshade = hs.ExtractHillShade(demRasterLayer.Raster, frm.Azimuth, frm.Altitude, frm.InModelShadows, frm.ZFactor);
                if (null == hillshade)
                {
                    MessageBox.Show("生成山体阴影失败！");
                    return;
                }

                //2.添加该山体阴影至输入栅格的后面
                int newLayerIndex = m_Application.MapControl.LayerCount;
                for (int i = 0; i < m_Application.MapControl.LayerCount; ++i)
                {
                    if (demRasterLayer == m_Application.MapControl.get_Layer(i))
                    {
                        newLayerIndex = i + 1;
                        break;
                    }
                }
                IRasterLayer newLayer = new RasterLayerClass();
                newLayer.CreateFromRaster(hillshade);
                newLayer.Name = demRasterLayer.Name + "_山体阴影";
                m_Application.MapControl.AddLayer(newLayer, newLayerIndex);

                //3.设置山体阴影渲染器
                IRasterRenderer renderer = StretchRasterRenderer(newLayer.Raster);
                newLayer.Renderer = renderer;
                m_Application.MapControl.Refresh();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(string.Format("生成山体阴影失败:{0}", ex.Message));
            }
        }


        //设置山体阴影渲染器
        private IRasterRenderer StretchRasterRenderer(IRaster raster)
        {
            //1.创建色带
            IAlgorithmicColorRamp colorRamp = new AlgorithmicColorRampClass();
            colorRamp.Size = 255;
            colorRamp.FromColor = new RgbColorClass() { Red = 0, Blue = 0, Green = 0 };
            colorRamp.ToColor = new RgbColorClass() { Red = 255, Blue = 255, Green = 255 };
            bool bSuccess;
            colorRamp.CreateRamp(out bSuccess);

            //2.创建拉伸渲染器
            IRasterRenderer rasterRenderer = new RasterStretchColorRampRendererClass();
            rasterRenderer.Raster = raster;
            rasterRenderer.Update();
            (rasterRenderer as IRasterStretchColorRampRenderer).BandIndex = 0;
            (rasterRenderer as IRasterStretchColorRampRenderer).ColorRamp = colorRamp;
            (rasterRenderer as IRasterStretch).StretchType = esriRasterStretchTypesEnum.esriRasterStretch_StandardDeviations;
            (rasterRenderer as IRasterStretch).StandardDeviationsParam = 2;

            return rasterRenderer;
        }
        
    }
}
