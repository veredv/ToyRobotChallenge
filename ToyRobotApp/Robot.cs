namespace ToyRobotApp;

public interface IRobot
{
    bool IsPlaced { get; }
    Position? Position { get; }
    Direction Facing { get; }
    void Place(Position position, Direction facing);
    Position GetNextForwardPosition();
    void MoveTo(Position position);
    void RotateLeft();
    void RotateRight();
}

public class Robot : IRobot
{
    public Position? Position { get; private set; }
    public Direction Facing { get; private set;}
    public bool IsPlaced => Position is not null;

    public void Place(Position position, Direction facing)
    {
        Position = position;
        Facing = facing;
    }
    
    public Position GetNextForwardPosition()
    {
        if (!IsPlaced) throw new InvalidOperationException("Robot is not placed");
        var position = Position!;
        return Facing switch
        {
            Direction.North => position with { Y = position.Y + 1 },
            Direction.East => position with { X = position.X + 1 },
            Direction.South => position with { Y = position.Y - 1 },
            Direction.West => position with { X = position.X - 1 },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void MoveTo(Position position) => Position = position;

    public void RotateLeft() => Facing = Facing switch
        {
            Direction.North => Direction.West,
            Direction.West => Direction.South,
            Direction.South => Direction.East,
            Direction.East => Direction.North,
            _ => throw new ArgumentOutOfRangeException()
        };
    
    public void RotateRight() => Facing = Facing switch
        {
            Direction.North => Direction.East,
            Direction.East => Direction.South,
            Direction.South => Direction.West,
            Direction.West => Direction.North,
            _ => throw new ArgumentOutOfRangeException()
        };
}