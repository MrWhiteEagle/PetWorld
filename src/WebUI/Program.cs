using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI;
using WebUI.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Configure AI services
var openAiApiKey = builder.Configuration["OpenAI:ApiKey"] ??
    Environment.GetEnvironmentVariable("OPENAI_API_KEY");

var openAiModel = builder.Configuration["OpenAI:Model"] ?? "gpt-4o-mini";

if (!string.IsNullOrEmpty(openAiApiKey))
{
    builder.Services.AddSingleton<IChatClient>(sp =>
    {
        var openAiClient = new OpenAIClient(openAiApiKey);
        return openAiClient.GetChatClient(openAiModel).AsChatClient();
    });
    builder.Services.AddScoped<IAiChatService, AiChatService>();
}
else
{
    Console.WriteLine("OpenAI API key not found. Using mock AI chat service.");
    builder.Services.AddScoped<IAiChatService, MockAiChatService>();
}

//QuickGridAdapter
builder.Services.AddQuickGridEntityFrameworkAdapter();

var app = builder.Build();

// Apply pending migrations automatically at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
