using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Employees.Queries;

public record GetEmployeesQuery(
    string? Search,
    string? Department,
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PaginatedList<EmployeeListDto>>>;

public record EmployeeListDto(
    Guid Id, string EmployeeCode, string FullName, string Email,
    string Role, string? Department, string? Position, bool IsActive);

public class GetEmployeesQueryHandler : IRequestHandler<GetEmployeesQuery, Result<PaginatedList<EmployeeListDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetEmployeesQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<EmployeeListDto>>> Handle(GetEmployeesQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Employees.Query()
            .Include(e => e.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(e =>
                e.User.FullName.ToLower().Contains(search) ||
                e.User.Email.ToLower().Contains(search) ||
                e.User.EmployeeCode.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Department))
            query = query.Where(e => e.Department == request.Department);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(e => e.User.FullName)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new EmployeeListDto(
                e.Id, e.User.EmployeeCode, e.User.FullName, e.User.Email,
                e.User.Role.ToString(), e.Department, e.Position, e.User.IsActive))
            .ToListAsync(cancellationToken);

        return Result<PaginatedList<EmployeeListDto>>.Success(
            new PaginatedList<EmployeeListDto>(items, totalCount, request.PageNumber, request.PageSize));
    }
}
