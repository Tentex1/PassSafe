using PassSafe.Models;

public class CriticalAction
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string IconKey { get; set; }
    public string Color { get; set; }
    public Password TargetPassword { get; set; } // Değiştir dediğinde hangi parolaya işlem yapacağını bilmek için tutuyoruz
}