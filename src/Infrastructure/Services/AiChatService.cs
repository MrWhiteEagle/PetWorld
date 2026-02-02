using System.Text.Json;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace Infrastructure.Services;

public class AiChatService : IAiChatService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IChatClient _chatClient;
    private const int MaxIterations = 3;

    public AiChatService(ApplicationDbContext dbContext, IChatClient chatClient)
    {
        _dbContext = dbContext;
        _chatClient = chatClient;
    }

    public async Task<ChatResultDto> ProcessQuestionAsync(string userPrompt)
    {
        var products = await _dbContext.Products.ToListAsync();
        var productContext = BuildProductContext(products);

        string writerResponse = string.Empty;
        bool isApproved = false;
        int iteration = 0;
        string feedback = string.Empty;

        while (!isApproved && iteration < MaxIterations)
        {
            iteration++;

            writerResponse = await GetWriterResponseAsync(userPrompt, productContext, feedback);

            var criticResult = await GetCriticEvaluationAsync(userPrompt, writerResponse);
            isApproved = criticResult.Approved;
            feedback = criticResult.Feedback;
        }

        var chatEntry = new ChatEntry
        {
            CreatedAt = DateTime.UtcNow,
            UserPrompt = userPrompt,
            Response = writerResponse,
            IterationCount = iteration,
            IsApprovedByCritic = isApproved
        };
        _dbContext.ChatEntries.Add(chatEntry);
        await _dbContext.SaveChangesAsync();

        return new ChatResultDto
        {
            Response = writerResponse,
            Iterations = iteration,
            IsApproved = isApproved
        };
    }

    private static string BuildProductContext(List<Product> products)
    {
        var productLines = products.Select(p =>
            $"- {p.Name} | Kategoria: {p.Category} | Cena: {p.Price} PLN | Opis: {p.Description}");
        return string.Join("\n", productLines);
    }

    private async Task<string> GetWriterResponseAsync(string userPrompt, string productContext, string previousFeedback)
    {
        var systemPrompt = $"""
            Jesteś pomocnym asystentem sklepu zoologicznego PetWorld.
            Odpowiadasz WYŁĄCZNIE w języku polskim.
            Pomagasz klientom w wyborze produktów dla ich zwierząt.

            Dostępne produkty w sklepie:
            {productContext}

            Zasady:
            - Odpowiadaj konkretnie i pomocnie
            - Polecaj produkty z powyższej listy, jeśli pasują do pytania
            - Podawaj ceny produktów
            - Bądź uprzejmy i profesjonalny
            """;

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };

        if (!string.IsNullOrEmpty(previousFeedback))
        {
            messages.Add(new ChatMessage(ChatRole.User,
                $"Poprzednia odpowiedź została odrzucona. Uwagi krytyka: {previousFeedback}\n\nPytanie klienta: {userPrompt}"));
        }
        else
        {
            messages.Add(new ChatMessage(ChatRole.User, userPrompt));
        }

        var response = await _chatClient.CompleteAsync(messages);
        return response.Message.Text ?? string.Empty;
    }

    private async Task<CriticResult> GetCriticEvaluationAsync(string userPrompt, string writerResponse)
    {
        var systemPrompt = """
            You are a quality assurance critic for a pet store AI assistant.
            Evaluate the writer's response based on these criteria:

            1. The response MUST be in Polish language
            2. The response should be helpful and relevant to the customer's question
            3. The response should mention specific products with prices when appropriate
            4. The response should be polite and professional
            5. The response should not contain factual errors about products

            Return your evaluation as JSON only, no other text:
            {"approved": true/false, "feedback": "explanation if not approved, or empty string if approved"}
            """;

        var userMessage = $"""
            Customer question: {userPrompt}

            Writer's response:
            {writerResponse}

            Evaluate this response and return JSON.
            """;

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userMessage)
        };

        var response = await _chatClient.CompleteAsync(messages);
        var responseText = response.Message.Text ?? "{}";

        try
        {
            var jsonStart = responseText.IndexOf('{');
            var jsonEnd = responseText.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                responseText = responseText.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            var result = JsonSerializer.Deserialize<CriticResult>(responseText, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result ?? new CriticResult { Approved = true, Feedback = string.Empty };
        }
        catch (JsonException)
        {
            return new CriticResult { Approved = true, Feedback = string.Empty };
        }
    }

    private class CriticResult
    {
        public bool Approved { get; set; }
        public string Feedback { get; set; } = string.Empty;
    }
}
