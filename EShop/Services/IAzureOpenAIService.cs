namespace EShop.Services
{
    public interface IAzureOpenAIService
    {
        Task<string> GetChatResponseAsync(string userMessage, List<string> conversationHistory);
        Task<string> GetStreamingChatResponseAsync(string userMessage, List<string> conversationHistory);
    }
}