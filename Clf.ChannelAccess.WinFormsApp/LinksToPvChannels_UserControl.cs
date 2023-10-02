//
// LinksToPvChannels_UserControl.cs
//
//

using System.Collections.Generic  ;
using System.Threading.Tasks ;

using Clf.Common.ExtensionMethods ;
using Clf.ChannelAccess.ExtensionMethods ;

using FluentAssertions ;

namespace ChannelAccess_WinFormsApp
{

  public partial class LinksToPvChannels_UserControl : System.Windows.Forms.UserControl
  {

    // private readonly Clf.ChannelAccess.IChannelsHub m_channelsHub = Clf.ChannelAccess.ChannelsHub.Instance ;

    private readonly Clf.ChannelAccess.IChannel m_channel ;

    private readonly int m_mainUiThreadId = System.Environment.CurrentManagedThreadId ;

    public LinksToPvChannels_UserControl ( )
    {
      InitializeComponent() ;

      Clf.ChannelAccess.Hub.OnInterestingEvent += (interestingEvent) => {
        System.Console.WriteLine(
          interestingEvent.ToString()
        ) ;
      } ;

      this.Load += new System.EventHandler(this.LinksToPvChannels_UserControl_Load) ;

      m_channel = Clf.ChannelAccess.Hub.GetOrCreateChannel("xx:one_long") ;
      //////////////// if ( Clf.ChannelAccess.IChannel.StateChangedEventIsSupported )
      //////////////// {
      ////////////////   m_channel.StateChanged += (change,newCurrentState) => {
      ////////////////     if ( 
      ////////////////       change.DescribesValueChange( 
      ////////////////         out var valueInfo,
      ////////////////         out var isInitialAcquisition
      ////////////////       )
      ////////////////     ) {
      ////////////////       m_textBox.Text = valueInfo.Value_AsDisplayString(
      ////////////////         Clf.ChannelAccess.WhichValueInfoElementsToInclude.AllAvailableElements
      ////////////////       ) ;
      ////////////////     }
      ////////////////   } ;
      //////////////// }
      //////////////// else
      {
        throw new System.ApplicationException("StateChange event is not supported") ;
      }

    }

    private async void LinksToPvChannels_UserControl_Load ( object? sender, System.EventArgs e )
    {
      try
      {
        
        bool connected = await m_channel.HasConnectedAndAcquiredValueAsync() ;
        m_pvInfoLabel.Text = (
          m_channel.IsConnected() 
          ? "PV is connected" 
          : "PV not yet connected"
        ) ;
      }
      catch ( System.Exception x )
      {
        // Hmm, not great but better than nothing ...
        m_pvInfoLabel.Text = x.Message ;
      }
    }

    private void m_button_Click ( object sender, System.EventArgs e )
    {
      if ( 
        m_textBox.Text.CanParseAs<int>(
          out int value
        )
      ) {
        // TODO : try/catch ...
        m_channel.PutValue(
          value + 1
        ) ;
      }

      // if ( 
      //   m_channel.CanGetValueAs<int>( 
      //     out var value 
      //   ) 
      // ) {
      //   // await m_channel.InitiateWriteOfNewValue_FireAndForget(
      //   m_button.Enabled = false ;
      //   try
      //   {
      //     // The 'await' might time out and throw an exception ...
      //     await m_channel.InitiateWrite_WaitingForAcknowledgementAsync(
      //       value + 1
      //     ) ;
      //   }
      //   catch ( System.Exception x )
      //   {
      //     m_pvInfoLabel.Text = x.Message ;
      //   }
      //   finally
      //   {
      //     m_button.Enabled = true ;
      //   }
      // }
    }

  }

}
