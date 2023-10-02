//
// TempFile.cs
// 

namespace Clf.ThinIoc
{

  // Based on an implementation by Marc Gravell ...
  // https://stackoverflow.com/questions/400140/how-do-i-automatically-delete-temp-files-in-c
  
  // TODO : MOVE TO COMMON !!!

  public sealed class TempFile : System.IDisposable
  {

    private string m_tempFilePath ;

    public TempFile ( string? path = null )
    {
      m_tempFilePath = path ?? System.IO.Path.GetTempFileName() ;
    }

    public string Path => m_tempFilePath ;
    
    ~TempFile ( ) 
    { 
      Dispose(false) ; 
    }

    public void Dispose ( ) 
    { 
      Dispose(true) ; 
    }

    private void Dispose ( bool disposing )
    {
      if ( disposing )
      {
        System.GC.SuppressFinalize(this) ;                
      }
      if ( m_tempFilePath != null )
      {
        try 
        { 
          System.IO.File.Delete(m_tempFilePath) ; 
        }
        catch 
        { 
          // Best effort ...
        } 
        m_tempFilePath = null! ;
      }
    }

    public static void UsageExample ( )
    {
      string path ;
      using ( var tmp = new TempFile() )
      {
        path = tmp.Path ;
        System.Console.WriteLine(
          System.IO.File.Exists(path)
        ) ;
      }
      System.Console.WriteLine(
        System.IO.File.Exists(path) 
      ) ;
    }
  
  }

}

