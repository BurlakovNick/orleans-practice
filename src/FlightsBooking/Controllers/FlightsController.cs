using System;
using System.Linq;
using System.Threading.Tasks;
using FlightsBooking.Model;
using FlightsBookingClient.Dto;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace FlightsBooking.Controllers
{
    [Route("api/[controller]")]
    public class FlightsController : Controller
    {
        private readonly IClusterClient clusterClient;

        public FlightsController(
            IClusterClient clusterClient
        )
        {
            this.clusterClient = clusterClient;
        }

        [HttpGet]
        public async Task<FlightDto[]> GetFlights()
        {
            var flightIds = new []
            {
                Guid.Parse("F33B139B-F2E7-41B7-9DE5-502ADCA58B73"),
                Guid.Parse("25277C64-3632-4D2E-BC52-224591368AD7"),
                Guid.Parse("7E7DD011-A73F-4065-8193-2DA25AAB2DDB"),
                Guid.Parse("1256F1E4-38A2-4E27-BA09-E7A459B98718"),
            };

            return await Task.WhenAll(
                flightIds.Select(id =>
                {
                    var flightGrain1 = clusterClient.GetGrain<IFlightGrain>(id);
                    return flightGrain1.GetFlightAsync();
                })).ConfigureAwait(true);
        }

        [HttpGet("{flightId}")]
        public async Task<FlightDto> GetFlight(Guid flightId)
        {
            var flightGrain = clusterClient.GetGrain<IFlightGrain>(flightId);
            return await flightGrain.GetFlightAsync().ConfigureAwait(true);
        }

        [HttpPost("{flightId}/seat/{seatId}/hold")]
        public async Task<Result> HoldAsync(Guid flightId, string seatId, [FromQuery] string userId, [FromQuery] int? holdSeconds = null)
        {
            var recipientGrain = clusterClient.GetGrain<IFlightGrain>(flightId);
            return await recipientGrain.HoldSeatAsync(userId, seatId, holdSeconds ?? 300).ConfigureAwait(true);
        }

        [HttpPost("{flightId}/seat/{seatId}/buy")]
        public async Task<Result> BuyAsync(Guid flightId, string seatId, [FromQuery] string userId)
        {
            var recipientGrain = clusterClient.GetGrain<IFlightGrain>(flightId);
            return await recipientGrain.BuySeatAsync(userId, seatId).ConfigureAwait(true);
        }
    }
}
