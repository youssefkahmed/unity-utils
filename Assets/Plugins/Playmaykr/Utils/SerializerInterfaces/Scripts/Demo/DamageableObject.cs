using UnityEngine;

namespace Utils.SerializedInterfaces.Demo
{
    public class DamageableObject : MonoBehaviour, IDamageable
    {
        public void Damage(float value)
        {
            Debug.Log($"[{nameof(DamageableObject)}] Damage = {value}");
        }
    }

    public interface IDamageable
    {
        void Damage(float value);
    }
}
