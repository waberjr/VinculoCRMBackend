namespace VinculoBackend.Domain.Enums;

public enum DonorPersonType
{
    Individual,
    Company,
}

public enum DonorStatus
{
    Lead,
    Active,
    Inactive,
    AtRisk,
    DoNotContact,
}

public enum DonationType
{
    OneTime,
    Recurring,
    Pledge,
}

public enum DonationStatus
{
    Pending,
    Confirmed,
    Overdue,
    Cancelled,
    Refunded,
}

public enum PaymentMethod
{
    Pix,
    CreditCard,
    Boleto,
    BankTransfer,
    Cash,
    Other,
}

public enum DonationPlanStatus
{
    Active,
    Paused,
    Cancelled,
}

public enum CampaignType
{
    Fundraising,
    Retention,
    Reactivation,
    Emergency,
    Other,
}

public enum CampaignStatus
{
    Draft,
    Active,
    Completed,
    Cancelled,
}

public enum ProjectStatus
{
    Draft,
    Active,
    Completed,
    Archived,
}

public enum ReceiptStatus
{
    Draft,
    Issued,
    Cancelled,
    Reissued,
}

public enum CampaignChannel
{
    Phone,
    WhatsApp,
    Email,
    SocialMedia,
    InPerson,
    Other,
}

public enum TaskType
{
    Call,
    WhatsApp,
    Email,
    FollowUp,
    PaymentReminder,
    ThankYou,
    DataUpdate,
    Other,
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Urgent,
}

public enum RelationshipTaskStatus
{
    Open,
    InProgress,
    Completed,
    Cancelled,
}

public enum ContactOutcome
{
    Reached,
    NoAnswer,
    InvalidContact,
    RequestedCallback,
    DonationConfirmed,
    NotInterested,
    DoNotContact,
    Other,
}

public enum TimelineEntryType
{
    Note,
    Donation,
    Task,
    Contact,
}

public enum PhoneType
{
    Mobile,
    WhatsApp,
    Home,
    Work,
}

public enum EmailType
{
    Personal,
    Work,
    Billing,
}
