//
// MiscellaneousTests.cs
//

using Xunit ;
using FluentAssertions ;
using System.Threading.Tasks ;
using Clf.ChannelAccess.ExtensionMethods ;
using Clf.ChannelAccess;

namespace Clf_ChannelAccess_Tests
{

  public class MiscellaneousTests
  {

    public MiscellaneousTests ( )
    {
      Clf.ChannelAccess.Settings.WhichDllsToUse = (
        // Clf.ChannelAccess.WhichDllsToUse.DaresburyReleaseDlls 
        Clf.ChannelAccess.WhichDllsToUse.ClfDebugDlls 
      ) ;
    }

    [Fact]
    public void IsValidPvIdentifier_tests_give_expected_results ( )
    {
      // using var _ = new OneTestRunningVerifier() ;
      Clf.ChannelAccess.Helpers.IsValidChannelName_tests_give_expected_results() ;
    }

    [Theory]
    // Scalar values
    [InlineData("s39"      , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string   ) , false , ""    , false , 0 )]
    [InlineData("i16"      , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short    ) , false , ""    , false , 0 )]
    [InlineData("f32"      , Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32  , typeof( float    ) , false , ""    , false , 0 )]
    [InlineData("enum:a,b" , Clf.ChannelAccess.DbFieldType.DBF_ENUM_i16   , typeof( short    ) , true  , "a,b" , false , 0 )]
    [InlineData("byte"     , Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte  , typeof( byte     ) , false , ""    , false , 0 )]
    [InlineData("i32"      , Clf.ChannelAccess.DbFieldType.DBF_LONG_i32   , typeof( int      ) , false , ""    , false , 0 )]
    [InlineData("f64"      , Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64 , typeof( double   ) , false , ""    , false , 0 )]
    // Array values                                                                 
    [InlineData("s39:4"    , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string[] ) , false , ""    , true  , 4 )]
    [InlineData("i16:4"    , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 4 )]
    [InlineData("f32:4"    , Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32  , typeof( float[]  ) , false , ""    , true  , 4 )]
    [InlineData("byte:4"   , Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte  , typeof( byte[]   ) , false , ""    , true  , 4 )]
    [InlineData("i32:4"    , Clf.ChannelAccess.DbFieldType.DBF_LONG_i32   , typeof( int[]    ) , false , ""    , true  , 4 )]
    [InlineData("f64:4"    , Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64 , typeof( double[] ) , false , ""    , true  , 4 )]
    // Array values                                                                                                    
    [InlineData("s39[4]"   , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string[] ) , false , ""    , true  , 4 )]
    [InlineData("i16[4]"   , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 4 )]
    [InlineData("f32[4]"   , Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32  , typeof( float[]  ) , false , ""    , true  , 4 )]
    [InlineData("byte[4]"  , Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte  , typeof( byte[]   ) , false , ""    , true  , 4 )]
    [InlineData("i32[4]"   , Clf.ChannelAccess.DbFieldType.DBF_LONG_i32   , typeof( int[]    ) , false , ""    , true  , 4 )]
    [InlineData("f64[4]"   , Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64 , typeof( double[] ) , false , ""    , true  , 4 )]
    public void CanCreateDbFieldDescriptorsFromStrings ( 
      string                        encodedString,
      Clf.ChannelAccess.DbFieldType dbFieldType,
      System.Type                   type,
      bool                          isEnumField,
      string                        enumValuesExpected,
      bool                          isArray,
      int                           nElements
    ) {
      // Clf.ChannelAccess.DbFieldDescriptor? result ;
      bool createSucceeded = Clf.ChannelAccess.DbFieldDescriptor.TryCreateFromEncodedString(
        encodedString,
        // out Clf.ChannelAccess.DbFieldDescriptor? dbFieldDescriptor
        out var dbFieldDescriptor
      ) ;
      createSucceeded.Should().BeTrue() ;
      dbFieldDescriptor.Should().NotBeNull() ;
      // Hmm, we're getting a spurious 'might be null' warning here ... ???
      // AHA, COULD FIX THIS BY INVOKING Assert() ...
      dbFieldDescriptor.DbFieldType.Should().Be(dbFieldType) ;
      dbFieldDescriptor.GetFieldDataType().Should().Be(type) ;
      dbFieldDescriptor.IsArray.Should().Be(isArray) ;
      if ( isArray )
      {
        dbFieldDescriptor.ElementsCountOnServer.Should().Be(nElements) ;
      }
      dbFieldDescriptor.IsEnumField(out var enumValues).Should().Be(isEnumField) ;
      if ( isEnumField )
      {
        enumValues.Should().BeEquivalentTo(enumValuesExpected.Split(',')) ;
      }
    }

    [Theory]
    // String value, specifying initial content as empty string
    [InlineData("s39|''"   , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string   ) , false , ""    , false , 0 , "" )]
    // Scalar values, specifying initial content
    [InlineData("s39|abc"  , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string   ) , false , ""    , false , 0 , "abc" )]
    [InlineData("i16|123"  , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short    ) , false , ""    , false , 0 , "123" )]
    [InlineData("f32|123"  , Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32  , typeof( float    ) , false , ""    , false , 0 , "123" )]
    [InlineData("enum:a,b" , Clf.ChannelAccess.DbFieldType.DBF_ENUM_i16   , typeof( short    ) , true  , "a,b" , false , 0         )]
    [InlineData("byte|123" , Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte  , typeof( byte     ) , false , ""    , false , 0 , "123" )]
    [InlineData("i32|123"  , Clf.ChannelAccess.DbFieldType.DBF_LONG_i32   , typeof( int      ) , false , ""    , false , 0 , "123" )]
    [InlineData("f64|123"  , Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64 , typeof( double   ) , false , ""    , false , 0 , "123" )]
    // Scalar values, not specifying initial content
    [InlineData("s39"      , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string   ) , false , ""    , false , 0 , null )]
    [InlineData("i16"      , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short    ) , false , ""    , false , 0 , null )]
    [InlineData("f32"      , Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32  , typeof( float    ) , false , ""    , false , 0 , null )]
    // [InlineData("enum"     , Clf.ChannelAccess.DbFieldType.DBF_ENUM_i16   , typeof( short    ) , true  , "a,b" , false , 0 , null )]
    [InlineData("byte"     , Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte  , typeof( byte     ) , false , ""    , false , 0 , null )]
    [InlineData("i32"      , Clf.ChannelAccess.DbFieldType.DBF_LONG_i32   , typeof( int      ) , false , ""    , false , 0 , null )]
    [InlineData("f64"      , Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64 , typeof( double   ) , false , ""    , false , 0 , null )]
    // Array values, not specifying initial content                                                                                                   
    [InlineData("s39[4]"   , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string[] ) , false , ""    , true  , 4 )]
    [InlineData("i16[4]"   , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 4 )]
    [InlineData("f32[4]"   , Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32  , typeof( float[]  ) , false , ""    , true  , 4 )]
    [InlineData("byte[4]"  , Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte  , typeof( byte[]   ) , false , ""    , true  , 4 )]
    [InlineData("i32[4]"   , Clf.ChannelAccess.DbFieldType.DBF_LONG_i32   , typeof( int[]    ) , false , ""    , true  , 4 )]
    [InlineData("f64[4]"   , Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64 , typeof( double[] ) , false , ""    , true  , 4 )]
    // Array values, specifying initial content, using ',' separator                                                                                                
    [InlineData("s39[5]|a,b,c,d,e"   , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string[] ) , false , ""    , true  , 5 , "a,b,c,d,e"   , new string[]{"a","b","c","d","e"} )]
    [InlineData("s39[5]|a,b,..."     , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string[] ) , false , ""    , true  , 5 , "a,b,..."     , new string[]{"a","b","a","b","a"} )]
    [InlineData("i16[5]|1,2,3,4,5"   , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 5 , "1,2,3,4,5"   , new short []{1,2,3,4,5}           )]
    [InlineData("i16[5]|1,2,3"       , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 5 , "1,2,3"       , new short []{1,2,3}               )]
    [InlineData("i16[5]|1,2,..."     , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 5 , "1,2,..."     , new short []{1,2,1,2,1}           )]
    [InlineData("i16[5]|1,..."       , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 5 , "1,..."       , new short []{1,1,1,1,1}           )]
    // Array values, specifying initial content, using ' ' separator                                                                                                       
    [InlineData("s39[5]|a b c d e"   , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string[] ) , false , ""    , true  , 5 , "a b c d e"   , new string[]{"a","b","c","d","e"} )]
    [InlineData("s39[5]|a b ..."     , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string[] ) , false , ""    , true  , 5 , "a b ..."     , new string[]{"a","b","a","b","a"} )]
    [InlineData("i16[5]|1 2 3 4 5"   , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 5 , "1 2 3 4 5"   , new short []{1,2,3,4,5}           )]
    [InlineData("i16[5]|1 2 3"       , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 5 , "1 2 3"       , new short []{1,2,3}               )]
    [InlineData("i16[5]|1 2 ..."     , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 5 , "1 2 ..."     , new short []{1,2,1,2,1}           )]
    [InlineData("i16[5]|1 ..."       , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 5 , "1 ..."       , new short []{1,1,1,1,1}           )]
    [InlineData("s39[5]|a_a b c d e" , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string[] ) , false , ""    , true  , 5 , "a_a b c d e" , new string[]{"a a","b","c","d","e"} )]
    //[InlineData("i16[5]|1,2,3,4,5"   , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 4 , "1,2,3,4" , new short []{1,2,3,4,5}           )]
    // [InlineData("f32[5]|1,2"       , Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32  , typeof( float[]  ) , false , ""    , true  , 4 , "1,2"     , null )]
    // [InlineData("byte[5]|1,2,..."  , Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte  , typeof( byte[]   ) , false , ""    , true  , 4 , "1,2,..." , null )]
    // [InlineData("i32[5]|1,..."     , Clf.ChannelAccess.DbFieldType.DBF_LONG_i32   , typeof( int[]    ) , false , ""    , true  , 4 , "1,..."   , null )]
    // [InlineData("f64[5]|1"         , Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64 , typeof( double[] ) , false , ""    , true  , 4 , "1"       , null )]
    // Array values - this syntax also works, but we don't document it !!                                                               
    [InlineData("s39:4"    , Clf.ChannelAccess.DbFieldType.DBF_STRING_s39 , typeof( string[] ) , false , ""    , true  , 4 )]
    [InlineData("i16:4"    , Clf.ChannelAccess.DbFieldType.DBF_SHORT_i16  , typeof( short[]  ) , false , ""    , true  , 4 )]
    [InlineData("f32:4"    , Clf.ChannelAccess.DbFieldType.DBF_FLOAT_f32  , typeof( float[]  ) , false , ""    , true  , 4 )]
    [InlineData("byte:4"   , Clf.ChannelAccess.DbFieldType.DBF_CHAR_byte  , typeof( byte[]   ) , false , ""    , true  , 4 )]
    [InlineData("i32:4"    , Clf.ChannelAccess.DbFieldType.DBF_LONG_i32   , typeof( int[]    ) , false , ""    , true  , 4 )]
    [InlineData("f64:4"    , Clf.ChannelAccess.DbFieldType.DBF_DOUBLE_f64 , typeof( double[] ) , false , ""    , true  , 4 )]
    public void CanCreateRecordDescriptorsFromStrings ( 
      string                        encodedString,
      Clf.ChannelAccess.DbFieldType dbFieldType,
      System.Type                   type,
      bool                          isEnumField,
      string                        enumValuesExpected,
      bool                          isArray,
      int                           nElements,
      string?                       initialValueAsString = null,
      object?                       initialValueAsObject = null
    ) {
      string pvName = "myPv" ;
      var channelDescriptor = Clf.ChannelAccess.ChannelDescriptor.FromEncodedString(
        pvName + '|' + encodedString
      ) ;
      channelDescriptor.ChannelName.AsValidatedName().ShortName_OmittingVAL.Should().Be(pvName) ;
      var dbFieldDescriptor = channelDescriptor.DbFieldDescriptor ;
      dbFieldDescriptor.DbFieldType.Should().Be(dbFieldType) ;
      dbFieldDescriptor.GetFieldDataType().Should().Be(type) ;
      dbFieldDescriptor.IsArray.Should().Be(isArray) ;
      if ( isArray )
      {
        dbFieldDescriptor.ElementsCountOnServer.Should().Be(nElements) ;
      }
      dbFieldDescriptor.IsEnumField(out var enumValues).Should().Be(isEnumField) ;
      if ( isEnumField )
      {
        enumValues.Should().BeEquivalentTo(enumValuesExpected.Split(',')) ;
      }
      if ( initialValueAsString != null )
      {
        channelDescriptor.InitialValueAsString.Should().Be(initialValueAsString) ;
      }
      if ( initialValueAsObject != null )
      {
        channelDescriptor.InitialValueAsString.Should().NotBeNull() ;
        channelDescriptor.InitialValueAsString.Should().Be(initialValueAsString) ;
        dbFieldDescriptor.TryParseValue(
          channelDescriptor.InitialValueAsString!,
          out var createdValue
        ).Should().BeTrue() ;
        createdValue.Should().BeEquivalentTo(initialValueAsObject) ;
      }
    }

    // Activate this test specifically, 
    // to ensure that ThinIoc.Server can be launched
    // [Fact]
    // public void CanLaunchThinIocServer ( )
    // {
    //   var runner = new Clf.ChannelAccess.ThinIocProcess(
    //     Clf.Common.PathUtilities.RootDirectoryHoldingDotNetGithubRepos
    //   + @"DotNet.ChannelAccess\Clf.ChannelAccess.Tests\xx.db"
    //     // @"C:\tmp\xx.db"
    //   ) ;
    //   var servers = System.Diagnostics.Process.GetProcessesByName("Clf.ThinIoc.Server") ;
    //   runner.Dispose() ;
    // }

    [Fact]
    public void PutValueResult_WorksAsExpected ( )
    {
      {
        var successA = Clf.ChannelAccess.PutValueResult.Success ;
        var successB = Clf.ChannelAccess.PutValueResult.Success ;
        var timeout  = Clf.ChannelAccess.PutValueResult.Timeout ;
        (
          successA == successB
        ).Should().BeTrue() ;
        (
          successA != timeout
        ).Should().BeTrue() ;
      }
    }

    [Fact]
    public void CreateIncrementedValue_WorksAsExpected ( )
    {
      // Scalar values
      Clf.ChannelAccess.Helpers.CreateIncrementedValue(1).Should().Be(2) ;
      Clf.ChannelAccess.Helpers.CreateIncrementedValue(1.0).Should().Be(2.0) ;
      Clf.ChannelAccess.Helpers.CreateIncrementedValue("a").Should().Be("a+1") ;
      // Array values
      Clf.ChannelAccess.Helpers.CreateIncrementedValue( new[]{1,1,1}       ).Should().BeEquivalentTo( new[]{2,2,2}       ) ;
      Clf.ChannelAccess.Helpers.CreateIncrementedValue( new[]{1.0,1.0,1.0} ).Should().BeEquivalentTo( new[]{2.0,2.0,2.0} ) ;
      Clf.ChannelAccess.Helpers.CreateIncrementedValue( new[]{"a","a"}     ).Should().BeEquivalentTo( new[]{"a+1","a+1"} ) ;
    }

    [Fact]
    public void ComparingChannelDescriptorsSpecifyingEnumValues_WorksAsExpected ( )
    {
      var a = ChannelDescriptor.FromEncodedString($"MyPvName|enum:Mode1,Mode2,Mode3,Mode4|2") ;
      var b = ChannelDescriptor.FromEncodedString($"MyPvName|enum:Mode1,Mode2,Mode3,Mode4|2") ;
      a.Equals(b).Should().BeTrue() ;
    }

    [Fact]
    public void ComparingDbFieldDescriptors_WorksAsExpected ( )
    {
      {
        var a = DbFieldDescriptor.CreateFromEncodedString("i16") ;
        var b = DbFieldDescriptor.CreateFromEncodedString("i16") ;
        a.Equals(b).Should().BeTrue() ;
      }
      {
        var a = DbFieldDescriptor.CreateFromEncodedString("i16:4") ;
        var b = DbFieldDescriptor.CreateFromEncodedString("i16:4") ;
        a.Equals(b).Should().BeTrue() ;
      }
      {
        var a = DbFieldDescriptor.CreateFromEncodedString("enum:Mode1,Mode2,Mode3,Mode4") ;
        var b = DbFieldDescriptor.CreateFromEncodedString("enum:Mode1,Mode2,Mode3,Mode4") ;
        a.Equals(b).Should().BeTrue() ;
      }
      {
        var a = DbFieldDescriptor.CreateFromEncodedString("enum:Mode1") ;
        var b = DbFieldDescriptor.CreateFromEncodedString("enum:Mode1") ;
        a.Equals(b).Should().BeTrue() ;
      }
    }

  }

}

