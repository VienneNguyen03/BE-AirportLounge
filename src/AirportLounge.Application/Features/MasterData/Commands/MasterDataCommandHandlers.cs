using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.MasterData.Commands;

public class MasterDataCommandHandlers
    : IRequestHandler<CreateMasterDataCommand, Result<Guid>>,
      IRequestHandler<UpdateMasterDataCommand, Result<bool>>,
      IRequestHandler<DeleteMasterDataCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public MasterDataCommandHandlers(IUnitOfWork unitOfWork, ICacheService cache, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateMasterDataCommand request, CancellationToken cancellationToken)
    {
        Guid newId = Guid.NewGuid();
        string? cacheKeyToRemove = null;
        
        switch (request.Type)
        {
            case MasterDataType.Department:
                if (await _unitOfWork.Departments.Query().AnyAsync(x => x.Name == request.Name, cancellationToken))
                    return Result<Guid>.Failure("Department with this name already exists.");
                
                var newDept = new Department { Id = newId, Name = request.Name, Description = request.Description };
                await _unitOfWork.Departments.AddAsync(newDept, cancellationToken);
                cacheKeyToRemove = CacheKeys.DepartmentsList;
                break;
                
            case MasterDataType.Position:
                if (await _unitOfWork.Positions.Query().AnyAsync(x => x.Name == request.Name, cancellationToken))
                    return Result<Guid>.Failure("Position with this name already exists.");
                
                var newPos = new Position { Id = newId, Name = request.Name, Description = request.Description };
                await _unitOfWork.Positions.AddAsync(newPos, cancellationToken);
                cacheKeyToRemove = CacheKeys.PositionsList;
                break;
                
            case MasterDataType.Skill:
                if (await _unitOfWork.Skills.Query().AnyAsync(x => x.Name == request.Name, cancellationToken))
                    return Result<Guid>.Failure("Skill with this name already exists.");
                
                var newSkill = new Skill { Id = newId, Name = request.Name, Description = request.Description };
                await _unitOfWork.Skills.AddAsync(newSkill, cancellationToken);
                cacheKeyToRemove = CacheKeys.SkillsList;
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (cacheKeyToRemove != null)
        {
            await _cache.RemoveAsync(cacheKeyToRemove, cancellationToken);
        }
        
        return Result<Guid>.Success(newId, $"{request.Type} created successfully");
    }

    public async Task<Result<bool>> Handle(UpdateMasterDataCommand request, CancellationToken cancellationToken)
    {
        string? cacheKeyToRemove = null;
        
        switch (request.Type)
        {
            case MasterDataType.Department:
                var dept = await _unitOfWork.Departments.GetByIdAsync(request.Id, cancellationToken);
                if (dept == null) return Result<bool>.Failure("Department not found");
                if (dept.Name != request.Name && await _unitOfWork.Departments.Query().AnyAsync(x => x.Name == request.Name, cancellationToken))
                    return Result<bool>.Failure("Department name already exists");
                    
                dept.Name = request.Name;
                dept.Description = request.Description;
                dept.IsDeleted = !request.IsActive;
                _unitOfWork.Departments.Update(dept);
                cacheKeyToRemove = CacheKeys.DepartmentsList;
                break;
                
            case MasterDataType.Position:
                var pos = await _unitOfWork.Positions.GetByIdAsync(request.Id, cancellationToken);
                if (pos == null) return Result<bool>.Failure("Position not found");
                if (pos.Name != request.Name && await _unitOfWork.Positions.Query().AnyAsync(x => x.Name == request.Name, cancellationToken))
                    return Result<bool>.Failure("Position name already exists");
                    
                pos.Name = request.Name;
                pos.Description = request.Description;
                pos.IsDeleted = !request.IsActive;
                _unitOfWork.Positions.Update(pos);
                cacheKeyToRemove = CacheKeys.PositionsList;
                break;
                
            case MasterDataType.Skill:
                var skill = await _unitOfWork.Skills.GetByIdAsync(request.Id, cancellationToken);
                if (skill == null) return Result<bool>.Failure("Skill not found");
                if (skill.Name != request.Name && await _unitOfWork.Skills.Query().AnyAsync(x => x.Name == request.Name, cancellationToken))
                    return Result<bool>.Failure("Skill name already exists");
                    
                skill.Name = request.Name;
                skill.Description = request.Description;
                skill.IsDeleted = !request.IsActive;
                _unitOfWork.Skills.Update(skill);
                cacheKeyToRemove = CacheKeys.SkillsList;
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (cacheKeyToRemove != null)
        {
            await _cache.RemoveAsync(cacheKeyToRemove, cancellationToken);
        }
        
        return Result<bool>.Success(true, $"{request.Type} updated successfully");
    }

    public async Task<Result<bool>> Handle(DeleteMasterDataCommand request, CancellationToken cancellationToken)
    {
        string? cacheKeyToRemove = null;
        
        switch (request.Type)
        {
            case MasterDataType.Department:
                var dept = await _unitOfWork.Departments.GetByIdAsync(request.Id, cancellationToken);
                if (dept == null) return Result<bool>.Failure("Not found");
                if (await _unitOfWork.Employees.Query().AnyAsync(e => e.DepartmentId == request.Id, cancellationToken))
                    return Result<bool>.Failure("Cannot delete because it is in use by an employee");
                
                _unitOfWork.Departments.Delete(dept);
                cacheKeyToRemove = CacheKeys.DepartmentsList;
                break;

            case MasterDataType.Position:
                var pos = await _unitOfWork.Positions.GetByIdAsync(request.Id, cancellationToken);
                if (pos == null) return Result<bool>.Failure("Not found");
                if (await _unitOfWork.Employees.Query().AnyAsync(e => e.PositionId == request.Id, cancellationToken))
                    return Result<bool>.Failure("Cannot delete because it is in use by an employee");
                
                _unitOfWork.Positions.Delete(pos);
                cacheKeyToRemove = CacheKeys.PositionsList;
                break;

            case MasterDataType.Skill:
                var skill = await _unitOfWork.Skills.GetByIdAsync(request.Id, cancellationToken);
                if (skill == null) return Result<bool>.Failure("Not found");
                
                _unitOfWork.Skills.Delete(skill);
                cacheKeyToRemove = CacheKeys.SkillsList;
                break;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (cacheKeyToRemove != null)
        {
            await _cache.RemoveAsync(cacheKeyToRemove, cancellationToken);
        }
        
        return Result<bool>.Success(true, $"{request.Type} deleted successfully");
    }
}
