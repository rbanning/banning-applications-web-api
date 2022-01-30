//using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace BanningApplications.WebApi.Helpers
{
    public static class GeoLocationExtensions
    {
		public const int SRID = 4326;

		public static GeometryFactory CreateGeometryFactory()
		{
			return NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: SRID);
		}

		public static Point CreatePoint(double longitude, double latitude)
		{
			return CreateGeometryFactory().CreatePoint(new Coordinate(longitude, latitude));
		}
	}
}
