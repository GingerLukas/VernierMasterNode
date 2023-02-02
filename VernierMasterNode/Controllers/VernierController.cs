using Microsoft.AspNetCore.Mvc;
using VernierMasterNode.Services;
using VernierMasterNode.Shared;

namespace VernierMasterNode.Controllers;

[ApiController]
[Route("[controller]")]
public class VernierController : ControllerBase
{
    private readonly DeviceService _deviceService;
    private readonly CommandService _commandService;

    public VernierController(IHttpContextAccessor contextAccessor, DeviceService deviceService,
        CommandService commandService)
    {
        _deviceService = deviceService;
        _commandService = commandService;
    }

    [HttpGet("[action]")]
    public IActionResult StartScan(string espAddress)
    {
        EspDevice? device = _deviceService.GetDevice(espAddress);
        if (device != null)
        {
            _commandService.StartScan(device.Name);
        }

        return Ok();
    }

    [HttpGet("[action]")]
    public IActionResult StopScan(string espAddress)
    {
        EspDevice? device = _deviceService.GetDevice(espAddress);
        if (device != null)
        {
            _commandService.StopScan(device.Name);
        }

        return Ok();
    }

    [HttpGet("[action]")]
    public IActionResult ConnectToAvailable(string espAddress)
    {
        EspDevice? device = _deviceService.GetDevice(espAddress);
        if (device != null)
        {
            foreach (ulong serialId in device.SeenDevices)
            {
                _commandService.ConnectToDevice(espAddress, serialId);
            }
        }

        return Ok();
    }
    
    [HttpGet("[action]")]
    public IActionResult StartSensor(string espAddress, UInt64 serialId, UInt32 sensorId)
    {
        EspDevice? device = _deviceService.GetDevice(espAddress);
        if (device == null)
        {
            return NotFound();
        }

        _commandService.StartSensor(espAddress, serialId, sensorId);

        return Ok();
    }
}