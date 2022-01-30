using System.Collections.Generic;
using BanningApplications.WebApi.Services.Trello.Models.TrelloDtos;

namespace BanningApplications.WebApi.Services.Trello.Models
{
    public class TrelloStickerBase
    {
	    public string Image { get; set; }
	    public string ImageUrl { get; set; }
	    public double Left { get; set; }
	    public double Top { get; set; }
	    public byte ZIndex { get; set; }
	    public double Rotate { get; set; }

	    public TrelloStickerBase()
	    {
		    //defaults
		    Top = 0;
		    Left = 0;
		    ZIndex = 1;
	    }

	    public string Serialize()
	    {
		    return TrelloDtoSerializer.Serialize(this);
	    }

	}

    public class TrelloSticker: TrelloStickerBase
    {
	    public string Id { get; set; }
    }

    public class TrelloStickerAlt : TrelloStickerBase
    {
		public string Id { get; set; }
		// ReSharper disable once InconsistentNaming
		public string _id { get; set; }


		public TrelloSticker ToSticker()
		{
			return new TrelloSticker()
			{
				Id = Id ?? _id,
				Image = Image,
				ImageUrl = ImageUrl,
				Left = Left,
				Top = Top,
				ZIndex = ZIndex,
				Rotate = Rotate
			};
		}
    }

    public interface IStickerResult
    {
	    public List<TrelloStickerAlt> Stickers { get; set; }
    }

}
