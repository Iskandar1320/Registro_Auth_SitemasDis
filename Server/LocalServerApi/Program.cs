using Newtonsoft.Json;
using System.Net;
using System.Text;

string ip = "127.0.0.1";
int port = 1234;

List<Usuario> usuarios = new List<Usuario>
{
    new Usuario
    {
        _id = Guid.NewGuid().ToString(),
        username = "manolo",
        password = "12345",
        estado = true,
        data = new UserData { score = 100 }
    }
};

Dictionary<string, string> tokens = new Dictionary<string, string>();

HttpListener listener = new HttpListener();
listener.Prefixes.Add($"http://{ip}:{port}/");

listener.Start();
Console.WriteLine($"Servidor escuchando en http://{ip}:{port}/");

while (true)
{
    HttpListenerContext context = await listener.GetContextAsync();
    _ = HandleRequest(context);
}

async Task HandleRequest(HttpListenerContext context)
{
    HttpListenerRequest request = context.Request;
    HttpListenerResponse response = context.Response;

    Console.WriteLine($"{request.HttpMethod} {request.RawUrl}");

    try
    {
        // POST /api/usuarios -> registro
        if (request.HttpMethod == "POST" && request.RawUrl == "/api/usuarios")
        {
            string requestBody = await ReadBody(request);
            AuthData authData = JsonConvert.DeserializeObject<AuthData>(requestBody);

            if (authData == null)
            {
                await SendJson(response, 400, new ErrorMessage
                {
                    msg = "Debe enviar un objeto JSON válido",
                    field = "body"
                });
                return;
            }

            if (string.IsNullOrWhiteSpace(authData.username))
            {
                await SendJson(response, 400, new ErrorMessage
                {
                    msg = "Debe enviar el campo username",
                    field = "username"
                });
                return;
            }

            if (string.IsNullOrWhiteSpace(authData.password))
            {
                await SendJson(response, 400, new ErrorMessage
                {
                    msg = "Debe enviar el campo password",
                    field = "password"
                });
                return;
            }

            if (usuarios.Any(u => u.username == authData.username))
            {
                await SendJson(response, 400, new ErrorMessage
                {
                    msg = "Ya existe un usuario con ese username",
                    field = "username"
                });
                return;
            }

            Usuario newUser = new Usuario
            {
                _id = Guid.NewGuid().ToString(),
                username = authData.username,
                password = authData.password,
                estado = true,
                data = new UserData { score = 0 }
            };

            usuarios.Add(newUser);

            await SendJson(response, 200, new RegistroResponse(newUser));
            return;
        }

        // POST /api/auth/login -> login
        if (request.HttpMethod == "POST" && request.RawUrl == "/api/auth/login")
        {
            string requestBody = await ReadBody(request);
            AuthData authData = JsonConvert.DeserializeObject<AuthData>(requestBody);

            if (authData == null)
            {
                await SendJson(response, 400, new ErrorMessage
                {
                    msg = "Debe enviar un objeto JSON válido",
                    field = "body"
                });
                return;
            }

            Usuario userDb = usuarios.FirstOrDefault(u => u.username == authData.username);

            if (userDb == null || userDb.password != authData.password)
            {
                await SendJson(response, 400, new ErrorMessage
                {
                    msg = "Usuario o contraseña incorrectos"
                });
                return;
            }

            string token = Guid.NewGuid().ToString();
            tokens[token] = userDb.username;

            await SendJson(response, 200, new LoginResponse(userDb, token));
            return;
        }

        // GET /api/usuarios -> listar usuarios
        if (request.HttpMethod == "GET" && request.RawUrl == "/api/usuarios")
        {
            string token = request.Headers["x-token"];

            if (!IsValidToken(token, tokens))
            {
                await SendJson(response, 401, new ErrorMessage
                {
                    msg = "Token inválido o faltante"
                });
                return;
            }

            await SendJson(response, 200, new UsersResponse(usuarios));
            return;
        }

        // GET /api/usuarios/{username} -> perfil
        if (request.HttpMethod == "GET" && request.RawUrl.StartsWith("/api/usuarios/"))
        {
            string token = request.Headers["x-token"];

            if (!IsValidToken(token, tokens))
            {
                await SendJson(response, 401, new ErrorMessage
                {
                    msg = "Token inválido o faltante"
                });
                return;
            }

            string username = request.RawUrl.Replace("/api/usuarios/", "");
            Usuario userDb = usuarios.FirstOrDefault(u => u.username == username);

            if (userDb == null)
            {
                await SendJson(response, 404, new ErrorMessage
                {
                    msg = "Usuario no encontrado"
                });
                return;
            }

            await SendJson(response, 200, new RegistroResponse(userDb));
            return;
        }

        // PATCH /api/usuarios -> actualizar score
        if (request.HttpMethod == "PATCH" && request.RawUrl == "/api/usuarios")
        {
            string token = request.Headers["x-token"];

            if (!IsValidToken(token, tokens))
            {
                await SendJson(response, 401, new ErrorMessage
                {
                    msg = "Token inválido o faltante"
                });
                return;
            }

            string requestBody = await ReadBody(request);
            ScoreUpdate scoreUpdate = JsonConvert.DeserializeObject<ScoreUpdate>(requestBody);

            if (scoreUpdate == null || string.IsNullOrWhiteSpace(scoreUpdate.username))
            {
                await SendJson(response, 400, new ErrorMessage
                {
                    msg = "Petición inválida",
                    field = "username"
                });
                return;
            }

            Usuario userDb = usuarios.FirstOrDefault(u => u.username == scoreUpdate.username);

            if (userDb == null)
            {
                await SendJson(response, 404, new ErrorMessage
                {
                    msg = "Usuario no encontrado"
                });
                return;
            }

            if (scoreUpdate.data == null)
            {
                await SendJson(response, 400, new ErrorMessage
                {
                    msg = "Debe enviar data.score",
                    field = "data"
                });
                return;
            }

            userDb.data.score = scoreUpdate.data.score;

            await SendJson(response, 200, new RegistroResponse(userDb));
            return;
        }

        // endpoint no encontrado
        await SendJson(response, 404, new ErrorMessage
        {
            msg = "Endpoint no encontrado"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error interno: " + ex.Message);

        await SendJson(response, 500, new ErrorMessage
        {
            msg = "Error interno del servidor"
        });
    }
}

static async Task<string> ReadBody(HttpListenerRequest request)
{
    using StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding);
    return await reader.ReadToEndAsync();
}

static bool IsValidToken(string token, Dictionary<string, string> tokens)
{
    return !string.IsNullOrWhiteSpace(token) && tokens.ContainsKey(token);
}

static async Task SendJson(HttpListenerResponse response, int statusCode, object data)
{
    string json = JsonConvert.SerializeObject(data);
    byte[] buffer = Encoding.UTF8.GetBytes(json);

    response.StatusCode = statusCode;
    response.ContentType = "application/json";
    response.ContentLength64 = buffer.Length;

    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
    response.OutputStream.Close();
}

class AuthData
{
    public string username;
    public string password;
}

class ErrorMessage
{
    public string msg;
    public string field;
}

class UserData
{
    public int score;
}

class Usuario
{
    public string _id;
    public string username;
    public string password;
    public bool estado;
    public UserData data;
}

class UsuarioDto
{
    public string _id;
    public string username;
    public bool estado;
    public UserData data;
}

class RegistroResponse
{
    public UsuarioDto usuario;

    public RegistroResponse(Usuario usuario)
    {
        this.usuario = new UsuarioDto
        {
            _id = usuario._id,
            username = usuario.username,
            estado = usuario.estado,
            data = usuario.data
        };
    }
}

class LoginResponse
{
    public UsuarioDto usuario;
    public string token;

    public LoginResponse(Usuario usuario, string token)
    {
        this.usuario = new UsuarioDto
        {
            _id = usuario._id,
            username = usuario.username,
            estado = usuario.estado,
            data = usuario.data
        };

        this.token = token;
    }
}

class UsersResponse
{
    public List<UsuarioDto> usuarios;

    public UsersResponse(List<Usuario> usuariosDb)
    {
        usuarios = usuariosDb.Select(u => new UsuarioDto
        {
            _id = u._id,
            username = u.username,
            estado = u.estado,
            data = u.data
        }).ToList();
    }
}

class ScoreUpdate
{
    public string username;
    public UserData data;
}