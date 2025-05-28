using Microsoft.AspNetCore.Mvc;
using EventOrganizer.Server.Repositories;
using EventOrganizer.Server.Tools;


namespace EventOrganizer.Server.UsersModule.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly IEmailService _emailService;

        public UsersController(IUserRepository repo, IEmailService emailService)
        {
            _repo = repo;
            _emailService = emailService;
        }
    }
}
