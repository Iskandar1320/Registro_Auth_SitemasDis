using System.Collections;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    [SerializeField] private ApiClient apiClient;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private UserManager userManager;

    private void Start()
    {
        RestoreSession();
    }

    public void Register(string username, string password)
    {
        StartCoroutine(RegisterCoroutine(username, password));
    }

    public void Login(string username, string password)
    {
        StartCoroutine(LoginCoroutine(username, password));
    }

    public void Logout()
    {
        SessionData.Clear();
        SessionStorage.Clear();
        uiManager.ShowAuthPanel();
        uiManager.ShowStatus("Sesión cerrada.", MessageType.Info);
    }

    private void RestoreSession()
    {
        string savedToken = SessionStorage.LoadToken();
        string savedUsername = SessionStorage.LoadUsername();

        if (string.IsNullOrEmpty(savedToken) || string.IsNullOrEmpty(savedUsername))
        {
            uiManager.ShowAuthPanel();
            return;
        }

        SessionData.Token = savedToken;
        StartCoroutine(userManager.GetProfileCoroutine(savedUsername));
    }

    private IEnumerator RegisterCoroutine(string username, string password)
    {
        RegisterRequest requestData = new RegisterRequest
        {
            username = username,
            password = password
        };

        yield return apiClient.PostJson(
            "/api/usuarios",
            requestData,
            onSuccess: (json) =>
            {
                uiManager.ShowStatus("Registro exitoso. Ahora inicia sesión.", MessageType.Success);
                Debug.Log("Registro OK: " + json);
            },
            onError: (error) =>
            {
                uiManager.ShowStatus("Error en registro:\n" + error, MessageType.Error);
                Debug.LogError(error);
            });
    }

    private IEnumerator LoginCoroutine(string username, string password)
    {
        LoginRequest requestData = new LoginRequest
        {
            username = username,
            password = password
        };

        yield return apiClient.PostJson(
            "/api/auth/login",
            requestData,
            onSuccess: (json) =>
            {
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(json);

                if (response == null || response.usuario == null || string.IsNullOrEmpty(response.token))
                {
                    uiManager.ShowStatus("No se pudo leer la respuesta del login.", MessageType.Error);
                    return;
                }

                SessionData.Token = response.token;
                SessionStorage.SaveSession(response.token, response.usuario.username);

                StartCoroutine(userManager.GetProfileCoroutine(response.usuario.username));
                uiManager.ShowStatus("Login exitoso.", MessageType.Success);
            },
            onError: (error) =>
            {
                uiManager.ShowStatus("Error en login:\n" + error, MessageType.Error);
                Debug.LogError(error);
            });
    }
}