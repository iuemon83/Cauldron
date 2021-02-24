using MagicOnion.Client;
using System;
using UnityEngine;

namespace Assets.Scripts
{
    class ClientLogger : IMagicOnionClientLogger
    {
        void IMagicOnionClientLogger.Debug(string message)
        {
            Debug.Log(message);
        }

        void IMagicOnionClientLogger.Error(Exception ex, string message)
        {
            Debug.LogError(ex);
            Debug.LogError(message);
        }

        void IMagicOnionClientLogger.Information(string message)
        {
            Debug.Log(message);
        }

        void IMagicOnionClientLogger.Trace(string message)
        {
            Debug.Log(message);
        }
    }
}
