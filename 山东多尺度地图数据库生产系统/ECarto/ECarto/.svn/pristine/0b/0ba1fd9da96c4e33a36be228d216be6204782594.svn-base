using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using System.Runtime.Serialization;
using System.Collections;

namespace SMGI.Common
{
    [Serializable]
    public class SymbolInfo:ISerializable
    {
        public SymbolInfo(SerializationInfo info, StreamingContext context)
        {
            SymbolGalleryItem = GConvert.Base64ToObject(info.GetString("symbol")) as IStyleGalleryItem3;
        }
        public SymbolInfo(IStyleGalleryItem3 SyItem)
        {
            SymbolGalleryItem = SyItem;
        }
        public SymbolInfo()
        {
        }
        public IStyleGalleryItem3 SymbolGalleryItem
        {
            get;
            set;
        }
        public string Name { 
            get
            {
                return SymbolGalleryItem.Name;
            }
        }
        public string Category
        {
            get { return SymbolGalleryItem.Category;}
        }
        public ISymbol Symbol
        {
            get { return SymbolGalleryItem.Item as ISymbol; }
        }
        public Guid Guid
        {
            get { return new Guid(SymbolGalleryItem.Tags); }
            set
            {
                SymbolGalleryItem.Tags = value.ToString();
            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("symbol", GConvert.ObjectToBase64(SymbolGalleryItem)); ;
        }
    }

    public class SymbolManager
    {
        Dictionary<Guid, SymbolInfo> infos;
        GApplication gapp;
        public SymbolManager(GApplication app)
        {
            gapp = app;
        }

        public Dictionary<Guid, SymbolInfo> ReadConfig(ISymbol pSymbol)
        {
            IList<string> plist;

            if (pSymbol is IMarkerSymbol)
            {
                plist = gapp.AppConfig[this.GetType().ToString() + ".MarkerSymbols"] as List<string>;
                
            }
            else if (pSymbol is ITextSymbol)
            {
                plist = gapp.AppConfig[this.GetType().ToString() + ".TextSymbols"] as List<string>;
            }
            else if (pSymbol is ILineSymbol)
            {
                plist = gapp.AppConfig[this.GetType().ToString() + ".LineSymbols"] as List<string>;
            }
            else if (pSymbol is IFillSymbol)
            {
                plist = gapp.AppConfig[this.GetType().ToString() + ".FillSymbols"] as List<string>;
                //       // this.SetFeatureClassStyle(es, "当前符号", "面");
            }
            else
            {
                plist=new List<string>();
            }
            infos = ListToDict(plist);
            return infos;
        }

        public Dictionary<Guid, SymbolInfo> ListToDict(IList<string> plist)
        {
            SymbolInfo pysm;
            Dictionary<Guid, SymbolInfo> info =new Dictionary<Guid,SymbolInfo>();
            if (plist != null)
            {
                foreach (string s in plist)
                {

                    pysm = GConvert.Base64ToObject(s) as SymbolInfo;
                    if (pysm == null)
                        continue;
                    info.Add(pysm.Guid, pysm);
                }         
            }
            return info;
        }

        public void SaveSymbolInfo(Dictionary<Guid, SymbolInfo> infos, ISymbol pSymbol)
        {
            IList<string> plist=new List<string>();
            foreach (var item in infos)
            {
                plist.Add(GConvert.ObjectToBase64(item.Value));
            }

            if (pSymbol is IMarkerSymbol)
            {
                gapp.AppConfig[this.GetType().ToString() + ".MarkerSymbols"] = plist;

            }
            else if (pSymbol is ITextSymbol)
            {
                gapp.AppConfig[this.GetType().ToString() + ".TextSymbols"] = plist;
            }
            else if (pSymbol is ILineSymbol)
            {
                gapp.AppConfig[this.GetType().ToString() + ".LineSymbols"] = plist;
            }
            else if (pSymbol is IFillSymbol)
            {
                gapp.AppConfig[this.GetType().ToString() + ".FillSymbols"] = plist;
                //       // this.SetFeatureClassStyle(es, "当前符号", "面");
            }
           
        }
        public SymbolInfo GetSymbolInfo(Guid guid)
        {
            SymbolInfo info=null;
            if (guid == null)
                return null;
           IEnumerable eum = infos.Where(q => q.Key == guid).Select(q => q.Key);
           foreach (SymbolInfo i in eum)
           {
               info = i;
           }
           return info;
        }

        public SymbolInfo[] GetSymbolInfo(string name,string categroy)
        {
            var  eum = from q in infos.Values
                              where (q.Name == name && q.Category == categroy)
                              select q;
            //infos.Where(q => q.Value.Name == name && q.Value.Category == categroy).Select(q=>q.Value).ToArray();
            //foreach (KeyValuePair<Guid, SymbolInfo> info in infos)
            //{
            //    if (info.Value.Name == name && info.Value.Category == categroy)
            //    {
                   
            //    }
            //}
            return eum.ToArray();
           // return null;
        }

        public SymbolInfo[] GetSymbolInfo()
        {

            return  infos.Values.ToArray();
        }

        public void AddSymbolInfo(SymbolInfo info) {
            infos.Add(info.Guid, info);
        }

        public void RemoveSymbolInof(Guid guid)
        {
            if (infos.ContainsKey(guid))
            {
                infos.Remove(guid);
            }
        }

        public System.Windows.Forms.DialogResult ShowSymbolDialog(ISymbol symbol)
        {
            return System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
