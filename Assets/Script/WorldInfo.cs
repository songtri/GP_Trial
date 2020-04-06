using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldInfo
{
	private WorldInfo()
	{
	}

	public static WorldInfo instance => Nested.instance;

	public class Nested
	{
		static Nested()
		{
		}

		internal static readonly WorldInfo instance = new WorldInfo();
	}

	public Character MainPlayer;
	public List<Character> EnemyList;
}
