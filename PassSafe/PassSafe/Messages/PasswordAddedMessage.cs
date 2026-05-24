namespace PassSafe.Messages
{
    using CommunityToolkit.Mvvm.Messaging.Messages;

    public record PasswordTransferData(string Title, string UserName, string Password, string Icon, string SecurityStatus, double SecurityProgress);
    /// <summary>
    /// Defines the <see cref="PasswordAddedMessage" />
    /// </summary>
    public class PasswordAddedMessage : ValueChangedMessage<PasswordTransferData>
    {
        public PasswordAddedMessage(PasswordTransferData value) : base(value)
        {
        }
    }
}
