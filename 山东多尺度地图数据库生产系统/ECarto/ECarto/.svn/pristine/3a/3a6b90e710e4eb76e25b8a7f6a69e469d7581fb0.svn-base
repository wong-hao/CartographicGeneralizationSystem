using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;


namespace SMGI.Common
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class SMGIAutoSaveAttribute : Attribute
    {
        // This is a positional argument
        public SMGIAutoSaveAttribute()
        {
        }
    }

    internal class AutoSaveProperty {
        internal object st;
        internal Config cfg;
        internal AutoSaveProperty(object what, Config config)
        {
            st = what;
            cfg = config;
        }

        internal void Save()
        {
            Type tp = st.GetType();
            var ppInfos = tp.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var info in ppInfos)
            {
                var paInfos = info.GetCustomAttributes(typeof(SMGIAutoSaveAttribute), false);
                if (paInfos != null && paInfos.Length > 0)
                {
                    //MethodInfo getMethod = info.GetGetMethod(true);
                    //if (getMethod == null)
                    //    continue;
                    //object value = getMethod.Invoke(st, null);
                    //cfg[tp.FullName + "." + info.Name] = value;
                    cfg[tp.FullName + "." + info.Name] = info.GetValue(st, null);
                }
            }
        }
        internal void Load() {
            Type tp = st.GetType();
            var ppInfos = tp.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            foreach (var info in ppInfos)
            {
                var paInfos = info.GetCustomAttributes(typeof(SMGIAutoSaveAttribute), false);
                if (paInfos != null && paInfos.Length > 0)
                {
                    try
                    {
                        //object value = cfg[tp.FullName + "." + info.Name];
                        info.SetValue(st, cfg[tp.FullName + "." + info.Name], null);
                    }
                    catch { 
                    }
                }
            }
        }
    }

}
