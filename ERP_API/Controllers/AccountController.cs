using Domin.TokenDto;
using ERP_API.App.IService;
using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.UsersEntity;
using Infrastructure.AppException;
using Infrastructure.Cache;
using Infrastructure.ORM;
using Infrastructure.PassowdHashing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SecondApi.Controllers;
using SherdProject.DTO;

namespace ERP_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : MasterController
    {
        private PasswordHashing passwordHashing;
        public AccountController(IUserService userService, IAppMemoryCache cache, DBContext context) : base(userService, cache, context)
        {
            passwordHashing = new PasswordHashing();
        }
        
    }
}
