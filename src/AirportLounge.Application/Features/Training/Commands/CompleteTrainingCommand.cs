using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Training.Commands;

public record CompleteTrainingCommand(
    Guid EnrollmentId,
    decimal Score,
    string? CertificateUrl) : IRequest<Result<bool>>;

public class CompleteTrainingCommandHandler : IRequestHandler<CompleteTrainingCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CompleteTrainingCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(CompleteTrainingCommand request, CancellationToken ct)
    {
        var enrollment = await _uow.TrainingEnrollments.GetByIdAsync(request.EnrollmentId, ct);
        if (enrollment is null)
            return Result<bool>.Failure("Enrollment not found");

        if (enrollment.Status == EnrollmentStatus.Completed || enrollment.Status == EnrollmentStatus.Failed)
            return Result<bool>.Failure("Enrollment is already finalized");

        var course = await _uow.TrainingCourses.GetByIdAsync(enrollment.CourseId, ct);
        if (course is null)
            return Result<bool>.Failure("Associated course not found");

        enrollment.Score = request.Score;
        enrollment.CompletedAt = DateTime.UtcNow;
        enrollment.CertificateUrl = request.CertificateUrl;
        enrollment.Status = request.Score >= course.PassingScore
            ? EnrollmentStatus.Completed
            : EnrollmentStatus.Failed;
        enrollment.UpdatedBy = _currentUser.Email;

        _uow.TrainingEnrollments.Update(enrollment);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.TrainingEnrollments(enrollment.EmployeeId), ct);

        return Result<bool>.Success(true, enrollment.Status == EnrollmentStatus.Completed
            ? "Training completed successfully"
            : "Training completed with a failing score");
    }
}
