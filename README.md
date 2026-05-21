# Toy Robot Code Challenge

## Repository access https://github.com/veredv/ToyRobotChallenge

## Design decisions

### Architecture
The application is structured around three core components:

- `Table` — defines the grid boundary and validates positions
- `Robot` — holds state (position and facing direction), executes commands when told to
- `CommandProcessor` — the operator; parses raw input, validates against the table, and delegates to the robot

The robot is deliberately "dumb" — it does what it's told without checking boundaries. The `CommandProcessor` is responsible for asking the `Table` whether a move is valid before instructing the `Robot`. This mirrors a real-world separation of concerns — the table defines the environment, the operator enforces the rules, the robot executes.

### Interface
`Robot` implements `IRobot` to allow the `CommandProcessor` to be tested in isolation using a mock, verifying delegation behaviour without coupling tests to `Robot` internals.

### Parsing
Commands are parsed strictly — exact format required, no extra spaces or arguments tolerated. `PLACE` is the only two-part command (`PLACE X,Y,F`) and is handled separately from single-word commands.

### Position
`Position` is a record struct — immutable value type. Movement produces a new `Position` rather than mutating the existing one, keeping `Robot` state changes explicit and intentional.

---

## Testing

Tests are split into three layers:

- **Unit tests (`RobotTests`, `TableTests`)** — test behaviour directly
- **Unit tests (`CommandProcessorTests`)** — test routing and delegation using a mock `IRobot` via NSubstitute
- **Integration tests (`IntegrationTests`)** — run the compiled binary against input files and assert on stdout

This separation ensures `Robot` logic and `CommandProcessor` routing are tested independently, with integration tests covering end-to-end scenarios from the spec.
An `IRobot` interface is used by `CommandProcessor` to make unit testing easier. I did not introduce additional abstractions where they did not provide clear value.

---

## Tradeoffs
A few minor trade-offs were made intentionally:
- Input parsing is strict rather than highly permissive. Invalid or malformed commands are ignored safely.
- `REPORT` writes directly to console output to keep the solution simple.
- Integration tests use `dotnet run` for end-to-end verification, which is slower and heavier than redirecting in-memory I/O streams using TextWriter and TextReaderbut provides confidence that the application works as a real console app.

---

## Coding Principles

The implementation aims to demonstrate:

- **Simplicity**: no unnecessary frameworks or patterns
- **Single Responsibility**: each class has one clear purpose
- **Separation of Concerns**: parsing, movement, bounds checking, and startup are separated
- **Encapsulation**: robot state is controlled through methods
- **Immutability**: `Position` is immutable
- **Testability**: unit tests and integration tests cover the behavior

---

## Running the tests

```bash
dotnet test
```

---
## Running the Application

### Prerequisites

- .NET SDK installed

### Run with an input file
```bash
dotnet run --project ToyRobotApp mycommands.txt
```

### Run with the default input file
If no file argument is supplied, the application defaults to `commands.txt` in the current directory:
```bash
dotnet run --project ToyRobotApp
```
