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
                Guid.Parse("8D3707DE-1A6A-449A-B935-AA8F039EE56D"),
                Guid.Parse("AC40C336-C58B-4369-BACD-9682CAE0A067"),
                Guid.Parse("EBFB351A-8C98-48E9-B23B-BAD413960DD9"),
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

        [HttpPost("{flightId}/seat/{seatId}/free")]
        public async Task<Result> FreeSeat(Guid flightId, string seatId, [FromQuery] string userId)
        {
            try
            {
                var recipientGrain = clusterClient.GetGrain<IFlightGrain>(flightId);
                return await recipientGrain.FreeSeatAsync(userId, seatId).ConfigureAwait(true);
            }
            catch (NotImplementedException)
            {
                return Result.Fail("Method is not implemented");
            }
        }
    }
}
