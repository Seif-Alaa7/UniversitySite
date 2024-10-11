using Data;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.Vaildations.StudentsValid
{
    public class UniqueSPhoneNumberAttribute : ValidationAttribute
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
                var PhoneNumber = value.ToString();

                var PhoneNumberExists = context.Students.Any(x => x.PhoneNumber == PhoneNumber);

                if (PhoneNumberExists)
                {
                    return new ValidationResult("This phone number is already associated with another account.");
                }
            }
            return ValidationResult.Success!;
        }
    }
}
