namespace ServiceBusManager.Models.Enums;

public enum ServiceBusOperation
{
    Peek,
    Receive,
    Purge,
    Delete,
    Complete,
    Abandon,
    DeadLetter,
    Send,
    Resubmit,
    Schedule
} 