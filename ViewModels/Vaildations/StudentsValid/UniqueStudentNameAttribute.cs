using Data;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.Vaildations.StudentsValid
{
    public class UniqueStudentNameAttribute : ValidationAttribute
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
                var StudentName = value.ToString();

                var StudentExists = context.Students.Any(m => m.Name == StudentName);

                if (StudentExists)
                {
                    return new ValidationResult("This Name is already exists.");
                }
            }
            return ValidationResult.Success!;
        }
    }
}
