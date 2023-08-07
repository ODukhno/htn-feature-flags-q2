using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;

namespace ECSConfigProxy.ECS
{
    public sealed class ECSConfigCache
    {
        private const int PollingPeriodInSec = 30;

        private readonly ConcurrentDictionary<string, ECSConfig> _ecsConfigurationValues;
        private readonly ECSAuthInfo _authInfo;

        public ECSConfigCache(IOptions<ECSAuthInfo> authInfo)
        {
            _ecsConfigurationValues = new ConcurrentDictionary<string, ECSConfig>(StringComparer.OrdinalIgnoreCase);

            _authInfo = authInfo.Value;

            UpdateConfigurationOnce().GetAwaiter().GetResult();

            Task.Run(UpdateConfiguration);
        }

        public IEnumerable<KeyValuePair<string, ECSConfig>> GetAll()
        {
            foreach (KeyValuePair<string, ECSConfig> pair in _ecsConfigurationValues)
            {
                yield return new KeyValuePair<string, ECSConfig>(pair.Key, pair.Value);
            }
        } 

        private async Task UpdateConfiguration()
        {
            while (true)
            {
                await UpdateConfigurationOnce();

                await Task.Delay(TimeSpan.FromSeconds(PollingPeriodInSec));
            }
        }

        private async Task UpdateConfigurationOnce()
        {
            try
            {
                ClientSecretCredential clientSecretCredential = new ClientSecretCredential(
                    _authInfo.TenantId,
                    _authInfo.ClientId,
                    _authInfo.ClientSecret);

                TokenRequestContext tokenRequestContext =
                    new TokenRequestContext(
                        new string[] { "https://ecs.skype.com/.default", });

                string token = clientSecretCredential.GetToken(tokenRequestContext).Token;

                using HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var builder = new UriBuilder("https://ecs.skype.net/api/v1/configurations");
                var query = HttpUtility.ParseQueryString(builder.Query);
                query["client"] = "q2ffhackaton";
                query["team"] = "q2ffhackaton";
                query["details"] = "true";
                builder.Query = query.ToString();
                string url = builder.ToString();

                var httpResult = await httpClient.GetAsync(url);

                string result = await httpResult.Content.ReadAsStringAsync();

                PopulateDictionary(result);

                // TODO emit metric via open telemetry
                Console.WriteLine(DateTime.Now.ToString() + " Config updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void PopulateDictionary(string ecsConfigurationsJson)
        {
            JsonDocument doc = JsonDocument.Parse(ecsConfigurationsJson);
            JsonElement configurations = doc.RootElement.GetProperty("configurations");
            JsonElement.ArrayEnumerator configurationsEnumerator = configurations.EnumerateArray();

            while (configurationsEnumerator.MoveNext())
            {
                JsonElement current = configurationsEnumerator.Current;

                string configurationType = current.GetProperty("configurationType").GetString()!;
                bool isDefault = configurationType.Equals("Default", StringComparison.OrdinalIgnoreCase);

                JsonElement configs = current.GetProperty("configs");

                JsonElement.ArrayEnumerator configsEnumerator = configs.EnumerateArray();

                while (configsEnumerator.MoveNext())
                {
                    JsonElement currentConfig = configsEnumerator.Current;

                    string stageName = currentConfig.GetProperty("name").GetString() ?? "default";
                    double allocationPercentage = currentConfig.GetProperty("allocationPercentage").GetDouble();
                    int priority = isDefault ? int.MaxValue : currentConfig.GetProperty("priority").GetInt32();

                    List<Filter> filtersLst = new List<Filter>();
                    JsonElement filters = currentConfig.GetProperty("filters");
                    JsonElement.ArrayEnumerator filtersEnumerator = filters.EnumerateArray();

                    while (filtersEnumerator.MoveNext())
                    {
                        JsonElement filterElement = filtersEnumerator.Current;
                        filtersLst.Add(filterElement.Deserialize<Filter>()!);
                    }

                    JsonElement config = currentConfig.GetProperty("config");
                    JsonElement.ObjectEnumerator configEnumerator = config.EnumerateObject();

                    while (configEnumerator.MoveNext())
                    {
                        JsonProperty element = configEnumerator.Current;

                        string key = element.Name;
                        object newValue = element.Value;

                        ConfigValue configValue = new ConfigValue
                        {
                            VariantName = stageName,
                            IsDefault = isDefault,
                            AllocationPercentage = allocationPercentage,
                            Priority = priority,
                            Filters = filtersLst,
                            Value = newValue,
                        };

                        Func<string, ECSConfig> addFunc = (_key) =>
                        {
                            var value = new ConcurrentDictionary<string, ConfigValue>();
                            value.AddOrUpdate(configValue.VariantName, configValue, (_, _) => configValue);
                            return new ECSConfig { ConfigValues = value };
                        };  

                        Func<string, ECSConfig, ECSConfig> updateFunc = (_, _config) =>
                        {
                            _config.ConfigValues.AddOrUpdate(configValue.VariantName, configValue, (_, _) => configValue);
                            return _config;
                        };

                        _ecsConfigurationValues.AddOrUpdate(
                            key,
                            addFunc,
                            updateFunc);
                    }
                }
            }
        }
    }
}
