using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace Wohnungssuche
{
  public static class NodeExtension
  {
    /// <summary>
    /// Ruf einen Node mit dem angegebenen Attributwert ab.
    /// </summary>
    /// <param name="attribute">Gesuchter Attributwert.</param>
    /// <returns>Der gefundene Node mit dem angegebenen Attributwert.</returns>
    /// <exception cref="NodeNotFoundException">Tritt auf wenn der Attributwert nicht gefunden wurde.</exception>
    public static HtmlNode GetNodeByAttribute(this HtmlNodeCollection nodes, string attribute, bool recursive = false)
    {
      // Inhaltsknoten durchlaufen.
      foreach (HtmlNode currentItem in nodes)
      {
        // Prüfen ob des aktuelle Knoten ein Attribut besitzt.
        if (currentItem.HasAttributes)
        {
          // Attributwert der Eigenschaft Klasse auslesen
          string attributeValue = currentItem.GetAttributeValue("class", null);

          if (attributeValue != null)
          {
            // Attributwert aufteilen und vergleichen.
            if (attributeValue == attribute || attributeValue.Split(' ').Contains(attribute))
            {
              return currentItem;
            }
          }
        }

        // Prüfen ob Unterelemente durchsucht werden sollen.
        if (recursive)
        {
          // Inhaltsknoten Abrufen
          if (currentItem.ChildNodes != null)
          {
            // Attributwert der Eigenschaft Klasse auslesen
            var attributeValue = currentItem.ChildNodes.GetNodeByAttribute(attribute, recursive);

            if (attributeValue != null)
            {
              return attributeValue;
            }
          }
        }
      }

      return null;
      // // Ausnahme Auslösen wenn kein passender Knoten gefunden wurde.
      // throw new NodeNotFoundException();
    }

    /// <summary>
    /// Ruf einen Node mit dem angegebenen Attributtype ab.
    /// </summary>
    /// <param name="type">Gesuchter Attributtype.</param>
    /// <returns>Der gefundene Node mit dem angegebenen Attributtype.</returns>
    /// <exception cref="NodeNotFoundException">Tritt auf wenn der Attributtype nicht gefunden wurde.</exception>
    public static HtmlNode GetNodeByType(this HtmlNodeCollection nodes, string type)
    {
      // Inhaltsknoten durchlaufen.
      foreach (HtmlNode currentItem in nodes)
      {
        // Prüfen ob der aktuelle Knoten ein Attribut besitzt.
        if (currentItem.HasAttributes)
        {
          // Attributwert der Eigenschaft Klasse auslesen
          string attributeValue = currentItem.GetAttributeValue(type, null);

          if (attributeValue != null)
          {
            return currentItem;
          }
        }

        // Inhaltsknoten Abrufen
        if (currentItem.ChildNodes != null)
        {
          // Attributwert der Eigenschaft Klasse auslesen
          var attributeValue = currentItem.ChildNodes.GetNodeByType(type);

          if (attributeValue != null)
          {
            return attributeValue;
          }
        }
      }

      return null;
      // // Ausnahme Auslösen wenn kein passender Knoten gefunden wurde.
      // throw new NodeNotFoundException();
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
        // Prüfen ob der aktuelle Knoten ein Attribut besitzt.
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

      return null;
      // // Ausnahme Auslösen wenn kein passender Knoten gefunden wurde.
      // throw new NodeNotFoundException();
    }


    public static HtmlNode GetNodeByTextValue(this HtmlNodeCollection nodes, string text)
    {
      // Inhaltsknoten durchlaufen.
      foreach (HtmlNode currentItem in nodes)
      {
        // Prüfen ob der aktuelle Knoten ein Attribut besitzt.
        if (currentItem.InnerText == text)
        {
          // Nächstgelegendes Element zurückgeben.
          return currentItem;
        }

        // Inhaltsknoten Abrufen
        if (currentItem.ChildNodes != null)
        {
          // Attributwert der Eigenschaft Klasse auslesen
          var node = currentItem.ChildNodes.GetNodeByTextValue(text);

          if (node != null)
          {
            return node;
          }
        }
      }

      return null;
      // // Ausnahme Auslösen wenn kein passender Knoten gefunden wurde.
      // throw new NodeNotFoundException();
    }
  }
}
