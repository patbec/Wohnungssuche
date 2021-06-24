# Wohnungssuche

![Screenshot Banner](https://github.com/patbec/Wohnungssuche/blob/master/preview-wohnungssuche.png?raw=true)

### Beschreibung
Eine kleine Anwendung um im Raum Würzburg automatisch nach neuen Wohnungen zu suchen. Der Anbieter Stadtbau bietet keine Möglichkeit sich neue Angebote via Mail zusenden zu lassen.

### Installation und Ausführung

Die Anwendung wird in den Ordner `/usr/share/wohnungssuche` installiert. Mit der im Installationsordner enthaltenen **systemd** Dienstdatei ist eine Ausführung im Hintergrund möglich.
Die Dienstdatei mit dem Befehl `sudo nano /usr/share/wohnungssuche/wohnungssuche.service` zum bearbeiten öffnen und die folgenden Umgebungsvariablen anpassen um das automatische versenden von Mails für neue Wohnungen zu aktivieren:

```
# Zieladresse, kann auch die eigene Mail sein.
# Empfohlen: Später eine Regel mit einer Weiterleitung an mehrere Adressen einrichten.
Environment=SMTP_ADDRESS=youraddress
# Adresse des Mailserver.
Environment=SMTP_SERVER=yourserver
# Benutzername für die Anmeldung am Mailserver.
Environment=SMTP_USERNAME=yourusername
# Kennwort für die Anmeldung am Mailserver.
Environment=SMTP_PASSWORD=yourpassword
```

Es wird für die Anmeldung am Mailserver immer SSL verwendet.

Die Anwendung sollte als normaler Benutzer mit eingeschränkten Rechten ausgeführt werden, siehe **systemd** unter `User=youruser`.

Wenn das bearbeiten der **systemd** Dienstdatei abgeschlossen ist, muss diese noch aktiviert werden:

```
# Die Dienstdatei zu den system-Diensten hinzufügen.
sudo cp /usr/share/wohnungssuche/wohnungssuche.service /etc/systemd/system/wohnungssuche.service
# Nach der hinzugefügten Datei suchen.
sudo systemctl daemon-reload
# Den Dienst aktivieren.
sudo systemctl enable wohnungssuche
# Den Dienst starten.
sudo systemctl start wohnungssuche
```

Mit dem folgenden Befehl kann geprüft werden ob der Dienst korrekt ausgeführt wird:

```
# Status Abfragen.
sudo systemctl status wohnungssuche
```

## Autor

* **Patrick Becker** - [GitHub](https://github.com/patbec)

E-Mail: [git.bec@outlook.de](mailto:git.bec@outlook.de)

## Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert - Weitere Informationen finden Sie in der Datei [LICENSE](LICENSE)
