using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissingComponentException : Exception
{
	public MissingComponentException()
	{
	}

	public MissingComponentException(string message) : base(message)
	{
	}
}
