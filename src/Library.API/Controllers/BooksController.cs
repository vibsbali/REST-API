using System;
using System.Collections.Generic;
using AutoMapper;
using Library.API.Entities;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    public class BooksController : Controller
    {
        private readonly ILibraryRepository _libraryRepository;

        public BooksController(ILibraryRepository libraryRepository)
        {
            _libraryRepository = libraryRepository;
        }

       
        [HttpGet]
        public IActionResult GetBooksForAuthor(Guid authorId)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var books = _libraryRepository.GetBooksForAuthor(authorId);
            var booksDto = Mapper.Map<IEnumerable<BookDto>>(books);

            return Ok(booksDto);
        }

        [HttpGet("{id}", Name = "GetBookForAuthor")]
        public IActionResult GetBookForAuthor(Guid authorId, Guid id)
        {
            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var book = _libraryRepository.GetBookForAuthor(authorId, id);

            if (book == null)
                return NotFound("Book Not Found");

            var bookDto = Mapper.Map<BookDto>(book);
            return Ok(bookDto);
        }

        [HttpPost]
        public IActionResult CreateBookForAuthor(Guid authorId, [FromBody] BookForCreationDto book)
        {
            if (book == null)
            {
                return BadRequest();
            }

            if (!_libraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var bookEntity = Mapper.Map<Book>(book);

            _libraryRepository.AddBookForAuthor(authorId, bookEntity);

            if (!_libraryRepository.Save())
            {
               throw  new Exception($"Creating a book for author {authorId} failed to save");
            }

            var bookToReturn = Mapper.Map<BookDto>(bookEntity);

            //Notice the bookToReturn (newly created book) at the end. It is required as this is 
            //going to be seralised into request body
            return CreatedAtRoute("GetBookForAuthor",
                new {authorId = authorId, id = bookToReturn.Id}, bookToReturn);
        }

       [HttpDelete("{id}")]
       public IActionResult DeleteBookForAuthor(Guid authorId, Guid id)
       {
          if (!_libraryRepository.AuthorExists(authorId))
          {
             return NotFound();
          }

          var bookForAuthorFromRepo = _libraryRepository.GetBookForAuthor(authorId, id);
          if (bookForAuthorFromRepo == null)
          {
             return NotFound();
          }

            _libraryRepository.DeleteBook(bookForAuthorFromRepo);
          if (!_libraryRepository.Save())
          {
             throw new Exception($"Deleting book {id} for author {authorId} failed on saving"); 
          }

          return NoContent();
       }
    }
}