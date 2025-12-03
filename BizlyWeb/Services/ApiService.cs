using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BizlyWeb.Services.Exceptions;
using Microsoft.AspNetCore.Http;

namespace BizlyWeb.Services
{
    /// <summary>
    /// Servicio base para comunicación con la API externa
    /// Capa de Datos - Maneja toda la comunicación HTTP con la API
    /// </summary>
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public ApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://apibizly.onrender.com";
            
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Obtiene el token JWT de la sesión
        /// </summary>
        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString("JWTToken");
        }

        /// <summary>
        /// Configura el header de autorización con el token JWT
        /// </summary>
        private void SetAuthorizationHeader()
        {
            var token = GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        /// <summary>
        /// Realiza una petición GET a la API
        /// </summary>
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync(endpoint);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                return default(T);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al realizar GET a {endpoint}: {ex.Message}", ex);
            }
        }

        private static TResponse? DeserializeResponse<TResponse>(string responseContent)
        {
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return default;
            }

            return JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private static void EnsureSuccess(HttpResponseMessage response, string endpoint, string responseContent)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException(
                    $"Error al invocar {endpoint}. Código: {(int)response.StatusCode}",
                    response.StatusCode,
                    responseContent);
            }
        }

        /// <summary>
        /// Realiza una petición POST a la API
        /// </summary>
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                SetAuthorizationHeader();
                var json = JsonSerializer.Serialize(data);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                EnsureSuccess(response, endpoint, responseContent);

                return DeserializeResponse<TResponse>(responseContent);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al realizar POST a {endpoint}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Realiza una petición PUT a la API
        /// </summary>
        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
        {
            try
            {
                SetAuthorizationHeader();
                var json = JsonSerializer.Serialize(data);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                EnsureSuccess(response, endpoint, responseContent);

                return DeserializeResponse<TResponse>(responseContent);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al realizar PUT a {endpoint}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Realiza una petición DELETE a la API
        /// </summary>
        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();
                EnsureSuccess(response, endpoint, responseContent);
                return true;
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al realizar DELETE a {endpoint}: {ex.Message}", ex);
            }
        }
    }
}

