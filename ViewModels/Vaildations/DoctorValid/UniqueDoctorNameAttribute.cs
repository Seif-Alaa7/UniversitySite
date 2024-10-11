using Data;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.Vaildations.DoctorValid
{
    public class UniqueDoctorNameAttribute : ValidationAttribute
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
                var DoctorName = value.ToString();

                var DoctorExists = context.Doctors.Any(m => m.Name == DoctorName);

                if (DoctorExists)
                {
                    return new ValidationResult("This Name is already exists.");
                }
            }
            return ValidationResult.Success!;
        }
    }
}
