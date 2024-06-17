using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace DatabaseUpdate
{
  internal class DomapRegistry {
    RegistryKey r;
    internal DomapRegistry(string CollectionName) {
      r = Registry.LocalMachine;
      r = open(r, "SOFTWARE");
      r = open(r, "Domap");
      r = open(r, "GZ-UMGS");
      r = open(r, "DatabaseUpdate");
      r = open(r, CollectionName);
    }

    internal void Save(string key, object value) {
      r.SetValue(key, value);
    }
    internal object Load(string key) {
      return r.GetValue(key, null);
    }

    RegistryKey open(RegistryKey p, string n) {
      RegistryKey r = p.OpenSubKey(n, true);
      if (r == null) {
        p.CreateSubKey(n);
        r = p.OpenSubKey(n, true);
      }
      return r;
    }
  }
}
