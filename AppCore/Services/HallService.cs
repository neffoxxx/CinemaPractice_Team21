using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AppCore.DTOs;
using Infrastructure.Entities;
using Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace AppCore.Services
{
    public class HallService : IHallService
    {
        private readonly IHallRepository _hallRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<HallService> _logger;

        public HallService(IHallRepository hallRepository, IMapper mapper, ILogger<HallService> logger)
        {
            _hallRepository = hallRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<HallDTO>> GetAllHallsAsync()
        {
            _logger.LogInformation("Getting all halls");
            var halls = await _hallRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<HallDTO>>(halls);
        }

        public async Task<HallDTO> GetHallByIdAsync(int id)
        {
            _logger.LogInformation("Getting hall by id: {Id}", id);
            var hall = await _hallRepository.GetByIdAsync(id);
            return _mapper.Map<HallDTO>(hall);
        }

        public async Task AddHallAsync(HallDTO hallDto)
        {
            _logger.LogInformation("Adding new hall: {Name}", hallDto.Name);
            var hall = _mapper.Map<Hall>(hallDto);
            await _hallRepository.AddAsync(hall);
        }

        public async Task UpdateHallAsync(HallDTO hallDto)
        {
            _logger.LogInformation("Updating hall: {Id}", hallDto.HallId);
            var hall = _mapper.Map<Hall>(hallDto);
            await _hallRepository.UpdateAsync(hall);
        }

        public async Task DeleteHallAsync(int id)
        {
            _logger.LogInformation("Deleting hall by id: {Id}", id);
            await _hallRepository.DeleteAsync(id);
        }
    }
    public interface IHallService
    {
        Task<IEnumerable<HallDTO>> GetAllHallsAsync();
        Task<HallDTO> GetHallByIdAsync(int id);
        Task AddHallAsync(HallDTO hallDto);
        Task UpdateHallAsync(HallDTO hallDto);
        Task DeleteHallAsync(int id);
    }
}