//
// Project: Mark5.Mobile.Common
// File: ICopiable.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

#pragma warning disable CS1701
namespace Mark5.Mobile.Common.Model
{
    public interface ICopiable<T>
    {

        T ShallowCopy();

        T DeepCopy();
    }
}

