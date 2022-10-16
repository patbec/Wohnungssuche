using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Wohnungssuche
{
  public class Program
  {
    /// <summary>
    /// Maximale Anzahl an Folgefehlern, bis die Anwendung beendet wird.
    /// </summary>
    const ulong MAX_ERROR_THRESHOLD = 15;

    static HttpClient httpClient;
    static MailClient mailClient;
    static string htmlDocumentAppException;
    static string htmlDocumentItemFound;

    static async Task Main()
    {
      httpClient = new HttpClient();

      // Get the smtp settings from the environment.
      mailClient = MailClient.CreateFromEnvironment();

      // Cache mailing templates from filesystem.
      htmlDocumentItemFound = Helper.GetRessource(Path.Combine("templates", "item.html"));
      htmlDocumentAppException = Helper.GetRessource(Path.Combine("templates", "error.html"));

      try
      {
        Console.WriteLine($"Stadtbau Wohnungssuche - " + Helper.GetVersion());
        Console.WriteLine($"Die Wohnungssuche wurde gestartet...");

        await Searching();
      }
      catch (OperationCanceledException) { }
      catch (Exception exception)
      {
        SendExceptionReport(exception);
        Console.WriteLine($"Die Wohnungssuche wurde aufgrund von zu vielen Fehlern gestoppt: {exception}");
      }
      finally
      {
        Console.WriteLine($"Die Wohnungssuche wurde beendet.");
      }
    }

    /// <summary>
    /// Search for flats.
    /// </summary>
    static async Task Searching()
    {
      ulong errors = 0;

      while (true)
      {
        try
        {
          await foreach (Wohnung item in GetItems())
          {
            if (!Cache.IsKnown(item))
            {
              Console.WriteLine("Es wurde eine neue Wohnung gefunden!");

              byte[] image = await httpClient.GetByteArrayAsync(item.Thumb);
              string base64EmbeddedPreviewImage = Convert.ToBase64String(image);

              string messageToSend = htmlDocumentItemFound
                  .ReplaceHtmlEncoded("@id", item.Id)
                  .ReplaceHtmlEncoded("@title", item.Title ?? "Es ist kein Titel vorhanden")
                  .ReplaceHtmlEncoded("@base64image", base64EmbeddedPreviewImage)
                  .ReplaceHtmlEncoded("@rooms", item.Rooms ?? "Unbekannt")
                  .ReplaceHtmlEncoded("@price", item.Price ?? "Unbekannt")
                  .ReplaceHtmlEncoded("@size", item.Size ?? "Unbekannt")
                  .ReplaceHtmlEncoded("@date", item.Date ?? "Unbekannt");

              mailClient.Send("Neue Wohnung gefunden", messageToSend);
              Cache.MakeKnown(item);
            }
          }

          Console.WriteLine("Suchvorgang wurde erfolgreich abgeschlossen.");

          errors = 0;

          await Task.Delay(TimeSpan.FromSeconds(600));
        }
        catch (Exception ex)
        {
          Console.WriteLine("Bei der Suche nach Wohnungen ist ein Fehler aufgetreten:" + Environment.NewLine + ex.Message);

          errors += 1;

          // Rethrow if threshold is reached.
          if (errors >= MAX_ERROR_THRESHOLD) throw;
        }
      }
    }

    /// <summary>
    /// Ruft eine Auflistung von Wohnungen ab. Die Einstellungen sowie Parameter werden im Konstruktor festgelegt.
    /// </summary>
    /// <returns>Auflistung von verfügbaren Wohnungen, die den Suchkriterien entsprechen.</returns>
    static async IAsyncEnumerable<Wohnung> GetItems()
    {
      string SearchBaseUri = "https://www.stadtbau-wuerzburg.de/wohnungssuche/";
      string XPathQuery = "//div[@class='immo-preview-group asidemain-container' and a[not(@class='immo-pinnded-listing-link')]]";


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
        yield return Wohnung.Parse(node);
      }
    }

    static void SendExceptionReport(Exception exception)
    {
      try
      {
        string messageToSend = htmlDocumentAppException
            .ReplaceHtmlEncoded("@threadshold", MAX_ERROR_THRESHOLD)
            .ReplaceHtmlEncoded("@message", exception.Message)
            .ReplaceHtmlEncoded("@stackwalk", exception.StackTrace);

        mailClient.Send("Anwendungsfehler", messageToSend);
      }
      catch (Exception innerException)
      {
        Console.WriteLine("Die Fehlermeldung konnte nicht per Mail versendet werden: " + Environment.NewLine + innerException.Message);
      }
    }
  }
}
