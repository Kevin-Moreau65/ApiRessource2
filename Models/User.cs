﻿namespace ApiRessource2.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string Pseudo { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? CreationDate { get; set; }
        public bool? IsConfirmed { get; set; }
        public bool? IsDeleted { get; set; }
        public int? IdRole { get; set; }
        public int IdZoneGeo { get; set; }
    }
}