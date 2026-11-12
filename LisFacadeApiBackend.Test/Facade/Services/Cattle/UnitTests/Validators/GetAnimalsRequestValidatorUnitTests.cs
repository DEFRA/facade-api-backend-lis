using LisFacadeApiBackend.Facade.Endpoints.Validators;
using LisFacadeApiBackend.Facade.LIS.Enums;
using LisFacadeApiBackend.Facade.LIS.Request;

namespace LisFacadeApiBackend.Test.Facade.Services.Cattle.UnitTests.Validators;

public class GetAnimalsRequestValidatorUnitTests
{
    private const string SiteIdentifier = "12/345/6789";

    private static GetAnimalsRequest BuildRequest(
        string? species = "cattle",
        string? siteIdentifier = SiteIdentifier,
        string? page = null,
        string? pageSize = null,
        string? order = null,
        string? sort = null)
    {
        var built = GetAnimalsRequestValidator.TryBuild(species, siteIdentifier, page, pageSize, order, sort, out var request, out var error);
        Assert.True(built, error);
        return request!;
    }

    private static string BuildError(
        string? species = "cattle",
        string? siteIdentifier = SiteIdentifier,
        string? page = null,
        string? pageSize = null,
        string? order = null,
        string? sort = null)
    {
        var built = GetAnimalsRequestValidator.TryBuild(species, siteIdentifier, page, pageSize, order, sort, out var request, out var error);
        Assert.False(built);
        Assert.Null(request);
        return error!;
    }

    [Fact]
    public void Builds_request_with_defaults_when_only_required_values_supplied()
    {
        var request = BuildRequest();

        Assert.Equal(Species.Cattle, request.Species);
        Assert.Equal(SiteIdentifier, request.SiteIdentifier);
        Assert.Equal(1, request.Page);
        Assert.Null(request.PageSize);
        Assert.Equal(AnimalOrder.AnimalIdentifier, request.Order);
        Assert.Equal("asc", request.Sort);
    }

    [Theory]
    [InlineData("cattle")]
    [InlineData("Cattle")]
    [InlineData("CATTLE")]
    [InlineData("  cattle  ")]
    public void Accepts_species_ignoring_case_and_surrounding_whitespace(string species)
    {
        var request = BuildRequest(species: species);

        Assert.Equal(Species.Cattle, request.Species);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("pigs")]
    public void Rejects_missing_or_unsupported_species(string? species)
    {
        var error = BuildError(species: species);

        Assert.Equal("species is required and must be a supported species", error);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rejects_missing_site_identifier(string? siteIdentifier)
    {
        var error = BuildError(siteIdentifier: siteIdentifier);

        Assert.Equal("siteIdentifier is required", error);
    }

    [Theory]
    [InlineData("1", 1)]
    [InlineData("3", 3)]
    [InlineData("+7", 7)]
    public void Parses_page_when_supplied(string page, int expected)
    {
        var request = BuildRequest(page: page);

        Assert.Equal(expected, request.Page);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("2147483648")]
    public void Rejects_page_that_is_not_a_whole_number(string page)
    {
        var error = BuildError(page: page);

        Assert.Equal("page must be a whole number", error);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void Rejects_page_less_than_one(string page)
    {
        var error = BuildError(page: page);

        Assert.Equal("page must be greater than or equal to 1", error);
    }

    [Theory]
    [InlineData("10", 10)]
    [InlineData("1", 1)]
    public void Parses_page_size_when_supplied(string pageSize, int expected)
    {
        var request = BuildRequest(pageSize: pageSize);

        Assert.Equal(expected, request.PageSize);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("2.0")]
    [InlineData("2147483648")]
    public void Rejects_page_size_that_is_not_a_whole_number(string pageSize)
    {
        var error = BuildError(pageSize: pageSize);

        Assert.Equal("pageSize must be a whole number", error);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    public void Rejects_page_size_less_than_one(string pageSize)
    {
        var error = BuildError(pageSize: pageSize);

        Assert.Equal("pageSize must be greater than or equal to 1", error);
    }

    [Theory]
    [InlineData("animalIdentifier", AnimalOrder.AnimalIdentifier)]
    [InlineData("AnimalIdentifier", AnimalOrder.AnimalIdentifier)]
    [InlineData("state", AnimalOrder.State)]
    [InlineData("STATE", AnimalOrder.State)]
    public void Parses_order_ignoring_case(string order, AnimalOrder expected)
    {
        var request = BuildRequest(order: order);

        Assert.Equal(expected, request.Order);
    }

    [Theory]
    [InlineData("ear")]
    [InlineData("sex")]
    public void Rejects_unsupported_order(string order)
    {
        var error = BuildError(order: order);

        Assert.Equal("order must be 'animalIdentifier' or 'state'", error);
    }

    [Theory]
    [InlineData("asc", "asc")]
    [InlineData("ASC", "asc")]
    [InlineData("desc", "desc")]
    [InlineData("DESC", "desc")]
    public void Normalises_sort_direction(string sort, string expected)
    {
        var request = BuildRequest(sort: sort);

        Assert.Equal(expected, request.Sort);
    }

    [Theory]
    [InlineData("up")]
    [InlineData("descending")]
    public void Rejects_unsupported_sort_direction(string sort)
    {
        var error = BuildError(sort: sort);

        Assert.Equal("sort must be 'asc' or 'desc'", error);
    }

    [Fact]
    public void Builds_request_with_all_query_values_supplied()
    {
        var request = BuildRequest(page: "2", pageSize: "25", order: "state", sort: "desc");

        Assert.Equal(2, request.Page);
        Assert.Equal(25, request.PageSize);
        Assert.Equal(AnimalOrder.State, request.Order);
        Assert.Equal("desc", request.Sort);
    }

    [Fact]
    public void Treats_blank_optional_values_as_not_supplied()
    {
        var request = BuildRequest(page: "  ", pageSize: "", order: " ", sort: "");

        Assert.Equal(1, request.Page);
        Assert.Null(request.PageSize);
        Assert.Equal(AnimalOrder.AnimalIdentifier, request.Order);
        Assert.Equal("asc", request.Sort);
    }

    [Fact]
    public void Reports_species_error_before_site_identifier_error()
    {
        var error = BuildError(species: null, siteIdentifier: null);

        Assert.Equal("species is required and must be a supported species", error);
    }
}
