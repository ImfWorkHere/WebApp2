using Application;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GeoNamesController : ControllerBase
{
    private readonly ILogger<GeoNamesController> _logger;
    private readonly IRepository _repository;

    public GeoNamesController(IRepository repository, ILogger<GeoNamesController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet("get")]
    public async Task<IActionResult> GetNames([FromQuery] GeoNamesSearchOptions dto)
    {
        try
        {
            _logger.LogInformation($"Searching for cities near ({dto.Latitude}, {dto.Longitude}).");
            //var items = await _mediator.Send(new GetItemsQuery(dto));
            var items = await _repository.GetRangeAsync(dto);

            if (items.Any()) return Ok(items);

            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to search cities near ({dto.Latitude}, {dto.Longitude}).");
            return Problem();
        }
    }
}