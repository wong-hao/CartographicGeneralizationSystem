using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Data.OleDb;

namespace SMGI.Common
{
    /// <summary>
    /// 符号类型
    /// </summary>
    public enum symbolType
    {
        FillSymbols, //= "Fill Symbols",
        MarkerSymbols,// = ,
        LineSymbols,//= "Line Symbols",
        TextLables// = "Text Symbols"
    }

    public static class SymbolClassString
    {

        //public const string GDB_TABLE_MARKER = "MARKER SYMBOLS";
        //public const string GDB_TABLE_LINE = "LINE SYMBOLS";
        //public const string GDB_TABLE_AREA = "FILL SYMBOLS";

        public const string FIELD_ID = "ID";
        public const string FIELD_CATEGORY = "CATEGORY";
        public const string FIELD_NAME = "NAME";
        public const string FIELD_OBJECT = "OBJECT";

        #region 符号库静态类型
        public static string 面符号 = "Fill Symbols";
        public static string 点符号 = "Marker Symbols";
        public static string 线符号 = "Line Symbols";
        public static string 普通标注 = "Labels";
        public static string 制图规则 = "Representation Rules";
        public static string Maplex标注 = "Maplex Labels";

        public static string AreaPatches = "Area Patches";
        public static string LinePatches = "Line Patches";
        public static string Backgrounds = "Backgrounds";
        public static string Borders = "Borders";
        public static string ColorRamps = "Color Ramps";
        public static string Colors = "Colors";
        public static string Hatches = "Hatches";
        public static string LegendItems = "Legend Items";
        public static string NorthArrows = "North Arrows";
        public static string SR = "Reference Systems";
        public static string RepMarkers = "Representation Markers";
        public static string ScaleBar = "Scale Bars";
        public static string ScaleText = "Scale Texts";
        public static string Shadows = "Shadows";
        public static string TextSyms = "Text Symbols";
        public static string VectorSetting = "Vectorization Settings";
        #endregion

        public static string SymbolClassStrByGeometryType(esriGeometryType type)
        {
            string s = "";
            switch (type)
            {
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                    s = 线符号;
                    break;
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                    s = 面符号;
                    break;
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                    s = 点符号;
                    break;
            }
            return s;
        }

    }


    /// <summary>
    /// 符号库管理器，支持缓存和多符号库管理
    /// 符号库的完整路径名 + 符号几何类型 为符号存取必须的Key值。
    /// 
    /// </summary>
    public class StyleManager
    {
        public static string BLANKSTYLE = @"\BLANK.ServerStyle";
        public static string DEFAULTSTYLE = @"\符号库.ServerStyle";
        public static string[] SymbolClasses = new string[] {SymbolClassString.点符号, 
                        SymbolClassString.面符号,
                        SymbolClassString.线符号,
                        SymbolClassString.普通标注,
                        SymbolClassString.制图规则,
                        SymbolClassString.Maplex标注 };

        private string s_path = "";
        public string DefaultStylePath
        {
            get { return s_path; }
            set { s_path = value; }
        }
        private IStyleGallery tempStyleGallery;
        public IStyleGallery StyleGallery
        {
            get { return tempStyleGallery; }
        }
        private GApplication app;
        //private Auto4DSymbolForm symForm;
        private static Dictionary<string, Dictionary<string, Dictionary<string, IStyleGalleryItem>>> cacheSymbols =
            new Dictionary<string, Dictionary<string, Dictionary<string, IStyleGalleryItem>>>();

        public StyleManager(GApplication application)
        {
            s_path = GApplication.ExePath + DEFAULTSTYLE;
            this.app = application;
            tempStyleGallery = InitServerStyleGallery(s_path);
            //symForm = new Auto4DSymbolForm(s_path);
        }

        //public IStyleGalleryItem SelectStyleGItem(ESRI.ArcGIS.Controls.esriSymbologyStyleClass stylecls,
        //    ISymbol symbol)
        //{
        //    symForm.AddSymbol(stylecls, symbol);
        //    if (symForm.DialogResult == System.Windows.Forms.DialogResult.OK)
        //    {
        //        return symForm.m_styleGalleryItem;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        ~StyleManager()
        {
            removeStyleGallery(tempStyleGallery);
        }

