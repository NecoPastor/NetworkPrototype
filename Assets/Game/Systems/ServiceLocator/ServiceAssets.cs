using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems.Service
{
    [CreateAssetMenu()]
    public class ServiceAssets : ScriptableObject
    {
        public List<MonoBehaviour> servicePrefabs;
    }
}