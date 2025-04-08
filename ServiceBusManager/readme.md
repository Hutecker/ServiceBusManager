# ServiceBusManager

ServiceBusManager is a cross-platform desktop application built with .NET MAUI that allows you to manage Azure Service Bus queues and topics. The app supports both Windows and macOS, providing a seamless experience for developers and administrators working with Azure Service Bus.

## Features

- **Queue Management**: 
  - View, create, update, and delete queues.
  - Peek and receive messages from queues.
  - Send test messages to queues.

- **Topic Management**:
  - View, create, update, and delete topics and subscriptions.
  - Peek and receive messages from subscriptions.
  - Send test messages to topics.

- **Cross-Platform Support**:
  - Fully functional on both Windows and macOS.

- **User-Friendly Interface**:
  - Intuitive and modern UI built with .NET MAUI.
  - Dark and light theme support.

## Prerequisites

Before running the application locally, ensure you have the following installed:

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- An active [Azure Subscription](https://azure.microsoft.com/free/) with an Azure Service Bus resource.
- (Optional) Visual Studio 2022 with the MAUI workload installed for development.

## Installation

1. Clone the repository:
   
```
   git clone https://github.com/Hutecker/ServiceBusManager.git
   cd ServiceBusManager
   
```

2. Restore dependencies:
   
```
   dotnet restore
   
```

3. Build the project:
   
```
   dotnet build
   
```

4. Run the application:
   
```
   dotnet run
   
```

## Configuration

1. Navigate to the `appsettings.json` file in the project directory.
2. Add your Azure Service Bus connection string:
   
```
   {
     "AzureServiceBus": {
       "ConnectionString": "YourAzureServiceBusConnectionString"
     }
   }
   
```

## Usage

1. Launch the application.
2. Enter your Azure Service Bus connection string in the settings.
3. Use the navigation menu to switch between queues and topics.
4. Perform operations such as creating, updating, or deleting queues/topics, and sending/receiving messages.

## Technologies Used

- **.NET MAUI**: For building the cross-platform UI.
- **Azure.Messaging.ServiceBus**: One of Azure's messaging platform.
- **MVVM Architecture**: For clean and maintainable code.

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a new branch:
   
```
   git checkout -b feature/YourFeatureName
   
```
3. Commit your changes:
   
```
   git commit -m "Add your message here"
   
```
4. Push to the branch:
   
```
   git push origin feature/YourFeatureName
   
```
5. Open a pull request.

## Contact

For questions or feedback, please reach out to:

- **GitHub**: [Hutecker](https://github.com/Hutecker)