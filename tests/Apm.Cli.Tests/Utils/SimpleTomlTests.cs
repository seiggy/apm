using Apm.Cli.Utils;
using AwesomeAssertions;

namespace Apm.Cli.Tests.Utils;

/// <summary>
/// Tests for the minimal TOML parser/serializer used by CodexClientAdapter.
/// Covers the subset of TOML we actually use: top-level strings,
/// dotted table sections ([mcp_servers.name]), string arrays, and roundtrip.
/// </summary>
public class SimpleTomlTests
{
    [Fact]
    public void Parse_RealisticCodexConfig()
    {
        var toml = """
            model_provider = "github-models"
            model = "gpt-4o-mini"

            [mcp_servers.github]
            command = "npx"
            args = ["--yes", "@modelcontextprotocol/server-github"]

            [mcp_servers.github.env]
            GITHUB_PERSONAL_ACCESS_TOKEN = "ghp_test123"

            [mcp_servers.filesystem]
            command = "npx"
            args = ["--yes", "@modelcontextprotocol/server-filesystem", "/home/user/projects"]

            [mcp_servers.docker_server]
            command = "docker"
            args = ["run", "-i", "--rm", "-e", "API_KEY", "mcp/server"]
            id = "docker-mcp-server"

            [mcp_servers.docker_server.env]
            API_KEY = "sk-test456"
            """;

        var result = SimpleToml.Parse(toml);

        // Top-level keys
        result["model_provider"].Should().Be("github-models");
        result["model"].Should().Be("gpt-4o-mini");

        // mcp_servers container
        var mcpServers = result["mcp_servers"] as Dictionary<string, object?>;
        mcpServers.Should().NotBeNull();
        mcpServers.Should().ContainKey("github");
        mcpServers.Should().ContainKey("filesystem");
        mcpServers.Should().ContainKey("docker_server");

        // github server — command + args + nested env
        var github = mcpServers!["github"] as Dictionary<string, object?>;
        github!["command"].Should().Be("npx");
        var githubArgs = github["args"] as List<object?>;
        githubArgs.Should().HaveCount(2);
        githubArgs![0].Should().Be("--yes");
        githubArgs[1].Should().Be("@modelcontextprotocol/server-github");
        var githubEnv = github["env"] as Dictionary<string, object?>;
        githubEnv!["GITHUB_PERSONAL_ACCESS_TOKEN"].Should().Be("ghp_test123");

        // filesystem server — args with path value
        var filesystem = mcpServers["filesystem"] as Dictionary<string, object?>;
        var fsArgs = filesystem!["args"] as List<object?>;
        fsArgs.Should().HaveCount(3);
        fsArgs![2].Should().Be("/home/user/projects");

        // docker server — command + id + 6-element args + env
        var docker = mcpServers["docker_server"] as Dictionary<string, object?>;
        docker!["command"].Should().Be("docker");
        docker["id"].Should().Be("docker-mcp-server");
        (docker["args"] as List<object?>).Should().HaveCount(6);
        var dockerEnv = docker["env"] as Dictionary<string, object?>;
        dockerEnv!["API_KEY"].Should().Be("sk-test456");
    }

    [Fact]
    public void Parse_EmptyOrMissingFile_ReturnsEmptyDict()
    {
        SimpleToml.Parse("").Should().BeEmpty();
        SimpleToml.Parse("# just a comment\n").Should().BeEmpty();
    }

    [Fact]
    public void Roundtrip_CodexConfigSurvivesSerializeThenParse()
    {
        var original = new Dictionary<string, object?>
        {
            ["model_provider"] = "github-models",
            ["model"] = "gpt-4o-mini",
            ["mcp_servers"] = new Dictionary<string, object?>
            {
                ["github"] = new Dictionary<string, object?>
                {
                    ["command"] = "npx",
                    ["args"] = new List<object?> { "--yes", "@modelcontextprotocol/server-github" },
                    ["env"] = new Dictionary<string, object?>
                    {
                        ["GITHUB_PERSONAL_ACCESS_TOKEN"] = "ghp_test123"
                    }
                }
            }
        };

        var toml = SimpleToml.Serialize(original);
        var parsed = SimpleToml.Parse(toml);

        parsed["model_provider"].Should().Be("github-models");

        var servers = parsed["mcp_servers"] as Dictionary<string, object?>;
        var github = servers!["github"] as Dictionary<string, object?>;
        github!["command"].Should().Be("npx");
        (github["args"] as List<object?>)![0].Should().Be("--yes");
        var env = github["env"] as Dictionary<string, object?>;
        env!["GITHUB_PERSONAL_ACCESS_TOKEN"].Should().Be("ghp_test123");
    }

    [Fact]
    public void Serialize_ProducesValidTomlForCodexConfig()
    {
        var data = new Dictionary<string, object?>
        {
            ["model_provider"] = "github-models",
            ["mcp_servers"] = new Dictionary<string, object?>
            {
                ["github"] = new Dictionary<string, object?>
                {
                    ["command"] = "npx",
                    ["args"] = new List<object?> { "--yes", "@mcp/server-github" }
                }
            }
        };

        var toml = SimpleToml.Serialize(data);

        toml.Should().Contain("model_provider = \"github-models\"");
        toml.Should().Contain("[mcp_servers.github]");
        toml.Should().Contain("command = \"npx\"");
        toml.Should().Contain("args = [\"--yes\", \"@mcp/server-github\"]");
    }
}
