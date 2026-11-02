using System.Text.Json;
using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Common;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class MovementBatch
{
    [JsonPropertyName("species")]
    public required Species Species { get; init; }

    [JsonPropertyName("animals")]
    public List<BatchAnimal>? Animals { get; init; }
}

[JsonConverter(typeof(BatchAnimalJsonConverter))]
public abstract class BatchAnimal
{
}

public sealed class BatchAnimalJsonConverter : JsonConverter<BatchAnimal>
{
    public override BatchAnimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException("A batch animal must be a JSON object.");
        }

        var comparison = options.PropertyNameCaseInsensitive
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var hasAnimalIdentifier = false;
        var hasQuantity = false;

        foreach (var property in root.EnumerateObject())
        {
            hasAnimalIdentifier |= property.Name.Equals("animalIdentifier", comparison);
            hasQuantity |= property.Name.Equals("quantity", comparison);
        }

        return (hasAnimalIdentifier, hasQuantity) switch
        {
            (true, false) => root.Deserialize<IdentifiedBatchAnimal>(options)!,
            (false, true) => root.Deserialize<MarkedBatchAnimal>(options)!,
            _ => throw new JsonException("A batch animal must contain exactly one of 'animalIdentifier' or 'quantity'.")
        };
    }

    public override void Write(Utf8JsonWriter writer, BatchAnimal value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class IdentifiedBatchAnimal : BatchAnimal
{
    [JsonPropertyName("animalIdentifier")]
    public required string AnimalIdentifier { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class MarkedBatchAnimal : BatchAnimal
{
    [JsonPropertyName("mark")]
    public string? Mark { get; init; }

    [JsonPropertyName("quantity")]
    public required int Quantity { get; init; }
}