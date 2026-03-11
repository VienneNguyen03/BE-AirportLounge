using AirportLounge.Application.Features.Employees.Commands;
using AirportLounge.Domain.Enums;
using FluentValidation.TestHelper;
using Xunit;

namespace AirportLounge.Tests.Validators;

public class CreateEmployeeCommandValidatorTests
{
    private readonly CreateEmployeeCommandValidator _validator = new();

    [Fact]
    public void ValidCommand_ShouldNotHaveErrors()
    {
        var command = new CreateEmployeeCommand(
            "EMP001", "John Doe", "john@test.com", "0901234567",
            "Password123", UserRole.Staff, null, null, "VIP Service",
            null, DateTime.UtcNow.Date,
            null, null, null, null,
            null, null, null, null, null,
            null, null, null, null, null,
            null);

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyEmployeeCode_ShouldHaveError(string? code)
    {
        var command = new CreateEmployeeCommand(
            code!, "John Doe", "john@test.com", null,
            "Password123", UserRole.Staff, null, null, null,
            null, DateTime.UtcNow.Date,
            null, null, null, null,
            null, null, null, null, null,
            null, null, null, null, null,
            null);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.EmployeeCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    public void InvalidEmail_ShouldHaveError(string email)
    {
        var command = new CreateEmployeeCommand(
            "EMP001", "John Doe", email, null,
            "Password123", UserRole.Staff, null, null, null,
            null, DateTime.UtcNow.Date,
            null, null, null, null,
            null, null, null, null, null,
            null, null, null, null, null,
            null);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void ShortPassword_ShouldHaveError()
    {
        var command = new CreateEmployeeCommand(
            "EMP001", "John Doe", "john@test.com", null,
            "12345", UserRole.Staff, null, null, null,
            null, DateTime.UtcNow.Date,
            null, null, null, null,
            null, null, null, null, null,
            null, null, null, null, null,
            null);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void ValidManagerRole_ShouldNotHaveError()
    {
        var command = new CreateEmployeeCommand(
            "MGR001", "Manager Name", "manager@test.com", null,
            "Manager@123", UserRole.Manager, null, null, null,
            null, DateTime.UtcNow.Date,
            null, null, null, null,
            null, null, null, null, null,
            null, null, null, null, null,
            null);

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
