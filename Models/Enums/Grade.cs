using System.ComponentModel.DataAnnotations;
namespace Models.Enums
{

    public enum Grade
    {
        [Display(Name = "A+")]
        APlus,
        A,
        [Display(Name = "B+")]
        BPlus,
        B,
        [Display(Name = "C+")]
        CPlus,
        C,
        D,
        F
    }
}
