using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace LibraryApp.Services;

public interface IToastService
{
    void Success(string title, string message, int duration = 5000);
    void Error(string title, string message, int duration = 7000);
    void Warning(string title, string message, int duration = 6000);
    void Info(string title, string message, int duration = 5000);
}

public class ToastService : IToastService
{
    private readonly ITempDataDictionary _tempData;

    public ToastService(ITempDataDictionary tempData)
    {
        _tempData = tempData;
    }

    public void Success(string title, string message, int duration = 5000)
    {
        AddToast("success", title, message, duration);
    }

    public void Error(string title, string message, int duration = 7000)
    {
        AddToast("error", title, message, duration);
    }

    public void Warning(string title, string message, int duration = 6000)
    {
        AddToast("warning", title, message, duration);
    }

    public void Info(string title, string message, int duration = 5000)
    {
        AddToast("info", title, message, duration);
    }

    private void AddToast(string type, string title, string message, int duration)
    {
        var toasts = GetToasts();
        toasts.Add(new ToastMessage
        {
            Type = type,
            Title = title,
            Message = message,
            Duration = duration
        });
        
        _tempData["ToastMessages"] = System.Text.Json.JsonSerializer.Serialize(toasts);
    }

    private List<ToastMessage> GetToasts()
    {
        if (_tempData["ToastMessages"] is string json && !string.IsNullOrEmpty(json))
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<ToastMessage>>(json) ?? new List<ToastMessage>();
            }
            catch
            {
                return new List<ToastMessage>();
            }
        }
        return new List<ToastMessage>();
    }
}

public class ToastMessage
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Duration { get; set; } = 5000;
}

// Extension methods for easier usage
public static class ToastExtensions
{
    public static void AddToast(this Controller controller, string type, string title, string message, int duration = 5000)
    {
        var toasts = GetToasts(controller.TempData);
        toasts.Add(new ToastMessage
        {
            Type = type,
            Title = title,
            Message = message,
            Duration = duration
        });
        
        controller.TempData["ToastMessages"] = System.Text.Json.JsonSerializer.Serialize(toasts);
    }

    public static void AddSuccess(this Controller controller, string title, string message, int duration = 5000)
    {
        controller.AddToast("success", title, message, duration);
    }

    public static void AddError(this Controller controller, string title, string message, int duration = 7000)
    {
        controller.AddToast("error", title, message, duration);
    }

    public static void AddWarning(this Controller controller, string title, string message, int duration = 6000)
    {
        controller.AddToast("warning", title, message, duration);
    }

    public static void AddInfo(this Controller controller, string title, string message, int duration = 5000)
    {
        controller.AddToast("info", title, message, duration);
    }

    private static List<ToastMessage> GetToasts(ITempDataDictionary tempData)
    {
        if (tempData["ToastMessages"] is string json && !string.IsNullOrEmpty(json))
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<ToastMessage>>(json) ?? new List<ToastMessage>();
            }
            catch
            {
                return new List<ToastMessage>();
            }
        }
        return new List<ToastMessage>();
    }
}