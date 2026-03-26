using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    [SerializeField] private ApiClient apiClient;
    [SerializeField] private UIManager uiManager;

    [Header("Ranking UI")]
    [SerializeField] private Transform rankingContainer;
    [SerializeField] private GameObject rankingItemPrefab;

    public void LoadRanking()
    {
        if (!SessionData.IsLoggedIn())
        {
            StartCoroutine(uiManager.SetStatus("Debes iniciar sesión."));
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
                    StartCoroutine(uiManager.SetStatus("No se pudo leer el ranking."));
                    return;
                }

                List<User> sortedUsers = response.usuarios
                    .OrderByDescending(user => user.score)
                    .ToList();

                DrawRanking(sortedUsers);
                uiManager.ShowRankingPanel();
            },
            onError: (error) =>
            {
                Debug.LogError(error);
                StartCoroutine(uiManager.SetStatus("Error cargando ranking."));
            });
    }

    private void DrawRanking(List<User> users)
    {
        ClearRanking();

        for (int i = 0; i < users.Count; i++)
        {
            GameObject item = Instantiate(rankingItemPrefab, rankingContainer);

            TMP_Text positionText = item.transform.Find("PositionText").GetComponent<TMP_Text>();
            TMP_Text usernameText = item.transform.Find("UsernameText").GetComponent<TMP_Text>();
            TMP_Text scoreText = item.transform.Find("ScoreText").GetComponent<TMP_Text>();

            positionText.text = (i + 1).ToString();
            usernameText.text = users[i].username;
            scoreText.text = users[i].score.ToString();
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