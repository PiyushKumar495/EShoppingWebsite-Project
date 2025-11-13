using Microsoft.AspNetCore.Mvc;
using EShop.Services;
using EShop.Repositories;
using EShop.Models;
using System.Security.Claims;

namespace EShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public partial class AIChatbotController : ControllerBase
    {
        private readonly IAzureOpenAIService _openAIService;
        private readonly IChatbotOperationsService _operationsService;

        private static readonly Dictionary<string, List<string>> _conversationHistory = new();

        public AIChatbotController(
            IAzureOpenAIService openAIService, 
            IChatbotOperationsService operationsService)
        {
            _openAIService = openAIService;
            _operationsService = operationsService;
        }

        [HttpPost("chat")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (request?.Message == null)
                throw new ArgumentNullException(nameof(request));
            
            if (string.IsNullOrEmpty(request.Message))
                throw new ArgumentNullException(nameof(request));

            var sessionKey = GetSessionKey();
            
            if (!_conversationHistory.TryGetValue(sessionKey, out var history))
            {
                history = new List<string>();
                _conversationHistory[sessionKey] = history;
            }
            history.Add(request.Message);

            // Check if this is an operational command
            var operationResult = await TryExecuteOperation(request.Message);
            if (operationResult != null)
            {
                history.Add(operationResult);
                if (history.Count > 10) history.RemoveRange(0, history.Count - 10);
                return Ok(new { reply = operationResult });
            }

            // Get AI response for general queries
            var response = await _openAIService.GetChatResponseAsync(request.Message, history);
            history.Add(response);
            
            if (history.Count > 10) history.RemoveRange(0, history.Count - 10);
            return Ok(new { reply = response });
        }

        private async Task<string?> TryExecuteOperation(string message)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = int.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : 0;
            var isAuthenticated = User.Identity?.IsAuthenticated == true;

            // Quick check for order status queries first
            if (IsOrderStatusQuery(message))
            {
                if (!isAuthenticated)
                    return "❌ Please login to check your order status.";
                return await _operationsService.ExecuteCustomerOperationAsync(message, userId);
            }

            // Check if it's an admin operation using simple keyword detection
            if (IsAdminOperation(message))
            {
                return await _operationsService.ExecuteAdminOperationAsync(message, userRole ?? string.Empty);
            }

            // Check if it's a customer operation using simple keyword detection
            if (IsCustomerOperation(message))
            {
                // Check for operations that need authentication
                if (RequiresAuthentication(message))
                {
                    if (!isAuthenticated)
                        return "❌ Please login to place an order.";
                    return await _operationsService.ExecuteCustomerOperationAsync(message, userId);
                }
                else
                {
                    return await _operationsService.ExecuteCustomerOperationAsync(message, isAuthenticated ? userId : null);
                }
            }

            return null;
        }

        private const string OrderKeyword = "order";
        
        [System.Text.RegularExpressions.GeneratedRegex(@"\b\d+\b")]
        private static partial System.Text.RegularExpressions.Regex NumberPattern();
        
        private static bool IsOrderStatusQuery(string message)
        {
            var lowerMessage = message.ToLower();
            return (lowerMessage.Contains(OrderKeyword) && lowerMessage.Contains("status")) ||
                   (lowerMessage.Contains(OrderKeyword) && lowerMessage.Contains("check")) ||
                   (lowerMessage.Contains("status") && NumberPattern().IsMatch(message));
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"^[^,]+,\d+,\d+,[^,]+$")]
        private static partial System.Text.RegularExpressions.Regex ProductAddPattern();
        
        [System.Text.RegularExpressions.GeneratedRegex(@"^\d+,\d+$")]
        private static partial System.Text.RegularExpressions.Regex UpdatePattern();
        
        [System.Text.RegularExpressions.GeneratedRegex(@"^[^,]+,[^,]+$")]
        private static partial System.Text.RegularExpressions.Regex CategoryPattern();
        
        private static bool IsAdminOperation(string message)
        {
            var lowerMessage = message.ToLower();
            return lowerMessage.Contains("add product") ||
                   lowerMessage.Contains("update product") ||
                   lowerMessage.Contains("delete product") ||
                   lowerMessage.Contains("add category") ||
                   lowerMessage.Contains("update category") ||
                   lowerMessage.Contains("delete category") ||
                   lowerMessage.Contains("low stock") ||
                   lowerMessage.Contains("pending orders") ||
                   lowerMessage.Contains(OrderKeyword + " statistics") ||
                   lowerMessage.Contains("sales report") ||
                   lowerMessage.Contains("user statistics") ||
                   lowerMessage.Contains("inventory report") ||
                   lowerMessage.Contains("update " + OrderKeyword) ||
                   lowerMessage.Contains("list categories") ||
                   (lowerMessage.Contains("delete") && !lowerMessage.Contains(OrderKeyword)) ||
                   ProductAddPattern().IsMatch(message) ||
                   UpdatePattern().IsMatch(message) ||
                   CategoryPattern().IsMatch(message);
        }

        private static bool IsCustomerOperation(string message)
        {
            var lowerMessage = message.ToLower();
            return lowerMessage.Contains("show products") ||
                   (lowerMessage.Contains("find") && !lowerMessage.Contains("add") && !lowerMessage.Contains("update") && !lowerMessage.Contains("delete")) ||
                   (lowerMessage.Contains("search") && !lowerMessage.Contains("add") && !lowerMessage.Contains("update") && !lowerMessage.Contains("delete")) ||
                   lowerMessage.Contains("add") && lowerMessage.Contains("cart") ||
                   lowerMessage.Contains("view cart") ||
                   lowerMessage.Contains("show cart") ||
                   lowerMessage.Contains("my cart") ||
                   lowerMessage.Contains("cancel " + OrderKeyword) ||
                   lowerMessage.Contains(OrderKeyword) && (lowerMessage.Contains("address") || lowerMessage.Contains("payment")) ||
                   lowerMessage.Contains("checkout") ||
                   lowerMessage.Contains("place " + OrderKeyword);
        }

        private static bool RequiresAuthentication(string message)
        {
            var lowerMessage = message.ToLower();
            return lowerMessage.Contains("checkout") ||
                   lowerMessage.Contains("place " + OrderKeyword) ||
                   (lowerMessage.Contains(OrderKeyword) && (lowerMessage.Contains("address") || lowerMessage.Contains("payment"))) ||
                   lowerMessage.Contains("cancel " + OrderKeyword);
        }



        [HttpPost("stream")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> StreamChat([FromBody] ChatRequest request)
        {
            var sessionKey = GetSessionKey();
            
            if (!_conversationHistory.TryGetValue(sessionKey, out var history))
            {
                history = new List<string>();
                _conversationHistory[sessionKey] = history;
            }
            history.Add(request.Message);

            var response = await _openAIService.GetStreamingChatResponseAsync(request.Message, history);
            
            history.Add(response);
            
            if (history.Count > 10)
            {
                history.RemoveRange(0, history.Count - 10);
            }

            return Ok(new { reply = response });
        }



        [HttpDelete("clear-history")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult ClearHistory()
        {
            var sessionKey = GetSessionKey();
            if (_conversationHistory.TryGetValue(sessionKey, out var history))
                history.Clear();
            
            return Ok(new { message = "Conversation history cleared." });
        }

        [HttpGet("debug-auth")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult DebugAuth()
        {
            return Ok(new { 
                isAuthenticated = User.Identity?.IsAuthenticated,
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                userRole = User.FindFirst(ClaimTypes.Role)?.Value,
                userName = User.FindFirst(ClaimTypes.Name)?.Value
            });
        }

        private string GetSessionKey()
        {
            return User.Identity?.IsAuthenticated == true
                ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anon"
                : HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "anon";
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }


}