using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PhoneScraper
{
    public class DataContext : DbContext
    {
        public DbSet<Manufacturer> Manufacturers { get; set; }
        public DbSet<Phone> Phones { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=data.db");
    }

    public class Manufacturer
    {
        public int ManufacturerId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public List<Phone> Phones { get; } = new List<Phone>();
    }

    public class Phone
    {
        public int PhoneId { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string UsbType { get; set; }
        public string Status { get; set; }
        public string Os { get; set; }
        public string Price { get; set; }
        public string Wlan { get; set; }
    }
}