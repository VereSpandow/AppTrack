using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using AppTrack.Models;
using AppTrack.Helpers;
using System.Data.SqlClient;
using System.Net.Mail;


// This controller is used to Login and Logout, supports Forgot Password etc for all users

namespace AppTrack.Controllers
{
    [Authorize]
    public class AccountController : BaseController
    {

        private DevProvidentIDOCEntities db = new DevProvidentIDOCEntities();

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [Authorize]
        public ActionResult VerifyStatus(string returnUrl)
        {
            bool isUserValid = true;
            int intStatusID = 99;
            string status = "";
            int customerType = 0;
            int? flag4 = 0;  // used to check for payment method updated 

            var currentUserId = User.Identity.GetUserId();
            if (currentUserId == null)
            {
                isUserValid = false;
            }
            else
            {
                //Instantiate the UserManager in ASP.Identity system so you can look up the user in the system
                var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
                //Get the User object
                var currentUser = manager.FindById(currentUserId);
                //The currentUser object has properties that match the columns in the AspNetUsers table
                // Note when assigning nullable column to int variable the ?? say set it to 0 if it is null
                if (currentUser == null)
                {
                    isUserValid = false;
                }

                DisplayName = currentUser.DisplayName ?? "";
                AdminID = currentUser.AdminID ?? 0;
                CustID = currentUser.CustID ?? 0;

                if (AdminID == 0 && CustID == 0)
                {
                    isUserValid = false;
                }
                else
                {

                    if (AdminID > 0)
                    {
                        // validate Admin status
                        C_AdminUser c_AdminUser = db.C_AdminUser.Find(AdminID);
                        if (c_AdminUser == null)
                        {
                            AuthenticationManager.SignOut();
                            TempData["errorMessage"] = "Invalid Login attempt";
                            return RedirectToAction("Login", "Account");
                        }
                        else
                        {
                            intStatusID = c_AdminUser.StatusID ?? 0;
                        }
                    }
                    else
                    {
                        // validate Admin status
                        C_Info c_Info = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetCustByCustID] @CustID",
                        new SqlParameter("@CustID", CustID)
                        ).FirstOrDefault();
                        if (c_Info == null)
                        {
                            AuthenticationManager.SignOut();
                            TempData["errorMessage"] = "Invalid Login attempt";
                            return RedirectToAction("Login", "Account");
                        }
                        else
                        {
                            intStatusID = c_Info.StatusID;  // c_Info.StatusiID is not nullable so no chance of null
                            status = c_Info.Status;  // c_Info.StatusiID is not nullable so no chance of null
                            customerType = c_Info.CustomerType ;  // c_Info.StatusiID is not nullable so no chance of null
                            flag4 = c_Info.Flag4;  // this is set to 1 during enrollment after a payment method is entered. Once a member is active, this doesn't matter
                            if (flag4 == null)
                            {
                                flag4 = 0;
                            }
                        }
                    }

                    if (intStatusID >= 3)
                    {
                        isUserValid = false;
                    }
                    if (status == "New" && customerType == 6 && intStatusID == 1 && flag4 == 1)
                    {
//                        return RedirectToAction("EnrollmentRedirect", "Enrollment", new { id = 33, custID = CustID });
                        AuthenticationManager.SignOut();
                        TempData["errorMessage"] = "Your account information is not complete. Please contact Member Services.";
                        return RedirectToAction("Login", "Account");
                    }    
                }
            }

            TempData["UserStatusID"] = intStatusID;

            if (isUserValid)
            {
                UserManager.ResetAccessFailedCountAsync(currentUserId);

                if (AdminID > 0 || customerType == 3)
                {
                    return RedirectToAction("Index","Member");
                }
                else 
                {
                    if (customerType == 4)
                    {
                        return RedirectToAction("Index", "SiteIMD");
                    }
                    else
                    {
                        return RedirectToAction("Index", "SiteMember");
                    }
                }
            }
            else
            {
                AuthenticationManager.SignOut();
                TempData["errorMessage"] = "Invalid Login attempt";
                return RedirectToAction("Login", "Account");
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.errorMessage = TempData["errorMessage"] ?? "";

            TempData["errorMessage"] = "";

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {

            if (!ModelState.IsValid)
            {
                return View(model);
            }


            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: true);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("VerifyStatus", "Account", new { returnUrl = returnUrl });
                case SignInStatus.LockedOut:
                    ModelState.AddModelError("", "Invalid login attempt.  Account is temporarily locked out.  Please use Forgot Password link to update your password.");
                    return View(model);
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, DisplayName = model.DisplayName };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
             return PartialView(model);
            // return RedirectToAction("Register", "Account");
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                int customerType = 0;
                var user = await UserManager.FindByNameAsync(model.Email);
//                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                 string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                 int? CustID = user.CustID;

