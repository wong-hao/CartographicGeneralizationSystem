using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
//*********************************
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Linq;

namespace SMGI.Common.AttributeTable
{
    internal class TableWrapper : BindingList<IRow>, ITypedList
    {
        //private varibles
        private ITable wrappedTable;//绑定的ITable
        private Hashtable hashTable;
        //private IFeatureLayer wrappedFeatureLayer;//目标图层
        private List<PropertyDescriptor> fakePropertiesList = new List<PropertyDescriptor>();//每一个元素表示ITable的一个字段
        private IWorkspaceEdit wkspcEdit;//编辑控制
        private IWorkspaceEditEvents_Event workspaceEditEvent;
        //private string m_FilterStr;
        //构造函数
        public TableWrapper(ITable tableToWrap, string filterString, bool IsShowingAll, ref Hashtable m_Hashtable)
        {
            //wrappedFeatureLayer = featureLayerToWrap;
            //wrappedTable = featureLayerToWrap as ITable;
            wrappedTable = tableToWrap;
            GenerateFakeProperties();
            AddData(filterString, IsShowingAll);
            wkspcEdit = ((IDataset)wrappedTable).Workspace as IWorkspaceEdit;
            workspaceEditEvent = wkspcEdit as IWorkspaceEditEvents_Event;
            //workspaceEditEvent.OnStartEditing += new IWorkspaceEditEvents_OnStartEditingEventHandler(workspaceEditEvent_OnStartEditing);
            //workspaceEditEvent.OnStopEditing += new IWorkspaceEditEvents_OnStopEditingEventHandler(workspaceEditEvent_OnStopEditing);
            //AllowNew = false;
            AllowRemove = wkspcEdit.IsBeingEdited();
            if (IsShowingAll)
            {
                m_Hashtable = hashTable;
            }
        }

        void workspaceEditEvent_OnStopEditing(bool saveEdits)
        {
            AllowNew = false;
            AllowRemove = false;
        }

        void workspaceEditEvent_OnStartEditing(bool withUndoRedo)
        {
            AllowNew = true;
            AllowRemove = true;
        }

        #region ITypedList Members

        /// <summary>
        /// 返回<see cref="T:System.ComponentModel.PropertyDescriptorCollection"></see>
        /// 绑定数据的每个项目的属性
        /// </summary>
        /// <param name="listAccessors"></param>
        /// <returns></returns>
        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            PropertyDescriptorCollection propCollection = null;
            if (null == listAccessors)
            {
                // Return all properties
                propCollection = new PropertyDescriptorCollection(fakePropertiesList.ToArray());
            }
            else
            {
                // Return the requested properties by checking each item in listAccessors
                // to make sure it exists in our property collection.
                List<PropertyDescriptor> tempList = new List<PropertyDescriptor>();
                foreach (PropertyDescriptor curPropDesc in listAccessors)
                {
                    if (fakePropertiesList.Contains(curPropDesc))
                    {
                        tempList.Add(curPropDesc);
                    }
                }
                propCollection = new PropertyDescriptorCollection(tempList.ToArray());
            }
            return propCollection;
        }

