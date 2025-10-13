using System.Collections;
using Microsoft.EntityFrameworkCore;
using OnlineBookingCore;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Repositories;
using OnlineBookingRespository.Data;

namespace OnlineBookingRespository;

public class UnitOfWork : IUnitOfWork
{
    private OnlineBookingContext _context;
    private readonly Hashtable _repositories;
    // key value pair   NameOfModel : GenericRepository<Model>  string->object

    public UnitOfWork(OnlineBookingContext context)
    {
        _context = context;
        _repositories = new Hashtable();
    }

    public ValueTask DisposeAsync() =>  _context.DisposeAsync();

    public DbContext Context { get => _context; }

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        var Type = typeof(T).Name;
        if (!_repositories.ContainsKey(Type)) // First Time
        {
            var Repository = new GenericRepository<T>(_context);
            _repositories.Add(Type, Repository);
        }
        return  _repositories[Type] as IGenericRepository<T>;
    }

    public async Task<int> CompleteAsync()  => await _context.SaveChangesAsync();
}