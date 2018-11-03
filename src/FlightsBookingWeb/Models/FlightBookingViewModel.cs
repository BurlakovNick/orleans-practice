using System;

namespace FlightsBookingWeb.Models
{
    public class FlightBookingViewModel
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string Date { get; set; }
        public string Number { get; set; }
        public SeatRowViewModel[] Rows { get; set; }
    }
}