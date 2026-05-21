using System.Diagnostics;

namespace ToyRobotTests.Integration;

public class IntegrationTests
{
    [Fact]
    public void ExampleA()
    {
        var result = RunApp("example_a.txt");
        Assert.Equal("0,1,NORTH", result.Output.Trim());
    }
    
    [Fact]
    public void ExampleB()
    {
        var result = RunApp("example_b.txt");
        Assert.Equal("0,0,WEST", result.Output.Trim());
    }
    
    [Fact]
    public void ExampleC()
    {
        var result = RunApp("example_c.txt");
        Assert.Equal("3,3,NORTH", result.Output.Trim());
    }
    
    [Fact]
    public void Place_OverridesPositionAfterMove()
    {
        var result = RunApp("place_overrides_after_move.txt");
        Assert.Equal("2,4,WEST", result.Output.Trim());
    }
    
    [Fact]
    public void Place_OutsideBounds_Subsequent_DoesNotChangeRobot()
    {
        var result = RunApp("place_outside_bounds_subsequent.txt");
        Assert.Equal("1,2,NORTH", result.Output.Trim());
    }
    
    [Fact]
    public void CommandsBeforeFirstPlace_AreIgnored()
    {
        var result = RunApp("ignored_pre_place_commands.txt");
        Assert.Equal("1,2,NORTH", result.Output.Trim());
    }
    
    [Fact]
    public void MultipleReports_Verification()
    {
        var result = RunApp("multiple_reports.txt");
        var outputLines = result.Output.Trim().Split(Environment.NewLine);
        
        Assert.Equal(8, outputLines.Length);
        Assert.Equal("0,0,NORTH", outputLines[0]);
        Assert.Equal("0,1,NORTH", outputLines[1]);
        Assert.Equal("0,1,WEST",  outputLines[2]);
        Assert.Equal("0,1,WEST",  outputLines[3]);
        Assert.Equal("0,1,NORTH", outputLines[4]);
        Assert.Equal("2,2,EAST",  outputLines[5]);
        Assert.Equal("4,2,EAST",  outputLines[6]);
        Assert.Equal("4,1,SOUTH", outputLines[7]);
    }

    [Fact]
    public void MissingFile_ExitWithClearMessage()
    {
        var result = RunApp("nofile.txt");
        Assert.Contains("Not Found", result.Error,  StringComparison.OrdinalIgnoreCase);
    }
    
    private static readonly string ProjectPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../ToyRobotApp"));

    private static readonly string TestDataPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../Integration/TestData"));

    private static AppResult RunApp(string fileName)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project {ProjectPath} {Path.Combine(TestDataPath, fileName)}",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });

        var outputTask = process!.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
    
        Task.WaitAll(outputTask, errorTask);
        process.WaitForExit();
        
        return new AppResult(outputTask.Result, errorTask.Result);
    }
    
    private record AppResult(string Output, string Error);
}