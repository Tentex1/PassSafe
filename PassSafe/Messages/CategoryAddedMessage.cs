namespace PassSafe.Messages
{
    using CommunityToolkit.Mvvm.Messaging.Messages;

    /// <summary>
    /// Defines the <see cref="CategoryAddedMessage" />
    /// </summary>
    public class CategoryAddedMessage : ValueChangedMessage<string>
    {
        public CategoryAddedMessage(string value) : base(value)
        {
        }
    }
}
