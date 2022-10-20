using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace Wohnungssuche
{
  public class MailClient
  {
    private readonly string _smtpHost;
    private readonly string _smtpUser;
    private readonly string _smtpPassword;
    private readonly string _smtpFromAddress;
    private readonly string _smtpToAddress;

    public static MailClient CreateFromDockerEnvironment()
    {
      // Get the smtp settings from the environment.
      string smtpHostFile = Environment.GetEnvironmentVariable("WH_SMTP_HOST_FILE");
      string smtpUserFile = Environment.GetEnvironmentVariable("WH_SMTP_USER_FILE");
      string smtpPasswordFile = Environment.GetEnvironmentVariable("WH_SMTP_PASSWORD_FILE");
      string smtpFromAddressFile = Environment.GetEnvironmentVariable("WH_SMTP_FROM_ADDRESS_FILE");
      string smtpToAddressFile = Environment.GetEnvironmentVariable("WH_SMTP_TO_ADDRESS_FILE");

      if (string.IsNullOrWhiteSpace(smtpHostFile))
      {
        throw new ArgumentNullException(nameof(smtpHostFile), "Set the WH_SMTP_HOST_FILE setting in the environment variables.");
      }
      if (string.IsNullOrWhiteSpace(smtpUserFile))
      {
        throw new ArgumentNullException(nameof(smtpUserFile), "Set the WH_SMTP_USER_FILE setting in the environment variables.");
      }
      if (string.IsNullOrWhiteSpace(smtpPasswordFile))
      {
        throw new ArgumentNullException(nameof(smtpPasswordFile), "Set the WH_SMTP_PASSWORD_FILE setting in the environment variables.");
      }
      if (string.IsNullOrWhiteSpace(smtpFromAddressFile))
      {
        throw new ArgumentNullException(nameof(smtpToAddressFile), "Set the WH_SMTP_FROM_ADDRESS_FILE setting in the environment variables.");
      }
      if (string.IsNullOrWhiteSpace(smtpToAddressFile))
      {
        throw new ArgumentNullException(nameof(smtpToAddressFile), "Set the WH_SMTP_TO_ADDRESS_FILE setting in the environment variables.");
      }

      return new MailClient(
        File.ReadAllText(smtpHostFile),
        File.ReadAllText(smtpUserFile),
        File.ReadAllText(smtpPasswordFile),
        File.ReadAllText(smtpFromAddressFile),
        File.ReadAllText(smtpToAddressFile)
        );
    }

    public MailClient(string smtpHost, string smtpUser, string smtpPassword, string smtpFromAddress, string smtpToAddress)
    {
      _smtpHost = smtpHost;
      _smtpUser = smtpUser;
      _smtpPassword = smtpPassword;
      _smtpFromAddress = smtpFromAddress;
      _smtpToAddress = smtpToAddress;
    }

    /// <summary>
    /// Sends an e-mail.
    /// </summary>
    /// <param name="title">Message title.</param>
    /// <param name="message">Content of the message as HTML.</param>
    public void Send(string title, string message)
    {
      // Create a new message.
      MailMessage mailMessage = new(_smtpFromAddress, _smtpToAddress, title, message)
      {
        // Enable support for HTML.
        IsBodyHtml = true
      };

      SmtpClient smtpClient = new(_smtpHost, 587)
      {
        EnableSsl = true,
        Credentials = new NetworkCredential(_smtpUser, _smtpPassword)

      };
#if DEBUG
      Console.WriteLine("Sending messages during a debug session is disabled.");
#else
      smtpClient.Send(mailMessage);
#endif
    }
  }
}