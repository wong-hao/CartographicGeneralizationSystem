using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Windows.Forms;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.esriSystem;
using SMGI.Common;
using stdole;
using System.Drawing;

namespace SMGI.Plugin.EmergencyMap
{
    public class InsertPictureCmd : SMGI.Common.SMGICommand
    {

        public InsertPictureCmd()
        {
            m_caption = "插入图片";
          
        }
        public override bool Enabled
        {
            get
            {
                return m_Application != null &&
                       m_Application.Workspace != null;
            }
        }
        public override void setApplication(GApplication app)
        {
            base.setApplication(app);

        }  

        public override void OnClick()
        {
            if (m_Application.ActiveView.FocusMap.ReferenceScale <= 0)
            {
                MessageBox.Show("请设置当前地图的参考比例尺！");
                return;
            }

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Title = "选择图片文件";
            dlg.AddExtension = true;
            dlg.Filter = "所有支持格式|*.bmp;*.jpg;*.gif;*.tif;*.emf;*.png;*.jp2|bmp 图像|*.bmp|jpeg 图像|*.jpg|gif 图像|*.gif|tiff 图像|*.tif|emf 图像|*.emf|png 图像|*.png|jpeg2000 图像|*.jp2";
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string picFileName = dlg.FileName;

            IElement element = CreatePictureElement(picFileName, (m_Application.ActiveView.Extent as IArea).Centroid, m_Application.ActiveView.FocusMap.ReferenceScale);
            if (element != null)
            {
                m_Application.MapControl.ActiveView.GraphicsContainer.AddElement(element, 0);
                m_Application.MapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, element, m_Application.ActiveView.Extent);
            }

            
        }

        private IElement CreatePictureElement(string picFileName, IPoint centerPt, double scale)
        {
            IPictureElement picElement = null;
            
            try
            {
                string name = System.IO.Path.GetFileNameWithoutExtension(picFileName);
                string extension = System.IO.Path.GetExtension(picFileName).ToLower();//带点的后缀
                switch (extension)
                {
                    case ".bmp":
                        picElement = new BmpPictureElementClass();
                        break;
                    case ".jpg":
                        picElement = new JpgPictureElementClass();
                        break;
                    case ".gif":
                        picElement = new GifPictureElementClass();
                        break;
                    case ".tif":
                        picElement = new TifPictureElementClass();
                        break;
                    case ".emf":
                        picElement = new EmfPictureElementClass();
                        break;
                    case ".png":
                        picElement = new PngPictureElementClass();
                        break;
                    case ".jp2":
                        picElement = new Jp2PictureElementClass();
                        break;
                }
                if (picElement == null)
                    return null;
                picElement.ImportPictureFromFile(picFileName);

                double picWidth = 0, picHeight = 0;
                (picElement as IPictureElement5).QueryIntrinsicSize(ref picWidth, ref picHeight);
                picWidth = picWidth / 2.8345;//磅转毫米
                picHeight = picHeight / 2.8345;//磅转毫米
                picWidth = picWidth * scale * 0.001;//毫米转米
                picHeight = picHeight * scale * 0.001;//毫米转米

                IEnvelope envelope = new EnvelopeClass();
                envelope.PutCoords(centerPt.X - picWidth * 0.5, centerPt.Y - picHeight * 0.5, centerPt.X + picWidth * 0.5, centerPt.Y + picHeight * 0.5);
                
                picElement.MaintainAspectRatio = true;
                picElement.SavePictureInDocument = false;
                (picElement as IElement).Geometry = envelope;
                (picElement as IElementProperties3).Name = name;

                return picElement as IElement;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
                System.Diagnostics.Trace.WriteLine(ex.Source);
                System.Diagnostics.Trace.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);

                return null;
            }
        }
    }
}
