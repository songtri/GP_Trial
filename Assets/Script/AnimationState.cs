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
	SlashAttack,
	BluntAttack,
	PierceAttack,
	HitByAttacker,
	HitByAttackerHeavy,
	AttackCancel,
	Die
}
