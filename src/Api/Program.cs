var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureControllers();
builder.Services.ConfigureApiVersioning();
builder.Services.ConfigureSwagger();

builder.Services.ConfigureDb(builder.Configuration.GetConnectionString("SqlConnection"));
builder.Services.ConfigureIdentity();

builder.Services.ConfigureAuth(builder.Configuration);
builder.Services.ConfigureServices();

var app = builder.Build();

await app.UseDbInitializer();
app.UseApiExceptionHandling();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
