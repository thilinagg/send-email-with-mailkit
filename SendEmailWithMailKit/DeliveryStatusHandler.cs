using MimeKit;

namespace SendEmailWithMailKit;

public class DeliveryStatusHandler
{
    public async Task ProcessDeliveryStatusNotification()
    {
        // In real scenario we should real this eml file via some other methodology. Like Azure function app or logic app 
        const string fileName = "Undeliverable_ Test Email.eml";
        var message = await GetMimeMessageFromEmlFile(fileName);

        if (message.Body is not MultipartReport report
            || report.ReportType == null
            || !report.ReportType.Equals("delivery-status", StringComparison.OrdinalIgnoreCase))
        {
            // this is not a delivery status notification message...
            return;
        }

        var messageId = message.MessageId;
        var responseUniqueId = ResponseUniqueId(message.InReplyTo);

        foreach (MimeEntity entity in report)
        {
            if (entity is not MessageDeliveryStatus mds)
                continue;

            ProcessReport(mds, messageId, responseUniqueId);
        }
    }

    private async Task<MimeMessage> GetMimeMessageFromEmlFile(string fileName)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "DeliveryNotifyEmlResource", fileName);
        var fileContent = await File.ReadAllBytesAsync(filePath);
        return await MimeMessage.LoadAsync(new MemoryStream(fileContent));
    }
    
    private static string ResponseUniqueId(string inReplyTo)
    {
        var start = inReplyTo.IndexOf("<", StringComparison.Ordinal) + 1;
        var end = inReplyTo.IndexOf("@", StringComparison.Ordinal);
        return inReplyTo.Substring(start, end - start);
    }

    private static void ProcessReport(MessageDeliveryStatus mds, string messageId, string responseUniqueId)
    {
        for (var i = 1; i < mds.StatusGroups.Count; i++)
        {
            var recipient = mds.StatusGroups[i]["Original-Recipient"];
            recipient ??= mds.StatusGroups[i]["Final-Recipient"];

            if (recipient is null)
                return;

            var action = mds.StatusGroups[i]["Action"];

            switch (action)
            {
                case "failed":
                    Console.WriteLine("Delivery of responseUniqueId {0} and message id {1} failed for {2}",
                        responseUniqueId, messageId, RecipientEmail(recipient));
                    break;
                case "delayed":
                    Console.WriteLine("Delivery of responseUniqueId {0} and message id {1} has been delayed for {2}",
                        responseUniqueId, messageId, RecipientEmail(recipient));
                    break;
                case "delivered":
                    Console.WriteLine("Delivery of responseUniqueId {0} and message id {1} has been delivered to {2}",
                        responseUniqueId, messageId, RecipientEmail(recipient));
                    break;
                case "relayed":
                    Console.WriteLine("Delivery of responseUniqueId {0} and message id {1} has been relayed for {2}",
                        responseUniqueId, messageId, RecipientEmail(recipient));
                    break;
                case "expanded":
                    Console.WriteLine(
                        "Delivery of responseUniqueId {0} and message id {1} has been delivered to {2} and relayed to the the expanded recipients",
                        responseUniqueId, messageId, RecipientEmail(recipient));
                    break;
            }
        }
    }

    private static string RecipientEmail(string recipient)
    {
        var index = recipient.IndexOf(';');
        return recipient[(index + 1)..];
    }
}