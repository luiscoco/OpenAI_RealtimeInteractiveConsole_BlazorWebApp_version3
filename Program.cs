using Azure.AI.OpenAI;
using Azure.Identity;
using BlazorApp2.Components;
using OpenAI;
using OpenAI.RealtimeConversation;
using System.ClientModel;

#pragma warning disable OPENAI002
#pragma warning disable
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<RealtimeConversationClient>(serviceProvider => GetConfiguredClient());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static RealtimeConversationClient GetConfiguredClient()
{
    //Configuration for AzureOpenAI
    string? aoaiEndpoint = null;
    string? aoaiUseEntra = null;
    string? aoaiDeployment = null;
    string? aoaiApiKey = null;

    //Configuration for OpenAI
    //string? oaiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    string? oaiApiKey = "OPENAI_API_KEY";


    if (aoaiEndpoint is not null && bool.TryParse(aoaiUseEntra, out bool useEntra) && useEntra)
    {
        return GetConfiguredClientForAzureOpenAIWithEntra(aoaiEndpoint, aoaiDeployment);
    }
    else if (aoaiEndpoint is not null && aoaiApiKey is not null)
    {
        return GetConfiguredClientForAzureOpenAIWithKey(aoaiEndpoint, aoaiDeployment, aoaiApiKey);
    }
    else if (aoaiEndpoint is not null)
    {
        throw new InvalidOperationException(
            $"AZURE_OPENAI_ENDPOINT configured without AZURE_OPENAI_USE_ENTRA=true or AZURE_OPENAI_API_KEY.");
    }
    else if (oaiApiKey is not null)
    {
        return GetConfiguredClientForOpenAIWithKey(oaiApiKey);
    }
    else
    {
        throw new InvalidOperationException(
            $"No environment configuration present. Please provide one of:\n"
                + " - AZURE_OPENAI_ENDPOINT with AZURE_OPENAI_USE_ENTRA=true or AZURE_OPENAI_API_KEY\n"
                + " - OPENAI_API_KEY");
    }
}

static RealtimeConversationClient GetConfiguredClientForAzureOpenAIWithEntra(
    string aoaiEndpoint,
    string? aoaiDeployment)
{
    Console.WriteLine($" * Connecting to Azure OpenAI endpoint (AZURE_OPENAI_ENDPOINT): {aoaiEndpoint}");
    Console.WriteLine($" * Using Entra token-based authentication (AZURE_OPENAI_USE_ENTRA)");
    Console.WriteLine(string.IsNullOrEmpty(aoaiDeployment)
        ? $" * Using no deployment (AZURE_OPENAI_DEPLOYMENT)"
        : $" * Using deployment (AZURE_OPENAI_DEPLOYMENT): {aoaiDeployment}");

    AzureOpenAIClient aoaiClient = new(new Uri(aoaiEndpoint), new DefaultAzureCredential());
    return aoaiClient.GetRealtimeConversationClient(aoaiDeployment);
}
static RealtimeConversationClient GetConfiguredClientForAzureOpenAIWithKey(
    string aoaiEndpoint,
    string? aoaiDeployment,
    string aoaiApiKey)
{
    Console.WriteLine($" * Connecting to Azure OpenAI endpoint (AZURE_OPENAI_ENDPOINT): {aoaiEndpoint}");
    Console.WriteLine($" * Using API key (AZURE_OPENAI_API_KEY): {aoaiApiKey[..5]}**");
    Console.WriteLine(string.IsNullOrEmpty(aoaiDeployment)
        ? $" * Using no deployment (AZURE_OPENAI_DEPLOYMENT)"
        : $" * Using deployment (AZURE_OPENAI_DEPLOYMENT): {aoaiDeployment}");

    AzureOpenAIClient aoaiClient = new(new Uri(aoaiEndpoint), new ApiKeyCredential(aoaiApiKey));
    return aoaiClient.GetRealtimeConversationClient(aoaiDeployment);
}

static RealtimeConversationClient GetConfiguredClientForOpenAIWithKey(string oaiApiKey)
{
    string oaiEndpoint = "https://api.openai.com/v1";
    Console.WriteLine($" * Connecting to OpenAI endpoint (OPENAI_ENDPOINT): {oaiEndpoint}");
    Console.WriteLine($" * Using API key (OPENAI_API_KEY): {oaiApiKey[..5]}**");

    OpenAIClient aoaiClient = new(new ApiKeyCredential(oaiApiKey));
    return aoaiClient.GetRealtimeConversationClient("gpt-4o-realtime-preview-2024-10-01");
}

