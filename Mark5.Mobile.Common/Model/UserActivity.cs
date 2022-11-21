using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mark5.Mobile.Common.Model
{
    public class UserActivity
    {
        public UserActivityType Type { get; set; }

        public string DescriptionEvent { get; set; }

        public string DescriptionAction { get; set; }

        public bool ConfirmationRequired { get; set; }

        public bool PerformOnOriginalDocument { get; set; }

        public bool AssignOriginalCategories { get; set; }

        public bool AssignOriginalExtraFields { get; set; }

        public List<Category> Categories { get; set; } = new List<Category>();

        public Dictionary<int, string> ExtraFields { get; set; } = new Dictionary<int, string>();

    }
}

