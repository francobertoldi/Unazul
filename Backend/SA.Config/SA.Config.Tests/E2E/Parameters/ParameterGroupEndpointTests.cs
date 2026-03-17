using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SA.Config.Api.ViewModels.Parameters;
using SA.Config.Tests.E2E.Fixtures;
using Xunit;

namespace SA.Config.Tests.E2E.Parameters;

public sealed class ParameterGroupEndpointTests : IClassFixture<ConfigWebAppFactory>
{
    private readonly ConfigWebAppFactory _factory;

    public ParameterGroupEndpointTests(ConfigWebAppFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateAuthenticatedClient(ConfigWebAppFactory? factory = null)
    {
        var f = factory ?? _factory;
        var client = f.CreateClient();
        var token = f.GenerateJwtToken();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Get_ParameterGroups_Returns_Categories()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/v1/parameter-groups");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();
        body.Should().NotBeNull();
        body!.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Create_ParameterGroup_Returns_201()
    {
        // Arrange
        using var factory = new ConfigWebAppFactory();
        var client = CreateAuthenticatedClient(factory);

        var request = new CreateParameterGroupRequest(
            "test_new_group",
            "Test New Group",
            "General",
            "mdi-test",
            99);

        // Act
        var response = await client.PostAsJsonAsync("/api/v1/parameter-groups", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ParameterGroupResponse>();
        body.Should().NotBeNull();
        body!.Code.Should().Be("test_new_group");
        body.Name.Should().Be("Test New Group");
    }

    [Fact]
    public async Task Delete_ParameterGroup_Without_Params_Returns_204()
    {
        // Arrange
        using var factory = new ConfigWebAppFactory();
        var client = CreateAuthenticatedClient(factory);

        // Create a group first
        var createRequest = new CreateParameterGroupRequest(
            "deletable_group",
            "Deletable Group",
            "General",
            "mdi-delete",
            100);

        var createResponse = await client.PostAsJsonAsync("/api/v1/parameter-groups", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ParameterGroupResponse>();

        // Act
        var deleteResponse = await client.DeleteAsync($"/api/v1/parameter-groups/{created!.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
