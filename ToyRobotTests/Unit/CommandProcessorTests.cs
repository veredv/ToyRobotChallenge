using NSubstitute;
using ToyRobotApp;

namespace ToyRobotTests.Unit;

public class CommandProcessorTests
{
    private const int TableSizeX = 5;
    private const int TableSizeY = 5;

    private readonly Table _table = new(TableSizeX, TableSizeY);
    private readonly IRobot _robot = Substitute.For<IRobot>();
    private readonly CommandProcessor _processor;

    protected CommandProcessorTests()
    {
        _processor = new CommandProcessor(_table, _robot);
    }

    public class BeforePlaceTests : CommandProcessorTests
    {
        [Theory]
        [MemberData(nameof(CommandsOtherThanPlace))]
        public void Command_BeforePlace_IsIgnored(string command)
        {
            _robot.IsPlaced.Returns(false);
            _processor.Process(command);
            
            _robot.DidNotReceive().MoveTo(Arg.Any<Position>());
            _robot.DidNotReceive().RotateLeft();
            _robot.DidNotReceive().RotateRight();
            _robot.DidNotReceive().Place(Arg.Any<Position>(), Arg.Any<Direction>());
        }

        public static TheoryData<string> CommandsOtherThanPlace =>
        [
            "MOVE",
            "RIGHT",
            "LEFT",
            "REPORT"
        ];
    }
    
    public class MoveTests : CommandProcessorTests
    {
        [Fact]
        public void Move_DelegatesToRobot()
        {
            var position = new Position(0, 0);
            var newPosition = new Position(0, 1);
            _robot.IsPlaced.Returns(true);
            _robot.Position.Returns(position);
            _robot.Facing.Returns(Direction.North);
            _robot.GetNextForwardPosition().Returns(newPosition);
            _processor.Process("MOVE");
            
            _robot.Received().MoveTo(newPosition);
        }
        
        [Fact]
        public void Move_FacingSouth_DecrementsY_AndNotX()
        {
            _processor.Process("PLACE 1,1,SOUTH");
            _processor.Process("MOVE");

            Assert.True(_robot.IsPlaced);
            Assert.Equal(1, _robot.Position!.X);
            Assert.Equal(0, _robot.Position.Y);
        }
        
        [Fact]
        public void Move_FacingEast_IncrementsX_AndNotY()
        {
            _processor.Process("PLACE 0,0,EAST");
            _processor.Process("MOVE");

            Assert.True(_robot.IsPlaced);
            Assert.Equal(1, _robot.Position!.X);
            Assert.Equal(0, _robot.Position.Y);
        }

        [Fact]
        public void Move_FacingWest_DecrementsX_AndNotY()
        {
            _processor.Process("PLACE 1,1,WEST");
            _processor.Process("MOVE");

            Assert.True(_robot.IsPlaced);
            Assert.Equal(0, _robot.Position!.X);
            Assert.Equal(1, _robot.Position.Y);
        }

        [Theory]
        [MemberData(nameof(EdgeCases))]
        public void Move_FallingOffTable_IsIgnored(string command, int expectedX, int expectedY)
        {
            _processor.Process(command);
            _processor.Process("MOVE");

            Assert.True(_robot.IsPlaced);
            Assert.Equal(expectedX, _robot.Position!.X);
            Assert.Equal(expectedY, _robot.Position.Y);
        }

        public static TheoryData<string, int, int> EdgeCases => new()
        {
            { "PLACE 0,4,NORTH", 0, 4 },
            { "PLACE 4,0,EAST",  4, 0 },
            { "PLACE 0,0,SOUTH", 0, 0 },
            { "PLACE 0,0,WEST",  0, 0 }
        };
    }
    
    public class RotateTests : CommandProcessorTests
    {
        [Theory]
        [MemberData(nameof(LeftRotations))]
        public void Left_RotatesAntiClockwise(string placementCommand, Direction expectedFacing) // NORTH -> WEST -> SOUTH -> EAST -> NORTH
        {
            _processor.Process(placementCommand);
            _processor.Process("LEFT");

            Assert.Equal(expectedFacing, _robot.Facing);
        }
        
        public static TheoryData<string, Direction> LeftRotations => new()
        {
            { "PLACE 0,0,NORTH", Direction.West },
            { "PLACE 0,0,WEST",  Direction.South },
            { "PLACE 0,0,SOUTH", Direction.East },
            { "PLACE 0,0,EAST",  Direction.North },
        };
        
        [Theory]
        [MemberData(nameof(RightRotations))]
        public void Right_RotatesClockwise(string placementCommand, Direction expectedFacing) // NORTH -> EAST -> SOUTH -> WEST -> NORTH
        {
            _processor.Process(placementCommand);
            _processor.Process("RIGHT");

            Assert.Equal(expectedFacing, _robot.Facing);
        }
        
        public static TheoryData<string, Direction> RightRotations => new()
        {
            { "PLACE 0,0,NORTH", Direction.East },
            { "PLACE 0,0,EAST",  Direction.South },
            { "PLACE 0,0,SOUTH", Direction.West},
            { "PLACE 0,0,WEST",  Direction.North },
        };
    }
    
