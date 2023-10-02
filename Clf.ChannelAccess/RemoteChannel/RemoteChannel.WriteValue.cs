//
// RemoteChannel_write_value.cs
//

using FluentAssertions ;

namespace Clf.ChannelAccess
{

  partial class RemoteChannel
  {

    public async override System.Threading.Tasks.Task<PutValueResult> PutValueAsync ( object valueToWrite ) 
    {

      // if ( valueToWrite is System.Collections.IEnumerable sequence )
      // {
      //   // Hmm, we could convert to an array ???
      // }

      //
      // Hmm, the channel needn't actually be active ; we could
      // queue up the write request so that we send the value
      // when the channel eventually does get connected.
      // That would work regardless of whether we are going to be
      // waiting for the callback ...
      //
      // BUT - actually that isn't useful, given that we have
      // the 'GetOrCreate()' mechanism ... we wouldn't necessarily
      // want to always write a value on first connect ?
      // 

      m_writeCompletedEvent.Reset() ;
      InitiateWrite ( 
        valueToWrite,
        raiseEventOnWriteCompletion : true
      ) ;

      RaiseInterestingEventNotification(
        new ProgressNotification.WaitingForEvent("writeCompleted")
      ) ;
      try
      {
        bool writeCompleted = await m_writeCompletedEvent.Task.WaitAsync(
          Settings.CommsTimeoutPeriodInEffect
        ).ConfigureAwait(false) ; ;
        RaiseInterestingEventNotification(
          writeCompleted
          ? new CommsNotification.WriteSucceeded(
            InternalHelpers.GetChannelValueSummaryAsFriendlyString(valueToWrite)
          )
          : new CommsNotification.WriteFailed(
            InternalHelpers.GetChannelValueSummaryAsFriendlyString(valueToWrite)
          )
        ) ;
        return ( 
          writeCompleted
          ? PutValueResult.Success
          : PutValueResult.RejectedByServer 
        ) ;
      }
      catch ( System.TimeoutException x )
      {
        x.ToString(); //TODO: Handle exception in Log... suppressing warning
        RaiseInterestingEventNotification(
          new CommsNotification.TimeoutExpired(
            "writeCompleted"
          )
        ) ;
        return PutValueResult.Timeout ;
      }
    }

    public async override System.Threading.Tasks.Task<PutValueResult> PutValueAckAsync ( object valueToWrite )
    {
      // Hmm, race condition here ???
      if ( m_valueUpdateReceived.IsSet )
      {
        m_valueUpdateReceived.Reset() ;
      }
      // Submit our request to  change the value 
      PutValueResult result = await PutValueAsync(valueToWrite).ConfigureAwait(false) ;
      if ( result != PutValueResult.Success )
      {
        // Our request didn't succeed, so there's
        // no point in waiting for the acknowlegement ...
        return result ;
      }
      try
      {
        // Hmm, is this bullet proof ??
        if ( m_valueAcquiredEvent.IsNotSet )
        {
          if ( IsSubscribedToValueChangeCallbacks )
          {
            // We haven't yet received the event that tells us
            // that the 'initial' value has been acquired,
            // so lets wait for that. Hmm, the assumption is that
            // the value we receive will be the one we wrote ??
            await m_valueAcquiredEvent.Task.WaitAsync(
              Settings.CommsTimeoutPeriodInEffect
            ).ConfigureAwait(false) ;
          }
          else
          {
            // We haven't subscribed to value change callbacks,
            // so all we can do is make a request ...
            await GetValueAsync().ConfigureAwait(false) ;
          }
        }
        else
        {
          // Wait for a 'subsequent' event ...
          await m_valueUpdateReceived.Task.WaitAsync(
            Settings.CommsTimeoutPeriodInEffect
          ).ConfigureAwait(false) ;
        }
        return PutValueResult.Success ;
      }
      catch ( System.TimeoutException x )
      {
        x.ToString(); //TODO: Handle exception in Log... suppressing warning
        RaiseInterestingEventNotification(
          new CommsNotification.TimeoutExpired(
            "writeAckCompleted"
          )
        ) ;
        return PutValueResult.Timeout ;
      }
    }

