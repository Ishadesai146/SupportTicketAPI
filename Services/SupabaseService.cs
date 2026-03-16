using Supabase;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
namespace SupportTicketAPI.Services
{
    public class SupabaseService
    {
        private readonly Client _client;
        private readonly string _bucket;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public SupabaseService(IConfiguration config, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> UploadStreamAsync(Stream stream, string fileName, string contentType)
        {
            try
            {
                var url = $"{_config["Supabase:Url"]}/storage/v1/object/{_config["Supabase:Bucket"]}/{fileName}";

                var content = new StreamContent(stream);
                content.Headers.ContentType =
                    new MediaTypeHeaderValue(contentType);

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _config["Supabase:Key"]);

                var response = await _httpClient.PostAsync(url, content);
                Console.WriteLine(response.Content.ReadAsStream());
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();

                    Console.WriteLine("Supabase upload failed. Status: {Status} Error: {Error}",
                        response.StatusCode, error);

                    throw new ApplicationException("File upload to storage failed.");
                }

                var publicUrl =
                    $"{_config["Supabase:Url"]}/storage/v1/object/{_config["Supabase:Bucket"]}/{fileName}";

                return publicUrl;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Network error while uploading file to Supabase: {ex.Message}");
                throw new ApplicationException("Storage service unavailable.");
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"Upload timeout while sending file to Supabase: {ex.Message}");
                throw new ApplicationException("File upload timeout.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error during Supabase upload: {ex.Message}");
                throw;
            }
        }
    }
}
