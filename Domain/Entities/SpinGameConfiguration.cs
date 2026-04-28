using System;

namespace CommUnityApp.Domain.Entities
{
    public class SpinGameConfiguration
    {
        public int ConfigId { get; set; }
        public int MaxSpinsPerDay { get; set; }
        public int NumberOfSections { get; set; }
        public DateTime GameStartDate { get; set; }
        public DateTime GameEndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

