using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var isEmail = request.EmailOrPhone.Contains('@');
        var user = isEmail
            ? await _unitOfWork.Users.Query().FirstOrDefaultAsync(
                u => u.Email == request.EmailOrPhone && u.IsActive, cancellationToken)
            : await _unitOfWork.Users.Query().FirstOrDefaultAsync(
                u => u.PhoneNumber == request.EmailOrPhone && u.IsActive, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure("Invalid email/phone or password");

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.Role.ToString());
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(60),
            new UserDto(user.Id, user.EmployeeCode, user.FullName, user.Email, user.Role.ToString()));

        return Result<LoginResponse>.Success(response, "Login successful");
    }
}
