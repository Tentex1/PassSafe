namespace PassSafe.Models
{
    /// <summary>
    /// Defines the <see cref="CriticalAction" />
    /// </summary>
    public class CriticalAction
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public string IconKey { get; set; }

        public string Color { get; set; }

        public Password TargetPassword { get; set; }
    }
}
