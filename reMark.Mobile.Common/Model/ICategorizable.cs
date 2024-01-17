using System;
using System.Collections.Generic;

namespace reMark.Mobile.Common.Model
{
    public interface ICategorizable
    {
        public List<Category> Categories { get; set; }
    }
}
