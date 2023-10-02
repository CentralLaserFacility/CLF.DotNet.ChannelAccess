## PV access from C#

Subtitle : lipstick on a pig ...

HISTORICAL, NEEDS UPDATING !!!

These notes are intended to cover everything that a UI developer needs to know about Epics,  IOC's, dbRecords and so on.

The details of those topics are overwhelmingly complicated and surprising. Our intent here is to boil down the key points into a useful top-level model for the kinds of things that are going on in an Epics IOC, that is 'sufficient' for UI development.

UI's access Epics using the Channel Access protocol, which lets client code interact with Epics via messages to and from 'Process Variables' (PV's). The PV interface shields the client from having to know anything about what's going on in the internals of Epics.

We'll be building UI's with high level C# 'PvProxy' classes. These provide mechanisms for connecting to PV's, reading and writing values in strongly typed fashion, and dealing with comms failures and alarm conditions. The proxies communicate with the PV's using 'async' methods so as to avoid blocking the UI thread, which remains responsive at all times.

The main purpose of this note is to summarise my limited understanding of Channel Access, and provide an opportunity for people who have a better grasp of the details (!) to correct any misconceptions.

#### Context

In an Epics system, there are IOC's ('input-output-controllers') which act as servers, and clients of various kinds that connect to those servers via the network. Each IOC publishes one or more 'process variables' (PV's), and clients typically provide a UI that supports reading and writing the 'values' associated with the PV's. 

Currently we develop client UI's using CSS ('Control System Studio') and Phoebus, both of which are based on Java. Going forward we want to also develop UI's using additional technologies such as WinUI, Uno, MAUI and Blazor, which are based on .Net and C#. These modern technologies offer benefits such as cross platform support (Windows/Android/Linux), the possibility of running UI's in a browser, advanced visua controls, and multiple window support. By orienting the UI designs around the MVVM pattern ('model-view-viewodel') we can plug in a 'view' layer using any of those various technologies, without changing the underlying code that defines the functionality of the UI.

The overall structure of a UI will look something like this :

    +------------------+      
    | Views defined in |    Views are 'dumb', just providing
    | XAML, HTML etc   |    a visual representation of the properties
    |                  |    and actions exposed by the ViewModel
    +------------------+
        |
    +------------+
    | ViewModels |          Properties, actions, event handling
    |   in C#    |          Making use of the Microsoft MVVM Toolkit
    |            |          All the functionality of a UI is defined here 
    +------------+
        |
    +------------+
    | PV Proxies |          High level interface to PV's
    |   in C#    |          Strongly typed, with support for 'async',
    |            |          comms delays and outages, alarm statuses etc
    +------------+
        |
    +----------------+
    | Channel Access |      Lower level wrapper around the 'C' API
    |   C# wrapper   |
    +----------------+
        |
    +------------------+
    | CA.DLL ('C API') |    Standard library provided as part of Epics
    +------------------+
        |
        ~ Network
        |
    +-------------------------+
    | PV's published by IOC's |    IOC's provide our interface to hardware devices
    +-------------------------+

  Several different Views can sit on top of a ViewModel, and this gives us a nice degree of future-proofing. 
  
  Whereas UI's built using CSS and Phoebus rely on 'scripting' code built into the UI logic (eg to enable/disable buttons under certain circumstances), with the MVVM scheme all this logic is pushed down to the ViewModel. The views are totally 'dumb', and just provide a visualisation of the behaviour coded into the ViewModel.

  Views will be composed from a mixture of (A) 'standard' UI components supplied by the underlying framework, eg windows and layout panels etc, and (B) custom widgets written in house, which connect to the PvProxy elements. 
  
  PvProxies provide high level mechanisms for dealing with the kinds of errors that have to be expected in a networked control system, and the corresponding 'widgets' will provide appropriate visuals eg to indicate when the network connection has been dropped, or that an alarm has been raised.

  #### Channel Access interface to PV's

  An IOC is defined in terms of Epics 'database records'. Each record has a unique name (eg 'xx:abc.VAL') and is accessible as a PV through Channel Access.
  
  Here are a few very simple dbRecord definitions, which we shall refer to later on when we're talking about the C# interfaces :

      record ( ao, "xx:myAO" ) # 'Analog output'
      {
        # The type of the value is inherently 'DOUBLE'
        field("EGU","Volts") # EGU specifies the 'units' of the value
      }

      record ( waveform, "xx:abc" ) # Base name of the PV is 'xx:abc'
      {
        # This 'waveform' record specifies a PV whose 'value'
        # is an array of 4 double-precision floating point values
        field(DESC,"This is a description of the record")
        field(FTVL,"DOUBLE") # FTVL : 'Field Type of VaLue'
        field(NELM,"4")      # Number of array elements
      }

      record ( waveform, "xx:xyz" )
      {
        field(FTVL,"SHORT") # Data type is 16-bit integer
        field(NELM,"1")     # Array length is ONE element
      }

