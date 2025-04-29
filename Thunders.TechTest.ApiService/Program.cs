using Thunders.TechTest.ApiService;
using Thunders.TechTest.ApiService.Infra;
using Thunders.TechTest.ApiService.Infra.ApplicationDbContext;
using Thunders.TechTest.ApiService.Services.Bus;
using Thunders.TechTest.ApiService.Services.Report;
using Thunders.TechTest.ApiService.Services.Toll;
using Thunders.TechTest.ApiService.Worker;
using Thunders.TechTest.OutOfBox.Database;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddControllers();

var features = Features.BindFromConfiguration(builder.Configuration);

// Add services to the container.
builder.Services.AddProblemDetails();
builder.AddRedisClient("cache");

if (features.UseMessageBroker)
{
    //builder.Services.AddBus(builder.Configuration, new SubscriptionBuilder());
    builder.AddRabbitMQClient("RabbitMq");
}

if (features.UseEntityFramework)
{
    builder.Services.AddSqlServerDbContext<AppDbContext>(builder.Configuration);
}

builder.Services.AddScoped<IBusService, BusService>();
builder.Services.AddScoped<ITollService, TollService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddHostedService<MessageConsumer>();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    await app.ConfigureDataBaseAsync();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.MapControllers();

app.Run();
