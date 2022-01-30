using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;

namespace BanningApplications.WebApi.Services.PDF
{

	public class PdfTransformationOptions
	{
		public bool Readonly { get; set; }
		public bool Flatten { get; set; }

		public PdfTransformationOptions()
		{
			Readonly = true;
			Flatten = false;
		}

		public static PdfTransformationOptions Editable =>
			new PdfTransformationOptions() {Readonly = false, Flatten = false};
		public static PdfTransformationOptions Protected =>
			new PdfTransformationOptions() {Readonly = true, Flatten = false};
		public static PdfTransformationOptions Flat =>
			new PdfTransformationOptions() {Readonly = true, Flatten = true};
	}

	public interface IPdfService
	{
		Stream CompleteForm(Stream inputStream, Dictionary<string, string> model, PdfTransformationOptions options = null);

        //HELPERS
        ICollection GetFormFields(Stream pdfStream);
        Hashtable GetFormValues(Stream pdfStream);
	}

	

    public class PdfService: IPdfService
    {

	    #region >> FORMS <<

	    public Stream CompleteForm(Stream inputStream, Dictionary<string, string> model, PdfTransformationOptions options = null)
	    {
		    model ??= new Dictionary<string, string>(); //in case a null dictionary is passed in
		    options ??= PdfTransformationOptions.Protected;

		    Stream outStream = new MemoryStream();
		    PdfReader reader = null;
		    PdfStamper stamper = null;

		    try
		    {
			    reader = new PdfReader(inputStream);
			    stamper = new PdfStamper(reader, outStream);
			    AcroFields form = stamper.AcroFields;
			    List<string> fields = form.Fields.Keys.Cast<string>().ToList();

				//set the fields
				foreach (var prop in model)
				{
					if (fields.Contains(prop.Key))
					{
						form.SetField(prop.Key, prop.Value);
						if (options.Readonly)
						{
							form.SetFieldProperty(prop.Key, "setfflags", PdfFormField.FF_READ_ONLY, null);  //mark as readonly
						}
					}
				}

				//flatten
				if (options.Flatten)
				{
					stamper.FormFlattening = true;
				}

				//DONE
				return outStream;

			}
			catch (PdfServiceException)
		    {
			    throw;
		    }
		    catch (Exception ex)
		    {
			    throw new PdfServiceException("Error completing the form fields from PDF file", ex);
		    }
		    finally
			{
				stamper?.Close();
				reader?.Close();
		    }
	    }

		#endregion

		#region >> HELPERS <<

		public ICollection GetFormFields(Stream pdfStream)
	    {
		    PdfReader reader = null;
		    try
		    {
			    reader = new PdfReader(pdfStream);
			    return reader.AcroFields.Fields.Keys;
		    }
		    catch (PdfServiceException)
		    {
			    throw;
		    }
		    catch (Exception ex)
		    {
			    throw new PdfServiceException("Error getting the form fields from PDF file", ex);
		    }
		    finally
		    {
			    reader?.Close();
		    }
	    }


		public Hashtable GetFormValues(Stream pdfStream)
	    {
		    PdfReader reader = null;
		    try
		    {
			    var ret = new Hashtable();
			    reader = new PdfReader(pdfStream);
			    var fields = reader.AcroFields;
				
			    foreach (var field in fields.Fields.Keys)
			    {
				    if (!ret.ContainsKey(field))
				    {
					    string value = fields.GetField(field.ToString());
					    ret.Add(field, value);
				    }
				}

			    return ret;
		    }
		    catch (PdfServiceException)
		    {
			    throw;
		    }
		    catch (Exception ex)
		    {
			    throw new PdfServiceException("Error getting the form values from PDF file", ex);
		    }
		    finally
		    {
			    reader?.Close();
		    }
	    }



		#endregion
	}
}
