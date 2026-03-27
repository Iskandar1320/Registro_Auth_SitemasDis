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
            uiManager.ShowStatus("Debes iniciar sesión.", MessageType.Error);
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
            onSuccess: (json) =>
            {
                UserResponse response = JsonUtility.FromJson<UserResponse>(json);

                if (response == null || response.usuario == null)
                {
                    uiManager.ShowAuthPanel();
                    uiManager.ShowStatus("No se pudo restaurar la sesión.", MessageType.Error);
                    return;
                }

                SessionData.CurrentUser = response.usuario;
                uiManager.ShowHomePanel(
                    response.usuario.username,
                    response.usuario.data.score
                );

                uiManager.ShowStatus("Sesión restaurada.", MessageType.Info);
            },
            onError: (error) =>
            {
                SessionData.Clear();
                SessionStorage.Clear();
                uiManager.ShowAuthPanel();
                uiManager.ShowStatus("Tu sesión expiró. Inicia sesión de nuevo.", MessageType.Error);
                Debug.LogError(error);
            });
    }

    private IEnumerator UpdateScoreCoroutine(int newScore)
    {
        UpdateUserRequest requestData = new UpdateUserRequest
        {
            username = SessionData.CurrentUser.username,
            data = new UserData
            {
                score = newScore
            }
        };

        yield return apiClient.PatchJson(
            "/api/usuarios",
            requestData,
            SessionData.Token,
            onSuccess: (json) =>
            {
                SessionData.CurrentUser.data.score = newScore;
                uiManager.UpdateScoreText(newScore);
                uiManager.ShowStatus("Score actualizado en el servidor.", MessageType.Success);
                Debug.Log("Update OK: " + json);
            },
            onError: (error) =>
            {
                if (error.Contains("401"))
                {
                    SessionData.Clear();
                    SessionStorage.Clear();
                    uiManager.ShowAuthPanel();
                }

                uiManager.ShowStatus("Error actualizando score:\n" + error, MessageType.Error);
                Debug.LogError(error);
            });
    }
}