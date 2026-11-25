namespace Prode.Domain.Entities;

// Usuario de la aplicación. La baja es lógica: no se borran sus datos.
public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    // Null si el usuario solo entra con Google.
    public string? PasswordHash { get; set; }

    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<LeagueMember> Memberships { get; set; } = new List<LeagueMember>();
}
