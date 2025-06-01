# Mozart.Encore

A cross-platform re-implementation of a game server for a certain retro online music game.  
This project is inspired by the _Mozart Project 0.028_.

> [!WARNING]
> This project is currently work in progress. 
> 
> The current implementation has discovered **all v3.10 request/response commands (op-codes)**.  
> Offline single player with mock data is supported. However, There is currently no persistent/in-memory storage or 
> session management implemented for multiplayer features. 

## Project Structure

| Project                               | Description                                 |
|---------------------------------------|---------------------------------------------|
| [Encore](Source/Mozart.Encore/Encore) | Custom TCP server framework                 |
| [Mozart](Source/Mozart.Encore/Mozart) | Game server implementation for v3.10 client |

## Native AOT

The [Encore](Source/Mozart.Encore/Encore) framework supports Native AOT compilation. When Native AOT is enabled, this project and all 
dependent NuGet packages / libraries are subject to [Native AOT limitations](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=windows%2Cnet8#limitations-of-native-aot-deployment).

Due to Native AOT limitation, you must avoid using `CommandController` entirely and use function-based routing instead:

```csharp
hostBuilder.ConfigureRoutes((context, routes) =>
{
    // Function-based routing: Safe with AOT
    routes.UseCodec<DefaultMessageCodec>()
        .Map(async Task<FooResponse> (Session session, FooRequest request) =>
        {
            // Implementation here...
            return new FooResponse();
        });
});
```
