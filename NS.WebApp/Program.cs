var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSoloDBDataServices(
    builder.Configuration.GetValue<string>("SoloDB:DatabasePath") ?? "nimble-sheet.db");

builder.Services.AddFastEndpoints(o =>
    o.Assemblies = [typeof(AssemblyMarker).Assembly]);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseFastEndpoints();

app.Run();
