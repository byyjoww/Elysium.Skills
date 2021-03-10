using System;
using UnityEngine;
using UnityEngine.Events;

internal interface ICollisionDetector
{
    event UnityAction<IDamageable> OnEnemyCollision;
    event UnityAction<Collider> OnAnyCollision;
}