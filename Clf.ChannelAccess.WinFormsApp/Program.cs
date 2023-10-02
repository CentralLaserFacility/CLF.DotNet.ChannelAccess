//
// Program.cs
//

namespace ChannelAccess_WinFormsApp
{

  static class Program
  {

    [System.STAThread]
    static void Main()
    {
      System.Windows.Forms.Application.SetHighDpiMode(System.Windows.Forms.HighDpiMode.SystemAware) ;
      System.Windows.Forms.Application.EnableVisualStyles() ;
      // System.Windows.Forms.Application.SetDefaultFont(
      //   new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point)
      // ) ;
      System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false) ;
      System.Windows.Forms.Application.Run(
        new MainForm()
      ) ;
    }

  }

}
