using ApplicationRegistry.Model;
using Common.Dependencies;
using Log;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace ApplicationRegistry.Common
{
    public static class Helpers
    {
        private static IConfiguration Configuration { get; set; }
        private static LogInstance Log { get; set; } = LogInstance.CreateLog();
        public static async Task<ApplicationRegistryEntry> GetRegistryEntry(LogInstance log, IConfiguration configuration)
        {
            Log ??= log;
            Configuration ??= configuration;

            AssemblyName applicationEntryAssembly = Assembly.GetEntryAssembly().GetName();
            ApplicationRegistryEntry registryEntry = new ApplicationRegistryEntry()
            {
                ApplicationName = applicationEntryAssembly.Name,
                ApplicationVersion = applicationEntryAssembly.Version?.ToString(),
                MachineName = Environment.MachineName,
                ApplicationPath = AppContext.BaseDirectory,
                BuildDateTime = new FileInfo(Assembly.GetEntryAssembly().Location).CreationTimeUtc   //RSH 2/1/24 - this may not be accurate, but, at least we have a date
            };
            registryEntry.ApplicationHash = await GetApplicationHash(registryEntry.ApplicationPath).ConfigureAwait(false);
            if (Dependencies.Hash == null)
            {
                Dependencies.Hash = registryEntry.ApplicationHash;
                Dependencies.Path = AppContext.BaseDirectory;

                Dependencies.ApplicationName = applicationEntryAssembly.Name;
            }
            return registryEntry;
        }

        public static IEnumerable<Assembly> GetAssemblies()
        {
            List<string> list = new List<string>();
            Stack<Assembly> stack = new Stack<Assembly>();
            stack.Push(Assembly.GetEntryAssembly());

            do
            {
                Assembly assembly = stack.Pop();
                yield return assembly;
                foreach (AssemblyName reference in assembly.GetReferencedAssemblies())
                {
                    if (!list.Contains(reference.FullName))
                    {
                        try
                        {
                            string assemblyName = reference.Name;
                            if (!assemblyName.StartsWith("System.") && !assemblyName.StartsWith("Microsoft."))
                            {
                                stack.Push(Assembly.Load(reference));
                                Log.LogDebug("Adding assembly {assemblyFullName}", reference.FullName);
                                list.Add(reference.FullName);
                            }
                            else
                            {
                                Log.LogDebug("Skipping assembly {assemblyName}", assemblyName);
                            }
                        }
                        catch
                        {
                            //RSH 6/16/21 - ignore - we may be looking at a MS .Net library - which we don't care about
                        }
                    }
                }
            }
            while (stack.Count > 0);
        }

        public static async Task<string> GetApplicationHash(string appPath)
        {
            if (string.IsNullOrEmpty(appPath))
            {
                throw new ArgumentException($"'{nameof(appPath)}' cannot be null or empty", nameof(appPath));
            }

            List<byte> byteList = new List<byte>();
            IEnumerable<Assembly> assemblies = GetAssemblies();
            foreach (Assembly assembly in assemblies.OrderBy((assembly) => assembly.FullName))
            {
                string location = assembly.Location;
                if (string.Equals(System.IO.Path.GetDirectoryName(location) + "\\", appPath))
                {
                    byte[] data = await System.IO.File.ReadAllBytesAsync(location).ConfigureAwait(false);
                    using (MD5 md5Hash = MD5.Create())
                    {
                        byte[] hash = md5Hash.ComputeHash(data);
                        byteList.AddRange(hash);
                    }
                }
            }
            return Convert.ToBase64String(byteList.ToArray());
        }

        public static List<Model.ApplicationDiscoveryEntry> GetDiscoveryEntries(int applicationInstanceID, int versionID)
        {
            List<Model.ApplicationDiscoveryEntry> discoveryEntries = new List<Model.ApplicationDiscoveryEntry>();
            IEnumerable<Assembly> assemblies = GetAssemblies();

            //string appPathRaw= AppContext.BaseDirectory;
            string appPath = GetApplicationBase();
            foreach (Assembly assembly in assemblies)
            {
                if (string.Equals(System.IO.Path.GetDirectoryName(assembly.Location) + "\\", appPath))
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        IEnumerable<string> customClassAttributes = type.CustomAttributes?.Select((attribute) => attribute.AttributeType.Name);
                        if (typeof(ControllerBase).IsAssignableFrom(type) || customClassAttributes.Contains("ApiControllerAttribute"))
                        {
                            SetupDiscoveryEntry(versionID, applicationInstanceID, discoveryEntries, type, out ParameterInfo parameter, out ApplicationDiscoveryEntry entry);
                            if (parameter != null && entry != null)
                            {
                                SetupDiscoveryURLs(entry);
                                SetupDiscoveryMethods(type, parameter, entry);
                            }
                        }
                    }
                }
            }
            return discoveryEntries;
        }

        private static void SetupDiscoveryEntry(int versionID, int applicationInstanceID, List<Model.ApplicationDiscoveryEntry> discoveryEntries, Type type, out ParameterInfo parameter, out ApplicationDiscoveryEntry entry)
        {
            string controllerName = string.Empty;
            controllerName = type.Name.Replace("Controller", "", StringComparison.OrdinalIgnoreCase);
            CustomAttributeData baseRouteAttribute = type.CustomAttributes?.Where((attr) => typeof(RouteAttribute).IsAssignableFrom(attr.AttributeType)).FirstOrDefault();

            if (baseRouteAttribute != null)
            {
                parameter = baseRouteAttribute.Constructor.GetParameters().Where((param) => param.Name == "template").FirstOrDefault();

                string baseRoute = string.Empty;
                if (parameter != null)
                {
                    baseRoute = (string)baseRouteAttribute.ConstructorArguments[parameter.Position].Value;
                }

                if (!string.IsNullOrEmpty(baseRoute))
                {
                    if (baseRoute.Contains("[controller]", StringComparison.OrdinalIgnoreCase))
                    {
                        baseRoute = baseRoute.Replace("[controller]", controllerName, StringComparison.OrdinalIgnoreCase);
                    }
                }

                string friendlyName = controllerName;
                CustomAttributeNamedArgument arguments = baseRouteAttribute.NamedArguments.Where((arg) => string.Equals(arg.MemberName, "Name", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (parameter != null)
                {
                    string typedValue = (string)arguments.TypedValue.Value;
                    if (!string.IsNullOrEmpty(typedValue))
                    {
                        friendlyName = typedValue;
                    }
                }

                entry = new ApplicationDiscoveryEntry()
                {
                    ControllerName = controllerName,
                    ControllerRoute = baseRoute,
                    FriendlyName = friendlyName,
                    ApplicationVersionID = versionID,
                    ApplicationInstanceID = applicationInstanceID
                };
                discoveryEntries.Add(entry);
            }
            else
            {
                parameter = null;
                entry = null;
            }
        }

        private static void SetupDiscoveryURLs(Model.ApplicationDiscoveryEntry entry)
        {
            string urls = Configuration.GetValue("urls");
            if (!string.IsNullOrEmpty(urls))
            {
                string[] urlArray = urls.Split(";");
                foreach (string url in urlArray)
                {
                    string urlNoPort = url;
                    int? port = null;
                    if (url.Contains(':') && url.IndexOf(':') < url.LastIndexOf(':'))
                    {
                        urlNoPort = url[..url.LastIndexOf(':')];
                        if (int.TryParse(url.AsSpan(url.LastIndexOf(':') + 1), out int tmpPort))
                        {
                            port = tmpPort;
                        }
                    }
                    ApplicationDiscoveryURL discoveryURL = new ApplicationDiscoveryURL()
                    {
                        URL = urlNoPort,
                        Port = port
                    };
                    entry.ApplicationDiscoveryURLs.Add(discoveryURL);
                }
            }
            else
            {
                string urlHost = Configuration.GetValue("urlHost", "");
                if (!string.IsNullOrEmpty(urlHost))
                {
                    if (Configuration.GetBoolValueWithDefault("EnableHttps", false))
                    {
                        if (Configuration.GetIntValueWithDefault("HttpsPort", 0) > 0)
                        {
                            ApplicationDiscoveryURL discoveryURL = new ApplicationDiscoveryURL()
                            {
                                URL = $"https://{urlHost}",
                                Port = Configuration.GetIntValueWithDefault("HttpsPort", 0)
                            };
                            entry.ApplicationDiscoveryURLs.Add(discoveryURL);
                        }
                    }
                    if (Configuration.GetBoolValueWithDefault("EnableHttp", false))
                    {
                        if (Configuration.GetIntValueWithDefault("HttpPort", 0) > 0)
                        {
                            ApplicationDiscoveryURL discoveryURL = new ApplicationDiscoveryURL()
                            {
                                URL = $"http://{urlHost}",
                                Port = Configuration.GetIntValueWithDefault("HttpPort", 0)
                            };
                            entry.ApplicationDiscoveryURLs.Add(discoveryURL);
                        }
                    }

                    if (entry.ApplicationDiscoveryURLs.Count == 0)
                    {
                        Log.LogError("No URLs found for application {applicationName}.  Check urls config or urlshost/enablehttps/httpsport/httpport", Dependencies.ApplicationName);
                    }
                }
                else
                {
                    Log.LogError("No URLs found for application {applicationName}.  Check urls config or urlshost/enablehttps/httpsport/httpport", Dependencies.ApplicationName);
                }
            }
        }

        private static void SetupDiscoveryMethods(Type type, ParameterInfo parameter, ApplicationDiscoveryEntry entry)
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
            {
                if (!method.GetCustomAttributes<CompilerGeneratedAttribute>(true).Any())
                {
                    IList<CustomAttributeData> data = method.GetCustomAttributesData();
                    string httpVerb = string.Empty;
                    string templateRoute = string.Empty;

                    foreach (CustomAttributeData attribute in data)
                    {
                        if (typeof(HttpMethodAttribute).IsAssignableFrom(attribute.AttributeType))
                        {
                            parameter = attribute.Constructor.GetParameters().Where((param) => param.Name == "template").FirstOrDefault();

                            httpVerb = attribute.AttributeType.Name.Replace("Http", "", StringComparison.OrdinalIgnoreCase).Replace("Attribute", "", StringComparison.OrdinalIgnoreCase);
                            if (parameter != null)
                            {
                                templateRoute = (string)attribute.ConstructorArguments[parameter.Position].Value;
                            }
                            methods.Add(method);

                            Model.ApplicationDiscoveryMethod discoveryMethod = new Model.ApplicationDiscoveryMethod()
                            {
                                HttpMethod = httpVerb,
                                MethodName = method.Name,
                                Template = templateRoute
                            };

                            Log.LogDebug("Found Http attribute {httpVerb} for method {methodName} with route {route}", httpVerb, method.Name, templateRoute);
                            entry.ApplicationDiscoveryMethods.Add(discoveryMethod);
                        }
                        else
                        {
                            Log.LogDebug("Found unrecognized attribute {attributeName} for method {methodName}", attribute.AttributeType.Name, method.Name);
                        }
                    }
                }
            }
        }

        public static string GetApplicationBase()
        {
            string appPathRaw = AppContext.BaseDirectory;
            if (appPathRaw.Contains("Tests", StringComparison.OrdinalIgnoreCase))
            {
                string[] pathElements = appPathRaw.Split("\\");
                string appPath = "";
                int index = 0;
                while (index < pathElements.Length && pathElements[index].ToLower() != "tests")
                {
                    if (index == 0)
                    {
                        appPath = pathElements[index];
                    }
                    else
                    {
                        appPath += "\\" + pathElements[index];
                    }
                    index++;
                }
                return appPath;
            }
            return appPathRaw;
        }
    }
}