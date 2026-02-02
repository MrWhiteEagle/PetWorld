namespace Domain.Entities;

public class ChatEntry
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserPrompt { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public int IterationCount { get; set; }
    public bool IsApprovedByCritic { get; set; }
}
