using System;

namespace CommUnityApp.Domain.Entities
{
    public class SpinGame
    {
        public int GameId { get; set; }
        public int BusinessId { get; set; }
        public string GameName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int ConfigId { get; set; }
        public int CreatedByAdminId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

