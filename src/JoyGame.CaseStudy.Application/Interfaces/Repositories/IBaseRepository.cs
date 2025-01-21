using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Domain.Common;

namespace JoyGame.CaseStudy.Application.Interfaces.Repositories;

public interface IBaseRepository<TEntity> where TEntity : BaseEntity
{
    Task<OperationResult<TEntity?>> GetByIdAsync(int id);
    Task<OperationResult<List<TEntity>>> GetAllAsync();
    Task<OperationResult<TEntity>> AddAsync(TEntity entity);
    Task<OperationResult<TEntity>> UpdateAsync(TEntity entity);
    Task<OperationResult<bool>> DeleteAsync(int id);
    Task<OperationResult<bool>> ExistsAsync(int id);
}