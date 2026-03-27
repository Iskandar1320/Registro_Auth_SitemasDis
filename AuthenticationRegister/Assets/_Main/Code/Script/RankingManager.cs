using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    [SerializeField] private ApiClient apiClient;
    [SerializeField] private UIManager uiManager;

    [Header("Ranking UI")]
    [SerializeField] private Transform rankingContainer;
    [SerializeField] private RankingItemUI rankingItemPrefab;

    public void LoadRanking()
    {
        if (!SessionData.IsLoggedIn())
        {
            uiManager.ShowStatus("Debes iniciar sesión.", MessageType.Error);
            uiManager.ShowAuthPanel();
            return;
        }

        StartCoroutine(LoadRankingCoroutine());
    }

    private IEnumerator LoadRankingCoroutine()
    {
        yield return apiClient.Get(
            "/api/usuarios",
            SessionData.Token,
            onSuccess: (json) =>
            {
                Debug.Log("JSON ranking: " + json);

                UsersResponse response = JsonUtility.FromJson<UsersResponse>(json);

                if (response == null || response.usuarios == null)
                {
                    uiManager.ShowStatus("No se pudo leer el ranking.", MessageType.Error);
                    return;
                }

                List<User> sortedUsers = response.usuarios
                    .OrderByDescending(user => user.data.score)
                    .ToList();

                DrawRanking(sortedUsers);
                uiManager.ShowRankingPanel();
            },
            onError: (error) =>
            {
                Debug.LogError(error);
                uiManager.ShowStatus("Error cargando ranking.", MessageType.Error);
            });
    }

    private void DrawRanking(List<User> users)
    {
        ClearRanking();

        for (int i = 0; i < users.Count; i++)
        {
            RankingItemUI item = Instantiate(rankingItemPrefab, rankingContainer);
            item.Setup(i + 1, users[i].username, users[i].data.score);
        }
    }

    private void ClearRanking()
    {
        for (int i = rankingContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(rankingContainer.GetChild(i).gameObject);
        }
    }
}