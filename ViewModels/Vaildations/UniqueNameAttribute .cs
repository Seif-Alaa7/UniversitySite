using Data;
using System.ComponentModel.DataAnnotations;

namespace HelwanUniversity.Vaildations
{
    public class UniqueNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var context = validationContext.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;

            if (context == null)
            {
                return new ValidationResult("Database context is not available.");
            }

            if (value != null)
            {
                var Name = value.ToString();

                var DepartmentExists = context.Departments.Any(u => u.Name == Name);
                var FacultyExists = context.Faculties.Any(u => u.Name == Name);
                var SubjectExists = context.Subjects.Any(u => u.Name == Name);

                if (DepartmentExists)
                {
                    return new ValidationResult("This Department is already exists.");
                }
                else if (FacultyExists)
                {
                    return new ValidationResult("This Faculty is already exists.");
                }
                else if (SubjectExists)
                {
                    return new ValidationResult("This Subject is already exists.");
                }
            }
            return ValidationResult.Success!;
        }
    }
}

