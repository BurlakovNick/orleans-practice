using System;
using FlightsBookingClient.Dto;

namespace FlightsBooking.Model
{
    public class Seat
    {
        public string Id => Row + Number;
        public int Row { get; set; }
        public string Number { get; set; }
        public SeatState State { get; set; }
        public DateTime? HeldUntil { get; set; }
        public string HeldByUserId { get; set; }

        public bool CanHold(string userId) => IsFree() || IsHeldBy(userId);

        public void Hold(string userId, TimeSpan holdTime)
        {
            if (!CanHold(userId))
            {
                throw new InvalidOperationException("Can't hold seat!");
            }

            State = SeatState.TemporarilyHeld;
            HeldUntil = DateTime.UtcNow.Add(holdTime);
            HeldByUserId = userId;
        }

        public bool CanBuy(string userId) => IsFree() || IsHeldBy(userId);

        public void Buy(string userId)
        {
            if (!CanBuy(userId))
            {
                throw new InvalidOperationException($"Can't buy seat!");
            }

            State = SeatState.Busy;
            HeldUntil = null;
            HeldByUserId = userId;
        }

        public void TryFree()
        {
            if (State == SeatState.Free)
            {
                return;
            }

            if (State == SeatState.TemporarilyHeld)
            {
                State = SeatState.Free;
                HeldByUserId = null;
                HeldUntil = null;
            }
        }

        public void TryFreeExpired()
        {
            if (State == SeatState.Free)
            {
                return;
            }

            if (State == SeatState.TemporarilyHeld && HeldUntil <= DateTime.UtcNow)
            {
                State = SeatState.Free;
                HeldByUserId = null;
                HeldUntil = null;
            }
        }

        public bool IsHeldBy(string userId) => State == SeatState.TemporarilyHeld &&
                                               HeldByUserId == userId;

        private bool IsFree() => State == SeatState.Free ||
                                 State == SeatState.TemporarilyHeld && HeldUntil <= DateTime.UtcNow;
    }
}