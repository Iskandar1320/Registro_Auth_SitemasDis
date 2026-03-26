using System.Collections;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    [SerializeField] private ApiClient apiClient;
    [SerializeField] private UIManager uiManager;

    public void UpdateScore(int newScore)
    {
        if (!SessionData.IsLoggedIn() || SessionData.CurrentUser == null)
        {
            StartCoroutine(uiManager.SetStatus("Debes iniciar sesión."));
            uiManager.ShowAuthPanel();
            return;
        }

        StartCoroutine(UpdateScoreCoroutine(newScore));
    }

    public IEnumerator GetProfileCoroutine(string username)
    {
        yield return apiClient.Get(
            "/api/usuarios/" + username,
            SessionData.Token,
            onSuccess: (System.Action<string>)((json) =>
            {
                UserResponse response = JsonUtility.FromJson<UserResponse>(json);

                if (response == null || response.usuario == null)
                {
                    uiManager.ShowAuthPanel();
                    StartCoroutine(uiManager.SetStatus("No se pudo restaurar la sesión."));
                    return;
                }

                SessionData.CurrentUser = response.usuario;
                uiManager.ShowHomePanel(response.usuario.username, response.usuario.score);
                StartCoroutine(uiManager.SetStatus("Sesión restaurada."));
            }),
            onError: (System.Action<string>)((error) =>
            {
                SessionData.Clear();
                SessionStorage.Clear();
                uiManager.ShowAuthPanel();
                StartCoroutine(uiManager.SetStatus("Tu sesión expiró. Inicia sesión de nuevo."));
                Debug.LogError(error);
            }));
    }

    private IEnumerator UpdateScoreCoroutine(int newScore)
    {
        UpdateUserRequest requestData = new UpdateUserRequest
        {
            username = SessionData.CurrentUser.username,
            data = new UserUpdateData
            {
                score = newScore
            }
        };

        yield return apiClient.PatchJson(
            "/api/usuarios",
            requestData,
            SessionData.Token,
            onSuccess: (System.Action<string>)((json) =>
            {
                SessionData.CurrentUser.score = newScore;
                uiManager.UpdateScoreText(newScore);
                StartCoroutine(uiManager.SetStatus("Score actualizado en el servidor."));
                Debug.Log("Update OK: " + json);
            }),
            onError: (System.Action<string>)((error) =>
            {
                if (error.Contains("401"))
                {
                    SessionData.Clear();
                    SessionStorage.Clear();
                    uiManager.ShowAuthPanel();
                }

                StartCoroutine(uiManager.SetStatus("Error actualizando score:\n" + error));
                Debug.LogError(error);
            }));
    }
}