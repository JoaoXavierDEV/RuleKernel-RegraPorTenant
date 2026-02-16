using RuleKernel.Core.Models;
using System.Net.Http.Json;
using System.Net;

namespace BlazorApp1.Services;

public sealed class FaturaClient
{
    private readonly HttpClient _http;

    public FaturaClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<Fatura> EmitirAsync(Guid tenantId, decimal valorPrincipal, CancellationToken cancellationToken = default)
    {
        var requestUri = $"api/tenants/{tenantId:D}/faturas/emitir";

        using var response = await _http.PostAsJsonAsync(requestUri, new EmitirFaturaRequest(valorPrincipal), cancellationToken);
        await EnsureSuccessWithBodyAsync(response, HttpMethod.Post, requestUri, cancellationToken);

        var fatura = await response.Content.ReadFromJsonAsync<Fatura>(cancellationToken: cancellationToken);
        return fatura ?? throw new InvalidOperationException("Resposta da API vazia ao emitir fatura.");
    }

    public async Task<Fatura> EmitirPorRegrasAsync(Guid tenantId, Guid regraVencimentoId, Guid regraDescontoId, decimal valorPrincipal, CancellationToken cancellationToken = default)
    {
        //var requestUri = $"api/tenants/{tenantId:D}/faturas/emitir-por-regras";
        var requestUri = $"api/tenants/{tenantId:D}/faturas/emitir-por-regras";

        using var response = await _http.PostAsJsonAsync(
            requestUri, new EmitirPorRegrasRequest(regraVencimentoId, regraDescontoId, valorPrincipal), cancellationToken);
        await EnsureSuccessWithBodyAsync(response, HttpMethod.Post, requestUri, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NoContent)
            throw new InvalidOperationException("A API retornou 204 (No Content) ao emitir fatura.");

        var fatura = await response.Content.ReadFromJsonAsync<Fatura>(cancellationToken: cancellationToken);
        return fatura ?? throw new InvalidOperationException("Resposta da API vazia ao emitir fatura.");
    }

    private static async Task EnsureSuccessWithBodyAsync(
        HttpResponseMessage response,
        HttpMethod method,
        string requestUri,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        string? body = null;
        try
        {
            body = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch
        {
            // ignora falha de leitura do corpo para não mascarar o status
        }

        var msg = $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase}) em {method} '{requestUri}'.";
        if (!string.IsNullOrWhiteSpace(body))
            msg += $" Corpo: {body}";

        throw new HttpRequestException(msg, inner: null, statusCode: response.StatusCode);
    }

    private sealed record EmitirFaturaRequest(decimal ValorPrincipal);

    private sealed record EmitirPorRegrasRequest(Guid RegraVencimentoId, Guid RegraDescontoId, decimal ValorPrincipal);
}
