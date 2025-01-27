using Microsoft.AspNetCore.Mvc.Rendering;
using Models;
using Models.Enums;

namespace Data.Repository.IRepository
{
    public interface ISubjectRepository
    {
        Subject GetOne(int Id);
        void Delete(Subject subject);
        void Update(Subject subject);
        void Add(Subject subject);
        List<SelectListItem> Select();
        void Save();
        bool ExistSubject(string Subject);
        List<Subject> GetSubjects(int studentID);
        IQueryable<Subject> SubjectsByDoctor(int id);
        string GetName(int id);
        Level GetLevel(int id);
        Semester GetSemester(int id);
        Dictionary<int, string> GetName(List<Subject> subjects);
        List<int> GetIds(List<Subject> subjects);
        List<SelectListItem> SelectSubjectsByFaculty(int id);
        public List<StudentSubjects> GetStudentGrades(int subjectId);
    }
}
