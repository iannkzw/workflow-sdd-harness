using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using WorkflowSddHarness.Domain.Model;

namespace WorkflowSddHarness.Infrastructure.Config;

public sealed record LoadResult
{
    public bool IsSuccess { get; init; }
    public HarnessConfig? Config { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static LoadResult Success(HarnessConfig config) =>
        new() { IsSuccess = true, Config = config };

    public static LoadResult Failure(IReadOnlyList<string> errors) =>
        new() { IsSuccess = false, Errors = errors };

    public static LoadResult Failure(string error) =>
        Failure(new[] { error });
}

public sealed class ConfigLoader
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly ConfigSchemaValidator _validator;

    public ConfigLoader() : this(new ConfigSchemaValidator()) { }

    public ConfigLoader(ConfigSchemaValidator validator)
    {
        _validator = validator;
    }

    public LoadResult Load(string configPath)
    {
        if (!File.Exists(configPath))
        {
            if (Directory.Exists(configPath))
                return LoadResult.Failure($"O caminho aponta para um diretório, não um arquivo: {configPath}");

            return LoadResult.Failure($"Arquivo de configuração não encontrado: {configPath}");
        }

        string content;
        try
        {
            content = File.ReadAllText(configPath, System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
            return LoadResult.Failure($"Erro ao ler o arquivo: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(content))
            return LoadResult.Failure("O arquivo de configuração está vazio ou contém apenas espaços em branco.");

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(content);
        }
        catch (JsonException ex)
        {
            return LoadResult.Failure($"JSON inválido: {ex.Message}");
        }

        if (node is null)
            return LoadResult.Failure("O arquivo de configuração resultou em JSON nulo após o parse.");

        var validation = _validator.Validate(node);
        if (!validation.IsValid)
        {
            var messages = new List<string>(validation.Errors.Count);
            foreach (var error in validation.Errors)
                messages.Add($"Campo '{error.Field}': {error.Message}");
            return LoadResult.Failure(messages);
        }

        HarnessConfig? config;
        try
        {
            config = JsonSerializer.Deserialize<HarnessConfig>(content, _jsonOptions);
        }
        catch (JsonException ex)
        {
            return LoadResult.Failure($"Erro ao desserializar o config: {ex.Message}");
        }

        if (config is null)
            return LoadResult.Failure("A desserialização do config retornou nulo.");

        return LoadResult.Success(config);
    }
}
