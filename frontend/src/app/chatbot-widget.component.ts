import { Component } from '@angular/core';
import { ChatbotService } from './chatbot.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-chatbot-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot-widget.component.html',
  styleUrls: ['./chatbot-widget.component.css']
})
export class ChatbotWidgetComponent {
  isOpen = false;
  userInput = '';
  isLoading = false;
  messages = [
    { sender: 'bot', text: '🤖 Hi! I\'m your AI shopping assistant. I can help you with products, orders, and any questions you have!' }
  ];

  constructor(private chatbotService: ChatbotService) {}

  onKeyPress(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      if (this.userInput.trim() === '/debug') {
        this.userInput = '';
        this.debugAuth();
        return;
      }
      this.sendMessage();
    }
  }

  toggleOpen() { 
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      setTimeout(() => this.scrollToBottom(), 100);
    }
  }

  sendMessage() {
    if (!this.userInput.trim() || this.isLoading) return;
    
    const userMsg = { sender: 'user', text: this.userInput };
    this.messages.push(userMsg);
    
    const message = this.userInput;
    this.userInput = '';
    this.isLoading = true;
    
    // Add typing indicator
    this.messages.push({ sender: 'bot', text: '🤖 Thinking...' });
    this.scrollToBottom();
    
    this.chatbotService.getBotReply(message).subscribe({
      next: (reply) => {
        this.messages.pop(); // Remove typing indicator
        this.messages.push({ sender: 'bot', text: reply });
        this.isLoading = false;
        this.scrollToBottom();
      },
      error: () => {
        this.messages.pop(); // Remove typing indicator
        this.messages.push({ sender: 'bot', text: 'Sorry, I\'m having trouble right now. Please try again.' });
        this.isLoading = false;
        this.scrollToBottom();
      }
    });
  }



  clearChat() {
    this.messages = [
      { sender: 'bot', text: '🤖 Hi! I\'m your AI shopping assistant. I can help you with products, orders, and any questions you have!' }
    ];
    this.chatbotService.clearHistory().subscribe();
  }

  debugAuth() {
    this.chatbotService.debugAuth().subscribe({
      next: (result) => {
        this.messages.push({ 
          sender: 'bot', 
          text: `🔍 Debug Info:\n• Authenticated: ${result.isAuthenticated}\n• User ID: ${result.userId || 'None'}\n• Role: ${result.userRole || 'None'}\n• Name: ${result.userName || 'None'}\n• Token in localStorage: ${!!localStorage.getItem('eshop_token')}` 
        });
        this.scrollToBottom();
      },
      error: () => {
        this.messages.push({ sender: 'bot', text: '❌ Debug failed - check console' });
        this.scrollToBottom();
      }
    });
  }

  private scrollToBottom() {
    setTimeout(() => {
      const chatBody = document.querySelector('.chatbot-body');
      if (chatBody) {
        chatBody.scrollTop = chatBody.scrollHeight;
      }
    }, 50);
  }
}
