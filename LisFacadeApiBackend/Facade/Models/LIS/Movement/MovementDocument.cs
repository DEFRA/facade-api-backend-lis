using System.Text.Json.Serialization;
using LisFacadeApiBackend.Facade.LIS.Enums;

namespace LisFacadeApiBackend.Facade.LIS.Movement;

public sealed class MovementDocument
{
    [JsonPropertyName("type")]
    public DocumentType? Type { get; init; }

    [JsonPropertyName("id")]
    public Guid? Id { get; init; }
}