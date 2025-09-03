using AdPlatforms.Data.Infrastructure;
using AdPlatforms.Data.Repositories;
using AdPlatforms.Domain.Repositories;
using AdPlatforms.Domain.UseCases;
using AdPlatforms.Domain.UseCases.Abstractions;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

// Add application services
builder.Services.AddSingleton<ILocationPlatformsDataSource, InMemoryLocationPlatformsDataSource>();
builder.Services.AddSingleton<ILocationPlatformsRepository, LocationPlatformsRepository>();
builder.Services.AddScoped<IAdPlatformSelectorUseCase, AdPlatformSelectorUseCase>();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage()
       .UseSwagger()
       .UseSwaggerUI();

    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
