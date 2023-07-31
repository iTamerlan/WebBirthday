// начальные данные
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;



var builder = WebApplication.CreateBuilder();

// получаем строку подключени€ из файла конфигурации
string connection = builder.Configuration.GetConnectionString("DefaultConnection");

// добавл€ем контекст ApplicationContext в качестве сервиса в приложение
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/users", async (ApplicationContext db) => await db.Users.OrderBy(p => p.Birthday).ToListAsync()); //.Take(5).OrderBy(p => p.DayOfYear) OrderBy(p => p.Birthday)

app.MapGet("/api/users/{id:int}", async (int id, ApplicationContext db) =>
{
    // получаем пользовател€ по id
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    // если не найден, отправл€ем статусный код и сообщение об ошибке
    if (user == null) return Results.NotFound(new { message = "ѕользователь не найден" });
    
    // если пользователь найден, отправл€ем его
    return Results.Json(user);
});

app.MapDelete("/api/users/{id:int}", async (int id, ApplicationContext db) =>
{
    // получаем пользовател€ по id
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    // если не найден, отправл€ем статусный код и сообщение об ошибке
    if (user == null) return Results.NotFound(new { message = "ѕользователь не найден" });

    // если пользователь найден, удал€ем его
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.Json(user);
});

app.MapPost("/api/users", async (User user, ApplicationContext db) =>
{
    // добавл€ем пользовател€ в массив
    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();
    return user;
});

app.MapPut("/api/users", async (User userData, ApplicationContext db) =>
{
    // получаем пользовател€ по id
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userData.Id);

    // если не найден, отправл€ем статусный код и сообщение об ошибке
    if (user == null) return Results.NotFound(new { message = "ѕользователь не найден" });

    // если пользователь найден, измен€ем его данные и отправл€ем обратно клиенту
    user.Name = userData.Name;
    user.Birthday = userData.Birthday;
    user.Type = userData.Type;
    user.Photo = userData.Photo;
    //user.Photo = Convert.FromBase64String(userData.Photo);

    await db.SaveChangesAsync();
    return Results.Json(user);
});

app.Run();



public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = ""; // им€ пользовател€
    [Column(TypeName = "date")]
    public DateTime Birthday { get; set; } // ƒень рождени€ пользовател€
    public bool Type { get; set; } // ¬ажное?
    public string? Photo { get; set; } // ‘ото пользовател€ // Convert.FromBase64String (string s);

    //public decimal DayOfYear => Birthday.DayOfYear;
    [NotMapped]
    public int? DayOfYear {
        get
        {
            int temp = Birthday.DayOfYear;
            int n = DateTime.Now.DayOfYear;
            if (temp < n)
            {
                temp += 366;
            }
            return temp;
        }
    }
}

public class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    //public string img = "";
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
        //Database.EnsureDeleted();
        Database.EnsureCreated();   // создаем базу данных при первом обращении
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "јн€", Birthday = Convert.ToDateTime("2005-08-04"), Type=true },
                new User { Id = 2, Name = "—ветлана »вановна", Birthday = Convert.ToDateTime("1974-12-31"), Type = false },
                new User { Id = 3, Name = "дедушка", Birthday = Convert.ToDateTime("1937-01-15"), Type = true }
        );
    }
}