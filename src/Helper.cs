using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace Wohnungssuche
{
  public static class Helper
  {
    public static string GetRessource(string fileName)
    {
      return File.ReadAllText(Path.Combine(AppContext.BaseDirectory, fileName));
    }

    public static Version GetVersion()
    {
      return Assembly.GetExecutingAssembly().GetName().Version;
    }

    /// <summary>
    /// Erweitert den Typ <see cref="string"/> um eine Funktion zum formatierten Ersetzen von Werten.
    /// </summary>
    public static string ReplaceHtmlEncoded(this string value, string oldValue, object newValue)
    {
      return value.Replace(oldValue, WebUtility.HtmlEncode(newValue.ToString()));
    }
  }
}