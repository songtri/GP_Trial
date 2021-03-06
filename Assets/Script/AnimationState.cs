﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationState
{
	Walking,
	Running,
	Sprint,
	Rage,
}

public enum AnimationStateTrigger
{
	SlashAttack,
	BluntAttack,
	PierceAttack,
	HitByAttacker,
	HitByAttackerHeavy,
	AttackCancel,
	FinalBlowA,
	FinalBlowB,
	Die,
	Init
}
