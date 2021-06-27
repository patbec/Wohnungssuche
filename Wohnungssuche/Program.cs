using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

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

        private static ApartmentFinder apaFinder;
        private static MailingHelper apaMail;
        private static string htmlBody;

        #endregion

        #region Constants

        private static readonly string SMTP_Address  = Environment.GetEnvironmentVariable("SMTP_ADDRESS");
        private static readonly string SMTP_Server   = Environment.GetEnvironmentVariable("SMTP_SERVER");
        private static readonly string SMTP_Username = Environment.GetEnvironmentVariable("SMTP_USERNAME");
        private static readonly string SMTP_Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

        #endregion

        static void Main(string[] args)
        {
            try
            {
                // Aktuelle Anwendungsversion auslesen.
                Version version = Assembly.GetExecutingAssembly().GetName().Version;

                // Infomeldung schreiben.
                ConsoleWriter.WriteLine("\nAnwendung zur Wohnungssuche", $"(Version {version})\n");

                // Vorlage zum Versenden von Mails einlesen.
                htmlBody = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "banner.template"));

#if DEBUG
                // Infomeldung schreiben.
                ConsoleWriter.WriteLine("[DEBUG]", "Das Senden von Nachrichten während einer Debugsitzung ist deaktiviert.");
#else
                // Fehlkonfiguration abfangen.
                if(String.IsNullOrWhiteSpace(SMTP_Address)) {
                    throw new ArgumentNullException(nameof(SMTP_Address), "Specify settings for using mails in the environment variables.");
                }
                if(String.IsNullOrWhiteSpace(SMTP_Server)) {
                    throw new ArgumentNullException(nameof(SMTP_Server), "Specify settings for using mails in the environment variables.");
                }
#endif

                // SmptClient zum versenden von Mails vorbereiten.
                apaMail = new MailingHelper(
                    SMTP_Address,
                    SMTP_Server,
                    SMTP_Username,
                    SMTP_Password);

                // Klasse mit den Sucheinstellungen für die Wohnungssuche.
                apaFinder = new ApartmentFinder();

                // Tritt auf wenn eine neue Wohnung gefunden wurde.
                apaFinder.NewApartmentFound     += ApartmentFoundEvent;
                // Tritt auf wenn ein Suchlauf erfolgreich Abgeschlossen wurde.
                apaFinder.SearchSuccessful      += SearchSuccessfulEvent;
                // Tritt auf wenn ein Anwendungsfehler aufgetreten ist.
                apaFinder.ErrorOccurred         += ErrorOccurredEvent;

                // Mit der Suche nach Wohnungen beginnen.
                apaFinder.Start();

                // Warten bis die Wohnungssuche beendet wurde.
                apaFinder.WaitOnExit();

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
        private static void ApartmentFoundEvent(object sender, ApartmentElement e)
        {
            byte[] data = Encoding.Default.GetBytes(e.Titel);
            byte[] result = MD5.Create().ComputeHash(data);

            string filename = Convert.ToHexString(result) + ".cache";
            string dirpath = Path.Combine(Path.GetTempPath(), "wohnungssuche");
            string filepath = Path.Combine(dirpath, filename);

            if(File.Exists(filepath)) {
                // Infomeldung schreiben.
                ConsoleWriter.WriteLine(GetCurrentCounter(), "Es wurde eine bereits gesendete Wohnung gefunden!", textColor: ConsoleColor.Yellow);
            }
            else {
                // Wohnung in den Cache schreiben, beim erneuten Programmstart werden nur neue Wohnungen gesendet.
                Directory.CreateDirectory(dirpath);
                File.Create(filepath).Close();

                // Infomeldung schreiben.
                ConsoleWriter.WriteLine(GetCurrentCounter(), "Es wurde eine neue Wohnung gefunden!", textColor: ConsoleColor.Yellow);

                // Nachricht als HTML formatieren.
                string htmlResult = htmlBody
                    .Replace("%Id%",        WebUtility.HtmlEncode(e.Id))
                    .Replace("%Titel%",     WebUtility.HtmlEncode(e.Titel))
                    .Replace("%Image%",     WebUtility.HtmlEncode(e.Thumb))
                    .Replace("%Rooms%",     WebUtility.HtmlEncode(e.Rooms))
                    .Replace("%Price%",     WebUtility.HtmlEncode(e.Price))
                    .Replace("%Size%",      WebUtility.HtmlEncode(e.LivingSpace))
                    .Replace("%Available%", WebUtility.HtmlEncode(e.Available));

#if DEBUG
                // Infomeldung schreiben.
                ConsoleWriter.WriteLine("[DEBUG]", "Das Senden von Nachrichten während einer Debugsitzung ist deaktiviert.");
#else
                // Benutzer via Mail über die neue Wohnung benachrichtigen.
                apaMail.SendMail("Neue Wohnung gefunden", htmlResult);
#endif
            }
        }

        /// <summary>
        /// Tritt auf wenn ein Suchlauf erfolgreich Abgeschlossen wurde.
        /// </summary>
        private static void SearchSuccessfulEvent(object sender, EventArgs eventArgs)
        {
            // Fehlerzähler Zurücksetzten.
            ErrorCounter = 0;

            // Infomeldung über die Anzahl der bisherigen Suchvorgänge schreiben.
            ConsoleWriter.WriteLine(GetCurrentCounter(), "Suchvorgang wurde erfolgreich abgeschlossen.");
        }

        /// <summary>
        /// Tritt auf wenn ein Anwendungsfehler aufgetreten ist.
        /// </summary>
        private static void ErrorOccurredEvent(object sender, Exception e)
        {
            // Fehlerzähler Hochzählen.
            ErrorCounter += 1;

            // Infomeldung mit der Fehlermeldung schreiben.
            ConsoleWriter.WriteLine(GetCurrentCounter(), "Bei der Suche nach Wohnungen ist ein Fehler aufgetreten:" + Environment.NewLine + e.Message, textColor: ConsoleColor.Red);

            if(ErrorCounter >= ErrorThreshold)
            {
                // Infomeldung schreiben.
                ConsoleWriter.WriteLine(GetCurrentCounter(), String.Format("Es wurde der Grenzwert für {0} Folgefehler erreicht, die Wohnungssuche wird nun beendet...", ErrorThreshold));

                try {
                    // Benutzer via Mail über die neue Wohnung benachrichtigen.
                    apaMail.SendMail("Application Error", String.Format("Es wurde der Grenzwert für {0} Folgefehler erreicht, die Wohnungssuche wird nun beendet.", ErrorThreshold));

                } catch (Exception ex) {
                    // Infomeldung schreiben.
                    ConsoleWriter.WriteLine(GetCurrentCounter(), String.Format("Es konnte keine Infomeldung über Fehler gesendet werden: {0}", ex.Message));
                }

                // Versuchen die Wohnungssuche zu beenden.
                apaFinder?.Stop();
            }
        }

        #endregion

        /// <summary>
        /// Gibt einen Zähler zurück, der angibt wie viele Suchvorgänge ausgeführt worden sind.
        /// Siehe <see cref="ApartmentNotifier.Counter"/> für weitere Information.
        /// </summary>
        /// <returns>Anzahl der Suchvorgänge.</returns>
        private static string GetCurrentCounter()
        {
            // Zähler auslesen.
            return $"[{apaFinder.Counter:D4}]";
    }
    }
}
