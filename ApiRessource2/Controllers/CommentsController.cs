using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiRessource2.Models;
using ApiRessource2.Helpers;
using System;

namespace ApiRessource2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly DataContext _context;

        public CommentsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Comment>> GetComment(int id)
        {
            var comment = await _context.Comments.Where(c => c.IsDeleted == false).FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return NotFound("Comment not found.");

            return comment;
        }

        [HttpGet("getallcommentsbyidressource/{ressourceId}")]
        public async Task<ActionResult<IEnumerable<Comment>>> GetAllCommentsByIdRessource(int ressourceId)
        {
            var ressourceComments = await _context.Comments
                .Where(c => !c.IsDeleted && c.ResourceId == ressourceId)
                .ToListAsync();

            if (ressourceComments.Count == 0)
            {
                return NotFound("Les ressources n'ont pas été trouvées");
            }

            return ressourceComments;
        }

        [HttpGet("getallcommentsbyiduser")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Comment>>> GetAllCommentsByIdUser()
        {
            User user = (User)HttpContext.Items["User"];
            var userId = user.Id;
            var userComments = await _context.Comments
                .Where(c => c.IsDeleted == false && c.UserId == userId)
                .ToListAsync();

            if (!userComments.Any())
                return NotFound("Les commentaires n'ont pas été trouvés");

            return userComments;
        }


        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutComment(int id, Comment comment)
        {
            User user = (User)HttpContext.Items["User"];
            var userId = user.Id;

            if (userId == 0)
                return NotFound("L'utilisateur n'a pas été trouvé.");

            var authorizationResult = await VerifyAuthorization(comment);
            if (authorizationResult != null)
                return authorizationResult;

            // Vérifier que l'id de l'entité à mettre à jour correspond à celui fourni dans l'URL
            if (id != comment.Id)
            {
                return BadRequest("L'ID fourni dans l'URL ne correspond pas à l'ID de l'entité.");
            }

            // Récupérer le commentaire correspondant à l'id dans la base de données
            var commentToUpdate = await _context.Comments.FindAsync(id);

            if (commentToUpdate == null)
            {
                return NotFound("Le commentaire n'a pas été trouvé.");
            }

            // Mettre à jour les propriétés du commentaire récupéré avec les nouvelles valeurs
            commentToUpdate.Content = comment.Content;

            _context.Entry(commentToUpdate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {

                return NotFound();

            }

            return Ok(commentToUpdate);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Comment>> PostComment(PostComment comment)
        {
            var ressourceExists = await _context.Resources.AnyAsync(r => r.Id == comment.ResourceId);
            if (!ressourceExists)
                return BadRequest("La ressource n'existe pas dans la base de données.");


            if (string.IsNullOrEmpty(comment.Content))
                return BadRequest(new { message = "Le contenu du commentaire est obligatoire." });

            User user = (User)HttpContext.Items["User"];
            Comment newComment = new Comment();

            newComment.DatePost = DateTime.Now;
            newComment.UserId = user.Id;
            newComment.Content = comment.Content;
            newComment.IsDeleted = false;
            newComment.ResourceId = comment.ResourceId;

            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            return Ok(comment);
        }

        [HttpPost("reply/{commentId}")]
        [Authorize]
        public async Task<ActionResult<Comment>> PostCommentReply(int commentId, Comment reply)
        {
            User user = (User)HttpContext.Items["User"];
            var userId = user.Id;

            var parentComment = await _context.Comments.FindAsync(commentId);
            if (parentComment == null)
                return NotFound("Le commentaire parent n'a pas été trouvé.");

            if (string.IsNullOrEmpty(reply.Content))
                return BadRequest("Le contenu de la réponse est obligatoire.");

            var ressourceExists = await _context.Resources.AnyAsync(r => r.Id == parentComment.ResourceId);
            if (!ressourceExists)
                return BadRequest("La ressource n'existe pas dans la base de données.");

            _context.Comments.Add(new Comment
            {
                DatePost = DateTime.Now,
                Content = reply.Content,
                IsDeleted = false,
                ResourceId = parentComment.ResourceId,
                UserId = userId,
                //CommentReplyId = commentId // L'ID du commentaire parent
            });

            await _context.SaveChangesAsync();

            return Ok(reply);
        }

        [HttpDelete("{id}")]
        [Authorize(Role.Moderator)]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return NotFound("Le commentaire n'a pas été trouvé.");

            comment.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<IActionResult> VerifyAuthorization(Comment comment)
        {
            User user = (User)HttpContext.Items["User"];
            var userId = user.Id;
            var isModerator = user != null && (user.Role == Role.Administrator || user.Role == Role.Moderator || user.Role == Role.SuperAdministrator);
            var isOwner = comment.UserId == userId;
            var isDeleted = !comment.IsDeleted;

            if (!isDeleted)
                return NotFound("Le commentaire que vous essayez de mettre à jour a été supprimé");

            if (!isModerator && !isOwner)
                return Unauthorized("Vous n'êtes pas autorisé à modifier ce commentaire.");

            return null;
        }
    }
}



