using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors")]
    public class AuthorsController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public AuthorsController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

        [HttpGet]
        public IActionResult GetAuthors()
        {
            var authors = _libraryRepository.GetAuthors();

            var authorsVm = Mapper.Map<IEnumerable<AuthorDto>>(authors);


            return Ok(authorsVm);
        }

        [HttpGet("{id}")]
        public IActionResult GetAuthor(Guid id)
        {
            var autorFromRepo = _libraryRepository.GetAuthor(id);

            if (autorFromRepo == null)
                return NotFound();

            var author = Mapper.Map<AuthorDto>(autorFromRepo);
            return Ok(author);
        }
    }
}