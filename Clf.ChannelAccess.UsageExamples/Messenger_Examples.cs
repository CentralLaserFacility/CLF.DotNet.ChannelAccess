//
// MessengerExperiments_01.cs
//

using System.Collections.Generic ;
using static CommunityToolkit.Mvvm.Messaging.IMessengerExtensions ;
using FluentAssertions ;

namespace Clf_ChannelAccess_UsageExamples
{

  public record MyMessage ( int Payload ) ;

  public static class MessengerExample
  {

    /*
 
        View  <------------>  ViewModel  <--------------->  Model
                               |
                              exposes various Properties,
                              and a 'PropertyChanged' event

        Each View 'binds' its visual widgets to Properties on the ViewModel
        Typically, a View hooks into the 'PropertyChanged' event in order
        to be informed whenever there's a change in a particular Property.
        This is an example of the 'Observer' design pattern.

        ViewModel.PropertyChanged += MyView.HandlePropertyChanged(propertyName) ;

        ...... then when the View is no longer required ...

        ViewModel.PropertyChanged -= MyView.HandlePropertyChanged(propertyName) ;
        // Must do this otherwise the View will not become eligible for Garbage Collection,
        // since the ViewModel's event will still have a reference to the ViewModel ...

        In this scheme there's a direct link between the publisher and each 'subscriber'
        
        This is the traditional mechanism used in 'MVVM' frameworks 

        =========================================================================

        Also very common and useful : a 'publish/subscribe' system ...

        AAA        BBB         CCC         DDD         EEE
         |          |           |           |           |
         |          |           |           |           |
       +-----------------------------------------------------+
       |  Globally accessible object to distribute messages  |
       +-----------------------------------------------------+

       Known as an 'event bus', or 'mediator, or 'messenger'.
       Just another variant of the 'Observer' pattern.

       Anyone can 'Send a Message' via the Messenger (ie be a Publisher).
       Anyone can 'subscribe' to messages of a particular type (ie be a Recipient).

       When a message is Sent, it gets broadcast to all Recipients.

       Note : NO DIRECT LINKS BETWEEN PUBLISHERS AND RECIPIENTS !!!
       This removes the obligation to un-subscribe (-=) as the
       link from publisher-to-recipient can be a Weak Reference,
       one that doesn't prevent Garbage Collection.

       Disadvantage - can be harder to debug !

    */

    // https://www.youtube.com/watch?v=ZwZvQIuX0AU

    // https://xamlbrewer.wordpress.com/2020/11/16/a-lap-around-the-microsoft-mvvm-toolkit/
    // https://docs.microsoft.com/en-us/windows/communitytoolkit/mvvm/messenger
    // https://www.twitch.tv/videos/707332546 (Messenger is about 30 minutes in)

    public class MyRecipient_A : CommunityToolkit.Mvvm.Messaging.IRecipient<MyMessage>
    {
      public int PayloadReceived { get ; private set ; } = 0 ;
      public MyRecipient_A ( )
      {
        // Register with the 'WeakReferenceMessenger' so that this instance
        // will receive notifications whenever someone publishes a 'MyMessage'.
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<MyMessage>(
          this
        ) ;
      }
      public void Receive ( MyMessage message )
      {
        PayloadReceived = message.Payload ;
      }
    }

    public class MyRecipient_B : CommunityToolkit.Mvvm.Messaging.IRecipient<MyMessage>
    {
      public int PayloadReceived { get ; private set ; } = 0 ;
      public MyRecipient_B ( )
      {
        // Register with the 'WeakReferenceMessenger' so that this instance
        // will receive notifications whenever someone publishes a 'MyMessage'.
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<MyMessage>(
          this
        ) ;
      }
      public void Receive ( MyMessage message )
      {
        // NOTE THAT IF ANY RECIPIENT'S RECEIVE METHOD THROWS AN EXCEPTION,
        // IT WILL PROPAGATE TO THE PLACE WHERE THE 'Send' OCCURRED
        // AND NO FURTHER RECIPENTS WILL RECEIVE THE MESSAGE !!!
        // SO IT'S ESSENTIAL THAT EACH 'RECEIVE' USES TRY/CATCH ...
        // throw new System.ApplicationException("Exception thrown in MyRecipient_A.Receive") ;
        PayloadReceived = message.Payload ;
      }
    }

    public static void Run ( )
    {
      var recipient_A = new MyRecipient_A() ;
      var recipient_B = new MyRecipient_B() ;
      var recipient_B2 = new MyRecipient_B() ;
      CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send<MyMessage>(
        new MyMessage(123)
      ) ;
      recipient_A.PayloadReceived.Should().Be(123) ;
      recipient_B.PayloadReceived.Should().Be(123) ;
      recipient_B2.PayloadReceived.Should().Be(123) ;
    }

  }

}

