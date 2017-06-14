using Microsoft.Maps.MapControl;

namespace GENIVisuals.viewClasses
{
    public class BingAppId
    {
        //
        // You must enter a valid Bing Maps key in the line below.
        //
        static private string BingMapsApplicationKey = "ReplaceWithAValidBingMapsAppKey";
        static public ApplicationIdCredentialsProvider Provider = new ApplicationIdCredentialsProvider(BingMapsApplicationKey);
    }
}
