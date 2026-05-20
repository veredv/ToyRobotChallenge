namespace ToyRobotApp;

public class CommandProcessor(Table table, IRobot robot)
{
    public void Process(string commandStr)
    {
        var commandParts = commandStr.Trim().ToUpperInvariant().Split(' ');
        switch (commandParts.Length) 
        {
            case 1 : ProcessCommand(commandParts[0]);
                break;
            case 2 when commandParts[0] == "PLACE" : ProcessPlace(commandParts[1]);
                break;
        }
    }

    private void ProcessMove()
    {
        var nextPosition = robot.GetNextForwardPosition();
        if (table.IsValidPosition(nextPosition))
        {
            robot.MoveTo(nextPosition);
        }
    }
    
    private void ProcessCommand(string command)
    {
        if (!robot.IsPlaced) return;
        switch (command)
        {
            case "MOVE" : ProcessMove(); break;
            case "LEFT" : ProcessLeft(); break;
            case "RIGHT" : ProcessRight(); break;
            case "REPORT" : Console.WriteLine(ProcessReport()); break;
            default: return;
        }
    }

    private void ProcessPlace(string placement)
    {
        var placementParts = placement.Split(',');
        if  (placementParts.Length != 3 ||
             !int.TryParse(placementParts[0], out var x) ||
             !int.TryParse(placementParts[1], out var y) ||
             !Enum.TryParse<Direction>(placementParts[2], true, out var facing)
             ) 
        {
            return;
        }
        var newPosition = new Position(x, y);
        if (table.IsValidPosition(newPosition))
        {
            robot.Place(newPosition, facing);
        }
    }

    private string ProcessReport() => $"{robot.Position!.X},{robot.Position.Y},{robot.Facing.ToString().ToUpperInvariant()}";

    private void ProcessRight() => robot.RotateRight();

    private void ProcessLeft() => robot.RotateLeft();
}