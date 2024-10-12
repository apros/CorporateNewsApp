namespace CorporateNews.Core.Models
{
    public class QuarterlySales
    {
        public DateTime Date { get; set; }
        public int Quarter { get; set; }
        public decimal PreviousSales { get; set; }
        public decimal CurrentSales { get; set; }
    }
}
