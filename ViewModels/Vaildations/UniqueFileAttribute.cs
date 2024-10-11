using Data;
using System.ComponentModel.DataAnnotations;

namespace HelwanUniversity.Vaildations
{
    public class UniqueFileAttribute : ValidationAttribute
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
                var FileName = value.ToString();

                var FileExists = context.UniFiles.Any(m => m.File == FileName);

                if (FileExists)
                {
                    return new ValidationResult("This File is already exists.");
                }
            }
            return ValidationResult.Success!;
        }
    }
}
