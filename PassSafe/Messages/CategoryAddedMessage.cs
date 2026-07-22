using CommunityToolkit.Mvvm.Messaging.Messages;

namespace PassSafe.Messages
{
    public class CategoryAddedMessage : ValueChangedMessage<string>
    {
        public CategoryAddedMessage(string value) : base(value)
        {
        }
    }
}