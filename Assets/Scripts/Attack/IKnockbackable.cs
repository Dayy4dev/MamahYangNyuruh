// IKnockbackable.cs
// Implementasikan interface ini di script enemy/karakter yang bisa kena knockback

public interface IKnockbackable
{
    void TakeKnockback(UnityEngine.Vector3 direction, float force, float duration);
}