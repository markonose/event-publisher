# EventPublisher
Written as a C# CLI application because due to problems with xml deserialization  
regarding trimming it sadly produces a 70 mb executable when publishing due to  
it being self contained, so once built 

## Build
Building it requires .NET 8.0

Supports: win-64x and linux-64x  
linux: dotnet publish CLI.csproj -c Debug -r linux-x64  
windows: dotnet publish CLI.csproj -c Debug -r win-x64

## Test
Application.IntegrationTests only work if there is an instance of rabbitmq  
running on localhost using guest guest credentials on / virtual host using  
the amq.headers exchange

## Dependencies
The app can only publish to RabbitMQ, tested using rabbitmq:3-management docker image

## Usage:
event-publisher [options]

Options:  
--file &lt;file&gt; (REQUIRED)            Path to file  
--hostnames &lt;hostnames&gt; (REQUIRED)  RabbitMQ hostnames  
--username &lt;username&gt;               RabbitMQ username [default: guest]  
--password &lt;password&gt;               RabbitMQ password [default: guest]  
--vhost &lt;vhost&gt;                     RabbitMQ virtual host [default: /]  
--exchange-name &lt;exchange-name&gt;     RabbitMQ headers exchange name [default: amq.headers]  
--version                                 Show version information  
-?, -h, --help                            Show help and usage information
