using System.Collections;
using TMPro;
using UnityEngine;

public enum MessageType
{
    Info,
    Success,
    Error
}

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject authPanel;
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject rankingPanel;

    [Header("Auth UI")]
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_Text statusText;

    [Header("Home UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text usernameHomeText;

    [Header("Project UI")]
    [SerializeField] private TMP_Text projectNameText;
    [SerializeField] private GameObject mensajeInicioInvalido;

    [Header("Managers")]
    [SerializeField] private AuthManager authManager;
    [SerializeField] private UserManager userManager;
    [SerializeField] private RankingManager rankingManager;

    private void Awake()
    {
        // projectNameText.text = "Tu Nombre Completo - Proyecto API Unity";
    }

    public void OnClickLogin()
    {
        string username = usernameInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Completa usuario y contraseña.", MessageType.Error);
            StartCoroutine(InvalidStart());
            return;
        }

        authManager.Login(username, password);
    }

    public void OnClickRegister()
    {
        string username = usernameInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            ShowStatus("Completa usuario y contraseña.", MessageType.Error);
            return;
        }

        authManager.Register(username, password);
    }

    public void OnClickLogout()
    {
        authManager.Logout();
    }

    public void OnClickScore()
    {
        if (SessionData.CurrentUser == null)
        {
            ShowStatus("No hay usuario logueado.", MessageType.Error);
            return;
        }

        SessionData.CurrentUser.data.score += 50;
        UpdateScoreText(SessionData.CurrentUser.data.score);
    }

    public void OnClickSaveScore()
    {
        if (SessionData.CurrentUser == null)
        {
            ShowStatus("No hay usuario logueado.", MessageType.Error);
            return;
        }

        userManager.UpdateScore(SessionData.CurrentUser.data.score);
    }

    public void OnClickShowRanking()
    {
        rankingManager.LoadRanking();
    }

    public void OnClickBackToHome()
    {
        if (SessionData.CurrentUser != null)
        {
            ShowHomePanel(
                SessionData.CurrentUser.username,
                SessionData.CurrentUser.data.score
            );
        }
        else
        {
            ShowAuthPanel();
        }
    }

    public void ShowAuthPanel()
    {
        authPanel.SetActive(true);
        homePanel.SetActive(false);
        rankingPanel.SetActive(false);
    }

    public void ShowHomePanel(string username, int score)
    {
        authPanel.SetActive(false);
        homePanel.SetActive(true);
        rankingPanel.SetActive(false);

        usernameHomeText.text = username;
        scoreText.text = score.ToString();
    }

    public void ShowRankingPanel()
    {
        authPanel.SetActive(false);
        homePanel.SetActive(false);
        rankingPanel.SetActive(true);
    }

    public void UpdateScoreText(int score)
    {
        scoreText.text = score.ToString();
    }

    public void ShowStatus(string message, MessageType type)
    {
        StartCoroutine(SetStatus(message, type));
    }

    private IEnumerator SetStatus(string message, MessageType type)
    {
        statusText.gameObject.SetActive(true);
        statusText.text = message;

        switch (type)
        {
            case MessageType.Success:
                statusText.color = Color.green;
                break;

            case MessageType.Error:
                statusText.color = Color.red;
                break;

            case MessageType.Info:
            default:
                statusText.color = Color.white;
                break;
        }

        yield return new WaitForSeconds(1.5f);
        statusText.gameObject.SetActive(false);
    }

    private IEnumerator InvalidStart()
    {
        mensajeInicioInvalido.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        mensajeInicioInvalido.gameObject.SetActive(false);
    }
}