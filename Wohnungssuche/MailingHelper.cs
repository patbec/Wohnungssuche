using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Wohnungssuche
{
    /// <summary>
    /// Erstellt einen neuen Mailclient.
    /// </summary>
    public class MailingHelper
    {
        #region Private Fields

        private readonly String address;
        private readonly String host;
        private readonly NetworkCredential credentials;

        #endregion

        /// <summary>
        /// Einstellungen festlegen um Mails zu versenden.
        /// </summary>
        /// <param name="source">Mailadresse von dem die Nachricht gesendet werden soll.</param>
        /// <param name="host">Adresse des Mailservers.</param>
        /// <param name="username">Benutzername des Mailpostfaches.</param>
        /// <param name="password">Kennwort des Mailpostfaches.</param>
        public MailingHelper(string source, string server, string username, string password)
        {
            address         = source;
            host            = server;
            credentials     = new NetworkCredential(username, password);
        }

        /// <summary>
        /// Sendet eine Mail mit den angegebenen Titel.
        /// </summary>
        /// <param name="titel">Titel der Nachricht.</param>
        /// <param name="message">Inhalt der Nachricht.</param>
        public void SendMail(string titel, string message)
        {
            // Neue Nachricht erstellen.
            MailMessage mail = new(address, address, titel, message)
            {
                // Unterstützung für HTML aktiveren.
                IsBodyHtml = true
            };

            // SmptServer anweisen die Mail Abzusenden.
            new SmtpClient(host, 587)
            {
                EnableSsl = true,
                // Zugangsdaten angeben.
                Credentials = credentials

            }.Send(mail);
        }
    }
}
