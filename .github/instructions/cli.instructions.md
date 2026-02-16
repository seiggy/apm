---
applyTo: "src/Apm.Cli/**"
description: "CLI Design Guidelines for visual output, styling, and user experience standards"
---

# CLI Design Guidelines

## Visual Design Standards

### Spectre.Console Usage
- **ALWAYS** use Spectre.Console for visual output
- Use the established `AnsiConsole` instance with custom theme
- Use `SpectreConsoleOutput` for consistent styled output

### Command Help Text
- **ALWAYS** include contextual emojis in command descriptions
- Format: `Description = "🚀 Initialize a new APM project"`
- Use semantic emojis that match the command purpose

### Status Symbols & Feedback
- Use consistent status symbols:
  - `✨` success/completion
  - `🚀` running/execution
  - `⚙️` configuration/setup
  - `💡` information/tips
  - `📋` lists/display
  - `👀` preview/inspection
  - `🤖` AI/models/runtime
  - `📊` status/metrics
  - `⚠️` warnings
  - `❌` errors
  - `✅` installed/confirmed

### Structured Output
- **Tables**: Use Spectre.Console tables for structured data (scripts, models, config, runtimes)
- **Panels**: Use Spectre.Console panels for grouped content, next steps, examples
- **Consistent Spacing**: Add empty lines between sections with `AnsiConsole.WriteLine()`

### Error Handling
- Use styled markup for all error messages: `AnsiConsole.MarkupLine("[red]❌ Error message[/]")`
- Always include contextual symbols
- Provide actionable suggestions when possible
- Maintain consistent error message format

## Code Organization

### Color Scheme
- Primary: cyan for titles and highlights
- Success: green with ✨ symbol
- Warning: yellow with ⚠️ symbol
- Error: red with ❌ symbol
- Info: blue with 💡 symbol
- Muted: dim for secondary text

### Table Design
- Include meaningful titles with emojis
- Use semantic column styling (bold for names, muted for details)
- Keep tables clean with appropriate padding
- Show status with symbols in dedicated columns

## Implementation Patterns

### Command Structure
```csharp
[Description("🚀 Action description")]
public sealed class MyCommand : AsyncCommand<MyCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandOption("--verbose")]
        [Description("Enable verbose output")]
        public bool Verbose { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        AnsiConsole.MarkupLine("[cyan]⚙️ Starting operation...[/]");

        // Main logic here

        AnsiConsole.MarkupLine("[green]✨ Operation complete![/]");
        return 0;
    }
}
```

### Table Creation
```csharp
var table = new Table()
    .Title("🚀 Title")
    .AddColumn(new TableColumn("Name").Header("[bold cyan]Name[/]"))
    .AddColumn(new TableColumn("Details"));

table.AddRow("item", "description");
AnsiConsole.Write(table);
```

### Panel Usage
```csharp
var panel = new Panel(content)
    .Header("📋 Section Title")
    .BorderColor(Color.Cyan);
AnsiConsole.Write(panel);
```

## Quality Standards

### User Experience
- Every action should have clear visual feedback
- Group related information in panels or tables
- Use consistent symbols throughout the application
- Provide helpful next steps and examples

### Performance
- NativeAOT compilation — avoid reflection where possible
- Use source generators for JSON serialization
- Keep startup time minimal

## What NOT to Do

- ❌ Never use plain `Console.WriteLine()` without styling
- ❌ Don't mix color schemes or symbols inconsistently
- ❌ Avoid walls of text without visual structure
- ❌ Don't use reflection-heavy patterns (breaks NativeAOT)
- ❌ Never sacrifice functionality for visuals

## Documentation Sync Requirements

### CLI Reference Documentation
- **ALWAYS** update `docs/cli-reference.md` when adding, modifying, or removing CLI commands
- **ALWAYS** update command help text, options, arguments, and examples in the reference
- **ALWAYS** verify examples in the documentation actually work with the current implementation
- **ALWAYS** keep the command list in sync with available commands

### Documentation Update Checklist
When changing CLI functionality, update these sections in `docs/cli-reference.md`:
- Command syntax and arguments
- Available options and flags
- Usage examples
- Return codes and error handling
- Quick reference sections

### Documentation Standards
- Use the same emojis in documentation as in CLI help text
- Include realistic, working examples that users can copy-paste
- Document both success and error scenarios
- Keep examples current with the latest syntax
- Maintain consistency between CLI help and reference documentation
