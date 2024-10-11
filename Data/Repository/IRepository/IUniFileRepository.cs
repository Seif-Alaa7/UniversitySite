using Models;

namespace Data.Repository.IRepository
{
    public interface IUniFileRepository
    {
        List<UniFile> GetAllImages();
        List<UniFile> GetAllVideos();
        UniFile GetFile(int id);
        void Update(UniFile File);
        void Add(UniFile File);
        void Delete(UniFile File);
        void Save();
        bool ExistVideo(string file);
    }
}
