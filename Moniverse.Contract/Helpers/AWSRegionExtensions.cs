namespace Moniverse.Contract
{
    public static class AWSRegionExtensions
    {
        public static string GetRegionName(this AWSRegion region)
        {
            string result = string.Empty;

            switch (region)
            {
                case AWSRegion.All:
                    result = "All Regions";
                    break;
                case AWSRegion.USEast_NorthVirg:
                    result = "US East (Northern Virginia)";
                    break;
                case AWSRegion.USWest_NorthCali:
                    result = "US West (Northern California)";
                    break;
                case AWSRegion.USWest_Oregon:
                    result = "US West (Oregon)";
                    break;
                case AWSRegion.EU_Ireland:
                    result = "EU (Ireland)";
                    break;
                case AWSRegion.AsiaPac_Singapore:
                    result = "Asia Pacific (Singapore)";
                    break;
                case AWSRegion.AsiaPac_Sydney:
                    result = "Asia Pacific (Sydney)";
                    break;
                case AWSRegion.AsiaPac_Tokyo:
                    result = "Asia Pacific (Tokyo)";
                    break;
                case AWSRegion.SouthAmer_SaoPaulo:
                    result = "South America (Sao Paulo)";
                    break;
            }

            return result;
        }

        public static string GetDatabaseString(this AWSRegion region)
        {

            string awsRegionString = "%%";

            switch (region)
            {
                case AWSRegion.USWest_Oregon:
                    awsRegionString = "%US West (Oregon)%";
                    break;
                case AWSRegion.USWest_NorthCali:
                    awsRegionString = "%US West (Northern California)%";
                    break;
                case AWSRegion.USEast_NorthVirg:
                    awsRegionString = "%US East (Northern Virginia)%";
                    break;
                case AWSRegion.SouthAmer_SaoPaulo:
                    awsRegionString = "%South America (Sao Paulo)%";
                    break;
                case AWSRegion.EU_Ireland:
                    awsRegionString = "%EU (Ireland)%";
                    break;
                case AWSRegion.AsiaPac_Tokyo:
                    awsRegionString = "%Asia Pacific (Tokyo)%";
                    break;
                case AWSRegion.AsiaPac_Sydney:
                    awsRegionString = "%Asia Pacific (Sydney)%";
                    break;
                case AWSRegion.AsiaPac_Singapore:
                    awsRegionString = "%Asia Pacific (Singapore)%";
                    break;
                case AWSRegion.All:
                default:
                    awsRegionString = "%%";
                    break;

            }
            return awsRegionString;
        }
    }
}