//
// File: TesterFactory.cs
// Author: Bartosz Cichecki <bgc@nordic-it.com>
//
// Copyright (c) 2016 Nordic IT
//

namespace Mark5.Mobile.Common.Tester
{
    public static class TesterFactory
    {
        public static ITester Create()
        {
            return new Tester();
        }
    }
}