The 'record' statement declares a PV, of a particular type such as 'waveform', 'ao' ('analog output') and so on. There are a couple of dozen different standard options for the 'record type', and the one's we're most likely to encounter (for EPAC) are

     aai        Analog Array Input    
     aao        Analog Array Output   
     ai         Analog Input          
     ao         Analog Output         
     bi         Binary Input          
     bo         Binary Output         
     longin     Long Input            
     longout    Long Output           
     lsi        Long String Input     
     lso        Long String Output    
     stringin   String Input          
     stringout  String Output         
     waveform   Waveform               

     motor      Motor control

Inside the curly braces of a 'record' declaration you can define various 'fields'. Different types of records have various options allowed for the fields. In the example above we've used the FTVL field to specify that data type for the array elements in out 'waveform' records, as DOUBLE or SHORT. For the 'ao' and 'ai' fields, that data type is implicitly DOUBLE.

In the examples above we've illustrated just a few 'fields'. Every record type defines lots of different fields, and if you omit them in the db definition they will assume default values.

Those were the most common 'standard' record types. We also have special record types such as the so called 'Motor Record', which is to do with controlling motors of various kinds. The Motor Record defines a large number of fields ; these are all 'available' for reading and writing, but depending on the type of motor we're connected to, only a subset of these will be relevant for any particular PV.

Running a tool such as 'softIoc' with the definitions above, results in four PV's being made available on the network :

      PV name   Data type of the PV 'value'
      =======   ===========================

      xx:abc    Array of 4 double precision floating point values

      xx:xyz    A single 16 bit integer value

      xx:myAO   A single double precision floating point value
      xx:myAI   A single double precision floating point value

Various command line tools let you interact with the PV's via 'Channel Access', which is the network protocol that lets clients access the PV's :

    caget     channelName             Read the value of a PV
    caput     channelName newValue    Write a new value to the PV

    cainfo    channelName             Display information about the PV, including its 'data type'

    camonitor channelName             Set up a 'monitor' that will listen for changes in the Value and Alarm-status

    camonitor -m valp channelName             Listen for changes in the Value, Alarm-status, Log, or Properties 

For example :

    caput -a xx:abc 1000 100 200
    => xx:abc 4 100 200 0 0          // Tells us the PV's value

    Note that following the PV name, you have to supply an argument which
    specifies the number of array elements, but that value is actually ignored. However many further values you supply are written to the array starting at the first element, and the remaining array elements are set to zero.

    caget xx:abc           ; get the 'value' of our PV
    => xx:abc 4 0 0 0 0    ; Tells us that the PV's value is an array with 4 elements, all zero

    Note that if you do a 'caget' before the value has been written to, the outcome
    is actually undefined. Successive 'cagets' will probably return different values every time, at least for the first element (the others are zero). This is a known bug.

    To avoid this bug, use the '-c' flag with 'caget'. This uses the 'callback' mechanism to query the value, and in this case the value you get back is an array with zero values.

    caput -a xx:abc 4 1 2 3 4 ; change the value of the 4 array elements
    caget xx:abc
    => pv:abc 4 1 2 3 4    ; Tells us the PV's value

    cainfo xx:abc
      State:            connected
      Host:             172.26.132.11:5064
      Access:           read, write
      Native data type: DBF_DOUBLE
      Request type:     DBR_DOUBLE
      Element count:    4

