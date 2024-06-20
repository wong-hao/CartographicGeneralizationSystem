using System;
using System.Collections.Generic;
using System.Text;

namespace BuildingGen {

  public class GGenParaInfo {
    public string Name { get; set; }
    public string Cato { get; set; }
    public string Info { get; set; }
  }

  public class GGenPara {
    internal Dictionary<string, object> defaultPara;
    internal Dictionary<string, object> data;
    internal GGenPara() {
      data = new Dictionary<string, object>();
      defaultPara = new Dictionary<string, object>();
    }
    internal byte[] Save() {
      return GConvert.WriteToBinary(data);
    }
    internal void Load(byte[] dd) {
      object obj = GConvert.ReadFromBinary(dd);
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
  }

}