    public class PlaceTests : CommandProcessorTests
    {
        [Theory]
        [MemberData(nameof(Placements))]
        public void ValidPlace_RobotIsPlacedOnTable(string command, Direction expectedFacing, int expectedX, int expectedY)
        {
            _processor.Process(command);

            Assert.True(_robot.IsPlaced);
            Assert.Equal(expectedFacing, _robot.Facing);
            Assert.Equal(expectedX, _robot.Position!.X);
            Assert.Equal(expectedY, _robot.Position.Y);
        }
        
        public static TheoryData<string, Direction, int, int> Placements => new()
        {
            { "PLACE 1,2,NORTH", Direction.North, 1, 2 },
            { "PLACE 0,4,EAST",  Direction.East , 0, 4 },
            { "PLACE 3,0,SOUTH", Direction.South, 3, 0 },
            { "PLACE 2,1,WEST",  Direction.West , 2, 1 }
        };

        [Fact]
        public void Place_OverridesPreviousPlace()
        {
            _processor.Process("PLACE 1,2,NORTH");
            _processor.Process("PLACE 2,4,WEST");

            const Direction expectedFacing = Direction.West;
            const int expectedX = 2;
            const int expectedY = 4;

            Assert.True(_robot.IsPlaced);
            Assert.Equal(expectedFacing, _robot.Facing);
            Assert.Equal(expectedX, _robot.Position!.X);
            Assert.Equal(expectedY, _robot.Position.Y);
        }
        
        [Fact]
        public void Place_OverridesPositionAfterMove()
        {
            _processor.Process("PLACE 1,2,SOUTH");
            _processor.Process("MOVE");
            _processor.Process("PLACE 2,4,WEST");

            const Direction expectedFacing = Direction.West;
            const int expectedX = 2;
            const int expectedY = 4;

            Assert.True(_robot.IsPlaced);
            Assert.Equal(expectedFacing, _robot.Facing);
            Assert.Equal(expectedX, _robot.Position!.X);
            Assert.Equal(expectedY, _robot.Position.Y);
        }
        
        [Theory]
        [MemberData(nameof(OutOfBoundsPlacements))]
        public void Place_OutsideBounds_FirstCommand_IsIgnored(string command) 
        {
            _processor.Process(command);

            Assert.False(_robot.IsPlaced);
        }
        
        [Theory]
        [MemberData(nameof(OutOfBoundsPlacements))]
        public void Place_OutsideBounds_Subsequent_DoesNotChangeRobot(string command)
        {
            _processor.Process("PLACE 1,2,NORTH");
            _processor.Process(command);
            
            const Direction expectedFacing = Direction.North;
            const int expectedX = 1;
            const int expectedY = 2;

            Assert.True(_robot.IsPlaced);
            Assert.Equal(expectedFacing, _robot.Facing);
            Assert.Equal(expectedX, _robot.Position!.X);
            Assert.Equal(expectedY, _robot.Position.Y);
        }

        public static TheoryData<string> OutOfBoundsPlacements =>
        [
            "PLACE 5,0,NORTH",
            "PLACE 0,5,NORTH",
            "PLACE 5,5,NORTH",
            "PLACE -1,0,NORTH",
            "PLACE 0,-1,NORTH"
        ];
    }
    
    public class InvalidCommandTests : CommandProcessorTests
    {
        [Theory]
        [MemberData(nameof(InvalidCommands))]
        public void InvalidCommand_IsIgnored(string command)
        {
            _processor.Process("PLACE 4,3,NORTH");
            _processor.Process(command);
            const int expectedX = 4;
            const int expectedY = 3;
            const Direction expectedFacing = Direction.North;
            
            Assert.True(_robot.IsPlaced);
            Assert.Equal(expectedFacing, _robot.Facing);
            Assert.Equal(expectedX, _robot.Position!.X);
            Assert.Equal(expectedY, _robot.Position.Y);
        }

        public static TheoryData<string> InvalidCommands =>
        [
            "",
            "    ",
            "PING",
            "PLACE f,0,NORTH",
            "PLACE 1,0",
            "PLACE 1,NORTH",
            "PLACE NORTH",
            "PLACE",
            "PLACE ",
            "PLACE ,",
            "PLACE ,,",
            "PLACE 1,2,0 NORTH", // Extra position
            "PLACE 1,2, NORTH", // Extra space
            "PLACE 1,2, NORTH, WEST",
            "MOVE LEFT",
            "MOVE 1,2,NORTH",
            "MOVE PLACE 1,2,NORTH",
            "RIGHT PLACE 1,2,NORTH",
            "LEFT PLACE 1,2,NORTH",
            "REPORT PLACE 1,2,NORTH",
            "PLACE 1,2,NORTH MOVE",
            "PLACE 1,2,NORTH LEFT",
            "PLACE 1,2,NORTH RIGHT",
            "PLACE 1,2,NORTH REPORT",
            "LEFT RIGHT"
        ];
    }

    public class ReportCommandTests : CommandProcessorTests
    {
        [Fact]
        public void Report_OutputPositionAndFacing()
        {
            _processor.Process("PLACE 1,2,NORTH");
            var actualOutput = CaptureOutput(() => _processor.Process("REPORT"));
            const string expectedOutput = "1,2,NORTH";
            
            Assert.Equal(expectedOutput, actualOutput);
        }

        private static string CaptureOutput(Action action)
        {
            var writer = new StringWriter();
            var originalOut =  Console.Out;
            Console.SetOut(writer);
            try
            {
                action();
            }
            finally
            {
                Console.SetOut(originalOut); // Set back
            }
            
            return writer.ToString().Trim();
        }
    }
}