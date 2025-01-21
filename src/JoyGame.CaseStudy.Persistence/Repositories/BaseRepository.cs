using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.Interfaces;
using JoyGame.CaseStudy.Application.Interfaces.Repositories;
using JoyGame.CaseStudy.Domain.Common;
using JoyGame.CaseStudy.Domain.Enums;
using JoyGame.CaseStudy.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace JoyGame.CaseStudy.Persistence.Repositories;

public class BaseRepository<TEntity>(ApplicationDbContext context) : IBaseRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly ApplicationDbContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();

    public virtual async Task<OperationResult<TEntity?>> GetByIdAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);

        if (entity == null || entity.Status == EntityStatus.Deleted)
            return OperationResult<TEntity?>.Failure(ErrorCode.EntityNotFound, "Entity not found");

        return OperationResult<TEntity?>.Success(entity);
    }

    public virtual async Task<OperationResult<List<TEntity>>> GetAllAsync()
    {
        var entities = await _dbSet
            .Where(e => e.Status != EntityStatus.Deleted)
            .ToListAsync();

        if (entities.Count == 0)
            return OperationResult<List<TEntity>>.Failure(ErrorCode.EntityNotFound, "No entities found");

        return OperationResult<List<TEntity>>.Success(entities);
    }

    public virtual async Task<OperationResult<TEntity>> AddAsync(TEntity entity)
    {
        var addResult = await _dbSet.AddAsync(entity);

        if (addResult.State != EntityState.Added)
            return OperationResult<TEntity>.Failure(ErrorCode.DatabaseError, "Error adding entity");

        var saveResult = await _context.SaveChangesAsync();

        if (saveResult == 0)
            return OperationResult<TEntity>.Failure(ErrorCode.DatabaseError, "Error saving entity");

        return OperationResult<TEntity>.Success(entity);
    }

    public virtual async Task<OperationResult<TEntity>> UpdateAsync(TEntity entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        var saveResult = await _context.SaveChangesAsync();

        if (saveResult == 0)
            return OperationResult<TEntity>.Failure(ErrorCode.DatabaseError, "Error saving entity");

        return OperationResult<TEntity>.Success(entity);
    }

    public virtual async Task<OperationResult<bool>> DeleteAsync(int id)
    {
        var entityOperationResult = await GetByIdAsync(id);

        if (entityOperationResult.Data == null)
            return OperationResult<bool>.Failure(ErrorCode.EntityNotFound, "Entity not found");

        entityOperationResult.Data.Status = EntityStatus.Deleted;
        var saveResult = await _context.SaveChangesAsync();

        if (saveResult == 0)
            return OperationResult<bool>.Failure(ErrorCode.DatabaseError, "Error deleting entity");

        return OperationResult<bool>.Success(true);
    }

    public virtual async Task<OperationResult<bool>> ExistsAsync(int id)
    {
        var anyResult = await _dbSet.AnyAsync(e => e.Id == id);

        return OperationResult<bool>.Success(anyResult);
    }
}