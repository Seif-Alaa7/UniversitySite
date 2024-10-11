using Data;
using System.ComponentModel.DataAnnotations;

namespace ViewModels.Vaildations.HighBoardValid
{
    public class UniqueHBNameAttribute : ValidationAttribute
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
                var HBName = value.ToString();

                var HBExists = context.HighBoards.Any(h => h.Name == HBName);

                if (HBExists)
                {
                    return new ValidationResult("This Name is already exists.");
                }
            }
            return ValidationResult.Success!;
        }
    }
}
