var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureDb(builder.Configuration.GetConnectionString("SqlConnection"));
builder.Services.ConfigureIdentity();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
