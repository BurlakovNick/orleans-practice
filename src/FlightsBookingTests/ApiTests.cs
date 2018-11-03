using System;
using System.Linq;
using System.Threading.Tasks;
using FlightsBookingClient;
using FlightsBookingClient.Dto;
using FluentAssertions;
using NUnit.Framework;

namespace FlightsBookingTests
{
    [TestFixture]
    public class ApiTests
    {
        private FlightsClient flightsClient;

        [SetUp]
        public void SetUp()
        {
            flightsClient = new FlightsClient(true);
        }

        [Test]
        public async Task Test_Booking()
        {
            var flightId = Guid.NewGuid();
            var seatId = "5A";

            await AssertFlightState(flightId, seatId, SeatState.Free);

            var actual = await flightsClient.HoldSeatAsync(flightId, seatId, "luke");
            actual.IsSuccess.Should().BeTrue();
            await AssertFlightState(flightId, seatId, SeatState.TemporarilyHeld);

            actual = await flightsClient.BuySeatAsync(flightId, seatId, "dart");
            actual.IsSuccess.Should().BeFalse();

            actual = await flightsClient.BuySeatAsync(flightId, seatId, "luke");
            actual.IsSuccess.Should().BeTrue();
            await AssertFlightState(flightId, seatId, SeatState.Busy);
        }

        [Test]
        public async Task Test_Hold_Another_Seat()
        {
            var flightId = Guid.NewGuid();
            var seatId1 = "1A";
            var seatId2 = "1B";

            await AssertFlightState(flightId, seatId1, SeatState.Free);
            await AssertFlightState(flightId, seatId2, SeatState.Free);

            var actual = await flightsClient.HoldSeatAsync(flightId, seatId1, "luke");
            actual.IsSuccess.Should().BeTrue();
            await AssertFlightState(flightId, seatId1, SeatState.TemporarilyHeld);

            actual = await flightsClient.HoldSeatAsync(flightId, seatId2, "luke");
            actual.IsSuccess.Should().BeTrue();
            await AssertFlightState(flightId, seatId1, SeatState.Free);
            await AssertFlightState(flightId, seatId2, SeatState.TemporarilyHeld);

            actual = await flightsClient.BuySeatAsync(flightId, seatId1, "luke");
            actual.IsSuccess.Should().BeTrue();
            await AssertFlightState(flightId, seatId1, SeatState.Busy);
            await AssertFlightState(flightId, seatId2, SeatState.Free);
        }

        [Test]
        public async Task Test_Seats_Are_Free_After_Hold_Time()
        {
            var flightId = Guid.NewGuid();
            var userId = "luke";
            var holdSeconds = 10;

            await ForEachSeat(async seat =>
            {
                await AssertFlightState(flightId, seat, SeatState.Free);

                var actual = await flightsClient.HoldSeatAsync(flightId, seat, userId, holdSeconds);
                actual.IsSuccess.Should().BeTrue();
            });

            await Task.Delay(TimeSpan.FromSeconds(holdSeconds * 2 + 1));

            await ForEachSeat(async seat =>
            {
                var flight = await flightsClient.GetFlightAsync(flightId);
                flight.Seats.Single(x => x.Id == seat).State.Should().Be(SeatState.Free);
            });

            async Task ForEachSeat(Func<string, Task> func)
            {
                foreach (var seatId in new [] { "1A", "1B", "1C"})
                {
                    await func(seatId);
                }
            }
        }

        private async Task AssertFlightState(Guid flightId, string seatId, SeatState expected)
        {
            var flight = await flightsClient.GetFlightAsync(flightId);
            flight.Seats.Single(x => x.Id == seatId).State.Should().Be(expected);
        }
    }
}
