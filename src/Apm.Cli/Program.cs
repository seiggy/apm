using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Spectre.Console.Cli;
using Apm.Cli.Commands;
using Apm.Cli.Commands.Config;
using Apm.Cli.Commands.Deps;

/// <summary>Entry point with DynamicDependency attributes for NativeAOT trimming support.</summary>
static partial class Program
{
    // Commands
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(InitCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(InitSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(InstallCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(InstallSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CompileCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(CompileSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RunCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(RunSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PreviewCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(PreviewSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ListCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ListSettings))]
    // Deps subcommands
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DepsListCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DepsListSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DepsTreeCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DepsTreeSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DepsVerifyCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DepsVerifySettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DepsUninstallCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DepsUninstallSettings))]
    // Config subcommands
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ConfigGetCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ConfigGetSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ConfigSetCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ConfigSetSettings))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ConfigShowCommand))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ConfigShowSettings))]
    // Spectre.Console.Cli internal commands
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.ExplainCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.VersionCommand", "Spectre.Console.Cli")]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Spectre.Console.Cli.XmlDocCommand", "Spectre.Console.Cli")]
    static int Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("apm");
            config.SetApplicationVersion(
                Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? "0.0.0");

            config.AddCommand<InitCommand>("init")
                .WithDescription("🚀 Initialize a new APM project");

            config.AddCommand<InstallCommand>("install")
                .WithDescription("📦 Install APM packages");

            config.AddCommand<CompileCommand>("compile")
                .WithDescription("🚀 Compile APM context into distributed AGENTS.md files");

            config.AddCommand<RunCommand>("run")
                .WithDescription("▶️  Run a script with parameters");

            config.AddBranch("deps", deps =>
            {
                deps.SetDescription("📋 Manage APM package dependencies");

                deps.AddCommand<DepsListCommand>("list")
                    .WithDescription("📋 List installed APM dependencies");

                deps.AddCommand<DepsTreeCommand>("tree")
                    .WithDescription("🌳 Show dependency tree");

                deps.AddCommand<DepsVerifyCommand>("verify")
                    .WithDescription("✅ Verify installed dependencies");

                deps.AddCommand<DepsUninstallCommand>("uninstall")
                    .WithDescription("🗑️  Uninstall APM packages");
            });

            config.AddBranch("config", cfg =>
            {
                cfg.SetDescription("⚙️  Configure APM CLI");

                cfg.AddCommand<ConfigGetCommand>("get")
                    .WithDescription("📖 Get configuration value");

                cfg.AddCommand<ConfigSetCommand>("set")
                    .WithDescription("✏️  Set configuration value");

                cfg.AddCommand<ConfigShowCommand>("show")
                    .WithDescription("📋 Show current configuration");
            });

            config.AddCommand<PreviewCommand>("preview")
                .WithDescription("👀 Preview a script's compiled prompt files");

            config.AddCommand<ListCommand>("list")
                .WithDescription("📋 List available scripts in the current project");
        });

        return app.Run(args);
    }
}
