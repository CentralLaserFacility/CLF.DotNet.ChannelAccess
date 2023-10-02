//
// TestsUsingLongArrays.cs
//

using Xunit ;
using FluentAssertions ;

namespace Clf_ChannelAccess_Tests
{

  public class TestsUsingLongArrays
  {

    //
    // Hmm, it's really not worth testing the limits on array sizes
    // as the max capacity is determined by EPICS_CA_MAX_ARRAY_BYTES
    // which gets configured in the server (IOC), either via an
    // environment variable or via an 'st.cmd' that sets it.
    //
    // The default is 16384 ? This is also the MINIMUM value that you can configure.
    //
    // Presumably the number of 'elements' is this byte-count value
    // divided by the element size, ie by 1 or 2 or 4 or 8 ?
    //

    // [Theory]
    // [InlineData( "short_100"   , 100   )]
    // [InlineData( "short_16383" , 16383 )]
    // [InlineData( "short_16384" , 16384 )]
    // // [InlineData( "short_16385" , 16385 )]
    // public void can_access_long_arrays_of_short ( string pvBaseName, int nElements )
    // {
    //   // Just occasionally the '16383' test fails with a '10' ...
    //   // However repeating the test never seems to provoke a failure
    //   int nRepeats = 2 ;
    //   for ( int iRepeat = 0 ; iRepeat < nRepeats ; iRepeat++ )
    //   {
    //     var pv = new ProcessVariable(
    //       PvNamePrefix + pvBaseName // + ".VAL"
    //     ) ;
    //     var valueToWriteAndVerify = new short[nElements] ;
    //     for ( int i = 0 ; i < valueToWriteAndVerify.Length ; i++ )
    //     {
    //       valueToWriteAndVerify[i] = (short) ( i + 1 ) ;
    //     }
    //     pv.Write(valueToWriteAndVerify) ;
    //     object valueReadBack = null ;
    //     pv.Read(
    //       out valueReadBack
    //     ) ;
    //     // ???
    //     if ( nElements <= 16384 )
    //     {
    //       valueReadBack.Should().BeOfType( 
    //         valueToWriteAndVerify.GetType()
    //       ) ;
    //       valueReadBack.Should().BeEquivalentTo(valueToWriteAndVerify) ;
    //     }
    //     else
    //     {
    //       valueReadBack.Should().NotBeEquivalentTo(valueToWriteAndVerify) ;
    //       // valueReadBack.Should().BeNull() ;
    //     }
    //   }
    // }
    // 
    // [Theory]
    // [InlineData( "double_16384" , 16384 )]
    // [InlineData( "double_16385" , 16385 )] // ?????????
    // public void can_access_long_arrays_of_double ( string pvBaseName, int nElements )
    // {
    //   var pv = new ProcessVariable(
    //     PvNamePrefix + pvBaseName // + ".VAL"
    //   ) ;
    //   var valueToWriteAndVerify = new double[nElements] ;
    //   for ( int i = 0 ; i < valueToWriteAndVerify.Length ; i++ )
    //   {
    //     valueToWriteAndVerify[i] = (double) ( i + 1 ) ;
    //   }
    //   pv.Write(valueToWriteAndVerify) ;
    //   object valueReadBack = null ;
    //   pv.Read(
    //     out valueReadBack
    //   ) ;
    //   // ???
    //   //if ( nElements <= 16384 )
    //   {
    //     valueReadBack.Should().BeOfType( 
    //       valueToWriteAndVerify.GetType()
    //     ) ;
    //     valueReadBack.Should().BeEquivalentTo(valueToWriteAndVerify) ;
    //   }
    //   // else
    //   // {
    //   //   valueReadBack.Should().NotBeEquivalentTo(valueToWriteAndVerify) ;
    //   //   // valueReadBack.Should().BeNull() ;
    //   // }
    // }
    // 
    // [Theory]
    // [InlineData( "char_100"   , 100   )]
    // [InlineData( "char_16383" , 16383 )]
    // [InlineData( "char_16384" , 16384 )]
    // [InlineData( "char_16385" , 16385 )]
    // [InlineData( "char_32767" , 32767 )]
    // [InlineData( "char_32768" , 32768 )]
    // public void can_access_long_arrays_of_char ( string pvBaseName, int nElements )
    // {
    //   var pv = new ProcessVariable(
    //     PvNamePrefix + pvBaseName // + ".VAL"
    //   ) ;
    //   var valueToWriteAndVerify = new byte[nElements] ;
    //   for ( int i = 0 ; i < valueToWriteAndVerify.Length ; i++ )
    //   {
    //     valueToWriteAndVerify[i] = (byte) ( i + 1 ) ;
    //   }
    //   pv.Write(valueToWriteAndVerify) ;
    //   object valueReadBack = null ;
    //   pv.Read(
    //     out valueReadBack
    //   ) ;
    //   //if ( nElements <= 16384 )
    //   {
    //     valueReadBack.Should().BeOfType( 
    //       valueToWriteAndVerify.GetType()
    //     ) ;
    //     valueReadBack.Should().BeEquivalentTo(valueToWriteAndVerify) ;
    //   }
    //   //else
    //   //{
    //   //  valueReadBack.Should().NotBeEquivalentTo(valueToWriteAndVerify) ;
    //   //  // valueReadBack.Should().BeNull() ;
    //   //}
    // }

  }

}
