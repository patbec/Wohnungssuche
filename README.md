# Wohnungssuche

Example of a <i>apartment</i> and <i>Added container error notification</i> email from the application.
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

Use Docker secrets and set the following environment variables to configure the mailing:
```
WH_SMTP_HOST_FILE
WH_SMTP_USER_FILE
WH_SMTP_PASSWORD_FILE
WH_SMTP_FROM_ADDRESS_FILE
WH_SMTP_TO_ADDRESS_FILE
```

Sample docker compose file:
```
version: '3.9'
volumes:
  wohnungssuche:
    external: true
secrets:
  smtp_password:
    external: true
  smtp_hostname:
    external: true
  smtp_username:
    external: true
  smtp_to_address:
    external: true
services:
  wohnungssuche:
    container_name: wohnungssuche
    image: beckerhub/wohnungssuche:latest
    restart: unless-stopped
    volumes:
      - wohnungssuche:/tmp/wohnungssuche/:rw
      - /etc/localtime:/etc/localtime:ro
    environment:
      - WH_SMTP_HOST_FILE=/run/secrets/smtp_hostname
      - WH_SMTP_USER_FILE=/run/secrets/smtp_username
      - WH_SMTP_PASSWORD_FILE=/run/secrets/smtp_password
      - WH_SMTP_FROM_ADDRESS_FILE=/run/secrets/smtp_username
      - WH_SMTP_TO_ADDRESS_FILE=/run/secrets/smtp_to_address
    secrets:
      - smtp_hostname
      - smtp_username
      - smtp_password
      - smtp_to_address
    networks:
      - default
    user: "${PUID}:${PGID}"

```
SSL is always used for logging in to the mail server.

## License

This project is licensed under MIT - See the [LICENSE](LICENSE) file for more information.

---
<p align="right">
    &uarr; <a href="#wohnungssuche">Back to top</a>
</p>