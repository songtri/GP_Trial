using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationState
{
	Walking,
	Running,
	CombatMode
}

public enum AnimationStateTrigger
{
	BasicAttack,
	HitByAttacker,
	AttackCancel,
}
