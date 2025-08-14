using UnityEngine;

namespace Utils.SerializedInterfaces.Demo
{
    [ExecuteInEditMode]
    public class DemoScript : MonoBehaviour
    {
        [SerializeField] private InterfaceReference<IDamageable> damageable;
        
        [RequireInterface(typeof(IDamageable))]
        [SerializeField] private MonoBehaviour attributeRestrictedToMono;
        
        [RequireInterface(typeof(IDamageable))]
        [SerializeField] private ScriptableObject attributeRestrictedToSO;
        
        private void OnEnable()
        {
            damageable?.Value?.Damage(500f);
        }
    }
}