namespace UhlnocsServer.Database
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private ApplicationDatabaseContext AppDbContext;

        public Repository(ApplicationDatabaseContext appDbContext)
        {
            AppDbContext = appDbContext;
        }

        public async Task Create(T entity)
        {
            var dbSet = AppDbContext.Set<T>();
            dbSet.Add(entity);
            await AppDbContext.SaveChangesAsync();
        }

        public IQueryable<T> Get()
        {
            var dbSet = AppDbContext.Set<T>();
            return dbSet.AsQueryable();
        }

        public async Task<T?> GetById(string id)
        {
            var dbSet = AppDbContext.Set<T>();
            return await dbSet.FindAsync(id);
        }

        public async Task Update(T entity)
        {
            var dbSet = AppDbContext.Set<T>();
            dbSet.Update(entity);
            await AppDbContext.SaveChangesAsync();
        }

        public async Task Delete(string id)
        {
            var entity = await GetById(id);
            if (entity != null)
            {
                var dbSet = AppDbContext.Set<T>();
                dbSet.Remove(entity);
                await AppDbContext.SaveChangesAsync();
            }
        }
    }
}
