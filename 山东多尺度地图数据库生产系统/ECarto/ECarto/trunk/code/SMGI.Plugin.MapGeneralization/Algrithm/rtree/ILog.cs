namespace DatabaseUpdate.RTree
{
  interface ILog
  {
    void Warn(string p);

    void Info(string p);

    void Debug(string p);

    bool IsDebugEnabled { get; }

    void Error(string p);
  }

  class XLog : ILog
  {
    #region ILog 成员

    public void Warn(string p)
    {
      
    }

    public void Info(string p)
    {
      
    }

    public void Debug(string p)
    {
      
    }

    public bool IsDebugEnabled
    {
      get { return false; } 
    }

    public void Error(string p)
    {
      
    }

    #endregion
  }

  static class LogManager
  {
    internal static ILog GetLogger(string p)
    {
      return new XLog();
    }
  }
}