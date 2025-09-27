namespace AccountManagement.Services.Interfaces;

public interface IService<T> where T : class
{
    Task CreateAsync(T account, Guid userId);
    Task<T> GetAsync(Guid userId);
    Task<List<T>> GetAllAsync(Guid userId);
    Task UpdateAsync(T account, Guid userId);
    Task DeleteAsync(Guid accountId, Guid userId);
}
