using ToyRobotApp;

namespace ToyRobotTests.Unit;

public class RobotTests
{
    public class MoveTests : RobotTests
    {
        [Fact]
        public void GetNextForwardPosition_WhenNotPlaced_Throws()
        {
            var robot = new Robot();
            Assert.Throws<InvalidOperationException>(robot.GetNextForwardPosition);
        }
        
        [Theory]
        [MemberData(nameof(Movements))]
        public void GetNextForwardPosition_ReturnsCorrectPosition(Direction originFacing, Position position, Position expectedNextPosition)
        {
            var robot = new Robot();
            robot.Place(position, originFacing);

            var nextPosition = robot.GetNextForwardPosition();

            Assert.Equal(expectedNextPosition, nextPosition);
        }

        public static TheoryData<Direction, Position, Position> Movements => new()
        {
            { Direction.North, new Position(0, 0), new Position(0, 1) },
            { Direction.South, new Position(1, 1), new Position(1, 0) },
            { Direction.East, new Position(0, 0), new Position(1, 0) },
            { Direction.West, new Position(1, 1), new Position(0, 1) },
        };
    }

    public class PlaceTests : RobotTests
    {
        [Fact]
        public void Place_SetsNewPosition()
        {
            var robot = new Robot();
            robot.Place(new Position(1, 1), Direction.West);

            var newPosition = new Position(2, 2);
            robot.Place(newPosition, Direction.East);

            Assert.Equal(newPosition, robot.Position);
        }

        [Fact]
        public void Place_SetsNewFacing()
        {
            var robot = new Robot();
            robot.Place(new Position(1, 1), Direction.South);
            robot.Place(new Position(2, 2), Direction.East);

            Assert.Equal(Direction.East, robot.Facing);
        }
    }

    public class IsPlacedTests : RobotTests
    {
        [Fact]
        public void IsPlaced_False_WhenPositionIsNull()
        {
            var robot = new Robot();

            Assert.False(robot.IsPlaced);
        }
        
        [Fact]
        public void IsPlaced_True_AfterPlace()
        {
            var robot = new Robot();
            robot.Place(new Position(1, 1), Direction.East);

            Assert.True(robot.IsPlaced);
        }
    }

    public class RotateTests : RobotTests
    {
        
        [Theory]
        [MemberData(nameof(LeftRotations))]
        public void Left_RotatesAntiClockwise(Direction originFacing, Direction expectedFacing) // NORTH -> WEST -> SOUTH -> EAST -> NORTH
        {
            var robot = new Robot();
            var robotPosition = new Position(0, 0);
            robot.Place(robotPosition, originFacing);
            
            robot.RotateLeft();

            Assert.Equal(expectedFacing, robot.Facing);
            Assert.Equal(robotPosition, robot.Position); // Verify Unchanged
        }
        
        public static TheoryData<Direction, Direction> LeftRotations => new()
        {
            { Direction.North, Direction.West },
            { Direction.West,  Direction.South },
            { Direction.South, Direction.East},
            { Direction.East,  Direction.North },
        };
        
        [Theory]
        [MemberData(nameof(RightRotations))]
        public void Right_RotatesClockwise(Direction originFacing, Direction expectedFacing) // NORTH -> EAST -> SOUTH -> WEST -> NORTH
        {
            var robot = new Robot();
            var robotPosition = new Position(0, 0);
            robot.Place(robotPosition, originFacing);
            
            robot.RotateRight();

            Assert.Equal(expectedFacing, robot.Facing);
            Assert.Equal(robotPosition, robot.Position); // Verify Unchanged
        }
        
        public static TheoryData<Direction, Direction> RightRotations => new()
        {
            { Direction.North, Direction.East },
            { Direction.East,  Direction.South },
            { Direction.South, Direction.West},
            { Direction.West,  Direction.North },
        };
    }
}