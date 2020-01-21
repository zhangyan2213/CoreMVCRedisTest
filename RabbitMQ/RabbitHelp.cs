using RabbitMQ.Client;
using System;
using System.Text;
namespace RabbitMQ
{
    public class RabbitHelp
    {
        public void Test(string message)
        {
            try
            {
                var qName = "HelloWorldQueue";
                var exchangeName = "fanoutchange1";
                var exchangeType = "topic";//topic、fanout
                var routingKey = "hello";
                var uri = new Uri("amqp://127.0.0.1:5672/");

                IConnectionFactory factory = new ConnectionFactory()
                {
                    UserName = "admin",
                    Password = "admin",
                    RequestedHeartbeat = 0,
                    Endpoint = new AmqpTcpEndpoint(uri)
                };
                //创建连接
                using (var connection = factory.CreateConnection())
                { 
                    //创建通道
                    var channel = connection.CreateModel();
                    channel.ExchangeDeclare(exchangeName,exchangeType);
                    //声明一个队列
                    channel.QueueDeclare(qName, false, false, false, null);
                    channel.QueueBind(qName, exchangeName, routingKey);
                    
                    var helloworld = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchangeName, routingKey, null, helloworld);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
         
        }
    }
}