When we do 'caget' with a PV name just by itself, we implicitly get the 'value' which is represented by a field we haven't mentioned yet, called VAL. All PV's publish a VAL field, and we can query it with by supplying the field's full name like this :

    caget xx:abc.VAL     ; VAL is implied if we don't specify a field name

Other named 'fields' of a PV can be queried in similar fashion :

    caget xx:abc:FTVL     Query the data type of our 'waveform' array
    => DOUBLE
    caget xx:abc:NELM     How many elements in the array ?
    => 4
    cainfo xx:abc:NELM    Query info about the 'NELM' field (number of array elements)
      State:            connected
      Host:             172.26.132.11:5064
      Access:           read, no write
      Native data type: DBF_DOUBLE    <============ ??? Really surprising ...
      Request type:     DBR_DOUBLE
      Element count:    1

The server supplies an item of a 'native type', according to the record class and which field you've queried. The 'request type' can be different (eg with the -d option in caget), but normally you'd query the value as the same type that it's defined as in the record.

Channel Access sees each PV field as a distinct entity, identified by its 'pv name'.

There's potential for misunderstanding here, so let's elaborate.

A PV has a 'base name' that is specified in the 'record' definition, eg 'xx:abc'. Every PV, of whatever 'record type', has the following fields that are always present :

    VAL      Value (read/write)
    NAME     Name (without .VAL) (read-only)
    DESC     Brief description of the record's purpose (read/write)
    STAT     The 'alarm status' of the record (read-only)
    SEVR     Severity of the alarm (read-only) 

Note that fields refer to the record 'as a whole'.

As a convenience, Channel Access doesn't require you to specify a field name as well as the base name. If you just use the base name, it will assume you want the '.VAL' field ; so for example 'xx:abc' is precisely equivalent to 'xx:abc.VAL'. This makes sense because a lot of the time, a client is just interested in the 'value' of the record.

As we've seen, a record defines a number of named 'fields' according to the 'record type' that it pertains to. All of these fields may be accessed via 'pvBaseName.fieldName'.

Interestingly, the information returned via Channel Access does not tell us the 'record type' that was specified in the dbRecord definition. And we can't ask Channel Access to tell us the names of the fields that are published by a particular PV. So a client using Channel Access has to know in advance what field names it can access. If it tries to access a field that isn't provided by the particular record, a timeout error will occur, as if the IOC hosting that record was not available on the network. Hmm.

Typically a PV 'record' will have a couple of dozen fields, identified by their 4-character mnemonics. Generally speaking, any of these can be read or written via Channel Access, but only a few of them are relevant to a UI. The most interesting fields are :

    NAME   Name of the PV
    DESC   Brief description (up to 40 characters)

    VAL    Value ; an array of 0..N elements
           of type byte/short/int/float/double/string

    STAT   Current 'alarm status'
    SEVR   Current 'alarm severity'

    EGU    Engineering Units (of the value)

    PREC   Display precision (number of digits after the decimal point)

    HIHI   High and Low alarm limits
    HIGH
    LOLO
    LOW

    HOPR   High Operating Range     # Typically used in graph displays
    LOPR   Low Operating Range 

The channel access 'C' API lets us read and write fields, one at a time. Most of the fields are single-valued, eg having a type of 'double' or 'short' or 'string'. The 'VAL' field however can have a value that is either a scalar value, or an array of values. Actually the API uses the same functions to read and write fields irrespective of whether the underlying type is a scalar value or an array ; for a scalar value, we use an array size of 1.

The following data types are supported :

    CHAR     8 bit byte
    SHORT    16 bit integer
    LONG     32 bit integer
    FLOAT    32 bit floating point
    DOUBLE   64 bit floating point

    STRING   ASCII characters (max length 40)

    ENUM     Up to 16 options

For 'array' types, the number of elements is configured in the dbRecord. 

The 'C' API lets you query the number of elements and the data type :

    nElements   = ca_element_count(channel) ; Channel is an opened channel to a
    elementType = ca_field_type(channel) ;    particular field on a PV, eg 'xx:myPv.VAL'

