using System;
using System.Net;
using System.Net.Mail;

namespace Wohnungssuche
{
  public class MailClient
  {
    private readonly string _mail;
    private readonly string _host;
    private readonly string _username;
    private readonly string _password;

    public static MailClient CreateFromEnvironment()
    {
      // Get the smtp settings from the environment.
      string mail = Environment.GetEnvironmentVariable("MAIL");
      string host = Environment.GetEnvironmentVariable("HOST");
      string username = Environment.GetEnvironmentVariable("USERNAME");
      string password = Environment.GetEnvironmentVariable("PASSWORD");

      if (string.IsNullOrWhiteSpace(mail))
      {
        throw new ArgumentNullException(nameof(mail), "Set the MAIL setting in the environment variables.");
      }
      if (string.IsNullOrWhiteSpace(host))
      {
        throw new ArgumentNullException(nameof(host), "Set the HOST setting in the environment variables.");
      }
      if (string.IsNullOrWhiteSpace(username))
      {
        throw new ArgumentNullException(nameof(username), "Set the USERNAME setting in the environment variables.");
      }
      if (string.IsNullOrWhiteSpace(password))
      {
        throw new ArgumentNullException(nameof(password), "Set the PASSWORD setting in the environment variables.");
      }

      return new MailClient(mail, host, username, password);
    }

    public MailClient(string mail, string host, string username, string password)
    {
      _mail = mail;
      _host = host;
      _username = username;
      _password = password;
    }
    /// <summary>
    /// Sends an e-mail.
    /// </summary>
    /// <param name="title">Message title.</param>
    /// <param name="message">Content of the message as HTML.</param>
    public void Send(string title, string message)
    {
      // Create a new message.
      MailMessage mailMessage = new(_mail, _mail, title, message)
      {
        // Enable support for HTML.
        IsBodyHtml = true
      };

      SmtpClient smtpClient = new(_host, 587)
      {
        EnableSsl = true,
        Credentials = new NetworkCredential(_username, _password)

      };
#if DEBUG
      Console.WriteLine("Sending messages during a debug session is disabled.");
#else
      smtpClient.Send(mailMessage);
#endif
    }
  }
}