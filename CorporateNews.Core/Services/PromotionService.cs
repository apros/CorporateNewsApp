using System;
using System.Collections.Generic;
using CorporateNews.Core.Models;

namespace CorporateNews.Core.Services
{
    public class PromotionService
    {
        public List<Promotion> GeneratePromotions()
        {
            var promotions = new List<Promotion>();
            var random = new Random();
            var startDate = DateTime.Now.AddYears(-5);

            for (int i = 0; i < 100; i++)
            {
                var daysToAdd = random.Next(0, 365 * 5);
                var promotionDate = startDate.AddDays(daysToAdd);

                promotions.Add(new Promotion
                {
                    Date = promotionDate,
                    EmployeeName = $"Employee {i + 1}",
                    NewPosition = $"Position {random.Next(1, 10)}"
                });
            }

            return promotions.OrderByDescending(p => p.Date).ToList();
        }
    }
}
