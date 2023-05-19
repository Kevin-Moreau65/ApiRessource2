using ApiRessource2.Helpers;
using ApiRessource2.Models;
using ApiRessource2.Models.Filter;
using ApiRessource2.Models.Wrapper;
using ApiRessource2.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace ApiRessource2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourcesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUriService uriService;

        public ResourcesController(DataContext context, IUriService uriService)
        {
            _context = context;
            this.uriService = uriService;
        }

        // GET: api/Resources
        [HttpGet]
        public async Task<IActionResult> GetResources([FromQuery] PaginationFilter filter, TriType triType, [System.Web.Http.FromUri] string? search = "")
        {
            User user = (User)HttpContext.Items["User"];
            var resource = new List<Resource>();
            try
            {
                var route = Request.Path.Value;
                var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
                IQueryable<Resource>? query;
                if (user.Role == Role.Administrator || user.Role == Role.SuperAdministrator)
                {
                    query = _context.Resources
                   .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                   .Take(validFilter.PageSize)
                   .Include(r => r.User)
                   .Include(r => r.Categorie)
                   .AsQueryable();
                } else
                {
                    query = _context.Resources.Where(r => r.IsDeleted == false)
                   .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                   .Take(validFilter.PageSize)
                   .Include(r => r.User)
                   .Include(r => r.Categorie)
                   .AsQueryable();
                }

                if (search != "" && search != null)
                {
                    query = query.Where(r => r.Title.ToLower().Contains(search.ToLower()) || r.Description.ToLower().Contains(search.ToLower())).AsQueryable();
                }

                if (triType == TriType.Alphabetique)
                {
                    resource = await query.OrderBy(q => q.Title).ToListAsync();
                }
                if (triType == TriType.Popularité)
                {
                    resource = await query.OrderByDescending(q => q.UpVote).ToListAsync();
                }
                if (triType == TriType.DateAsc)
                {
                    resource = await query.OrderBy(q => q.CreationDate.Date).ThenBy(q => q.CreationDate.TimeOfDay).ToListAsync();
                }
                if (triType == TriType.DateDesc)
                {
                    resource = await query.OrderByDescending(q => q.CreationDate).ThenBy(q => q.CreationDate.TimeOfDay).ToListAsync();
                }
                foreach (var item in resource)
                {
                    item.User = new User() { Id = item.User.Id, Role = item.User.Role, Username = item.User.Username };
                }

                var totalRecords = resource.Count();
                var pagedReponse = PaginationHelper.CreatePagedReponse<Resource>(resource, validFilter, totalRecords, uriService, route);
                return Ok(pagedReponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET: api/Resources/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetResource(int id)
        {
            try
            {
                var resource = await _context.Resources.AsNoTracking().Where(r => r.Id == id).Include(r => r.Categorie)
                    .Include(r => r.Comments.Where(c => !c.IsDeleted)).ThenInclude(c => c.User)
                    .Include(r => r.User)
                    .FirstOrDefaultAsync();
                if (resource == null)
                {
                    return NotFound(new { message = $"La ressource {id} n'a pas été trouvée ou n'existe pas." });
                }

                User usertoreturn = new User()
                {
                    Id = resource.User.Id,
                    Username = resource.User.Username,
                    Role = resource.User.Role
                };

                resource.User = usertoreturn;
                User user = (User)HttpContext.Items["User"];
                if (user != null)
                {
                    await _context.Consultations.AddAsync(new Consultation { Date = DateTime.Now, ResourceId = id, UserId = user.Id });
                    await _context.SaveChangesAsync();
                    Voted voted = await _context.Voteds
                                        .Where(v => v.ResourceId == resource.Id)
                                        .Where(v => v.UserId == user.Id)
                                        .FirstOrDefaultAsync();

                    if (voted != null)
                    {
                        resource.Voted = voted;
                    }

                    Favoris favoris = await _context.Favoris
                                        .Where(v => v.ResourceId == resource.Id)
                                        .Where(v => v.UserId == user.Id)
                                        .FirstOrDefaultAsync();
                    if (favoris != null)
                    {
                        resource.Favoris = favoris;
                    }
                }
                Response<Resource> response = new(resource, $"La ressource {id} a été trouvée.");
                return Ok(response);
            }
            catch (Exception ex)
            {
                Response<Resource> response = new(null, ex.Message);
                return BadRequest(response);
            }
        }

        // PUT: api/Resources/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutResource(int id, PostResource postresource)
        {
            User user = (User)HttpContext.Items["User"];
            var resource = await _context.Resources.FindAsync(id);

            if (user.Id != resource.UserId && user.Role != Role.Administrator && user.Role != Role.SuperAdministrator)
            {
                return BadRequest("Impossible de modifier cette ressource.");
            }

            if (resource == null)
            {
                return NotFound("La ressource n'a pas été trouvé.");
            }

            // Mettre à jour les propriétés de la ressource existante avec les nouvelles valeurs
            resource.Title = postresource.Title;
            resource.Description = postresource.Description;
            resource.Path = postresource.Path;
            resource.CategorieId = postresource.CategorieId;

            try
            {
                _context.Update(resource);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
        [Authorize(Role.Administrator, Role.SuperAdministrator)]
        [HttpPut("{id}/restore")]
        public async Task<ActionResult<Resource>> RestoreResource(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null)
            {
                return NotFound();
            }
            resource.IsDeleted = false;
            await _context.SaveChangesAsync();
            return Ok(resource);
        }
        // POST: api/Resources
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Resource>> PostResource(PostResource postresource)
        {
            User user = (User)HttpContext.Items["User"];
            Resource resource = new()
            {
                Title = postresource.Title,
                Description = postresource.Description,
                CreationDate = DateTime.Now,
                Path = postresource.Path,
                IsDeleted = false,
                UpVote = 0,
                DownVote = 0,
                CategorieId = postresource.CategorieId,
                UserId = user.Id
            };
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResource", new { id = resource.Id }, resource);
        }

        // DELETE: api/Resources/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            User user = (User)HttpContext.Items["User"];
            if (user.Id == resource.UserId || user.Role == Role.Administrator || user.Role == Role.SuperAdministrator)
            {
                if (resource == null)
                {
                    return NotFound();
                }
                resource.IsDeleted = true;
                _context.Resources.Update(resource);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            return NoContent();
        }

        // PUT: api/Resources/5/upvote
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}/upvote")]
        public async Task<IActionResult> Upvote(int id)
        {
            User user = (User)HttpContext.Items["User"];
            if (user == null)
            {
                return BadRequest(new { message = "La ressource est introuvable." });
            }
            try
            {
                var resourceToUpdate = await _context.Resources.FindAsync(id);

                if (resourceToUpdate == null)
                {
                    return BadRequest(new { message = "La ressource est introuvable." });
                }

                var currentVote = await _context.Voteds.Where(v => v.UserId == user.Id).Where(v => v.ResourceId == id).FirstOrDefaultAsync();
                if (currentVote == null || currentVote.Type == "downvote")
                {
                    if (currentVote != null && currentVote.Type == "downvote")
                    {
                        _context.Remove(currentVote);
                        resourceToUpdate.DownVote--;
                    }
                    resourceToUpdate.UpVote++;
                    var voted = new Voted
                    {
                        ResourceId = id,
                        UserId = user.Id,
                        Type = "upvote"
                    };
                    _context.Voteds.Add(voted);
                    _context.Update(resourceToUpdate);
                    await _context.SaveChangesAsync();
                }
                else if (currentVote.Type == "upvote")
                {
                    return BadRequest(new {Message = "Vous avez déjà upvoter pour la ressource" });
                }
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new System.Web.Http.HttpResponseException(HttpStatusCode.InternalServerError);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new System.Web.Http.HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        // PUT: api/Resources/5/upvote/
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}/downvote")]
        public async Task<IActionResult> Downvote(int id)
        {
            User user = (User)HttpContext.Items["User"];
            if (user == null)
            {
                return BadRequest(new { message = "La ressource est introuvable." });
            }
            try
            {
                var resourceToUpdate = await _context.Resources.FindAsync(id);

                if (resourceToUpdate == null)
                {
                    return BadRequest(new { message = "La ressource est introuvable." });
                }

                var currentVote = await _context.Voteds.Where(v => v.UserId == user.Id).Where(v => v.ResourceId == id).FirstOrDefaultAsync();
                if (currentVote == null || currentVote.Type == "upvote")
                {
                    if (currentVote != null && currentVote.Type == "upvote")
                    {
                        _context.Remove(currentVote);
                        resourceToUpdate.UpVote--;
                    }
                    resourceToUpdate.DownVote++;
                    var voted = new Voted
                    {
                        ResourceId = id,
                        UserId = user.Id,
                        Type = "downvote"
                    };
                    _context.Voteds.Add(voted);
                    _context.Update(resourceToUpdate);
                    await _context.SaveChangesAsync();
                }
                else if(currentVote.Type == "downvote")
                {
                    return BadRequest(new { Message = "Vous avez déjà downvoter pour la ressource" });
                }
                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new System.Web.Http.HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        [HttpDelete("{id}/cancelVote")]
        [Authorize]
        public async Task<ActionResult<Favoris>> CancelVote(int id)
        {
            User user = (User)HttpContext.Items["User"];
            var userId = user.Id;
            Voted? voted = await _context.Voteds.FirstOrDefaultAsync((f) => f.UserId == userId && f.ResourceId == id);
            if (voted == null)
            {
                return BadRequest(new { Message = "Cette ressource n'est pas voté." });
            }
            var ressource = await _context.Resources.FirstOrDefaultAsync((f) => f.Id == id);
            if (voted.Type == "upvote") ressource.UpVote--;
            else ressource.DownVote--;
            _context.Voteds.Remove(voted);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}