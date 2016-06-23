using System.ServiceProcess;

namespace Es2Csv.Service
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ScheduledService(), 
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
