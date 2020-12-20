﻿using System;
using System.Threading;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.SystemConsole.Themes;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            // Enable the selflog output
            SelfLog.Enable(Console.Error);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: SystemConsoleTheme.Literate)
                .WriteTo.Elasticsearch(
                    new ElasticsearchSinkOptions(
                            new Uri("http://84.38.183.219:9200")) // for the docker-compose implementation
                        {
                            AutoRegisterTemplate = true,
                            OverwriteTemplate = true,
                            DetectElasticsearchVersion = true,
                            AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                            NumberOfReplicas = 1,
                            NumberOfShards = 2,
                            //BufferBaseFilename = "./buffer",
                            // RegisterTemplateFailure = RegisterTemplateRecovery.FailSink,
                            FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                            EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                               EmitEventFailureHandling.WriteToFailureSink |
                                               EmitEventFailureHandling.RaiseCallback,
                        })
                .CreateLogger();

            Log.Information("Hello, world!");

            int a = 10, b = 0;
            try
            {
                Log.Debug("Dividing {A} by {B}", a, b);
                Console.WriteLine(a / b);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Something went wrong");
            }

            // Introduce a failure by storing a field as a different type
            Log.Debug("Reusing {A} by {B}", "string", true);

            Log.CloseAndFlush();
            Console.WriteLine("Press any key to continue...");
            while (!Console.KeyAvailable)
            {
                Thread.Sleep(500);
            }
        }
    }
}