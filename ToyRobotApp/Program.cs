using ToyRobotApp;

const int tableSizeX = 5;
const int tableSizeY = 5;
const string defaultFilePath = "commands.txt";

var filePath = args.Length > 0 ? args[0] : defaultFilePath;
if (!File.Exists(filePath))
{
    Console.Error.WriteLine($"File not found: {filePath}");
    return;
}

var lines = File.ReadAllLines(filePath);

var table = new Table(tableSizeX, tableSizeY);
var robot = new Robot();
var processor = new CommandProcessor(table, robot);

foreach (var line in lines)
    processor.Process(line);