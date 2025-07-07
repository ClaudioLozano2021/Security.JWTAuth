// Programa de consola que permite:
// 1. Crear un usuario (vía API)
// 2. Hacer login y obtener el JWT
// 3. Probar un endpoint protegido usando el token

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

string baseUrl = "http://localhost:5000";
HttpClient httpClient = new();
string? jwtToken = null;

while (true)
{
    Console.Clear();
    Console.WriteLine("============================");
    Console.WriteLine(" JWT DEMO – CONSOLA CLIENTE ");
    Console.WriteLine("============================\n");
    Console.WriteLine("Servidor: http://localhost:5000\n");

    Console.WriteLine("\nMENÚ PRINCIPAL\n");
    Console.WriteLine("1. Crear usuario");
    Console.WriteLine("2. Login");
    Console.WriteLine("3. Acceder a endpoint protegido");
    Console.WriteLine("4. Ver información de sesión");
    Console.WriteLine("5. Logout");
    Console.WriteLine("0. Salir");
    Console.Write("Seleccione una opción: ");
    string opcion = Console.ReadLine() ?? "";

    switch (opcion)
    {
        case "1":
            await CrearUsuario();
            break;
        case "2":
            jwtToken = await Login();
            break;
        case "3":
            await AccederEndpointProtegido(jwtToken);
            break;
        case "4":
            await VerInformacionSesion(jwtToken);
            break;
        case "5":
            await Logout(jwtToken);
            jwtToken = null;
            break;
        case "0":
            Console.WriteLine("Saliendo...");
            return;
        default:
            Console.WriteLine("Opción inválida.");
            break;
    }
}

async Task CrearUsuario()
{
    Console.Write("Ingrese nombre de usuario: ");
    string username = Console.ReadLine() ?? "";
    Console.Write("Ingrese contraseña: ");
    string password = Console.ReadLine() ?? "";

    var user = new { Username = username, Password = password };
    var content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

    var response = await httpClient.PostAsync($"{baseUrl}/api/auth/register", content);
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Usuario creado correctamente.");
    }
    else
    {
        Console.WriteLine("Error al crear usuario: " + response.StatusCode);
    }
}

async Task<string?> Login()
{
    Console.Write("Usuario: ");
    string username = Console.ReadLine() ?? "";
    Console.Write("Contraseña: ");
    string password = Console.ReadLine() ?? "";

    var loginData = new { Username = username, Password = password };
    var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

    var response = await httpClient.PostAsync($"{baseUrl}/api/auth/login", content);
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine("Login erroneo.");
        Console.WriteLine("Presione cualquier tecla.");
        Console.ReadKey();
        return null;
    }

    var body = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<TokenResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    Console.WriteLine("Token recibido.");
    Console.WriteLine("Presione cualquier tecla.");
    Console.ReadKey();
    return result?.AccessToken;
}

async Task AccederEndpointProtegido(string? token)
{
    if (string.IsNullOrEmpty(token))
    {
        Console.WriteLine("Debe loguearse primero.");
        Console.WriteLine("Presione cualquier tecla.");
        Console.ReadKey();
        return;
    }

    using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/secure/test");
    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await httpClient.SendAsync(requestMessage);
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Respuesta protegida: " + result);
        Console.ReadKey();
    }
    else
    {
        Console.WriteLine($"Error al acceder al recurso protegido: {response.StatusCode}");
        string details = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Detalles: " + details);
        Console.ReadKey();
    }
}

async Task VerInformacionSesion(string? token)
{
    if (string.IsNullOrEmpty(token))
    {
        Console.WriteLine("Debe loguearse primero.");
        Console.WriteLine("Presione cualquier tecla.");
        Console.ReadKey();
        return;
    }

    using var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/api/auth/session-info");
    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await httpClient.SendAsync(requestMessage);
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Información de sesión:");
        Console.WriteLine(result);
        Console.ReadKey();
    }
    else
    {
        Console.WriteLine($"Error al obtener información de sesión: {response.StatusCode}");
        string details = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Detalles: " + details);
        Console.ReadKey();
    }
}

async Task Logout(string? token)
{
    if (string.IsNullOrEmpty(token))
    {
        Console.WriteLine("No hay sesión activa.");
        Console.WriteLine("Presione cualquier tecla.");
        Console.ReadKey();
        return;
    }

    using var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/api/auth/logout");
    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

    var response = await httpClient.SendAsync(requestMessage);
    if (response.IsSuccessStatusCode)
    {
        var result = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Logout exitoso: " + result);
        Console.ReadKey();
    }
    else
    {
        Console.WriteLine($"Error al hacer logout: {response.StatusCode}");
        string details = await response.Content.ReadAsStringAsync();
        Console.WriteLine("Detalles: " + details);
        Console.ReadKey();
    }
}

record TokenResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = "";
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = "";
}
