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
    public class ConsultationPerDay
    {
        public DateTime Date { get; set; }
        public int Number { get; set; }
    }

    public class StatistiqueController : ControllerBase
    {
        private readonly DataContext _context;

        public StatistiqueController(DataContext context)
        {
            _context = context;
        }

        // GET: StatistiqueController
        [HttpGet("consultation/lastmonth")]
        public async Task<ActionResult<Result<int>>> NbRessourceConsulteLastMonth()
        {
            List<int> list = new List<int>();
            DateTime lastmonth = DateTime.Now.AddDays(-29);
            List<Consultation> lstConsultation = await _context.Consultations.Where(objet => objet.Date >= lastmonth && objet.Date <= DateTime.Now).ToListAsync();
            return Ok(new Result<int>() { Data = lstConsultation.Count });
        }

        [HttpGet("consultation/lastmonthperday")]
        public async Task<ActionResult<Result<IEnumerable<ConsultationPerDay>>>> NbRessourceConsulteLastMonthPerDay()
        {
            List<int> list = new List<int>();
            DateTime lastmonth = DateTime.Now.AddDays(-29);
            List<Consultation> lstConsultation = await _context.Consultations.Where(objet => objet.Date >= lastmonth && objet.Date <= DateTime.Now).ToListAsync();
            // Obtenir la date du dernier jour (aujourd'hui)
            DateTime dernierJour = DateTime.Now.Date;

            // Obtenir la date du premier jour (il y a précisément 30 jours)
            DateTime premierJour = dernierJour.AddDays(-29);

            // Créer le tableau du nombre de dates par jour
            var tableauNombreDates = new List<ConsultationPerDay>();

            // Parcourir les jours entre le premier et le dernier jour
            DateTime jourCourant = premierJour;
            while (jourCourant <= dernierJour)
            {
                int nombreDates = lstConsultation.Count(c => c.Date.Date == jourCourant);
                tableauNombreDates.Add(new ConsultationPerDay { Date= jourCourant, Number= nombreDates});
                jourCourant = jourCourant.AddDays(1);
            }
            return Ok(new Result<IEnumerable<ConsultationPerDay>>() { Data = tableauNombreDates });
        }
    }
}