        /// <summary>
        /// 返回List的名称
        /// </summary>
        /// <param name="listAccessors"></param>
        /// <returns></returns>
        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return ((IDataset)wrappedTable).Name;
        }

        #endregion ITypedList Members

        public bool UseCVDomains
        {
            set
            {
                foreach (FieldPropertyDescriptor curPropDesc in fakePropertiesList)
                {
                    if (curPropDesc.HasCVDomain)
                    {
                        // Field has a coded value domain so turn the usage of this on or off
                        // as requested
                        curPropDesc.UseCVDomain = value;
                    }
                }
            }
        }

        #region Protected Overrides

        bool isSorted = false;
        PropertyDescriptor sortProperty;
        private ListSortDirection sortDirection = ListSortDirection.Ascending;

        protected override bool SupportsSortingCore
        {
            get
            {
                return true;
            }
        }

        protected override bool IsSortedCore
        {
            get
            {
                return isSorted;
            }
        }
        protected override PropertyDescriptor SortPropertyCore
        {
            get
            {
                return sortProperty;
            }
        }
        protected override ListSortDirection SortDirectionCore
        {
            get
            {
                return sortDirection;
            }
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            var fieldprop = prop as FieldPropertyDescriptor;
            if (fieldprop == null)
                return;

            List<IRow> rows = new List<IRow>();
            if (direction == ListSortDirection.Ascending)
            {
                rows = (from x in Items
                        where fieldprop.GetValue(x).ToString() == "<空>" 
                        select x).ToList();

                rows.AddRange((from x in Items
                               where fieldprop.GetValue(x).ToString() != "<空>"
                               orderby fieldprop.GetValue(x) ascending
                               select x).ToList());
            }
            else
            {
                rows = (from x in Items
                        where fieldprop.GetValue(x).ToString() != "<空>"
                        orderby fieldprop.GetValue(x) descending
                        select x).ToList();

                rows.AddRange((from x in Items
                        where fieldprop.GetValue(x).ToString() == "<空>"
                        select x).ToList());
            }
            Items.Clear();
            
            foreach (var r in rows)
            {
                Items.Add(r);
            }
            isSorted = true;
            sortProperty = prop;
            sortDirection = direction;
            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }
        protected override void RemoveSortCore()
        {
            base.RemoveSortCore();
        }



        /// <summary>
        /// add新要素
        /// </summary>
        /// <param name="e"></param>
        protected override void OnAddingNew(AddingNewEventArgs e)
        {
            // Check that we can add rows, this property could have been changed
            if (AllowNew)
            {
                // Need to create a new IRow
                IRow newRow = wrappedTable.CreateRow();
                e.NewObject = newRow;

                // Loop through fields and set default values
                for (int fieldCount = 0; fieldCount < newRow.Fields.FieldCount; fieldCount++)
                {
                    IField curField = newRow.Fields.get_Field(fieldCount);
                    if (curField.Editable)
                    {
                        newRow.set_Value(fieldCount, curField.DefaultValue);
                    }
                }
                // Save default values
                bool weStartedEditing = StartEditOp();
                newRow.Store();
                StopEditOp(weStartedEditing);
                base.OnAddingNew(e);
            }
        }

        /// <summary>
        /// 在指定Index处删除Item
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index)
        {
            // Check that we can still delete rows, this property could have been changed
            if (AllowRemove)
            {
                // Get the corresponding IRow
                IRow itemToRemove = Items[index];
                bool weStartedEditing = StartEditOp();
                // Delete the row
                itemToRemove.Delete();
                StopEditOp(weStartedEditing);
                base.RemoveItem(index);
            }
        }

        #endregion Protected Overrides


        #region Private Methods
        /// <summary>
        /// 生成FakeProperties
        /// </summary>
        private void GenerateFakeProperties()
        {
            // Loop through fields in wrapped table
            for (int fieldCount = 0; fieldCount < wrappedTable.Fields.FieldCount; fieldCount++)
            {
                if (wrappedTable is IGeoFeatureLayer)
                {
                    var gl = wrappedTable as IGeoFeatureLayer;
                    if (gl.Renderer is IRepresentationRenderer)
                    {
                        var rpc = (gl.Renderer as IRepresentationRenderer).RepresentationClass;
                        if (rpc.OverrideFieldIndex == fieldCount)
                            continue;
                    }
                }

                // Create a new property descriptor to represent the field
                FieldPropertyDescriptor newPropertyDesc = new FieldPropertyDescriptor(
                  wrappedTable, wrappedTable.Fields.get_Field(fieldCount).Name, fieldCount);


                fakePropertiesList.Add(newPropertyDesc);
            }
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        private void AddData(string m_FilterString, bool m_IsShowingAll)
        {
            // Get a search cursor that returns all rows. Note we do not want to recycle
            // the returned IRow otherwise all rows in the bound control will be identical
            // to the last row read...
            IQueryFilter queryFilter = new QueryFilterClass();
            queryFilter.WhereClause = m_FilterString;
            if (m_IsShowingAll)
            {
                 ICursor cur = wrappedTable.Search(queryFilter, false);
                 IRow curRow = cur.NextRow();
                 int bindingID = 0;
                 hashTable = new Hashtable();
                 while (null != curRow)
                 {
                     //调整的代码
                     int FontSizeIndex = 0;
                     if ((FontSizeIndex = curRow.Fields.FindField("FontSize")) != -1)
                     {
                         object Obvalue = curRow.get_Value(FontSizeIndex);
                         if (Obvalue != null && !Convert.IsDBNull(Obvalue) && Obvalue.ToString() != "")
                         {
                             //2.8345 默认磅值，需要调整为毫米值0.3528
                             Obvalue = Convert.ToDecimal(Convert.ToDouble(Obvalue) / 2.8345);
                         }
                         curRow.set_Value(FontSizeIndex, Obvalue);
                         //curRow.Store();
                     }
                     //
                     this.Add(curRow);
                     hashTable.Add(curRow.OID, bindingID++);
                     curRow = cur.NextRow();
                 }
                 System.Runtime.InteropServices.Marshal.ReleaseComObject(cur);
            }
            else
            {
                ICursor cur;
                ITableSelection tableSelection = wrappedTable as ITableSelection;
                tableSelection.SelectionSet.Search(queryFilter, false, out cur);
                IRow curRow = cur.NextRow();
                while (null != curRow)
                {
                    this.Add(curRow);
                    curRow = cur.NextRow();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(cur);
            }
           
            
        }

        /// <summary>
        /// 打开编辑
        /// </summary>
        /// <returns></returns>
        private bool StartEditOp()
        {
            // Check to see if we're editing
            if (!wkspcEdit.IsBeingEdited())
            {
                return false;
            }
            // Start operation
            wkspcEdit.StartEditOperation();
            return true;
        }

        /// <summary>
        /// 结束编辑
        /// </summary>
        /// <param name="weStartedEditing"></param>
        private void StopEditOp(bool weStartedEditing)
        {
            // Stop edit operation
            wkspcEdit.StopEditOperation();
        }

        #endregion Private Methods

    }
}
