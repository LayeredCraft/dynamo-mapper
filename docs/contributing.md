# Contributing

Thank you for your interest in contributing to DynamoMapper!

## Development Setup

1. Clone the repository
2. Install the SDK specified in `global.json`. This checkout requires .NET 11 preview.
3. Run `dotnet restore`
4. Run `dotnet build`

## Running Tests

```bash
dotnet test
```

## Pull Requests

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## Code Style

- Follow standard C# conventions
- Add XML documentation comments to public APIs
- Keep generated code readable

See `AGENTS.md` in the repository for implementation guidelines.

## Documentation

Install [uv](https://docs.astral.sh/uv/) and build with the locked dependencies:

```bash
uv sync --locked
uv run mkdocs build --strict
```
