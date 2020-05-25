using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using smartHookah.Models;
using smartHookah.Models.Db;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SecurityToken = System.IdentityModel.Tokens.SecurityToken;

namespace smartHookah.Controllers
{
    using Microsoft.Owin;
    using Newtonsoft.Json.Linq;
    using smartHookah.Services.Person;
    using smartHookah.Services.Redis;
    using System.Configuration;
    using System.Data.Entity.Migrations;
    using System.Net.Http;

    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;

        private ApplicationUserManager _userManager;

        private readonly IOwinContext owinContext;

        private readonly IPersonService personService;

        private readonly IRedisService redisService;

        private readonly IAccountService accountService;

        private readonly SmartHookahContext db;

        private static string fbAppToken = "1107199546054049|dlrAZ0Z5cltOcdfPnc7r7MYTRds";

        private TokenValidationParameters validationParameters;

        public AccountController(IOwinContext owinContext, IPersonService personService, IAccountService accountService,
            IRedisService redisService, SmartHookahContext db)
        {
            this.owinContext = owinContext;
            this.personService = personService;
            this.accountService = accountService;
            this.redisService = redisService;
            this.db = db;
        }

        public AccountController(
            ApplicationUserManager userManager,
            ApplicationSignInManager signInManager,
            IOwinContext owinContext,
            IPersonService personService,
            IAccountService accountService)
        {
            this.UserManager = userManager;
            this.SignInManager = signInManager;
            this.owinContext = owinContext;
            this.personService = personService;
            this.accountService = accountService;
            
        }

        private async Task LoadAuth0()
        {
            var client = new HttpClient();
            var url = $"{ConfigurationManager.AppSettings["Auth0Domain"]}.well-known/openid-configuration";
           
            CancellationToken cancellationToken = new CancellationToken();
            var data = await OpenIdConnectConfigurationRetriever.GetAsync(url, cancellationToken);
            validationParameters =
                new TokenValidationParameters
                {
                    ValidIssuer = ConfigurationManager.AppSettings["Auth0Domain"],
                    ValidAudiences = new[] { ConfigurationManager.AppSettings["Auth0ApiIdentifier"] },
                    IssuerSigningKeys = data.SigningKeys
                };
        }

        public ApplicationSignInManager SignInManager
        {
            get { return this._signInManager ?? this.HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }

            private set { this._signInManager = value; }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return this._userManager ?? this.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }

