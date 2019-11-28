
# Harvest All Logging

Example of tapping into all of the logging data your application provides

## Information available when an application uses structured logging


### The LogType information

The LogType shows information which identify what log message has been written. For any given message type these values should not be variable. They are often used by log storage and alert generating systems to group, filter, and categorize messages.

The combination of `CategoryName` + `EventId {Id, Name}` should be sufficiently unique to itentify a log type.

If the `EventId` is `{0, NullOrEmpty}` then the `CategoryName` + `OriginalFormat` should be sufficient to 
identify a log type.

### The Log information

Each log message has name-value structued `Properties`, and also a simple `Message` line that can produced by combining `Properties` with `OriginalFormat`.

Text-line centric loggers typically display `Message` instead of individual properties.

Structured data loggers typically persist `Properties`. Technically speaking, they do not need to persist the `Message` if 
the `OriginalFormat` and `Properties` are recorded. However, many structured loggers do produce and record `Message` as 
a full-text column for readability and searchability.

### The Scope information

This information is introduced by calls to logger.BeginScope(). It is associated with all messages up until Dispose is called 
on the object returned by BeginScope.

Similar to a log message, each scope has name-value structured `Properties` and simple 
text representation of the scope properties called `Levels` in this example.

```cs
public string ActionThatLogs(string flavor, string color, string text)
{
    using (_logger.BeginScope("Flavor:{Flavor}", flavor))
    {
        _logger.LogInformation(new EventId(1, "EncodingData"), "Encoding user-provided value '{Text}' with '{Color}'", text, color);
                
        var protector = _protectionProvider.CreateProtector("MyExample", color);
        var ciphertext = protector.Protect(text);
                
        _logger.LogInformation(new EventId(2, "ResultingSize"), "Resulting data is {ProtectedLength} characters long", ciphertext.Length);

        return ciphertext;
    }
}
```

```yaml
- LogType:
    CategoryName: ExampleWebApplication.Controllers.HomeController
    LogLevel: Information
    EventId:
      Id: 1
      Name: EncodingData
    OriginalFormat: Encoding user-provided value '{Text}' with '{Color}'
  Log:
    Message: Encoding user-provided value 'Some text' with 'red'
    Properties:
      Color: red
      Text: Some text
  Scope:
    Properties:
      TraceId: 1284a649-45939d518eaccce3
      ParentId: ''
      Flavor: cherry
      RequestPath: /Home/ActionThatLogs
      SpanId: '|1284a649-45939d518eaccce3.'
      ActionName: ExampleWebApplication.Controllers.HomeController.ActionThatLogs (ExampleWebApplication)
      RequestId: 0HLRJMF9107PH:0000000D
      ActionId: 4425bea0-9bbb-40e4-8bbd-90e11aaade18
    Levels:
    - 'RequestPath:/Home/ActionThatLogs RequestId:0HLRJMF9107PH:0000000D, SpanId:|1284a649-45939d518eaccce3., TraceId:1284a649-45939d518eaccce3, ParentId:'
    - ExampleWebApplication.Controllers.HomeController.ActionThatLogs (ExampleWebApplication)
    - Flavor:cherry

- LogType:
    CategoryName: Microsoft.AspNetCore.DataProtection.KeyManagement.KeyRingBasedDataProtector
    LogLevel: Trace
    EventId:
      Id: 31
    OriginalFormat: Performing protect operation to key {KeyId:B} with purposes {Purposes}.
  Log:
    Message: Performing protect operation to key {3db7d888-76cb-47f0-85bd-84d9d60e5f5a} with purposes ('C:\...\github.com\lodejard\harvest-all-logging\src\ExampleWebApplication', 'MyExample', 'red').
    Properties:
      Purposes: ('C:\...\github.com\lodejard\harvest-all-logging\src\ExampleWebApplication', 'MyExample', 'red')
      KeyId: 3db7d888-76cb-47f0-85bd-84d9d60e5f5a
  Scope:
    Properties:
      TraceId: 1284a649-45939d518eaccce3
      ParentId: ''
      Flavor: cherry
      RequestPath: /Home/ActionThatLogs
      SpanId: '|1284a649-45939d518eaccce3.'
      ActionName: ExampleWebApplication.Controllers.HomeController.ActionThatLogs (ExampleWebApplication)
      RequestId: 0HLRJMF9107PH:0000000D
      ActionId: 4425bea0-9bbb-40e4-8bbd-90e11aaade18
    Levels:
    - 'RequestPath:/Home/ActionThatLogs RequestId:0HLRJMF9107PH:0000000D, SpanId:|1284a649-45939d518eaccce3., TraceId:1284a649-45939d518eaccce3, ParentId:'
    - ExampleWebApplication.Controllers.HomeController.ActionThatLogs (ExampleWebApplication)
    - Flavor:cherry

- LogType:
    CategoryName: ExampleWebApplication.Controllers.HomeController
    LogLevel: Information
    EventId:
      Id: 2
      Name: ResultingSize
    OriginalFormat: Resulting data is {ProtectedLength} characters long
  Log:
    Message: Resulting data is 134 characters long
    Properties:
      ProtectedLength: 134
  Scope:
    Properties:
      TraceId: 1284a649-45939d518eaccce3
      ParentId: ''
      Flavor: cherry
      RequestPath: /Home/ActionThatLogs
      SpanId: '|1284a649-45939d518eaccce3.'
      ActionName: ExampleWebApplication.Controllers.HomeController.ActionThatLogs (ExampleWebApplication)
      RequestId: 0HLRJMF9107PH:0000000D
      ActionId: 4425bea0-9bbb-40e4-8bbd-90e11aaade18
    Levels:
    - 'RequestPath:/Home/ActionThatLogs RequestId:0HLRJMF9107PH:0000000D, SpanId:|1284a649-45939d518eaccce3., TraceId:1284a649-45939d518eaccce3, ParentId:'
    - ExampleWebApplication.Controllers.HomeController.ActionThatLogs (ExampleWebApplication)
    - Flavor:cherry
```

## Additional information available when an exception is logged

If an Exception is passed along with a log event there is a large amount of data it carries.

It is up to the logger how much of that data will be captured. This example walks the inner-exception chain
and captures the FullName of the exception `Type`, the `Message` property, and the `Stack` property. 

The stack is not shown in this markdown file for brevity.

```yaml
- LogType:
    CategoryName: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware
    LogLevel: Error
    EventId:
      Id: 1
      Name: UnhandledException
    OriginalFormat: An unhandled exception has occurred while executing the request.
  Log:
    Message: An unhandled exception has occurred while executing the request.
    Properties: {}
    Exception:
    - Type: System.ApplicationException
      Message: Unable to show privacy
    - Type: System.IO.FileNotFoundException
      Message: Could not find file 'c:\autoexec.bat'.
  Scope:
    Properties:
      RequestId: '0HLRJLTR0RQRD:0000000D'
      SpanId: '|317129b-47139405049770f0.'
      RequestPath: /Home/Privacy
      ParentId: ''
      TraceId: '317129b-47139405049770f0'
    Levels:
    - 'RequestPath:/Home/Privacy RequestId:0HLRJLTR0RQRD:0000000D, SpanId:|317129b-47139405049770f0., TraceId:317129b-47139405049770f0, ParentId:'
```
