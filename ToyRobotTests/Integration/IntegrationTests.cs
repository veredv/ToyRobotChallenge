using System.Diagnostics;

public class IntegrationTests
{
    [Fact]
    public void ExampleA()
    {
        var result = RunApp("TestData/example_a.txt");
        Assert.Equal("0,1,NORTH", result.Trim());
    }
    
    [Fact]
    public void ExampleB()
    {
        var result = RunApp("TestData/example_b.txt");
        Assert.Equal("0,0,WEST", result.Trim());
    }
    
    [Fact]
    public void ExampleC()
    {
        var result = RunApp("TestData/example_c.txt");
        Assert.Equal("3,3,NORTH", result.Trim());
    }

    private string RunApp(string filePath)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project ToyRobot {filePath}",
            RedirectStandardOutput = true
        });

        return process!.StandardOutput.ReadToEnd();
    }
}