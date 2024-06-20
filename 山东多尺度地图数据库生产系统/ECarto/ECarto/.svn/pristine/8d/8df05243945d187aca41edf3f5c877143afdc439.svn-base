using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
namespace SMGI.Common
{
    class CommonTemplateClass:ITemplateClass
    {
        public CommonTemplateClass()
        { 
        }

        public string Name
        {
            get { return "通用模板"; }
        }

        public bool IsValid(GTemplate t)
        {
            return true;
        }

        public bool IsValidWorkspace(ESRI.ArcGIS.Geodatabase.IWorkspace w)
        {
            return true;
        }

        public void MatchCurrentWorkspace(GTemplate t)
        {
            //donothing
        }

        public string[] Products
        {
            get
            {
                return new string[]{
                    "ECarto"
                };
            }
        }
    }
}
