using System.Diagnostics;

namespace ToyRobotTests.Integration;

public class IntegrationTests
{
    [Fact]
    public void ExampleA()
    {
        var result = RunApp("TestData/example_a.txt");
        Assert.Equal("0,1,NORTH", result.Output.Trim());
    }
    
    [Fact]
    public void ExampleB()
    {
        var result = RunApp("TestData/example_b.txt");
        Assert.Equal("0,0,WEST", result.Output.Trim());
    }
    
    [Fact]
    public void ExampleC()
    {
        var result = RunApp("TestData/example_c.txt");
        Assert.Equal("3,3,NORTH", result.Output.Trim());
    }

    [Fact]
    public void MissingFile_ExitWithClearMessage()
    {
        var result = RunApp("nofile.txt");
        Assert.Contains("Not Found", result.Error,  StringComparison.OrdinalIgnoreCase);
    }

    private static AppResult RunApp(string filePath)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project ToyRobot {filePath}",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });
        process!.WaitForExit();

        return new AppResult(
            process!.StandardOutput.ReadToEnd(),
            process!.StandardError.ReadToEnd()
            );
    }

    private record AppResult(string Output, string Error);
}