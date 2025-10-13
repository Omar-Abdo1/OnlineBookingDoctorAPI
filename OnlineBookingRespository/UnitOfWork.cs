using Microsoft.EntityFrameworkCore;
using OnlineBookingCore;
using OnlineBookingCore.Repositories;

namespace OnlineBookingRespository;

public class UnitOfWork : IUnitOfWork
{
    DbContext IUnitOfWork.Context => throw new NotImplementedException();

    Task<int> IUnitOfWork.CompleteAsync()
    {
        throw new NotImplementedException();
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        throw new NotImplementedException();
    }

    IGenricRepository<T> IUnitOfWork.Repository<T>()
    {
        throw new NotImplementedException();
    }
}