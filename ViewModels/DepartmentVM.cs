namespace ViewModels
{
    public class DepartmentVM
    {
        public int Id { get; set; }
        public int HeadId { get; set; }
        public string Name { get; set; } = null!;
        public int FacultyId { get; set; }
        public int Allowed { get; set; }
    }
}
