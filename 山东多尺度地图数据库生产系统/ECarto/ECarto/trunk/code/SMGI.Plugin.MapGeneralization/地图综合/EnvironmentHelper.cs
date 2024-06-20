using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SMGI.Common;
using ESRI.ArcGIS.Geoprocessor;

namespace SMGI.Plugin.MapGeneralization
{
    class EnvironmentHelper
    {
        GApplication app;
        public string TPXElement
        {
            get
            {
                object obj = app.AppConfig["YJ_TPXElement"];
                if (obj == null)
                {
                    return "TP10W";
                }
                else
                {
                    return obj.ToString();
                }
            }
            set
            {
                app.AppConfig["YJ_TPXElement"] = value;
            }
        }
        public int MapScale {
            get {
                object obj = app.AppConfig["YJ_MapScale"];
                if (obj==null|| Convert.ToInt32(obj)==0)
                {
                    return 100000;
                }
                else
                {
                    return Convert.ToInt32(obj);
                }
            }
            set
            {
                app.AppConfig["YJ_MapScale"] = value; 
            }
        }

        public string LastGDBPath
        {
            get
            {
                object obj = app.AppConfig["YJ_LastGDBPath"];
                if (obj == null)
                {
                    return "";
                }
                else
                {
                    return obj.ToString();
                }
            }
            set
            {
                app.AppConfig["YJ_LastGDBPath"] = value;
            }
        }

        public EnvironmentHelper(GApplication application)
        {
            app = application;
        }

        public static string ReturnGPMessages(Geoprocessor gp)
        {
            string msg = "";
            if (gp.MessageCount > 0)
            {
                for (int i = 0; i <= gp.MessageCount - 1; i++)
                {
                    msg+=gp.GetMessage(i);
                }
            }
            return msg;
        }
    }
}
