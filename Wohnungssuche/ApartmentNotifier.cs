using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wohnungssuche
{
    /// <summary>
    /// Sucht in einem Zeitintervall nach neuen Wohnungen und löst bei einem Fund ein Event aus.
    /// </summary>
    public class ApartmentNotifier
    {
        #region Private Fields

        private Task                        m_listener;
        private CancellationTokenSource     m_token;

        private readonly ApartmentSearcher  m_apartmentSearcher;
        private readonly SemaphoreSlim      m_lockToken;

        private readonly Dictionary<string, ApartmentItem> m_listApartments;

        #endregion

        #region Properties

        /// <summary>
        /// Gibt einen Wert zurück ob der Client nach neuen Wohnungen sucht.
        /// </summary>
        public bool IsActive
        {
            get => m_listener?.Status == TaskStatus.Running;
        }

        /// <summary>
        /// Gibt die Anzahl der Suchvorgänge zurück.
        /// </summary>
        public long Counter { get; private set; }

        /// <summary>
        /// Gibt den Zeitraum in Sekunden zurück, nachdem die Suche nach neuen Wohnungen wiederholt wird.
        /// Dieser wird beträgt standardmäßig 60 Sekunden.
        /// </summary>
        public int Interval { get; }

        /// <summary>
        /// Gibt eine Auflistung von gefundenen Wohnungen zurück.
        /// Wird eine neue Wohnung gefunden die sich nicht in dieser Auflistung befindet,
        /// wird das Event <see cref="NewApartmentFound"/> ausgelöst.
        /// </summary>
        public ReadOnlyDictionary<string, ApartmentItem> CurrentApartments { get; }

        #endregion

        #region Events

        /// <summary>
        /// Tritt auf wenn eine neue Wohnung gefunden wurde.
        /// </summary>
        public EventHandler<ApartmentItem> NewApartmentFound;

        /// <summary>
        /// Tritt auf wenn ein Suchfehler aufgetreten ist.
        /// </summary>
        public EventHandler<Exception> ErrorOccurred;

        /// <summary>
        /// Tritt auf wenn ein Suchlauf erfolgreich abgeschlossen wurde.
        /// </summary>
        public EventHandler SearchSuccessful;

        #endregion

        /// <summary>
        /// Erstellt eine neue Klasse die in dem angegebenen Zeitraum nach neuen Wohnungen sucht und 
        /// bei einem neuen Suchtreffer das Event <see cref="NewApartmentFound"/> auslöst.
        /// </summary>
        /// <param name="interval">Zeitraum in Sekunden zurück nachdem die Suche nach neuen Wohnungen wiederholt wird.</param>
        public ApartmentNotifier(ApartmentSearcher apartmentSearcher, int interval = 60)
        {
            if (interval <= 0)
                throw new ArgumentOutOfRangeException("The time specification is invalid.");

            if (apartmentSearcher == null)
                throw new ArgumentNullException(nameof(apartmentSearcher));


            // Sucher übergeben.
            m_apartmentSearcher = apartmentSearcher;

            // Zeitintervall festlegen.
            Interval = interval;

            // Neue Auflistung für gefundene Wohnungen erstellen.
            m_listApartments = new Dictionary<string, ApartmentItem>();

            // Neues Sperrobjekt erstellen.
            m_lockToken = new SemaphoreSlim(1, 1);

            // Nur lesbare Auflistung für die Eigenschaft erstellen.
            CurrentApartments = new ReadOnlyDictionary<string, ApartmentItem>(m_listApartments);
        }

        /// <summary>
        /// Startet die Suche nach neuen Wohnungen.
        /// </summary>
        public void Start()
        {
            try
            {
                // Warten bis der Vorgang zugelassen wird.
                m_lockToken.Wait();

                // Prüfen ob der Task beendet ist.
                if (!IsActive)
                {
                    // Prüfen ob das Abbruchtoken erneuert werden muss.
                    if (m_token == null || m_token.IsCancellationRequested)
                        m_token = new CancellationTokenSource();

                    // Neuen Task erstellen und Starten.
                    m_listener = Task.Factory.StartNew(ListenOnApartments);
                }
            }
            catch (Exception ex)
            {
                // Ausnahme auslösen.
                throw new ApplicationException("The search for new apartments could not be started.", ex);
            }
            finally
            {
                // Sperrobjekt lösen.
                m_lockToken.Release();
            }
        }

        /// <summary>
        /// Beendet die Suche nach neuen Wohnungen.
        /// </summary>
        public void Stop()
        {
            try
            {
                // Warten bis der Vorgang zugelassen wird.
                m_lockToken.Wait();

                // Prüfen ob der Task läuft.
                if (IsActive)
                {
                    // Abbruchanforderung an den Task senden.
                    m_token.Cancel();

                    // Warten bis der Thread beendet wurde.
                    WaitOnExit();
                }
            }
            catch (Exception ex)
            {
                // Ausnahme auslösen.
                throw new ApplicationException("The search for new apartments could not be stopped.", ex);
            }
            finally
            {
                // Sperrobjekt lösen.
                m_lockToken.Release();
            }
        }

        /// <summary>
        /// Wartet solange bis der Hintergrundtask beendet wurde.
        /// </summary>
        public void WaitOnExit()
        {
            m_token?.Token.WaitHandle?.WaitOne();
        }

        #region Helper / Internal

        /// <summary>
        /// Sucht in dem mit <see cref="Interval"/> angegebenen Zeitraum nach neuen Wohnungen.
        /// </summary>
        private void ListenOnApartments()
        {
            // Infomeldung schreiben.
            ConsoleWriter.WriteLine("Die Wohnungssuche wurde gestartet...");

            // Solange ausführen bis eine Abbruchanforderung gesendet wurde.
            while (!m_token.IsCancellationRequested)
            {
                // Zähler Hochzählen.
                Counter += 1;

                try
                {
                    // Wohnung von der API Abrufen.
                    IEnumerable<ApartmentItem> result =
                        m_apartmentSearcher.GetItems();

                    // Ergebnisse nach neuen Wohnungen durchsuchen.
                    foreach (ApartmentItem apartment in result)
                    {
                        // Versuchen die angegebene Wohnung hinzuzufügen.
                        if(TryAdd(apartment))
                        {
                            // Event auslösen, dass eine neue Wohnung gefunden wurde.
                            NewApartmentFound?.Invoke(this, apartment);
                        }
                    }

                    // Event auslösen, dass der Suchvorgang erfolgreich beendet wurde.
                    SearchSuccessful?.Invoke(this, EventArgs.Empty);
                }
                catch(Exception ex)
                {
                    // Event auslösen, dass ein Anwendungsfehler aufgetreten ist.
                    ErrorOccurred?.Invoke(this, ex);
                }

                // Zeitraum abwarten bis nach neuen Wohnungen gesucht wird.
                Thread.Sleep(Interval * 1000);
            }

            // Infomeldung schreiben.
            ConsoleWriter.WriteLine("Die Wohnungssuche wurde beendet.");
        }

        /// <summary>
        /// Versucht die angegebene Wohnung zur Auflistung hinzuzufügen.
        /// </summary>
        /// <param name="apartment">Das Wohnungsobjekt.</param>
        /// <returns>False, wenn die angegebene Wohnung bereits bekannt ist. True, wenn das hinzufügen erfolgreich war.</returns>
        private bool TryAdd(ApartmentItem apartment)
        {
            if (apartment is null)
                throw new ArgumentNullException(nameof(apartment));


            // Auflistung für andere Threads sperren.
            lock(m_listApartments)
            {
                // Prüfen ob die angegebene Wohnung bereits bekannt ist.
                if (m_listApartments.ContainsKey(apartment.Titel))
                {
                    // Wohnung existiert bereits.
                    return false;
                }

                // Neue Wohnung zur Auflistung hinzufügen.
                m_listApartments.Add(apartment.Titel, apartment);

                // Wohnung ist unbekannt und wurde erfolgreich zur Auflistung hinzugefügt.
                return true;
            }
        }

        #endregion
    }
}