    public override void PutValue ( object valueToWrite ) 
    {
      InitiateWrite ( 
        valueToWrite                : valueToWrite,
        raiseEventOnWriteCompletion : false
      ) ;
    }

    public void PutValueParsedFromString ( string stringValueToParse ) 
    {
      if ( FieldInfo is null )
      {
        // Inappropriate API call - we haven't yet connected !!!
        // TODO : SHOULD LOG THIS AS A USAGE ERROR ...
      }
      else 
      {
        if ( 
          FieldInfo.DbFieldDescriptor.TryParseValue(
            stringValueToParse,
            out object? value
          )
        ) {
          PutValue(value) ;
        }
        else
        {
          // Inappropriate API call - conversion failed !
          // TODO : SHOULD LOG THIS AS A USAGE ERROR ...
        }
      }
    }

    private void InitiateWrite ( 
      object valueToWrite,
      bool   raiseEventOnWriteCompletion
    ) {

      RaiseInterestingEventNotification(
        new ProgressNotification.ActionNotification(
          $"InitiateWrite : {
            InternalHelpers.GetChannelValueSummaryAsFriendlyString(valueToWrite)
          }"
        )
      ) ;

      if ( 
        LowLevelApi.DllFunctions.ca_state(m_channelHandle) 
        != LowLevelApi.ChannelStateQueryResult.CurrentlyConnected 
      ) {
        RaiseInterestingEventNotification(
          new AnomalyNotification.UnexpectedApiCall(
            $"InitiateWrite called when not connected"
          )
        ) ;
        return ;
      }

      FieldInfo!.Should().NotBeNull() ;
      FieldInfo!.DbFieldDescriptor.IsWriteable.Should().BeTrue() ;

      var fieldType                = FieldInfo!.DbFieldDescriptor.DbFieldType ;
      int nElementsDefinedOnServer = FieldInfo!.DbFieldDescriptor.ElementsCountOnServer ;

      LowLevelApi.DllFunctions.ca_element_count(m_channelHandle).Should().Be(
        nElementsDefinedOnServer
      ) ;

      if ( nElementsDefinedOnServer == 1 )
      {
        // We'll write a single value ...
        valueToWrite.GetType().IsArray.Should().BeFalse() ;
        switch ( fieldType )
        {
        case DbFieldType.DBF_DOUBLE_f64:
          WriteAsScalarValue<double>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_SHORT_i16:
          WriteAsScalarValue<short>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_FLOAT_f32:
          WriteAsScalarValue<float>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_ENUM_i16:
          WriteAsScalarValue<short>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_CHAR_byte:
          WriteAsScalarValue<byte>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_LONG_i32:
          WriteAsScalarValue<int>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_STRING_s39:
          WriteAsScalarValue<LowLevelApi.ByteArray_40>(
            new LowLevelApi.ByteArray_40(
              (string) valueToWrite
            )
          ) ;
          break ;
        default:
          throw fieldType.AsUnexpectedEnumValueException() ;
        }
      }
      else
      {
        // We'll write an array ...
        valueToWrite.GetType().IsArray.Should().BeTrue() ;
        switch ( fieldType )
        {
        case DbFieldType.DBF_DOUBLE_f64:
          WriteAsArrayValue<double>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_SHORT_i16:
          WriteAsArrayValue<short>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_FLOAT_f32:
          WriteAsArrayValue<float>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_ENUM_i16:
          WriteAsArrayValue<short>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_CHAR_byte:
          WriteAsArrayValue<byte>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_LONG_i32:
          WriteAsArrayValue<int>(
            valueToWrite
          ) ;
          break ;
        case DbFieldType.DBF_STRING_s39:
          // Writing an array of strings is a bit complicated.
          // We need to create a similarly sized array of the
          // 'ByteArray_40' structures, and populate each element
          // of that with characters from the incoming string-array.
          string[] s_array = (string[])valueToWrite ;
          var s39_array = new LowLevelApi.ByteArray_40[s_array.Length] ;
          for (int iString = 0 ; iString < s_array.Length ; iString++)
          {
            s39_array[iString] = new LowLevelApi.ByteArray_40(
              s_array[iString]
            ) ;
          }
          WriteAsArrayValue<LowLevelApi.ByteArray_40>(
            s39_array
          ) ;
          break ;
        default:
          throw fieldType.AsUnexpectedEnumValueException() ;
        }
      }

