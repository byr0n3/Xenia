# Xenia

A simple, minimalistic HTTP server written in C#.

## Usage

### Returning raw HTML

### `Program.cs`

```csharp
internal static class Program
{
    public static void Main(string[] args)
    {
        var options = new ServerOptions("0.0.0.0", 80);
  
        // Optionally, you can give the server an instance of a `CancellationToken`. 
        var server = new Server(options);

        // Register a request handler. This tells the server to execute the given function for the given path.
        server.AddRequestHandler(new RequestHandler("/"u8, Program.Handler));

        var thread = new Thread(server.Listen);
        thread.Start();
    }

    // A request handler takes 2 arguments: the request instance, and a ResponseBuilder.
    // The request instance contains all the parsed data from the request: path, headers, request body and some other stuff you might need to handle the request.
    // The ResponseBuilder is a utility class that will make writing data to the response easier. It also has some extension methods to easily write commonly used data types.
    private static void Handler(in Request request, ref ResponseBuilder response)
    {
        var html = "<html><body><h1>Hello world!</h1></html></body>"u8;

        response.AppendHtml(in request, in StatusCodes.Status200OK, html);
    }
}
```

### Rendering Razor pages

Making Xenia render your Razor files is very simple. Simply create your Razor page, and
call `server.AddRazorPage<RazorPage>(path);` when initializing your server instance. This will automatically register a
request handler for the given path and will handle rendering the Razor page and writing the data and correct headers to
the response.

### `Test.razor`

```razor
@using System.Text
@using Byrone.Xenia.Data

<h1>Hello from @(Encoding.UTF8.GetString(Request.Path))!</h1>

@code {
	
	// Declaring a `Request` variable with the `[CascadingParameter]` attribute allows you to access the request in the Razor scope.
	[CascadingParameter]
	public Request Request { get; init; }

}
```

### `Program.cs`

```csharp
internal static class Program
{
    public static void Main(string[] args)
    {
        var options = new ServerOptions("0.0.0.0", 80);
 
        var server = new Server(options);

        // Register a request handler for the given path and Razor page.
        server.AddRazorPage<Test>("/"u8);

        var thread = new Thread(server.Listen);
        thread.Start();
    }
}
```

### Returning JSON

`Person.cs` contains the model that the JSON data will be based off of. We define a `JsonSerializerContext` to help the
C# runtime figure out how to turn the model into JSON. This is a lot faster than letting the C# runtime run reflection
on the model in runtime, and allows for Native AOT support. Note how the struct extends from `IJson<T>`. This will allow
you to easily return the JSON data in the request handler.

In the request handler, simply call `response.AppendJson(in request, in StatusCodes.Status200OK, model);` to write the
JSON data of the given model to the response, with the appropriate headers.

### `Person.cs`

```csharp
// In this example, we define a JsonSerializerContext in the same file as the model.
// However, you could define 1 global JsonSerializerContext and allow it to serialize multiple different data models.
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Person))]
internal sealed partial class PersonSerializerContext : JsonSerializerContext;

// This is the data model we're going to serialize. This can also be a class.
public readonly struct Person : IJson<Person>
{
    public static JsonTypeInfo<Person> TypeInfo =>
        PersonSerializerContext.Default.Person;
    
    [JsonPropertyName("name")] public string Name { get; init; }

    [JsonPropertyName("age")] public int Age { get; init; }
}
```

### `Program.cs`

```csharp
internal static class Program
{
    public static void Main(string[] args)
    {
        var options = new ServerOptions("0.0.0.0", 80);

        var server = new Server(options);

        server.AddRequestHandler(new RequestHandler("/"u8, Program.Handler));

        var thread = new Thread(server.Listen);
        thread.Start();
    }

    private static void Handler(in Request request, ref ResponseBuilder response)
    {
        // Create a new instance of our data model.
        var person = new Person()
        {
            Name = "John Doe",
            Age = 22,
        };

        // Write the serialized data model to the response, with the correct headers.
        response.AppendJson(in request, in StatusCodes.Status200OK, person);
    }
}
```

### Using query parameters

The `Request` struct contains a property called `Query`. These are the raw bytes of the query of the path.
Using `QueryParameters.Parse` you can get a handy `RentedArray<KeyValue>` instance which you can use to access the query
parameters.

### `Program.cs`

```csharp
internal static class Program
{
    public static void Main(string[] args)
    {
        var options = new ServerOptions("0.0.0.0", 80);
  
        var server = new Server(options);

        server.AddRequestHandler(new RequestHandler("/"u8, Program.Handler));

        var thread = new Thread(server.Listen);
        thread.Start();
    }

    private static void Handler(in Request request, ref ResponseBuilder response)
    {
        // Make sure to dispose the instance when you're done using it!
        using (var queryParameters = QueryParameters.Parse(request.Query))
        {
            foreach (var param in queryParameters)
            {
                // Do something with the query parameter...
            }
            
            if (queryParameters.TryGetParameter("key"u8, out var key))
            {
                // Do something with the query parameter...
            }
        }
        
        var html = "<html><body><h1>Hello world!</h1></html></body>"u8;

        response.AppendHtml(in request, in StatusCodes.Status200OK, html);
    }
}
```

### Response compression

Xenia supports built-in support for GZip, Deflate and Brotli compression. Response compression
is enabled by default. You can customize this behavior by adding `CompressionMethod` flags to the `ServerOptions`
constructor. Let's use the [Rendering Razor pages](#rendering-razor-pages) example and customize the compression
behavior.

```csharp
internal static class Program
{
    public static void Main(string[] args)
    {
        // Disable compression
        var compression = CompressionMethod.None;

        // Only support GZip compression
        var compression = CompressionMethod.GZip;

        // Only support Brotli & GZip compression
        var compression = CompressionMethod.Brotli & CompressionMethod.GZip;
        
        var options = new ServerOptions("0.0.0.0", 80, compression);
 
        var server = new Server(options);

        server.AddRazorPage<Test>("/"u8);

        var thread = new Thread(server.Listen);
        thread.Start();
    }
}
```

### Serving static files

Xenia has built-in support for serving static files. You can define an array of static file directories in
the `ServerOptions` struct. In this example, we have a directory called `_static`. This directory contains a
file `styles.css` and a file called `secret.txt`, the latter being contained in a directory called `dont_access`. By
starting the server and navigating to `http://localhost/styles.css`, you can access the contents of the `styles.css`
file. Keep in mind that every file in this directory will be accessible. Navigating
to `http://localhost/dont_access/secret.txt` will return the contents of the `secret.txt` file. Please don't use a
static files directory to store sensitive data.

```csharp
internal static class Program
{
    public static void Main(string[] args)
    {
        var options = new ServerOptions("0.0.0.0", 80)
        {
            StaticFiles = new[] { "_static" },    
        };
 
        var server = new Server(options);
    }
}
```

```xml

<Project Sdk="Microsoft.NET.Sdk.Razor">
    <ItemGroup>
        <Content Include="_static\**">
            <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
        </Content>
    </ItemGroup>
</Project>
```
