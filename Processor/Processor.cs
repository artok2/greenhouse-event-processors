using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.EventHubs;
using System.Linq;
using System.Text;
using GreenHouse.Event.Processor.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents.Client;

namespace GreenHouse.Event.Processor
{

    public static class  Processor
    {        

        [FunctionName("EventHubTrigger")]
        public static async Task EventHubTrigger(
            [EventHubTrigger(
                Constants.EVENT_HUB_TELEMETRY_GREENHOUSE,            
                Connection = "IotEventHub:ConnectionString"
                )] EventData[] events,
            [CosmosDB(
                databaseName: Constants.COSMOS_DB_DATABASE_NAME,
                collectionName: Constants.COSMOS_DB_CONTAINER_NAME,
                CreateIfNotExists=true,
                ConnectionStringSetting =  "CosmosDb:ConnectionString"
                )] IAsyncCollector<object> items,
            ILogger log)
        {

            var exceptions = new List<Exception>();

            TelemetryGreenHouse deserialized = null;

            foreach (EventData eventData in events)
            {
                try
                {

                    var jsonBody = Encoding.UTF8.GetString(eventData.Body);
                    
                    log.LogInformation($"V2 C# IoT Hub trigger function processed a message: {jsonBody}");
                    if (jsonBody.Contains("[")) return;

                    var data = JsonConvert.DeserializeObject<TelemetryGreenHouse>(jsonBody);
                    var telemetryEntity = new TelmetryEntity
                    {
                        PartitionKey = "temperature",// data.PartitionKey,                      
                        DeviceId = data.DeviceId,
                        Altitude = data.Altitude,
                        Pressure = data.Pressure,
                        Humidity = data.Humidity,
                        CPUTemperature = data.CPUTemperature,
                        Temperature = data.Temperature
                    };

                    // Id = eventData.SystemProperties["iothub-connection-device-id"].ToString(),
                    await items.AddAsync(telemetryEntity);

                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }


          [FunctionName("GetTemperature")]
        public static IActionResult GetTemperature(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "temperature/{devicename}")] HttpRequest req,
            [CosmosDB(databaseName: Constants.COSMOS_DB_DATABASE_NAME,
                collectionName: Constants.COSMOS_DB_CONTAINER_NAME,
                CreateIfNotExists=true,
              ConnectionStringSetting = "CosmosDb:ConnectionString",
          SqlQuery = "SELECT top 1 * FROM greenhouse where greenhouse.deviceId ={devicename} order by greenhouse._ts desc")
              ]
           
            IEnumerable<TelmetryEntity> entities,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
    

            if (entities is null )
            {
                return new NotFoundResult();
            }
            var enitity =  entities.FirstOrDefault();

            log.LogInformation(enitity?.Temperature.ToString());

            return new OkObjectResult(enitity);
        }

        [FunctionName("GetTemperatures")]
        public static IActionResult GetTemperatures(
         [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "temperatures")] HttpRequest req,
         [CosmosDB(databaseName: Constants.COSMOS_DB_DATABASE_NAME,
                collectionName: Constants.COSMOS_DB_CONTAINER_NAME,
                CreateIfNotExists=true,
              ConnectionStringSetting = "CosmosDb:ConnectionString",
          SqlQuery = "SELECT TOP 10 * FROM greenhouse where greenhouse.partitionKey ='temperature' order by greenhouse._ts desc")
              ]

            IEnumerable<TelmetryEntity> entities,
         ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            if (entities is null)
            {
                return new NotFoundResult();
            }
       

            return new OkObjectResult(entities);
        }
    }

}