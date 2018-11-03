using System;
using System.Linq;
using System.Threading.Tasks;
using FlightsBookingClient;
using FlightsBookingClient.Dto;
using Microsoft.AspNetCore.Mvc;
using FlightsBookingWeb.Models;

namespace FlightsBookingWeb.Controllers
{
    public class FlightController : Controller
    {
        private readonly FlightsClient flightsClient;

        public FlightController()
        {
            flightsClient = new FlightsClient();
        }

        public async Task<IActionResult> Index()
        {
            var flights = await flightsClient.GetFlightsAsync().ConfigureAwait(true);

            var flightListViewModel = new FlightListViewModel
            {
                Flights = flights
                    .Select(f => new FlightViewModel
                    {
                        Id = f.Id,
                        Number = f.Number,
                        Date = f.Date.ToShortDateString()
                    })
                    .ToArray()
            };

            return View(flightListViewModel);
        }

        public async Task<IActionResult> GetFlight(Guid flightId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                var loginForm = new LoginForm { FlightId = flightId};
                return View("Login", loginForm);
            }

            var flight = await flightsClient.GetFlightAsync(flightId).ConfigureAwait(true);

            var bookingViewModel = new FlightBookingViewModel
            {
                Id = flight.Id,
                UserId = userId,
                Number = flight.Number,
                Date = flight.Date.ToShortDateString(),
                Rows = BuildSeats(userId, flight)
            };

            return View("FlightBooking", bookingViewModel);
        }

        public async Task<IActionResult> GetFlightSeats(Guid flightId, string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest();
            }

            var flight = await flightsClient.GetFlightAsync(flightId).ConfigureAwait(true);
            var seats = BuildSeats(userId, flight);

            return View("Seats", seats);
        }

        [HttpPost]
        public async Task<IActionResult> HoldSeat(Guid flightId, string userId, string seatId)
        {
            var result = await flightsClient.HoldSeatAsync(flightId, seatId, userId).ConfigureAwait(true);
            return Ok(result);
        }

        private static SeatRowViewModel[] BuildSeats(string userId, FlightDto flight)
        {
            return flight.Seats
                .GroupBy(x => x.Row)
                .Select(seatRow => new SeatRowViewModel
                {
                    Row = seatRow.Key,
                    Seats = seatRow.Select(seat => new SeatViewModel
                        {
                            IsBusy = seat.State != SeatState.Free,
                            IsHoldByMe = seat.HeldByUserId == userId,
                            Number = seat.Number,
                            Row = seat.Row
                        })
                        .ToArray()
                })
                .OrderBy(x => x.Row)
                .ToArray();
        }
    }
}
