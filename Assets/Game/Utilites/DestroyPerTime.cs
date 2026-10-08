using System.Collections;
using UnityEngine;

namespace Game.Utilities
{
    public class DestroyPerTime : MonoBehaviour
    {
        [SerializeField] private float lifetime = 1f;
        private Coroutine destroyRoutine;

        private void OnEnable()
        {
            // «апускаем корутину удалени€
            destroyRoutine = StartCoroutine(DestroyRoutine());
        }

        public void SetLifetime(float time)
        {
            lifetime = time;

            // ѕерезапускаем корутину, если врем€ изменилось
            if (destroyRoutine != null)
                StopCoroutine(destroyRoutine);

            destroyRoutine = StartCoroutine(DestroyRoutine());
        }

        private IEnumerator DestroyRoutine()
        {
            yield return new WaitForSeconds(lifetime);

            if (gameObject != null)
                Destroy(gameObject);
        }

        private void OnDisable()
        {
            // Ќа вс€кий случай очищаем корутину
            if (destroyRoutine != null)
                StopCoroutine(destroyRoutine);
        }
    }
}