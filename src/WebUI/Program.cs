using Infrastructure;

ILogger? logger = null;
try
{
    var builder = WebApplication.CreateBuilder(args);
    var config = builder.Configuration;
    config.AddEnvironmentVariables();

    builder.Logging.AddConsole();

    // Add services to the container.

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddInfrastructure(config);

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddSwaggerGen();

    var app = builder.Build();
    logger = app.Logger;

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger?.LogCritical(ex, "Found critical unhandled error.");
}
