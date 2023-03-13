﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiRessource2;
using ApiRessource2.Models;
using ApiRessource2.Models.Wrapper;
using ApiRessource2.Models.Filter;
using ApiRessource2.Helpers;
using ApiRessource2.Services;
using NuGet.Packaging;

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
        public async Task<IActionResult> GetResources([FromQuery] PaginationFilter filter, TriType triType)
        {
            var resource = new List<Resource>();
            try
            {
                var route = Request.Path.Value;
                var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
                var query = _context.Resources
                   .Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                   .Take(validFilter.PageSize)
                   .Include(r => r.User)
                   .AsQueryable();

                if(triType == TriType.Alphabetique)
                {
                    resource = await query.OrderBy(q => q.Title).ToListAsync();
                }
                if (triType == TriType.Popularité)
                {
                    resource = await query.OrderByDescending(q => q.UpVote).ToListAsync();
                }
                if (triType == TriType.DateAsc)
                {
                    resource = await query.OrderBy(q => q.CreationDate.Date).ThenBy(q=>q.CreationDate.TimeOfDay).ToListAsync();
                }
                if (triType == TriType.DateDesc)
                {
                    resource = await query.OrderByDescending(q => q.CreationDate).ThenBy(q => q.CreationDate.TimeOfDay).ToListAsync();
                }


                var totalRecords = await _context.Resources.CountAsync();
                var pagedReponse = PaginationHelper.CreatePagedReponse<Resource>(resource, validFilter, totalRecords, uriService, route);
                return Ok(pagedReponse);
            }
            catch(Exception ex)
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
                var resource = await _context.Resources.Where(r=>r.Id == id).Include(r=>r.Comments).FirstOrDefaultAsync();
                if (resource == null)
                {
                    string[] errorList = new string[] { $"La ressource {id} n'a pas été trouvée ou n'existe pas." };
                    Response<Resource> responseNotFound = new Response<Resource>(null, $"La ressource {id} n'a pas été trouvée ou n'existe pas.");
                    return NotFound(responseNotFound);
                }

                Response<Resource> response = new Response<Resource>(resource, $"La ressource {id} a été trouvée.");
                return Ok(response);

            }
            catch(Exception ex)
            {
                Response<Resource> response = new Response<Resource>(null, ex.Message);
                return BadRequest(response);
            }
            
        }


        // GET: api/Resources/search/
        [HttpGet("search")]
        public async Task<IActionResult> GetFiltredResource(string search, [FromQuery] PaginationFilter filter)
        {
            var route = Request.Path.Value;
            var validFilter = new PaginationFilter(filter.PageNumber, filter.PageSize);
            var query = _context.Resources
                .Where(r => r.IsDeleted == false)
                .Where(r => r.Title.ToLower().Contains(search.ToLower()))
                .AsQueryable();

            var resource = await query.Skip((validFilter.PageNumber - 1) * validFilter.PageSize)
                .Take(validFilter.PageSize)
                .Include(r => r.User)
                .ToListAsync();

            if (resource == null)
            {
                return NotFound();
            }

            var totalRecords = await _context.Resources.CountAsync();
            var pagedReponse = PaginationHelper.CreatePagedReponse<Resource>(resource, validFilter, totalRecords, uriService, route);
            return Ok(pagedReponse);
        }


        // PUT: api/Resources/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutResource(int id, PostResource postresource)
        {
            User user = (User)HttpContext.Items["User"];
            var resource = await _context.Resources.FindAsync(id);

            if(user.Id != resource.UserId)
            {
                return BadRequest("Impossible de modifier cette ressource.");
            }

            if (resource == null)
            {
                return NotFound("Le commentaire n'a pas été trouvé.");
            }

            if(postresource == null)
            {
                return BadRequest("Il manque des champs dans le formulaire.");
            }

            // Mettre à jour les propriétés du commentaire existant avec les nouvelles valeurs
            resource.Title = postresource.Title;
            resource.Description = postresource.Description;
            resource.Path = postresource.Path;
            resource.Type = postresource.Type;

            try
            {
                _context.Update(resource);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }


        }


        // POST: api/Resources
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Resource>> PostResource(PostResource postresource)
        {
            User user = (User)HttpContext.Items["User"];
            Resource resource = new Resource()
            {
                Title = postresource.Title,
                Description = postresource.Description,
                CreationDate = DateTime.Now,
                Path = postresource.Path,
                IsDeleted = false,
                UpVote = 0,
                DownVote = 0,
                Type = postresource.Type,
                UserId = user.Id
            };
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetResource", new { id = resource.Id }, resource);
        }

        // DELETE: api/Resources/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResource(int id)
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource == null)
            {
                return NotFound();
            }

            _context.Resources.Update(resource);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // PUT: api/Resources/upvote/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("upvote")]
        public async Task<IActionResult> upvote(int idresource)
        {
            User user = (User)HttpContext.Items["User"];
            if (idresource == null || idresource == 0 || user.Id == null || user.Id == 0)
            {
                return BadRequest("La ressource est introuvable.");
            }


            var voted = await _context.Voteds.Where(v => v.UserId == user.Id).Where(v => v.RessourceId == idresource).FirstOrDefaultAsync();
            if (voted == null)
            {
                var resourceToUpdate = await _context.Resources.FindAsync(idresource);

                if (resourceToUpdate == null)
                {
                    return NotFound("La ressource est introuvable.");
                }

                resourceToUpdate.UpVote++;
                voted = new Voted() { };
                voted.RessourceId = idresource;
                voted.UserId = user.Id;
                _context.Voteds.Add(voted);
                _context.Update(resourceToUpdate);



                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }
            else
            {
                return BadRequest("Vous avez deja donner votre avis sur cette ressource.");
            }



            return NoContent();
        }



        // PUT: api/Resources/upvote/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize]
        [HttpPut("downvote")]
        public async Task<IActionResult> downvote(int idresource)
        {
            User user = (User)HttpContext.Items["User"];
            if (idresource == null || idresource == 0 || user.Id == null || user.Id == 0)
            {
                return BadRequest("La ressource est introuvable.");
            }
            
            
            var voted = await _context.Voteds.Where(v => v.UserId == user.Id).Where(v => v.RessourceId == idresource).FirstOrDefaultAsync();
            if (voted == null)
            {
                var resourceToUpdate = await _context.Resources.FindAsync(idresource);

                if (resourceToUpdate == null)
                {
                    return NotFound("La ressource est introuvable.");
                }

                resourceToUpdate.DownVote++;
                voted = new Voted() { };
                voted.RessourceId = idresource;
                voted.UserId = user.Id;
                _context.Voteds.Add(voted);
                _context.Update(resourceToUpdate);



                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {

                }
            }
            else
            {
                return BadRequest("Vous avez deja donner votre avis sur cette ressource.");
            }



            return NoContent();
        }


        private bool ResourceExists(int id)
        {
            return _context.Resources.Any(e => e.Id == id);
        }
    }
}
