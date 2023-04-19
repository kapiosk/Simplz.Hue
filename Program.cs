var builder = WebApplication.CreateBuilder(args);

await using var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
