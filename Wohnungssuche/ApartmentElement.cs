using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;

namespace Wohnungssuche
{
    /// <summary>
    /// Objekt das eine Wohnung darstellt.
    /// </summary>
    public class ApartmentElement
    {
        #region Parser Data
        
        private const string ImmoIdentifier = "immodb_box row";
        private const string ImmoTitle      = "immodb_title";
        private const string ImmoThumb      = "wohnungBild";
        private const string ImmoContent    = "immodb_box_content";
        private const string ImmoData       = "immodb_bottom";
        private const string ImmoAvailable  = "available";
        private const string ImmoRoomCount  = "immodb_roomcount";
        private const string ImmoPrice      = "immodb_km";
        private const string ImmoSize       = "size";

        #endregion

        #region Properties

        /// <summary>
        /// Link zur Wohnung.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Kurze Zusammenfassung der Wohnung.
        /// </summary>
        public string Titel { get; }

        /// <summary>
        /// Mietpreis ohne Nebenkosten.
        /// </summary>
        public string Price { get; }

        /// <summary>
        /// Größe des Wohnraumes in m².
        /// </summary>
        public string LivingSpace { get; }

        /// <summary>
        /// Anzahl von Zimmern.
        /// </summary>
        public string Rooms { get; }

        /// <summary>
        /// Vorschaubild der Wohnung.
        /// </summary>
        public string Thumb { get; }

        /// <summary>
        /// Datum ab wann die Wohnung verfügbar ist.
        /// </summary>
        public string Available { get; }

        #endregion

        /// <summary>
        /// Erstellt ein neues Wohnungsobjekt mit den angegebenen Eigenschaften.
        /// </summary>
        /// <param name="id">Link zur Expose.</param>
        /// <param name="titel">Kurze Zusammenfassung der Wohnung.</param>
        /// <param name="price">Mietpreis ohne Nebenkosten.</param>
        /// <param name="livingSpace">Größe des Wohnraumes in m².</param>
        /// <param name="rooms">Anzahl von Zimmern.</param>
        /// <param name="thumb">Vorschaubild der Wohnung.</param>
        /// <param name="available">Datum ab wann die Wohnung verfügbar ist.</param>
        public ApartmentElement(string id, string titel, string price, string livingSpace, string rooms, string thumb, string available)
        {
            Id          = id;
            Titel       = titel;
            Price       = price;
            LivingSpace = livingSpace;
            Rooms       = rooms;
            Thumb       = thumb;
            Available   = available;
        }

        /// <summary>
        /// Gibt eine Zeichenfolge mit den gesetzten Eigenschaften zurück.
        /// </summary>
        /// <returns>Zeichenfolge mit gesetzten Eigenschaften der Wohnung.</returns>
        public override string ToString()
        {
            return
                Titel.PadRight(70)                                                  + Environment.NewLine +
                "       " + Rooms.PadRight(35)      + LivingSpace.PadRight(35)      + Environment.NewLine +
                "       " + Price.PadRight(35)      + Available.PadRight(35)        + Environment.NewLine;
        }

        /// <summary>
        /// Gibt eine Zeichenfolge mit den gesetzten Eigenschaften als HTML-Element zurück.
        /// </summary>
        /// <returns>Zeichenfolge mit gesetzten Eigenschaften der Wohnung.</returns>
        public string ToString(bool enableHtml)
        {
            if ( ! enableHtml)
                return ToString();

            return
                $"<b><a href=\"{Id}\">{Titel}</a></b><br>" +
                $"<br>Preis:<br>{Price}<br>" +
                $"<br>Wohnraum:<br>{LivingSpace} / {Rooms}<br>" +
                $"<br>Verfügbar ab: {Available}<br>" +
                $"<img src=\"{Thumb}\"></img>";
                
        }

        #region Static

        private static string GetImage(HtmlNode node) {
            try {
                var style = node.Attributes[2].Value;

                int charStart = style.IndexOf('(');
                int charEnd = style.IndexOf(')');

                if(charStart == -1 || charEnd == -1) {
                    return "";
                }

                return style.Substring(charStart + 1,
                                       charEnd - charStart - 1);
            } catch {
                return "";
            }
        }
        /// <summary>
        /// Konvertiert eine Teilzeichenfolge der zurückgegebenen Antwort der API und erstellt ein Wohnungsobjekt. 
        /// </summary>
        /// <param name="node">Teilzeichenfolge der zurückgegebenen Antwort der API.</param>
        /// <returns>Das erstellte Wohnungsobjekt.</returns>
        public static ApartmentElement Parse(HtmlNode node)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            if (node.GetAttributeValue("class", null) != ImmoIdentifier)
                throw new ArgumentException("The apartment data could not be read out because the input data has an incorrect format.", new InvalidDataException("The class does not contain an expected attribute."));


            HtmlNodeCollection items = node.ChildNodes;

            // Vorschaubild Abrufen.
            HtmlNode nodeThumb = items.GetNodeByAttribute(ImmoThumb)?.FirstChild;

            // Details der Wohnung Abrufen.
            HtmlNode nodeContent = items.GetNodeByAttribute(ImmoContent);

            // Ausnahme wenn Inhaltsknoten nicht gefunden.
            if(nodeContent == null)
                throw new NodeAttributeNotFoundException();

            // Erweiterte Inhaltsbeschreibung Abrufen.
            HtmlNodeCollection itemsContent = nodeContent.ChildNodes;

            // Id der Wohnung Abrufen.
            HtmlNode nodeId = nodeContent.LastChild;

            // Titel der Wohnung Abrufen.
            HtmlNode nodeTitle = itemsContent.GetNodeByAttribute(ImmoTitle)?.FirstChild;

            // Verfügbarkeit der Wohnung Abrufen.
            HtmlNode nodeAvailable = itemsContent.GetNodeByAttribute(ImmoAvailable)?.FirstChild?.NextSibling;

            // Zimmeranzahl, Mietpreis und Größe Abrufen.
            HtmlNodeCollection nodeData = itemsContent.GetNodeByAttribute(ImmoData)?.ChildNodes;

            // Anzahl der Zimmer Abrufen.
            HtmlNode nodeRooms = nodeData.GetNodeById(ImmoRoomCount)?.FirstChild?.NextSibling;

            // Mietpreis Abrufen.
            HtmlNode nodePrice = nodeData.GetNodeByAttribute(ImmoPrice)?.FirstChild?.NextSibling;

            // Wohnraum in m² Abrufen.
            HtmlNode nodeLivingSpace = nodeData.GetNodeByAttribute(ImmoSize)?.FirstChild?.NextSibling;

            string id = "https://www.stadtbau-wuerzburg.de" + WebUtility.HtmlDecode(nodeId.Attributes[0].Value);
            string titel = WebUtility.HtmlDecode(nodeTitle?.InnerText ?? "Kein Titel vorhanden").Trim();
            string price = WebUtility.HtmlDecode(nodePrice?.InnerText ?? "Unbekannt");
            string livingSpace = WebUtility.HtmlDecode(nodeLivingSpace?.InnerText ?? "Unbekannt");
            string rooms = WebUtility.HtmlDecode(nodeRooms?.InnerText ?? "Unbekannt");
            string thumb = WebUtility.HtmlDecode(GetImage(nodeThumb));
            string available = WebUtility.HtmlDecode(nodeAvailable?.InnerText ?? "Unbekannt").Trim();

            // Neues Wohnungsobjekt erstellen und zurückgeben.
            return new ApartmentElement(
                id,
                titel,
                price,
                livingSpace,
                rooms, 
                thumb, 
                available);
        }

        #endregion
    }
}
