namespace ApplicationRegistry.Model
{
    public class ApplicationDiscoveryRouteItem
    {
        public string HttpVerb { get; set; }
        public string Route { get; set; }
        public string MethodName { get; set; }

        public ApplicationDiscoveryRouteItem(string httpVerb, string route, string methodName)
        {
            HttpVerb = httpVerb;
            Route = route;
            MethodName = methodName;
        }
    }
}
