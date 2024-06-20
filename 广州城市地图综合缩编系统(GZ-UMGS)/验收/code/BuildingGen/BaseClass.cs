using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.ADF.BaseClasses;

namespace BuildingGen
{
    public interface IGenCreate
    {
        void OnGenCreate(GApplication app);
    }

    public class GenDefaultPara
    {
        public string Name { get; private set; }
        public object DefaultValue { get; private set; }
        public GenDefaultPara(string name, object defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
        }
    }

    public abstract class BaseGenTool : BaseTool, IGenCreate
    {
        protected GApplication m_application;
        protected GenDefaultPara[] m_usedParas = null;

        #region IGenCreate 成员

        public virtual void OnGenCreate(GApplication app)
        {
            this.m_application = app;
            if (m_usedParas != null)
            {
                foreach (GenDefaultPara para in m_usedParas)
                {
                    app.GenPara.RegistPara(para.Name, para.DefaultValue);
                }
            }
        }

        public override void OnCreate(object hook)
        {
            //throw new NotImplementedException();
        }
        #endregion
    }

    public abstract class BaseGenCommand : BaseCommand, IGenCreate
    {
        protected GApplication m_application;
        protected GenDefaultPara[] m_usedParas = null;
        #region IGenCommand 成员

        public virtual void OnGenCreate(GApplication app)
        {
            this.m_application = app;
            if (m_usedParas != null)
            {
                foreach (GenDefaultPara para in m_usedParas)
                {
                    app.GenPara.RegistPara(para.Name, para.DefaultValue);
                }
            }
        }

        public override void OnCreate(object hook)
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
