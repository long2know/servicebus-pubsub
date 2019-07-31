using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using Newtonsoft.Json;

namespace ServiceBusSpikeSub
{
    public class Application
    {
        private string _connString;
        private string _topicName;
        private string _subscriptionName;

        static ISubscriptionClient _client;

        private AppSettings _appSettings;
        private ILogger _log;

        public Application(IOptions<AppSettings> appSettings, ILogger<Application> log)
        {
            _appSettings = appSettings.Value;
            _log = log;

            _connString = _appSettings.SubEndpoint;
            _topicName = _appSettings.TopicName;
            _subscriptionName = _appSettings.SubName;

            _client = new SubscriptionClient(_connString, _topicName, _subscriptionName);
        }

        public async Task Run()
        {
            _log.LogDebug("Starting application");

            _log.LogDebug("======================================================");
            _log.LogDebug("Press ENTER key to exit after receiving all the messages.");
            _log.LogDebug("======================================================");

            // Register subscription message handler and receive messages in a loop.
            RegisterOnMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await _client.CloseAsync();
        }

        public void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that processes messages.
            _client.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        public async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            _log.LogDebug($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            var json = Encoding.UTF8.GetString(message.Body);
            var requestPayload = JsonConvert.DeserializeObject<RequestPayload>(json);

            // Serialize back to confirm deserialization worked
            _log.LogDebug($"Deserialized JSON and serialized: { JsonConvert.SerializeObject(requestPayload, Formatting.Indented) }");
            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            await _client.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
            // If subscriptionClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        public Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            _log.LogDebug($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            _log.LogDebug("Exception context for troubleshooting:");
            _log.LogDebug($"- Endpoint: {context.Endpoint}");
            _log.LogDebug($"- Entity Path: {context.EntityPath}");
            _log.LogDebug($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
