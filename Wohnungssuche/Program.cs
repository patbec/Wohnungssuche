using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Wohnungssuche
{
    public class Program
    {
        #region Private Fields

        /// <summary>
        /// Zählt aufeinanderfolgende Fehlermeldungen.
        /// </summary>
        private static long ErrorCounter = 0;
        private static readonly long ErrorThreshold = 15;

        private static ApartmentSearcher apaSearch;
        private static ApartmentNotifier apaNotifi;
        private static MailingHelper     apaMail;

        #endregion

        #region Constants

        private const string SMTP_Address   = "app@bec-wolke.de";
        private const string SMTP_Server    = "bec-wolke.de";
        private const string SMTP_Username  = "app@bec-wolke.de";
        private const string SMTP_Password  = "testinghall";

        #endregion

        static void Main(string[] args)
        {
            try
            {
                // Infomeldung schreiben.
                ConsoleWriter.WriteLine("\nAnwendung zur Wohnungssuche", "(Version 1.0)\n");

                // SmptClient zum versenden von Mails vorbereiten.
                apaMail = new MailingHelper(
                    SMTP_Address,
                    SMTP_Server,
                    SMTP_Username,
                    SMTP_Password);

                // Klasse mit den Sucheinstellungen für die Wohnungssuche.
                apaSearch = new ApartmentSearcher(1, 6);

                // Infomeldung schreiben.
                ConsoleWriter.WriteLine($"Konfiguration der Anwendung:", $"\n{apaSearch}\n");

                // Klasse um die Ergebnisse der Wohnungssuche aufzubereiten.
                apaNotifi = new ApartmentNotifier(apaSearch, 60);

                // Tritt auf wenn eine neue Wohnung gefunden wurde.
                apaNotifi.NewApartmentFound     += ApartmentFoundEvent;
                // Tritt auf wenn ein Suchlauf erfolgreich Abgeschlossen wurde.
                apaNotifi.SearchSuccessful      += SearchSuccessfulEvent;
                // Tritt auf wenn ein Anwendungsfehler aufgetreten ist.
                apaNotifi.ErrorOccurred         += ErrorOccurredEvent;

                // Mit der Suche nach Wohnungen beginnen.
                apaNotifi.Start();

                // Warten bis die Wohnungssuche beendet wurde.
                apaNotifi.WaitOnExit();

                //Thread.Sleep(Timeout.Infinite)
            }
            catch (Exception ex)
            {
                // Infomeldung mit der Fehlermeldung schreiben.
                ConsoleWriter.WriteLine("Es ist ein Anwendungsfehler aufgetreten:" + Environment.NewLine + ex.Message, ConsoleColor.Red);
            }
        }

        #region Events

        /// <summary>
        /// Tritt auf wenn eine neue Wohnung gefunden wurde.
        /// </summary>
        private static void ApartmentFoundEvent(object sender, ApartmentItem e)
        {
            // Infomeldung schreiben.
            ConsoleWriter.WriteLine("Es wurde eine neue Wohnung gefunden!", ConsoleColor.Yellow);

            // Nachricht als HTML formatieren.
            string htmlBody = e.ToString(enableHtml: true);

#if DEBUG
            // Infomeldung schreiben.
            ConsoleWriter.WriteLine("[DEBUG]", "Das Senden von Nachrichten während einer Debugsitzung ist deaktiviert.");
#else
            // Benutzer via Mail über die neue Wohnung benachrichtigen.
            apaMail.SendMail("Neue Wohnung gefunden", htmlBody);
#endif
        }

        /// <summary>
        /// Tritt auf wenn ein Suchlauf erfolgreich Abgeschlossen wurde.
        /// </summary>
        private static void SearchSuccessfulEvent(object sender, long e)
        {
            // Fehlerzähler Zurücksetzten.
            ErrorCounter = 0;

            // Infomeldung über die Anzahl der bisherigen Suchvorgänge schreiben.
            ConsoleWriter.WriteLine($"[{e.ToString("D4")}]", "Suchvorgang wurde erfolgreich abgeschlossen.");
        }

        /// <summary>
        /// Tritt auf wenn ein Anwendungsfehler aufgetreten ist.
        /// </summary>
        private static void ErrorOccurredEvent(object sender, Exception e)
        {
            // Fehlerzähler Hochzählen.
            ErrorCounter += 1;

            // Infomeldung mit der Fehlermeldung schreiben.
            ConsoleWriter.WriteLine("Bei der Suche nach Wohnungen ist ein Fehler aufgetreten:" + Environment.NewLine + e.Message, ConsoleColor.Red);

            if(ErrorCounter >= ErrorThreshold)
            {
                // Infomeldung schreiben.
                ConsoleWriter.WriteLine(String.Format("Es wurde der Grenzwert für {0} Folgefehler erreicht, die Wohnungssuche wird nun beendet...", ErrorThreshold));

                // Versuchen die Wohnungssuche zu beenden.
                apaNotifi?.Stop();
            }
        }

        #endregion
    }
}
