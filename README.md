# EsService2Csv2Zip2Aws
A service that runs once a day, takes files documents from elasticsearch, send them to a csv file, zip that file and uploade it to a s3 AWS bucket

This is a upgraded version of my previus Es2Csv program which you can see here:\
https://github.com/kaptajn93/Es2Csv

EsService2Csv2Zip2Aws is a windows service, that runs once a day and searches through all documents of a specifyed type, in an index, and takes out the propperties and map them to an csv-file. Furthermore it zip that csv file and uploade it to a AWS s3 bucket. 
This is a little handy program especially if you use it to handle logfiles from elasticsearch. In my case my index where given by the type and date from when they where made. Therefore in my case, i would run my service every day, taking yesterdays logfiles. This is done usinge a "fromCurrentDate" integer, that specefies what index to handle, by default its -1 but this can be set in the configfile, or tweeked in the code, to match your needs.

##### For this program to work, you must specify the fallowing in the Es2Csv.Config file:
- Uri to your elasticsearch node.
- Index to search in.
- Type to search for in index.
- AWS access key
- AWS secretKey
- Filepath to specify where you want the csv-file to be placed (name is given by the index).
- Mappings of the propperties you want to map to csv.

You are also able to change the default values of "From", "Size" and "SortBy", where the "SortBy" can be set to a field-name like "@timestamp" or other fields you want to sort your search by. Default is set to _doc, which is the fastest way to get get all documents, but not in a given order.
You can also set the formCurrentDate as mentioned above.

Remember, my index to search in was named {type}{date} where:
```
{
 var searchDate = (DateTime.Now.AddDays(fromCurrentDate)).ToString("yyyy.MM.dd");
 var searchIndex = $"{index}-{searchDate}";
}
```
where my searchIndex where the index to be searched in, in elasticsearch. This should be the only code your would have to change, if your indexes dont match that format.

you must specefy a config file in order for the program to work.

#### How the configFile should look like:
```
{
  "uri": "http://54.171.247.56:9200",
  "from": 0,
  "size": 10000,
  "index": "logstash",
  "fromCurrentDate": "-1",
  "type": "searchlogger",
  "sortBy": "@timestamp",
  "filePath": "c:\\users\\hsm\\documents\\visual studio 2015\\Projects\\Es2Csv\\Es2Csv\\Logs\\",
  "awsAccesskey": "AKIAJ7FOCCZY7MUBVJYA",
  "awsSecretkey": "Z9AO9LbV0oQ5P+fZ2Xe4INyC1aEK6QwCZtcWPqp7",
  "awsBucketName": "your-bucket-name",
  "awsDirectoryName": "",
  "mappings": {
    "@timestamp": "Timestamp",
	"GOO": "QueryApplication",
	"logdata.message.Query.SearchText": "QuerySearchText",
	"logdata.message.Query.Params.searchDirection": "QuerySearchDirection",
	"logdata.message.Query.Level": "QueryLevel",
	"logdata.message.Query.DimensionType": "DimensionType",
	"logdata.message.Query.BookIds": "QueryBookIds",
	"logdata.message.Result.Hits": "ResultHits",
	"logdata.message.Result.IsCached": "ResultIsCached",
	"logdata.message.Result.BookIds": "ResultBookIds",
	"logdata.message.Session.ClientIp": "ClientIp",
	"logdata.message.Session.ClientType": "ClientType",
	"logdata.message.Session.SessionId": "SessionId",
	"logdata.message.CallDuration": "TimeElapsed",
	"logdata.message.Session.CustomerId": "CustomerId",
	"logdata.message.Session.CompanyId": "CompanyId",
	"logdata.message.Session.LoginProvider": "LoginProvider",
	"logdata.message.HostName": "Domain",
	"IP-0A0001E3": "Webserver"
   } 
}
```
As you can see it is possible to go down nested objects as well as setting default values like i did in the Webserver and QueryApplication.
 - proppertes to the left are your propperties from elasticsearch.
 - propperties to the right are what you are mapping them to be in the csv-file.

In the configfile you should replace the awsBucketName with your own aws bucket name, as well with  the directory name, if you wanted placed under one.
### To run the program do the fallowing:

- open a PowerShell admin prompt.

now whrite the fallowing in the prompt:
```sh
New-Service -name TEST -displayName TEST -binaryPathName "C:\Your-filepath-to-projekt\EsService2Csv2Zip2Aws\Es2Csv.Service\bin\Debug\Es2Csv.Service.exe -c C:\filepath-to-your-confile.config -t Hours:Minuts"
```
The service name named TEST and display name TEST can be replaced by whatever you whant your service  to be called.

Hours and Minuts have to be specefied using numbers like : "10:30" it is representet by ints, so if yor want to run your service at "09:09" you have to whrite it like "9:9", where "10:30" still would be "10:30".

This program does not take care of AM and PM, so this should be reformatted if this is what you use. 

Now the csv and zip-file should be placed under the folder you specifyed under filePath. The zip-file should also apear under the specified AWS bucket.

hope this helped you out.

#### Cheers!
