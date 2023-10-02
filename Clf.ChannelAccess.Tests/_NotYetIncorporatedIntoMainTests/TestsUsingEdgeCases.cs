//
// TestsUsingEdgeCases.cs
//

using Xunit ;
using FluentAssertions ;

namespace ClfSharp_ChannelAccess_Tests
{

  // The record name can be composed of the following characters:
  //   a-z A-Z 0-9 _ - + : [ ] < > ;

  public class TestsUsingEdgeCases
  {

    #if false

    [Theory]
    [InlineData( "abc_ABC_123-+[]<>;_max_length_of_pv_name_is_60_characters"  , (short) 123 )]
    // [InlineData( "abc_ABC_123-+[]<>;_max_length_of_pv_name_is_60_charactersX" , (short) 123 )]
    public void can_access_pv_with_60_character_name ( 
      string pvBaseName, 
      object valueToWriteAndVerify 
    ) {
      foreach ( var mode in System.Enum.GetValues<PvMode_TimeOrControl>() )
      {
        var pv = new ProcessVariable(
          PvNamePrefix + pvBaseName // + ".VAL"
        ) ;
        pv.Mode = mode ;
        pv.Write(valueToWriteAndVerify) ;
        object valueReadBack = null ;
        pv.Read(
          out valueReadBack
        ) ;
        valueReadBack.Should().BeOfType(
          valueToWriteAndVerify.GetType()
        ) ;
        valueReadBack.Should().BeEquivalentTo(valueToWriteAndVerify) ;
      }
    }

    // [Fact]
    // public void Test_writing_short_from_string ( )
    // {
    //   var pv = new ChannelAccess.ProcessVariable(
    //     "xx:one_short.VAL"
    //   ) ;
    //   object valueRead = null ;
    //   // Hmm, in the original version we've got to do a 'read' before a 'write'
    //   // because otherwise the channel won't have been set up ...
    //   // pv.Read(
    //   //   ref valueRead
    //   // ) ;
    //   pv.Write((short)3) ;
    //   pv.Read(
    //     ref valueRead
    //   ) ;
    //   valueRead.Should().Be(3) ;
    //   pv.Write("4") ;
    //   pv.Read(
    //     ref valueRead
    //   ) ;
    //   valueRead.Should().Be(4) ;
    // }
    
    [Theory]
    [InlineData( "four_shorts_not_written", typeof(short[]) )]
    public void accessing_unwritten_value_gives_random_result ( 
      string      pvBaseName, 
      System.Type valueType 
    ) {
      foreach ( var mode in System.Enum.GetValues<PvMode_TimeOrControl>() )
      {
        var pv = new ProcessVariable(
          PvNamePrefix + pvBaseName // + ".VAL"
        ) ;
        pv.Mode = mode ;
        object valueReadBack = null ;
        pv.Read(
          out valueReadBack
        ) ;
        // We expect to get back a short[4],
        // whose elements are random ...
        valueReadBack.Should().BeOfType(
          valueType
        ) ;
        int nElementsRead = (
          (System.Array) valueReadBack
        ).Length ;
        nElementsRead.Should().Be(4) ;
        // Since we have a 'waveform' record,
        // there should be a read-only 'NORD' field
        // and its value should be zero ...
        var pv_NORD = new ProcessVariable(
          PvNamePrefix + pvBaseName + ".NORD"
        ) ;
        pv_NORD.Read(
          out valueReadBack
        ) ;
        valueReadBack.Should().Be(0) ;
      }
    }

    #endif

  }

}
