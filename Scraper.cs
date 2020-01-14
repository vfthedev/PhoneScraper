using System;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;

namespace PhoneScraper
{
    public class Scraper
    {
        private string baseUrl;
        
        public Scraper(string url)
        {
            using (var db = new DataContext())
            {
                // check db migration
                db.Database.EnsureCreated();
                
                // remove old entries
                db.Database.ExecuteSqlRaw("DELETE FROM Manufacturers");
                db.Database.ExecuteSqlRaw("DELETE FROM Phones");
            }

            baseUrl = url;
        }
        
        public void GetPhoneManufacturers()
        {
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(baseUrl);

            var list = htmlDoc.DocumentNode.SelectNodes("//div[@class='brandmenu-v2 light l-box clearfix']/ul/li/a");

            // Store the data
            using (var db = new DataContext())
            {
                foreach (var node in list.Elements())
                {
                    string nodeUrl = node.ParentNode.GetAttributeValue("href", "URL missing");
                    string nodeName = node.InnerHtml;
                
                    db.Add(new Manufacturer
                    {
                        Name = nodeName,
                        Url = baseUrl + nodeUrl
                    });
                    
                    Console.WriteLine("Retrieving the device list for " + nodeName);
                    GetPhoneList(nodeName, nodeUrl);
                }

                db.SaveChanges();
            }
        }

        private void GetPhoneList(string manufacturerName, string manufacturerUrl)
        {
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(baseUrl + manufacturerUrl);

            var nodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='makers']/ul/li");

            using (var db = new DataContext())
            {
                foreach (var node in nodes.Elements())
                {
                    // TODO: ADD COMPANY NAME
                    string phoneName = node.ChildNodes["strong"].ChildNodes["span"].InnerHtml;
                    string phoneUrl = node.GetAttributeValue("href", "URL missing");
                    
                    db.Add(new Phone
                    {
                        Name = phoneName,
                        Url = baseUrl + phoneUrl
                    });
                }

                db.SaveChanges();
            }

            // TODO: Pagination
            // Try going to the next page if it exists
            
        }

        // Go through all phone db entries and populate the missing data
        public void GetPhoneDetails()
        {
            using (var db = new DataContext())
            {
                IQueryable<Phone> phones = db.Phones;

                foreach (Phone phone in phones)
                {
                    // Not using the async version for now because of too many requests server ban
                    ParsePhoneDetails(phone);
                }
            }
        }
        
        private void ParsePhoneDetails(Phone phone)
        {
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(phone.Url);

            using (var db = new DataContext())
            {
                // Phone name - we already have this
                // string phoneName = htmlDoc.DocumentNode.SelectSingleNode("//h1[@class='specs-phone-name-title']").InnerHtml;

                try
                {
                    phone.UsbType = htmlDoc.DocumentNode.SelectSingleNode("//td[@data-spec='usb']").InnerHtml;
                    phone.Status = htmlDoc.DocumentNode.SelectSingleNode("//td[@data-spec='status']").InnerHtml;
                    phone.Os = htmlDoc.DocumentNode.SelectSingleNode("//td[@data-spec='os']").InnerHtml;
                    phone.Price = htmlDoc.DocumentNode.SelectSingleNode("//td[@data-spec='price']").InnerHtml;
                    phone.Wlan = htmlDoc.DocumentNode.SelectSingleNode("//td[@data-spec='wlan']").InnerHtml;
                }
                catch (NullReferenceException)
                {
                    phone.UsbType = "NullReferenceException";
                }
                
                db.Phones.Update(phone);

                db.SaveChangesAsync();
            }
        }
        
        private async void ParsePhoneDetailsAsync(Phone phone)
        {
            HtmlWeb web = new HtmlWeb();
            var htmlDoc = web.Load(phone.Url);

            using (var db = new DataContext())
            {
                // Phone name - we already have this
                // string phoneName = htmlDoc.DocumentNode.SelectSingleNode("//h1[@class='specs-phone-name-title']").InnerHtml;
                
                phone.UsbType = htmlDoc.DocumentNode.SelectSingleNode("//td[@data-spec='usb']").InnerHtml;
                phone.Status = htmlDoc.DocumentNode.SelectSingleNode("//td[@data-spec='status']").InnerHtml;
                phone.Os = htmlDoc.DocumentNode.SelectSingleNode("//td[@data-spec='os']").InnerHtml;
                phone.Price = htmlDoc.DocumentNode.SelectSingleNode("//td[@data-spec='price']").InnerHtml;
                phone.Wlan = htmlDoc.DocumentNode.SelectSingleNode("//td[@data-spec='wlan']").InnerHtml;

                db.Phones.Update(phone);

                await db.SaveChangesAsync();
            }
        }
    }
}