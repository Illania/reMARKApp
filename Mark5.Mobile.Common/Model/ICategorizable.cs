using System;
using System.Collections.Generic;

namespace Mark5.Mobile.Common.Model
{
    public interface ICategorizable
    {
        public List<Category> Categories { get; set; }
    }
}
