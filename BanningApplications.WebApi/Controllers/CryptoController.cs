using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace BanningApplications.WebApi.Controllers
{
	[Route("api/_crypto")]
	[AllowAnonymous]
	[ApiController]
    public class CryptoController: ApiBaseController
    {
	    private readonly List<string> _authApi = new List<string>
	    {
		    "5b8305c8-98a7-4466-8320-b496855974bf",
		    "e58e8859-796e-4f2a-8d39-27bf2c842940",
		    "0555d93b-f6b1-4d41-87eb-26ad3a4ea8c4"
	    };

	    [HttpPost("encrypt")]
	    public IActionResult Encrypt([FromBody] CryptoEncryptDto model)
	    {
		    try
		    {
			    var tuple = Helpers.CryptoExtensions.SymmetricEncryption.InitSymmetricEncryptionKey_IV();
			    var cipher = Helpers.CryptoExtensions.SymmetricEncryption.Encrypt(model.Text, tuple.key, tuple.iv);
			    return new OkObjectResult(new CryptoDecryptDto()
			    {
				    Key = tuple.key,
				    Iv = tuple.iv,
				    Cipher = cipher
			    });
		    }
		    catch (Exception e)
		    {
			    return new BadRequestObjectResult(new {error = e.Message});
		    }

	    }

	    [HttpPost("decrypt")]
	    public IActionResult Decrypt([FromBody] CryptoDecryptDto model)
	    {
		    try
		    {
			    var result = Helpers.CryptoExtensions.SymmetricEncryption.Decrypt(model.Cipher, model.Key, model.Iv);
			    return new OkObjectResult(new
			    {
				    result
			    });
		    }
		    catch (Exception e)
		    {
			    return new BadRequestObjectResult(new {error = e.Message});
		    }

	    }




	    protected bool AuthenticatePublicApi()
	    {
		    var auth = Request.Headers.FirstOrDefault(m => string.Equals(m.Key,"X-Hallpass-Api", StringComparison.CurrentCultureIgnoreCase)).Value;

		    return !(StringValues.IsNullOrEmpty(auth)) && _authApi.Exists(m => string.Equals(m, auth.First(), StringComparison.CurrentCultureIgnoreCase));

	    }


	    public class CryptoEncryptDto
	    {
			[Required]
		    public string Text { get; set; }
	    }
	    public class CryptoDecryptDto
	    {

			[Required]
		    public string Key { get; set; }
			[Required]
		    public string Iv { get; set; }
			[Required]
		    public string Cipher { get; set; }
	    }
    }
}
