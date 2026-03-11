using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.MasterData.Queries;

public record MasterDataDto(Guid Id, string Name, string? Description, bool IsActive);

// Departments
public record GetDepartmentsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<MasterDataDto>>>;

// Positions
public record GetPositionsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<MasterDataDto>>>;

// Skills
public record GetSkillsQuery(int PageNumber = 1, int PageSize = 10) : IRequest<Result<PaginatedList<MasterDataDto>>>;
