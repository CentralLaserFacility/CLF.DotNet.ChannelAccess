//
// ApiWrapper.Delegates.cs
//

using System ;
using System.Runtime.InteropServices ;

namespace Clf.ChannelAccess.LowLevelApi
{

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ConnectionCallback ( ConnectionStatusChangedEventArgs args ) ;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ValueUpdateCallback ( ValueUpdateNotificationEventArgs args ) ;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ExceptionHandlerCallback ( ExceptionHandlerEventArgs args ) ;

    // Necessary for 'ca_replace_printf_handler',
    // but problematic to make it work cross-platform ...

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void PrintfCallback ( string format, ArgIterator va_list ) ;

}
