using System;

namespace reMark.Mobile.Common.Model
{
    public interface IBusinessEntity
    {
        int Id { get; set; }
        Guid Guid { get; set; }
        ObjectType ObjectType { get; }
        ModuleType ModuleType { get; }
    }
}