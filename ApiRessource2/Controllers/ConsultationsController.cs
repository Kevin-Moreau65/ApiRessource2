using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiRessource2.Models;
using System.Security.Claims;
using ApiRessource2.Helpers;
using System.Linq;

namespace ApiRessource2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationsController : ControllerBase
    {
        private readonly DataContext _context;

        public ConsultationsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Consultations
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Consultation>>> getAllconsultationsbyid(int id)
        {
            return _context.Consultations.Where(c => c.Id == id).ToList();

        }
        [HttpGet("getallconsultationsbyiduser")]
        [Authorize]
        public async Task<ActionResult<List<Consultation>>>GetConsultationById(int id)
        {
            User user = (User)HttpContext.Items["User"];
            var userId = user.Id;

            var consultation = await _context.Consultations.Where(c => c.UserId == userId).Include(c => c.Resource).ToArrayAsync();
            if (consultation == null)
                return NotFound("L'utilisateur n'a pas été trouvé.");

            if (consultation == null)
                return NotFound("Le favoris que vous essayez de mettre à jour a été supprimé");

            return Ok(consultation);
            ;
        }

        // PUT: api/Consultations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutConsultation(int id)
        {
            User user = (User)HttpContext.Items["User"];
            var userId = user.Id;
            if (userId == null)
                return NotFound("L'utilisateur n'a pas été trouvé.");

            Consultation consultation = _context.Consultations.Where(c => c.Id == id && c.UserId == userId).FirstOrDefault();
            if (consultation == null)
                return NotFound("La consultation que vous essayez de mettre à jour n'a pas été trouvée.");

            var authorizationResult = await VerifyAuthorization(consultation);
            if (authorizationResult != null)
                return authorizationResult;

            consultation.Date = DateTime.Now;
            consultation.UserId = userId;


            _context.Entry(consultation).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(consultation);
        }

        // POST: api/Consultations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Consultation>> PostConsultation(Consultation consultation, int RessourceId)
        {
            User user = (User)HttpContext.Items["User"];
            var userId = user.Id;
            if (userId == null)
                return NotFound("L'utilisateur n'a pas été trouvé.");

            consultation.ResourceId = RessourceId;
            consultation.Date = DateTime.Now;
            consultation.UserId = userId;
            _context.Consultations.Add(consultation);
            await _context.SaveChangesAsync();

            return Ok(consultation);
        }

        // DELETE: api/Consultations/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteConsultation(Consultation consultation)
        {
            var authorizationResult = await VerifyAuthorization(consultation);
            if (authorizationResult != null)
                return authorizationResult;

            _context.Consultations.Remove(consultation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<IActionResult> VerifyAuthorization(Consultation consultation)
        {
            User user = (User)HttpContext.Items["User"];
            var userId = user.Id;
            if (userId == null)
                return NotFound("L'utilisateur n'a pas été trouvé.");

            if (consultation == null)
                return NotFound("La consultation que vous essayez de Supprimer a deja été supprimé");

            var isModerator = user != null && (user.Role == Role.Administrator || user.Role == Role.Moderator || user.Role == Role.SuperAdministrator);
            var isOwner = _context.Consultations.Any(c => c.Id == consultation.Id && c.UserId == (userId));

            if (!isModerator && !isOwner)
                return Unauthorized("Vous n'êtes pas autorisé à supprimer cette consultation.");

            return NoContent();
        }
    }
}
