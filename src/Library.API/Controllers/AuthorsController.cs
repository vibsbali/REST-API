using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
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

        [HttpGet("{id}", Name = "GetAuthor")]
        //The name is associated with Create Author Method below 
        public IActionResult GetAuthor(Guid id)
        {
            var authorFromRepo = _libraryRepository.GetAuthor(id);

            if (authorFromRepo == null)
                return NotFound();

            var author = Mapper.Map<AuthorDto>(authorFromRepo);
            return Ok(author);
        }

        [HttpPost]
        public IActionResult CreateAuthor([FromBody] AuthorForCreationDto author)
        {
            if (author == null)
            {
                return BadRequest();
            }

            var authorEntitiy = Mapper.Map<Author>(author);
            _libraryRepository.AddAuthor(authorEntitiy);
            if (!_libraryRepository.Save())
            {
                throw new Exception("Creating an author failed on save");
            }

            var authorToReturn = Mapper.Map<AuthorDto>(authorEntitiy);
            return CreatedAtRoute("GetAuthor", new {id = authorToReturn.Id}, authorToReturn);
        }
    }
}