      LowLevelApi.DllFunctions.ca_flush_io() ;

      //
      // Local functions -------------------------------------------------
      //
      // We supply an appropriate data block according to the 'dbfType' we're writing :
      //
      //  DBF_STRING
      //    Pointer to array of 'byte', of length '40*nElementsOfThatTypeToWrite'
      //
      //  DBF_SHORT 
      //    Pointer to array of 'short', of length 'nElementsOfThatTypeToWrite'
      //
      //  DBF_FLOAT 
      //    Pointer to array of 'float', of length 'nElementsOfThatTypeToWrite'
      //
      //  DBF_ENUM  
      //    Pointer to array of 'short', of length 1
      //
      //  DBF_CHAR  
      //    Pointer to array of 'byte', of length 'nElementsOfThatTypeToWrite'
      //
      //  DBF_LONG  
      //    Pointer to array of 'int', of length 'nElementsOfThatTypeToWrite'
      //
      //  DBF_DOUBLE
      //    Pointer to array of 'double', of length 'nElementsOfThatTypeToWrite'
      //
      // We're assuming that the function we're calling, 'ca_array_put()',
      // makes an internal copy of the data we pass in, and that when we return
      // from this function the pointer we passed in doesn't need to remain valid.
      //

      unsafe void WriteAsScalarValue<T> ( 
        object valueAsObject
      )
      where T : unmanaged
      {
        FieldInfo!.DbFieldDescriptor.ElementsCountOnServer.Should().Be(1) ;
        T value = (T) valueAsObject ;
        void * pValue = &value ;
        if ( raiseEventOnWriteCompletion )
        {
          LowLevelApi.DllFunctions.ca_array_put_callback(
            channel             : m_channelHandle,
            dbFieldDescriptor   : FieldInfo!.DbFieldDescriptor,
            pValueToWrite       : pValue,
            valueUpdateCallback : DllCallbackHandlers.WriteCompletedEventCallbackHandler,
            userArg             : this.ChannelIdentifier
          ) ;
          RaiseInterestingEventNotification(
            new ProgressNotification.ApiCallCompleted("ca_array_put_callback")
          ) ;
        }
        else
        {
          LowLevelApi.DllFunctions.ca_array_put(
            channel           : m_channelHandle,
            dbFieldDescriptor : FieldInfo!.DbFieldDescriptor,
            pValueToWrite     : pValue
          ) ;
          RaiseInterestingEventNotification(
            new ProgressNotification.ApiCallCompleted("ca_array_put (no callback)")
          ) ;
        }
      }

      unsafe void WriteAsArrayValue<T> (
        object arrayToWrite_asObject
      ) 
      where T : unmanaged
      {
        var arrayToWrite_T = (T[]) arrayToWrite_asObject ;
        int nArrayElementsToWrite = arrayToWrite_T.Length ;
        nArrayElementsToWrite.Should().BeGreaterOrEqualTo(1) ;
        fixed ( T * pFirstArrayElement = arrayToWrite_T )
        {
          if ( raiseEventOnWriteCompletion )
          {
            LowLevelApi.DllFunctions.ca_array_put_callback(
              channel             : m_channelHandle,
              dbFieldDescriptor   : FieldInfo!.DbFieldDescriptor,
              pValueToWrite       : pFirstArrayElement,
              valueUpdateCallback : DllCallbackHandlers.WriteCompletedEventCallbackHandler,
              userArg             : this.ChannelIdentifier
            ) ;
            RaiseInterestingEventNotification(
              new ProgressNotification.ApiCallCompleted("ca_array_put_callback")
            ) ;
          }
          else
          {
            LowLevelApi.DllFunctions.ca_array_put(
              channel           : m_channelHandle,
              dbFieldDescriptor : FieldInfo!.DbFieldDescriptor,
              pValueToWrite     : pFirstArrayElement
            ) ;
            RaiseInterestingEventNotification(
              new ProgressNotification.ApiCallCompleted("ca_array_put (no callback)")
            ) ;
          }
        }
      }

    }


  }

}