            private set { this._userManager = value; }
        }

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            // So that the user can be referred back to where they were when they click logon
            if (string.IsNullOrEmpty(returnUrl) && this.Request.UrlReferrer != null)
                returnUrl = this.Server.UrlEncode(this.Request.UrlReferrer.PathAndQuery);

            if (this.Url.IsLocalUrl(returnUrl) && !string.IsNullOrEmpty(returnUrl))
            {
                this.ViewBag.ReturnURL = returnUrl;
            }

            return this.View();

            // ViewBag.ReturnUrl = returnUrl;
            // return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await this.SignInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return this.RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return this.View("Lockout");
                case SignInStatus.RequiresVerification:
                    return this.RedirectToAction(
                        "SendCode",
                        new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return this.View(model);
            }
        }

        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await this.SignInManager.HasBeenVerifiedAsync())
            {
                return this.View("Error");
            }

            return this.View(
                new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await this.SignInManager.TwoFactorSignInAsync(
                model.Provider,
                model.Code,
                isPersistent: model.RememberMe,
                rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return this.RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return this.View("Lockout");
                case SignInStatus.Failure:
                default:
                    this.ModelState.AddModelError(string.Empty, "Invalid code.");
                    return this.View(model);
            }
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return this.View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    DisplayName = model.DisplayName
                };
                var result = await this.UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    var person = new Person();
                    person.GameProfile = new GameProfile();
                    user.Person = person;

                    await this.UserManager.UpdateAsync(user);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    string code = await this.UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    var callbackUrl = this.Url.Action(
                        "ConfirmEmail",
                        "Account",
                        new { userId = user.Id, code = code },
                        protocol: this.Request.Url.Scheme);
                    await this.UserManager.SendEmailAsync(
                        user.Id,
                        "Confirm your account",
                        "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return this.RedirectToAction("Index", "Home");
                }

                this.AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RegeneratePersons()
        {
            var badUsers = this.db.Users.Where(a => a.PersonId == null);

            foreach (var user in badUsers)
            {
                var person = new Person();
                person.GameProfile = new GameProfile();
                user.Person = person;
                this.db.Users.AddOrUpdate(user);
            }

            await this.db.SaveChangesAsync();
            return null;
        }

        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return this.View("Error");
            }

            var result = await this.UserManager.ConfirmEmailAsync(userId, code);
            return this.View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return this.View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this.UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await this.UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return this.View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await this.UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = this.Url.Action(
                    "ResetPassword",
                    "Account",
                    new { userId = user.Id, code = code },
                    protocol: this.Request.Url.Scheme);
                await this.UserManager.SendEmailAsync(
                    user.Id,
                    "Reset Password",
                    "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return this.RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return this.View();
        }

        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? this.View("Error") : this.View();
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this.UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return this.RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var result = await this.UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return this.RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            this.AddErrors(result);
            return this.View();
        }

        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return this.View();
        }

        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            this.Session["Workaround"] = 0;
            this.ControllerContext.HttpContext.Session.RemoveAll();

            // Request a redirect to the external login provider
            return new ChallengeResult(
                provider,
                this.Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await this.SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return this.View("Error");
            }

            var userFactors = await this.UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose })
                .ToList();
            return this.View(
                new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View();
            }

            // Generate the token and send it
            if (!await this.SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return this.View("Error");
            }

            return this.RedirectToAction(
                "VerifyCode",
                new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await this.AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return this.RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await this.SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return this.RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return this.View("Lockout");
                case SignInStatus.RequiresVerification:
                    return this.RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    this.ViewBag.ReturnUrl = returnUrl;
                    this.ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return this.View(
                        "ExternalLoginConfirmation",
                        new ExternalLoginConfirmationViewModel
                        {
                            Email = loginInfo.Email,
                            DisplayName = loginInfo.DefaultUserName
                        });
            }
        }

        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(
            ExternalLoginConfirmationViewModel model,
            string returnUrl)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return this.RedirectToAction("Index", "Manage");
            }

            if (this.ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await this.AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return this.View("ExternalLoginFailure");
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    DisplayName = model.DisplayName
                };
                var result = await this.UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await this.UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        var matchUser = await UserManager.FindByEmailAsync(model.Email);
                        user.Person = matchUser != null ? matchUser.Person : Person.CreateDefault();

                        await this.UserManager.UpdateAsync(user);
                        await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return this.RedirectToLocal(returnUrl);
                    }
                }

                this.AddErrors(result);
            }

            this.ViewBag.ReturnUrl = returnUrl;
            return this.View(model);
        }

        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            this.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return this.RedirectToAction("Index", "Home");
        }

        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return this.View();
        }

        private async Task<RegistrationInfo> GetUserInfo(string provider, string token)
        {
            RegistrationInfo result = new RegistrationInfo();

            if (provider == "Facebook")
            {
                var fbEndpoint = string.Format(
                    "https://graph.facebook.com/me?fields=name,email&access_token={0}",
                    token
                );

                var client = new HttpClient();
                var uri = new Uri(fbEndpoint);
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                dynamic jObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                var name = jObj["name"];
                var email = jObj["email"];
                result.DisplayName = name;
                result.Email = email;
                return result;
            }

            return null;
        }

        private async Task<ParsedExternalAccessToken> VerifyExternalAccessToken(string provider, string accessToken)
        {
            ParsedExternalAccessToken parsedToken = null;

            var verifyTokenEndPoint = string.Empty;

            if (provider == "Auth0")
            {
                if(validationParameters == null)
                {
                    await this.LoadAuth0();
                }
             
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                var user = handler.ValidateToken(accessToken, this.validationParameters, out var validatedToken);
            }

            if (provider == "Facebook")
            {
                // You can get it from here: https://developers.facebook.com/tools/accesstoken/
                // More about debug_tokn here: http://stackoverflow.com/questions/16641083/how-does-one-get-the-app-access-token-for-debug-token-inspection-on-facebook

                verifyTokenEndPoint = string.Format(
                    "https://graph.facebook.com/debug_token?input_token={0}&access_token={1}",
                    accessToken,
                    fbAppToken);
            }
            else if (provider == "Google")
            {
                verifyTokenEndPoint = string.Format(
                    "https://www.googleapis.com/oauth2/v1/tokeninfo?access_token={0}",
                    accessToken);
            }
            else
            {
                return null;
            }

            var client = new HttpClient();
            var uri = new Uri(verifyTokenEndPoint);
            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                dynamic jObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                parsedToken = new ParsedExternalAccessToken();

                if (provider == "Facebook")
                {
                    parsedToken.user_id = jObj["data"]["user_id"];
                    parsedToken.app_id = jObj["data"]["app_id"];

                    if (!string.Equals(
                        ConfigurationManager.AppSettings["fbAppId"],
                        parsedToken.app_id,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
                else if (provider == "Google")
                {
                    parsedToken.user_id = jObj["user_id"];
                    parsedToken.app_id = jObj["audience"];

                    if (!string.Equals(
                        ConfigurationManager.AppSettings["googleAppId"],
                        parsedToken.app_id,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
            }

            return parsedToken;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("ObtainLocalAccessToken")]
        public async Task<JObject> ObtainLocalAccessToken(string provider, string externalAccessToken)
        {
            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(externalAccessToken))
            {
                return null;

                // return BadRequest("Provider or external access token is not sent");
            }

            if (provider == "Manapipes")
            {
                var userId = this.redisService.GetPerson(externalAccessToken);
                var manaUser = this.UserManager.FindById(userId);
                var accessTokenResponseMana =
                    this.accountService.GenerateLocalAccessTokenResponse(manaUser, this.UserManager);

                return await accessTokenResponseMana;
            }

            var verifiedAccessToken = await this.VerifyExternalAccessToken(provider, externalAccessToken);
            if (verifiedAccessToken == null)
            {
                return null;

                // return BadRequest("Invalid Provider or External Access Token");
            }

            ApplicationUser user =
                await this.UserManager.FindAsync(new UserLoginInfo(provider, verifiedAccessToken.user_id));

            bool hasRegistered = user != null;

            // register new
            if (!hasRegistered)
            {
                var userInfo = await this.GetUserInfo(provider, externalAccessToken);

                if (userInfo != null)
                {
                    var matchUser = await UserManager.FindByEmailAsync(userInfo.Email);
                    if (matchUser != null)
                    {
                        var addLoginResult = await this.UserManager.AddLoginAsync(matchUser.Id,
                            new UserLoginInfo(provider, verifiedAccessToken.user_id));
                        if (addLoginResult.Succeeded)
                        {
                            user = matchUser;
                            await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        }
                    }
                    else
                    {
                        var newUser = new ApplicationUser
                        {
                            UserName = userInfo.Email,
                            Email = userInfo.Email,
                            DisplayName = userInfo.DisplayName
                        };

                        var result = await this.UserManager.CreateAsync(newUser);
                        if (result.Succeeded)
                        {
                            var createdUser = await UserManager.FindByEmailAsync(userInfo.Email);
                            result = await this.UserManager.AddLoginAsync(matchUser.Id,
                                new UserLoginInfo(provider, verifiedAccessToken.user_id));
                            if (result.Succeeded)
                            {
                                user = createdUser;
                                user.Person = Person.CreateDefault();

                                await this.UserManager.UpdateAsync(user);
                                await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                            }
                        }
                    }
                }
            }


            // generate access token response
            var accessTokenResponse = this.accountService.GenerateLocalAccessTokenResponse(user, this.UserManager);

            return await accessTokenResponse;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._userManager != null)
                {
                    this._userManager.Dispose();
                    this._userManager = null;
                }

                if (this._signInManager != null)
                {
                    this._signInManager.Dispose();
                    this._signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get { return this.HttpContext.GetOwinContext().Authentication; }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                this.LoginProvider = provider;
                this.RedirectUri = redirectUri;
                this.UserId = userId;
            }

            public string LoginProvider { get; set; }

            public string RedirectUri { get; set; }

            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = this.RedirectUri };
                if (this.UserId != null)
                {
                    properties.Dictionary[XsrfKey] = this.UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, this.LoginProvider);
            }
        }

        #endregion
    }

    public class ParsedExternalAccessToken
    {
        public string user_id { get; set; }
        public string app_id { get; set; }
    }

    public class RegistrationInfo
    {
        public string Email { get; set; }

        public string DisplayName { get; set; }
    }
}