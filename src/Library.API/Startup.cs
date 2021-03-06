﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Library.API.Services;
using Library.API.Entities;
using Library.API.Helpers;
using Library.API.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;

namespace Library.API
{
   public class Startup
   {
      public static IConfiguration Configuration;

      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;
      }

      // This method gets called by the runtime. Use this method to add services to the container.
      // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddMvc(setupAction =>
         {
            setupAction.ReturnHttpNotAcceptable = true;
            setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());
            setupAction.InputFormatters.Add(new XmlDataContractSerializerInputFormatter());
         });

         // register the DbContext on the container, getting the connection string from
         // appSettings (note: use this during development; in a production environment,
         // it's better to store the connection string in an environment variable)
         var connectionString = Configuration["connectionStrings:libraryDBConnectionString"];
         services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));


         // register the repository
         // seeding happens in Configure method
         services.AddScoped<ILibraryRepository, LibraryRepository>();
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IHostingEnvironment env,
          ILoggerFactory loggerFactory, LibraryContext libraryContext)
      {
         if (env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
         }
         else
         {
            app.UseExceptionHandler(appBuilder =>
            {
               appBuilder.Run(async context =>
                   {
                   context.Response.StatusCode = 500;
                   await context.Response.WriteAsync("An unexpected fault happened!");
                });
            });
         }

         AutoMapper.Mapper.Initialize(cfg =>
         {
            cfg.CreateMap<Author, AuthorDto>()
                   .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                       $"{src.FirstName} {src.LastName}"))
                   .ForMember(dest => dest.Age, opt => opt.MapFrom(src =>
                       src.DateOfBirth.GetCurrentAge()));

               //Books
               cfg.CreateMap<Book, BookDto>();

               //Creation of Author
               cfg.CreateMap<AuthorForCreationDto, Author>();

            cfg.CreateMap<BookForCreationDto, Book>();
         });

         //setup seeding, the database is setup in ConfigureServices
         libraryContext.EnsureSeedDataForContext();

         app.UseMvc();
      }
   }
}
