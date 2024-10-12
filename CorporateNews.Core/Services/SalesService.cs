using System;
using System.Collections.Generic;
using CorporateNews.Core.Models;

namespace CorporateNews.Core.Services
{
    public class SalesService
    {
        public List<QuarterlySales> GenerateQuarterlySales()
        {
            var sales = new List<QuarterlySales>();
            var startDate = DateTime.Now.AddYears(-5);
            decimal currentSales = 10000;

            for (int year = 0; year < 5; year++)
            {
                for (int quarter = 1; quarter <= 4; quarter++)
                {
                    var previousSales = currentSales;
                    currentSales *= 2;

                    sales.Add(new QuarterlySales
                    {
                        Date = startDate.AddMonths(year * 12 + (quarter - 1) * 3),
                        Quarter = quarter,
                        PreviousSales = previousSales,
                        CurrentSales = currentSales
                    });
                }
            }

            return sales;
        }
    }
}
