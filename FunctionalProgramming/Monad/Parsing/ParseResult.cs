using System;
using System.Collections.Generic;

namespace FunctionalProgramming.Monad.Parsing
{
    /// <summary>
    /// This interface represents a sum type for a parse result for a given input type.
    /// </summary>
    /// <typeparam name="TInput">The type of input being parsed</typeparam>
    /// <typeparam name="TResult">The parsed result</typeparam>
    public interface IParseResult<out TInput, out TResult>
    {
        /// <summary>
        /// This method attempts to simulate ML style pattern matching for the algebraic data type IParseResult. Each case
        /// is represented by a function that's given access to internal values of the specific representation.
        /// </summary>
        /// <typeparam name="T">The type of result to generate</typeparam>
        /// <param name="success">A lambda that computes a 'T off of a successful parse result</param>
        /// <param name="failure">A lambda that computes a 'T off of a failed parse result</param>
        /// <returns>The value computed by the lambda corresponding to the specific type matched on</returns>
        T Match<T>(Func<TResult, IStream<TInput>, T> success, Func<string, IStream<TInput>, T> failure);
    }

    /// <summary>
    /// Represents successfully parsing some portion of TInput into a TResult
    /// </summary>
    /// <typeparam name="TInput">The type of input being parsed</typeparam>
    /// <typeparam name="TResult">The type of result generated</typeparam>
    public class SuccessResult<TInput, TResult> : IParseResult<TInput, TResult>
    {
        private readonly TResult _t;
        private readonly IStream<TInput> _rest;

        public SuccessResult(TResult t, IStream<TInput> rest)
        {
            _t = t;
            _rest = rest;
        }

        public T Match<T>(Func<TResult, IStream<TInput>, T> success, Func<string, IStream<TInput>, T> failure)
        {
            return success(_t, _rest);
        }

        public override string ToString()
        {
            return _t.ToString();
        }
    }

    /// <summary>
    /// Represents the failure to parse some portion of TInput into a TResult
    /// </summary>
    /// <typeparam name="TInput">The type of input being parsed</typeparam>
    /// <typeparam name="TResult">The type of result that wasn't able to be generated</typeparam>
    public class FailureResult<TInput, TResult> : IParseResult<TInput, TResult>
    {
        private readonly string _error;
        private readonly IStream<TInput> _rest;

        public FailureResult(string error, IStream<TInput> rest)
        {
            _error = error;
            _rest = rest;
        }

        public T Match<T>(Func<TResult, IStream<TInput>, T> success, Func<string, IStream<TInput>, T> failure)
        {
            return failure(_error, _rest);
        }

        public override string ToString()
        {
            return string.Format("Parsing failed: {0}", _error);
        }
    }
}
