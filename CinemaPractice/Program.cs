using AppCore.Mapping;
using AppCore.Validators;
using AppCore.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using System;
using Infrastructure.Interfaces;
using AppCore.Interfaces;
using AppCore.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Database Context
builder.Services.AddDbContext<CinemaDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine); // Keep logging for dev, remove for prod
});

// Configure Repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IHallRepository, HallRepository>();

// Configure Services
builder.Services.AddScoped<IGenreService, GenreService>();
builder.Services.AddScoped<IHallService, HallService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IActorService, ActorService>();

// Configure Validators
builder.Services.AddScoped<IValidator<MovieDTO>, MovieValidator>();
builder.Services.AddScoped<IValidator<SessionDTO>, SessionValidator>();
builder.Services.AddScoped<IValidator<TicketDTO>, TicketValidator>();
builder.Services.AddScoped<IValidator<HallDTO>, HallValidator>();

//Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Configure Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.LogoutPath = "/User/Logout";
        options.AccessDeniedPath = "/User/AccessDenied";
        options.Cookie.Name = "CinemaPracticeAuth";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Film/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Film}/{action=Index}/{id?}");

app.Run();