namespace Prode.Domain.Entities;

// Liga de amigos. La baja es lógica.
public class League
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;

    public int OwnerUserId { get; set; }
    public User OwnerUser { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? DeactivatedAtUtc { get; set; }

    public ICollection<LeagueTournament> Tournaments { get; set; } = new List<LeagueTournament>();
    public ICollection<LeagueMember> Members { get; set; } = new List<LeagueMember>();
}
