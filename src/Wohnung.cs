﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
    private const string ImmoTitle = "immodb_title";
    private const string ImmoThumb = "immo-prev-thumb module";
    private const string ImmoObject = "immo-preview-txt module";
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
    /// <param name="titel">Kurze Zusammenfassung der Wohnung.</param>
    /// <param name="price">Mietpreis ohne Nebenkosten.</param>
    /// <param name="size">Größe des Wohnraumes in m².</param>
    /// <param name="rooms">Anzahl von Zimmern.</param>
    /// <param name="thumb">Vorschaubild der Wohnung.</param>
    /// <param name="date">Datum ab wann die Wohnung verfügbar ist.</param>
    public Wohnung(long id, string titel, string price, string size, string rooms, string thumb, string date)
    {
      Id = id;
      Titel = titel;
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
          Titel.PadRight(70) + Environment.NewLine +
          "       " + Rooms.PadRight(35) + LivingSpace.PadRight(35) + Environment.NewLine +
          "       " + Price.PadRight(35) + Available.PadRight(35) + Environment.NewLine;
    }

    /// <summary>
    /// Gibt eine Zeichenfolge mit den gesetzten Eigenschaften als HTML-Element zurück.
    /// </summary>
    /// <returns>Zeichenfolge mit gesetzten Eigenschaften der Wohnung.</returns>
    public string ToString(bool enableHtml)
    {
      if (!enableHtml)
        return ToString();

      return
          $"<b><a href=\"{Id}\">{Titel}</a></b><br>" +
          $"<br>Preis:<br>{Price}<br>" +
          $"<br>Wohnraum:<br>{LivingSpace} / {Rooms}<br>" +
          $"<br>Verfügbar ab: {Available}<br>" +
          $"<img src=\"{Thumb}\"></img>";

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
      HtmlNode nodeAvailable = itemsContent.GetNodeByTextValue(ImmoAvailable)?.NextSibling?.NextSibling;

      // Anzahl der Zimmer Abrufen.
      HtmlNode nodeRooms = itemsContent.GetNodeByTextValue(ImmoRoomCount)?.NextSibling?.NextSibling;

      // Mietpreis Abrufen.
      HtmlNode nodePrice = itemsContent.GetNodeByTextValue(ImmoPrice)?.NextSibling?.NextSibling;

      // Wohnraum in m² Abrufen.
      HtmlNode nodeSize = itemsContent.GetNodeByTextValue(ImmoSize)?.NextSibling?.NextSibling;

      string title = WebUtility.HtmlDecode(nodeObject?.InnerText ?? "Kein Titel vorhanden").Trim();
      string price = WebUtility.HtmlDecode(nodePrice?.InnerText ?? "Unbekannt");
      string size = WebUtility.HtmlDecode(nodeSize?.InnerText ?? "Unbekannt");
      string rooms = WebUtility.HtmlDecode(nodeRooms?.InnerText ?? "Unbekannt");

      string available = WebUtility.HtmlDecode(nodeAvailable?.InnerText ?? "Unbekannt").Trim();

      string thumb = WebUtility.HtmlDecode(nodeThumb.Attributes[0].Value);
      string link = WebUtility.HtmlDecode(nodeObject.Attributes[0].Value);

      // Neues Wohnungsobjekt erstellen und zurückgeben.
      return new Wohnung(
          link,
          title,
          price,
          size,
          rooms,
          thumb,
          available);
    }

    #endregion
  }
}
