using System;

namespace Wohnungssuche
{
  public class Mailing
  {
    /// <summary>
    /// Sends an e-mail.
    /// </summary>
    /// <param name="title">Message title.</param>
    /// <param name="message">Content of the message as HTML.</param>
    public static void Send(string title, string message)
    {
#if DEBUG
      // Infomeldung schreiben.
      Console.WriteLine("Sending messages during a debug session is disabled.");
#else
      // Get the smtp settings from the environment.
      string mail = Environment.GetEnvironmentVariable("SMTP_MAIL");
      string host = Environment.GetEnvironmentVariable("SMTP_HOST");
      string username = Environment.GetEnvironmentVariable("SMTP_USERNAME");
      string password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");


      if (String.IsNullOrWhiteSpace(mail))
      {
        throw new ArgumentNullException(nameof(mail), "Set the smtp mail setting in the environment variables.");
      }
      if (String.IsNullOrWhiteSpace(host))
      {
        throw new ArgumentNullException(nameof(host), "Set the smtp host setting in the environment variables.");
      }
      if (String.IsNullOrWhiteSpace(username))
      {
        throw new ArgumentNullException(nameof(username), "Set the smtp username setting in the environment variables.");
      }
      if (String.IsNullOrWhiteSpace(password))
      {
        throw new ArgumentNullException(nameof(password), "Set the smtp password setting in the environment variables.");
      }

      // Create a new message.
      MailMessage mail = new(mail, address, title, message)
      {
        // Enable support for HTML.
        IsBodyHtml = true
      };

      new SmtpClient(host, 587)
      {
        EnableSsl = true,
        Credentials = credentials

      }.Send(mail);
    }
#endif
    }
  }
}