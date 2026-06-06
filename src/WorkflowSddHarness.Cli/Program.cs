using System.CommandLine;
using WorkflowSddHarness.Cli.Commands;

var rootCommand = new RootCommand("Workflow SDD Harness \u2014 orquestrador de coding agents");

var configCommand = new Command("config", "Comandos relacionados \u00e0 configura\u00e7\u00e3o do projeto-alvo");
configCommand.Add(ConfigShowCommand.Build());
rootCommand.Add(configCommand);

return rootCommand.Parse(args).Invoke();
