namespace ToyRobotApp;

public class Robot
{
    public bool IsPlaced { get; private set;}
    public int Y { get; set; }
    public int X { get; set; }
    public Direction Facing { get; private set;}
}