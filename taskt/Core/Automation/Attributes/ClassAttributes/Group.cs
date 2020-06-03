﻿using System;

namespace taskt.Core.Automation.Attributes.ClassAttributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Group : Attribute
    {
        public string Name { get; private set; }
        public Group(string name)
        {
            Name = name;
        }
    }
}
