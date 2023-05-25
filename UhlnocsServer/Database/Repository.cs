namespace UhlnocsServer.Database
{
    // this class implements the most common queries to db
    public class Repository<T> : IRepository<T> where T : class
    {
        public Repository()
        {

        }

        public async Task Create(T entity)
        {
            ApplicationDatabaseContext appDbContext = new();
            var dbSet = appDbContext.Set<T>();
            dbSet.Add(entity);
            await appDbContext.SaveChangesAsync();
        }

        public IQueryable<T> Get()
        {
            ApplicationDatabaseContext appDbContext = new();
            var dbSet = appDbContext.Set<T>();
            return dbSet.AsQueryable();
        }

        public async Task<T?> GetById(string id)
        {
            ApplicationDatabaseContext appDbContext = new();
            var dbSet = appDbContext.Set<T>();
            return await dbSet.FindAsync(id);
        }

        public async Task Update(T entity)
        {
            ApplicationDatabaseContext appDbContext = new();
            var dbSet = appDbContext.Set<T>();
            dbSet.Update(entity);
            await appDbContext.SaveChangesAsync();
        }

        public async Task Delete(string id)
        {
            ApplicationDatabaseContext appDbContext = new();
            var entity = await GetById(id);
            if (entity != null)
            {
                var dbSet = appDbContext.Set<T>();
                dbSet.Remove(entity);
                await appDbContext.SaveChangesAsync();
            }
        }
    }
}
