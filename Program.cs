using Microsoft.EntityFrameworkCore;
using Splitkaro.API.Data;
using Splitkaro.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ✅ CORS (Angular → .NET)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Email service
builder.Services.AddScoped<EmailService>();

// DB Context
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ CORS MUST be here
app.UseCors("AllowAngular");

app.UseAuthorization();
app.MapControllers();
app.Run();
