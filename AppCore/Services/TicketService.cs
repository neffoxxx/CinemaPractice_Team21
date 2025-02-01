using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Infrastructure.Entities;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using AppCore.DTOs;

namespace AppCore.Services
{
    public class TicketService : ITicketService
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly ISessionRepository _sessionRepository;
        private readonly ILogger<TicketService> _logger;
        private readonly IMapper _mapper;

        public TicketService(ITicketRepository ticketRepository,
        ILogger<TicketService> logger,
        ISessionRepository sessionRepository,
        IMapper mapper)
        {
            _ticketRepository = ticketRepository;
            _logger = logger;
            _sessionRepository = sessionRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<TicketDTO>> GetAllTicketsAsync()
        {
            _logger.LogInformation("Getting all tickets with related data");
            var tickets = await _ticketRepository.GetAllWithDetailsAsync();
            return _mapper.Map<IEnumerable<TicketDTO>>(tickets);
        }

        public async Task<TicketDTO> GetTicketByIdAsync(int id)
        {
            _logger.LogInformation("Getting ticket by ID: {Id}", id);
            var ticket = await _ticketRepository.GetByIdWithDetailsAsync(id);
            if (ticket == null)
            {
                _logger.LogWarning("Ticket not found with id: {Id}", id);
                return null;
            }

            return _mapper.Map<TicketDTO>(ticket);
        }

        public async Task UpdateTicketAsync(TicketDTO ticketDto)
        {
            _logger.LogInformation("Updating ticket, ID: {TicketId}", ticketDto.TicketId);

            var ticket = await _ticketRepository.GetByIdAsync(ticketDto.TicketId);
            if (ticket == null)
            {
                _logger.LogError("Ticket not found, ID: {TicketId}", ticketDto.TicketId);
                throw new Exception($"Ticket not found, ID: {ticketDto.TicketId}");
            }
            _mapper.Map(ticketDto, ticket);

            await _ticketRepository.UpdateAsync(ticket);

        }

        public async Task DeleteTicketAsync(int id)
        {
            _logger.LogInformation("Deleting ticket, ID: {TicketId}", id);
            await _ticketRepository.DeleteAsync(id);
        }

        public async Task<bool> IsSeatAvailable(int sessionId, int rowNumber, int seatNumber)
        {
            _logger.LogInformation("Checking seat availability for session {SessionId}, row {RowNumber}, seat {SeatNumber}",
                sessionId, rowNumber, seatNumber);

            var isSeatBooked = await _ticketRepository.AnyAsync(
                t => ((Ticket)t).SessionId == sessionId && ((Ticket)t).RowNumber == rowNumber && ((Ticket)t).SeatNumber == seatNumber.ToString()
            );
            return !isSeatBooked;
        }

        public async Task<(int Row, int Seat)> CalculateSeatPosition(int globalSeatNumber, int seatsPerRow)
        {
            _logger.LogInformation("Calculating seat position for global seat number: {GlobalSeatNumber}, with seats per row: {SeatsPerRow}", globalSeatNumber, seatsPerRow);
            if (seatsPerRow <= 0)
            {
                _logger.LogError("Seats per row must be greater than 0.");
                throw new ArgumentOutOfRangeException(nameof(seatsPerRow), "Seats per row must be greater than 0.");
            }
            if (globalSeatNumber <= 0)
            {
                _logger.LogError("Global seat number must be greater than 0.");
                throw new ArgumentOutOfRangeException(nameof(globalSeatNumber), "Global seat number must be greater than 0.");
            }
            int rowNumber = ((globalSeatNumber - 1) / seatsPerRow) + 1;
            int seatInRow = ((globalSeatNumber - 1) % seatsPerRow) + 1;

            _logger.LogInformation("Seat position calculated: Row {RowNumber}, Seat {SeatInRow}", rowNumber, seatInRow);
            return (rowNumber, seatInRow);
        }

        public async Task<List<int>> GetAvailableSeatsForRow(int sessionId, int rowNumber, int seatsPerRow)
        {
            _logger.LogInformation("Getting available seats for session: {SessionId}, row: {RowNumber}, seats per row: {SeatsPerRow}", sessionId, rowNumber, seatsPerRow);

            if (sessionId <= 0)
            {
                _logger.LogError("SessionId must be greater than 0");
                throw new ArgumentOutOfRangeException(nameof(sessionId), "SessionId must be greater than 0.");
            }
            if (rowNumber <= 0)
            {
                _logger.LogError("Row number must be greater than 0");
                throw new ArgumentOutOfRangeException(nameof(rowNumber), "Row number must be greater than 0.");
            }
            if (seatsPerRow <= 0)
            {
                _logger.LogError("Seats per row must be greater than 0");
                throw new ArgumentOutOfRangeException(nameof(seatsPerRow), "Seats per row must be greater than 0.");
            }

            var tickets = await _ticketRepository.GetAllAsync();
            var bookedSeatsInRow = tickets
                .Where(t => t.SessionId == sessionId && t.RowNumber == rowNumber)
                .Select(t => int.Parse(t.SeatNumber))
                .ToList();

            var availableSeats = Enumerable.Range(1, seatsPerRow)
                .Where(seat => !bookedSeatsInRow.Contains(seat))
                .ToList();

            _logger.LogInformation("Found {Count} available seats for session: {SessionId}, row: {RowNumber}", availableSeats.Count, sessionId, rowNumber);
            return availableSeats;
        }

        public async Task<List<int>> GetRowsWithAvailableSeats(int sessionId, int totalRows, int seatsPerRow)
        {
            _logger.LogInformation("Getting rows with available seats for session: {SessionId}, total rows: {TotalRows}, seats per row: {SeatsPerRow}", sessionId, totalRows, seatsPerRow);
            if (sessionId <= 0)
            {
                _logger.LogError("SessionId must be greater than 0.");
                throw new ArgumentOutOfRangeException(nameof(sessionId), "SessionId must be greater than 0.");
            }
            if (totalRows <= 0)
            {
                _logger.LogError("Total rows must be greater than 0.");
                throw new ArgumentOutOfRangeException(nameof(totalRows), "Total rows must be greater than 0.");
            }
            if (seatsPerRow <= 0)
            {
                _logger.LogError("Seats per row must be greater than 0.");
                throw new ArgumentOutOfRangeException(nameof(seatsPerRow), "Seats per row must be greater than 0.");
            }
            var tickets = await _ticketRepository.GetAllAsync();

            var availableRows = new List<int>();

            for (int row = 1; row <= totalRows; row++)
            {
                var bookedSeatsInRow = tickets
                    .Where(t => t.SessionId == sessionId && t.RowNumber == row)
                    .Select(t => int.Parse(t.SeatNumber))
                    .ToList();

                if (bookedSeatsInRow.Count < seatsPerRow)
                {
                    availableRows.Add(row);
                }
            }
            _logger.LogInformation("Found {Count} rows with available seats for session: {SessionId}", availableRows.Count, sessionId);
            return availableRows;
        }
    }
}