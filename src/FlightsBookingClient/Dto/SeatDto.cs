﻿using System;
using Newtonsoft.Json;

namespace FlightsBookingClient.Dto
{
    public class SeatDto
    {
        [JsonProperty("id")]
        public string Id => Row + Number;

        [JsonProperty("row")]
        public int Row { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("state")]
        public SeatState State { get; set; }

        [JsonProperty("held-until")]
        public DateTime? HeldUntil { get; set; }

        [JsonProperty("held-by-user-id")]
        public string HeldByUserId { get; set; }
    }
}