﻿using AutoMapper;
using eTickets.Data.Services.UnitOfWork;
using eTickets.Models;
using eTickets.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace eTickets.Web.Controllers
{
    public class MovieController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MovieController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            IEnumerable<Movie> movies = await _unitOfWork.movieRepository.GetAllAsync(tracked:false);

            List<MovieDto> movieDtos = _mapper.Map<List<MovieDto>>(movies);  
            return View(movieDtos);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> Create(MovieCreateDto createDto)
        {
            if (createDto == null) 
            {
                return BadRequest();
            }
            
            if (ModelState.IsValid)
            {
                // related entities -- cinema,producer,category
                Cinema cinema = await _unitOfWork.cinemaRepository.GetAsync(filter:x=>x.Id == createDto.CinemaId);
                Producer producer = await _unitOfWork.producerRepository.GetAsync(filter:x=>x.Id == createDto.ProducerId);
                Category category = await _unitOfWork.categoryRepository.GetAsync(filter:x=>x.Id == createDto.CategoryId);

                // check if those related entities exists in db or not
                if (cinema == null) 
                {
                    ModelState.AddModelError("","no cinemas found with this id");
                    return BadRequest(ModelState);
                }

                createDto.Cinema = cinema;

                if (producer == null) 
                {
                    ModelState.AddModelError("", "no producers found with this id");
                    return BadRequest(ModelState);
                }

                createDto.Producer = producer;

                if (category == null) 
                {
                    ModelState.AddModelError("", "no categories found with this id");
                    return BadRequest(ModelState);
                }
                
                createDto.Category = category;

                // after we make sured that related entities exists in db .. we will create movie
                Movie movieToDb = _mapper.Map<Movie>(createDto);
                await _unitOfWork.movieRepository.Create(movieToDb);
                return RedirectToAction("Index");
            }
            return View(createDto);
        }
    }
}
