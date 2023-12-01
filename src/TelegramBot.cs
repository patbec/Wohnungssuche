using System;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Wohnungssuche
{
  public class TelegramBot
  {
    private readonly TelegramBotClient _telegramBot;
    private readonly string _telegramToken;
    private readonly string[] _telegramChannel;

    public static TelegramBot CreateFromDockerEnvironment()
    {
      // Get the telegram settings from the environment.
      string telegramToken = Helper.GetEnvironmentVariable("TELEGRAM_TOKEN");
      string telegramChannel = Helper.GetEnvironmentVariable("TELEGRAM_CHANNEL");

      if (string.IsNullOrWhiteSpace(telegramToken))
      {
        throw new ArgumentNullException(nameof(telegramToken), "Set the TELEGRAM_TOKEN setting in the environment variables.");
      }
      if (string.IsNullOrWhiteSpace(telegramChannel))
      {
        throw new ArgumentNullException(nameof(telegramChannel), "Set the TELEGRAM_CHANNEL setting in the environment variables.");
      }

      return new TelegramBot(telegramToken, telegramChannel);
    }

    public TelegramBot(string telegramToken, string[] telegramUsers)
    {
      var botClient = new TelegramBotClient(telegramToken, );
      _telegramToken = telegramToken;
      _telegramUsers = telegramUsers;
    }

    /// <summary>
    /// Sends an message to all users.
    /// </summary>
    /// <param name="message">Content of the message.</param>
    public void Send(string message)
    {

    }
  }
}