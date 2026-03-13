# Testing Guide for PswChallenge

## Project Structure

```
tests/
├── PswChallenge.Application.Tests/
│   ├── Queries/
│   │   └── GetHolidaysQueryHandlerTests.cs
│   └── Services/
│       └── AuthServiceTests.cs
├── PswChallenge.Api.Tests/
│   └── Endpoints/
│       ├── AuthEndpointTests.cs
│       └── HolidaysEndpointTests.cs
└── PswChallenge.Infra.Tests/
    └── ExternalServices/
        └── BrasilApiHolidayServiceTests.cs
```

## Test Organization

### Application Layer Tests
Tests for business logic, queries, and services:
- **GetHolidaysQueryHandler**: CQRS query handler with MediatR
- **AuthService**: JWT token generation and credential validation

### Infrastructure Layer Tests
Tests for external service integrations:
- **BrasilApiHolidayService**: API response mapping and type conversion

### API Layer Tests
Tests for endpoint behavior and HTTP contracts:
- **HolidaysEndpoint**: GET /holidays/{year} endpoint
- **AuthEndpoint**: POST /auth/login endpoint

## Running Tests

### All Tests
```bash
dotnet test
```

### Specific Test Project
```bash
dotnet test tests/PswChallenge.Application.Tests/
dotnet test tests/PswChallenge.Infra.Tests/
dotnet test tests/PswChallenge.Api.Tests/
```

### Specific Test Class
```bash
dotnet test --filter "ClassName=GetHolidaysQueryHandlerTests"
```

### Specific Test Method
```bash
dotnet test --filter "Name=Handle_WithValidYear_ReturnsSuccessResponse"
```

### With Code Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Patterns Used

### Arrange-Act-Assert (AAA)
All tests follow the AAA pattern for clarity:
```csharp
[Fact]
public async Task TestName_Scenario_ExpectedBehavior()
{
    // Arrange - Setup test data and mocks
    var input = new TestData();
    _mockService.Setup(x => x.Method(input)).ReturnsAsync(result);

    // Act - Execute the code under test
    var actual = await _handler.Handle(input, CancellationToken.None);

    // Assert - Verify the results
    actual.Should().NotBeNull();
    actual.Succeeded.Should().BeTrue();
}
```

### Mocking with Moq
```csharp
var mockService = new Mock<IService>();
mockService
    .Setup(x => x.MethodAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(expectedResult);

// Verify calls
mockService.Verify(x => x.MethodAsync(year, token), Times.Once);
```

### Assertions with FluentAssertions
```csharp
result.Should().NotBeNull();
result.Succeeded.Should().BeTrue();
result.Data.Should().HaveCount(2);
result.Data.Should().ContainEquivalentOf(expectedItem);
```

## Coverage Goals

- **Application Layer**: High coverage (>90%) - Core business logic
- **Infrastructure Layer**: High coverage (>85%) - External integrations
- **API Layer**: Medium coverage (>70%) - Endpoint contracts

## Adding New Tests

1. Create test file in appropriate `Tests` project
2. Follow naming convention: `ClassNameTests.cs`
3. Use `[Fact]` for simple tests, `[Theory]` for parameterized tests
4. Follow AAA pattern
5. Use descriptive test names
6. Mock external dependencies
7. Run tests: `dotnet test`

