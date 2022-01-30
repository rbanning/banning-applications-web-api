using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Services.Queue
{
    public class QueueService: IQueueService
    {
		private readonly QueueServiceClient _serviceClient;

		public QueueService(QueueServiceClient serviceClient)
		{
			_serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
		}

		protected async Task<QueueClient> GetQueueClient(string queueName)
		{
			QueueClient client = _serviceClient.GetQueueClient(queueName);
			await client.CreateIfNotExistsAsync();

			return client;
		}

		public Dtos.Queue.QueueMessageDto CreateMessage(string email, string subject, string message)
		{
			return new Dtos.Queue.QueueMessageDto()
			{
				Email = email,
				Subject = subject,
				Message = message
			};
		}

		public async Task<SendReceipt> SendMessageAsync(string queueName, Dtos.Queue.QueueMessageDto message)
		{
			if (message == null) { return null; }
			QueueClient client = await GetQueueClient(queueName);			
			Azure.Response<SendReceipt> response = await client.SendMessageAsync(message.Serialize());
			return response.Value ?? null;
		}


		public async Task<List<QueueMessage>> ReceiveMessagesAsncy(string queueName)
		{
			QueueClient client = await GetQueueClient(queueName);
			QueueMessage[] messages = await client.ReceiveMessagesAsync();
			return messages.ToList();
		}

	}
}
