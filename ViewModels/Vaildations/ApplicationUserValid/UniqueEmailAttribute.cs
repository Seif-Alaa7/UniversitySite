using Data;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.Vaildations.ApplicationUserValid
{
    public class UniqueEmailAttribute : ValidationAttribute
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
                var EmailExists = value.ToString();

                var Email = context.ApplicationUsers.Any(u => u.Email == EmailExists);

                if (Email)
                {
                    return new ValidationResult("This Email is already exists.");
                }
            }
            return ValidationResult.Success!;
        }
    }
}
