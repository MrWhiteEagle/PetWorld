namespace Application.DTOs;

public class ChatResultDto
{
    public string Response { get; set; } = string.Empty;
    public int Iterations { get; set; }
    public bool IsApproved { get; set; }
}
