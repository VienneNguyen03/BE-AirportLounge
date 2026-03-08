using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Training.Commands;

public record EnrollInCourseCommand(
    Guid EmployeeId,
    Guid CourseId) : IRequest<Result<Guid>>;

public class EnrollInCourseCommandHandler : IRequestHandler<EnrollInCourseCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public EnrollInCourseCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(EnrollInCourseCommand request, CancellationToken ct)
    {
        var course = await _uow.TrainingCourses.GetByIdAsync(request.CourseId, ct);
        if (course is null || !course.IsActive)
            return Result<Guid>.Failure("Course not found or inactive");

        var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct);
        if (employee is null)
            return Result<Guid>.Failure("Employee not found");

        var alreadyEnrolled = await _uow.TrainingEnrollments.Query()
            .AnyAsync(e => e.EmployeeId == request.EmployeeId
                        && e.CourseId == request.CourseId
                        && (e.Status == EnrollmentStatus.Enrolled || e.Status == EnrollmentStatus.InProgress), ct);

        if (alreadyEnrolled)
            return Result<Guid>.Failure("Employee is already enrolled in this course");

        var enrollment = new TrainingEnrollment
        {
            EmployeeId = request.EmployeeId,
            CourseId = request.CourseId,
            Status = EnrollmentStatus.Enrolled,
            EnrolledAt = DateTime.UtcNow,
            CreatedBy = _currentUser.Email
        };

        await _uow.TrainingEnrollments.AddAsync(enrollment, ct);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.TrainingEnrollments(request.EmployeeId), ct);

        return Result<Guid>.Success(enrollment.Id, "Enrolled in course");
    }
}
