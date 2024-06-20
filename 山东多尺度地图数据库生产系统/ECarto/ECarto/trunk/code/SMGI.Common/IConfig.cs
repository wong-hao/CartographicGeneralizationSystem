using System;
namespace SMGI.Common
{
    public interface IConfig
    {
        object this[string key] { get; set; }
        string GetOriginValue(string key);
        void SetOriginValue(string key,string value);
    }
}
