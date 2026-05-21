namespace ToyRobotApp;

public record Table(int Width, int Height)
{
    public bool IsValidPosition(Position position) =>
        position.X >= 0 &&
        position.X < Width &&
        position.Y >= 0 &&
        position.Y < Height;
}