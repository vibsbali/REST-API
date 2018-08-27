﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Library.API.Controllers
{
   [Route("api/authorcollections")]
    public class AuthorCollectionsController : Controller
    {
       private ILibraryRepository _libraryRepository;
       public AuthorCollectionsController(ILibraryRepository libraryRepository)
       {
           _libraryRepository = libraryRepository;
       }

       [HttpPost]
       public IActionResult CreateAuthorCollection([FromBody] IEnumerable<AuthorForCreationDto> authorCollection)
       {
          if (authorCollection == null)
          {
             return BadRequest();
          }

          var authorEntities = Mapper.Map<IEnumerable<Author>>(authorCollection);
          foreach (var authorEntity in authorEntities)
          {
             _libraryRepository.AddAuthor(authorEntity);
          }

           if(!_libraryRepository.Save())
              throw new Exception("Creating an author collection failed on save");

          var authorCollectionToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
          var idsAsString = string.Join(",", authorCollectionToReturn.Select(a => a.Id));

          return CreatedAtRoute("GetAuthorCollection", new {ids = idsAsString}, authorCollectionToReturn);

       }

       [HttpGet("({ids})", Name = "GetAuthorCollection")]
       public IActionResult GetAuthorCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))] IEnumerable<Guid> ids)
       {
          if (ids == null)
          {
             return BadRequest();
          }

          var authorEntities = _libraryRepository.GetAuthors(ids);
          if (ids.Count() != authorEntities.Count())
          {
             return NotFound();
          }

          var authorsToReturn = Mapper.Map<IEnumerable<AuthorDto>>(authorEntities);
          return Ok(authorsToReturn);
       }

       [HttpPost("{id}")]
       public IActionResult BlockAuthorCreation(Guid id)
       {
          if (_libraryRepository.AuthorExists(id))
          {
             return new StatusCodeResult(StatusCodes.Status409Conflict);
          }

          return NotFound();
       }
    }
}