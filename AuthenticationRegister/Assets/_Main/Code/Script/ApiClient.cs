using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour
{
    [SerializeField] private string baseUrl = "https://sid-restapi.onrender.com";

    public IEnumerator PostJson<TRequest>(
        string endpoint,
        TRequest body,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string json = JsonUtility.ToJson(body);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();
        HandleResponse(request, onSuccess, onError);
    }

    public IEnumerator PatchJson<TRequest>(
        string endpoint,
        TRequest body,
        string token,
        Action<string> onSuccess,
        Action<string> onError)
    {
        string json = JsonUtility.ToJson(body);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "PATCH");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-token", token);

        yield return request.SendWebRequest();
        HandleResponse(request, onSuccess, onError);
    }

    public IEnumerator Get(
        string endpoint,
        string token,
        Action<string> onSuccess,
        Action<string> onError)
    {
        using UnityWebRequest request = UnityWebRequest.Get(baseUrl + endpoint);

        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("x-token", token);
        }

        yield return request.SendWebRequest();
        HandleResponse(request, onSuccess, onError);
    }

    private void HandleResponse(
        UnityWebRequest request,
        Action<string> onSuccess,
        Action<string> onError)
    {
        bool success = request.result == UnityWebRequest.Result.Success &&
                       request.responseCode >= 200 &&
                       request.responseCode < 300;

        if (success)
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
        else
        {
            string error =
                $"HTTP {request.responseCode}\n" +
                $"Unity error: {request.error}\n" +
                $"Body: {request.downloadHandler.text}";
            onError?.Invoke(error);
        }
    }
}