using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.MasterData.Commands;

public enum MasterDataType
{
    Department,
    Position,
    Skill
}

public record CreateMasterDataCommand(MasterDataType Type, string Name, string? Description) : IRequest<Result<Guid>>;

public record UpdateMasterDataCommand(MasterDataType Type, Guid Id, string Name, string? Description, bool IsActive) : IRequest<Result<bool>>;

public record DeleteMasterDataCommand(MasterDataType Type, Guid Id) : IRequest<Result<bool>>;
