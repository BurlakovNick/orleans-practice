using System;
using System.Linq;
using FlightsBookingClient.Dto;

namespace FlightsBooking.Model
{
    public class FlightFactory : IFlightFactory
    {
        public Flight Create()
        {
            var random = new Random();
            return new Flight
            {
                IsActivated = true,
                Number = new string(Enumerable
                    .Range(0, 5)
                    .Select(i => (char) ((int) '0' + random.Next(0, 10)))
                    .ToArray()),
                Date = DateTime.UtcNow,
                Seats = Enumerable
                    .Range(1, 10)
                    .SelectMany(row => Enumerable
                        .Range(0, 6)
                        .Select(seat => new Seat
                        {
                            Row = row,
                            Number = ((char) ('A' + seat)).ToString(),
                            State = SeatState.Free,
                            HeldUntil = null,
                            HeldByUserId = null
                        }))
                    .ToArray()
            };
        }
    }
}