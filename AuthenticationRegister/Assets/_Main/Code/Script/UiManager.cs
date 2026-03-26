using System.Collections;
using TMPro;
using UnityEngine;

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
    //[SerializeField] private TMP_InputField newScoreInputField;

    [Header("Project UI")]
    [SerializeField] private TMP_Text projectNameText;
    [SerializeField] private GameObject mensajeInicioInvalido;

    [Header("Managers")]
    [SerializeField] private AuthManager authManager;
    [SerializeField] private UserManager userManager;
    [SerializeField] private RankingManager rankingManager;

    private void Awake()
    {
        //projectNameText.text = "Tu Nombre Completo - Proyecto API Unity";
    }

    public void OnClickLogin()
    {
        string username = usernameInputField.text.Trim();
        string password = passwordInputField.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            StartCoroutine(SetStatus("Completa usuario y contraseña."));
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
            StartCoroutine(SetStatus("Completa usuario y contraseña."));
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
            StartCoroutine(SetStatus("No hay usuario logueado."));
            return;
        }

        SessionData.CurrentUser.score += 50;

        UpdateScoreText(SessionData.CurrentUser.score);
    }

    public void OnClickSaveScore()
    {
        if (SessionData.CurrentUser == null)
        {
            StartCoroutine(SetStatus("No hay usuario logueado."));
            return;
        }

        userManager.UpdateScore(SessionData.CurrentUser.score);
    }

    public void OnClickShowRanking()
    {
        rankingManager.LoadRanking();
    }

    public void OnClickBackToHome()
    {
        if (SessionData.CurrentUser != null)
        {
            ShowHomePanel(SessionData.CurrentUser.username, SessionData.CurrentUser.score);
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

    public IEnumerator SetStatus(string message)
    {
        statusText.gameObject.SetActive(true);
        statusText.text = message;
        yield return new WaitForSeconds(1.5f);
        statusText.gameObject.SetActive(false);
    }
    public IEnumerator InvalidStart()
    {
        mensajeInicioInvalido.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        mensajeInicioInvalido.gameObject.SetActive(false);
    }
}