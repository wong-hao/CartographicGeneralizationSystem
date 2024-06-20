using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Geodatabase;

namespace DatabaseUpdate
{
  class MatchResult
  {
    IFeature newFeature;
    IFeature orgFeature;
    double rate;

    public MatchResult(IFeature newFeature,IFeature orgFeature,double rate)
    {
      this.newFeature = newFeature;
      this.orgFeature = orgFeature;
      this.rate = rate;
    }
    public string 新测要素
    {
      get
      {
        if (newFeature != null)
          return string.Format("{0}:[{1}]", (newFeature.Table as IDataset).Name, newFeature.OID);
        else
          return "未匹配";
      }
    }

    public string 原始要素
    {
      get
      {
        if (orgFeature != null)
          return string.Format("{0}:[{1}]", (orgFeature.Table as IDataset).Name, orgFeature.OID);
        else
          return "未匹配";
      }
    }

    public double 匹配度
    {
      get 
      {
        return rate;
      }
    }

  }
}
