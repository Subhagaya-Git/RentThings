using Azure;
using Azure.Communication.Sms;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace RentThings.Api.Services
{
    // 1. Interface එක මෙතැනම නිර්මාණය කිරීම (මෙය Program.cs එකට අවශ්‍ය වේ)
    public interface INotificationService
    {
        Task<bool> SendSmsAsync(string toPhoneNumber, string message);
    }

    // 2. Service Implementation එක
    public class NotificationService : INotificationService
    {
        private readonly string? _connectionString;

        public NotificationService(IConfiguration configuration)
        {
            // appsettings.json එකෙන් Connection String එක කියවීම
            _connectionString = configuration["Azure:Communication:ConnectionString"];
        }

        public async Task<bool> SendSmsAsync(string toPhoneNumber, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    Console.WriteLine("[ACS SMS Error]: Connection string is missing in appsettings.json.");
                    return false;
                }

                // Azure SDK SmsClient එක සාදා ගැනීම
                SmsClient smsClient = new SmsClient(_connectionString);

                // ටෙස්ට් කිරීම් සඳහා From එකට "Azure" භාවිත කළ හැක
                var response = await smsClient.SendAsync(
                    from: "Azure", 
                    to: toPhoneNumber, // ලංකාවේ අංක: +94756868881
                    message: message
                );

                Console.WriteLine($"[ACS SMS Success]: Message sent successfully to {toPhoneNumber}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ACS SMS Error]: {ex.Message}");
                return false;
            }
        }
    }
}