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
            return _mapper.Map<IEnumerable<TicketDTO>>(tickets) ?? Enumerable.Empty<TicketDTO>();
        }

        public async Task<TicketDTO?> GetTicketByIdAsync(int id)
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

            var tickets = await _ticketRepository.GetTicketsBySessionAsync(sessionId);
            var isBooked = tickets.Any(t => 
                t.RowNumber == rowNumber && 
                t.SeatNumber == seatNumber.ToString() && 
                t.Status == "Booked");

            _logger.LogInformation("Seat in session {SessionId}, row {RowNumber}, seat {SeatNumber} is {Status}",
                sessionId, rowNumber, seatNumber, isBooked ? "booked" : "available");

            return !isBooked;
        }

        public async Task<(int Row, int Seat)> CalculateSeatPosition(int globalSeatNumber, int seatsPerRow)
        {
            await Task.CompletedTask; // Якщо немає реальних асинхронних операцій
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
            _logger.LogInformation("Starting GetRowsWithAvailableSeats with parameters:" +
                "\nsessionId: {SessionId}" +
                "\ntotalRows: {TotalRows}" +
                "\nseatsPerRow: {SeatsPerRow}", 
                sessionId, totalRows, seatsPerRow);

            // Валідація вхідних параметрів
            if (sessionId <= 0 || totalRows <= 0 || seatsPerRow <= 0)
            {
                _logger.LogError("Invalid parameters received");
                throw new ArgumentException("All parameters must be greater than 0");
            }

            try
            {
                // Отримуємо заброньовані квитки
                var tickets = await _ticketRepository.GetTicketsBySessionAsync(sessionId);
                _logger.LogInformation("Found {Count} booked tickets for session {SessionId}", 
                    tickets.Count(), sessionId);

                // ВАЖЛИВО: Завжди повертаємо всі ряди, якщо немає заброньованих квитків
                if (!tickets.Any())
                {
                    var allRows = Enumerable.Range(1, totalRows).ToList();
                    _logger.LogInformation("No booked tickets found. Returning all rows: {Rows}", 
                        string.Join(", ", allRows));
                    return allRows;
                }

                var availableRows = new List<int>();

                // Перевіряємо кожен ряд
                for (int row = 1; row <= totalRows; row++)
                {
                    var bookedSeatsInRow = tickets.Count(t => t.RowNumber == row);
                    _logger.LogInformation("Row {Row}: {BookedCount} seats booked out of {TotalSeats}", 
                        row, bookedSeatsInRow, seatsPerRow);

                    if (bookedSeatsInRow < seatsPerRow)
                    {
                        availableRows.Add(row);
                        _logger.LogInformation("Added row {Row} to available rows", row);
                    }
                }

                // Додаткова перевірка: якщо список порожній, але повинен бути не порожнім
                if (!availableRows.Any() && tickets.Count() < (totalRows * seatsPerRow))
                {
                    _logger.LogWarning("No available rows found but there should be some. Returning all rows.");
                    return Enumerable.Range(1, totalRows).ToList();
                }

                _logger.LogInformation("Returning {Count} available rows: {Rows}", 
                    availableRows.Count, string.Join(", ", availableRows));

                return availableRows;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRowsWithAvailableSeats");
                // В разі помилки повертаємо всі ряди, щоб не блокувати бронювання
                return Enumerable.Range(1, totalRows).ToList();
            }
        }
    }
}