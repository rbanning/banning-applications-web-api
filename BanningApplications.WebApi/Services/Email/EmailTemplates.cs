using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BanningApplications.WebApi.Services.Email
{
    public static class EmailTemplates
    {
        public enum TemplateType
		{
			notification,
			transaction,
			contact
		}

		private static Dictionary<TemplateType, EmailTemplate> _templates;

		static EmailTemplates()
		{
			_templates = new Dictionary<TemplateType, EmailTemplate>();
			_templates.Add(TemplateType.transaction, 
				new EmailTemplate("d-9323e43cff1b44b0ba93179e54e2e123", "Transaction",
					new string[] { "title", "text", "btn_href", "btn_text", "footer" }));
			_templates.Add(TemplateType.notification, 
				new EmailTemplate("d-9c1358594a0a4082b64c12582d7eb768", "Notification",
					new string[] { "title", "text", "large_text", "footer" }));
			_templates.Add(TemplateType.contact, 
				new EmailTemplate("d-39a63b93b1ba4c2aaf6b2bcc22747464", "Contact",
					new string[] { "name", "email", "phone", "client", "source", "text", "meta", "footer" }));
		}

		public static EmailTemplate Get(TemplateType type)
		{
			if (_templates.Keys.Contains(type))
			{
				return _templates[type];
			}
			//
			throw new ArgumentException($"Unsupported template type - {type.ToString()}");
		}

		public class EmailTemplate
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public string Subject { get; set; }
			public string PreHeader { get; set; }
			public Dictionary<string, object> Fields { get; set; }
			
			public EmailTemplate(string id, string name, string[] fields)
			{
				Id = id;
				Name = name;
				Fields = new Dictionary<string, object>();
				foreach (var item in fields)
				{
					Fields.Add(item, null);
				}
			}

			public bool HasField(string name)
			{
				return Fields.Keys.Contains(name);
			}

			public dynamic ToTemplateData()
			{
				var expandoObj = new ExpandoObject();
				var expandoObjCollection = (ICollection<KeyValuePair<string, object>>)expandoObj;

				//add the subject and preheader
				expandoObjCollection.Add(new KeyValuePair<string, object>("subject", Subject));
				expandoObjCollection.Add(new KeyValuePair<string, object>("header", PreHeader));

				foreach (var keyValuePair in Fields)
				{
					expandoObjCollection.Add(keyValuePair);
				}

				dynamic eoDynamic = expandoObj;
				return eoDynamic;
			}
		}
    }
}
