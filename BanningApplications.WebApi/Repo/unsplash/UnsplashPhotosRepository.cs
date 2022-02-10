using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BanningApplications.WebApi.Data;
using BanningApplications.WebApi.Dtos;
using BanningApplications.WebApi.Dtos.unsplash;
using BanningApplications.WebApi.Entities.unsplash;
using BanningApplications.WebApi.Helpers;

namespace BanningApplications.WebApi.Repo.unsplash
{

	public interface IUnsplashPhotosRepository: IGenericBaseRepository<UnsplashPhoto>
	{
        //GETTERS
        Task<IList<UnsplashPhoto>> GetAllByPhotographerAsync(string username, bool inclArchived = false);
        Task<IList<UnsplashPhoto>> GetAllByTagAsync(string tag, bool inclArchived = false);
        Task<IList<UnsplashPhoto>> GetAllByTopicAsync(string topic, bool inclArchived = false);
	}

    public class UnsplashPhotosRepository: GenericBaseRepository<UnsplashPhoto, UnsplashDbContext>, IUnsplashPhotosRepository
    {
	    public UnsplashPhotosRepository(UnsplashDbContext context)
		    :base(context)
	    {}

	    #region >> Custom GETTERS <<

	    public async Task<IList<UnsplashPhoto>> GetAllByPhotographerAsync(string username, bool inclArchived = false)
	    {
		    return await GetAllAsync(m => m.UserName == username, inclArchived);
	    }

	    public async Task<IList<UnsplashPhoto>> GetAllByTagAsync(string tag, bool inclArchived = false)
	    {
		    return await GetAllAsync(m => m.TagsJson.Contains(tag), inclArchived);
	    }

	    public async Task<IList<UnsplashPhoto>> GetAllByTopicAsync(string topic, bool inclArchived = false)
	    {
		    return await GetAllAsync(m => m.TopicsJson.Contains(topic), inclArchived);
	    }


	    #endregion

	    #region >> PATCHES <<

	    protected override UnsplashPhoto PatchAdd(UnsplashPhoto model, PatchDto patch, string modifiedBy)
	    {
		    var processed = false;

		    switch (patch.Path.ToLowerInvariant())
		    {
			    case "/tag":
			    case "/tags":
				    model.TagsJson = model.TagsJson.AddToListString(patch.Value);
				    processed = true;
				    break;
			    case "/topic":
			    case "/topics":
				    model.TopicsJson = model.TopicsJson.AddToListString(patch.Value);
				    processed = true;
				    break;
		    }

		    if (processed)
		    {
			    return UpdateEntityMeta(model, modifiedBy);
		    }
		    //else
		    return model;

	    }

	    protected override UnsplashPhoto PatchRemove(UnsplashPhoto model, PatchDto patch, string modifiedBy)
	    {
		    var processed = false;

		    switch (patch.Path.ToLowerInvariant())
		    {
			    case "/tag":
			    case "/tags":
				    model.TagsJson = model.TagsJson.RemoveFromListString(patch.Value);
				    processed = true;
				    break;
			    case "/topic":
			    case "/topics":
				    model.TopicsJson = model.TopicsJson.RemoveFromListString(patch.Value);
				    processed = true;
				    break;
		    }

		    if (processed)
		    {
			    return UpdateEntityMeta(model, modifiedBy);
		    }
		    //else
		    return model;
	    }

	    protected override UnsplashPhoto PatchReplace(UnsplashPhoto model, PatchDto patch, string modifiedBy)
	    {
		    var processed = false;

		    switch (patch.Path.ToLowerInvariant())
		    {
				case "/username":
					model.UserName = patch.Value;
					processed = true;
					break;
				case "/blur":
				case "/blurhash":
					model.BlurHash = patch.Value;
					processed = true;
					break;
				case "/desc":
				case "/description":
					model.Description = patch.Value;
					processed = true;
					break;
				case "/alt":
				case "/altdescription":
					model.AltDescription = patch.Value;
					processed = true;
					break;
				case "/color":
					model.Color = patch.Value;
					processed = true;
					break;

				//numbers
				case "/width":
					if (int.TryParse(patch.Value, out int intWidth))
					{
						model.Width = intWidth;
						processed = true;
						break;
					}
					else
					{
						throw new PatchException($"Invalid integer value for {patch.Path}");
					}
				case "/height":
					if (int.TryParse(patch.Value, out int intHeight))
					{
						model.Height = intHeight;
						processed = true;
						break;
					}
					else
					{
						throw new PatchException($"Invalid integer value for {patch.Path}");
					}
					

				//dates
				case "/published":
					if (DateTime.TryParse(patch.Value, out DateTime pubTime))
					{
						model.Published = pubTime;
						processed = true;
						break;
					}
					else
					{
						throw new PatchException($"Invalid datetime value for ${patch.Path}");
					}

				// lists
			    case "/tag":
			    case "/tags":
					//validate
					var tags = patch.Value.ToListString();
				    model.TagsJson = tags.ToDelimString();
				    processed = true;
				    break;
			    case "/topic":
			    case "/topics":
					//validate
					var topics = patch.Value.ToListString();
				    model.TopicsJson = topics.ToDelimString();
				    processed = true;
				    break;


				// location
				case "/location":
					UnsplashLocationDto location;
					try
					{
						location = patch.Value.Deserialize<UnsplashLocationDto>();
					}
					catch (Exception)
					{
						location = new UnsplashLocationDto()
						{
							Name = patch.Value
						};
					}

					model.Location = location?.Serialize();
					processed = true;
					break;

				//Urls
				case "url":
				case "urls":
					UnsplashPhotoUrlsDto urls;
					try
					{
						urls = patch.Value.Deserialize<UnsplashPhotoUrlsDto>();
					}
					catch (Exception)
					{
						throw new PatchException("Invalid urls object");
					}

					model.UrlsJson = urls?.Serialize();
					processed = true;
					break;
		    }

		    if (processed)
		    {
			    return UpdateEntityMeta(model, modifiedBy);
		    }
		    //else
		    return model;
	    }

	    #endregion
    }
}
