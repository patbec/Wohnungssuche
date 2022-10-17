# Wohnungssuche

Example of a <i>apartment</i> and <i>error notification</i> email from the application.
![Screenshot Banner](/docs/screenshot-item-dark.png#gh-dark-mode-only)
![Screenshot Banner](/docs/screenshot-item-light.png#gh-light-mode-only)
![Screenshot Banner](/docs/screenshot-error-dark.png#gh-dark-mode-only)
![Screenshot Banner](/docs/screenshot-error-light.png#gh-light-mode-only)

## Description
A small application to automatically search for new apartments in the Würzburg area. The provider Stadtbau does not offer the possibility to receive new offers via e-mail.

## Configuration
Send the mail to yourself and create a forwarding rule that affects the mail. Alternatively, you can use a distribution list in Azure Active Directory.

There are the following mail titles:
```
Neue Wohnung gefunden
```
```
Anwendungsfehler
```

Use the docker container an and set this enviroment variables:
```
SMTP_MAIL=your@mail.com
SMTP_HOST=mailserver
SMTP_USERNAME=username
SMTP_PASSWORD=password
```

SSL is always used for logging in to the mail server.

## License

This project is licensed under MIT - See the [LICENSE](LICENSE) file for more information.

---
<p align="right">
    &uarr; <a href="#wohnungssuche">Back to top</a>
</p>