var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.DrawLastRun_Web>("web");

builder.Build().Run();
