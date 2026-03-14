namespace Repository.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    ICartRepository Carts { get; }
    IOrderRepository Orders { get; }
    INotificationRepository Notifications { get; }
    IChatRepository Chats { get; }
    IStoreLocationRepository StoreLocations { get; }
    IOtpRepository Otps { get; }

    Task<int> SaveChangesAsync();
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation);
    Task ExecuteInTransactionAsync(Func<Task> operation);
}
