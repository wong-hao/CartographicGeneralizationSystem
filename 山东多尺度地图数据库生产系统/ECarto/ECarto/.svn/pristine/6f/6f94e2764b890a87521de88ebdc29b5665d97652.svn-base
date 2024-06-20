using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.DisplayUI;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
namespace SMGI.Common {
    public partial class BaseSymbolPropertyPage 
        : UserControl
        ,
        IComPropertyPage,
        IPropertyPageContext,
        ISymbolPropertyPage
    {
        protected ISymbol m_Symbol;
        protected string m_pageTitle;
        protected IComPropertyPageSite m_pageSite;
        protected bool m_dirtyFlag;
        public BaseSymbolPropertyPage() {
            InitializeComponent();
        } 

        private void SetPageDirty(bool dirty) {
            if (m_dirtyFlag != dirty) {
                m_dirtyFlag = dirty;
                if (m_pageSite != null)
                    m_pageSite.PageChanged();

                ((IComPropertyPage)this).Apply();
            }
        }

        #region IComPropertyPage Members

        string IComPropertyPage.Title {
            get {
                return m_pageTitle;
            }
            set {
                //TODO: Uncomment if title can be modified
                m_pageTitle = value;
            }
        }

        int IComPropertyPage.Width {
            get { return this.Width; }
        }

        int IComPropertyPage.Height {
            get { return this.Height; }
        }

        int IComPropertyPage.Activate() {
            //TODO: Add other page initialization

            return this.Handle.ToInt32();
        }

        void IComPropertyPage.Deactivate() {
            //TODO: Release resources and objects
            this.Dispose(true);
        }

        /// <summary>
        /// Indicates if the page applies to the specified objects
        /// Do not hold on to the objects here.
        /// </summary>
        bool IComPropertyPage.Applies(ESRI.ArcGIS.esriSystem.ISet objects) {
            if (objects == null || objects.Count == 0)
                return false;

            bool isEditable = false;
            objects.Reset();
            object testObject;
            while ((testObject = objects.Next()) != null) {
                if (testObject != null) {
                    if (testObject.GetType() == m_Symbol.GetType()) {
                        isEditable = true;
                    }
                }
            }

            return isEditable;
        }

        /// <summary>
        /// Supplies the page with the object(s) to be edited
        /// </summary>
        void IComPropertyPage.SetObjects(ESRI.ArcGIS.esriSystem.ISet objects) {
            if (objects == null || objects.Count == 0)
                return;

            objects.Reset();
            object testObject;
            while ((testObject = objects.Next()) != null) {
                //TODO: Hold on to applicable object if necessary
                if (testObject != null) {
                    if (testObject.GetType() == m_Symbol.GetType()) {
                        m_Symbol = testObject as ISymbol;
                    }
                }
            }
        }

        IComPropertyPageSite IComPropertyPage.PageSite {
            set {
                m_pageSite = value;
            }
        }

        /// <summary>
        /// Indicates if the Apply button should be enabled
        /// </summary>
        bool IComPropertyPage.IsPageDirty {
            get { return m_dirtyFlag; }
        }

        public virtual void Apply() {
            if (m_dirtyFlag) {
                //TODO: Apply change to objects
                //get the LogoMarkerSymbol from the dictionaty
                 if (null != m_Symbol)
                    //calling IPropertyPageContext.QueryObject will apply changes to the object
                     QueryObject(m_Symbol);


                SetPageDirty(false);
            }
        }

        void IComPropertyPage.Cancel() {
            if (m_dirtyFlag) {
                //TODO: Reset UI or any temporary changes made to objects

                SetPageDirty(false);
            }
        }

        void IComPropertyPage.Show() {
            this.Show();
        }

        void IComPropertyPage.Hide() {
            this.Hide();
        }

        string IComPropertyPage.HelpFile {
            get { return string.Empty; }
        }

        int IComPropertyPage.get_HelpContextID(int controlID) {
            return 0;
        }

        int IComPropertyPage.Priority {
            get {
                return 2;
            }
            set {
            }
        }

        #endregion

        #region IPropertyPageContext 成员

        bool IPropertyPageContext.Applies(object unkArray) {
            object[] arr = (object[])unkArray;
            if (null == arr || 0 == arr.Length)
                return false;

            for (int i = 0; i < arr.Length; i++) {
                if (arr[i].GetType() == m_Symbol.GetType())
                    return true;
            }

            return false;
        }

        void IPropertyPageContext.Cancel() {
            return;
        }

        object IPropertyPageContext.CreateCompatibleObject(object kind) {
            return null;
        }

        string IPropertyPageContext.GetHelpFile(int controlID) {
            return string.Empty;
        }

        int IPropertyPageContext.GetHelpId(int controlID) {
            return -1;
        }

        int IPropertyPageContext.Priority {
            get { return 2; }
        }

        public void QueryObject(object theObject) {
            (theObject as IClone).Assign(m_Symbol as IClone);
        }

        #endregion

        #region ISymbolPropertyPage 成员

        esriUnits ISymbolPropertyPage.Units {
            get;
            set;
        }

        #endregion
    }
}
