using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

namespace SendEmailWithMailKit;

public class CustomSmtpClient: SmtpClient
{
    protected override string GetEnvelopeId (MimeMessage message)
    {
        return message.MessageId;
    }
    
    protected override DeliveryStatusNotification? GetDeliveryStatusNotifications (MimeMessage message, MailboxAddress mailbox)
    {
        return DeliveryStatusNotification.Success | DeliveryStatusNotification.Failure;
    }
}