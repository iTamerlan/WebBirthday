// ��������� ������
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;



var builder = WebApplication.CreateBuilder();

// �������� ������ ����������� �� ����� ������������
string connection = builder.Configuration.GetConnectionString("DefaultConnection");

// ��������� �������� ApplicationContext � �������� ������� � ����������
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/users", async (ApplicationContext db) => await db.Users.OrderBy(p => p.Birthday).ToListAsync()); //.Take(5).OrderBy(p => p.DayOfYear) OrderBy(p => p.Birthday)

app.MapGet("/api/users/{id:int}", async (int id, ApplicationContext db) =>
{
    // �������� ������������ �� id
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    if (user == null) return Results.NotFound(new { message = "������������ �� ������" });
    
    // ���� ������������ ������, ���������� ���
    return Results.Json(user);
});

app.MapDelete("/api/users/{id:int}", async (int id, ApplicationContext db) =>
{
    // �������� ������������ �� id
    User? user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    if (user == null) return Results.NotFound(new { message = "������������ �� ������" });

    // ���� ������������ ������, ������� ���
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.Json(user);
});

app.MapPost("/api/users", async (User user, ApplicationContext db) =>
{
    // ��������� ������������ � ������
    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();
    return user;
});

app.MapPut("/api/users", async (User userData, ApplicationContext db) =>
{
    // �������� ������������ �� id
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userData.Id);

    // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
    if (user == null) return Results.NotFound(new { message = "������������ �� ������" });

    // ���� ������������ ������, �������� ��� ������ � ���������� ������� �������
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
    public string Name { get; set; } = ""; // ��� ������������
    [Column(TypeName = "date")]
    public DateTime Birthday { get; set; } // ���� �������� ������������
    public bool Type { get; set; } // ������?
    public string? Photo { get; set; } // ���� ������������ // Convert.FromBase64String (string s);

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
        Database.EnsureCreated();   // ������� ���� ������ ��� ������ ���������
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "���", Birthday = Convert.ToDateTime("2005-08-04"), Type=true },
                new User { Id = 2, Name = "�������� ��������", Birthday = Convert.ToDateTime("1974-12-31"), Type = false },
                new User { Id = 3, Name = "�������", Birthday = Convert.ToDateTime("1937-01-15"), Type = true }
        );
    }
}