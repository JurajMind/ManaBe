namespace smartHookah.Controllers.Mobile
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;

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
            get => this._userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();

            private set => this._userManager = value;
        }
        private ApplicationUserManager _userManager;
        private readonly AuthRepository _repo;

        public AccountController(IAccountService accountService)
        {
            this.accountService = accountService;
            this._repo = new AuthRepository();
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

            var errorResult = this.GetErrorResult(result);

            if (errorResult != null) return errorResult;

            var newUser = await this.UserManager.FindByEmailAsync(userModel.Email);
            var tokenResponse = this.accountService.GenerateLocalAccessTokenResponse(newUser, this.UserManager);
      
            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(JsonConvert.SerializeObject(await tokenResponse));
            return this.ResponseMessage(response);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) this._repo.Dispose();

            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null) return this.InternalServerError();

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                    foreach (var error in result.Errors) this.ModelState.AddModelError(string.Empty, error);

                if (this.ModelState.IsValid) return this.BadRequest();

                return this.BadRequest(this.ModelState);
            }

            return null;
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