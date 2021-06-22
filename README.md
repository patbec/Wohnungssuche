# Wohnungssuche

![Screenshot Mail](https://github.com/patbec/Wohnungssuche/blob/master/preview-wohnungssuche.png?raw=true)

### Beschreibung
Eine kleine Anwendung um im Raum Würzburg automatisch nach neuen Wohnungen zu suchen. Der Anbieter Stadtbau bietet keine Möglichkeit sich neue Angebote via Mail zusenden zu lassen.

### Installation und Ausführung

In den Projektdateien gibt es den Ordner **Help**, dort liegt eine **systemd** Dienstdatei für die Ausführung im Hintergrund.
Um das automatische versenden von Mails für neue Wohnungen zu aktivieren, die Smtp-Daten eines Mail Kontos in die **systemd** Datei eintragen. Es wird für die Anmeldung SSL verwendet.

Die Anwendung sollte als normaler Benutzer mit eingeschränkten Rechten ausgeführt werden, siehe **systemd** unter User=youruser.

## Autor

* **Patrick Becker** - [GitHub](https://github.com/patbec)

E-Mail: [git.bec@outlook.de](mailto:git.bec@outlook.de)

## Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert - Weitere Informationen finden Sie in der Datei [LICENSE](LICENSE)
