using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Services.Email
{
    public class EmailService
    {
		private IConfiguration _config;
		private Identity.RegisteredScopes.Scope _scope;

		public EmailService(IConfiguration config, string scope)
			:this(config, Identity.RegisteredScopes.Find(scope))
		{}

		public EmailService(IConfiguration config, Identity.RegisteredScopes.Scope scope)
		{
			_config = config;
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));
		}



		#region >> HIGH LEVEL (PUBLIC) METHODS <<

		public async Task<EmailResponse> SendEmailConfirmationLinkMessageAsync(EmailAddress to, string linkUrl, string linkLabel = "Activate Your Account", Identity.RegisteredContacts.ContactSet.WhichContact from = Identity.RegisteredContacts.ContactSet.WhichContact.Support, List<string> categories = null)
		{
			var template = BuildTemplate(EmailTemplates.TemplateType.transaction, "Activate Your Account", "Please confirm your email address");
			template.Fields["text"] = "Thank you for registering with our site and you are almost done.  Please use the link below to confirm your email address and activate your account. The activation link will expire in 3 days.";
			template.Fields["btn_href"] = linkUrl;
			template.Fields["btn_text"] = linkLabel;
			return await SendAsync(to, from, template, categories);
		}

		public async Task<EmailResponse> SendEmailConfirmationCodeMessageAsync(EmailAddress to, string code, string expires, Identity.RegisteredContacts.ContactSet.WhichContact from = Identity.RegisteredContacts.ContactSet.WhichContact.Support, List<string> categories = null)
		{
			var template = BuildTemplate(EmailTemplates.TemplateType.notification, "Activate Your Account", "Please confirm your email address");
			template.Fields["text"] = $"Thank you for registering with our site and you are almost done.  Please enter the code shown below into your app to activate your account. The activation code will expire {expires}.";
			template.Fields["large_text"] = $"CODE: {code}";
			return await SendAsync(to, from, template, categories);
		}

		public async Task<EmailResponse> SendPasswordResetLinkMessageAsync(EmailAddress to, string linkUrl, string linkLabel = "Reset Your Password", Identity.RegisteredContacts.ContactSet.WhichContact from = Identity.RegisteredContacts.ContactSet.WhichContact.Support, List<string> categories = null)
		{
			var template = BuildTemplate(EmailTemplates.TemplateType.transaction, "Forgot Your Password?", "Reset your password using the link below.");
			template.Fields["text"] = "Oops - Did you forget your password?  It happens to the best of us. Use the link below to reset your password. If you did not request to have your password reset, just ignore this email.  The password reset link will expire in 3 hours.";
			template.Fields["btn_href"] = linkUrl;
			template.Fields["btn_text"] = linkLabel;
			return await SendAsync(to, from, template, categories);
		}

		public async Task<EmailResponse> SendEmailPasswordResetCodeMessageAsync(EmailAddress to, string code, string expires, Identity.RegisteredContacts.ContactSet.WhichContact from = Identity.RegisteredContacts.ContactSet.WhichContact.Support, List<string> categories = null)
		{
			var template = BuildTemplate(EmailTemplates.TemplateType.notification, "Forgot Your Password?", "Reset your password using the code below.");
			template.Fields["text"] = $"Oops - Did you forget your password?  It happens to the best of us. Please enter the code shown below into your app to reset your password. If you did not request to have your password reset, just ignore this email.  The password reset code will expire {expires}.";
			template.Fields["large_text"] = $"CODE: {code}";
			return await SendAsync(to, from, template, categories);
		}


		public async Task<EmailResponse> SendEmailNotificationMessageAsync(EmailAddress to, string subject, string text, Identity.RegisteredContacts.ContactSet.WhichContact from = Identity.RegisteredContacts.ContactSet.WhichContact.Support, List<string> categories = null)
		{
			var template = BuildTemplate(EmailTemplates.TemplateType.notification, subject, $"{_scope.Contacts.Organization.Name} Notification");
			template.Fields["text"] = text;
			return await SendAsync(to, from, template, categories);
		}
		public async Task<EmailResponse> SendEmailNotificationMessageAsync(EmailAddress to, string subject, string text, EmailAddress from, List<string> categories = null)
		{
			var template = BuildTemplate(EmailTemplates.TemplateType.notification, subject, $"{_scope.Contacts.Organization.Name} Notification");
			template.Fields["text"] = text;
			return await SendAsync(to, from, template, categories);
		}

		#endregion


		#region >> LOW LEVEL (PUBLIC) METHODS <<


		public EmailTemplates.EmailTemplate BuildTemplate(EmailTemplates.TemplateType type, string subject, string preHeader, Dictionary<string, object> additionalData = null)
		{
			var template = EmailTemplates.Get(type);
			template.Subject = subject;
			template.PreHeader = preHeader;
			if (template.HasField("footer")) { template.Fields["footer"] = _scope.Contacts.Organization.ToHtmlString(); }
			if (template.HasField("title")) { template.Fields["title"] = _scope.Contacts.Organization.Name; }
			if (additionalData != null)
			{
				foreach (var item in additionalData)
				{
					if (template.HasField(item.Key))
					{
						template.Fields[item.Key] = item.Value;
					}
				}
			}

			return template;
		}

		public async Task<EmailResponse> SendAsync(EmailAddress to, Identity.RegisteredContacts.ContactSet.WhichContact from, EmailTemplates.EmailTemplate template, List<string> categories = null)
		{
			return await SendAsync(to, _scope.Contacts.Use(from).ToEmailAddress(), template, categories);
		}

		public async Task<EmailResponse> SendAsync(Identity.RegisteredContacts.ContactSet.WhichContact to,
			EmailAddress from, EmailTemplates.EmailTemplate template, List<string> categories = null)
		{
			return await SendAsync(_scope.Contacts.Use(to).ToEmailAddress(), from, template, categories);
		}

		public async Task<EmailResponse> SendAsync(EmailAddress to, EmailAddress from, EmailTemplates.EmailTemplate template, List<string> categories = null)
		{
			string message = $"{template.Name} email";
			try
			{
				if (categories == null) { categories = new List<string>(); }
				if (!categories.Contains(_scope.Code)) { categories.Add(_scope.Code); }

				var client = GetClient();
				var msg = BuildMessage(to, from, template.Id, template.ToTemplateData(), categories);
				// ReSharper disable once RedundantNameQualifier
				SendGrid.Response response = await client.SendEmailAsync(msg).ConfigureAwait(false);
				string responseBody = await response.Body.ReadAsStringAsync();
				return new EmailResponse() {
					Message = message + " sent",
					StatusCode = Convert.ToInt32(response.StatusCode),
					Body = responseBody,
					RawResponse = response
				};
			}
			catch (Exception ex)
			{
				return new EmailResponse() { ErrorMessage = ex.Message, Message = message + " error" };
			}
		}


		private SendGridMessage BuildMessage(EmailAddress to, EmailAddress from, string templateId, object templateData, List<string> categories = null)
		{
			var msg = new SendGridMessage()
			{
				From = from
			};

			msg.AddTo(to);
			msg.SetTemplateId(templateId);
			msg.SetTemplateData(templateData);

			if (categories != null)
			{
				msg.AddCategories(categories);
			}

			return msg;
		}

		private SendGridClient GetClient()
		{
			return new SendGridClient(ApiKey());
		}

		private string ApiKey()
		{
			return _config.GetValue<string>(ConfigKeys.EmailApi);
		}

		#endregion

	}
}
