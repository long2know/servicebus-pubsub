using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using Newtonsoft.Json;
using AutoFixture;

namespace ServiceBusSpikePub
{
    public class Application
    {
        private string _connString;
        private string _topicName;
        private ITopicClient _topicClient;
        private int _numberOfRequests = 10;

        private AppSettings _appSettings;
        private ILogger _log;

        public Application(IOptions<AppSettings> appSettings, ILogger<Application> log)
        {
            _appSettings = appSettings.Value;
            _log = log;

            _connString = _appSettings.PubEndpoint;
            _topicName = _appSettings.TopicName;

            _topicClient = new TopicClient(_connString, _topicName);
        }

        public async Task Run()
        {
            _log.LogDebug("Starting application");

            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after sending all the messages.");
            Console.WriteLine("======================================================");

            // Send messages.
            await SendMessagesAsync(_numberOfRequests);

            Console.ReadKey();

            await _topicClient.CloseAsync();
        }

        public async Task SendMessagesAsync(int numberOfRequestsToSend)
        {
            try
            {
                for (var i = 0; i < numberOfRequestsToSend; i++)
                {
                    // Create a mocked request
                    var requestPayload = new Fixture().Create<RequestPayload>();

                    // Now convert to json
                    var messageBody = JsonConvert.SerializeObject(requestPayload);
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the topic.
                    await _topicClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }
    }
}