The returned 'elementType' is one of the following :

    DBF_CHAR  
    DBF_SHORT 
    DBF_LONG  
    DBF_FLOAT 
    DBF_DOUBLE

    DBF_STRING

    DBF_ENUM  

The one we haven't met yet is the 'enum' type. Here the 'value' is an integer in the range 0..15,
and it's associated with an array of up to 16 26-character strings that correspond to 'friendly names' for those enum values. The names are accessed when we initially connect to the PV, and subsequently we just receive the integer value.

In a 'record' definition, the enum 'string' values would be set up using a bizarre scheme that defines a pair of fields for each option :

    ZRST,ZRVL	    Zero String	and Zero Value       
    ONST,ONVL	    One String and One Value    
    TWST,TWVL	    Two        
    THST,THVL	    Three    
    FRST,FRVL	    Four       
    FVST,FVVL	    Five       
    SXST,SXVL	    Six        
    SVST,SVVL	    Seven    
    EIST,EIVL	    Eight    
    NIST,NIVL	    Nine       
    TEST,TEVL	    Ten        
    ELST,ELVL	    Eleven   
    TVST,TVVL	    Twelve   
    TTST,TTVL	    Thirteen 
    FTST,FTVL	    Fourteen 
    FFST,FFVL	    Fifteen  

Someone must have thought this was cool, back in the 90's maybe.

#### Synchronous versus non-blocking access

The 'C' API provides 'blocking' functions that read and write PV values :

    // Pseudocode

    channel = ca_connect(channelName) ;

    errorCode = ca_get_value( 
      channel, 
      pAllocatedDataBlock, 
      timeToWaitInMillsecs 
    ) ;

The synchronous 'get' API is useful for experimenting, but it's not appropriate for a GUI ; if the PV is not available, the function will block the calling thread for a period of time which may be up to the specified timeout, before returning the value or returning an error code.

Luckily, Channel Access provides a nice mechanism for making asynchronous 'requests' for reads and writes, which return immediately having transmitted a message to the server (the IOC). When the request has been fulfilled, or the operation fails with a timeout, an event is raised which tells us the result.

The PvProxy classes will take full advantage of this mechanism.

PvProxies will deliberately not 'hide' the complications that are inherent in distributed-system communications. For example
- when we write a value to a PV, we'll wait for a positive acknowledgement from the IOC before raising a UI event that indicates the value has been changed. 
- when the proxy detects that the network link has gone down, its Value will be changed to 'null' to indicate that it's no longer known.

In a ViewModel, a change to a Calue will typically be triggered via an IAsyncCommand. While that command is in progress, the corresponding Value will be shown as 'about to change', eg via a coloured border (tbd).

If comms has gone down, we'll show the Value as 'unknown' so as not to be telling lies ; but it might be useful to indicate the 'last known value', eg by showing it in grey ? This possibility will be supported by the PvProxy API.

#### Registering for Change Notifications

The most common situation is for a UI to connect to the 'VAL' field of a PV, and receive notifications whenever that value changes. 

Note that more than one 'client' can be connected to a PV, and any one of them can request changes at any time. Last one wins, and every change is broadcast to every connected client.

There's no facility whereby clients can be only granted read-only access to a PV ; any client can both read and write the value.

#### Sending 'commands' via PV Value Changes

Sadly the v3 Channel Access mechanism doesn't provide a means for sending a Command to an IOC, eg a 'start-moving' command to a Motor. The best we can do is write a value to a PV, which will have been configured (in the dbRecord definitions) to perform an appropriate action whenever its value changes. Note that PV's of this nature do not necessarily hold a copy of the numeric value that has been set ;  it's perfectly likely that if we do a 'read' of the value we've just written, we won't get back the expected number. In some cases the PV will treat an incoming value as a 'command' if the value is different from the current one ; in other cases the incoming value can be anything at all, and can be the same as the current value. It all depends how the IOC has been set up.

In most cases the IOC publishes a value-changed message whenever a value is written that differes from its current value. However you can't necessarily rely on this ; in some cases a message will be generated even if the 'new' value is the same as the 'old' value.

