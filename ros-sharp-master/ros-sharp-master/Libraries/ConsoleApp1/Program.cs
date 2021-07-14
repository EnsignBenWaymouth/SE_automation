using System;
using RosBridgeClient;
using std_msgs = RosSharp.RosBridgeClient.Messages.Standard;

public class ConsoleExample
{
    public static void Main(string[] args)
    {
        // WebSocket Connection:
        RosSocket rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketNetProtocol("ws://xxx.xxx.xxx.xxx:9090"));

        // Publication:
        string publication_id = rosSocket.Advertise<std_msgs.String>("publication_test");
        std_msgs.String message = new std_msgs.String("publication test message data");
        rosSocket.Publish(publication_id, message);

        // Subscription:
        string subscription_id = rosSocket.Subscribe<std_msgs.String>("/subscription_test", SubscriptionHandler);
        subscription_id = rosSocket.Subscribe<std_msgs.String>("/subscription_test", SubscriptionHandler);

        Console.WriteLine("Press any key to close...");
        Console.ReadKey(true);
        rosSocket.Close();
    }

    private static void SubscriptionHandler(std_msgs.String message)
    {
        Console.WriteLine((message).data);
    }
}