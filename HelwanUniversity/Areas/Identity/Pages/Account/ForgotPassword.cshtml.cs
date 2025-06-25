// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Data.Repository;
using Data.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Models.Enums;
using Models;
using ViewModels.Vaildations.ApplicationUserValid;
using HelwanUniversity.Services;

namespace HelwanUniversity.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IUniFileRepository uniFileRepository;
        private readonly IActivityLogger activityLogger;
        private readonly IHighBoardRepository highBoardRepository;
        private readonly IStudentRepository studentRepository;
        private readonly IDoctorRepository doctorRepository;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender,IUniFileRepository uniFileRepository
            , IActivityLogger activityLogger, IHighBoardRepository highBoardRepository, IDoctorRepository doctorRepository,
            IStudentRepository studentRepository)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            this.uniFileRepository = uniFileRepository;
            this.activityLogger = activityLogger;
            this.highBoardRepository = highBoardRepository;
            this.studentRepository = studentRepository;
            this.doctorRepository = doctorRepository;
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
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                var applicationUserId = await _userManager.GetUserIdAsync(user);

                UserRole userRole = UserRole.Admin;
                string userName = "Unknown";
                int recordId = 0;
                string jobTitle = "";

                if (studentRepository.IsStudent(applicationUserId))
                {
                    var student = studentRepository.GetByUserId(applicationUserId);
                    userRole = UserRole.Student;
                    userName = student.Name;
                    recordId = student.Id;
                }
                else if (doctorRepository.IsDoctor(applicationUserId))
                {
                    var doctor = await doctorRepository.GetEntityByUserIdAsync(applicationUserId) as Doctor;
                    userRole = UserRole.Doctor;
                    userName = doctor.Name;
                    recordId = doctor.Id;
                    jobTitle = doctor.JobTitle.ToString();
                }
                else if (highBoardRepository.IsHighboard(applicationUserId))
                {
                    var highBoard = highBoardRepository.GetByUserId(applicationUserId);
                    userName = highBoard.Name;
                    recordId = highBoard.Id;
                    jobTitle = highBoard.JobTitle.ToString();

                    if (highBoard.JobTitle == JobTitle.President || highBoard.JobTitle == JobTitle.VicePrecident || highBoard.JobTitle == JobTitle.VP_For_AcademicAffairs)
                    {
                        userRole = UserRole.Admin;
                    }
                    else
                    {
                        userRole = UserRole.HighBoard;
                    }
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                activityLogger.Log(
                    actionType: "Password Reset Request",
                    tableName: "AspNetUsers",
                    recordId: recordId,
                    description: $"{userRole} '{userName}' requested to reset their password. Reset link sent to '{Input.Email}'.",
                    userId: recordId,
                    userName: userName,
                    userRole: userRole
                );

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
