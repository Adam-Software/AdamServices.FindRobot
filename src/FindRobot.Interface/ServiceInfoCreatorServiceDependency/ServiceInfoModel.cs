using System.Collections.Generic;

namespace FindRobot.Interface.ServiceInfoCreatorServiceDependency
{
    public class ServiceInfoModel
    {
        public ServiceSection Services { get; set; } = new ServiceSection();
        public ExecutionOptionsSection ExecutionOptions = new ExecutionOptionsSection();
        public CompilerOptions CompilerOptions { get; set; } = new CompilerOptions();
        public Systemd Systemd { get; set; } = new Systemd();
    }

    public class ServiceSection
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public ProjectType ProjectType { get; set; } = ProjectType.None;
        public List<string> Dependencies { get; set; } = [];
    }
    
    public class ExecutionOptions
    {
        public string Arguments { get; set; } = string.Empty;
    }

    public class CompilerOptions
    {
        public string ServiceProjectPath { get; set; } = string.Empty;
        public string ServiceOutputPath { get; set; } = string.Empty;
        public string ServiceBuildPath { get; set; } = string.Empty;
    }

    public class Systemd
    {
        public string ServiceFilePath { get; set; } = string.Empty;
    }

    public enum ProjectType
    {
        None = 0,   
        DotnetProject = 1,
        PythonProject = 2
    }
}
