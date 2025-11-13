using EShop.Models;
using EShop.Repositories;
using System.Text.RegularExpressions;

namespace EShop.Services
{
    public partial class ChatbotOperationsService : IChatbotOperationsService
    {
        private const string OnlinePayment = "ONLINE";
        private static readonly char[] SplitSeparators = { ' ', ',' };
        private static readonly char[] CommaSeparator = { ',' };
        private readonly RepositoryService _repositories;

        public ChatbotOperationsService(RepositoryService repositories)
        {
            _repositories = repositories;
        }

        public async Task<string> ExecuteAdminOperationAsync(string operation, string userRole)
        {
            if (userRole != "Admin") return "❌ Admin access required for this operation.";
            return await RouteAdminOperation(operation);
        }

        private async Task<string> RouteAdminOperation(string operation)
        {
            var lowerOp = operation.ToLower();

            // Product Management
            var productResult = await TryHandleProductOperations(lowerOp, operation);
            if (productResult != null) return productResult;

            // Category Management
            var categoryResult = await TryHandleCategoryOperations(lowerOp, operation);
            if (categoryResult != null) return categoryResult;

            // Order Management & Analytics
            var orderResult = await TryHandleOrderOperations(lowerOp, operation);
            if (orderResult != null) return orderResult;

            return "ℹ️ Available admin operations: add/update/delete products, manage categories, order management, sales reports, user statistics.";
        }

        private async Task<string?> TryHandleProductOperations(string lowerOp, string operation)
        {
            if (lowerOp.Contains("add product", StringComparison.OrdinalIgnoreCase) || IsCommaDelimitedProductAdd(operation)) return await HandleAddProduct(operation);
            if (lowerOp.Contains("update product", StringComparison.OrdinalIgnoreCase) || IsCommaDelimitedUpdate(operation)) return await HandleUpdateProduct(operation);
            if (lowerOp.Contains("delete product", StringComparison.OrdinalIgnoreCase) || (lowerOp.StartsWith("delete ", StringComparison.OrdinalIgnoreCase) && !lowerOp.Contains("category", StringComparison.OrdinalIgnoreCase))) return await HandleDeleteProduct(operation);
            if (lowerOp.Contains("low stock", StringComparison.OrdinalIgnoreCase)) return await HandleLowStock();
            return null;
        }

        private async Task<string?> TryHandleCategoryOperations(string lowerOp, string operation)
        {
            if (lowerOp.Contains("add category", StringComparison.OrdinalIgnoreCase)) return await HandleAddCategory(operation);
            if (lowerOp.Contains("update category", StringComparison.OrdinalIgnoreCase) || IsCommaDelimitedCategoryUpdate(operation)) return await HandleUpdateCategory(operation);
            if (lowerOp.Contains("delete category", StringComparison.OrdinalIgnoreCase)) return await HandleDeleteCategory(operation);
            if (lowerOp.Contains("list categories", StringComparison.OrdinalIgnoreCase)) return await HandleListCategories();
            return null;
        }

        private async Task<string?> TryHandleOrderOperations(string lowerOp, string operation)
        {
            if (lowerOp.Contains("pending orders", StringComparison.OrdinalIgnoreCase)) return await HandlePendingOrders();
            if (lowerOp.Contains("order statistics", StringComparison.OrdinalIgnoreCase)) return await HandleOrderStatistics();
            if (lowerOp.Contains("update order", StringComparison.OrdinalIgnoreCase)) return await HandleUpdateOrder(operation);
            if (lowerOp.Contains("sales report", StringComparison.OrdinalIgnoreCase) || lowerOp.Contains("revenue", StringComparison.OrdinalIgnoreCase)) return await HandleSalesReport();
            if (lowerOp.Contains("user statistics", StringComparison.OrdinalIgnoreCase)) return await HandleUserStatistics();
            if (lowerOp.Contains("inventory report", StringComparison.OrdinalIgnoreCase)) return await HandleInventoryReport();
            return null;
        }

        public async Task<string> ExecuteCustomerOperationAsync(string operation, int? userId = null)
        {
            return await RouteCustomerOperation(operation, userId);
        }

        private async Task<string> RouteCustomerOperation(string operation, int? userId)
        {
            var lowerOp = operation.ToLower();

            // Product search operations
            if (IsProductSearchOperation(lowerOp)) return await HandleProductSearch(operation);

            // Cart operations
            if (IsAddToCartOperation(lowerOp)) return await HandleFlexibleAddToCart(operation, userId);
            if (IsViewCartOperation(lowerOp)) return await HandleViewCart(userId);
            if (IsClearCartOperation(lowerOp)) return await HandleClearCart(userId);
            if (IsCheckoutOperation(lowerOp)) return await HandleFlexibleCheckout(operation, userId);

            // Order operations
            if (IsOrderStatusOperation(lowerOp)) return await HandleFlexibleOrderStatus(operation, userId);
            if (IsCancelOrderOperation(lowerOp)) return await HandleFlexibleOrderCancellation(operation, userId);
            if (IsPlaceOrderOperation(lowerOp)) return await HandleFlexiblePlaceOrder(operation, userId);

            return "ℹ️ I can help you search products, check order status, cancel orders, add/view cart items, place orders, or answer questions about our store.";
        }

        private static bool IsProductSearchOperation(string lowerOp) =>
            lowerOp.Contains("search", StringComparison.OrdinalIgnoreCase) || 
            lowerOp.Contains("find", StringComparison.OrdinalIgnoreCase) || 
            lowerOp.Contains("show", StringComparison.OrdinalIgnoreCase);

        private static bool IsAddToCartOperation(string lowerOp) =>
            lowerOp.Contains("add to cart", StringComparison.OrdinalIgnoreCase) || 
            (lowerOp.Contains("add", StringComparison.OrdinalIgnoreCase) && lowerOp.Contains("cart", StringComparison.OrdinalIgnoreCase));

        private static bool IsViewCartOperation(string lowerOp) =>
            lowerOp.Contains("view cart", StringComparison.OrdinalIgnoreCase) || 
            lowerOp.Contains("show cart", StringComparison.OrdinalIgnoreCase) || 
            lowerOp.Contains("my cart", StringComparison.OrdinalIgnoreCase);

        private static bool IsClearCartOperation(string lowerOp) =>
            lowerOp.Contains("clear cart", StringComparison.OrdinalIgnoreCase) || 
            lowerOp.Contains("empty cart", StringComparison.OrdinalIgnoreCase);

