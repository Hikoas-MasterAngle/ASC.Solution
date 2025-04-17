using ASC.Model.BaseTypes;
using ASC.Web.Areas.Accounts.Models;
using ASC.Web.Extensions;
using ASC.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASC.Web.Areas.Accounts.Controllers
{
    [Authorize]
    [Area("Accounts")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly Microsoft.AspNetCore.Identity.UI.Services.IEmailSender _emailSender;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ServiceEngineers()
        {
            var serviceEngineers = await _userManager.GetUsersInRoleAsync(Roles.Engineer.ToString());
            HttpContext.Session.SetSession("ServiceEngineers", serviceEngineers);

            return View(new ServiceEngineerViewModel
            {
                ServiceEngineers = serviceEngineers?.ToList(),
                Registration = new ServiceEngineerRegistrationViewModel { IsEdit = false }
            });
        }

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            var customers = await _userManager.GetUsersInRoleAsync(Roles.User.ToString());
            HttpContext.Session.SetSession("Customers", customers);
            return View(new CustomerViewModel
            {
                Customers = customers == null ? null : customers.ToList(),
                Registration = new CustomerRegistrationViewModel() { IsEdit = false }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Customers(CustomerViewModel customer)
        {
            customer.Customers = ASC.Web.Extensions.SessionExtensions.GetSession<List<IdentityUser>>(HttpContext.Session, "Customers");
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            if (customer.Registration.IsEdit)
            {
                var user = await _userManager.FindByEmailAsync(customer.Registration.Email);
                var identity = await _userManager.GetClaimsAsync(user);
                var isActiveClaim = identity.SingleOrDefault(p => p.Type == "IsActive");
                var removeClaimResult = await _userManager.RemoveClaimAsync(user, new Claim(isActiveClaim.Type, isActiveClaim.Value));
                var addClaimResult = await _userManager.AddClaimAsync(user, new Claim(isActiveClaim.Type, customer.Registration.IsActive.ToString()));

                if (customer.Registration.IsActive)
                {
                    await _emailSender.SendEmailAsync(customer.Registration.Email, "Account Modified", $"Your account has been activated, Email: {customer.Registration.Email}");
                }
                else
                {
                    await _emailSender.SendEmailAsync(customer.Registration.Email, "Account Deactivated", $"Your account has been deactivated.");
                }
            }

            return RedirectToAction("Customers");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceEngineers(ServiceEngineerViewModel serviceEngineer)
        {
            serviceEngineer.ServiceEngineers = ASC.Web.Extensions.SessionExtensions.GetSession<List<IdentityUser>>(HttpContext.Session, "ServiceEngineers");

            if (!ModelState.IsValid)
            {
                return View(serviceEngineer);
            }

            var user = await _userManager.FindByEmailAsync(serviceEngineer.Registration.Email);
            if (serviceEngineer.Registration.IsEdit)
            {
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return View(serviceEngineer);
                }

                user.UserName = serviceEngineer.Registration.UserName;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    result.Errors.ToList().ForEach(err => ModelState.AddModelError("", err.Description));
                    return View(serviceEngineer);
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, serviceEngineer.Registration.Password);
                if (!passwordResult.Succeeded)
                {
                    passwordResult.Errors.ToList().ForEach(err => ModelState.AddModelError("", err.Description));
                    return View(serviceEngineer);
                }

                var claims = await _userManager.GetClaimsAsync(user);
                var isActiveClaim = claims.FirstOrDefault(c => c.Type == "IsActive");
                if (isActiveClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, isActiveClaim);
                }
                await _userManager.AddClaimAsync(user, new Claim("IsActive", serviceEngineer.Registration.IsActive.ToString()));
            }
            else
            {
                if (user != null)
                {
                    ModelState.AddModelError("", "User already exists.");
                    return View(serviceEngineer);
                }

                var newUser = new IdentityUser
                {
                    UserName = serviceEngineer.Registration.UserName,
                    Email = serviceEngineer.Registration.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newUser, serviceEngineer.Registration.Password);
                if (!result.Succeeded)
                {
                    result.Errors.ToList().ForEach(err => ModelState.AddModelError("", err.Description));
                    return View(serviceEngineer);
                }

                await _userManager.AddClaimAsync(newUser, new Claim(ClaimTypes.Email, newUser.Email));
                await _userManager.AddClaimAsync(newUser, new Claim("IsActive", serviceEngineer.Registration.IsActive.ToString()));

                var roleResult = await _userManager.AddToRoleAsync(newUser, Roles.Engineer.ToString());
                if (!roleResult.Succeeded)
                {
                    roleResult.Errors.ToList().ForEach(err => ModelState.AddModelError("", err.Description));
                    return View(serviceEngineer);
                }
            }

            var subject = serviceEngineer.Registration.IsActive ? "Account Created/Modified" : "Account Deactivated";
            var message = serviceEngineer.Registration.IsActive
                ? $"Email: {serviceEngineer.Registration.Email}\nPassword: {serviceEngineer.Registration.Password}"
                : "Your account has been deactivated.";

            await _emailSender.SendEmailAsync(serviceEngineer.Registration.Email, subject, message);

            return RedirectToAction("ServiceEngineers");
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var user = HttpContext.User.GetCurrentUserDetails();
            return View(new ProfileModel() { UserName = user.Name });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileModel profile)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _userManager.FindByEmailAsync(HttpContext.User.GetCurrentUserDetails().Email);
            user.UserName = profile.UserName;
            IdentityResult result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                result.Errors.ToList().ForEach(p => ModelState.AddModelError("", p.Description));
                return View();
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Dashboard", "Dashboard", new { area = "ServiceRequests" });
        }
    }
}
