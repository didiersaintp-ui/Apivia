using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Apivia.Services.DataDictionary", Version = "v1" });
});

// Add DbContext here when ready
// builder.Services.AddDbContext<AppDbContext>(...);

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", service = "DataDictionary" }));

app.MapControllers();

app.Run();
