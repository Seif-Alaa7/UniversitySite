using Data.Repository.IRepository;
using Models;

namespace Data.Repository
{
    public class UniFileRepository : IUniFileRepository
    {
        private readonly ApplicationDbContext context;

        public UniFileRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public List<UniFile> GetAllImages()
        {
            var IMGs = context.UniFiles.Where(x=>x.ContentType == Models.Enums.Filetype.IMG).ToList();
            return IMGs;
        }
        public List<UniFile> GetAllVideos()
        {
            var Videos = context.UniFiles.Where(x => x.ContentType == Models.Enums.Filetype.Video).ToList();
            return Videos;
        }
        public UniFile GetFile(int id)
        {
            var file = context.UniFiles.Find(id);
            return file;
        }
        public void Update(UniFile File)
        {
            context.UniFiles.Update(File);
        }
        public void Add(UniFile File)
        {
            context.UniFiles.Add(File);
        }
        public void Delete(UniFile File)
        {
            context.UniFiles.Remove(File);
        }
        public void Save ()
        {
            context.SaveChanges();
        }
        public bool ExistVideo(string file)
        {
            var FileExists = context.UniFiles.Any(m => m.File == file);
            return FileExists;
        }
    }
}