#### Status and 'severity'

A PV has an 'alarm status' as defined in the STAT field, which is an integer.

    NO_ALARM        0
    READ            1
    WRITE           2
    HIHI            3
    HIGH            4
    LOLO            5
    LOW             6
    STATE           7
    COS             8
    COMM            9
    TIMEOUT        10
    HWLIMIT        11
    CALC           12
    SCAN           13
    LINK           14
    SOFT           15
    BAD_SUB        16
    UDF            17
    DISABLE        18
    SIMM           19
    READ_ACCESS    20
    WRITE_ACCESS   21

Hmm, this is of type DBF_ENUM however there are 21 values ???

It seems that the string representations are 'hard wired', and are not transmitted as char[] values in response to the initial connection, as happens with other fields.

Can't find a detailed description of these options - even in https://epics.anl.gov/EpicsDocumentation/ExtensionsManuals/AlarmHandler/alhUserGuide-1.2.35/ALHUserGuide.html#2_4_3

There's also an 'alarm severity' field SEVR that can be one of four values :

    NO_ALARM
    MINOR
    MAJOR
    INVALID

The 'severity' is a boiled-down version of the Alarm Status. Any given STAT value will give rise to a particular SEVR value. $$$ Is that true ???

Epics UI's with EDM and CSS etc use particular colours to indicate particular alarm conditions, and we should respect these in our UI designs.

#### Time stamps

A time stamp can be transmitted along with the Value sent by Epics. It will be useful to capture this.

#### Error codes

The 'C' API's return various error codes. Some of these are categorised as 'defunct', which means that they shouldn't be being returned by PV's that have been implemented recently. However we'll need to accommodate these, as we might be connecting to a PV that is quite ancient and returns 'defunct' codes.

    ECA_NORMAL          SUCCESS    

    ECA_IODONE          INFO      
    ECA_IOINPROGRESS    INFO      
    ECA_CHIDNOTFND      INFO       defunct
    ECA_CHIDRETRY       INFO       defunct
    ECA_NEWCONN         INFO       defunct

    ECA_ALLOCMEM        WARNING    
    ECA_TOLARGE         WARNING    
    ECA_TIMEOUT         WARNING   
    ECA_GETFAIL         WARNING   
    ECA_PUTFAIL         WARNING   
    ECA_BADCOUNT        WARNING   
    ECA_DISCONN         WARNING   
    ECA_DBLCHNL         WARNING   
    ECA_NORDACCESS      WARNING   
    ECA_NOWTACCESS      WARNING   
    ECA_NOSEARCHADDR    WARNING   
    ECA_NOCONVERT       WARNING   
    ECA_ISATTACHED      WARNING   
    ECA_UNAVAILINSERV   WARNING   
    ECA_CHANDESTROY     WARNING   
    ECA_16KARRAYCLIENT  WARNING   
    ECA_CONNSEQTMO      WARNING   
    ECA_UNRESPTMO       WARNING   
    ECA_CONN            WARNING    defunct
    ECA_UKNCHAN         WARNING    defunct
    ECA_UKNFIELD        WARNING    defunct
    ECA_NOSUPPORT       WARNING    defunct
    ECA_STRTOBIG        WARNING    defunct
    ECA_DBLCLFAIL       WARNING    defunct
    ECA_ADDFAIL         WARNING    defunct
    ECA_BUILDGET        WARNING    defunct
    ECA_NEEDSFP         WARNING    defunct
    ECA_OVEVFAIL        WARNING    defunct
    ECA_NEWADDR         WARNING    defunct
    ECA_NOCACTX         WARNING    defunct
    ECA_EMPTYSTR        WARNING    defunct
    ECA_NOREPEATER      WARNING    defunct
    ECA_NOCHANMSG       WARNING    defunct
    ECA_DLCKREST        WARNING    defunct
    ECA_SERVBEHIND      WARNING    defunct
    ECA_NOCAST          WARNING    defunct

    ECA_BADSTR          ERROR     
    ECA_BADTYPE         ERROR     
    ECA_EVDISALLOW      ERROR     
    ECA_BADMONID        ERROR     
    ECA_BADMASK         ERROR     
    ECA_BADSYNCGRP      ERROR     
    ECA_PUTCBINPROG     ERROR     
    ECA_ANACHRONISM     ERROR     
    ECA_BADCHID         ERROR     
    ECA_BADFUNCPTR      ERROR     
    ECA_BADPRIORITY     ERROR     
    ECA_NOTTHREADED     ERROR     
    ECA_MAXIOC          ERROR      defunct
    ECA_UKNHOST         ERROR      defunct
    ECA_UKNSERV         ERROR      defunct
    ECA_SOCK            ERROR      defunct
    ECA_DISCONNCHID     ERROR      defunct

    ECA_INTERNAL        FATAL   
    ECA_DEFUNCT         FATAL      defunct