                 if (CustID != null && CustID != 0)
                 {
                     C_Info c_Info = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetCustByCustID] @CustID",
                     new SqlParameter("@CustID", CustID)
                     ).FirstOrDefault();

                     if (c_Info == null)
                     {
                         TempData["errorMessage"] = "Unable to request password reset";
                         return RedirectToAction("Login", "Account");
                     }
                     else
                     {
                         customerType = c_Info.CustomerType;
                     }
                 }
                 var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code, customerType = customerType }, protocol: Request.Url.Scheme);		
                // the usermanager sendemailasync was resulting in text email not html  changed to use different email methodology
                //                 await UserManager.SendEmailAsync(user.Id, "Reset Password", "<h1>Password Reset</h1>Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                 string emailSubject = "AppTrack Reset Password";
                 string bodyCopy = "<br/><br/>We received your request to reset your password.<br/><br/>Please click <a href=\"" + callbackUrl + "\">here</a> to reset your password.<br/><br/>";
                 string emailBody = Constants.emailTemplate;
                 emailBody = emailBody.Replace("#SITEURL#",Constants.siteURL);
                 emailBody = emailBody.Replace("#BODYCOPY#",bodyCopy);
                 SmtpClient client = new SmtpClient();
                 MailMessage mailMessage = new MailMessage();
                 MailAddress FromAddress = new MailAddress(Constants.adminEmailFrom, "AppTrack Member Services");
                 mailMessage.From = FromAddress;
                 mailMessage.To.Add(model.Email);
                 mailMessage.Subject = emailSubject;
                 mailMessage.Body = emailBody;
                 mailMessage.IsBodyHtml = true;
                 try
                 {
                     client.Send(mailMessage);
                 }
                 catch
                 {
                     ViewBag.ErrorMessage = "An unexpected error was encountered. Please try again.";
                 }
                 return View("ForgotPasswordConfirmation");
//                 return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        

        public async Task<ActionResult> RequestResetPassword()
        {
            int customerType = 0;

            if (AdminID > 0)
            {
                if (CustID != null)
                {
                    C_Info c_Info = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetCustByCustID] @CustID",
                    new SqlParameter("@CustID", CustID)
                    ).FirstOrDefault();
                    if (c_Info == null)
                    {
                        TempData["errorMessage"] = "Unable to request password reset for Member";
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        string memberEmail = c_Info.Email;
                        var user = await UserManager.FindByNameAsync(memberEmail);
                        if (user != null)
                        {
                            DisplayName = user.DisplayName ?? "";
                            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);

                            return RedirectToAction("ResetPassword", new { code = code, customerType = c_Info.CustomerType });
                        }
                    }
                }
                else
                {
                    TempData["errorMessage"] = "Unable to request password reset for Member";
                    return RedirectToAction("Login", "Account");
                }
            }
            else
            {
                var currentUserId = User.Identity.GetUserId();
                if (currentUserId != null)
                {
                    //Get the User object
                    var currentUser = UserManager.FindById(currentUserId);
                    //The currentUser object has properties that match the columns in the AspNetUsers table
                    // Note when assigning nullable column to int variable the ?? say set it to 0 if it is null
                    if (currentUser != null)
                    {
                        DisplayName = currentUser.DisplayName ?? "";
                        int? CustID = currentUser.CustID;
 
                        if (CustID != null)
                        {
                            C_Info c_Info = db.Database.SqlQuery<C_Info>("exec dbo.[LB_GetCustByCustID] @CustID",
                            new SqlParameter("@CustID", CustID)
                            ).FirstOrDefault();

                            if (c_Info == null)
                            {
                                TempData["errorMessage"] = "Unable to request password reset for Member";
                                return RedirectToAction("Login", "Account");
                            }
                            else
                            {
                                customerType = c_Info.CustomerType;
                            }
                        }
                        
                        string code = await UserManager.GeneratePasswordResetTokenAsync(currentUser.Id);

                        return RedirectToAction("ResetPassword", new { code = code, customerType = customerType });
                    }
                }
            }
            return RedirectToAction("Login");
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code, int customerType)
        {
            ViewBag.CustomerType = customerType;
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                ViewBag.errorMessage = "We were unable to verify your email address. Please try again.";
                return View();
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                user.LockoutEndDateUtc = DateTime.UtcNow.AddMinutes(-1);
                await UserManager.UpdateAsync(user);

                return RedirectToAction("ResetPasswordConfirmation", new { customerType = model.CustomerType });
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation(int customerType)
        {
            ViewBag.CustomerType = customerType;
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            string RedirectController = "Account";
            string RedirectAction = "Login";

            if (User.IsInRole("Member") || User.IsInRole("IMD"))
            {
                RedirectController = "Home";
                RedirectAction = "Index";            
            }

            AuthenticationManager.SignOut();
            if (Request.Cookies["CustID"] != null)
            {
                var c = new HttpCookie("CustID");
                c.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(c);
            }
            //Response.Redirect(Constants.siteURL);

            return RedirectToAction(RedirectAction, RedirectController);
        }

        // GET: /Account/LogOff
        [HttpGet]
        public ActionResult LogOut()
        {
            string RedirectController = "Account";
            string RedirectAction = "Login";

            if (User.IsInRole("Member") || User.IsInRole("IMD"))
            {
                RedirectController = "Home";
                RedirectAction = "Index";
            }

            AuthenticationManager.SignOut();
            return RedirectToAction(RedirectAction, RedirectController);
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}