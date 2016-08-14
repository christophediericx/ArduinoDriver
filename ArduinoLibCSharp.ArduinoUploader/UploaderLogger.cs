using System;
using NLog;

namespace ArduinoLibCSharp.ArduinoUploader
{
    internal class UploaderLogger
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        internal static void LogAndThrowError<TException>(string errorMessage) where TException : Exception, new()
        {
            Logger.Error(errorMessage);
            var exception = (TException)Activator.CreateInstance(typeof(TException), errorMessage);
            throw exception;
        }
    }
}
