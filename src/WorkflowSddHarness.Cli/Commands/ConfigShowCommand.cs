using System.CommandLine;
using WorkflowSddHarness.Infrastructure.Config;

namespace WorkflowSddHarness.Cli.Commands;

public static class ConfigShowCommand
{
    public static Command Build()
    {
        var configOption = new Option<string>("--config")
        {
            Description = "Caminho para o config.json do projeto-alvo",
            Required = true
        };

        var command = new Command("show")
        {
            Description = "Exibe a configuração carregada e os caminhos resolvidos"
        };
        command.Add(configOption);

        command.SetAction((Func<ParseResult, int>)(parseResult =>
        {
            var output = parseResult.InvocationConfiguration.Output;
            var configPath = parseResult.GetValue(configOption)!;

            var loader = new ConfigLoader();
            var result = loader.Load(configPath);

            if (!result.IsSuccess)
            {
                foreach (var err in result.Errors)
                    output.WriteLine($"Erro: {err}");
                return 1;
            }

            var config = result.Config!;
            var paths = TargetProjectPaths.Resolve(configPath, config);

            output.WriteLine("=== Configuração ===");
            output.WriteLine($"version:   {config.Version}");
            output.WriteLine($"providers: {string.Join(", ", config.Providers.Keys)}");
            output.WriteLine($"steps:     {string.Join(", ", config.Steps.Keys)}");
            output.WriteLine();
            output.WriteLine("=== Caminhos Resolvidos ===");
            output.WriteLine($"projectRoot: {paths.ProjectRoot}");
            output.WriteLine($"specsRoot:   {paths.SpecsRoot}");
            output.WriteLine($"rulesDir:    {paths.RulesDir}");
            output.WriteLine($"src:         {paths.Src}");

            var missing = paths.MissingRequiredPaths();
            if (missing.Count > 0)
            {
                output.WriteLine();
                output.WriteLine("Aviso: caminhos obrigatórios ausentes:");
                foreach (var m in missing)
                    output.WriteLine($"  ! {m}");
            }

            return 0;
        }));

        return command;
    }
}
