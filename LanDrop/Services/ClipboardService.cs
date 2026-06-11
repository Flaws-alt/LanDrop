using System.Windows;

namespace LanDrop.Services;

public class ClipboardService
{
    public bool HasText()
    {
        try
        {
            return Clipboard.ContainsText() && !string.IsNullOrWhiteSpace(Clipboard.GetText());
        }
        catch
        {
            return false;
        }
    }

    public string GetText()
    {
        try
        {
            return Clipboard.GetText();
        }
        catch
        {
            return string.Empty;
        }
    }
}
