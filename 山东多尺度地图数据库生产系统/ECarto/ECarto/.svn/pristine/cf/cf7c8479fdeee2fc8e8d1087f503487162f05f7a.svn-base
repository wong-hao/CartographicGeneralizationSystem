using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMGI.Common
{
    [Serializable]
    public class ParameterInfo
    {
        public string Name;
        public string Category;
        public string Info;
        public object Value;
        public string Key
        {
            get
            {
                return GetKey(Name,Category);
            }
        }
        static public string GetKey(string name, string category)
        {
            return category + "_" + name;
        }
    }

    public class Parameter
    {
        internal Dictionary<string, ParameterInfo> defaultPara;
        internal Dictionary<string, ParameterInfo> data;
        internal Parameter()
        {
            data = new Dictionary<string, ParameterInfo>();
            defaultPara = new Dictionary<string, ParameterInfo>();
        }
        internal byte[] Save()
        {
            return GConvert.WriteToBinary(data);
        }
        internal void Load(byte[] dd)
        {
            object obj = GConvert.ReadFromBinary(dd);
            if (obj is Dictionary<string, ParameterInfo>)
            {
                this.data = obj as Dictionary<string, ParameterInfo>;
                Sync();
            }
        }
        internal string SaveToString()
        {
            return System.Convert.ToBase64String(Save());
        }
        internal void LoadFromString(string v)
        {
            try
            {
                Load(System.Convert.FromBase64String(v));
            }
            catch { }
        }
        internal void Sync()
        {
            foreach (string key in defaultPara.Keys)
            {
                if (!data.ContainsKey(key))
                {
                    data[key] = defaultPara[key];
                }
            }

        }
        //public object this[string key]
        //{
        //    get
        //    {
        //        return data.ContainsKey(key) ? data[key].Value : (defaultPara.ContainsKey(key) ? defaultPara[key].Value : null);
        //    }
        //    set
        //    {
        //        data[key].Value = value;
        //    }
        //}

        public object this[string name,string type]
        {
            get
            {
                string key = ParameterInfo.GetKey(name, type);
                return data.ContainsKey(key) ? data[key].Value : (defaultPara.ContainsKey(key) ? defaultPara[key].Value : null);
            }
            set
            {
                string key = ParameterInfo.GetKey(name, type);
                data[key].Value = value;
            }
        }


        public bool RegistParameter(string name, string category, object defaultValue, string info = "无描述")
        {
            ParameterInfo pinfo = new ParameterInfo();
            pinfo.Name = name;
            pinfo.Category = category;
            pinfo.Info = info;
            pinfo.Value = defaultValue;

            if (defaultPara.ContainsKey(pinfo.Key))
            {
                return false;
            }
            else
            {
                defaultPara[pinfo.Key] = pinfo;
                if (!data.ContainsKey(pinfo.Key))
                {
                    data[pinfo.Key] = pinfo;
                }
                return true;
            }
        }
        
    }



}
