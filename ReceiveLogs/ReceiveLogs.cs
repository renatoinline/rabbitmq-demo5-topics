using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace ReceiveLogs
{
    class ReceiveLogs
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Console.WriteLine("Write the log types you want to receive:");
            Console.WriteLine("The options are: system.info, application.warning and system.error");
            Console.WriteLine("Try combination with * and # to bind to desired topic");
            Console.WriteLine("Ex.: '*.info' , 'system.*'");



            var input = Console.ReadLine();

            var severities = input.Split(' ');

            var factory = new ConnectionFactory()
            {
                HostName = "192.168.15.33",
                UserName = "renatocolaco",
                Password = "secnet123"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // This tells RabbitMQ not to give more than one message to a worker at a time
                // prefetchCount: 1
                channel.ExchangeDeclare(exchange: "topic_logs", type: "topic");

                var queueName = channel.QueueDeclare().QueueName;

                foreach (var sev in severities)
                {
                    channel.QueueBind(queue: queueName, exchange: "topic_logs", routingKey: sev);
                }
                
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) => {

                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine(" [x] message received {0}", message);
                    Thread.Sleep(4000);
                    Console.WriteLine(" [#] Done");
                };

                channel.BasicConsume(
                    queue: queueName,
                    autoAck: true,
                    consumer: consumer
                    );

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
