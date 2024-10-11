using Data.Repository.IRepository;
using Models;
namespace Data.Repository
{
    public class UniversityRepository : IUniversityRepository
    {
        private readonly ApplicationDbContext context;
        public UniversityRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public void Update(University university)
        {
            context.University.Update(university);
        }
        public University? Get()
        {
            var university = context.University.Find(1);
            return university;
        }
        public void Save()
        {
            context.SaveChanges();
        }
    }
}
