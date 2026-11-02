using LisFacadeApiBackend.Facade.Cattle.Request;

namespace LisFacadeApiBackend.Facade.Services.Cattle.ApiClient;

public sealed class CattleApiClient(HttpClient httpClient) : ICattleApiClient
{
    public async Task<HttpResponseMessage> GetCattleDetailsAsync(
        string earTag,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<HttpResponseMessage> SearchHoldingsAsync(
        string? search, int? page, int? pageSize, string? sort, string? order,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetHoldingAsync(
        string county, string parish, string holding, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetCattleForHoldingMultiSegmentAsync(
        string county, string parish, string holding, string? earTag, string? breed, string? sex,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetCattleForHoldingAsync(
        string cph, string? earTag, string? breed, string? sex, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetBundlesForHoldingMultiSegmentAsync(
        string county, string parish, string holding, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetBundlesForHoldingAsync(
        string cph, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> CreateRegistrationBundleAsync(
        RegistrationBundleRequest request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> ValidateRegistrationBundleAsync(
        string id, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetUserDetailsAsync(
        string subject, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> CreateUserAsync(
        CreateUserRequest? request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<HttpResponseMessage> GetDetailedHealthAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
