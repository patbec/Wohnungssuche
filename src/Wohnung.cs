using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using HtmlAgilityPack;

namespace Wohnungssuche
{
  /// <summary>
  /// Object that represents a flat.
  /// </summary>
  public class Wohnung
  {
    #region Parser Data

    private const string ImmoIdentifier = "immo-preview-group asidemain-container";

    // private const string ImmoTitle = "immodb_title";
    // private const string ImmoThumb = "immo-prev-thumb module";
    // private const string ImmoObject = "immo-preview-txt module";

    private const string ImmoData = "immo-data";
    private const string ImmoAvailable = "Verfügbar ab:";
    private const string ImmoRoomCount = "Zimmeranzahl: ";
    private const string ImmoPrice = "Kaltmiete:";
    private const string ImmoSize = "Gr&ouml;&szlig;e:";

    #endregion

    #region Properties

    /// <summary>
    /// Id zur Wohnung.
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// Kurze Zusammenfassung der Wohnung.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Mietpreis ohne Nebenkosten.
    /// </summary>
    public string Price { get; }

    /// <summary>
    /// Größe des Wohnraumes in m².
    /// </summary>
    public string Size { get; }

    /// <summary>
    /// Anzahl von Räumen.
    /// </summary>
    public string Rooms { get; }

    /// <summary>
    /// Link zum Vorschaubild der Wohnung.
    /// </summary>
    public string Thumb { get; }

    /// <summary>
    /// Datum ab wann die Wohnung verfügbar ist.
    /// </summary>
    public string Date { get; }

    #endregion

    /// <summary>
    /// Erstellt ein neues Wohnungsobjekt mit den angegebenen Eigenschaften.
    /// </summary>
    /// <param name="id">Id der Expose.</param>
    /// <param name="title">Kurze Zusammenfassung der Wohnung.</param>
    /// <param name="price">Mietpreis ohne Nebenkosten.</param>
    /// <param name="size">Größe des Wohnraumes in m².</param>
    /// <param name="rooms">Anzahl von Zimmern.</param>
    /// <param name="thumb">Vorschaubild der Wohnung.</param>
    /// <param name="date">Datum ab wann die Wohnung verfügbar ist.</param>
    public Wohnung(long id, string title, string price, string size, string rooms, string thumb, string date)
    {
      Id = id;
      Title = title;
      Price = price;
      Size = size;
      Rooms = rooms;
      Thumb = thumb;
      Date = date;
    }

    /// <summary>
    /// Gibt eine Zeichenfolge mit den gesetzten Eigenschaften zurück.
    /// </summary>
    /// <returns>Zeichenfolge mit gesetzten Eigenschaften der Wohnung.</returns>
    public override string ToString()
    {
      return
          Title.PadRight(70) + Environment.NewLine +
          "       " + Rooms.PadRight(35) + Size.PadRight(35) + Environment.NewLine +
          "       " + Price.PadRight(35) + Date.PadRight(35) + Environment.NewLine;
    }

    #region Static

    /// <summary>
    /// Konvertiert eine Teilzeichenfolge der zurückgegebenen Antwort der API und erstellt ein Wohnungsobjekt.
    /// </summary>
    /// <param name="node">Teilzeichenfolge der zurückgegebenen Antwort der API.</param>
    /// <returns>Das erstellte Wohnungsobjekt.</returns>
    public static Wohnung Parse(HtmlNode node)
    {
      if (node is null)
        throw new ArgumentNullException(nameof(node));

      if (node.GetAttributeValue("class", null) != ImmoIdentifier)
        throw new ArgumentException("The apartment data could not be read out because the input data has an incorrect format.", new InvalidDataException("The class does not contain an expected attribute."));


      HtmlNodeCollection items = node.ChildNodes;

      // Vorschaubild und Abrufen.
      HtmlNode nodeThumb = items.GetNodeByType("src");

      // Hyperlink und Titel der Wohnung Abrufen.
      HtmlNode nodeObject = items.GetNodeByType("href");

      // Details der Wohnung Abrufen.
      HtmlNode nodeContent = items.GetNodeByAttribute(ImmoData, true);

      // Ausnahme wenn Inhaltsknoten nicht gefunden.
      if (nodeContent == null || nodeObject == null)
        throw new NodeAttributeNotFoundException();


      // Erweiterte Inhaltsbeschreibung Abrufen.
      HtmlNodeCollection itemsContent = nodeContent.ChildNodes;

      // Verfügbarkeit der Wohnung Abrufen.
      HtmlNode nodeDate = itemsContent.GetNodeByTextValue(ImmoAvailable)?.NextSibling?.NextSibling;

      // Anzahl der Zimmer Abrufen.
      HtmlNode nodeRooms = itemsContent.GetNodeByTextValue(ImmoRoomCount)?.NextSibling?.NextSibling;

      // Mietpreis Abrufen.
      HtmlNode nodePrice = itemsContent.GetNodeByTextValue(ImmoPrice)?.NextSibling?.NextSibling;

      // Wohnraum in m² Abrufen.
      HtmlNode nodeSize = itemsContent.GetNodeByTextValue(ImmoSize)?.NextSibling?.NextSibling;

      string title = WebUtility.HtmlDecode(nodeObject?.InnerText?.Trim() ?? null);
      string price = WebUtility.HtmlDecode(nodePrice?.InnerText ?? null);
      string size = WebUtility.HtmlDecode(nodeSize?.InnerText ?? null);
      string rooms = WebUtility.HtmlDecode(nodeRooms?.InnerText ?? null);

      string date = WebUtility.HtmlDecode(nodeDate?.InnerText?.Trim() ?? null);

      string thumb = WebUtility.HtmlDecode(nodeThumb.Attributes[0].Value);
      string link = WebUtility.HtmlDecode(nodeObject.Attributes[0].Value);

      long id = GetObjectIdFromUrl(link);

      // Neues Wohnungsobjekt erstellen und zurückgeben.
      return new Wohnung(
          id,
          title,
          price,
          size,
          rooms,
          thumb,
          date);
    }

    /// <summary>
    /// Gibt den Parameter object_id aus dem Weblink zurück.
    ///
    /// bsp.: Bei diesem Link wird die Zahl 633 zurückgegeben.
    /// https://www.stadtbau-wuerzburg.de/wohnungsdetail/?object_id=633
    /// </summary>
    /// <param name="weblink">Weblink zur Stadtbauseite.</param>
    /// <returns>Eindeutige Wohnungs-Id bei Stadtbau.</returns>
    /// <exception cref="FormatException">Die Wohnungs-Id konnte nicht ausgelesen werden.</exception>
    /// <exception cref="ArgumentNullException">Weblink ist nicht angegeben.</exception>
    private static long GetObjectIdFromUrl(string weblink)
    {
      if (string.IsNullOrEmpty(weblink))
      {
        throw new ArgumentNullException(nameof(weblink));
      }

      try
      {
        string linkQuery = new Uri(weblink).Query;
        string linkParameter = HttpUtility.ParseQueryString(linkQuery).Get("object_id");

        return long.Parse(linkParameter);
      }
      catch (Exception innerException)
      {
        throw new FormatException("Parameter object_id could not be read from the web link.", innerException);
      }
    }


    #endregion
  }
}
