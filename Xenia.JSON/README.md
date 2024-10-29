# Xenia.JSON

Helpers for working with JSON content and (de)serializing JSON using the built-in `System.Text.Json` package.

## Usage

### `IJson<T>`

Interface for simpler JSON (de)serialization using Source Generation. Requires you to implement a
`static JsonTypeInfo<T> TypeInfo` which will be used when using the custom `JsonSerializer` provided by this package.

```csharp
using Byrone.Xenia;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

// Person.cs
public readonly struct Person : IJson<Person>
{
    public static JsonTypeInfo<Person> TypeInfo =>
        AppJsonSerializerContext.Default.Person!;
    
    public required string FirstName { get; init; }
    
    public required string LastName { get; init; }
    
    [JsonPropertyName("dob")] public required System.DateOnly DateOfBirth { get; init; }
    
    [JsonIgnore] public int Age =>
        (int)((System.DateTime.UtcNow - this.DateOfBirth.ToDateTime(default)).TotalDays / 365.242199);
}

// AppJsonSerializerContext.cs
[JsonSourceGenerationOptions(
	GenerationMode = JsonSourceGenerationMode.Metadata,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
	PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower
)]
[JsonSerializable(typeof(Person))]
internal sealed partial class AppJsonSerializerContext : JsonSerializerContext;
```

### `JsonSerializer`

Custom methods that call the built-in `System.Text.Json.JsonSerializer` for classes/structs implementing `IJson<T>`. It
will use the implemented `TypeInfo` property to (de)serialize JSON using Source Generation.

```csharp
using Byrone.Xenia;

var utf8Json = "..."u8;
var person = JsonSerializer.Deserialize<Person>(utf8Json);

var writer = new Utf8JsonWriter(stream);
JsonSerializer.Serialize(writer, person);
```

### Extension methods

Extension methods for the `Request` struct for deserializing the request body as a JSON value. This method doesn't
validate if the request contains valid JSON data, you should do this yourself by validating the `Content-Type` header of
the request.

```csharp
using Byrone.Xenia;

static IResponse RequestHandler(in Request request)
{
    var person = request.GetBodyAsJson<Person>();
}
```
