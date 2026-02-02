using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using System.Linq;

namespace Infrastructure.Services;

public class MockAiChatService(ApplicationDbContext context) : IAiChatService
{
    private readonly ApplicationDbContext _context = context;
    public async Task<ChatResultDto> ProcessQuestionAsync(string userPrompt)
    {
        //Delay simulation
        await Task.Delay(500);
        var product = _context.Products.OrderBy(p => Guid.NewGuid()).FirstOrDefault();
        string finalAnswer = $"Dzień dobry! W odpowiedzi na pytanie o '{userPrompt}', " +
                               $"szczególnie polecam produkt: {product?.Name}. " +
                               $"Jest to idealny wybór w kategorii {product?.Category}.";

        // Save chat entry to database for history tracking
        var chatEntry = new ChatEntry
        {
            CreatedAt = DateTime.UtcNow,
            UserPrompt = userPrompt,
            Response = finalAnswer,
            IterationCount = 1,
            IsApprovedByCritic = true
        };
        _context.ChatEntries.Add(chatEntry);
        await _context.SaveChangesAsync();

        return new ChatResultDto
        {
            Response = finalAnswer,
            IsApproved = true,
            Iterations = 1,
        };
    }
}
