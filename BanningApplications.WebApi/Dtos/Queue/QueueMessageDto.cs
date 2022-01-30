using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace BanningApplications.WebApi.Dtos.Queue
{
    public class QueueMessageDto
    {
        [Required, DataType(DataType.EmailAddress), MaxLength(250)]
		public string Email { get; set; }
		[Required, MaxLength(50)]
		public string Subject { get; set; }
		[Required, MaxLength(5000)]
		public string Message { get; set; }
		public DateTime DateCreated { get; set; }

		public QueueMessageDto()
		{
			this.DateCreated = DateTime.UtcNow;
		}

		public string Serialize()
		{
			var bytes = Encoding.Default.GetBytes(JsonSerializer.Serialize(this));
			return Convert.ToBase64String(bytes);
			//return Encoding.UTF8.GetString(bytes);
		}
	}
}
