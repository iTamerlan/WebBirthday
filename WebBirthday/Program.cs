// начальные данные
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;



var builder = WebApplication.CreateBuilder();

// получаем строку подключения из файла конфигурации
string connection = builder.Configuration.GetConnectionString("DefaultConnection");

// добавляем контекст ApplicationContext в качестве сервиса в приложение
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/users", async (ApplicationContext db) => await db.Users.OrderBy(p => p.Birthday).ToListAsync()); //.Take(5).OrderBy(p => p.DayOfYear) OrderBy(p => p.Birthday)

app.MapGet("/api/users/{id:int}", async (int id, ApplicationContext db) =>
{
    // получаем пользователя по id
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    // если не найден, отправляем статусный код и сообщение об ошибке
    if (user == null) return Results.NotFound(new { message = "Пользователь не найден" });
    
    // если пользователь найден, отправляем его
    return Results.Json(user);
});

app.MapDelete("/api/users/{id:int}", async (int id, ApplicationContext db) =>
{
    // получаем пользователя по id
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    // если не найден, отправляем статусный код и сообщение об ошибке
    if (user == null) return Results.NotFound(new { message = "Пользователь не найден" });

    // если пользователь найден, удаляем его
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.Json(user);
});

app.MapPost("/api/users", async (User user, ApplicationContext db) =>
{
    // добавляем пользователя в массив
    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();
    return user;
});

app.MapPut("/api/users", async (User userData, ApplicationContext db) =>
{
    // получаем пользователя по id
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userData.Id);

    // если не найден, отправляем статусный код и сообщение об ошибке
    if (user == null) return Results.NotFound(new { message = "Пользователь не найден" });

    // если пользователь найден, изменяем его данные и отправляем обратно клиенту
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
    public string Name { get; set; } = ""; // имя пользователя
    [Column(TypeName = "date")]
    public DateTime Birthday { get; set; } // День рождения пользователя
    public bool Type { get; set; } // Важное?
    public string? Photo { get; set; } // Фото пользователя // Convert.FromBase64String (string s);

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
                new User { Id = 1, Name = "Аня", Birthday = Convert.ToDateTime("2005-08-04"), Type=true },
                new User { Id = 2, Name = "Светлана Ивановна", Birthday = Convert.ToDateTime("1974-12-31"), Type = false },
                new User { Id = 3, Name = "дедушка", Birthday = Convert.ToDateTime("1937-01-15"), Type = true }
        );
    }
}