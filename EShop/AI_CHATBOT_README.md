# AI-Powered Chatbot Implementation

This implementation replaces the existing rule-based chatbot with Azure OpenAI GPT-4o-mini for intelligent conversations.

## Features

- **Smart Conversations**: Uses Azure OpenAI GPT-4o-mini for natural language understanding
- **Ecommerce Context**: Pre-loaded with product and category information
- **Conversation Memory**: Maintains conversation history per session
- **Order Integration**: Can check order status with authentication
- **Streaming Support**: Supports both regular and streaming responses

## API Endpoints

### 1. Chat Endpoint
```
POST /api/AIChatbot/chat
Content-Type: application/json

{
  "message": "Hello, I'm looking for electronics"
}
```

### 2. Streaming Chat
```
POST /api/AIChatbot/stream
Content-Type: application/json

{
  "message": "Tell me about your products"
}
```

### 3. Order Status Check
```
POST /api/AIChatbot/order-status
Authorization: Bearer <token>
Content-Type: application/json

{
  "orderId": 123
}
```

### 4. Clear History
```
DELETE /api/AIChatbot/clear-history
```

### 5. Test Endpoint
```
POST /api/TestAI/test
Content-Type: application/json

{
  "message": "Hello AI!"
}
```

## Configuration

The Azure OpenAI settings are configured in `appsettings.json`:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://pstestopenaidply-fsdk4djwjny7u.openai.azure.com/",
    "ApiKey": "5fa776418961493280e4d0506f07b6fd",
    "DeploymentName": "pstestopenaidply-fsdk4djwjny7u"
  }
}
```

## Usage Examples

### Basic Chat
```bash
curl -X POST "https://localhost:7000/api/AIChatbot/chat" \
  -H "Content-Type: application/json" \
  -d '{"message": "What products do you have?"}'
```

### Order Status (Authenticated)
```bash
curl -X POST "https://localhost:7000/api/AIChatbot/order-status" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{"orderId": 123}'
```

## Key Improvements Over Previous Chatbot

1. **Natural Language Processing**: Understands context and intent better
2. **Dynamic Responses**: Generates contextual responses instead of predefined rules
3. **Product Awareness**: Automatically includes current product catalog in responses
4. **Conversation Flow**: Maintains context across multiple messages
5. **Scalable**: Easy to extend with new capabilities

## System Prompt Context

The AI assistant is configured with:
- Current product catalog (top 10 products)
- Available categories
- Store policies (shipping, returns, payment methods)
- Support contact information

## Memory Management

- Conversation history is maintained per session
- Limited to last 10 messages to manage memory
- Session identified by user ID (authenticated) or IP address (anonymous)

## Testing

Use the `/api/TestAI/test` endpoint to verify the Azure OpenAI connection is working properly.

## Security

- Order status requires authentication
- API key is stored in configuration (consider using Azure Key Vault for production)
- Conversation history is stored in memory (consider persistent storage for production)