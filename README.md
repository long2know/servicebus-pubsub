# servicebus-pubsub
Simple .NET Core application to test Azure Servicebus topic pub-sub

This set of console applications use .NET Core secrets.  You will have to create your secrets to use it.

To get started, in the Azure Portal:
Ccreate a service bus namespace.
Add a topic to the service bus.
Add access policies (I created a separates ones for pub and sub with sub having only listen priviledges)
Add a subscription to the topic
Copy the pub/sub/topicname/subscription name and add them to dotnet core secrets from within one of the application folders.

dotnet user-secrets set "AppSettings:PubEndpoint" "Endpoint=sb..."
dotnet user-secrets set "AppSettings:SubEndpoint" "Endpoint=sb..."
dotnet user-secrets set "AppSettings:SubName" "your sub name.."
dotnet user-secrets set "AppSettings:TopicName" "your topic name.."

More info about using secrets with a .NET Core console applicaiton can be seen on my blog:

https://long2know.com/2018/02/net-core-console-app-dependency-injection-and-user-secrets/
