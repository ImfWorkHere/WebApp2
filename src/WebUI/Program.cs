using Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Information()
	.WriteTo.Console()
	.WriteTo.Seq("http://seq:5341")
	.CreateLogger();

try
{
	Log.Information("Starting web application");

	var builder = WebApplication.CreateBuilder(args);

    var config = builder.Configuration;
    config.AddEnvironmentVariables();

    builder.Host.UseSerilog();

	// Add services to the container.

	builder.Services.AddSwaggerGen();
	builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddInfrastructure(config);

	var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
	Log.CloseAndFlush();
}
