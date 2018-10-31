using System.Threading.Tasks;
using FlightsBooking.Dto;
using Orleans;

namespace FlightsBooking.Model
{
    public interface IFlightGrain : IGrainWithGuidKey
    {
        Task<FlightDto> GetFlightAsync();
        Task<Result> HoldSeatAsync(string userId, string seatId, int holdSeconds);
        Task<Result> BuySeatAsync(string userId, string seatId);
    }
}