        private static bool IsCheckoutOperation(string lowerOp) =>
            lowerOp.Contains("checkout", StringComparison.OrdinalIgnoreCase) || (lowerOp.Contains("order", StringComparison.OrdinalIgnoreCase) && lowerOp.Contains("cart", StringComparison.OrdinalIgnoreCase));

        private static bool IsOrderStatusOperation(string lowerOp) =>
            lowerOp.Contains("order status", StringComparison.OrdinalIgnoreCase) || (lowerOp.Contains("check", StringComparison.OrdinalIgnoreCase) && lowerOp.Contains("order", StringComparison.OrdinalIgnoreCase));

        private static bool IsCancelOrderOperation(string lowerOp) =>
            lowerOp.Contains("cancel order", StringComparison.OrdinalIgnoreCase);

        private static bool IsPlaceOrderOperation(string lowerOp) =>
            lowerOp.Contains("order", StringComparison.OrdinalIgnoreCase) && (lowerOp.Contains("address", StringComparison.OrdinalIgnoreCase) || lowerOp.Contains("payment", StringComparison.OrdinalIgnoreCase));

        [GeneratedRegex(@"add product (.+?) price (\d+) stock (\d+) category (.+)", RegexOptions.IgnoreCase)]
        private static partial Regex ProductAddRegex();
        
