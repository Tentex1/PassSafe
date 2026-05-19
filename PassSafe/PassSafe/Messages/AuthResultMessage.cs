using CommunityToolkit.Mvvm.Messaging.Messages;
using Plugin.Maui.Biometric;

namespace PassSafe.Messages;

// Hem alıcının hem vericinin bildiği ortak mesaj zarfı bu!
public class AuthResultMessage : ValueChangedMessage<AuthenticationResponse>
{
    public AuthResultMessage(AuthenticationResponse response) : base(response)
    {
    }
}