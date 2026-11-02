using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LisFacadeApiBackend.Facade.LIS.Error;

public sealed class LisProblemDetails
{
    [JsonPropertyName("type")]
    [Url]
    [StringLength(1024)]
    public required string Type { get; init; }

    [JsonPropertyName("status")]
    [Range(100, 599)]
    public required int Status { get; init; }

    [JsonPropertyName("title")]
    [StringLength(1024)]
    public required string Title { get; init; }

    [JsonPropertyName("detail")]
    [StringLength(4096)]
    public required string Detail { get; init; }

    [JsonPropertyName("errors")]
    public LisProblemDetails? Errors { get; init; }
}

public sealed class LisErrors : List<LisProblemDetails>, IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Count is < 1 or > 1000)
        {
            yield return new ValidationResult("Errors must contain between 1 and 1000 items.");
        }
    }
}