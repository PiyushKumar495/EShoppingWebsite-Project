# AI Chatbot Frontend Implementation

## Overview
The frontend now includes an AI-powered chatbot widget that connects to your Azure OpenAI backend service.

## Features

### 🤖 **Smart AI Chatbot Widget**
- **Location**: Fixed position in bottom-right corner
- **Always Available**: Accessible from any page
- **Responsive Design**: Works on desktop and mobile

### 💬 **Conversation Features**
- **Natural Language**: Powered by GPT-4o-mini
- **Context Awareness**: Remembers conversation history
- **Typing Indicators**: Shows when AI is thinking
- **Auto-scroll**: Automatically scrolls to latest messages

### 🛍️ **Ecommerce Integration**
- **Product Search**: Ask about products naturally
- **Order Tracking**: Check order status with order ID
- **Support**: Get help with shipping, returns, payments
- **Authentication**: Secure order status for logged-in users

### 🎨 **User Experience**
- **Clean Interface**: Modern, professional design
- **Loading States**: Visual feedback during API calls
- **Error Handling**: Graceful error messages
- **Clear Chat**: Reset conversation anytime

## Usage Examples

### Basic Conversations
```
User: "Hello"
AI: "Hi! I'm your AI shopping assistant. How can I help you today?"

User: "What products do you have?"
AI: "We have a variety of products including electronics, clothing, books, and more. Here are some popular items..."
```

### Product Search
```
User: "Show me electronics"
AI: "Here are some electronics we have available: [product listings with prices and stock]"

User: "I need a laptop under $1000"
AI: "I can help you find laptops in your budget. Let me show you some options..."
```

### Order Tracking
```
User: "Check my order status for order 123"
AI: "Let me check that for you... Order #123: Status: Shipped, Total: ₹2,500, Placed on: 2024-01-15"
```

## Technical Implementation

### Service Layer (`chatbot.service.ts`)
- **API Integration**: Connects to `/api/AIChatbot` endpoints
- **Authentication**: Handles JWT tokens for order status
- **Error Handling**: Graceful fallbacks for network issues

### Component (`chatbot-widget.component.ts`)
- **State Management**: Tracks conversation and loading states
- **Smart Detection**: Automatically detects order status queries
- **Auto-scroll**: Keeps latest messages visible

### Styling (`chatbot-widget.component.css`)
- **Modern Design**: Gradient backgrounds and smooth animations
- **Responsive**: Adapts to different screen sizes
- **Accessibility**: Proper contrast and focus states

## API Endpoints Used

1. **POST** `/api/AIChatbot/chat` - Main chat endpoint
2. **POST** `/api/AIChatbot/stream` - Streaming responses (future use)
3. **POST** `/api/AIChatbot/order-status` - Order status checking
4. **DELETE** `/api/AIChatbot/clear-history` - Clear conversation

## Configuration

### Proxy Setup (`proxy.conf.json`)
```json
{
  "/api": {
    "target": "http://localhost:5208",
    "secure": false,
    "changeOrigin": true
  }
}
```

### Component Integration (`app.component.html`)
```html
<app-navbar></app-navbar>
<router-outlet />
<app-chatbot-widget></app-chatbot-widget>
```

## Development

### Running the Frontend
```bash
cd frontend
npm install
ng serve
```

### Testing the Chatbot
1. Start the backend API (port 5208)
2. Start the frontend (port 4200)
3. Click the chatbot widget in bottom-right corner
4. Try example queries:
   - "Hello"
   - "What products do you have?"
   - "Check order 123"
   - "What's your return policy?"

## Browser Support
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Mobile Responsiveness
- Optimized for mobile screens
- Touch-friendly interface
- Proper viewport handling

The chatbot widget is now fully integrated and ready to provide intelligent customer support across your entire ecommerce application!