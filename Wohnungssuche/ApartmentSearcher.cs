using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace Wohnungssuche
{
    /// <summary>
    /// Ruft eine Auflistung von Wohnungen ab.
    /// </summary>
    public class ApartmentSearcher 
    {
        #region Request Data

        private const string SearchBaseUri = "https://immoblue.aareon.com/stadtbau-wuerzburg/expose/index/suche";

        private const string POST_MinRoom = "objd__zimmer_from=";
        private const string POST_MaxRoom = "objd__zimmer_to=";
        private const string POST_MinPrice = "objd__nkm_from=";
        private const string POST_MaxPrice = "objd__nkm_to=";
        private const string POST_MinLivingSpace = "objd__wohnflaeche_from=";
        private const string POST_MaxLivingSpace = "objd__wohnflaeche_to=";
        private const string POST_Region = "obj__region_id=";
        private const string POST_End = "submit=Suchen";

        private const string XPathQuery = "//div[@class='trefferliste-item']";

        private const string Unknown = "N/A";

        #endregion

        #region Properties

        /// <summary>
        /// Mindestanzahl von Zimmern. Dieser Wert darf nicht null sein.
        /// </summary>
        public uint MinRoom { get; }

        /// <summary>
        /// Maximalanzahl von Zimmern. Dieser Wert darf nicht null sein.
        /// </summary>
        public uint MaxRoom { get; }

        /// <summary>
        /// Mindestwert des Mietpreises. Geben Sie Null an, wenn die Ergebnisse nicht nach diesem Suchbegriff gefiltert werden sollen.
        /// </summary>
        public uint? MinPrice { get; }

        /// <summary>
        /// Maximalwert des Mietpreises. Geben Sie Null an, wenn die Ergebnisse nicht nach diesem Suchbegriff gefiltert werden sollen.
        /// </summary>
        public uint? MaxPrice { get; }

        /// <summary>
        /// Mindestgröße des Wohnraumes in m². Geben Sie Null an, wenn die Ergebnisse nicht nach diesem Suchbegriff gefiltert werden sollen.
        /// </summary>
        public uint? MinLivingSpace { get; }

        /// <summary>
        /// Maximalgröße des Wohnraumes in m². Geben Sie Null an, wenn die Ergebnisse nicht nach diesem Suchbegriff gefiltert werden sollen.
        /// </summary>
        public uint? MaxLivingSpace { get; }

        /// <summary>
        /// Stadtteil in dem nach Wohnungen gesucht werden soll. Geben Sie Null an, wenn die Ergebnisse nicht nach diesem Suchbegriff gefiltert werden sollen.
        /// </summary>
        public Regions? Region { get; }

        #endregion

        /// <summary>
        /// Klasse konfigurieren um nach Wohnungen zu Suchen.
        /// </summary>
        /// <param name="minRoom">Mindestanzahl von Zimmern. Dieser Wert darf nicht null sein.</param>
        /// <param name="maxRoom">Maximalanzahl von Zimmern. Dieser Wert darf nicht null sein.</param>
        /// <param name="minPrice">Mindestwert des Mietpreises. Geben Sie Null an, wenn die Ergebnisse nicht nach diesem Suchbegriff gefiltert werden sollen.</param>
        /// <param name="maxPrice">Maximalwert des Mietpreises. Geben Sie Null an, wenn die Ergebnisse nicht nach diesem Suchbegriff gefiltert werden sollen.</param>
        /// <param name="minLivingSpace">Mindestgröße des Wohnraumes in m². Geben Sie Null an, wenn die Ergebnisse nicht nach diesem Suchbegriff gefiltert werden sollen.</param>
        /// <param name="maxLivingSpace">Maximalgröße des Wohnraumes in m². Geben Sie Null an, wenn die Ergebnisse nicht nach diesem Suchbegriff gefiltert werden sollen.</param>
        /// <param name="region">Stadtteil in dem nach Wohnungen gesucht werden soll. Geben Sie Null an, wenn die Ergebnisse nicht nach diesem Suchbegriff gefiltert werden sollen.</param>
        public ApartmentSearcher(uint minRoom = 1, uint maxRoom = 6, uint? minPrice = null, uint? maxPrice = null, uint? minLivingSpace = null, uint? maxLivingSpace = null, Regions? region = null)
        {
            MinRoom = minRoom;
            MaxRoom = maxRoom;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            MinLivingSpace = minLivingSpace;
            MaxLivingSpace = maxLivingSpace;
            Region = region;
        }

        /// <summary>
        /// Ruft eine Auflistung von Wohnungen ab. Die Einstellungen sowie Parameter werden im Konstruktor festgelegt.
        /// </summary>
        /// <returns>Auflistung von verfügbaren Wohnungen, die den Suchkriterien entsprechen.</returns>
        public IEnumerable<ApartmentItem> GetItems()
        {
            // Zeichenfolge mit den Suchkriterien erstellen.
            string postData =
                POST_MinRoom + MinRoom                  + "&" +
                POST_MaxRoom + MaxRoom                  + "&" +
                POST_MinPrice + MinPrice                + "&" +
                POST_MaxPrice + MaxPrice                + "&" +
                POST_MinLivingSpace + MinLivingSpace    + "&" +
                POST_MaxLivingSpace + MaxLivingSpace    + "&" +
                POST_Region + Region                    + "&" +
                POST_End;

            // Anfrage senden.
            string response =
                GetResponse(SearchBaseUri, "POST", "application/x-www-form-urlencoded", postData);

            // Neues Dokument erstellen.
            HtmlDocument pageDocument = new HtmlDocument();

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
                yield return ApartmentItem.Parse(node);
            }
        }

        /// <summary>
        /// Gibt eine Zeichenfolge mit den gesetzten Sucheinstellungen zurück.
        /// </summary>
        /// <returns>Zeichenfolge mit gesetzten Sucheinstellungen.</returns>
        public override string ToString()
        {
            return
                String.Format("Zimmer       {0} bis {1}", MinRoom, MaxRoom)                                                               + Environment.NewLine +
                String.Format("Preis        {0} bis {1}", MinPrice?.ToString() ?? Unknown, MaxPrice?.ToString() ?? Unknown)               + Environment.NewLine +
                String.Format("Größe        {0} bis {1}", MinLivingSpace?.ToString() ?? Unknown, MaxLivingSpace?.ToString() ?? Unknown)   + Environment.NewLine +
                String.Format("Stadtteil    {0}", Region?.ToString() ?? Unknown);
        }

        #region Helper / Internal

        /// <summary>
        /// Erstellt einen WebRequest mit den angegebenen Daten und gibt die Antwort zurück.
        /// </summary>
        /// <param name="uri">Die URL die geöffnet werden soll.</param>
        /// <param name="method">Die zu verwendende Methode.</param>
        /// <param name="contentType">Der Typ des Inhaltes.</param>
        /// <param name="data">POST-Daten die als Parameter gesendet werden sollen.</param>
        /// <returns>Die Antwort der Anfrage.</returns>
        private static string GetResponse(string uri, string method, string contentType, string data)
        {
            // Anfrage senden.
            WebResponse response =
                CreateRequest(uri, method, contentType, data);

            // Beginne Antwort der Anfrage auszulesen.
            StreamReader rstream = new StreamReader(
                response.GetResponseStream());

            // Bis zum Streamende lesen.
            return rstream.ReadToEnd();
        }

        /// <summary>
        /// Erstellt einen WebRequest mit den angegebenen Daten und gibt die Antwort zurück.
        /// </summary>
        /// <param name="uri">Die URL die geöffnet werden soll.</param>
        /// <param name="method">Die zu verwendende Methode.</param>
        /// <param name="contentType">Der Typ des Inhaltes.</param>
        /// <param name="data">POST-Daten die als Parameter gesendet werden sollen.</param>
        /// <returns>Die Antwort der Anfrage.</returns>
        private static WebResponse CreateRequest(string uri, string method, string contentType, string data)
        {
            if (String.IsNullOrEmpty(uri))
                throw new ArgumentNullException(nameof(uri));

            if (String.IsNullOrEmpty(method))
                throw new ArgumentNullException(nameof(method));

            if (String.IsNullOrEmpty(data))
                throw new ArgumentNullException(nameof(data));


            byte[] buffer = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;

            request.Method          = method;
            request.ContentType     = contentType;
            request.ContentLength   = buffer.Length;

            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Flush();
            }

            return request.GetResponse() as HttpWebResponse;
        }

        #endregion
    }
}
