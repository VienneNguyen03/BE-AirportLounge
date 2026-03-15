using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Employees.Queries;

public record GetEmployeesQuery(
    string? Search,
    string? Department,
    int PageNumber = 1,
    int PageSize = 10,
    bool IncludeInactive = false
) : IRequest<Result<PaginatedList<EmployeeListDto>>>;

public record EmployeeListDto(
    Guid Id, string EmployeeCode, string FullName, string Email, string PhoneNumber,
    string Role, string? Department, string? Position, bool IsActive);

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, Result<PaginatedList<EmployeeListDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEmployeesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<EmployeeListDto>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.Employee> query = _unitOfWork.Employees.Query();

        if (request.IncludeInactive)
        {
            query = query.IgnoreQueryFilters();
        }

        query = query
            .Include(e => e.User)
            .Include(e => e.Department)
            .Include(e => e.Position);

        if (!request.IncludeInactive)
        {
            query = query.Where(e => e.User.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(e =>
                e.User.FullName.ToLower().Contains(search) ||
                e.User.Email.ToLower().Contains(search) ||
                e.EmployeeCode.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
            query = query.Where(e => e.Department != null && e.Department.Name == request.Department);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.User.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EmployeeListDto(
                e.Id, e.EmployeeCode, e.User.FullName, e.User.Email, e.User.PhoneNumber ?? string.Empty,
                e.User.Role.ToString(), e.Department != null ? e.Department.Name : null, e.Position != null ? e.Position.Name : null, e.User.IsActive))
            .ToListAsync(cancellationToken);

        return Result<PaginatedList<EmployeeListDto>>.Success(
            new PaginatedList<EmployeeListDto>(items, totalCount, request.PageNumber, request.PageSize));
    }
}
