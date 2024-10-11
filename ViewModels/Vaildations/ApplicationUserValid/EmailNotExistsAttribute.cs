using Data;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.Vaildations.ApplicationUserValid
{
    public class EmailNotExistsAttribute : ValidationAttribute
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
                var Email = value.ToString();

                var EmailExists = context.ApplicationUsers.Any(u => u.Email == Email);

                if (!EmailExists)
                {
                    return new ValidationResult("Error!! This Email is Not Correct, Try Again...");
                }
            }
            return ValidationResult.Success!;
        }
    }
}
