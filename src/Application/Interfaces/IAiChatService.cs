using Application.DTOs;

namespace Application.Interfaces;

public interface IAiChatService
{
    Task<ChatResultDto> ProcessQuestionAsync(string userPrompt);
}
