using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BizlyWeb.Services.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<ApiService>? _logger;
        private readonly string _baseUrl;

        public ApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<ApiService>? logger = null)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
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
                // Limpiar headers anteriores para evitar conflictos
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
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
                var token = GetToken();
                
                // Log para debugging (solo en desarrollo)
                if (string.IsNullOrEmpty(token))
                {
                    _logger?.LogWarning("Intento de acceso a {Endpoint} sin token JWT", endpoint);
                }
                else
                {
                    _logger?.LogDebug("Accediendo a {Endpoint} con token JWT presente", endpoint);
                }
                
                var response = await _httpClient.GetAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                
                // Si es error de autorización, lanzar excepción específica
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger?.LogWarning("Error de autorización al acceder a {Endpoint}. Status: {StatusCode}, Response: {ResponseContent}", 
                        endpoint, response.StatusCode, responseContent);
                    throw new ApiException(
                        $"No tienes permisos para acceder a {endpoint}",
                        response.StatusCode,
                        responseContent);
                }
                
                // Para otros errores, lanzar ApiException
                if (!response.IsSuccessStatusCode)
                {
                    throw new ApiException(
                        $"Error al realizar GET a {endpoint}. Código: {(int)response.StatusCode}",
                        response.StatusCode,
                        responseContent);
                }
                
                return default(T);
            }
            catch (ApiException)
            {
                throw; // Re-throw ApiException
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
                var token = GetToken();
                
                // Log para debugging
                if (string.IsNullOrEmpty(token))
                {
                    _logger?.LogWarning("Intento de POST a {Endpoint} sin token JWT", endpoint);
                }
                
                var jsonOptions = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = false
                };
                var json = JsonSerializer.Serialize(data, jsonOptions);
                
                // Log del JSON enviado (solo para debugging de registro)
                if (endpoint.Contains("/auth/registro-emprendedor"))
                {
                    _logger?.LogInformation("JSON enviado a registro: {Json}", json);
                }
                
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Manejar errores de autorización específicamente
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger?.LogWarning("Error de autorización al acceder a {Endpoint}. Status: {StatusCode}, Response: {ResponseContent}", 
                        endpoint, response.StatusCode, responseContent);
                    throw new ApiException(
                        $"No tienes permisos para acceder a {endpoint}",
                        response.StatusCode,
                        responseContent);
                }
                
                EnsureSuccess(response, endpoint, responseContent);

                // Log temporal para debugging (solo para login)
                if (endpoint.Contains("/auth/login"))
                {
                    _logger?.LogInformation("Login API Response: {Response}", responseContent);
                }

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
                var token = GetToken();
                
                // Log para debugging
                if (string.IsNullOrEmpty(token))
                {
                    _logger?.LogWarning("Intento de PUT a {Endpoint} sin token JWT", endpoint);
                }
                
                var jsonOptions = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = false
                };
                var json = JsonSerializer.Serialize(data, jsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Manejar errores de autorización específicamente
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || 
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    _logger?.LogWarning("Error de autorización al acceder a {Endpoint}. Status: {StatusCode}, Response: {ResponseContent}", 
                        endpoint, response.StatusCode, responseContent);
                    throw new ApiException(
                        $"No tienes permisos para acceder a {endpoint}",
                        response.StatusCode,
                        responseContent);
                }
                
                EnsureSuccess(response, endpoint, responseContent);

                // Si es 204 NoContent, retornar default (null)
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent || string.IsNullOrWhiteSpace(responseContent))
                {
                    return default(TResponse);
                }

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

