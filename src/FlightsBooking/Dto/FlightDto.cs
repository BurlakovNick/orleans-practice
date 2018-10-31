using System;
using Newtonsoft.Json;

namespace FlightsBooking.Dto
{
    public class FlightDto
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("seats")]
        public SeatDto[] Seats { get; set; }
    }
}