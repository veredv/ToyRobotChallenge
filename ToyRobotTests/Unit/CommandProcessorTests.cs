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
            var newPosition = new Position(0, 1);
            _robot.IsPlaced.Returns(true);
            _robot.GetNextForwardPosition().Returns(newPosition);
            _processor.Process("MOVE");
            
            _robot.Received().MoveTo(newPosition);
        }
        
        [Theory]
        [MemberData(nameof(EdgeCases))]
        public void Move_FallingOffTable_IsIgnored(Position illegalPosition)
        {
            _robot.IsPlaced.Returns(true);
            _robot.GetNextForwardPosition().Returns(illegalPosition);
            _processor.Process("MOVE");
            
            _robot.DidNotReceive().MoveTo(Arg.Any<Position>());
        }

        public static TheoryData<Position> EdgeCases =>
        [
            new(0, 5),
            new(5, 0),
            new(-1, 4),
            new(0, -1)
        ];
    }
    
    public class RotateTests : CommandProcessorTests
    {
        [Fact]
        public void Left_DelegatesToRobot()
        {
            _robot.IsPlaced.Returns(true);
            _processor.Process("LEFT");

            _robot.Received().RotateLeft();
        }
        
        [Fact]
        public void Right_DelegatesToRobot()
        {
            _robot.IsPlaced.Returns(true);
            _processor.Process("RIGHT");

            _robot.Received().RotateRight();
        }
    }
    
    public class PlaceTests : CommandProcessorTests
    {
        [Theory]
        [MemberData(nameof(Placements))]
        public void ValidPlace_DelegatesToRobot(string command, Direction facing, Position position)
        {
            _processor.Process(command);

            _robot.Received().Place(position, facing);
        }
        
        public static TheoryData<string, Direction, Position> Placements => new()
        {
            { "PLACE 1,2,NORTH", Direction.North, new(1, 2) },
            { "PLACE 0,4,EAST",  Direction.East , new(0, 4) },
            { "PLACE 3,0,SOUTH", Direction.South, new(3, 0) },
            { "PLACE 2,1,WEST",  Direction.West , new(2, 1) }
        };

        [Fact]
        public void Place_OverridesPreviousPlace()
        {
            _processor.Process("PLACE 1,2,NORTH");
            _processor.Process("PLACE 2,4,WEST");

            _robot.Received(1).Place(new Position(1, 2), Direction.North);
            _robot.Received(1).Place(new Position(2, 4), Direction.West);
        }

        [Theory]
        [MemberData(nameof(OutOfBoundsPlacements))]
        public void Place_OutsideBounds_IsIgnored(string command) 
        {
            _processor.Process(command);

            _robot.DidNotReceive().Place(Arg.Any<Position>(),  Arg.Any<Direction>());
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
            _processor.Process(command);
            
            _robot.DidNotReceive().Place(Arg.Any<Position>(), Arg.Any<Direction>());
            _robot.DidNotReceive().RotateLeft();
            _robot.DidNotReceive().RotateRight();
            _robot.DidNotReceive().MoveTo(Arg.Any<Position>());
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
            _robot.IsPlaced.Returns(true);
            _robot.Position.Returns(new Position(1, 2));
            _robot.Facing.Returns(Direction.North);
            
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