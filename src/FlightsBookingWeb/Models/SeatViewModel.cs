namespace FlightsBookingWeb.Models
{
    public class SeatViewModel
    {
        public string Id => $"{Row}{Number}";
        public bool IsBusy { get; set; }
        public bool IsHoldByMe { get; set; }
        public bool IsBought { get; set; }
        public int Row { get; set; }
        public string Number { get; set; }
    }
}