using RuleKernel.Core.Models;
using System.Net.Http.Json;

namespace BlazorApp1.Services;

public sealed class RuleProvider
{
    private readonly HttpClient _http;

    private static readonly Uri RelativeEndpoint = new("api/rules-csharp", UriKind.Relative);

    public RuleProvider(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<Rule>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Rule>? rules;
        try
        {
            rules = await _http.GetFromJsonAsync<List<Rule>>(RelativeEndpoint, cancellationToken);
        }
        catch (Exception ex)
        {
            var baseAddress = _http.BaseAddress;
            var requestUri = baseAddress is null ? new Uri(RelativeEndpoint.ToString(), UriKind.Relative) : new Uri(baseAddress, RelativeEndpoint);
            throw new InvalidOperationException($"Falha ao carregar regras em '{requestUri}'. Verifique se a API está rodando e se o `ApiBaseUrl` está correto.", ex);
        }

        return rules ?? [];

    }
}
