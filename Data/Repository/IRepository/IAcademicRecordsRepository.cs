﻿using Models;
using Models.Enums;

namespace Data.Repository.IRepository
{
    public interface IAcademicRecordsRepository
    {
        List<AcademicRecords> GetAll();
        void Update(AcademicRecords academicRecords);
        void Save();
        decimal CalculateGpaSemester(int studentId, Semester semester);
        decimal CalculateGPATotal(int studentId);
        void DeleteByStudent(int studentId);
        AcademicRecords GetStudent(int id);
        Dictionary<int, (Level Level, Semester Semester)> GetLevelANDSemester(List<Student> students);
        List<object> GetChartData(int departmentId);
        decimal FindAvgGPAByDepartmentAndFilters(int departmentId, Level? level, Gender? gender);
        decimal FindAvgGPAByDepartment(int departmentId);
        decimal FindAvgGPAByDepartmentAndLevel(int departmentId, Level level);
        decimal FindAvgGPAByDepartmentLevelGender(int departmentId, Level level, Gender gender);
        public List<(int SubjectId, string SubjectName, double AvgGpa)> GetLowestAvgGpaSubjectsByDepartment(int departmentId, int top);
        List<(int SubjectId, string SubjectName, double Rate)> GetSubjectRateByDepartment(int departmentId, int top, bool isSuccessRate);
    }
}
