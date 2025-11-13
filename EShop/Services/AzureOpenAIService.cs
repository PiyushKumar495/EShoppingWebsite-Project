using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using EShop.Repositories;
using EShop.Models;

namespace EShop.Services
{
    public class AzureOpenAIService : IAzureOpenAIService
    {
        private readonly ChatClient _chatClient;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Category> _categoryRepository;

        public AzureOpenAIService(IConfiguration configuration, 
            IGenericRepository<Product> productRepository,
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<Order> orderRepository)
        {
            var endpoint = new Uri(configuration["AzureOpenAI:Endpoint"]!);
            var apiKey = configuration["AzureOpenAI:ApiKey"]!;
            var deploymentName = configuration["AzureOpenAI:DeploymentName"]!;

            var azureClient = new AzureOpenAIClient(endpoint, new AzureKeyCredential(apiKey));
            _chatClient = azureClient.GetChatClient(deploymentName);
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<string> GetChatResponseAsync(string userMessage, List<string> conversationHistory)
        {
            var messages = await BuildChatMessages(userMessage, conversationHistory);
            
            var requestOptions = new ChatCompletionOptions()
            {
                MaxOutputTokenCount = 1000,
                Temperature = 0.7f,
                TopP = 1.0f,
            };

            var response = await _chatClient.CompleteChatAsync(messages, requestOptions);
            return response.Value.Content[0].Text;
        }

        public async Task<string> GetStreamingChatResponseAsync(string userMessage, List<string> conversationHistory)
        {
            var messages = await BuildChatMessages(userMessage, conversationHistory);
            
            var response = _chatClient.CompleteChatStreaming(messages);
            var fullResponse = new System.Text.StringBuilder();

            foreach (var update in response)
            {
                foreach (var contentPart in update.ContentUpdate)
                {
                    fullResponse.Append(contentPart.Text);
                }
            }

            return fullResponse.ToString();
        }

        private async Task<List<ChatMessage>> BuildChatMessages(string userMessage, List<string> conversationHistory)
        {
            var messages = new List<ChatMessage>();
            
            // System message with ecommerce context
            var systemPrompt = await BuildSystemPrompt();
            messages.Add(new SystemChatMessage(systemPrompt));

            // Add conversation history
            for (int i = 0; i < conversationHistory.Count; i++)
            {
                if (i % 2 == 0)
                    messages.Add(new UserChatMessage(conversationHistory[i]));
                else
                    messages.Add(new AssistantChatMessage(conversationHistory[i]));
            }

            // Add current user message
            messages.Add(new UserChatMessage(userMessage));

            return messages;
        }

        private async Task<string> BuildSystemPrompt()
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();

            var productList = products.Take(10).Select(p => 
                $"- {p.Name}: Rs{p.Price} (Stock: {p.StockQuantity})").ToList();
            
            var categoryList = categories.Select(c => c.CategoryName).ToList();

            return $@"You are an AI assistant for EShop ecommerce website. You support both CUSTOMERS and ADMINS.

## CUSTOMER OPERATIONS:
1. **Product Search**: ""Show me electronics"", ""Find laptops under Rs50000""
2. **Order Tracking**: ""Check order 22"", ""Order status #22"" - REQUIRES LOGIN
3. **Shopping Help**: Product recommendations, comparisons
4. **Account Support**: Login help, password reset guidance
5. **General Queries**: Shipping, returns, payments

## ADMIN OPERATIONS (REQUIRES ADMIN LOGIN):
1. **Product Management**:
   - ""Add product [name] price [amount] stock [qty] category [name]""
   - ""Update product [id] price [amount]""
   - ""Delete product [id]""
   - ""Show low stock products""

2. **Order Management**:
   - ""Show order [id]""
   - ""Update order [id] status [status]""
   - ""Show pending orders""
   - ""Order statistics""

3. **Category Management**:
   - ""Add category [name]""
   - ""Update category [id] name [new_name]""
   - ""Delete category [id]""
   - ""List categories""

4. **Analytics & Reports**:
   - ""Sales report""
   - ""Revenue statistics""
   - ""User statistics""
   - ""Inventory report""

## CURRENT DATA:
**Categories**: {string.Join(", ", categoryList)}

**Sample Products**:
{string.Join("\n", productList)}

## IMPORTANT SECURITY RULES:
- NEVER provide order details without proper authentication
- For order status queries, direct users to login first
- Admin operations require admin authentication
- Do not access or display sensitive order information

## RESPONSE GUIDELINES:
- For order status requests, tell users they need to login first
- For admin operations, tell users they need admin access
- For customer queries, be helpful and friendly
- Always provide accurate product data from the current information above
- Guide users to appropriate authentication when needed

**Store Policies**: COD/UPI payments, Free shipping >Rs500, 30-day returns
**Support**: support@eshop.com or 1800-123-456";
        }
    }
}