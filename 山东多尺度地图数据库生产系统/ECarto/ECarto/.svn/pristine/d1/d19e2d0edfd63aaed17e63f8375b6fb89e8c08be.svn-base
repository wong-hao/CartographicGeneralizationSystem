using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geometry;

namespace SMGI.Common {

    public enum LogType { 
        Information,
        Warning,
        Error
    }


    [Serializable]
    internal class LogInfo {
        public DateTime Time { get; private set; }
        public string Message { get; private set; }
        public LogType Type { get; private set; }
        internal LogInfo(string msg, LogType tp) {            
            Message = msg;
            Type = tp;
            this.Time = DateTime.Now;
        }
    }
    public class LogManager {
        Config config;
        List<LogInfo> infos;
        internal LogManager(GWorkspace workspace) {
            this.config = workspace.MapConfig;
            infos = new List<LogInfo>();
            Load();
        }
        internal void Load()
        {
            infos.Clear();
            LogInfo[] infs = config["_gLog"] as LogInfo[];
            if (infs != null) {
                infos.AddRange(infs);
            }
        }
        internal void Save() {
            LogInfo[] infs = infos.ToArray();
            config["_gLog"] = infs;
        }
        public void Information(string msg) {
            infos.Add(new LogInfo(msg,LogType.Information));
        }
        public void Warning(string msg) {
            infos.Add(new LogInfo(msg, LogType.Warning));
        }
        public void Error(string msg) {
            infos.Add(new LogInfo(msg, LogType.Error));
        }
    }
}
