using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Services.Queue
{
    public interface IQueueService
    {
		Dtos.Queue.QueueMessageDto CreateMessage(string email, string subject, string message);
		Task<SendReceipt> SendMessageAsync(string queueName, Dtos.Queue.QueueMessageDto message);
		Task<List<QueueMessage>> ReceiveMessagesAsncy(string queueName);
    }
}
