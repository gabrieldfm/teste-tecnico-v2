var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithDataVolume();

var rabbitMqPassword = builder.AddParameter("RabbitMqPassword", true);
var rabbitMq = builder.AddRabbitMQ("RabbitMq", password: rabbitMqPassword)
    .WithDataVolume()
    .WithVolume("/etc/rabbitmq")
    .WithManagementPlugin();

var sqlServerPassword = builder.AddParameter("SqlServerInstancePassword", true);
var sqlServer = builder.AddSqlServer("SqlServerInstance", sqlServerPassword, 52599)
    .WithDataVolume();

var database = sqlServer.AddDatabase("ThundersTechTestDb", "ThundersTechTest");

var apiService = builder.AddProject<Projects.Thunders_TechTest_ApiService>("apiservice")
    .WithReference(rabbitMq)
    .WaitFor(rabbitMq)
    .WithReference(database)
    .WaitFor(database)
    .WithReference(cache)
    .WaitFor(cache);

builder.Build().Run();
