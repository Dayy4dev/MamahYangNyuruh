using UnityEngine;

public interface IKnockbackable
{
    void TakeKnockback(Vector3 direction, float force, float duration);
}