using VernierMasterNode.Hubs;
using VernierMasterNode.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();


builder.Services.AddSingleton<VernierUdpService>();
builder.Services.AddSingleton<SensorService>();
builder.Services.AddSingleton<HearBeatService>();
builder.Services.AddSingleton<DeviceService>();
builder.Services.AddSingleton<CommandService>();
builder.Services.AddSingleton<EventService>();

var app = builder.Build();
app.MapHub<RealtimeHub>("/Realtime");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();


app.UseAuthorization();

app.MapControllers();




VernierUdpService? udpService = app.Services.GetService<VernierUdpService>();
SensorService? sensorService = app.Services.GetService<SensorService>();
HearBeatService? heartbeatService = app.Services.GetService<HearBeatService>();
DeviceService? deviceService = app.Services.GetService<DeviceService>();
CommandService? commandService = app.Services.GetService<CommandService>();
EventService? eventService = app.Services.GetService<EventService>();

app.Run();