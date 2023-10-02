//
// RemoteChannel_read_value_blocking.cs
//

using FluentAssertions ;
using Clf.Common.ExtensionMethods ;
using static Clf.ChannelAccess.LowLevelApi.EcaCodeExtensionMethods ;
using System.Diagnostics.CodeAnalysis ;
using System.Threading.Tasks ;

namespace Clf.ChannelAccess
{

  partial class RemoteChannel
  {

    // Blocking ... NOT SAFE TO USE !!!

    private unsafe ValueInfo? GetValue_Blocking_NotSafeToUse ( )
    {

      if ( ! this.m_currentStateSnapshot.CurrentState.FieldInfoIsKnown )
      {
        throw new UsageErrorException("Channel hasn't ever been connected") ;
      }

      FieldInfo fieldInfo = this.m_currentStateSnapshot.CurrentState.FieldInfo! ;

      DbFieldType fieldType = fieldInfo.DbFieldDescriptor.DbFieldType ;

      LowLevelApi.DbRecordRequestDescriptor dbrDescriptor = new LowLevelApi.DbRecordRequestDescriptor(
        fieldType,
        ValueAccessMode
      ) ;

      int nElementsOfThatTypeWanted = fieldInfo.DbFieldDescriptor.ElementsCountOnServer ;

      //
      // For a synchronous read, we have to allocate the storage
      // into which the call to 'ca_array_get' will write the result.
      //
      // Allocate one more byte than necessary, so that we can
      // verify that it hasn't been overwritten ...
      //
      //         'necessary' bytes (8)       Additional byte
      //     #############################   |
      //   +---+---+---+---+---+---+---+---+---+
      //   | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 
      //   +---+---+---+---+---+---+---+---+---+
      //     |                               |
      //     pAllocatedMemory                pByteBeyondLastNecessaryByte
      //     9 bytes                         = pAllocatedMemory + 8
      //
      // EVEN BETTER, WE COULD WRITE A KNOWN NON-ZERO SEQUENCE OF 4 BYTES !!!
      // AND MAKE SURE THAT IT'S STILL INTACT AFTER THE API CALL HAS RETURNED.
      //
      int nNecessaryBytes = dbrDescriptor.HowManyDbRecordBytesRequiredForArraySize(
        nElementsOfThatTypeWanted
      ) ;
      int nBytesToAllocate = (
        nNecessaryBytes
      + 1
      + 1000 // !!!
      ) ;
      byte * pAllocatedMemory = (byte*) System.Runtime.InteropServices.NativeMemory.AllocZeroed(
        (nuint) nBytesToAllocate
      ) ;
      byte * pByteBeyondLastNecessaryByte = pAllocatedMemory + nNecessaryBytes ;
      // (*pByteBeyondLastNecessaryByte) = (byte) 0x37 ;

      try
      {

        //
        // Hmm, we should deal with some specific error codes ?
        //  ECA_BADCOUNT   - Requested count larger than native element count
        //  ECA_NORDACCESS - Read access denied
        //  ECA_DISCONN    - Channel is disconnected
        //

        LowLevelApi.DllFunctions.ca_array_get(
          channel                         : m_channelHandle,
          dbrType                         : dbrDescriptor.DbrType,
          nElementsOfThatTypeWanted       : nElementsOfThatTypeWanted,
          pMemoryAllocatedToHoldDbrStruct : pAllocatedMemory
        ) ;

        RaiseInterestingEventNotification(
          new ProgressNotification.ApiCallCompleted(
            $"ca_array_get with DBR type {(int)dbrDescriptor.DbrType} = {dbrDescriptor.DbrType} ; nElementsnElementsWanted={nElementsOfThatTypeWanted}"
          )
        ) ;

        // bool readSucceeded = LowLevelApi.DllFunctions.ca_pend_io(Settings.TimeoutPeriod.Value) ;
        // if ( ! readSucceeded )
        // {
        //   return null ;
        // }
         
        #if true
          // Occasionally, this fails :
          // API call failed (ca_pend_io) on #145 : ECA_MESSAGE_EVDISALLOW (#26)
          // Inappropriate call from an event handler ... ??? WTF ???
          // Also we occasionally get a timeout ... ???
          bool 
          ioCompleted = LowLevelApi.DllFunctions.ca_pend_io(
            // 10.0
            2.0
          ) ;
          if ( ioCompleted is false )
          {  
            throw new TimeoutException("Timeout waiting for ca_pend_io") ;
          }
        #else
          // Hmm, does this work any better ??
          // No, we still get timouts, intermittently ...
          // and more frequently (??) than with 'ca_pend_io()'
          LowLevelApi.DllFunctions.ca_flush_io() ;
          int nSleeps = 0 ;
          while ( ChannelAccessApi.DllFunctions.ca_test_io() is false )
          {
            System.Threading.Thread.Sleep(100) ;
            nSleeps++ ;
            if ( nSleeps == 10 )
            {
              throw new TimeoutException("Timeout waiting for ca_test_io") ;
            }
          }
        #endif
      
        (*pByteBeyondLastNecessaryByte).Should().Be(0) ;
        return LowLevelApi.Unsafe.CreateValueInfoFromDbRecordStruct(
          this,
          pAllocatedMemory,
          dbrDescriptor,
          nElementsOfThatTypeWanted,
          fieldInfo,
          out var enumFieldInfo
        ) ;
      }
      finally
      {
        System.Runtime.InteropServices.NativeMemory.Free(pAllocatedMemory) ;
      }
    }

  }

}