namespace LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

public sealed class CattleApiOptions
{
    public const string SectionName = "CattleApi";

    /// <summary>Keyed client per endpoint name. Values are <see cref="CattleApiClientKeys"/>; endpoints not listed use Fake.</summary>
    public Dictionary<string, string> ClientModes { get; init; } = [];
}