        public string getLabel(string GBValue)
        {
            string result = "";
            if (app != null)
            {
                CodeInfo code = app.ClassificationAndCode.FromCode(GBValue);
                if (code != null)
                {
                    result = code.Name;
                }
            }

            return result;
        }


        //****
        private IStyleGallery InitServerStyleGallery(string filepath)
        {
            IStyleGallery styleGallery = new ServerStyleGalleryClass();
            try
            {
                IStyleGalleryStorage styleGalleryStorage = styleGallery as IStyleGalleryStorage;
                for (int i = 0; i < styleGalleryStorage.FileCount; i++)
                {
                    styleGalleryStorage.RemoveFile(styleGalleryStorage.get_File(i));
                }
                styleGalleryStorage.AddFile(filepath);
                styleGalleryStorage.TargetFile = filepath;
            }
            catch (Exception ex)
            {

            }
            return styleGallery;
        }
        private byte[] otob(object o)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream rems = new MemoryStream();
            formatter.Serialize(rems, o);
            return rems.GetBuffer();
        }

        public string OpenStyleAccessMDB(string mdbPath)
        {
            string serverstyle = string.Empty;
            try
            {
                string newpath = CopyNewServerStylePath();
                // Connect to Style (using ADO.NET)
                string connection = "Provider=Microsoft.Jet.OleDb.4.0;Data Source=" + mdbPath + ";";
                OleDbConnection oleConnection = new OleDbConnection(connection);
                oleConnection.Open();

                // Style Hardcode Table Names and Fields
                string[] tables = new string[] { 
                    SymbolClassString.点符号, 
                    SymbolClassString.线符号, 
                    SymbolClassString.面符号,
                    SymbolClassString.制图规则
                };
                // Loop For Each Symbol Table in Style
                foreach (string table in tables)
                {
                    // Construct SQL statement
                    string query = "SELECT * FROM [" + table + "] ";

                    // Connect to the Access File and create reader
                    OleDbCommand command = new OleDbCommand(query, oleConnection);
                    OleDbDataReader dataReader = command.ExecuteReader();
                    if (!dataReader.HasRows) { continue; }

                    // Find column indexes
                    int indexId = dataReader.GetOrdinal(SymbolClassString.FIELD_ID);
                    int indexCategory = dataReader.GetOrdinal(SymbolClassString.FIELD_CATEGORY);
                    int indexName = dataReader.GetOrdinal(SymbolClassString.FIELD_NAME);
                    int idxOBJ = dataReader.GetOrdinal(SymbolClassString.FIELD_OBJECT);


                    // Read each row in table
                    while (dataReader.Read())
                    {
                        // Get row values
                        try
                        {
                            int id = dataReader.GetInt32(indexId);
                            string category = dataReader.GetString(indexCategory);
                            string name = dataReader.GetString(indexName);
                            object oo = dataReader.GetValue(idxOBJ);
                            // Get ESRI Symbol

                            IStyleGalleryItem item = new ServerStyleGalleryItemClass();
                            item.Category = category;
                            item.Item = DeserializeStyleItem(oo);
                            item.Name = name;

                            AddStyleItem(newpath, tempStyleGallery, item);
                        }
                        catch { continue; }
                    }
                    // Close Reader
                    dataReader.Close();
                }
                // Close Connection
                oleConnection.Close();
                serverstyle = newpath;
            }
            catch
            {
            }
            return serverstyle;
        }


        private IPersistStream DeserializeStyleItem(object oo)
        {

            IPersistStream s = null;
            try
            {
                System.Array symObj = oo as System.Array;
                byte[] typeArr = new byte[16];
                System.Array.Copy(symObj, 0, typeArr, 0, 16);
                Guid g = new Guid(typeArr);
                Type t = Type.GetTypeFromCLSID(g);
                s = (IPersistStream)Activator.CreateInstance(t);
                byte[] infoArr = new byte[symObj.Length - 16];
                System.Array.Copy(symObj, 16, infoArr, 0, symObj.Length - 16);

                IMemoryBlobStream stream =
                    new MemoryBlobStreamClass();
                (stream as IMemoryBlobStreamVariant).ImportFromVariant(infoArr);
                s.Load(stream as IStream);
            }
            catch
            {

            }
            return s;
        }


