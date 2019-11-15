// OSC Jack - Open Sound Control plugin for Unity
// https://github.com/keijiro/OscJack

using System;
using System.Collections.Generic;

namespace OscJack
{
    public sealed class OscMessageDispatcher
    {
        #region Callback delegate definition

        #endregion

        #region Public accessors

        public void AddCallback(string address, Action<string, OscDataHandle> callback)
        {
            lock (_callbackMap)
            {
                if (_callbackMap.ContainsKey(address))
                    _callbackMap[address] += callback;
                else
                    _callbackMap[address] = callback;
            }
        }

        public void RemoveCallback(string address, Action<string, OscDataHandle> callback)
        {
            lock (_callbackMap)
            {
                var temp = _callbackMap[address] - callback;
                if (temp != null)
                    _callbackMap[address] = temp;
                else
                    _callbackMap.Remove(address);
            }
        }

        #endregion

        #region Handler invocation

        internal void Dispatch(string address, OscDataHandle data)
        {
            lock (_callbackMap)
            {
                Action<string, OscDataHandle> callback;

                // Address-specified callback
                if (_callbackMap.TryGetValue(address, out callback))
                    callback(address, data);

                // Monitor callback
                if (_callbackMap.TryGetValue(string.Empty, out callback))
                    callback(address, data);
            }
        }

        #endregion

        #region Private fields

        Dictionary<string, Action<string, OscDataHandle>>
            _callbackMap = new Dictionary<string, Action<string, OscDataHandle>>();

        #endregion
    }
}
