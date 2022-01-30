using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloPrefs
    {
		public enum PermissionLevelEnum
		{
			org,
			board,
			@private,
			@public
		}

		public enum VotingEnum
		{
			disabled,
			enabled,
			members,
			observers,
			org,
			@public
		}



		public PermissionLevelEnum PermissionLevel { get; set; }
		public bool HideVotes { get; set; }
		public VotingEnum Voting { get; set; }
		public string Comments { get; set; }
		public bool IsTemplate { get; set; }
		public bool CalendarFeedEnabled { get; set; }
		public string Background { get; set; }
		public string BackgroundImage { get; set; }
		public List<BackgroundImageScaledClass> BackgroundImageScaled { get; set; }


		public class BackgroundImageScaledClass
		{
			public int Width { get; set; }
			public int Height { get; set; }
			public string Url { get; set; }
		}
    }
}
