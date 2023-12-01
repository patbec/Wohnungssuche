# Wohnungssuche

Example of a <i>apartment</i> notification from the telegram bot.

![Screenshot Banner](/docs/screenshot-telegram.png)

## Description
A small application to automatically search for new apartments in the Würzburg area. The provider Stadtbau does not offer the possibility to receive new offers via push notification.

## Configuration

In the previous application version, emails were used for notifications. Due to limitations in sending mails (Microsoft identifies the mails partially as spam, SMTP basic auth is no longer supported etc.) a <b>Telegram bot</b> is now used.

Use Docker secrets and set the following environment variables to configure the mailing:
```
TELEGRAM_TOKEN_FILE
TELEGRAM_CHANNEL_FILE

OR

TELEGRAM_TOKEN
TELEGRAM_CHANNEL
```
> An environment variable such as `TELEGRAM_CHANNEL` is preferred over the file reference `TELEGRAM_CHANNEL_FILE`.

Sample docker compose file:
```
version: '3.9'
volumes:
  wohnungssuche:
    external: true
secrets:
  telegram_token:
    external: true
  telegram_channel:
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
      - TELEGRAM_TOKEN_FILE=/run/secrets/telegram_token
      - TELEGRAM_CHANNEL_FILE=/run/secrets/telegram_channel
    secrets:
      - telegram_token
      - telegram_channel
    networks:
      - default
    user: "${PUID}:${PGID}"
```

## License

This project is licensed under MIT - See the [LICENSE](LICENSE) file for more information.

---
<p align="right">
    &uarr; <a href="#wohnungssuche">Back to top</a>
</p>