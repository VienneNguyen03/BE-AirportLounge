using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<LoginResponse>>;
