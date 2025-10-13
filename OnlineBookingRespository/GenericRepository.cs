using System.Linq.Expressions;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Repositories;

namespace OnlineBookingRespository;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    Task IGenericRepository<T>.AddAsync(T entity)
    {
        throw new NotImplementedException();
    }

    Task<int> IGenericRepository<T>.CountAsync(Expression<Func<T, bool>> criteria)
    {
        throw new NotImplementedException();
    }

    void IGenericRepository<T>.Delete(T entity)
    {
        throw new NotImplementedException();
    }

    void IGenericRepository<T>.DeleteRange(IEnumerable<T> entities)
    {
        throw new NotImplementedException();
    }

    Task<IReadOnlyList<T>> IGenericRepository<T>.GetAllByConditionAsync(int? pageIndex, int? pageSize, Expression<Func<T, bool>> criteria, Expression<Func<T, object>> orderBy, bool descending, bool asNoTracking, params Expression<Func<T, object>>[] includes)
    {
        throw new NotImplementedException();
    }

    Task<T?> IGenericRepository<T>.GetEntityByConditionAsync(Expression<Func<T, bool>> expression, bool asNoTracking, params Expression<Func<T, object>>[] includes)
    {
        throw new NotImplementedException();
    }

    void IGenericRepository<T>.Update(T entity)
    {
        throw new NotImplementedException();
    }
}