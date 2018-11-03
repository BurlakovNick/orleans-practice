using System;
using System.Linq;
using FlightsBookingClient.Dto;

namespace FlightsBooking.Model
{
    public class Flight
    {
        public bool IsActivated { get; set; }
        public DateTime Date { get; set; }
        public string Number { get; set; }
        public Seat[] Seats { get; set; }

        public bool HasHeldSeats => Seats.Any(s => s.State == SeatState.TemporarilyHeld);

        public Result TryHold(string userId, string seatId, TimeSpan holdTime)
        {
            var seat = Seats.FirstOrDefault(s => s.Id == seatId);
            if (seat == null)
            {
                return Result.Fail("Seat not found");
            }

            if (!seat.CanHold(userId))
            {
                return Result.Fail("Seat is busy");
            }

            FreeSeats(userId);
            seat.Hold(userId, holdTime);
            return Result.Success();
        }

        public Result TryBuy(string userId, string seatId)
        {
            var seat = Seats.FirstOrDefault(s => s.Id == seatId);
            if (seat == null)
            {
                return Result.Fail("Seat not found");
            }

            if (!seat.CanBuy(userId))
            {
                return Result.Fail("Seat is busy");
            }

            FreeSeats(userId);
            seat.Buy(userId);
            return Result.Success();
        }

        public void FreeExpiredSeats()
        {
            foreach (var seat in Seats)
            {
                seat.TryFreeExpired();
            }
        }

        private void FreeSeats(string userId)
        {
            foreach (var seat in Seats.Where(x => x.IsHeldBy(userId)))
            {
                seat.TryFree();
            }
        }
    }
}