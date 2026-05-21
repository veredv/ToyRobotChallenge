using ToyRobotApp;

namespace ToyRobotTests.Unit;

public class TableTests
{
    private const int TableSizeX = 5;
    private const int TableSizeY = 5;
    private readonly Table _table = new(TableSizeX, TableSizeY);

    [Theory]
    [MemberData(nameof(ValidPositions))]
    public void IsValidPosition_ReturnsTrue(Position position)
    {
        Assert.True(_table.IsValidPosition(position));
    }

    [Theory]
    [MemberData(nameof(InvalidPositions))]
    public void IsValidPosition_ReturnsFalse(Position position)
    {
        Assert.False(_table.IsValidPosition(position));
    }

    public static TheoryData<Position> ValidPositions =>
    [
        new(0, 0),
        new(4, 4),
        new(0, 4),
        new(4, 0),
        new(2, 2)
    ];

    public static TheoryData<Position> InvalidPositions =>
    [
        new(5, 0),
        new(0, 5),
        new(5, 5),
        new(-1, 0),
        new(0, -1)
    ];
}