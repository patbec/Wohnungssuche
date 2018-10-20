using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Wohnungssuche
{
    /// <summary>
    /// Objekt das eine Wohnung darstellt.
    /// </summary>
    public class ApartmentItem
    {
        #region Parser Data

        private const string SiteHeader     = "trefferliste-item-header";
        private const string SiteContent    = "trefferliste-item-content";
        private const string SiteAddress    = "trefferliste-item-adresse";
        private const string SiteData       = "trefferliste-item-daten";
        private const string SiteThumb      = "trefferliste-item-thumb";
        private const string SiteIdentifier = "trefferliste-item";

        #endregion

        #region Properties

        /// <summary>
        /// Kurze Zusammenfassung der Wohnung.
        /// </summary>
        public string Titel { get; }

        /// <summary>
        /// Postleitzahl mit Stadtnamen in der die Wohnung liegt.
        /// </summary>
        public string City { get; }

        /// <summary>
        /// Straße mit Hausnummer in der die Wohnung liegt.
        /// </summary>
        public string Street { get; }

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
        /// <param name="titel">Kurze Zusammenfassung der Wohnung.</param>
        /// <param name="city">Postleitzahl mit Stadtnamen in der die Wohnung liegt.</param>
        /// <param name="street">Straße mit Hausnummer in der die Wohnung liegt.</param>
        /// <param name="price">Mietpreis ohne Nebenkosten.</param>
        /// <param name="livingSpace">Größe des Wohnraumes in m².</param>
        /// <param name="rooms">Anzahl von Zimmern.</param>
        /// <param name="thumb">Vorschaubild der Wohnung.</param>
        /// <param name="available">Datum ab wann die Wohnung verfügbar ist.</param>
        public ApartmentItem(string titel, string city, string street, string price, string livingSpace, string rooms, string thumb, string available)
        {
            Titel       = titel;
            City        = city;
            Street      = street;
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
                Titel.PadRight(70)                                                + Environment.NewLine +
                "       " + Street.PadRight(35)      + Price.PadRight(35)         + Environment.NewLine +
                "       " + City.PadRight(35)        + LivingSpace.PadRight(35)   + Environment.NewLine +
                "       " + Available.PadRight(35)   + Rooms.PadRight(35)         + Environment.NewLine;
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
                Titel + "<br>" +
                $"<br>Adresse:<br>{City}, {Street}<br>" +
                $"<br>Preis:<br>{Price}<br>" +
                $"<br>Wohnraum:<br>{LivingSpace} / {Rooms}<br>" +
                $"<br>{Available}";
        }

        #region Static

        /// <summary>
        /// Konvertiert eine Teilzeichenfolge der zurückgegebenen Antwort der API und erstellt ein Wohnungsobjekt. 
        /// </summary>
        /// <param name="node">Teilzeichenfolge der zurückgegebenen Antwort der API.</param>
        /// <returns>Das erstellte Wohnungsobjekt.</returns>
        public static ApartmentItem Parse(HtmlNode node)
        {
            if (node is null)
                throw new ArgumentNullException(nameof(node));

            if (node.GetAttributeValue("class", null) != SiteIdentifier)
                throw new ArgumentException("The apartment data could not be read out because the input data has an incorrect format.", new InvalidDataException("The class does not contain an expected attribute."));


            HtmlNodeCollection items = node.ChildNodes;

            // Titel der Wohnung Abrufen.
            HtmlNode nodeTitle = items.GetNodeByAttribute(SiteHeader).FirstChild;

            // Details der Wohnung Abrufen.
            HtmlNode nodeContent = items.GetNodeByAttribute(SiteContent);

            // Erweiterte Inhaltsbeschreibung Abrufen.
            HtmlNodeCollection itemsContent = nodeContent.ChildNodes;

            // Straße, PLZ und Stadt Abrufen.
            HtmlNode nodeAddress = itemsContent.GetNodeByAttribute(SiteAddress);

            // Erweiterte Ortsbeschreibung Abrufen.
            HtmlNodeCollection itemsAddress = nodeAddress.ChildNodes;

            // Straßennamen Abrufen.
            HtmlNode nodeStreet = itemsAddress[0];

            // Postleitzahl mit Stadtnamen Abrufen.
            HtmlNode nodeCity = itemsAddress[2];

            // Datum ab wann die Wohnung verfügbar ist Abrufen.
            HtmlNode nodeAvailable = itemsAddress[4];

            // Mietpreis, Größe und Zimmeranzahl Abrufen.
            HtmlNode nodeData = itemsContent.GetNodeByAttribute(SiteData);

            // Erweiterte Wohnungsbeschreibung Abrufen.
            HtmlNodeCollection itemsData = nodeData.ChildNodes;

            // Mietpreis Abrufen.
            HtmlNode nodePrice = itemsData[0];

            // Wohnraum in m² Abrufen.
            HtmlNode nodeLivingSpace = itemsData[2];

            // Anzahl der Zimmer Abrufen.
            HtmlNode nodeRooms = itemsData[4];

            // Vorschaubild Abrufen.
            HtmlNode nodeThumb = itemsContent.GetNodeByAttribute(SiteThumb);

            // Abgerufene Date in das passende Format konvertieren, 
            // wenn ein Wert ein falsches Format hat wird dieser mit null übergeben.

            string titel = nodeTitle.InnerText;
            string city = nodeCity.InnerText;
            string street = nodeStreet.InnerText;
            string price = nodePrice.InnerText;
            string livingSpace = nodeLivingSpace.InnerText;
            string rooms = nodeRooms.InnerText;
            string thumb = null;
            string available = nodeAvailable.InnerText;

            // Neues Wohnungsobjekt erstellen und zurückgeben.
            return new ApartmentItem(
                titel,
                city, 
                street,
                price,
                livingSpace,
                rooms, 
                thumb, 
                available);
        }

        #endregion
    }
}
