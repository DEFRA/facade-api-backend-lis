using LisFacadeApiBackend.Facade.Cattle.Request;

namespace LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

public interface ICattleApiClient
{
    Task<HttpResponseMessage> GetCattleDetailsAsync(
        string earTag,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> SearchHoldingsAsync(
        string? search,
        int? page,
        int? pageSize,
        string? sort,
        string? order,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> GetHoldingAsync(
        string county,
        string parish,
        string holding,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> GetCattleForHoldingMultiSegmentAsync(
        string county,
        string parish,
        string holding,
        string? earTag,
        string? breed,
        string? sex,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> GetCattleForHoldingAsync(
        string cph,
        string? earTag,
        string? breed,
        string? sex,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> GetBundlesForHoldingMultiSegmentAsync(
        string county,
        string parish,
        string holding,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> GetBundlesForHoldingAsync(
        string cph,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> CreateRegistrationBundleAsync(
        RegistrationBundleRequest request,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> ValidateRegistrationBundleAsync(
        string id,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> GetUserDetailsAsync(
        string subject,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> CreateUserAsync(
        CreateUserRequest? request,
        CancellationToken cancellationToken);

    Task<HttpResponseMessage> GetDetailedHealthAsync(
        CancellationToken cancellationToken);
}