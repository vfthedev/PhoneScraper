namespace PhoneScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            var scraper = new Scraper("https://www.gsmarena.com/");
            
            // Collect the list of page urls that need to be parsed
            scraper.GetPhoneManufacturers();

            scraper.GetPhoneDetails();
        }
    }
}