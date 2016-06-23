using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Es2Csv.Exsport.S3;
using Nest;
using Newtonsoft.Json;
using Topshelf;

namespace Es2Csv
{
    public class Program
    {
        static ElasticManager _manager = new ElasticManager();
        static MappingConfiguration _config = new MappingConfiguration();

        public static void Main(string[] args)
        {
            if (args.Length == 0)
                args = Environment.GetCommandLineArgs();

            Start(args);
        }

        public static void Start(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                #region -- Validate user command arguments: -- 
                // Values are available here
                if (string.IsNullOrEmpty(options.ConfigurationFile))
                {
                    if (!File.Exists(options.ConfigurationFile))
                    {
                        Console.WriteLine($"{options.ConfigurationFile} is not a valid filepath!");
                        return;
                    }
                }
               

                #endregion

                #region -- Validate config file: --

                MappingConfiguration config;
                try
                {
                    config = GetFile(options.ConfigurationFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}!");
                    return;
                }

                if (ValidateConfigFile())
                {
                    // var index = "logstash.yyyy.MM.dd";
                    // var index = config.Index ?? config.indexPattern;
                    var index = config.Index;
                    var size = config.Size;
                    var from = config.From;
                    var type = config.Type;
                    var mappings = config.Mappings;
                    var sortBy = config.SortBy;
                    var filePath = config.FilePath;
                    var fromCurrentDate = config.FromCurrentDate;
                    var awsAccesskey = config.AwsAccesskey;
                    var awsSecretkey = config.AwsSecretkey;

                    #endregion

                    _manager.NodeUri = config.Uri;

                    var searchDate = (DateTime.Now.AddDays(fromCurrentDate)).ToString("yyyy.MM.dd");
                    var searchIndex = $"{index}-{searchDate}";

                    try
                    {
                        Measure(() =>
                        {
                            // 1) search elasticsearch 
                            var hits = _manager.EntrySearch(from, size, searchIndex, type, mappings, sortBy);
                            string csvFilename = "";
                            string csvString = "";

                            // 2) generate csv:
                            if (hits.Any())
                            {
                                var mapper = new Mapper();
                                csvString = mapper.MapToCsv(hits, mappings);
                                csvFilename = $"{filePath}{type}.{searchIndex}.csv";
                            }
                            else
                            {
                                var msg = "could not find any searchResponse, please check your config-file or Elasticsearch data";
                                Console.WriteLine(msg);
                            }

                            // 3) Zip file
                            var zipFileName = $"{filePath}{type}.{searchIndex}.zip";
                            File.AppendAllText(csvFilename, csvString, Encoding.UTF8);
                            var zippedFileEntry = Zip(csvFilename, zipFileName);


                            // 4) Upload to S3
                            // preparing our file and directory names
                            //string fileToBackup = @"d:\mybackupFile.zip"; // test file
                            string myBucketName = "es2csv-backup"; //your s3 bucket name goes here
                            string s3DirectoryName = "";
                            string s3FileName = $"{zipFileName}";
                            AmazonUploader myUploader = new AmazonUploader(awsAccesskey, awsSecretkey);
                            //"AKIAJ7FOCCZY7MUBVJYA", "Z9AO9LbV0oQ5P+fZ2Xe4INyC1aEK6QwCZtcWPqp7"
                            try
                            {
                                myUploader.UploadToS3(zipFileName, myBucketName, s3DirectoryName, s3FileName);
                            }
                            catch (Exception ex)
                            {
                                var msg = "could not opload to aws, please check config-file";
                                NullReferenceException nullReference = new NullReferenceException(msg, ex);
                                throw nullReference;
                            }

                            csvString = null;

                        });
                    }
                    catch (Exception ex)
                    {
                        var msg = "could not find any searchResponse, please check config-file";
                        NullReferenceException nullReference = new NullReferenceException("msg", ex);
                        throw nullReference;
                    }
                }
            }
            else
            {
                Console.WriteLine(
                    "Need filepath to your config-file. Please enter: \"Es2Csv.exe -c \"your-config-file-path\"\"");
            }
        }

        private static ZipArchiveEntry Zip(string filePath, string zipFilePath)
        {
            ZipArchiveEntry result = null;
            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                result = zipArchive.CreateEntryFromFile(filePath, Path.GetFileName(filePath), CompressionLevel.Optimal);
            }
            return result;
        }

        private static void Measure(Action action)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            action.Invoke();

            TimeSpan ts = stopWatch.Elapsed;
            // Format and display the TimeSpan value. 
            string elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
            Console.WriteLine("Elapsed: " + elapsedTime);
        }

        private static void WriteMissingArg(string argument)
        {
            Console.WriteLine($"{argument} is not defined");
        }

        public static MappingConfiguration GetFile(string filepath)
        {
            try
            {
                var contents = File.ReadAllText(filepath);
                _config = JsonConvert.DeserializeObject<MappingConfiguration>(contents);
            }
            catch (Exception ex)
            {
                FileNotFoundException fileNotFound = new FileNotFoundException("invalid filepat", ex);
                throw fileNotFound;
            }
            return _config;
        }

        private static bool ValidateConfigFile()
        {
            if (_config.Index == null)
            {
                WriteMissingArg("index");
                return false;
            }
            if (_config.Uri == null)
            {
                WriteMissingArg("uri");
                return false;
            }
            if (_config.Mappings == null)
            {
                WriteMissingArg("mappings");
                return false;
            }
            if (_config.Type == null)
            {
                WriteMissingArg("type");
                return false;
            }
            if (_config.Size <= 0)
            {
                WriteMissingArg("size");
                return false;
            }
            if (_config.SortBy == null)
            {
                WriteMissingArg("sortBy");
                return false;
            }
            if (_config.FilePath == null)
            {
                WriteMissingArg("filePath");
                return false;
            }
            if (_config.AwsAccesskey == null)
            {
                WriteMissingArg("awsAccesskey");
                return false;
            }
            if (_config.AwsSecretkey == null)
            {
                WriteMissingArg("awsSecretkey");
                return false;
            }
          return true;
        }

        public void Stop()
        {
            Console.WriteLine("Es2Csv is Stopped");
        }
    }
}


