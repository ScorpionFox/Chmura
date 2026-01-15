using Microsoft.EntityFrameworkCore;
using NotesApi.Data;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Connection string by environment
var csName = builder.Environment.IsProduction() ? "Docker" : "Default";
var cs = builder.Configuration.GetConnectionString(csName);

builder.Services.AddDbContext<NoteDbContext>(options =>
{
    options.UseSqlServer(cs);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NoteDbContext>();

    var retries = 30;                 // 30 * 5s = max ~150s
    var delay = TimeSpan.FromSeconds(5);

    while (true)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (SqlException ex) when (
            ex.Number == -2 || // timeout
            ex.Number == 53 || // network/instance not found
            ex.Number == 40 || // could not open connection
            ex.Number == 18456   // login failed
        )
        {
            retries--;
            if (retries <= 0) throw;

            Console.WriteLine($"[DB] SQL not ready yet (#{ex.Number}). Retrying in {delay.TotalSeconds}s... ({retries} left)");
            Thread.Sleep(delay);
        }
        catch (SqlException ex) when (ex.Number == 1801)
        {
            // Database already exists
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }
    }
}

app.Run();
