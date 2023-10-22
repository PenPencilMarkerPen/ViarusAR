﻿using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ComponentRestrictionAttribute : PropertyAttribute
{
	public readonly Type type;
	public ComponentRestrictionAttribute(Type type)
	{
		this.type = type;
	}

}