        private void AddStylePath(IStyleGallery gallery, string stylePath)
        {
            if (tempStyleGallery == null) { return; }
            try
            {
                IStyleGalleryStorage styleGalleryStorage = gallery as IStyleGalleryStorage;
                bool exist = false;
                for (int i = 0; i < styleGalleryStorage.FileCount; i++)
                {
                    if (styleGalleryStorage.get_File(i) == stylePath)
                    {
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    styleGalleryStorage.AddFile(stylePath);
                }
                if (!cacheSymbols.ContainsKey(stylePath))
                {
                    cacheSymbols[stylePath] = new Dictionary<string, Dictionary<string, IStyleGalleryItem>>();
                    CacheAllItems(stylePath, gallery, SymbolClasses);
                }
                styleGalleryStorage.TargetFile = stylePath;
            }
            catch { }
        }





        public ISymbol getSymbol(string key, string renderType, string renderValue)
        {
            ISymbol tempSymbol = null;
            try
            {
                IStyleGalleryItem it = getStyleItemFromCache(key, renderType, renderValue);
                if (it != null) { tempSymbol = (ISymbol)it.Item; }
                if (tempSymbol == null && tempStyleGallery != null)
                {
                    IStyleGalleryItem tempItem;
                    tempItem = GetStyleItemByValue(key, tempStyleGallery, renderType.ToString(), renderValue);
                    if (tempItem == null)
                    {
                        throw new Exception();
                    }
                    tempSymbol = (tempItem.Item as IClone).Clone() as ISymbol;
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(tempItem);
                }
            }
            catch
            {

            }
            finally
            {
                //removeStyleGallery(tempStyleGallery);
            }
            return tempSymbol;
        }

        public bool setSymbol()
        {
            bool isSuccess = false;

            return isSuccess;
        }


        public bool addRepRule(string filename, IRepresentationRuleItem repRuleItem, string itemName)
        {
            bool result = false;
            if (tempStyleGallery != null)
            {
                IStyleGalleryItem styleItem = new ServerStyleGalleryItemClass();

                styleItem.Item = repRuleItem;
                styleItem.Category = repRuleItem.GeometryType.ToString();
                styleItem.Name = itemName;
                if (getRepRule(filename, itemName) != null)
                {
                    //tempStyleGallery.UpdateItem(styleItem);
                }
                else
                {
                    tempStyleGallery.AddItem(styleItem);
                }
                result = true;
            }
            return result;
        }

        public IRepresentationRuleItem getRepRule(string key, string repValue)
        {
            IRepresentationRuleItem tempRepRuleItem = null;
            try
            {
                if (tempStyleGallery != null)
                {
                    IStyleGalleryItem tempItem;

                    tempItem = GetStyleItemByValue(key, tempStyleGallery, "Representation Rules", repValue);
                    if (tempItem == null)
                    {
                        throw new Exception();
                    }
                    tempRepRuleItem = (tempItem.Item as IClone).Clone() as IRepresentationRuleItem;
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(tempItem);
                }
            }
            catch
            {

            }
            finally
            {
                //removeStyleGallery(tempStyleGallery);
            }
            return tempRepRuleItem;
        }

        public void AddStyleItem(
            string filePath,
            IStyleGallery styleGallery,
            IStyleGalleryItem item)
        {
            try
            {
                if (styleGallery != null && item != null)
                {
                    AddStylePath(styleGallery, filePath);
                    styleGallery.AddItem(item);
                }
            }
            catch (Exception exx)
            {

            }
            finally
            {
                CacheAllItems(filePath, styleGallery, SymbolClasses);
            }
        }


        public void UpdateStyleItem(
            string filePath,
            IStyleGallery styleGallery,
            IStyleGalleryItem item)
        {
            try
            {
                if (styleGallery != null && item != null)
                {
                    AddStylePath(styleGallery, filePath);
                    styleGallery.UpdateItem(item);
                }
            }
            catch (Exception exx)
            {

            }
        }


        public void DeleteStyleItem(
            string filePath,
            IStyleGallery styleGallery,
            IStyleGalleryItem item)
        {
            try
            {
                if (styleGallery != null && item != null)
                {
                    AddStylePath(styleGallery, filePath);
                    styleGallery.RemoveItem(item);
                }
            }
            catch (Exception exx)
            {
            }
        }



        /// <summary>
        /// 从缓存中读取,并保证相应的键值不为空
        /// </summary>
        /// <param name="key"></param>
        /// <param name="renderType"></param>
        /// <param name="renderValue"></param>
        /// <returns></returns>
        public IStyleGalleryItem getStyleItemFromCache(string key, string renderType, string renderValue)
        {
            IStyleGalleryItem item = null;
            AddStylePath(tempStyleGallery, key);

            if (StyleManager.cacheSymbols.ContainsKey(key))
            {
                if (cacheSymbols[key][renderType] != null)
                {
                    item = cacheSymbols[key][renderType][renderValue];
                }
                else
                {
                    cacheSymbols[key][renderType] = new Dictionary<string, IStyleGalleryItem>();
                }
            }
            return item == null ? null : (item as IClone).Clone() as IStyleGalleryItem;
        }


        /// <summary>
        /// No Cache,不读缓存，读符号库
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="styleGallery"></param>
        /// <param name="SymType"></param>
        /// <param name="sValue"></param>
        /// <returns></returns>
        public IStyleGalleryItem GetStyleItemByValue(
            string filePath,
            IStyleGallery styleGallery,
            string SymType,
            string sValue)
        {

            IEnumStyleGalleryItem enumStyleItem = null;
            IStyleGalleryItem tempStyleItem = null;
            IStyleGalleryItem resultItem = null;
            try
            {
                AddStylePath(styleGallery, filePath);
                enumStyleItem = styleGallery.get_Items(SymType, filePath, "");
                enumStyleItem.Reset();
                while ((tempStyleItem = enumStyleItem.Next()) != null)
                {
                    //match successfully
                    if (tempStyleItem.Name == sValue)
                    {
                        resultItem = tempStyleItem;
                        break;
                    }
                }
            }
            catch (Exception exx)
            {
                System.Diagnostics.Debug.WriteLine(exx.Message);
            }
            finally
            {
                if (enumStyleItem != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(enumStyleItem);
                }
            }
            return resultItem;
        }


        /// <summary>
        /// 从缓存中取回所有符号
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="styleGallery"></param>
        /// <param name="SymType"></param>
        /// <param name="forceLoad">  是否强行加载符号库并置入缓存 </param>
        /// <returns></returns>
        public IStyleGalleryItem[] GetAllStyleItems(
            string filePath,
            IStyleGallery styleGallery,
            string SymType, bool forceLoad = false)
        {
            List<IStyleGalleryItem> items = new List<IStyleGalleryItem>();
            IEnumStyleGalleryItem enumStyleItem = null;
            IStyleGalleryItem tempStyleItem = null;
            try
            {
                AddStylePath(styleGallery, filePath);
                if (!StyleManager.cacheSymbols[filePath].ContainsKey(SymType))
                {
                    StyleManager.cacheSymbols[filePath].Add(SymType, new Dictionary<string, IStyleGalleryItem>());
                }
                if (!forceLoad)
                {
                    items.AddRange(StyleManager.cacheSymbols[filePath][SymType].Values.ToArray());
                }
                else
                {
                    StyleManager.cacheSymbols[filePath][SymType].Clear();
                    enumStyleItem = styleGallery.get_Items(SymType, filePath, "");
                    enumStyleItem.Reset();
                    while ((tempStyleItem = enumStyleItem.Next()) != null)
                    {

                        if (StyleManager.cacheSymbols[filePath].ContainsKey(SymType) &&
                            !StyleManager.cacheSymbols[filePath][SymType].ContainsKey(tempStyleItem.Name))
                        {
                            StyleManager.cacheSymbols[filePath][SymType].Add(tempStyleItem.Name,
                                (tempStyleItem as IClone).Clone() as IStyleGalleryItem);
                            items.Add((tempStyleItem as IClone).Clone() as IStyleGalleryItem);
                        }
                    }
                }
            }
            catch (Exception exx)
            {
                System.Diagnostics.Debug.WriteLine(exx.Message);
            }
            finally
            {
                if (enumStyleItem != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(enumStyleItem);
                }
            }
            return items.ToArray();
        }


        private void CacheAllItems(
            string filePath,
            IStyleGallery styleGallery,
            string[] SymTypes)
        {
            IEnumStyleGalleryItem enumStyleItem = null;
            IStyleGalleryItem tempStyleItem = null;
            foreach (var SymType in SymTypes)
            {
                if (!StyleManager.cacheSymbols[filePath].ContainsKey(SymType))
                {
                    StyleManager.cacheSymbols[filePath].Add(SymType, new Dictionary<string, IStyleGalleryItem>());
                }
                StyleManager.cacheSymbols[filePath][SymType].Clear();
                try
                {
                    enumStyleItem = styleGallery.get_Items(SymType, filePath, "");
                    enumStyleItem.Reset();
                    while ((tempStyleItem = enumStyleItem.Next()) != null)
                    {
                        try
                        {
                            if (!StyleManager.cacheSymbols[filePath][SymType].ContainsKey(tempStyleItem.Name))
                            {
                                StyleManager.cacheSymbols[filePath][SymType].Add(tempStyleItem.Name,
                                    (tempStyleItem as IClone).Clone() as IStyleGalleryItem);
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                catch
                {
                    continue;
                }
                finally
                {
                    if (enumStyleItem != null)
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(enumStyleItem);
                    }
                }
            }
        }


        public void removeStyleGallery(IStyleGallery styleGallery)
        {
            try
            {
                IStyleGalleryStorage styleGalleryStorage = styleGallery as IStyleGalleryStorage;
                for (int i = 0; i < styleGalleryStorage.FileCount; i++)
                {
                    styleGalleryStorage.RemoveFile(styleGalleryStorage.get_File(i));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

        }


        private string CopyNewServerStylePath()
        {
            string res = string.Empty;
            try
            {
                DateTime now = DateTime.Now;
                string time = now.ToString("yyyyMMddHHmmss");
                string blank = GApplication.ExePath + BLANKSTYLE;
                string newName = GApplication.ExePath + @"\STYLE" + time + ".ServerStyle";
                if (File.Exists(blank))
                {
                    File.Copy(blank, newName, true);
                    res = newName;
                }
            }
            catch
            {

            }
            return res;
        }

        //*******************************静态函数

        public static bool IsValidServerStyleFile(string file)
        {
            bool isValid = false;
            IStyleGallery styleGallery = new ServerStyleGalleryClass();
            try
            {
                if (!File.Exists(file)) { throw new Exception(); }
                IStyleGalleryStorage styleGalleryStorage = styleGallery as IStyleGalleryStorage;
                styleGalleryStorage.AddFile(file);
                styleGalleryStorage.TargetFile = file;
                if (styleGallery.ClassCount > 0)
                {
                    isValid = true;
                }
            }
            catch
            {
                isValid = false;
            }
            finally
            {
                styleGallery.Clear();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(styleGallery);
            }

            return isValid;
        }

        public static ISymbol OptimizeSymbol(ISymbol sym)
        {
            if (sym == null) { return null; }
            if (sym is IFillSymbol)
            {
                sym = OptimizeFillSymbol(sym as IFillSymbol);
            }
            return sym;
        }
        private static ISymbol OptimizeFillSymbol(IFillSymbol f)
        {
            if (f == null) { return null; }
            IFillSymbol newsym = (f as IClone).Clone() as IFillSymbol;
            if (newsym is IMultiLayerFillSymbol)
            {
                IMultiLayerFillSymbol multiFillSymbol = newsym as IMultiLayerFillSymbol;
                for (int i = 0; i < multiFillSymbol.LayerCount; i++)
                {
                    IFillSymbol fillSymbol = multiFillSymbol.get_Layer(i);
                    if (fillSymbol is IMarkerFillSymbol)
                    {
                        (multiFillSymbol as ILayerVisible).set_LayerVisible(i, false);
                    }
                }
            }
            return (ISymbol)newsym;
        }

    }
}
