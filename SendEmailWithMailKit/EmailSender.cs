using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SendEmailWithMailKit;

public class EmailSender
{
    public async Task SendAsync()
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse("<from email>"));
        message.To.Add(MailboxAddress.Parse("<to email>"));

        message.Subject = "Test Email Subject";
        message.Body = new TextPart("plain")
        {
            Text = "This is a test email sent using MailKit."
        };

        using var client = new CustomSmtpClient();
        client.Connect("smtp-mail.outlook.com", 587, SecureSocketOptions.StartTls); // this smtp host is outlook
        client.Authenticate("<from email>", "<password>");

        PrintCapabilities(client);
        
        client.MessageSent += (_, args) =>
        {
            // This responseUniqueId can use the validate delivery status with the "InReplyTo" or "Reference" in delivery report
            var responseUniqueId = ResponseUniqueId(args.Response);
            Console.WriteLine($"Message sent with status {args.Response} and {responseUniqueId} and Message-Id header value {message.MessageId}");
        };
        
        await client.SendAsync(message);
        client.Disconnect(true);
    }

    private void PrintCapabilities(SmtpClient client)
    {
        if (client.Capabilities.HasFlag (SmtpCapabilities.Size))
            Console.WriteLine ("The SMTP server has a size restriction on messages: {0}.", client.MaxSize);

        if (client.Capabilities.HasFlag (SmtpCapabilities.Dsn))
            Console.WriteLine ("The SMTP server supports delivery-status notifications.");

        if (client.Capabilities.HasFlag (SmtpCapabilities.EightBitMime))
            Console.WriteLine ("The SMTP server supports Content-Transfer-Encoding: 8bit");

        if (client.Capabilities.HasFlag (SmtpCapabilities.BinaryMime))
            Console.WriteLine ("The SMTP server supports Content-Transfer-Encoding: binary");

        if (client.Capabilities.HasFlag (SmtpCapabilities.UTF8))
            Console.WriteLine ("The SMTP server supports UTF-8 in message headers.");
    }

    private static string ResponseUniqueId(string response)
    {
        var start = response.IndexOf("<", StringComparison.Ordinal) + 1;
        var end = response.IndexOf("@", StringComparison.Ordinal);
        return response.Substring(start, end - start);
    }
}