# HRConnect Backend - Notification Infrastruture Documentation

## Overview 

This document describes how the notifications system should be used in the system and how ti is architectured. The pipeline is as follows

 ```mermaid
 flowchart LR 
 A[Created Notification] --> B[Notifcation Factory]
 subgraph  
    direction TD
    D
    E
    F
 end
 B[Factory]-->C[Service]
 C[Service]-->D[Dispatcher]
 D[Dispatcher]-->E[InApp]
 D[Dispatcher]-->F[Email]
 ```

## Usage
Following creating a notification, with 1 or more delivery channels, the factory needs to be called (via Depenedency Injection)
to be able to introduce the notification into the pipeline 

```C#
 // NotificationFactory.cs (see implemtation on line:32)
 INotification.ProduceNotificationAsync(CreateNotificationDto);
```
### Service and Dispatchers
The service class is responsible for making notificationsa and saving them to the database using 
```INotificationService.TryCreateAndDispatch(Notification)```.
Idempotency Keys are used to create uniqueness and prevent duplicate notifications for an employee.
The keys is a hash calculated from parts of the notification like so,
```C#
// NotificationService.cs (see line:103)
$"{request.Type}:{request.EmployeeId}:{request.DeliveryChannel}:{request.Message.Trim()}"
```
where Delivery Channels are flagged enums like InApp, Email and potentially more future delivery 
channels.
Since the Delivery Channels are flagged, they can be piped together to instruct the dispatcher to
send to multiple Delivery Channels
```C#
// notification will be sent through to within the app AND email
notification.DeliveryChannel = Delivery.InApp | Delivery.Email;
```

Once a notification has been successfully created or returned by the service, the 
```INotification.DispatchNotificationAsync(Notification)``` immediately sends out notifications to
 the correct Delivery Channel









