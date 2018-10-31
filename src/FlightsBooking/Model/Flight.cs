using System;
using System.Linq;
using FlightsBooking.Dto;

namespace FlightsBooking.Model
{
    public class Flight
    {
        public bool IsActivated { get; set; }
        public DateTime Date { get; set; }
        public string Number { get; set; }
        public Seat[] Seats { get; set; }

        public bool HasHeldSeats => Seats.Any(s => s.State == SeatState.TemporarilyHeld);

        public void FreeSeats()
        {
            foreach (var seat in Seats)
            {
                seat.TryFree();
            }
        }
    }
}