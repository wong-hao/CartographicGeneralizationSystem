using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace DomapTool {
  public class GGenPara {
    private static GGenPara para;
    public static GGenPara Para {
      get {
        if (para == null) {
          LoadPara();
          return para;
        }
        else
          return para;
      }
    }
    internal Dictionary<string, object> defaultPara;
    internal Dictionary<string, object> data;
    
    internal GGenPara() {
      data = new Dictionary<string, object>();
      defaultPara = new Dictionary<string, object>();
    }
    internal byte[] Save() {
      return WriteToBinary(data);
    }
    internal void Load(byte[] dd) {
      object obj = ReadFromBinary(dd);
      if (obj is Dictionary<string, object>) {
        this.data = obj as Dictionary<string, object>;
        Sync();
      }
    }
    internal string SaveToString() {
      return Convert.ToBase64String(Save());
    }
    internal void LoadFromString(string v) {
      try {
        Load(Convert.FromBase64String(v));
      }
      catch { }
    }
    internal void Sync() {
      foreach (string key in defaultPara.Keys) {
        if (!data.ContainsKey(key)) {
          data[key] = defaultPara[key];
        }
      }

    }
    public object this[string key] {
      get {
        return data.ContainsKey(key) ? data[key] : (defaultPara.ContainsKey(key) ? defaultPara[key] : null);
      }
      set {
        data[key] = value;
      }
    }

    public bool RegistPara(string name, object defaultValue) {
      if (defaultPara.ContainsKey(name)) {
        return false;
      }
      else {
        defaultPara[name] = defaultValue;
        if (!data.ContainsKey(name)) {
          data[name] = defaultValue;
        }
        return true;
      }
    }

    public static byte[] WriteToBinary(object instance) {
      BinaryFormatter formatter = new BinaryFormatter();
      using (MemoryStream ms = new MemoryStream()) {
        formatter.Serialize(ms, instance);
        return ms.ToArray();
      }
    }
    public static object ReadFromBinary(byte[] data) {
      BinaryFormatter formatter = new BinaryFormatter();
      using (MemoryStream ms = new MemoryStream(data)) {
        return formatter.Deserialize(ms);
      }
    }

    public static void LoadPara() {
      if (para == null) {
        para = new GGenPara();
      }

      DomapRegistry reg = new DomapRegistry("GenPara");
      byte[] obj = reg.Load("Para") as byte[];
      if (obj == null)
        return;

      para.Load(obj);
    }
    public static void SavePara() {
      if (para == null) {
         LoadPara();
      }

      DomapRegistry reg = new DomapRegistry("GenPara");
      byte[] obj = para.Save();
      reg.Save("Para",obj);
    }
  }

}
