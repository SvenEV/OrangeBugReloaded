using System;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Logs information.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Occurs when a less relevant info text has been logged.
        /// </summary>
        public static event Action<object> Logged;

        /// <summary>
        /// Occurs when a more relevant info text has been logged.
        /// </summary>
        public static event Action<object> Info;

        /// <summary>
        /// Occurs when a warning has been logged.
        /// </summary>
        public static event Action<object> Warning;

        /// <summary>
        /// Occurs when an error has been logged.
        /// </summary>
        public static event Action<object> Error;

        /// <summary>
        /// Logs a less relevant info.
        /// </summary>
        /// <param name="text">Text</param>
        public static void Log(object text)
        {
            Logged?.Invoke(text);
        }

        /// <summary>
        /// Logs a more relevant info.
        /// </summary>
        /// <param name="text">Text</param>
        public static void LogInfo(object text)
        {
            Info?.Invoke(text);
        }

        /// <summary>
        /// Logs a warning.
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="throwException">True if an exception should be thrown</param>
        public static void LogWarning(object text, bool throwException = false)
        {
            if (throwException) throw new InvalidOperationException(text.ToString());
            else Warning?.Invoke(text);
        }

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="throwException">True if an exception should be thrown</param>
        public static void LogError(object text, bool throwException = false)
        {
            if (throwException) throw new InvalidOperationException(text.ToString());
            else Error?.Invoke(text);
        }
    }
}
