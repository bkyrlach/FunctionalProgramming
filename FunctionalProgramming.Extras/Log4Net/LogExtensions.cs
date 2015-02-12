using System;
using FunctionalProgramming.Basics;
using FunctionalProgramming.Monad;
using log4net;

namespace FunctionalProgramming.Extras.Log4Net
{
    /// <summary>
    /// Extensions to Log4Net that wrap the standard log functions in an Io, allowing log statements to be composed into a program
    /// </summary>
    public static class LogExtensions
    {
        #region debug
        public static Io<Unit> DebugIo(this ILog logger, string msg)
        {
            return Io.Apply(() => logger.Debug(msg));
        }

        public static Io<Unit> DebugIo(this ILog logger, string msg, Exception ex)
        {
            return Io.Apply(() => logger.Debug(msg, ex));
        }

        public static Io<Unit> DebugIo(this ILog logger, Exception ex)
        {
            return Io.Apply(() => logger.Debug(ex));
        }

        public static Io<Unit> DebugFormatIo(this ILog logger, string formatString, params object[] args)
        {
            return Io.Apply(() => logger.DebugFormat(formatString, args));
        }
        #endregion debug

        #region info
        public static Io<Unit> InfoIo(this ILog logger, string msg)
        {
            return Io.Apply(() => logger.Info(msg));
        }

        public static Io<Unit> InfoIo(this ILog logger, string msg, Exception ex)
        {
            return Io.Apply(() => logger.Info(msg, ex));
        }

        public static Io<Unit> InfoIo(this ILog logger, Exception ex)
        {
            return Io.Apply(() => logger.Info(ex));
        }

        public static Io<Unit> InfoFormatIo(this ILog logger, string formatString, params object[] args)
        {
            return Io.Apply(() => logger.InfoFormat(formatString, args));
        }
        #endregion info

        #region warn
        public static Io<Unit> WarnIo(this ILog logger, string msg)
        {
            return Io.Apply(() => logger.Warn(msg));
        }

        public static Io<Unit> WarnIo(this ILog logger, string msg, Exception ex)
        {
            return Io.Apply(() => logger.Warn(msg, ex));
        }

        public static Io<Unit> WarnIo(this ILog logger, Exception ex)
        {
            return Io.Apply(() => logger.Warn(ex));
        }

        public static Io<Unit> WarnFormatIo(this ILog logger, string formatString, params object[] args)
        {
            return Io.Apply(() => logger.WarnFormat(formatString, args));
        }
        #endregion warn

        #region error
        public static Io<Unit> ErrorIo(this ILog logger, string msg)
        {
            return Io.Apply(() => logger.Error(msg));
        }

        public static Io<Unit> ErrorIo(this ILog logger, string msg, Exception ex)
        {
            return Io.Apply(() => logger.Error(msg, ex));
        }

        public static Io<Unit> ErrorIo(this ILog logger, Exception ex)
        {
            return Io.Apply(() => logger.Error(ex));
        }

        public static Io<Unit> ErrorFormatIo(this ILog logger, string formatString, params object[] args)
        {
            return Io.Apply(() => logger.ErrorFormat(formatString, args));
        }
        #endregion error

        #region fatal
        public static Io<Unit> FatalIo(this ILog logger, string msg)
        {
            return Io.Apply(() => logger.Fatal(msg));
        }

        public static Io<Unit> FatalIo(this ILog logger, string msg, Exception ex)
        {
            return Io.Apply(() => logger.Fatal(msg, ex));
        }

        public static Io<Unit> FatalIo(this ILog logger, Exception ex)
        {
            return Io.Apply(() => logger.Fatal(ex));
        }

        public static Io<Unit> FatalFormatIo(this ILog logger, string formatString, params object[] args)
        {
            return Io.Apply(() => logger.FatalFormat(formatString, args));
        }
        #endregion fatal
    }
}
