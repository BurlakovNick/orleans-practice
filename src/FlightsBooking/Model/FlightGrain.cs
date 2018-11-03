using System;
using System.Linq;
using System.Threading.Tasks;
using FlightsBookingClient.Dto;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Providers;

namespace FlightsBooking.Model
{
    [StorageProvider(ProviderName = "SqlStorage")]
    public class FlightGrain : Grain<Flight>, IFlightGrain
    {
        private readonly ILogger logger;
        private IDisposable freeSeatsTimer;

        public FlightGrain(
            ILogger<FlightGrain> logger
            )
        {
            this.logger = logger;
        }

        //todo: сделать логирование с помощью фильтров

        public override async Task OnActivateAsync()
        {
            Log("Activating...");

            if (State == null || State.IsActivated == false)
            {
                Log("New flight, let's initialize state");

                var random = new Random();
                State = new Flight
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

            RegisterFreeSeatsTimerIfNeed();

            await base.OnActivateAsync();

            Log("Activated!");
        }

        public Task<FlightDto> GetFlightAsync()
        {
            var flight = State;

            return Task.FromResult(
                new FlightDto
                {
                    Id = GrainReference.GetPrimaryKey(),
                    Number = flight.Number,
                    Date = flight.Date,
                    Seats = flight.Seats.Select(s => new SeatDto
                        {
                            Row = s.Row,
                            Number = s.Number,
                            State = s.State,
                            HeldUntil = s.HeldUntil,
                            HeldByUserId = s.HeldByUserId
                        })
                        .ToArray()
                });
        }

        public async Task<Result> HoldSeatAsync(string userId, string seatId, int holdSeconds)
        {
            var holdTime = TimeSpan.FromSeconds(holdSeconds);
            var result = State.TryHold(userId, seatId, holdTime);
            if (!result.IsSuccess)
            {
                return result;
            }

            RegisterFreeSeatsTimerIfNeed();

            await WriteStateAsync();
            return Result.Success();
        }

        public async Task<Result> BuySeatAsync(string userId, string seatId)
        {
            var result = State.TryBuy(userId, seatId);
            if (!result.IsSuccess)
            {
                return result;
            }

            await WriteStateAsync();
            return Result.Success();
        }

        private async Task FreeSeats(object args)
        {
            Log("freeing seats...");

            if (State.HasHeldSeats)
            {
                State.FreeExpiredSeats();
            }

            if (!State.HasHeldSeats)
            {
                freeSeatsTimer.Dispose();
            }

            await WriteStateAsync();
        }

        private void RegisterFreeSeatsTimerIfNeed()
        {
            if (State.HasHeldSeats && freeSeatsTimer == null)
            {
                freeSeatsTimer = RegisterTimer(FreeSeats, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            }
        }

        private void Log(string message)
        {
            logger.LogInformation($"{GetFlightId()}: {message}");
        }

        private string GetFlightId()
        {
            return GrainReference.ToKeyString();
        }
    }
}