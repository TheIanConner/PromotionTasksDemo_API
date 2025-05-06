using Microsoft.EntityFrameworkCore;
using PromotionTasksService.Data;
using PromotionTasksService.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.MaxDepth = 64;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
// TODO: Remove the swagger middleware when the API is ready for production.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.Run();
