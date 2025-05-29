namespace SystemWystawianiaOpiniiRestauracjom.Mvc.Models
{
    public class RestaurantRankingModel
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
