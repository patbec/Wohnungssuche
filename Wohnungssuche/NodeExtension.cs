using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

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
                    // Attributwert der Eigenschaft Klasse auslesen und vergleichen.
                    if (currentItem.GetAttributeValue("class", null) == attribute)
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
