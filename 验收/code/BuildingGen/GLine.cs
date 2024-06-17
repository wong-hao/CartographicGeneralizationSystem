using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace BuildingGen.GLine {

    [Serializable]
    internal class TableInfo {
        internal string StrokeRalationTableName;
        internal string ConnectTableName;
        internal string StrokeInfoTableName;
        //internal string LineInfoTableName;
        //internal string ConfilctTableName;
        internal TableInfo(GLayerInfo info) {
            StrokeRalationTableName = info.Layer.Name + "_StrokeRalation";
            ConnectTableName = info.Layer.Name + "_ConnectInfo";
            StrokeInfoTableName = info.Layer.Name + "_StrokeInfo";
            //LineInfoTableName = info.Layer.Name + "_LineInfo";
        }
    }
    internal class LineRalation {
        GWorkspace ws;
        GLayerInfo layerInfo;
        IFeatureClass fc;
        internal ITable StrokeRalationTable;
        internal ITable StrokeInfoTable;
        //internal ITable LineInfoTable;
        internal ITable ConnectTable;

        static GeometryEnvironmentClass ge = new GeometryEnvironmentClass();
        internal LineRalation(GWorkspace ws, GLayerInfo layerInfo) {
            this.ws = ws;
            this.fc = (layerInfo.Layer as IFeatureLayer).FeatureClass;
            this.layerInfo = layerInfo;
            if (layerInfo.LayerType != GCityLayerType.道路 || (layerInfo.Layer as IFeatureLayer).FeatureClass.ShapeType != esriGeometryType.esriGeometryPolyline) {
                throw new Exception();
            }
            if (layerInfo.otherInfo is TableInfo) {
                OpenTable();
                //GetStrokeLine();
                //GetConnectLine();
            } else {
                CreateTable();
                GetStrokeRelation();
                GetConnectEx();
            }
        }
        private void CreateTable() {
            
            IFeatureWorkspace fws = ws.Workspace as IFeatureWorkspace;
            TableInfo tableInfo = new TableInfo(layerInfo);

            #region 创建stroke关系
            if (true) {
                FieldsClass fs = new FieldsClass();
                IFieldEdit2 fOID = new FieldClass();
                fOID.Name_2 = "OID";
                fOID.Type_2 = esriFieldType.esriFieldTypeOID;
                fs.AddField(fOID as IField);
                IFieldEdit2 fStroke = new FieldClass();
                fStroke.Name_2 = "Stroke_ID";
                fStroke.Type_2 = esriFieldType.esriFieldTypeInteger;
                fs.AddField(fStroke as IField);
                IFieldEdit2 fLineID = new FieldClass();
                fLineID.Name_2 = "Line_ID";
                fLineID.Type_2 = esriFieldType.esriFieldTypeInteger;
                fs.AddField(fLineID as IField);
                StrokeRalationTable = fws.CreateTable(tableInfo.StrokeRalationTableName, fs, null, null, "");
            }
            #endregion
            #region 创建连接关系表
            if (true) {
                FieldsClass fs = new FieldsClass();
                IFieldEdit2 fOID = new FieldClass();
                fOID.Name_2 = "OID";
                fOID.Type_2 = esriFieldType.esriFieldTypeOID;
                fs.AddField(fOID as IField);
                IFieldEdit2 fStroke = new FieldClass();
                fStroke.Name_2 = "Line_ID";
                fStroke.Type_2 = esriFieldType.esriFieldTypeInteger;
                fs.AddField(fStroke as IField);
                IFieldEdit2 fLineID = new FieldClass();
                fLineID.Name_2 = "ConnectLine_ID";
                fLineID.Type_2 = esriFieldType.esriFieldTypeInteger;
                fs.AddField(fLineID as IField);
                ConnectTable = fws.CreateTable(tableInfo.ConnectTableName, fs, null, null, "");
            }
            #endregion
            #region 创建Stroke信息关系
            if (true) {
                FieldsClass fs = new FieldsClass();
                IFieldEdit2 fOID = new FieldClass();
                fOID.Name_2 = "OID";
                fOID.Type_2 = esriFieldType.esriFieldTypeOID;
                fs.AddField(fOID as IField);
                //IFieldEdit2 fLineID = new FieldClass();
                //fLineID.Name_2 = "Stroke_ID";
                //fLineID.Type_2 = esriFieldType.esriFieldTypeInteger;
                //fs.AddField(fLineID as IField);
                IFieldEdit2 fStrokeLength = new FieldClass();
                fStrokeLength.Name_2 = "Stroke_Length";
                fStrokeLength.Type_2 = esriFieldType.esriFieldTypeDouble;
                fs.AddField(fStrokeLength as IField);
                IFieldEdit2 fConnectLength = new FieldClass();
                fConnectLength.Name_2 = "Connect_Count";
                fConnectLength.Type_2 = esriFieldType.esriFieldTypeInteger;
                fs.AddField(fConnectLength as IField);
                StrokeInfoTable = fws.CreateTable(tableInfo.StrokeInfoTableName, fs, null, null, "");
            }
            #endregion
            layerInfo.otherInfo = tableInfo;
            ws.Save();            
        }
        private void OpenTable() {
            IFeatureWorkspace fws = ws.Workspace as IFeatureWorkspace;
            TableInfo tableInfo = layerInfo.otherInfo as TableInfo;
            StrokeRalationTable = fws.OpenTable(tableInfo.StrokeRalationTableName);
            ConnectTable = fws.OpenTable(tableInfo.ConnectTableName);
            StrokeInfoTable = fws.OpenTable(tableInfo.StrokeInfoTableName);
        }

        private void GetStrokeRelation() {
            IFeatureCursor fCursor = fc.Search(null, true);
            IFeature feature = null;
            int strokeid = 0;
            IQueryFilter qf = new QueryFilterClass();
            int sIndex = StrokeRalationTable.FindField("Stroke_ID");
            int lIndex = StrokeRalationTable.FindField("Line_ID");
            while ((feature = fCursor.NextFeature()) != null) {
                qf.WhereClause = "Line_ID = " + feature.OID;
                if (StrokeRalationTable.RowCount(qf) == 0) {
                    IRow strokeInfoRow = StrokeInfoTable.CreateRow();
                    strokeid = strokeInfoRow.OID;

                    IRow row = StrokeRalationTable.CreateRow();
                    row.set_Value(sIndex, strokeid);
                    row.set_Value(lIndex, feature.OID);
                    row.Store();
                    strokeLine next = FindStrokeLine(fc, feature.OID, true);
                    double length = (feature.Shape as IPolyline).Length;
                    while (next != null) {
                        qf.WhereClause = "Line_ID = " + next.OID;
                        if (StrokeRalationTable.RowCount(qf) != 0) {
                            break;
                        }
                        row = StrokeRalationTable.CreateRow();
                        row.set_Value(sIndex, strokeid);
                        row.set_Value(lIndex, next.OID);
                        row.Store();
                        length += next.length;
                        next = FindStrokeLine(fc, next.OID, !next.fromPoint);
                    }
                    next = FindStrokeLine(fc, feature.OID, false);
                    while (next != null) {
                        qf.WhereClause = "Line_ID = " + next.OID;
                        if (StrokeRalationTable.RowCount(qf) != 0) {
                            break;
                        }
                        row = StrokeRalationTable.CreateRow();
                        row.set_Value(sIndex, strokeid);
                        row.set_Value(lIndex, next.OID);
                        row.Store();
                        length += next.length;
                        next = FindStrokeLine(fc, next.OID, !next.fromPoint);
                    }
                    strokeInfoRow.set_Value(strokeInfoRow.Fields.FindField("Stroke_Length"), length);
                    strokeInfoRow.Store();
                }
            }
        }
        internal class strokeLine {
            internal int OID;
            internal bool fromPoint;
            internal double length;
        }
        internal static strokeLine FindStrokeLine(IFeatureClass fc, int oid, bool fromPoint) {
            IFeature self = fc.GetFeature(oid);
            strokeLine result = new strokeLine();

            IPoint passPoint = null;
            IPoint beforePoint = null;
            if (fromPoint) {
                passPoint = (self.Shape as IPolyline).FromPoint;
                beforePoint = (self.Shape as IPointCollection).get_Point(1);
            } else {
                passPoint = (self.Shape as IPolyline).ToPoint;
                beforePoint = (self.Shape as IPointCollection).get_Point((self.Shape as IPointCollection).PointCount - 2);
            }
            ISpatialFilter sf = new SpatialFilterClass();
            sf.Geometry = (passPoint as ITopologicalOperator).Buffer(0.01);
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            int count = fc.FeatureCount(sf);
            if (count < 2) {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(self);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(sf);
                return null;
            } else {
                IFeatureCursor fCursor = fc.Search(sf, true);
                IFeature f = null;
                double minAngle = double.MaxValue;
                result.OID = -1;
                result.fromPoint = true;
                while ((f = fCursor.NextFeature()) != null) {
                    if (f.OID == self.OID) {
                        continue;
                    }
                    IPoint fp = (f.Shape as IPolyline).FromPoint;
                    if ((passPoint as IProximityOperator).ReturnDistance(fp) < 0.01) {
                        IPoint toPoint = (f.Shape as IPointCollection).get_Point(1);
                        double a = Angle(beforePoint, passPoint, toPoint);
                        if (a < minAngle && a < Math.PI / 6) {
                            result.OID = f.OID;
                            result.fromPoint = true;
                            result.length = (f.Shape as IPolyline).Length;
                        }
                    }
                    IPoint tp = (f.Shape as IPolyline).ToPoint;
                    if ((passPoint as IProximityOperator).ReturnDistance(tp) < 0.01) {
                        IPoint toPoint = (f.Shape as IPointCollection).get_Point((f.Shape as IPointCollection).PointCount - 2);
                        double a = Angle(beforePoint, passPoint, toPoint);
                        if (a < minAngle && a < Math.PI / 6) {
                            result.OID = f.OID;
                            result.fromPoint = false;
                            result.length = (f.Shape as IPolyline).Length;
                        }
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(self);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(sf);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);

                if (result.OID != -1) {
                    return result;
                } else {
                    return null;
                }
            }
        }
        static double Angle(IPoint from, IPoint pass, IPoint to) {
            double a = ge.ConstructThreePoint(from, pass, to);
            return Math.PI - Math.Abs(a);
        }

        /// <summary>
        /// 获取到的是线与线之间的连接关系
        /// </summary>
        private void GetConnectLine() {
            int cIndex = ConnectTable.FindField("ConnectLine_ID");
            int lIndex = ConnectTable.FindField("Line_ID");

            IFeatureCursor fCursor = fc.Search(null, true);
            IFeature feature = null;
            ISpatialFilter sf = new SpatialFilterClass();
            sf.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            while ((feature = fCursor.NextFeature()) != null) {
                sf.Geometry = feature.Shape;
                IFeatureCursor insertCursor = fc.Search(sf, true);
                IFeature insertFeature = null;
                while ((insertFeature = insertCursor.NextFeature()) != null) {
                    if (insertFeature.OID != feature.OID) {
                        IRow row = ConnectTable.CreateRow();
                        row.set_Value(lIndex, feature.OID);
                        row.set_Value(cIndex, insertFeature.OID);
                        row.Store();
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(insertCursor);
            }
        }
        /// <summary>
        /// 获取到的是stroke 与stroke 之间的连接关系
        /// </summary>
        private void GetConnectEx() {
            int cIndex = ConnectTable.FindField("ConnectLine_ID");
            int lIndex = ConnectTable.FindField("Line_ID");

            ICursor cursor = StrokeInfoTable.Search(null, true);
            IQueryFilter qf_strokeRelation = new QueryFilterClass();
            ISpatialFilter sf_ConnectRelation = new SpatialFilterClass();
            IQueryFilter qf_ConnectRelationExist = new QueryFilterClass();
            sf_ConnectRelation.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            int lineIDIndex = StrokeRalationTable.FindField("Line_ID");

            IRow strokeInfoRow = null;
            while ((strokeInfoRow = cursor.NextRow())!= null) {
                int strokeID = strokeInfoRow.OID;
                System.Diagnostics.Debug.WriteLine("["+System.DateTime.Now.ToString() +"]"+ strokeID);
                int  connectCount = 0;
                qf_strokeRelation.WhereClause = "Stroke_ID = " + strokeID;
                ICursor strokeCursor = StrokeRalationTable.Search(qf_strokeRelation, true);
                IRow row_strokeRelation = null;
                while ((row_strokeRelation = strokeCursor.NextRow()) != null) {
                    int lineID = Convert.ToInt32(row_strokeRelation.get_Value(lineIDIndex));
                    IFeature f = fc.GetFeature(lineID);
                    sf_ConnectRelation.Geometry = f.Shape;
                    IFeatureCursor fCursor = fc.Search(sf_ConnectRelation, true);
                    IFeature connectFeature = null;
                    while ((connectFeature = fCursor.NextFeature())!= null) {
                        int connectStrokeID = GetStrokeID(connectFeature.OID);
                        if (strokeID == connectStrokeID)
                            continue;
                        qf_ConnectRelationExist.WhereClause = "Line_ID = "+ strokeID +" And ConnectLine_ID = " + connectStrokeID;
                        if (ConnectTable.RowCount(qf_ConnectRelationExist) > 0) {
                            continue;
                        }
                        IRow newConnectRow = ConnectTable.CreateRow();
                        newConnectRow.set_Value(lIndex, strokeID);
                        newConnectRow.set_Value(cIndex, connectStrokeID);
                        newConnectRow.Store();
                        connectCount++;
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(newConnectRow);
                    }
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(fCursor);
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(f);
                }
                strokeInfoRow.set_Value(strokeInfoRow.Fields.FindField("Connect_Count"), connectCount);
                strokeInfoRow.Store();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(strokeCursor);
            }
        }

        private void GetStrokInfo() {
            IDataStatistics ds = new DataStatisticsClass();
            ICursor cursor = StrokeRalationTable.Search(null, true);
            ds.Cursor = cursor;
            ds.Field = "Stroke_ID";
            System.Collections.IEnumerator enumerator = ds.UniqueValues;
            enumerator.Reset();
            IQueryFilter qf_strokeRelation = new QueryFilterClass();
            IQueryFilter qf_ConnectRelation = new QueryFilterClass();
            int lineIDIndex = StrokeRalationTable.FindField("Line_ID");
            while (enumerator.MoveNext()) {
                int id = Convert.ToInt32(enumerator.Current);                
                qf_strokeRelation.WhereClause = "Stroke_ID = " + id;
                ICursor strokeCursor = StrokeRalationTable.Search(qf_strokeRelation, true);
                IRow row_strokeRelation = null;
                double strokeLength = 0;
                while ((row_strokeRelation = strokeCursor.NextRow())!= null) {
                    int lineID = Convert.ToInt32(row_strokeRelation.get_Value(lineIDIndex));
                    IFeature line = fc.GetFeature(lineID);
                    IPolyline l = line.Shape as IPolyline;
                    strokeLength += l.Length;
                }
                IRow row = StrokeInfoTable.CreateRow();
                row.set_Value(row.Fields.FindField("Stroke_ID"), id);
                row.set_Value(row.Fields.FindField("StrokeLength"), strokeLength);
                row.Store();
                System.Runtime.InteropServices.Marshal.ReleaseComObject(strokeCursor);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(row);
            }
            ICursor updataCursor = StrokeInfoTable.Update(null, true);
            IRow uRow = null;
            while ((uRow = updataCursor.NextRow()) != null) {
                qf_ConnectRelation.WhereClause = "Line_ID = " + uRow.OID;
                ICursor connectCursor = ConnectTable.Search(qf_ConnectRelation, true);
                IRow cRow = null;
                while ((cRow = connectCursor.NextRow())!=null) {
                    int cStrokeID = Convert.ToInt32(cRow.get_Value(cRow.Fields.FindField("ConnectLine_ID")));
                }
            }
        }

        public int GetStrokeID(int lineID) {
            IQueryFilter qf = new QueryFilterClass();
            qf.WhereClause = "Line_ID = " + lineID;
            ICursor cursor = StrokeRalationTable.Search(qf, true);
            IRow row = cursor.NextRow();
            if (row == null)
                return -1;

            return Convert.ToInt32(row.get_Value(row.Fields.FindField("Stroke_ID")));
        }
    }

}
