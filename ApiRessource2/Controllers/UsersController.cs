//using ApiRessource2.Migrations;
using ApiRessource2.Helpers;
using ApiRessource2.Models;
using ApiRessource2.Models.Admin;
using ApiRessource2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiRessource2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private IUserService _userService;

        public UsersController(DataContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate(AuthenticateRequest model)
        {
            if (!Tools.IsEmailValid(model.Email))
            {
                return BadRequest(new { message = "Username or password is incorrect" });
            }
            var user = _context.Users.SingleOrDefault(x => x.Email == model.Email);
            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });
            if (user.IsDeleted)
                return BadRequest(new { message = "Votre compte est suspendu" });
            var token = _userService.Authenticate(model, user);

            if (token == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(token);
        }

        // GET: api/Users
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/Users/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(PostUser postuser)
        {
            if (!Tools.IsEmailValid(postuser.Email))
                return BadRequest("Une adresse mail valide doit etre rentré.");
            if (!Tools.IsValidPhoneNumber(postuser.PhoneNumber))
                return BadRequest("Un numéro de téléphone valide doit etre rentré et doit respecter ce format : +33XXXXXXX .");
            if (!Tools.IsValidPassword(postuser.Password))
                return BadRequest("Le mot de passe ne convient pas car il ne contient pas : 1 majuscule, 1 minuscule, 1 chiffre, 1 caractère spécial et 8 caractère minimum.");
            //TODO: vérifier si email et pseudo unique, a faire dans la base en modifiant les class peut etre ?
            User checkUser = _context.Users.Where(u => u.Email == postuser.Email).FirstOrDefault();
            if (checkUser != null)
            {
                return BadRequest(new { message = "L'adresse mail est deja utilisée" });
            }

            checkUser = _context.Users.Where(u => u.Username == postuser.Username).FirstOrDefault();
            if (checkUser != null)
            {
                return BadRequest(new { message = "Le nom d'utilisateur est deja utilisé" });
            }

            User user = new User()
            {
                FirstName = postuser.FirstName,
                LastName = postuser.LastName,
                Email = postuser.Email,
                Username = postuser.Username,
                PhoneNumber = postuser.PhoneNumber,
                Password = Tools.HashCode(postuser.Password),
                CreationDate = DateTime.UtcNow,
                IsConfirmed = false,
                IsDeleted = false,
                Role = Role.User,
                ZoneGeoId = 10
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [Authorize(Role.Administrator, Role.SuperAdministrator)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            user.IsDeleted = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Role.Administrator, Role.SuperAdministrator)]
        [HttpPut("unban/{id}")]
        public async Task<IActionResult> UnbanUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            User usermod = (User)HttpContext.Items["User"];
                if (usermod == null)
                {
                    return NotFound();
                }
                if (user == null)
                {
                    return NotFound();
                }
            user.IsDeleted = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Role.SuperAdministrator)]
        [HttpPost("admin/create")]
        public async Task<IActionResult> AdminCreate(CreateUser createUser)
        {
            if (!Tools.IsEmailValid(createUser.Email))
                return BadRequest("Une adresse mail valide doit etre rentré.");
            if (!Tools.IsValidPhoneNumber(createUser.PhoneNumber))
                return BadRequest("Un numéro de téléphone valide doit etre rentré et doit respecter ce format : +33XXXXXXX .");
            if (!Tools.IsValidPassword(createUser.Password))
                return BadRequest("Le mot de passe ne convient pas car il ne contient pas : 1 majuscule, 1 minuscule, 1 chiffre, 1 caractère spécial et 8 caractère minimum.");
            //TODO: vérifier si email et pseudo unique, a faire dans la base en modifiant les class peut etre ?
            User checkUser = _context.Users.Where(u => u.Email == createUser.Email).FirstOrDefault();
            if (checkUser != null)
            {
                return BadRequest(new { message = "L'adresse mail est deja utilisée" });
            }

            checkUser = _context.Users.Where(u => u.Username == createUser.Username).FirstOrDefault();
            if (checkUser != null)
            {
                return BadRequest(new { message = "Le nom d'utilisateur est deja utilisé" });
            }

            User user = new User()
            {
                FirstName = createUser.FirstName,
                LastName = createUser.LastName,
                Email = createUser.Email,
                Username = createUser.Username,
                PhoneNumber = createUser.PhoneNumber,
                Password = Tools.HashCode(createUser.Password),
                CreationDate = DateTime.UtcNow,
                IsConfirmed = false,
                IsDeleted = false,
                Role = createUser.Role,
                ZoneGeoId = 10
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}