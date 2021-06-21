using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Wohnungssuche
{
    /// <summary>
    /// Erweitert den Typ <see cref="String"/> um eine Funktion zum formatieren Anzeigen von Werten.
    /// </summary>
    public static class NodeExtension
    {
        /// <summary>
        /// Ruf einen Node mit dem angegebenen Attributwert ab.
        /// </summary>
        /// <param name="attribute">Gesuchter Attributwert.</param>
        /// <returns>Der gefundene Node mit dem angegebenen Attributwert.</returns>
        /// <exception cref="NodeNotFoundException">Tritt auf wenn der Attributwert nicht gefunden wurde.</exception>
        public static HtmlNode GetNodeByAttribute(this HtmlNodeCollection nodes, string attribute)
        {
            // Inhaltsknoten durchlaufen.
            foreach (HtmlNode currentItem in nodes)
            {
                // Prüfen ob des aktuelle Knoten ein Attribut besitzt.
                if (currentItem.HasAttributes)
                {
                    // Attributwert der Eigenschaft Klasse auslesen
                    string attributeValue = currentItem.GetAttributeValue("class", null);

                    if(attributeValue != null)
                    {
                        // Attributwert aufteilen und vergleichen.
                        if(attributeValue.Split(' ').Contains(attribute)) {
                            return currentItem;    
                        }
                    }
                }
            }

            // Ausnahme Auslösen wenn kein passender Knoten gefunden wurde.
            throw new NodeNotFoundException();
        }

        /// <summary>
        /// Ruf einen Node mit dem angegebenen Attributwert ab.
        /// </summary>
        /// <param name="attribute">Gesuchter Attributwert.</param>
        /// <returns>Der gefundene Node mit dem angegebenen Attributwert.</returns>
        /// <exception cref="NodeNotFoundException">Tritt auf wenn der Attributwert nicht gefunden wurde.</exception>
        public static HtmlNode GetNodeById(this HtmlNodeCollection nodes, string id)
        {
            // Inhaltsknoten durchlaufen.
            foreach (HtmlNode currentItem in nodes)
            {
                // Prüfen ob des aktuelle Knoten ein Attribut besitzt.
                if (currentItem.HasAttributes)
                {
                    // Attributwert der Eigenschaft Id auslesen und vergleichen.
                    if (currentItem.GetAttributeValue("id", null) == id)
                    {
                        // Wenn der Wert übereinstimmt zurückgeben.
                        return currentItem;
                    }
                }
            }

            // Ausnahme Auslösen wenn kein passender Knoten gefunden wurde.
            throw new NodeNotFoundException();
        }
    }
}