$$$ Hmm, can't seem to find a detailed description of these.

?? What does 'ECA' stand for ??

In the 'Proxies' API, errors will typically throw expections, and warnings will be logged.

May need to have a more refined scheme ??

#### Creating and destroying the 'context'and 'channels'

To start off with let's illustrate 'synchronous' interactions with one or more PV's :

    // Create a 'context' for the interactions.
    // Presumably this sets up some static data in the current thread ?
    // There's no 'return value' to deal with.
    // This can be called many times, with no ill effect.

    eca_status = ca_context_create (
      pre-emptive-callbacks : true
    ) ;

    // For each PV we're interested in ...
    {
      eca_status = create_channel(
        channelName,
        callbackFunction,     // Null if we're not using callbacks
        out channelDescriptor // 'handle' for accessing the PV
      ) ;
      // Wait for completion (or error)
      eca_status = ca_pend_io(timeout) ; 
      // Work with the PV via the channelDescriptor
      // 
    }

    // Release resources.

    ca_context_destroy() ;

#### Information passed in 'structs' via messages

#### Synchronous API calls

#### API calls using 'events'

#### P/Invoke vs Windows Runtime Component ?

The current 'low level' code uses P/Invoke to access the C DLL. This works by having hand-coded C# definitions for the function call signatures and the data structures etc, which we hope (!) match exactly with the exports from the DLL. This is quite complex and error prone.

An alternative would be to implement a Windows Runtime Component that offers the .Net interface that we desire, and would be implemented in C++ ; making calls into the CA library natively. The compiler would then raise errors if there were mismatches in the definitions.

The downside is that Windows Runtime Components are not going to be supported on Linux. In principle, P/Invoke will work on Linux - but we should check that (A) this mechanism actually does work with the CA linux library, and (B) that there's a good reason to be running this kind of C# UI code on Linux. Maybe to implement a Blazor Server-side UI with .Net running on Linux ??

Plan : implement a thin layer over the 'ca_' API calls, with a class that wraps the IntPtr 'handle' and provides methods that work with enus etc rather than integer codes. The ProcessVariable class will use that wrapper rather than raw 'ca_' calls. In a future version this new class will be exported as a Windows Runtime Component.

#### References

https://docs.epics-controls.org/en/latest/guides/EPICS_Intro.html

https://epics.anl.gov/base/R3-15/9-docs/CAref.html // ?? LATEST 

https://epics.anl.gov/base/R3-15/8-docs/RecordReference.html

https://docs.epics-controls.org/en/latest/guides/EPICS_Process_Database_Concepts.html

https://epics.anl.gov/base/R3-15/6-docs/CAproto/index.html ********

https://epics.anl.gov/EpicsDocumentation/ExtensionsManuals/AlarmHandler/alhUserGuide-1.2.35/ALHUserGuide.html

https://epics-modules.github.io/motor/motorRecord.html


#### Limits 

Max length of a PV name : 26 characters (including the field name). 

Or maybe it's 36 these days (see db_name_dim in db_access.h) ??

PV name can contain a-z A-Z 0-9 _ - + : [ ] < > ;

A '.' is not permitted in a PV name, as it's reserved to separate the PV name from the 'field' name.

Typically a PV name is made up as a 'prefix' part plus a 'body', separated by a special character such as ':'.

