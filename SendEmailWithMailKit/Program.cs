using SendEmailWithMailKit;

var emailSender = new EmailSender();
await emailSender.SendAsync();

// var deliveryStatusHandler = new DeliveryStatusHandler();
// await deliveryStatusHandler.ProcessDeliveryStatusNotification();

Console.ReadLine();