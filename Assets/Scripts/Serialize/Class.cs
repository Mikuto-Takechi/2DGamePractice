//< cite >< a href = "https://gist.github.com/hand-shouta-baba/72d0e82707e5ca820c87defabe9fd9da#file-jsondictionary-cs" > ˆø—pŒ³ </ a ></ cite >
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kogane
{
    [Serializable]
    public class JsonDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [Serializable]
        private struct KeyValuePair
        {
            [SerializeField][UsedImplicitly] private TKey key;
            [SerializeField][UsedImplicitly] private TValue value;

            public TKey Key => key;
            public TValue Value => value;

            public KeyValuePair(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
            }
        }

        [SerializeField][UsedImplicitly] private KeyValuePair[] dictionary = default;

        private Dictionary<TKey, TValue> m_dictionary;

        public Dictionary<TKey, TValue> Dictionary => m_dictionary;

        public JsonDictionary(Dictionary<TKey, TValue> dictionary)
        {
            m_dictionary = dictionary;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            dictionary = m_dictionary
                    .Select(x => new KeyValuePair(x.Key, x.Value))
                    .ToArray()
                ;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_dictionary = dictionary.ToDictionary(x => x.Key, x => x.Value);
            dictionary = null;
        }
    }
}