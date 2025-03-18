using FindRobot.Interface;
using FindRobot.Interface.ServiceInfoCreatorServiceDependency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FindRobot.Services
{
    public class ServiceInfoCreatorService : IServiceInfoCreatorService
    {
        #region Services
        
        ILogger<ServiceInfoCreatorService> mLogger;

        #endregion

        #region Var

        private static readonly JsonSerializerOptions mJsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
        };

        #endregion

        #region Const

        private const string cServiceInfoDefaultName = "service_info.json";
        private const ProjectType cDefaultProjectType = ProjectType.DotnetProject;

        #endregion

        #region ~

        public ServiceInfoCreatorService(IServiceProvider serviceProvider) 
        {
            mLogger = serviceProvider.GetRequiredService<ILogger<ServiceInfoCreatorService>>();

            mJsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        #endregion


        public async void UpdateOrCreateServiceInfoFile()
        {
            try
            {
                //default repo root path
                var path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.Parent;

                ServiceInfoModel serviceInfo = new();
                serviceInfo = await ReadJsonFileAsync<ServiceInfoModel>(path.FullName, cServiceInfoDefaultName);
                
                Assembly entryAssembly = Assembly.GetEntryAssembly();

                string appVersion = entryAssembly.GetName().Version.ToString();
                string appName = entryAssembly.GetName().Name;

                if (!serviceInfo.Services.Version.Equals(appVersion))
                {
                    mLogger.LogWarning("Version in {file_name} changed!", cServiceInfoDefaultName);
                    mLogger.LogWarning("Old value: {serviceInfoVersion}, new value: {appVersion}", serviceInfo.Services.Version, appVersion);
                    serviceInfo.Services.Version = appVersion;
                }

                if (!serviceInfo.Services.Name.Equals(appName))
                {
                    mLogger.LogWarning("Name in {file_name} changed!", cServiceInfoDefaultName);
                    mLogger.LogWarning("Old value: {serviceInfoName}, new value: {appName}", serviceInfo.Services.Name, appName);
                    serviceInfo.Services.Name = appName;
                }

                if(serviceInfo.Services.ProjectType == ProjectType.None)
                {
                    mLogger.LogWarning("ProjectType in {file_name} changed!", cServiceInfoDefaultName);
                    mLogger.LogWarning("Old value: {serviceInfoName}, new value: {appName}", serviceInfo.Services.ProjectType, cDefaultProjectType);
                    serviceInfo.Services.ProjectType = ProjectType.DotnetProject;
                }

                await SerializeAndSaveJsonFilesAsync(serviceInfo, path.FullName, cServiceInfoDefaultName);
            }
            catch (Exception ex)
            {
                mLogger.LogError("{error}", ex.Message);
            }
        }


        #region PrivateMethods

        private static async Task<T> ReadJsonFileAsync<T>(string path, string fileName) where T : class, new()
        {
            string filePath = Path.Combine(path, fileName);

            if (!File.Exists(filePath))
            {
                return new T();
            }

            string jsonString = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(jsonString, mJsonSerializerOptions);
        }

        private static async Task SerializeAndSaveJsonFilesAsync<T>(T content, string path, string fileName) where T : class
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filePath = Path.Combine(path, fileName);

            string jsonString = JsonSerializer.Serialize(content, mJsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, jsonString);
        }

        #endregion
    }
}
