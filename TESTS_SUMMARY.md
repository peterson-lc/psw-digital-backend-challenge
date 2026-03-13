# PswChallenge Unit Tests Summary

## Test Execution Results

✅ **All Tests Passing: 18/18**

### Test Projects Overview

| Project | Tests | Status | Duration |
|---------|-------|--------|----------|
| PswChallenge.Application.Tests | 7 | ✅ Passed | 136 ms |
| PswChallenge.Infra.Tests | 4 | ✅ Passed | 79 ms |
| PswChallenge.Api.Tests | 7 | ✅ Passed | 61 ms |
| **Total** | **18** | **✅ Passed** | **276 ms** |

## Test Coverage by Layer

### Application Layer (7 tests)
- **GetHolidaysQueryHandler** (3 tests)
  - `Handle_WithValidYear_ReturnsSuccessResponse` - Validates successful query execution
  - `Handle_WithEmptyResult_ReturnsSuccessResponseWithEmptyData` - Tests empty result handling
  - `Handle_WithCancellationToken_PassesTokenToService` - Verifies cancellation token propagation

- **AuthService** (4 tests)
  - `LoginAsync_WithValidCredentials_ReturnsTokenAndExpiration` - Valid login flow
  - `LoginAsync_WithInvalidEmail_ThrowsUnauthorizedAccessException` - Invalid email handling
  - `LoginAsync_WithInvalidPassword_ThrowsUnauthorizedAccessException` - Invalid password handling
  - `LoginAsync_GeneratedToken_ContainsCorrectClaims` - JWT token validation

### Infrastructure Layer (4 tests)
- **BrasilApiHolidayService** (4 tests)
  - `GetHolidaysByYearAsync_WithValidYear_ReturnsMappedHolidays` - API response mapping
  - `GetHolidaysByYearAsync_WithEmptyResponse_ReturnsEmptyList` - Empty response handling
  - `GetHolidaysByYearAsync_MapsDateCorrectly` - Date field mapping validation
  - `GetHolidaysByYearAsync_PassesCancellationTokenToApi` - Token propagation

### API Layer (7 tests)
- **HolidaysEndpoint** (3 tests)
  - `GetHolidaysAsync_WithValidYear_ReturnsOkResult` - Valid endpoint response
  - `GetHolidaysAsync_WithDifferentYears_SendsCorrectQuery` - Query parameter handling
  - `GetHolidaysAsync_WithCancellationToken_PassesTokenToMediator` - Token handling

- **AuthEndpoint** (4 tests)
  - `LoginAsync_WithValidCredentials_ReturnsSuccessResponse` - Valid login response
  - `LoginAsync_WithInvalidCredentials_ThrowsUnauthorizedAccessException` - Error handling
  - `LoginAsync_ReturnsApiResponseModel_WithCorrectStructure` - Response format validation
  - `LoginAsync_WithValidCredentials_CallsAuthServiceOnce` - Service invocation verification

## Testing Framework & Tools

- **Framework**: xUnit 2.9.3
- **Mocking**: Moq 4.20.70
- **Assertions**: FluentAssertions 6.12.1
- **Test SDK**: Microsoft.NET.Test.Sdk 17.14.1
- **Coverage Tool**: coverlet.collector 6.0.4

## Test Naming Convention

All tests follow the pattern: `MethodName_Scenario_ExpectedBehavior`

Example: `LoginAsync_WithValidCredentials_ReturnsTokenAndExpiration`

## Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/PswChallenge.Application.Tests/

# Run with coverage
dotnet test /p:CollectCoverage=true
```

