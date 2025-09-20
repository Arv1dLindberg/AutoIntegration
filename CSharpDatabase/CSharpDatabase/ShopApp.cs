using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpDatabase
{
    internal class ShopApp
    {
        private readonly ShopDbContext dbContext;

        public ShopApp(ShopDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        internal void Init()
        {
            if( dbContext.Products.Count() == 0)
            {
                dbContext.Products.AddRange(
                    new Product { Name = "Produkt 1", Description = "Beskrivning 1", Price = 100.00m },
                    new Product { Name = "Produkt 2", Description = "Beskrivning 2", Price = 200.00m },
                    new Product { Name = "Produkt 3", Description = "Beskrivning 3", Price = 300.00m }
                );
                dbContext.SaveChanges();
            }
        }

        internal void RunMenu()
        {
            Console.WriteLine("Välkommen till ShopApp!");
            foreach (var product in dbContext.Products)
            {
                Console.WriteLine($"Id: {product.Id}, Name: {product.Name}, Description: {product.Description}, Price: {product.Price:C}");
            }
        }
    }
}
