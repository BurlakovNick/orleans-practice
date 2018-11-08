using System.Threading.Tasks;
using FlightsBookingClient.Dto;
using Orleans;

namespace FlightsBooking.Model
{
    public interface IFlightGrain : IGrainWithGuidKey
    {
        Task<FlightDto> GetFlightAsync();
        Task<Result> HoldSeatAsync(string userId, string seatId, int holdSeconds);
        Task<Result> BuySeatAsync(string userId, string seatId);
        Task<Result> FreeSeatAsync(string userId, string seatId);
    }
}