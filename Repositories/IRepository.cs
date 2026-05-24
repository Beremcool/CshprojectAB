namespace TaskHub.Repositories;

public interface IRepository<T> : IDisposable
{
    Task SaveAsync(List<T> items);

    Task<List<T>> LoadAsync();
}
