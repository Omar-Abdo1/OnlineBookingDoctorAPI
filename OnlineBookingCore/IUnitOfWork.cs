using Microsoft.EntityFrameworkCore;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Repositories;

namespace OnlineBookingCore;

public interface IUnitOfWork : IAsyncDisposable
{
    IGenericRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> CompleteAsync();
    public DbContext Context { get; }
}