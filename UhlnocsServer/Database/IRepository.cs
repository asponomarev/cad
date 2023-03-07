namespace UhlnocsServer.Database
{
    public interface IRepository<T> where T : class
    {
        Task Create(T entity);

        IQueryable<T> Get();

        Task<T?> GetById(string id);

        Task Update(T entity);

        Task Delete(string id);        
    }
}
