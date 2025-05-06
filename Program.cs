using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PromotionTasksService.Data;
using PromotionTasksService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddScoped<PromotionTasksService.Services.PromotionTasksService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ReleaseService>();
builder.Services.AddScoped<DatabaseSeeder>();
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
// TODO: Remove the swagger middleware when the API is ready for production.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();
    var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();
    var seeder = new DatabaseSeeder(dbContext, logger);
    seeder.SeedAsync().GetAwaiter().GetResult();
}

app.Run();
