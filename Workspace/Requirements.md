# Azure Service Bus Manager - Requirements Document

## Overview
The Azure Service Bus Manager is a desktop application designed to simplify the management of Azure Service Bus resources, including queues, topics, subscriptions, and messages. The application aims to provide a user-friendly interface with advanced features for monitoring, managing, and troubleshooting Service Bus entities.

## Goals
- Provide a desktop-based cross-platform Service Bus Manager that isn't crap, crashes, or costs money.
- Enable efficient management of Service Bus queues, topics, and subscriptions.
- Offer advanced features for message inspection, sending, and troubleshooting.
- Ensure a seamless user experience with a focus on performance and usability.

## Features

### High Priority (MVP)
1. **Queue Management**
   - View a list of all queues.
   - Create, update, and delete queues.
   - View queue properties (e.g., message count, size, status).
   - Purge messages from a queue and deadletter

2. **Topic and Subscription Management**
   - View a list of all topics and their subscriptions.
   - Create, update, and delete topics and subscriptions.
   - View topic and subscription properties.
   - Purge topic/subscription

3. **Message Management**
   - Peek/Recieve at messages in queues or subscriptions.
   - Send messages to queues or topics.
   - Resubmit or dead-letter messages.
   - Delete specific messages.

4. **Connection Management**
   - Support for multiple Service Bus namespaces.
   - Securely store and manage connection strings.

5. **Monitoring and Troubleshooting**
   - Real-time monitoring of queue and topic metrics.
   - View dead-letter messages and their details.
   - Console to view logs in app real time

### Medium Priority
1. **Advanced Filtering**
   - Filter messages by properties or content.
   - Search for specific messages.

2. **Batch Operations**
   - Send or delete messages in bulk.
   - Resubmit multiple dead-letter messages.

3. **User Roles and Permissions**
   - Define roles for read-only or admin access.
   - Restrict access to specific namespaces or entities.

4. **Customizable Dashboards**
   - Create dashboards to monitor specific queues or topics.
   - Add widgets for metrics like message count, throughput, etc.

5. **Connection Management**
   - Az login to pull in all Service Bus namespaces using user rbac/managed identity

6. **Monitoring and TroubleShooting**
   - Export logs for debugging. 

### Low Priority
1. **Integration with Azure Monitor**
   - View Azure Monitor alerts related to Service Bus.
   - Link to Azure Monitor logs for deeper insights.

2. **Theming and Customization**
   - Support for light and dark themes.
   - Customizable layouts and views.

3. **Offline Mode**
   - Allow users to view cached data when offline.
   - Sync changes when reconnected.

## Non-Functional Requirements
- **Performance:** The application should handle large namespaces with large message counts with minimal latency.
- **Security:** Ensure secure storage of connection strings and sensitive data.
- **Cross-Platform:** Support Windows and macOS.
- **Scalability:** Handle namespaces with thousands of entities.

## Prioritization Strategy
- **High Priority:** Features essential for the MVP and core functionality.
- **Medium Priority:** Features that enhance usability and efficiency.
- **Low Priority:** Features that provide additional value but are not critical.

## Future Enhancements
- Support for hybrid cloud environments.
- Integration with other Azure services (e.g., Event Grid, Logic Apps).
- Plugin system for custom extensions.