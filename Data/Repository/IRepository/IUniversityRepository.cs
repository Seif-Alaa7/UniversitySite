using Models;

namespace Data.Repository.IRepository
{
    public interface IUniversityRepository
    {
        void Update(University university);
        University? Get();
        void Save();
    }
}
