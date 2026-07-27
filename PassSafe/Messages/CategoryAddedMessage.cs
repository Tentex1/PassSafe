namespace PassSafe.Messages
{
    using CommunityToolkit.Mvvm.Messaging.Messages;

    /// <summary>
    /// A messaging contract used to notify the SafeViewModel when a user creates a new custom category.
    /// Carries the name of the newly created category as a string payload.
    /// </summary>
    public class CategoryAddedMessage : ValueChangedMessage<string>
    {
        public CategoryAddedMessage(string value) : base(value)
        {
        }
    }
}