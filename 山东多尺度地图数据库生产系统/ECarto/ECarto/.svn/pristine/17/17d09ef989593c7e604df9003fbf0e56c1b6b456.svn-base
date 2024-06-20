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
using System.Runtime.InteropServices;
using ESRI.ArcGIS.CartoUI;
using ESRI.ArcGIS.Framework;

namespace SMGI.Plugin.EmergencyMap
{
    public class EditElementPropertyCmd : SMGI.Common.SMGICommand
    {

        public EditElementPropertyCmd()
        {
            m_caption = "图形属性";
          
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
            List<IElement> selElementList = new List<IElement>();
            #region 获取当前选择的图形元素
            IViewManager viewManager = m_Application.ActiveView as IViewManager;
            IEnumElement enumElement = viewManager.ElementSelection as IEnumElement;
            enumElement.Reset();
            IElement ele = null;
            while ((ele = enumElement.Next()) != null)
            {
                selElementList.Add(ele);
            }
            Marshal.ReleaseComObject(enumElement);
            #endregion

            if (selElementList.Count != 1)
            {
                MessageBox.Show("请选择要查看或编辑属性的单个图形元素!");
                return;
            }

            
            IElement selElement = selElementList.First();
            bool res = EditElementProperty(selElement, true);
            if (res)
                (m_Application.ActiveView).PartialRefresh(esriViewDrawPhase.esriViewGraphics, selElement, null);

        }

        public static bool EditElementProperty(IElement ele, bool bHideApplyButton = false)
        {
            IComPropertySheet comPropSheet = null;
            if (ele is IPictureElement)
            {
                comPropSheet = new ComPropertySheet();
                
                //框架
                IPropertyPage framePage = new FramePropertyPageClass();
                comPropSheet.AddPage(framePage);

                //大小和位置 报错？
                //IPropertyPage sizeAndPosPage = new SizeAndPositionPropertyPageClass();
                //comPropSheet.AddPage(sizeAndPosPage);

                //PictureElementPropertyPage

                comPropSheet.ActivePage = 0;
            }
            else if(ele is ITextElement)
            {
                comPropSheet = new ComPropertySheet();

                //文本
                IPropertyPage textPage = new TextElementPropertyPageClass();
                comPropSheet.AddPage(textPage);

                comPropSheet.ActivePage = 0;
            }
            else
            {
                //MarkerElementPropertyPageClass 
                //AreaGraphicPropertyPageClass 
                //FillShapeElementPropertyPageClass 
                //LengthGraphicPropertyPageClass 
                //LineElementPropertyPageClass  

                MessageBox.Show("暂不支持该类型图形元素！");
                return false;
            }

            if (comPropSheet == null)
                return false;

            comPropSheet.Title = "属性";
            comPropSheet.HideApplyButton = bHideApplyButton;
            comPropSheet.AddCategoryID(new UIDClass());

            ISet elementSet = new ESRI.ArcGIS.esriSystem.SetClass();
            elementSet.Add(ele);
            elementSet.Reset();

            return comPropSheet.EditProperties(elementSet, 0);
        }
    }
}
