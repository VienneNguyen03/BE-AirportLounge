using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Auth.Commands;

/// <param name="EmailOrPhone">Email or phone number for login</param>
public record LoginCommand(string EmailOrPhone, string Password) : IRequest<Result<LoginResponse>>;

public record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Role);
