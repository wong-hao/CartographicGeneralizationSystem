using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;

namespace SMGI.Common {
    [Serializable]
    public class CodeInfo {
        internal CodeInfo(string name, string code) {
            Name = name;
            Code = code;
        }
        public string Name { get; private set; }
        public string Code { get; private set; }

        public string FullName {
            get {
                if (this.Parent == null)
                    return this.Name;
                else
                    return this.Parent.FullName + "." + this.Name;
            }
        }
        public ClassificationType ClassType {
            get {
                return ClassificationAndCode.CodeType(this.Code);
            }
        }
        [NonSerialized]
        CodeInfo parent;
        public CodeInfo Parent { get { return parent; } internal set { parent = value; } }

        public override string ToString() {
            return string.Format("[{0},{1}]" ,this.Code ,this.FullName);
        }
    }

    public enum ClassificationType {
        未定义 = int.MinValue,
        大类 = 1,
        中类 = 2,
        小类 = 4,
        子类 = 6
    }
    public class ClassificationAndCode {
        GApplication app;
        Dictionary<string, CodeInfo> nameDic;
        Dictionary<string, CodeInfo> codeDic;
        List<CodeInfo> infos;
        
        internal ClassificationAndCode(GApplication app) {
            this.app = app;
            infos = new List<CodeInfo>();
            codeDic = new Dictionary<string, CodeInfo>();
            nameDic = new Dictionary<string, CodeInfo>();
            this.Load();
            this.Save();
        }
        private void AddToDic(CodeInfo info) {
            codeDic.Add(info.Code, info);
            nameDic.Add(info.FullName, info);
        }
        private void AddParentAndAddToDict() {
            Queue<CodeInfo> noParent = new Queue<CodeInfo>(infos);
            while (noParent.Count > 0) {
                CodeInfo info = noParent.Dequeue();
                if (info.ClassType == ClassificationType.大类) {
                    AddToDic(info);
                    continue;
                }
                ClassificationType tp = NearestParent(info.ClassType);
                string code = ToParentCode(info.Code, tp);
                if (codeDic.ContainsKey(code)) {
                    info.Parent = codeDic[code];
                    AddToDic(info);
                    continue;
                }
                else {
                    noParent.Enqueue(info);
                }
            }
        }
        private void LoadFromDBF() {
            
            string name = @"GB名称代码.dbf";
            IWorkspace ws = GApplication.ShpFactory.OpenFromFile(GApplication.ExePath, 0);
            ITable tb = (ws as IFeatureWorkspace).OpenTable(name);
            ICursor c = tb.Search(null, true);
            IRow row = null;
            int code_idx = tb.FindField("分类代码");
            int name_idx = tb.FindField("要素名称");
            while ((row = c.NextRow())!=null) {
                CodeInfo info = new CodeInfo(row.get_Value(name_idx).ToString(),
                    row.get_Value(code_idx).ToString());
                infos.Add(info);
            }
            System.Runtime.InteropServices.Marshal.ReleaseComObject(c);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(tb);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);
        }
        internal void Load() {
            infos.Clear();
            object v = this.app.AppConfig["_gCode"];
            if (v == null) {
                LoadFromDBF();
            }
            else {
                infos.AddRange(v as CodeInfo[]);
            }
            AddParentAndAddToDict();
        }
        internal void Save() {
            this.app.AppConfig["_gCode"] = infos.ToArray();
        }

        public CodeInfo FromCode(string code) {
            try {
                return codeDic[code];
            }
            catch (Exception) {
                return null;                
            }
        }
        public CodeInfo FromFullName(string fullname) {
            
            try {
                return nameDic[fullname];
            }
            catch (Exception) {
                return null;
            }
        }
        public CodeInfo[] FromName(string name) {
            List<CodeInfo> codes = new List<CodeInfo>();
            foreach (var item in infos) {
                if (item.Name == name) {
                    codes.Add(item);
                }
            }
            return codes.ToArray();
        }
        public static string ToParentCode(string code ,ClassificationType type) {
            string pcode = code.Substring(0,(int)type);
            return pcode.PadRight(6, '0');
        }
        public static ClassificationType CodeType(string code) {
            string pcode = code.TrimEnd(new char[] { '0' });
            int min = int.MaxValue;
            foreach (var item in Enum.GetValues(typeof(ClassificationType))) {
                if (pcode.Length <= (int)item && min > (int)item)
                    min = (int)item;
            }
            return (ClassificationType)min;
        }
        public static ClassificationType NearestParent(ClassificationType tp) {
            int max = int.MinValue;
            foreach (var item in Enum.GetValues(typeof(ClassificationType))) {
                if ((int)tp > (int)item && max < (int)item)
                    max = (int)item;
            }
            return (ClassificationType)max;
        }
        /// <summary>
        /// 返回父类下的所有子类
        /// </summary>
        /// <param name="parentClassCode">分类，如果为null或enpty则返回所有类别</param>
        /// <returns></returns>
        public CodeInfo[] GetCodeInfos(string parentClassCode) {
            if (parentClassCode == null && parentClassCode == string.Empty) {
                return infos.ToArray();
            }
            List<CodeInfo> result = new List<CodeInfo>();
            try {
                CodeInfo parentInfo = codeDic[parentClassCode];

                foreach (var item in infos) {
                    if (item.Parent == parentInfo)
                        result.Add(item);
                }
            }
            catch (Exception) {                
                
            }
            return result.ToArray();
        }
    }
}


/*
2013年3月17日1:02:37 周启
修改了以下bug：
（1）由于大小类有重名，所以导致实现的时候出现错误
（2）由于Excel中漏掉了一项410200，导致导入出现死循环
（3）忘记添加到Application中去
TODO
（1）使用说明
（2）健壮性不强，如果dbf在外面被改，极有可能出现死循环
*/