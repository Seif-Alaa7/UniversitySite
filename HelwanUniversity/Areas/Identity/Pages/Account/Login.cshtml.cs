// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ViewModels.Vaildations.ApplicationUserValid;
using Data.Repository.IRepository;

namespace HelwanUniversity.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IUniFileRepository uniFileRepository;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger,IUniFileRepository uniFileRepository)
        {
            _signInManager = signInManager;
            _logger = logger;
            this.uniFileRepository = uniFileRepository;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [EmailNotExists]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
            var imgs = uniFileRepository.GetAllImages();
            ViewData["TitleLogo"] = imgs[0].File;
            ViewData["ImgSignIn"] = imgs[3].File;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);

                if (user == null)
                {
                    ModelState.AddModelError("Input.Email", "Email does not exist.");

                    var Imgs = uniFileRepository.GetAllImages();
                    ViewData["TitleLogo"] = Imgs[0].File;
                    ViewData["ImgSignIn"] = Imgs[3].File;
                    return Page();
                }

                var result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    var roles = await _signInManager.UserManager.GetRolesAsync(user);

                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "University", new { area = "Admin" });
                    }
                    else if (roles.Contains("Doctor"))
                    {
                        return RedirectToAction("Index", "University", new { area = "Doctors" });
                    }
                    else if (roles.Contains("Student"))
                    {
                        return RedirectToAction("Index", "University", new { area = "Students" });
                    }
                    else
                    {
                        // If the user has no role, redirect to a default area
                        return RedirectToAction("Index", "University", new { area = "User" });
                    }
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError("Input.Password", "Invalid password.");

                    var Imgs = uniFileRepository.GetAllImages();
                    ViewData["TitleLogo"] = Imgs[0].File;
                    ViewData["ImgSignIn"] = Imgs[3].File;
                    return Page();
                }
            }
            var imgs = uniFileRepository.GetAllImages();
            ViewData["TitleLogo"] = imgs[0].File;
            ViewData["ImgSignIn"] = imgs[3].File;
            return Page();
        }
    }

}
