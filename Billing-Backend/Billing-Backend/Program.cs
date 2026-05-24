using Billing_Backend.Data;
using Billing_Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 👇 1. ADD CORS POLICY CONFIGURATION HERE 👇
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Points directly to your Angular app port
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Assign Database Context Service connected to PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Assign Business Logic Services
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<BillService>();
builder.Services.AddScoped<PdfService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Billing API v1");
    });
}

app.UseHttpsRedirection();

// 👇 2. MIDDLEWARE PIPELINE REGISTRATION 👇
// This MUST be placed precisely before UseAuthorization and MapControllers!
app.UseCors("AllowFrontend");   

app.UseAuthorization();

app.MapControllers();

app.Run();