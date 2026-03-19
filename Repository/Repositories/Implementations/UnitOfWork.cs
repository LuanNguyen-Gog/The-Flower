using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Repository.Models;
using Repository.Repositories.Interfaces;

namespace Repository.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly SalesAppDBContext _context;
    private IDbContextTransaction? _transaction;

    // Lazy-loaded repositories
    private IUserRepository? _userRepository;
    private IProductRepository? _productRepository;
    private ICartRepository? _cartRepository;
    private IOrderRepository? _orderRepository;
    private INotificationRepository? _notificationRepository;
    private IChatRepository? _chatRepository;
    private IStoreLocationRepository? _storeLocationRepository;


    public UnitOfWork(SalesAppDBContext context)
    {
        _context = context;
    }

    // Repository properties with lazy initialization
    public IUserRepository Users
    {
        get { return _userRepository ??= new UserRepository(_context); }
    }

    public IProductRepository Products
    {
        get { return _productRepository ??= new ProductRepository(_context); }
    }

    public ICartRepository Carts
    {
        get { return _cartRepository ??= new CartRepository(_context); }
    }

    public IOrderRepository Orders
    {
        get { return _orderRepository ??= new OrderRepository(_context); }
    }

    public INotificationRepository Notifications
    {
        get { return _notificationRepository ??= new NotificationRepository(_context); }
    }

    public IChatRepository Chats
    {
        get { return _chatRepository ??= new ChatRepository(_context); }
    }

    public IStoreLocationRepository StoreLocations
    {
        get { return _storeLocationRepository ??= new StoreLocationRepository(_context); }
    }

    /// <summary>
    /// Saves all changes made to the context
    /// </summary>
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes an operation within a transaction
    /// If the operation succeeds, the transaction is committed automatically
    /// If an exception occurs, the transaction is rolled back
    /// </summary>
    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation)
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = await operation();
            await _transaction.CommitAsync();
            return result;
        }
        catch
        {
            await _transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Executes an operation within a transaction (no return value)
    /// </summary>
    public async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        _transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await operation();
            await _transaction.CommitAsync();
        }
        catch
        {
            await _transaction.RollbackAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
