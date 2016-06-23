using System;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using System.Timers;


namespace Es2Csv.Service
{
    public partial class ScheduledService : ServiceBase
    {
        static System.Timers.Timer timer;
        //   static string nextIndex;
        private static string _scheduledRunningTime;
        private static string[] _arguments;

        public ScheduledService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Thread.Sleep(10000);
            if (args.Length == 0)
                args = Environment.GetCommandLineArgs();

            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                if (string.IsNullOrEmpty(options.WhenToRunService))
                {
                    if (!File.Exists(options.WhenToRunService))
                    {
                        Console.WriteLine($"{options.WhenToRunService} is not a valid timeperiod!");
                        return;
                    }
                }
                try
                {
                    string timeToRun = options.WhenToRunService;
                    _scheduledRunningTime = timeToRun;
                    timer = new System.Timers.Timer { Interval = 60000 };
                    _arguments = args;
                    timer.Elapsed += (timer_Elapsed);
                    timer.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}!");
                    return;
                }
            }
            else
            {
                Console.WriteLine("When to run is not set as a parameter");
            }
        }

        static void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Displays and Logs Message
            string _CurrentTime = String.Format("{0}:{1}", DateTime.Now.Hour, DateTime.Now.Minute);
            //nextIndex = String.Format("{0}", DateTime.UtcNow.Date.AddDays(-6));
            if (_CurrentTime == _scheduledRunningTime)
            {
                Es2Csv.Program.Main(_arguments);
            }
        }

        protected override void OnStop()
        {
        }
    }
}
