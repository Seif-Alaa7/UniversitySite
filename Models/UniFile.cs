using Models.Enums;

namespace Models
{
    public class UniFile
    {
        public int Id { get; set; }
        public string File { get; set; } = null!;
        public Filetype ContentType  { get; set; }
    }
}
