using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedVariable

namespace BanningApplications.WebApi.Identity
{
    public static class RegisteredContacts
    {
		public enum ContactGroup
		{
			Hallpass,
			BanningApps,
		}

		private static Dictionary<ContactGroup, ContactSet> _directory;

		static RegisteredContacts()
		{
			var hallpassOrg = new Organization()
			{
				Name = "Hallpass and Friends",
				Address = "1107 Fair Oaks, #112",
				City = "So. Pasadena",
				ST = "CA",
				Zip = "91030"
			};

			var bannAppsOrg = new Organization()
			{
				Name = "Banning Applications",
				Address = "1107 Fair Oaks, #112",
				City = "So. Pasadena",
				ST = "CA",
				Zip = "91030"
			};

			var hallpass = new ContactSet()
			{
				Organization = hallpassOrg,
				Support = new Contact() { Name = "Hallpass Support", Email = "support@hallpassandfriends.com", Phone = "6269216880" },
				Admin = new Contact() { Name = "Hallpass Admin", Email = "admin@hallpassandfriends.com", Phone = "6269216880" },
				Info = new Contact() { Name = "Hallpass Info", Email = "info@hallpassandfriends.com", Phone = "6269216880" },
				Manager = new Contact() { Name = "Hallpass Manager", Email = "info@hallpassandfriends.com", Phone = "6269216880" },
			};
			var banningApps = new ContactSet()
			{
				Organization = bannAppsOrg,
				Support = new Contact() { Name = "Banning Apps Support", Email = "support@ibanning.com", Phone = "6269216880" },
				Admin = new Contact() { Name = "Banning Apps Admin", Email = "admin@ibanning.com", Phone = "6269216880" },
				Info = new Contact() { Name = "Banning Apps Info", Email = "admin@ibanning.com", Phone = "6269216880" },
				Manager = new Contact() { Name = "Banning Apps Manager", Email = "admin@ibanning.com", Phone = "6269216880" },
			};
			
			_directory = new Dictionary<ContactGroup, ContactSet>();

			_directory.Add(ContactGroup.Hallpass, hallpass);
			_directory.Add(ContactGroup.BanningApps, banningApps);
		}


		public static ContactSet Get(ContactGroup group)
		{
			if (_directory.Keys.Contains(group))
			{
				return _directory[group];
			}

			//else
			throw new ArgumentException($"Unsupported contact group - {group.ToString()}");
		}


		public class Contact
		{
			public string Name { get; set; }
			public string Email { get; set; }
			public string Phone { get; set; }

			public EmailAddress ToEmailAddress()
			{
				return new EmailAddress(Email, Name);
			}
		}

		public class ContactSet
		{
			public enum WhichContact
			{
				Support,
				Admin,
				Info,
				Manager
			}

			public Organization Organization { get; set; }
			public Contact Support { get; set; }
			public Contact Admin { get; set; }
			public Contact Info { get; set; }
			public Contact Manager { get; set; }

			public Contact Use(WhichContact which)
			{
				switch (which)
				{
					case WhichContact.Support:
						return Support;
					case WhichContact.Admin:
						return Admin;
					case WhichContact.Info:
						return Info;
					case WhichContact.Manager:
						return Manager;
					default:
						throw new NotSupportedException($"Could not find the requested contact - {which.ToString()}");
				}
			}
		}

		public class Organization
		{
			public string Name { get; set; }
			public string Address { get; set; }
			public string City { get; set; }
			// ReSharper disable once InconsistentNaming
			public string ST { get; set; }
			public string Zip { get; set; }

			public string ToHtmlString()
			{
				//return $"{Name} ▪ {Address} ▪ {City}, {ST} {Zip}";
				return $"{Name} ⋅ {Address} ⋅ {City}, {ST} {Zip}";
			}
		}
    }
}
