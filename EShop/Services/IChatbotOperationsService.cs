namespace EShop.Services
{
    public interface IChatbotOperationsService
    {
        Task<string> ExecuteAdminOperationAsync(string operation, string userRole);
        Task<string> ExecuteCustomerOperationAsync(string operation, int? userId = null);

    }
}