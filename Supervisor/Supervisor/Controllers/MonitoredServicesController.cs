namespace Supervisor.Controllers;

using Microsoft.AspNetCore.Mvc;
using Mock;
using Models.DbModels;
using Services;

[ApiController]
[Route("monitoredServices")]
public class MonitoredServicesController : Controller
{
    private readonly IMonitoredServicesRepository _repository;

    public MonitoredServicesController(IMonitoredServicesRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var services = await _repository.GetAllAsync();
        return Ok(services);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var service = await _repository.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound();
        }

        return Ok(service);
    }

    [HttpPost]
    public async Task<IActionResult> Add(MonitoredService service)
    {
        await _repository.AddAsync(service);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, MonitoredService service)
    {
        if (id != service.Id)
        {
            return BadRequest();
        }

        await _repository.UpdateAsync(service);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _repository.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("fillMockServices")]
    public async Task<IActionResult> FillMockServices()
    {
        await MockServices.GenerateAndAddServices(_repository);

        return Ok();
    }
}