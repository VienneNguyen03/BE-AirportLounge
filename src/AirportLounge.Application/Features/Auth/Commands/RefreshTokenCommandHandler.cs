using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.ValidateAccessToken(request.AccessToken);

        var user = userId.HasValue
            ? await _unitOfWork.Users.Query()
                .FirstOrDefaultAsync(u => u.Id == userId.Value && u.IsActive, cancellationToken)
            : null;

        if (user is null || user.RefreshToken != request.RefreshToken ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return Result<LoginResponse>.Failure("Invalid or expired refresh token");
        }

        var newAccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LoginResponse>.Success(new LoginResponse(
            newAccessToken,
            newRefreshToken,
            DateTime.UtcNow.AddMinutes(60),
            new UserDto(user.Id, user.EmployeeCode, user.FullName, user.Email, user.Role.ToString())));
    }
}