        [GeneratedRegex(@"update product (\d+) price (\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex ProductPriceUpdateRegex();
        
        [GeneratedRegex(@"update product (\d+) stock (\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex ProductStockUpdateRegex();
        
        [GeneratedRegex(@"delete (?:product )?(\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex DeleteByIdRegex();
        
        [GeneratedRegex(@"delete (?:product )?(.+)", RegexOptions.IgnoreCase)]
        private static partial Regex DeleteByNameRegex();
        
        [GeneratedRegex(@"add category (.+)", RegexOptions.IgnoreCase)]
        private static partial Regex AddCategoryRegex();
        
        [GeneratedRegex(@"update category (\d+) name (.+)", RegexOptions.IgnoreCase)]
        private static partial Regex UpdateCategoryRegex();
        
        [GeneratedRegex(@"delete category (\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex DeleteCategoryByIdRegex();
        
        [GeneratedRegex(@"delete category (.+)", RegexOptions.IgnoreCase)]
        private static partial Regex DeleteCategoryByNameRegex();
        
        [GeneratedRegex(@"update order (\d+) status (\w+)", RegexOptions.IgnoreCase)]
        private static partial Regex UpdateOrderRegex();
        
        [GeneratedRegex(@"(\d+)")]
        private static partial Regex NumberRegex();
        
        [GeneratedRegex(@"cancel order (\d+)", RegexOptions.IgnoreCase)]
        private static partial Regex CancelOrderRegex();
        
        [GeneratedRegex(@"add (?:(\d+) )?(.+?) to cart", RegexOptions.IgnoreCase)]
        private static partial Regex AddToCartRegex();
        
        [GeneratedRegex(@"add (.+?) cart", RegexOptions.IgnoreCase)]
        private static partial Regex AddToCartAltRegex();
        
        [GeneratedRegex(@"order\s+(\d+)\s+(.+?)\s*,\s*address\s+(.+?)\s*,\s*payment\s+mode\s+(\w+)", RegexOptions.IgnoreCase)]
        private static partial Regex PlaceOrderRegex();
        
        [GeneratedRegex(@"(?:order cart|checkout)\s*,\s*address\s+(.+?)\s*,\s*payment\s+mode\s+(\w+)", RegexOptions.IgnoreCase)]
        private static partial Regex CheckoutRegex();
        
        [GeneratedRegex(@"multi order\s+(.+?)\s*,\s*address\s+(.+?)\s*,\s*payment\s+mode\s+(\w+)", RegexOptions.IgnoreCase)]
        private static partial Regex MultiOrderRegex();
        
        [GeneratedRegex(@"(\d+)\s+(.+)")]
        private static partial Regex ProductQuantityRegex();
        
        [GeneratedRegex(@"address\s+([^,]+)", RegexOptions.IgnoreCase)]
        private static partial Regex AddressRegex();

        // Helper methods to detect comma-delimited formats
        private static bool IsCommaDelimitedProductAdd(string operation)
        {
            var parts = operation.Split(',');
            return parts.Length >= 4 && decimal.TryParse(parts[1].Trim(), out _) && int.TryParse(parts[2].Trim(), out _);
        }
        
        private static bool IsCommaDelimitedUpdate(string operation)
        {
            var parts = operation.Split(',');
            return parts.Length == 2 && decimal.TryParse(parts[1].Trim(), out _) && !operation.ToLower().Contains("update", StringComparison.OrdinalIgnoreCase);
        }
        
        private static bool IsCommaDelimitedCategoryUpdate(string operation)
        {
            var parts = operation.Split(',');
            return parts.Length == 2 && !decimal.TryParse(parts[1].Trim(), out _) && !operation.ToLower().Contains("update", StringComparison.OrdinalIgnoreCase);
        }

        // Admin Operations
        private async Task<string> HandleAddProduct(string operation)
        {
            string name, categoryName;
            decimal price;
            int stock;

            // Try comma-delimited format first: "Iphone,60000,100,Electronic"
            if (IsCommaDelimitedProductAdd(operation))
            {
                var parts = operation.Split(',');
                name = parts[0].Trim();
                price = decimal.Parse(parts[1].Trim());
                stock = int.Parse(parts[2].Trim());
                categoryName = parts[3].Trim();
            }
            else
            {
                // Try standard format: "add product iPhone price 60000 stock 100 category Electronic"
                var match = ProductAddRegex().Match(operation);
                if (!match.Success) return "❌ Format: 'add product [name] price [amount] stock [qty] category [name]' or '[name],[price],[stock],[category]'";

                name = match.Groups[1].Value.Trim();
                price = decimal.Parse(match.Groups[2].Value);
                stock = int.Parse(match.Groups[3].Value);
                categoryName = match.Groups[4].Value.Trim();
            }

            var categories = await _repositories.CategoryRepository.GetAllAsync();
            var category = categories.FirstOrDefault(c => c.CategoryName?.Equals(categoryName, StringComparison.OrdinalIgnoreCase) == true);
            
            if (category == null) return $"❌ Category '{categoryName}' not found. Available: {string.Join(", ", categories.Select(c => c.CategoryName))}";

            var product = new Product
            {
                Name = name,
                Description = $"New product: {name}",
                Price = price,
                StockQuantity = stock,
                CategoryId = category.CategoryId
            };

            await _repositories.ProductRepository.AddAsync(product);
            return $"✅ Product '{name}' added successfully! ID: {product.ProductId}";
        }

        private async Task<string> HandleUpdateProduct(string operation)
        {
            if (operation.Contains(',')) return await HandleCommaDelimitedUpdate(operation);
            
            var priceMatch = ProductPriceUpdateRegex().Match(operation);
            if (priceMatch.Success) return await UpdateProductPrice(priceMatch);
            
            var stockMatch = ProductStockUpdateRegex().Match(operation);
            if (stockMatch.Success) return await UpdateProductStock(stockMatch);
            
            return "❌ Format: 'update product [id] price [amount]', 'update product [id] stock [qty]', or '[id/name],[newPrice]'";
        }

        private async Task<string> HandleCommaDelimitedUpdate(string operation)
        {
            var parts = operation.Split(',');
            if (parts.Length < 2) return "❌ Invalid format.";

            var identifier = parts[0].Trim();
            var newPrice = decimal.Parse(parts[1].Trim());
            
            var product = int.TryParse(identifier, out int id)
                ? await _repositories.ProductRepository.GetByIdAsync(id)
                : (await _repositories.ProductRepository.GetAllAsync()).FirstOrDefault(p => p.Name?.Equals(identifier, StringComparison.OrdinalIgnoreCase) == true);
            
            if (product == null) return $"❌ Product '{identifier}' not found.";
            
            product.Price = newPrice;
            await _repositories.ProductRepository.UpdateAsync(product);
            return $"✅ Product '{product.Name}' price updated to ₹{newPrice}";
        }

        private async Task<string> UpdateProductPrice(Match priceMatch)
        {
            var id = int.Parse(priceMatch.Groups[1].Value);
            var price = decimal.Parse(priceMatch.Groups[2].Value);
            var product = await _repositories.ProductRepository.GetByIdAsync(id);
            if (product == null) return $"❌ Product with ID {id} not found.";
            
            product.Price = price;
            await _repositories.ProductRepository.UpdateAsync(product);
            return $"✅ Product '{product.Name}' price updated to ₹{price}";
        }

        private async Task<string> UpdateProductStock(Match stockMatch)
        {
            var id = int.Parse(stockMatch.Groups[1].Value);
            var stock = int.Parse(stockMatch.Groups[2].Value);
            var product = await _repositories.ProductRepository.GetByIdAsync(id);
            if (product == null) return $"❌ Product with ID {id} not found.";
            
            product.StockQuantity = stock;
            await _repositories.ProductRepository.UpdateAsync(product);
            return $"✅ Product '{product.Name}' stock updated to {stock}";
        }

        private async Task<string> HandleDeleteProduct(string operation)
        {
            // Try ID format first
            var idMatch = DeleteByIdRegex().Match(operation);
            if (idMatch.Success)
            {
                return await DeleteProductById(int.Parse(idMatch.Groups[1].Value));
            }

            // Try name format
            var nameMatch = DeleteByNameRegex().Match(operation);
            if (nameMatch.Success)
            {
                return await DeleteProductByName(nameMatch.Groups[1].Value.Trim().Trim('\'', '"'));
            }

            return "❌ Format: 'delete [id]' or 'delete [name]'";
        }

        private async Task<string> DeleteProductById(int id)
        {
            var product = await _repositories.ProductRepository.GetByIdAsync(id);
            if (product == null) return $"❌ Product with ID {id} not found.";

            await _repositories.ProductRepository.DeleteAsync(id);
            return $"✅ Product '{product.Name}' deleted successfully!";
        }

        private async Task<string> DeleteProductByName(string name)
        {
            var products = await _repositories.ProductRepository.GetAllAsync();
            var product = products.FirstOrDefault(p => 
                p.Name?.Trim().Trim('\'', '"').Equals(name, StringComparison.OrdinalIgnoreCase) == true);
            
            if (product == null) return $"❌ Product '{name}' not found.";

            await _repositories.ProductRepository.DeleteAsync(product.ProductId);
            return $"✅ Product '{product.Name}' deleted successfully!";
        }

        private async Task<string> HandleLowStock()
        {
            var products = await _repositories.ProductRepository.GetAllAsync();
            var lowStock = products.Where(p => p.StockQuantity < 10).ToList();
            
            if (lowStock.Count == 0) return "✅ All products have sufficient stock!";
            
            return "⚠️ Low Stock Products:\n" + string.Join("\n", lowStock.Select(p => 
                $"• {p.Name} (ID: {p.ProductId}): {p.StockQuantity} left"));
        }

        private async Task<string> HandleAddCategory(string operation)
        {
            var match = AddCategoryRegex().Match(operation);
            if (!match.Success) return "❌ Format: 'add category [name]'";

            var name = match.Groups[1].Value.Trim();
            var categories = await _repositories.CategoryRepository.GetAllAsync();
            
            if (categories.Any(c => c.CategoryName?.Equals(name, StringComparison.OrdinalIgnoreCase) == true))
                return $"❌ Category '{name}' already exists.";

            var category = new Category { CategoryName = name };
            await _repositories.CategoryRepository.AddAsync(category);
            return $"✅ Category '{name}' created successfully! ID: {category.CategoryId}";
        }

        private async Task<string> HandleUpdateCategory(string operation)
        {
            return operation.Contains(',') && !operation.ToLower().Contains("update category", StringComparison.OrdinalIgnoreCase)
                ? await TryUpdateCategoryCommaFormat(operation) ?? await UpdateCategoryStandardFormat(operation)
                : await UpdateCategoryStandardFormat(operation);
        }

        private async Task<string?> TryUpdateCategoryCommaFormat(string operation)
        {
            var parts = operation.Split(',');
            if (parts.Length < 2) return null;

            var identifier = parts[0].Trim();
            var newName = parts[1].Trim();
            
            var category = int.TryParse(identifier, out int id) 
                ? await _repositories.CategoryRepository.GetByIdAsync(id)
                : (await _repositories.CategoryRepository.GetAllAsync()).FirstOrDefault(c => c.CategoryName?.Equals(identifier, StringComparison.OrdinalIgnoreCase) == true);
            
            if (category == null) return $"❌ Category '{identifier}' not found.";
            
            var oldName = category.CategoryName;
            category.CategoryName = newName;
            await _repositories.CategoryRepository.UpdateAsync(category);
            return $"✅ Category '{oldName}' updated to '{newName}'";
        }

        private async Task<string> UpdateCategoryStandardFormat(string operation)
        {
            var match = UpdateCategoryRegex().Match(operation);
            if (!match.Success) return "❌ Format: 'update category [id] name [new_name]' or '[id/oldName],[newName]'";

            var categoryId = int.Parse(match.Groups[1].Value);
            var categoryNewName = match.Groups[2].Value.Trim();

            var categoryToUpdate = await _repositories.CategoryRepository.GetByIdAsync(categoryId);
            if (categoryToUpdate == null) return $"❌ Category with ID {categoryId} not found.";

            var previousName = categoryToUpdate.CategoryName;
            categoryToUpdate.CategoryName = categoryNewName;
            await _repositories.CategoryRepository.UpdateAsync(categoryToUpdate);
            return $"✅ Category '{previousName}' updated to '{categoryNewName}'";
        }

        private async Task<string> HandleDeleteCategory(string operation)
        {
            // Try ID format first
            var idMatch = DeleteCategoryByIdRegex().Match(operation);
            if (idMatch.Success)
            {
                var id = int.Parse(idMatch.Groups[1].Value);
                var category = await _repositories.CategoryRepository.GetByIdAsync(id);
                if (category == null) return $"❌ Category with ID {id} not found.";

                var products = await _repositories.ProductRepository.GetAllAsync();
                if (products.Any(p => p.CategoryId == id))
                    return $"❌ Cannot delete category '{category.CategoryName}' - it contains products.";

                await _repositories.CategoryRepository.DeleteAsync(id);
                return $"✅ Category '{category.CategoryName}' deleted successfully!";
            }
            
            // Try name format
            var nameMatch = DeleteCategoryByNameRegex().Match(operation);
            if (nameMatch.Success)
            {
                var name = nameMatch.Groups[1].Value.Trim();
                var categories = await _repositories.CategoryRepository.GetAllAsync();
                var category = categories.FirstOrDefault(c => c.CategoryName?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
                
                if (category == null) return $"❌ Category '{name}' not found.";

                var products = await _repositories.ProductRepository.GetAllAsync();
                if (products.Any(p => p.CategoryId == category.CategoryId))
                    return $"❌ Cannot delete category '{category.CategoryName}' - it contains products.";

                await _repositories.CategoryRepository.DeleteAsync(category.CategoryId);
                return $"✅ Category '{category.CategoryName}' deleted successfully!";
            }
            
            return "❌ Format: 'delete category [id]' or 'delete category [name]'";
        }


        private async Task<string> HandleListCategories()
        {
            var categories = await _repositories.CategoryRepository.GetAllAsync();
            if (!categories.Any()) return "📂 No categories found.";
            
            return "📂 Categories:\n" + string.Join("\n", categories.Select(c => 
                $"• ID: {c.CategoryId} - {c.CategoryName}"));
        }

        private async Task<string> HandlePendingOrders()
        {
            var orders = await _repositories.OrderRepository.GetAllAsync();
            var pending = orders.Where(o => o.Status == OrderStatus.Pending).ToList();
            
            if (pending.Count == 0) return "✅ No pending orders!";
            
            return "📋 Pending Orders:\n" + string.Join("\n", pending.Select(o => 
                $"• Order #{o.OrderId}: ₹{o.TotalAmount} - {o.OrderDate:yyyy-MM-dd}"));
        }

        private async Task<string> HandleOrderStatistics()
        {
            var orders = await _repositories.OrderRepository.GetAllAsync();
            var today = DateTime.Today;
            
            return $"📊 Order Statistics:\n" +
                   $"• Total Orders: {orders.Count()}\n" +
                   $"• Today's Orders: {orders.Count(o => o.OrderDate.Date == today)}\n" +
                   $"• Pending: {orders.Count(o => o.Status == OrderStatus.Pending)}\n" +
                   $"• Shipped: {orders.Count(o => o.Status == OrderStatus.Shipped)}\n" +
                   $"• Delivered: {orders.Count(o => o.Status == OrderStatus.Delivered)}";
        }

        private async Task<string> HandleUpdateOrder(string operation)
        {
            var match = UpdateOrderRegex().Match(operation);
            if (!match.Success) return "❌ Format: 'update order [id] status [Pending/Shipped/Delivered]'";

            var id = int.Parse(match.Groups[1].Value);
            var statusStr = match.Groups[2].Value;

            if (!Enum.TryParse<OrderStatus>(statusStr, true, out var status))
                return "❌ Invalid status. Use: Pending, Shipped, Delivered, Cancelled";

            var order = await _repositories.OrderRepository.GetByIdAsync(id);
            if (order == null) return $"❌ Order #{id} not found.";

            order.Status = status;
            await _repositories.OrderRepository.UpdateAsync(order);
            return $"✅ Order #{id} status updated to {status}";
        }

        private async Task<string> HandleSalesReport()
        {
            var orders = await _repositories.OrderRepository.GetAllAsync();
            var payments = await _repositories.PaymentRepository.GetAllAsync();
            
            var completedPayments = payments.Where(p => p.Status == PaymentStatus.Completed);
            var totalRevenue = completedPayments.Sum(p => p.Amount);
            var completedCount = completedPayments.Count();
            var avgOrder = completedCount > 0 ? totalRevenue / completedCount : 0;
            
            return $"💰 Sales Report:\n" +
                   $"• Total Revenue: ₹{totalRevenue:N2}\n" +
                   $"• Completed Orders: {completedCount}\n" +
                   $"• Average Order Value: ₹{avgOrder:N2}\n" +
                   $"• This Month Orders: {orders.Count(o => o.OrderDate.Month == DateTime.Now.Month)}";
        }

        private async Task<string> HandleUserStatistics()
        {
            var users = await _repositories.UserRepository.GetAllAsync();
            var orders = await _repositories.OrderRepository.GetAllAsync();
            
            return $"👥 User Statistics:\n" +
                   $"• Total Users: {users.Count()}\n" +
                   $"• Admins: {users.Count(u => u.Role == UserRole.Admin)}\n" +
                   $"• Customers: {users.Count(u => u.Role == UserRole.Customer)}\n" +
                   $"• Active Users (with orders): {orders.Select(o => o.UserId).Distinct().Count()}";
        }

        private async Task<string> HandleInventoryReport()
        {
            var products = await _repositories.ProductRepository.GetAllAsync();
            var totalValue = products.Sum(p => p.Price * p.StockQuantity);
            var lowStock = products.Count(p => p.StockQuantity < 10);
            var outOfStock = products.Count(p => p.StockQuantity == 0);
            
            return $"📦 Inventory Report:\n" +
                   $"• Total Products: {products.Count()}\n" +
                   $"• Total Stock Value: ₹{totalValue:N2}\n" +
                   $"• Low Stock (<10): {lowStock}\n" +
                   $"• Out of Stock: {outOfStock}";
        }

        // Customer Operations
        private async Task<string> HandleProductSearch(string operation)
        {
            var products = await _repositories.ProductRepository.GetAllAsync();
            
            var searchTerms = operation.ToLower().Split(' ');
            var foundProducts = products.Where(p =>
                searchTerms.Any(term =>
                    p.Name?.Contains(term, StringComparison.OrdinalIgnoreCase) == true ||
                    p.Description?.Contains(term, StringComparison.OrdinalIgnoreCase) == true
                )).Take(5).ToList();

            if (foundProducts.Count == 0) return "❌ No products found matching your search.";

            return "🛍️ Found Products:\n" + string.Join("\n", foundProducts.Select(p =>
                $"• {p.Name} - ₹{p.Price} (Stock: {p.StockQuantity})"));
        }

        private async Task<string> HandleOrderStatus(string operation, int? userId)
        {
            if (!userId.HasValue)
                return "❌ Please login to check your order status.";

            var match = NumberRegex().Match(operation);
            if (!match.Success) return "❌ Please provide order ID. Example: 'check order 22'";

            var orderId = int.Parse(match.Groups[1].Value);
            var order = await _repositories.OrderRepository.GetByIdAsync(orderId);
            
            if (order == null) return $"❌ Order #{orderId} not found.";
            
            if (order.UserId != userId.Value)
                return "❌ This order doesn't belong to your account.";

            return $"📦 Order #{order.OrderId}:\n" +
                   $"• Status: {order.Status}\n" +
                   $"• Total: ₹{order.TotalAmount}\n" +
                   $"• Placed: {order.OrderDate:yyyy-MM-dd}\n" +
                   $"• Payment: {order.PaymentMethod}";
        }

        private async Task<string> HandleOrderCancellation(string operation, int? userId)
        {
            if (!userId.HasValue)
                return "❌ Please login to cancel your order.";

            var match = CancelOrderRegex().Match(operation);
            if (!match.Success) return "❌ Please provide order ID. Example: 'cancel order 12'";

            var orderId = int.Parse(match.Groups[1].Value);
            var order = await _repositories.OrderRepository.GetByIdAsync(orderId);
            
            if (order == null) return $"❌ Order #{orderId} not found.";
            
            if (order.UserId != userId.Value)
                return "❌ This order doesn't belong to your account.";

            if (order.Status == OrderStatus.Delivered)
                return $"❌ Order #{orderId} has already been delivered and cannot be cancelled.";

            if (order.Status == OrderStatus.Cancelled)
                return $"ℹ️ Order #{orderId} is already cancelled.";

            // Restore product stock
            var orderItems = await _repositories.OrderItemRepository.GetAllAsync();
            var orderItemsForOrder = orderItems.Where(oi => oi.OrderId == orderId).ToList();
            
            foreach (var orderItem in orderItemsForOrder)
            {
                var product = await _repositories.ProductRepository.GetByIdAsync(orderItem.ProductId);
                if (product != null)
                {
                    product.StockQuantity += orderItem.Quantity;
                    await _repositories.ProductRepository.UpdateAsync(product);
                }
            }

            order.Status = OrderStatus.Cancelled;
            await _repositories.OrderRepository.UpdateAsync(order);
            
            return $"✅ Order #{orderId} has been cancelled successfully. Stock quantities have been restored. Refund will be processed within 3-5 business days.";
        }

        private async Task<string> HandleAddToCart(string operation, int? userId)
        {
            if (!userId.HasValue)
                return "❌ Please login to add items to your cart.";

            // Try different formats: "add iPhone to cart", "add product 5 to cart", "add 2 iPhone to cart"
            var match = AddToCartRegex().Match(operation);
            if (!match.Success)
                match = AddToCartAltRegex().Match(operation);
            
            if (!match.Success) return "❌ Format: 'add [product name] to cart' or 'add [quantity] [product name] to cart'";

            var quantityStr = match.Groups[1].Value;
            var productIdentifier = match.Groups[2].Value.Trim();
            
            if (string.IsNullOrEmpty(productIdentifier))
                productIdentifier = match.Groups[1].Value.Trim();

            int quantity = 1;
            if (!string.IsNullOrEmpty(quantityStr) && int.TryParse(quantityStr, out int parsedQty))
                quantity = parsedQty;

            // Find product by ID or name
            Product? product = null;
            if (int.TryParse(productIdentifier, out int productId))
            {
                product = await _repositories.ProductRepository.GetByIdAsync(productId);
            }
            else
            {
                var products = await _repositories.ProductRepository.GetAllAsync();
                product = products.FirstOrDefault(p => 
                    p.Name?.Contains(productIdentifier, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (product == null) return $"❌ Product '{productIdentifier}' not found.";
            
            if (product.StockQuantity < quantity)
                return $"❌ Only {product.StockQuantity} units of '{product.Name}' available in stock.";

            // Get or create user's cart
            var carts = await _repositories.CartRepository.GetAllAsync();
            var cart = carts.FirstOrDefault(c => c.UserId == userId.Value);
            
            if (cart == null)
            {
                cart = new Cart { UserId = userId.Value };
                await _repositories.CartRepository.AddAsync(cart);
            }

            // Check if product already exists in cart
            var cartItems = await _repositories.CartItemRepository.GetAllAsync();
            var existingItem = cartItems.FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == product.ProductId);
            
            if (existingItem != null)
            {
                // Update existing cart item
                existingItem.Quantity += quantity;
                existingItem.TotalPrice = existingItem.Quantity * product.Price;
                await _repositories.CartItemRepository.UpdateAsync(existingItem);
                
                return $"✅ Updated '{product.Name}' in your cart! Total quantity: {existingItem.Quantity}, Price: ₹{existingItem.TotalPrice}";
            }
            else
            {
                // Add new cart item
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = product.ProductId,
                    Quantity = quantity,
                    TotalPrice = quantity * product.Price
                };
                
                await _repositories.CartItemRepository.AddAsync(cartItem);
                
                return $"✅ Added {quantity}x '{product.Name}' to your cart! Price: ₹{cartItem.TotalPrice}";
            }
        }

        private async Task<string> HandleViewCart(int? userId)
        {
            if (!userId.HasValue)
                return "❌ Please login to view your cart.";

            var carts = await _repositories.CartRepository.GetAllAsync();
            var cart = carts.FirstOrDefault(c => c.UserId == userId.Value);
            
            if (cart == null)
                return "🛒 Your cart is empty. Start shopping to add items!";

            var cartItems = await _repositories.CartItemRepository.GetAllAsync();
            var userCartItems = cartItems.Where(ci => ci.CartId == cart.CartId).ToList();
            
            if (userCartItems.Count == 0)
                return "🛒 Your cart is empty. Start shopping to add items!";

            var products = await _repositories.ProductRepository.GetAllAsync();
            var cartDetails = userCartItems.Select(ci =>
            {
                var product = products.FirstOrDefault(p => p.ProductId == ci.ProductId);
                return new
                {
                    ProductName = product?.Name ?? "Unknown Product",
                    Quantity = ci.Quantity,
                    UnitPrice = product?.Price ?? 0,
                    TotalPrice = ci.TotalPrice
                };
            }).ToList();

            var totalAmount = cartDetails.Sum(cd => cd.TotalPrice);
            var itemCount = cartDetails.Sum(cd => cd.Quantity);

            var cartSummary = "🛒 Your Cart:\n" +
                             string.Join("\n", cartDetails.Select(cd =>
                                 $"• {cd.ProductName} x{cd.Quantity} - ₹{cd.TotalPrice}")) +
                             $"\n\n💰 Total: ₹{totalAmount} ({itemCount} items)";

            return cartSummary;
        }

        private async Task<string> HandlePlaceOrder(string operation, int? userId)
        {
            if (!userId.HasValue)
                return "❌ Please login to place an order.";

            // Parse order format: "order 5 jeans, address lucknow, payment mode cod"
            var match = PlaceOrderRegex().Match(operation);
            if (!match.Success)
                return "❌ Format: 'order [quantity] [product], address [location], payment mode [cod/online]'";

            var quantity = int.Parse(match.Groups[1].Value);
            var productName = match.Groups[2].Value.Trim();
            var address = match.Groups[3].Value.Trim();
            var paymentMode = match.Groups[4].Value.Trim().ToUpper();

            if (paymentMode != "COD" && paymentMode != OnlinePayment)
                return "❌ Payment mode must be 'COD' or 'ONLINE'";

            // Find product
            var products = await _repositories.ProductRepository.GetAllAsync();
            var product = products.FirstOrDefault(p => p.Name?.Contains(productName, StringComparison.OrdinalIgnoreCase) == true);

            if (product == null) return $"❌ Product '{productName}' not found.";
            
            if (product.StockQuantity < quantity)
                return $"❌ Only {product.StockQuantity} units of '{product.Name}' available in stock.";

            // Create order
            var totalAmount = product.Price * quantity;
            var order = new Order
            {
                UserId = userId.Value,
                TotalAmount = totalAmount,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                PaymentMethod = paymentMode == "COD" ? PaymentMethod.COD : PaymentMethod.UPI,
                ShippingAddress = address
            };

            await _repositories.OrderRepository.AddAsync(order);

            // Create order item
            var orderItem = new OrderItem
            {
                OrderId = order.OrderId,
                ProductId = product.ProductId,
                Quantity = quantity,
                Price = totalAmount
            };

            await _repositories.OrderItemRepository.AddAsync(orderItem);

            // Update product stock
            product.StockQuantity -= quantity;
            await _repositories.ProductRepository.UpdateAsync(product);

            // Create payment record
            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = totalAmount,
                PaymentDate = DateTime.Now,
                Mode = paymentMode == "COD" ? PaymentMode.COD : PaymentMode.UPI,
                Status = paymentMode == "COD" ? PaymentStatus.Pending : PaymentStatus.Completed
            };

            await _repositories.PaymentRepository.AddAsync(payment);

            return $"✅ Order placed successfully!\n" +
                   $"📦 Order ID: #{order.OrderId}\n" +
                   $"🛍️ Product: {product.Name} x{quantity}\n" +
                   $"💰 Total: ₹{totalAmount}\n" +
                   $"📍 Address: {address}\n" +
                   $"💳 Payment: {paymentMode}\n" +
                   $"📅 Expected delivery: 3-5 business days";
        }

        private async Task<string> HandleOrderFromCart(string operation, int? userId)
        {
            if (!userId.HasValue) return "❌ Please login to order from your cart.";

            var (address, paymentMode, validationError) = ValidateCheckoutRequest(operation);
            if (validationError != null) return validationError;

            var (_, cartItems, cartValidationError) = await ValidateUserCart(userId.Value);
            if (cartValidationError != null) return cartValidationError;

            var (products, stockValidationError) = await ValidateCartStock(cartItems);
            if (stockValidationError != null) return stockValidationError;

            return await ProcessCartOrder(userId.Value, cartItems, products, address, paymentMode);
        }

        private static (string address, string paymentMode, string? error) ValidateCheckoutRequest(string operation)
        {
            var match = CheckoutRegex().Match(operation);
            if (!match.Success) return ("", "", "❌ Format: 'order cart, address [location], payment mode [cod/online]' or 'checkout, address [location], payment mode [cod/online]'");

            var address = match.Groups[1].Value.Trim();
            var paymentMode = match.Groups[2].Value.Trim().ToUpper();

            return paymentMode != "COD" && paymentMode != "ONLINE" 
                ? ("", "", "❌ Payment mode must be 'COD' or 'ONLINE'")
                : (address, paymentMode, null);
        }

        private async Task<(Cart? cart, List<CartItem> cartItems, string? error)> ValidateUserCart(int userId)
        {
            var carts = await _repositories.CartRepository.GetAllAsync();
            var cart = carts.FirstOrDefault(c => c.UserId == userId);
            if (cart == null) return (null, new(), "❌ Your cart is empty. Add items to cart first.");

            var cartItems = await _repositories.CartItemRepository.GetAllAsync();
            var userCartItems = cartItems.Where(ci => ci.CartId == cart.CartId).ToList();
            
            return userCartItems.Count == 0 
                ? (null, new(), "❌ Your cart is empty. Add items to cart first.")
                : (cart, userCartItems, null);
        }

        private async Task<(List<Product> products, string? error)> ValidateCartStock(List<CartItem> cartItems)
        {
            var products = (await _repositories.ProductRepository.GetAllAsync()).ToList();
            
            foreach (var cartItem in cartItems)
            {
                var product = products.FirstOrDefault(p => p.ProductId == cartItem.ProductId);
                if (product == null || product.StockQuantity < cartItem.Quantity)
                {
                    var productName = product?.Name ?? "Unknown Product";
                    var availableStock = product?.StockQuantity ?? 0;
                    return (new(), $"❌ Insufficient stock for '{productName}'. Available: {availableStock}, Required: {cartItem.Quantity}");
                }
            }
            
            return (products, null);
        }

        private async Task<string> ProcessCartOrder(int userId, List<CartItem> cartItems, List<Product> products, string address, string paymentMode)
        {
            var totalAmount = cartItems.Sum(ci => ci.TotalPrice);
            var order = await CreateOrderFromCart(userId, totalAmount, address, paymentMode);
            var orderSummary = await CreateOrderItemsAndUpdateStock(order.OrderId, cartItems, products);
            await ClearUserCart(cartItems);
            await CreatePaymentRecord(order.OrderId, totalAmount, paymentMode);

            return $"✅ Order placed successfully from your cart!\n📦 Order ID: #{order.OrderId}\n🛍️ Items:\n{string.Join("\n", orderSummary)}\n💰 Total: ₹{totalAmount}\n📍 Address: {address}\n💳 Payment: {paymentMode}\n📅 Expected delivery: 3-5 business days\n🛒 Your cart has been cleared.";
        }

        private async Task<Order> CreateOrderFromCart(int userId, decimal totalAmount, string address, string paymentMode)
        {
            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                PaymentMethod = paymentMode == "COD" ? PaymentMethod.COD : PaymentMethod.UPI,
                ShippingAddress = address
            };
            await _repositories.OrderRepository.AddAsync(order);
            return order;
        }

        private async Task<List<string>> CreateOrderItemsAndUpdateStock(int orderId, List<CartItem> cartItems, List<Product> products)
        {
            var orderSummary = new List<string>();
            foreach (var cartItem in cartItems)
            {
                var product = products.FirstOrDefault(p => p.ProductId == cartItem.ProductId);
                if (product != null)
                {
                    await _repositories.OrderItemRepository.AddAsync(new OrderItem
                    {
                        OrderId = orderId,
                        ProductId = product.ProductId,
                        Quantity = cartItem.Quantity,
                        Price = cartItem.TotalPrice
                    });

                    product.StockQuantity -= cartItem.Quantity;
                    await _repositories.ProductRepository.UpdateAsync(product);
                    orderSummary.Add($"• {product.Name} x{cartItem.Quantity} - ₹{cartItem.TotalPrice}");
                }
            }
            return orderSummary;
        }

        private async Task ClearUserCart(List<CartItem> cartItems)
        {
            foreach (var cartItem in cartItems)
            {
                await _repositories.CartItemRepository.DeleteAsync(cartItem.CartItemId);
            }
        }

        private async Task CreatePaymentRecord(int orderId, decimal amount, string paymentMode)
        {
            await _repositories.PaymentRepository.AddAsync(new Payment
            {
                OrderId = orderId,
                Amount = amount,
                PaymentDate = DateTime.Now,
                Mode = paymentMode == "COD" ? PaymentMode.COD : PaymentMode.UPI,
                Status = paymentMode == "COD" ? PaymentStatus.Pending : PaymentStatus.Completed
            });
        }

        private async Task<string> HandleClearCart(int? userId)
        {
            if (!userId.HasValue)
                return "❌ Please login to clear your cart.";

            var carts = await _repositories.CartRepository.GetAllAsync();
            var cart = carts.FirstOrDefault(c => c.UserId == userId.Value);
            
            if (cart == null)
                return "🛒 Your cart is already empty.";

            var cartItems = await _repositories.CartItemRepository.GetAllAsync();
            var userCartItems = cartItems.Where(ci => ci.CartId == cart.CartId).ToList();
            
            if (userCartItems.Count == 0)
                return "🛒 Your cart is already empty.";

            // Delete all cart items
            foreach (var cartItem in userCartItems)
            {
                await _repositories.CartItemRepository.DeleteAsync(cartItem.CartItemId);
            }

            return "✅ Your cart has been cleared successfully!";
        }

        private async Task<string> HandleMultiProductOrder(string operation, int? userId)
        {
            if (!userId.HasValue) return "❌ Please login to place an order.";

            var match = MultiOrderRegex().Match(operation);
            if (!match.Success) return $"❌ Format: 'multi order [qty] [product], [qty] [product], address [location], payment mode [cod/online]'. Received: '{operation}'";

            var productsStr = match.Groups[1].Value.Trim();
            var address = match.Groups[2].Value.Trim();
            var paymentMode = match.Groups[3].Value.Trim().ToUpper();

            if (paymentMode != "COD" && paymentMode != "ONLINE") return "❌ Payment mode must be 'COD' or 'ONLINE'";

            var parseResult = await ParseMultiProductItems(productsStr);
            if (parseResult.ErrorMessage != null) return parseResult.ErrorMessage;

            return await CreateMultiProductOrder(userId.Value, parseResult.OrderItems, parseResult.TotalAmount, address, paymentMode);
        }

        private async Task<(List<(Product product, int quantity)> OrderItems, decimal TotalAmount, string? ErrorMessage)> ParseMultiProductItems(string productsStr)
        {
            var productParts = productsStr.Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            if (productParts.Count == 0) return (new(), 0, $"❌ No products found in: '{productsStr}'");

            var orderItems = new List<(Product product, int quantity)>();
            var products = await _repositories.ProductRepository.GetAllAsync();
            decimal totalAmount = 0;

            foreach (var part in productParts)
            {
                var productMatch = ProductQuantityRegex().Match(part.Trim());
                if (!productMatch.Success) return (new(), 0, $"❌ Invalid format for '{part.Trim()}'. Use: '[quantity] [product name]'");

                var quantity = int.Parse(productMatch.Groups[1].Value);
                var productName = productMatch.Groups[2].Value.Trim();
                var product = products.FirstOrDefault(p => p.Name?.Contains(productName, StringComparison.OrdinalIgnoreCase) == true);

                if (product == null) return (new(), 0, $"❌ Product '{productName}' not found. Parsed from: '{part.Trim()}'");
                if (product.StockQuantity < quantity) return (new(), 0, $"❌ Only {product.StockQuantity} units of '{product.Name}' available in stock.");

                orderItems.Add((product, quantity));
                totalAmount += product.Price * quantity;
            }

            return (orderItems, totalAmount, null);
        }

        private async Task<string> CreateMultiProductOrder(int userId, List<(Product product, int quantity)> orderItems, decimal totalAmount, string address, string paymentMode)
        {
            var order = new Order
            {
                UserId = userId,
                TotalAmount = totalAmount,
                OrderDate = DateTime.Now,
                Status = OrderStatus.Pending,
                PaymentMethod = paymentMode == "COD" ? PaymentMethod.COD : PaymentMethod.UPI,
                ShippingAddress = address
            };

            await _repositories.OrderRepository.AddAsync(order);

            var orderSummary = new List<string>();
            foreach (var (product, quantity) in orderItems)
            {
                await _repositories.OrderItemRepository.AddAsync(new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = product.ProductId,
                    Quantity = quantity,
                    Price = product.Price * quantity
                });

                product.StockQuantity -= quantity;
                await _repositories.ProductRepository.UpdateAsync(product);
                orderSummary.Add($"• {product.Name} x{quantity} - ₹{product.Price * quantity}");
            }

            await _repositories.PaymentRepository.AddAsync(new Payment
            {
                OrderId = order.OrderId,
                Amount = totalAmount,
                PaymentDate = DateTime.Now,
                Mode = paymentMode == "COD" ? PaymentMode.COD : PaymentMode.UPI,
                Status = paymentMode == "COD" ? PaymentStatus.Pending : PaymentStatus.Completed
            });

            return $"✅ Multi-product order placed successfully!\n📦 Order ID: #{order.OrderId}\n🛍️ Items:\n{string.Join("\n", orderSummary)}\n💰 Total: ₹{totalAmount}\n📍 Address: {address}\n💳 Payment: {paymentMode}\n📅 Expected delivery: 3-5 business days";
        }

        // Flexible AI-friendly handlers
        private async Task<string> HandleFlexibleAddToCart(string operation, int? userId)
        {
            if (!userId.HasValue) return "❌ Please login to add items to your cart.";

            var products = await _repositories.ProductRepository.GetAllAsync();
            var matchingProducts = products.Where(product => operation.Contains(product.Name ?? "", StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (matchingProducts.Count == 0) return "❌ No products found in your request.";

            var tasks = matchingProducts.Select(async product =>
            {
                var quantity = ExtractQuantityForProduct(operation, product.Name ?? "");
                return await HandleAddToCart($"add {quantity} {product.Name} to cart", userId);
            });
            
            var results = await Task.WhenAll(tasks);
            return string.Join("\n", results);
        }

        private static int ExtractQuantityForProduct(string operation, string productName)
        {
            var words = operation.Split(' ');
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i + 1].Contains(productName, StringComparison.OrdinalIgnoreCase) && int.TryParse(words[i], out int qty))
                {
                    return qty;
                }
            }
            return 1;
        }

        private async Task<string> HandleFlexibleCheckout(string operation, int? userId)
        {
            if (!userId.HasValue)
                return "❌ Please login to checkout.";

            // Extract address and payment using simple text analysis
            var words = operation.Split(SplitSeparators, StringSplitOptions.RemoveEmptyEntries);
            var address = "Not specified";
            var paymentMode = "COD";
            
            // Find address after "address" keyword
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i].ToLower().Contains("address", StringComparison.OrdinalIgnoreCase) && i + 1 < words.Length)
                {
                    address = words[i + 1].Trim();
                    break;
                }
            }
            
            // Detect payment method
            var lowerOp = operation.ToLower();
            if (lowerOp.Contains("online", StringComparison.OrdinalIgnoreCase) || lowerOp.Contains("upi", StringComparison.OrdinalIgnoreCase)) paymentMode = OnlinePayment;
            
            return await HandleOrderFromCart($"checkout, address {address}, payment mode {paymentMode}", userId);
        }

        private async Task<string> HandleFlexibleOrderStatus(string operation, int? userId)
        {
            // Find any number in the operation text
            var words = operation.Split(' ');
            var orderIdWord = words.FirstOrDefault(word => int.TryParse(word, out _));
            if (orderIdWord != null && int.TryParse(orderIdWord, out int orderId))
            {
                return await HandleOrderStatus($"order status {orderId}", userId);
            }
            return "❌ Please provide order ID.";
        }

        private async Task<string> HandleFlexibleOrderCancellation(string operation, int? userId)
        {
            // Find any number in the operation text
            var words = operation.Split(' ');
            var orderIdWord = words.FirstOrDefault(word => int.TryParse(word, out _));
            if (orderIdWord != null && int.TryParse(orderIdWord, out int orderId))
            {
                return await HandleOrderCancellation($"cancel order {orderId}", userId);
            }
            return "❌ Please provide order ID.";
        }

        private async Task<string> HandleFlexiblePlaceOrder(string operation, int? userId)
        {
            if (!userId.HasValue) return "❌ Please login to place an order.";

            var (address, paymentMode) = ExtractOrderDetails(operation);
            var orderItems = await ExtractOrderItems(operation);

            return orderItems.Count switch
            {
                1 => await HandlePlaceOrder($"order {orderItems[0].Item2} {orderItems[0].Item1}, address {address}, payment mode {paymentMode}", userId),
                > 1 => await HandleMultiProductOrder($"multi order {string.Join(", ", orderItems.Select(i => $"{i.Item2} {i.Item1}"))}, address {address}, payment mode {paymentMode}", userId),
                _ => "❌ No products found in your order request."
            };
        }

        private static (string address, string paymentMode) ExtractOrderDetails(string operation)
        {
            var address = "Not specified";
            var paymentMode = "COD";
            var lowerOp = operation.ToLower();

            if (lowerOp.Contains("address", StringComparison.OrdinalIgnoreCase))
            {
                var addressMatch = AddressRegex().Match(operation);
                if (addressMatch.Success) address = addressMatch.Groups[1].Value.Trim();
            }

            if (lowerOp.Contains("online", StringComparison.OrdinalIgnoreCase) || lowerOp.Contains("upi", StringComparison.OrdinalIgnoreCase))
                paymentMode = OnlinePayment;

            return (address, paymentMode);
        }

        private async Task<List<(string, int)>> ExtractOrderItems(string operation)
        {
            var products = await _repositories.ProductRepository.GetAllAsync();
            return products
                .Where(product => operation.Contains(product.Name ?? "", StringComparison.OrdinalIgnoreCase))
                .Select(product => (product.Name ?? "", ExtractQuantityForProduct(operation, product.Name ?? "")))
                .ToList();
        }
    }
}
