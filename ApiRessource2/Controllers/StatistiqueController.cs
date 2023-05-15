﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiRessource2.Models;
using System.Security.Claims;
using ApiRessource2.Helpers;
using System.Linq;

namespace ApiRessource2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatistiqueController : ControllerBase
    {
        private readonly DataContext _context;

        public StatistiqueController(DataContext context)
        {
            _context = context;
        }

        // GET: StatistiqueController
        [HttpGet("nbconsulatationlasmonth")]
        public async Task<ActionResult<int>> NbRessourceConsulteLastMonth()
        {
            List<int> list = new List<int>();
            DateTime lastmonth = DateTime.Now.AddDays(-29);
            List<Consultation> lstConsultation = await _context.Consultations.Where(objet => objet.Date >= lastmonth && objet.Date <= DateTime.Now).ToListAsync();
            return Ok(lstConsultation.Count);
        }

        [HttpGet("nbconsulatationlasmonthperday")]
        public async Task<ActionResult<IEnumerable<int>>> NbRessourceConsulteLastMonthPerDay()
        {
            List<int> list = new List<int>();
            DateTime lastmonth = DateTime.Now.AddDays(-29);
            List<Consultation> lstConsultation = await _context.Consultations.Where(objet => objet.Date >= lastmonth && objet.Date <= DateTime.Now).ToListAsync();
            // Obtenir la date du dernier jour (aujourd'hui)
            DateTime dernierJour = DateTime.Now.Date;

            // Obtenir la date du premier jour (il y a précisément 30 jours)
            DateTime premierJour = dernierJour.AddDays(-29);

            // Créer le tableau du nombre de dates par jour
            var tableauNombreDates = new List<int>();

            // Parcourir les jours entre le premier et le dernier jour
            DateTime jourCourant = premierJour;
            while (jourCourant <= dernierJour)
            {
                int nombreDates = lstConsultation.Count(c => c.Date.Date == jourCourant);
                tableauNombreDates.Add(nombreDates);
                jourCourant = jourCourant.AddDays(1);
            }
            return Ok(tableauNombreDates);
        }
    }
}
