namespace smartHookah.Controllers.Mobile
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;
    using System.Web.Http.ModelBinding;
    using System.Web.Http.Results;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;

    using Newtonsoft.Json;

    using smartHookah.Models;
    using smartHookah.Services.Person;

    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private readonly IAccountService accountService;


        public ApplicationUserManager UserManager
        {
            get => this.userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            private set => this.userManager = value;
        }

        public ApplicationSignInManager SignInManager
        {
            get => this.signInManager ?? HttpContext.Current.GetOwinContext().Get<ApplicationSignInManager>();

            private set => this.signInManager = value;
        }

        private ApplicationUserManager userManager;

        private ApplicationSignInManager signInManager;

        private readonly AuthRepository repo;

        public AccountController(IAccountService accountService)
        {
            this.accountService = accountService;
            this.repo = new AuthRepository();
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(UserModel userModel)
        {
            if (!this.ModelState.IsValid) return this.BadRequest(this.ModelState);

            var user = new ApplicationUser
                           {
                               UserName = userModel.Email,
                               Email = userModel.Email,
                               DisplayName = userModel.UserName
                           };
            var result = await this.UserManager.CreateAsync(user,userModel.Password);

            if (result.Succeeded)
            {
                await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                var person = new Person();
                person.GameProfile = new GameProfile();
                user.Person = person;
                await this.UserManager.UpdateAsync(user);
                // Send an email with this link
                string code = await this.UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                var callbackUrl = this.Url.Link(
                    "ConfirmEmail",
                    new { controller = "Account", userId = user.Id, code = code });
                await this.UserManager.SendEmailAsync(
                    user.Id,
                    "Confirm your account",
                    "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
            }

            var errorResult = this.GetErrorResult(result);

            if (errorResult != null) return errorResult;

            var newUser = await this.UserManager.FindByEmailAsync(userModel.Email);
            var tokenResponse = this.accountService.GenerateLocalAccessTokenResponse(newUser, this.UserManager);
      
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(await tokenResponse));
            return this.ResponseMessage(response);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("ForgotPassword")]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await this.UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await this.UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    var nonConfirmEmailResponse = this.Request.CreateResponse(HttpStatusCode.OK);
                    return this.ResponseMessage(nonConfirmEmailResponse);
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await this.UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = this.Url.Link(
                    "ResetPassword",
                    new { controller = "Account", userId = user.Id, code = code });
                await this.UserManager.SendEmailAsync(
                    user.Id,
                    "Reset Password",
                    "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                var response = this.Request.CreateResponse(HttpStatusCode.OK);
                return this.ResponseMessage(response);
            }

            // If we got this far, something failed, redisplay form
            return this.BadRequest(this.ModelState);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) this.repo.Dispose();

            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null) return this.InternalServerError();

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                    foreach (var error in result.Errors) this.ModelState.AddModelError(string.Empty, this.TransformErrorCode(error));

                if (this.ModelState.IsValid) return this.BadRequest();

                return this.BadRequest(this.ModelState);
            }

            return null;
        }

        private string TransformErrorCode(string error)
        {
            if(error.StartsWith("Passwords must have at least one digit")) return "ERR_PASS_DIGIT";
            if (error.Contains("is invalid, can only contain letters or digits.")) return "ERR_NAME";
            if (error.Contains("is already taken")) return "ERR_EMAIL_REGISTERED";
            return $"ERR_UNKNOWN :{error}";

        }
    }

    public class InvalidModelStateResultCode : InvalidModelStateResult
    {
        public List<string> ErrorCodes;

        public InvalidModelStateResultCode(ModelStateDictionary modelState, bool includeErrorDetail, IContentNegotiator contentNegotiator, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            : base(modelState, includeErrorDetail, contentNegotiator, request, formatters)
        {
            handleModelState();
        }

        public InvalidModelStateResultCode(ModelStateDictionary modelState, ApiController controller)
            : base(modelState, controller)
        {
            handleModelState();
        }

        private void handleModelState()
        {
            foreach (var modelStateValue in this.ModelState.Values)
            {
               this.ErrorCodes.Add(modelStateValue.Value.AttemptedValue);
            }
        }
    }

    public class AuthRepository : IDisposable
    {
        private readonly SmartHookahContext _ctx;

        private readonly UserManager<IdentityUser> _userManager;

        public AuthRepository()
        {
            this._ctx = new SmartHookahContext();
            this._userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(this._ctx));
        }

        public void Dispose()
        {
            this._ctx.Dispose();
            this._userManager.Dispose();
        }

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            var user = await this._userManager.FindAsync(userName, password);

            return user;
        }

        public async Task<IdentityResult> RegisterUser(UserModel userModel)
        {
            var user = new IdentityUser { UserName = userModel.UserName };

            var result = await this._userManager.CreateAsync(user, userModel.Password);

            return result;
        }
    }
}