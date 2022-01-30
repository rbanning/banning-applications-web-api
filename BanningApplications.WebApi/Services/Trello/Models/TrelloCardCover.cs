using System;
using System.Collections.Generic;
using System.Linq;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloCardCover
    {
		public enum Colors
		{
			pink,
			yellow,
			lime,
			blue,
			black,
			orange,
			red,
			purple,
			sky,
			green
		}

		public enum Brightnesses
		{
			dark,
			light
		}

		public enum Sizes
		{
			normal,
			full
		}

		public string Color { get; set; }
		public string Brightness { get; set; }
		public string Size { get; set; }
		public string Url { get; set; }	//must be an Unsplash Url (https://images.unsplash.com...)

		public string IdAttachment { get; set; }  //Used if setting an attached image as the cover.

		public TrelloCardCover()
		{
			Brightness = null;
			Size = Sizes.normal.ToString();
		}

		public TrelloCardCover(Colors color)
			:this(color.ToString())
		{ }

		public TrelloCardCover(string color)
			:this()
		{
			this.Color = color;
		}

		public TrelloCardCover(Colors color, Brightnesses brightness, Sizes size)
		{
			Color = color.ToString();
			Brightness = brightness.ToString();
			Size = size.ToString();
		}

		public static List<string> ValidColors()
		{
			return Enum.GetNames(typeof(Colors)).ToList();
		}
		public static bool IsValidColor(string color)
		{
			return !string.IsNullOrEmpty(color) && ValidColors().Contains(color.ToLower());
		}
	}
}
