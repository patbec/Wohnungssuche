using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Internal;

namespace Wohnungssuche
{
  public class Program
  {
    static async Task Main(string[] args)
    {
      try
      {
        Console.WriteLine($"Stadtbau Wohnungssuche - " + GetCurrentVersion());
        Console.WriteLine($"Die Wohnungssuche wurde gestartet...");

        await Searching();
      }
      catch (OperationCanceledException) { }
      catch (Exception ex)
      {
        SendExceptionMail(ex);
        Console.WriteLine($"Die Wohnungssuche wurde aufgrund von zu vielen Fehlern beendet: {ex}");
      }
      finally
      {
        Console.WriteLine($"Die Wohnungssuche wurde beendet.");
      }
    }

    private void SendExceptionMail(Exception ex)
    {
      StringWriter messageToSend = new();

      messageToSend.WriteLine("<p style='font-family: system-ui'>Die Wohnungssuche wurde aufgrund von zu vielen Fehlern beendet.</p>");
      messageToSend.WriteLine();
      messageToSend.WriteLine("<p style='font-family: monospace'>{0}</p>", ex);

      bool isSucessfully = Mailing.TrySend("Anwendungsfehler", messageToSend.ToString());
      if (!isSucessfully)
      {
        Console.WriteLine("Die Fehlermeldung konnte nicht per Mail versendet werden.");
      }
    }

    /// <summary>
    /// Search for flats.
    /// </summary>
    static async Task Searching()
    {
      ulong errors = 0;
      ulong threshold = 15;

      while (true)
      {
        try
        {
          foreach (Wohnung item in await GetItems())
          {
            if (!Cache.IsKnown(item))
            {
              Console.WriteLine("Es wurde eine neue Wohnung gefunden!");

              // Nachricht als HTML formatieren.
              string messageToSend = htmlBody
                  .Replace("@id", WebUtility.HtmlEncode(e.Id))
                  .Replace("@title", WebUtility.HtmlEncode(e.Title))
                  .Replace("@image", WebUtility.HtmlEncode(e.Thumb))
                  .Replace("@rooms", WebUtility.HtmlEncode(e.Rooms))
                  .Replace("@price", WebUtility.HtmlEncode(e.Price))
                  .Replace("@size", WebUtility.HtmlEncode(e.Size))
                  .Replace("@date", WebUtility.HtmlEncode(e.Date));

              Mailing.Send("Neue Wohnung gefunden", messageToSend);
              Cache.MakeKnown(item);
            }
          }

          Console.WriteLine("Suchvorgang wurde erfolgreich abgeschlossen.");

          errors = 0;

          await Task.Delay(TimeSpan.FromSeconds(600));
        }
        catch (Exception ex)
        {
          Console.WriteLine("Bei der Suche nach Wohnungen ist ein Fehler aufgetreten:" + Environment.NewLine + e.Message);

          errors += 1;

          // Rethrow if threshold is reached.
          if (_current >= _threshold) throw ex;
        }
      }
    }

    /// <summary>
    /// Ruft eine Auflistung von Wohnungen ab. Die Einstellungen sowie Parameter werden im Konstruktor festgelegt.
    /// </summary>
    /// <returns>Auflistung von verfügbaren Wohnungen, die den Suchkriterien entsprechen.</returns>
    static async IAsyncEnumerator<Wohnung> GetItems()
    {
      string SearchBaseUri = "https://www.stadtbau-wuerzburg.de/wohnungssuche/";
      string XPathQuery = "//div[@class='immo-preview-group asidemain-container']";


      // Anfrage senden.
      string response = await new HttpClient().GetStringAsync(SearchBaseUri);

      // Neues Dokument erstellen.
      HtmlDocument pageDocument = new();

      // Serverantwort als HTML darstellen.
      pageDocument.LoadHtml(response);

      // Mit XPath verfügbaren Wohnungen auslesen.
      HtmlNodeCollection nodes = pageDocument.DocumentNode.SelectNodes(XPathQuery);

      // Falls keine Wohnungen vorhanden sind, Abbrechen.
      if (nodes == null)
        yield break;

      // Auflistung von gefundenen Wohnungen.
      foreach (HtmlNode node in nodes)
      {
        // Aus der Teilzeichenfolge ein Wohnungsobjekt erstellen und zurückgeben.
        yield return ApartmentElement.Parse(node);
      }
    }

    static Version GetCurrentVersion()
    {
      return Assembly.GetExecutingAssembly().GetName().Version;
    }
  }
}
