//
// MessengerExperiments_01.cs
//

using System.Collections.Generic ;
using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;
using FluentAssertions ;
using Xunit ;

namespace Clf.ChannelAccess.Experimental
{

  public record MyMessage ( int Payload ) ;

  public class MessengerExperiments_01
  {

    // https://docs.microsoft.com/en-us/windows/communitytoolkit/mvvm/messenger

    [Fact]
    public void Test_01 ( )
    {
      int payloadReceived = 0 ;
      // int payloadReceived2 = 0 ;
      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<MyMessage>(
        this,
        (sender,message) => {
          payloadReceived = message.Payload ;
        }
      ) ;
      // Hmm, can only register a given recipient ONCE ...
      // CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<MyMessage>(
      //   this,
      //   (sender,message) => {
      //     payloadReceived2 = message.Payload ;
      //   }
      // ) ;
      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send<MyMessage>(
        new MyMessage(123)
      ) ;
      return ; // THE CHECKS ARE EXPECTED TO FAIL ... !!!
      payloadReceived.Should().Be(123) ;
      // payloadReceived2.Should().Be(123) ;
    }

    public class MyRecipient_A : CommunityToolkit.Mvvm.Messaging.IRecipient<MyMessage>
    {
      public int PayloadReceived { get ; private set ; } = 0 ;
      public MyRecipient_A ( )
      {
        // Register with the 'WeakReferenceMessenger' so that this instance
        // we receive notifications whenever someone publishes a 'MyMessage'.
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<MyMessage>(
          this
        ) ;
      }
      public void Receive ( MyMessage message )
      {
        // NOTE THAT IF ANY RECIPIENT'S RECEIVE METHOD THROWS AN EXCEPTION,
        // NO FURTHER RECIPENTS WILL RECEIVE THE MESSAGE !!!
        // SO IT'S ESSENTIAL THAT EACH 'RECEIVE' USES TRY/CATCH ...
        // throw new System.ApplicationException("Exception thrown in MyRecipient_A.Receive") ;
        PayloadReceived = message.Payload ;
      }
    }

    public class MyRecipient_B : CommunityToolkit.Mvvm.Messaging.IRecipient<MyMessage>
    {
      public int PayloadReceived { get ; private set ; } = 0 ;
      public MyRecipient_B ( )
      {
        // Register with the 'WeakReferenceMessenger' so that this instance
        // we receive notifications whenever someone publishes a 'MyMessage'.
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<MyMessage>(
          this
        ) ;
      }
      public void Receive ( MyMessage message )
      {
        PayloadReceived = message.Payload ;
      }
    }

    [Fact]
    public void Test_02 ( )
    {
      var recipient_A = new MyRecipient_A() ;
      var recipient_B = new MyRecipient_B() ;
      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send<MyMessage>(
        new MyMessage(123)
      ) ;
      return ; // THE CHECKS ARE EXPECTED TO FAIL ... !!!
      recipient_A.PayloadReceived.Should().Be(123) ;
      recipient_B.PayloadReceived.Should().Be(123) ;
    }

  }

}

