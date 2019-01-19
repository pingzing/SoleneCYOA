using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Solene.Models
{
    public struct MaybeResult<T, E>
    {
        private T _result;
        private E _error; 

        public bool IsOk { get; private set; }
        public bool IsError => !IsOk;

        public static MaybeResult<T, E> CreateOk(T result)
        {
            return new MaybeResult<T, E>
            {
                IsOk = true,                
                _result = result,
            };
        }

        public static MaybeResult<T, E> CreateError(E error)
        {
            return new MaybeResult<T, E>
            {
                IsOk = false,                
                _error = error,
            };
        }

        public T Unwrap()
        {
            if (!IsOk)
            {
                throw new MaybeResultException("Cannot unwrap a MaybeResult that IsError.");
            }

            return _result;
        }

        public T UnwrapOr(T or)
        {
            if (IsOk)
            {
                return _result;
            }
            else
            {
                return or;
            }
        }
       
        public E UnwrapError()
        {
            if (IsOk)
            {
                throw new MaybeResultException("Cannot unwrap the Error on a MaybeResult that IsOk.");
            }

            return _error;
        }

        public E UnwrapErrorOr(E or)
        {
            if (!IsOk)
            {
                return _error;
            }
            else
            {
                return or;
            }
        }
    }

    public class MaybeResultException : Exception
    {
        public MaybeResultException()
        {
        }

        public MaybeResultException(string message) : base(message)
        {
        }

        public MaybeResultException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MaybeResultException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