Max length of a 'string' value : 
- 40 for a 'normal' string eg a DESC description field.
- Up to 65535 for a 'long string' ??
String values are in ASCII. 

Enums : up to 16 options, each defined with a string of up to 26 ASCII characters.

#### Miscellaneous notes

Commands accepted by 'softIoc' :

https://docs.epics-controls.org/en/latest/appdevguide/IOCTestFacilities.html?highlight=dbpf#overview

#### QUESTIONS

mbbi - enum ?? ONVL ...

The 'graphic' structs are meant for getting waveforms for display ?

The 'ctrl' structs are meant for situations where the client is SETTING values, and wants to know the upper and lower limits ?

We're often also interested in the timestamp, but sadly that isn't part of the ctrl and gr structs.

The 'status' struct gives us the bare minimum, ie alarm status + severity, and the value.

Can data for scalar values be transferred as string ??

-------------------


Gillian Black

  Db record equates to a PV ... ?
  Yes, but to the VAL field.

  Hmm, a PV with a 'field name' gets you to a named Field of the record ???

  Half a million PV's.

  It's been said that a PV equates to a db record. That's true in that the 'value' of a PV is the record's VAL field if you don't specify a field name, eg 'caget mypv' gets you the same resut as 'caget mypv.VAL'. You can access any field of a record by giving its name, eg 'caget mypv.DESC' which returns the 'description' field. So it seems that a PV really equates to a particular *field* of a db record - by default, the VAL field. Is that a good way to think of it ? 

Juniper Willard ; Michael Bree

  Rapid Screen Builder, for 'quick and dirty screens' ...

Day 2, Lightning talks

  PV Monitor (Michael Bree) ***
    Discovering PV's that are available, IOC status etc

  Qt Applications at TRIUMF

  Tools for troubleshooting devices through Channel Access ??
    Richard Farnsworth

  ITER control system, lessons learnt, Bertrand Bauvir
    Worried about higher criticality systems (summary at the very end) ?

  React AutomationLogic Studio Alarm Handler
    A new web-based EPICS alarm handler has been designed. The alarm handler GUI is built on top of the React AutomationLogic Studio (RAS) platform.
    https://whova.com/portal/webapp/cidte_202107/Agenda/1763819
    https://github.com/wduckitt/React-AutomationLogic-Studio
    React AutomationLogic Studio is a new software platform to enable the control of large scientific equipment through EPICS.

----------------

https://epics.anl.gov/tech-talk/2012/msg00252.php
  When you the access the VAL field of a waveform record through CA or a database link, the data is actually fetched from the prec->bptr field, which gets initialized in the first pass of record initialization to point to a buffer that holds NELM values of the type FTVL.  Device support should copy its data into this buffer, never modify the prec->bptr field itself.
  > For a case of array of STRINGs it could have different organizations. Like:
  > 1) elements delimited by NULLes
  >      aaa\0bbbbbb\0cc\0...
  > 2) each element of array has a fixed length chars and the names then
  > padded with NULL-es like below
  >      aaa\0\0\0\0\0\0\0bbbbbb\0\0\0\0cc\0\0\0\0\0\0\0\0
  When FTVL=STRING the array elements are all MAX_STRING_SIZE=40 characters long, thus when NELM=2 the buffer is 80 bytes long and the second string element starts at offset 40 into the buffer.
  https://epics.anl.gov/tech-talk/2012/msg00251.php
  record( waveform, "$(PSNAME):boardNames")
  {
    field(DESC, "Board type names")
    field(DTYP, "CAEN x527 generic HV Mainframe")
    field(SCAN, "Passive")
    field(PINI, "YES")
    field(FTVL, "STRING")
    field(INP, "@$(PSNAME) boardNames") *************
  }

-----------

PvFieldProxy<T>

Event History ??

WPF BASED EPICS SERVER AND ITS APPLICATION IN CSNS (2013)
https://inspirehep.net/files/e076ec53c4a3f6c1f17134d1691fcdc5

-------------------

Higher level types

Aha, define CA low level stuff via an interface !
Then can use either CA DLL, or a web-sockets implementation ??


















