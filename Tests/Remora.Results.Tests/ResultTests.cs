//
//  ResultTests.cs
//
//  Author:
//       Jarl Gullberg <jarl.gullberg@gmail.com>
//
//  Copyright (c) Jarl Gullberg
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using Xunit;

#pragma warning disable CS0618

namespace Remora.Results.Tests;

/// <summary>
/// Tests the <see cref="Result"/> struct.
/// </summary>
public static class ResultTests
{
    /// <summary>
    /// Tests the <see cref="Result.IsSuccess"/> property.
    /// </summary>
    public class IsSuccess
    {
        /// <summary>
        /// Tests whether <see cref="Result.IsSuccess"/> returns true on a successful result.
        /// </summary>
        [Fact]
        public void ReturnsTrueForSuccessfulResult()
        {
            var successful = Result.FromSuccess();
            Assert.True(successful.IsSuccess);
        }

        /// <summary>
        /// Tests whether <see cref="Result.IsSuccess"/> returns false on an unsuccessful result.
        /// </summary>
        [Fact]
        public void ReturnsFalseForUnsuccessfulResult()
        {
            var unsuccessful = Result.FromError(new GenericError("Dummy error"));
            Assert.False(unsuccessful.IsSuccess);
        }
    }

    /// <summary>
    /// Tests the <see cref="Result.Inner"/> property.
    /// </summary>
    public class Inner
    {
        /// <summary>
        /// Tests whether <see cref="Result.Inner"/> returns null if no wrapped result exists.
        /// </summary>
        [Fact]
        public void ReturnsNullIfNoWrappedResultExists()
        {
            var plainResult = Result.FromError(new GenericError("Dummy error"));
            Assert.Null(plainResult.Inner);
        }

        /// <summary>
        /// Tests whether <see cref="Result.Inner"/> returns a valid object if a wrapped result exists.
        /// </summary>
        [Fact]
        public void ReturnsObjectIfWrappedResultExists()
        {
            var wrapped = Result.FromError(new GenericError("Dummy wrapped"));
            var plainResult = Result.FromError(new GenericError("Wrapping"), wrapped);

            Assert.NotNull(plainResult.Inner);
        }

        /// <summary>
        /// Tests whether <see cref="Result.Inner"/> returns the correct object if a wrapped result exists.
        /// </summary>
        [Fact]
        public void ReturnsCorrectObjectIfWrappedResultExists()
        {
            var wrapped = Result.FromError(new GenericError("Dummy wrapped"));
            var plainResult = Result.FromError(new GenericError("Wrapping"), wrapped);

            Assert.Equal(wrapped, plainResult.Inner);
        }
    }

    /// <summary>
    /// Tests the <see cref="Result.Error"/> property.
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Tests whether <see cref="Result.Error"/> returns null if the result is successful.
        /// </summary>
        [Fact]
        public void ReturnsNullIfResultIsSuccessful()
        {
            var successful = Result.FromSuccess();
            Assert.Null(successful.Error);
        }

        /// <summary>
        /// Tests whether <see cref="Result.Error"/> returns an object if the result is unsuccessful.
        /// </summary>
        [Fact]
        public void ReturnsObjectIfResultIsUnsuccessful()
        {
            var unsuccessful = Result.FromError(new GenericError("Dummy error"));
            Assert.NotNull(unsuccessful.Error);
        }

        /// <summary>
        /// Tests whether <see cref="Result.Error"/> returns the correct object if the result is unsuccessful.
        /// </summary>
        [Fact]
        public void ReturnsCorrectObjectIfResultIsUnsuccessful()
        {
            var expected = new GenericError("Dummy error");
            var unsuccessful = Result.FromError(expected);

            Assert.Same(expected, unsuccessful.Error);
        }
    }

    /// <summary>
    /// Tests the <see cref="Result.Map{TOut}"/> method.
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Tests whether the method can perform a mapping.
        /// </summary>
        [Fact]
        public void CanMap()
        {
            var some = Result.FromSuccess();

            var mapped = some.Map(1);

            Assert.True(mapped.IsSuccess);
        }

        /// <summary>
        /// Tests whether the method returns an unsuccessful result if the original result is unsuccessful.
        /// </summary>
        [Fact]
        public void ReturnsUnsuccessfulIfOriginalIsUnsuccessful()
        {
            Result err = new InvalidOperationError();

            var mapped = err.Map(1);

            Assert.False(mapped.IsSuccess);
        }

        /// <summary>
        /// Tests whether the method preserves the inner result if the original result has one.
        /// </summary>
        [Fact]
        public void PreservesInnerResult()
        {
            Result inner = new NotFoundError();
            var err = Result.FromError(new InvalidOperationError(), inner);

            var mapped = err.Map(1);

            Assert.False(mapped.IsSuccess);
            Assert.Equal(mapped.Inner, inner);
        }
    }

    /// <summary>
    /// Tests the <see cref="Result.MapOrElse{TOut}"/> method.
    /// </summary>
    public class MapOrElse
    {
        /// <summary>
        /// Tests whether the method can perform a mapping.
        /// </summary>
        [Fact]
        public void ReturnsValueIfSuccessful()
        {
            var some = Result.FromSuccess();

            var mapped = some.MapOrElse(2, (_, _) => 1);

            Assert.Equal(2, mapped);
        }

        /// <summary>
        /// Tests whether the method can perform a mapping.
        /// </summary>
        [Fact]
        public void ReturnsFallbackIfUnsuccessful()
        {
            Result err = new InvalidOperationError();

            var mapped = err.MapOrElse(2, (_, _) => 1);

            Assert.Equal(1, mapped);
        }
    }

    /// <summary>
    /// Tests the <see cref="Result.MapError{TError}(Func{IResultError, IResult?, TError})"/> method and its overloads.
    /// </summary>
    public class MapError
    {
        /// <summary>
        /// Tests whether the method can perform a mapping.
        /// </summary>
        [Fact]
        public void CanMapError()
        {
            Result err = new InvalidOperationError();

            var mapped = err.MapError((_, _) => new NotFoundError());

            Assert.False(mapped.IsSuccess);
            Assert.IsType<NotFoundError>(mapped.Error);
        }

        /// <summary>
        /// Tests whether the method can perform a mapping and overwrite the inner error.
        /// </summary>
        [Fact]
        public void CanMapErrorWithNewInner()
        {
            Result err = new InvalidOperationError();

            var mapped = err.MapError((_, _) => (new NotFoundError(), Result.FromError(new InvalidOperationError())));

            Assert.False(mapped.IsSuccess);
            Assert.IsType<NotFoundError>(mapped.Error);
            Assert.NotNull(mapped.Inner);
            Assert.IsType<InvalidOperationError>(mapped.Inner!.Error);
        }

        /// <summary>
        /// Tests whether the method returns a successful result if the original result is successful.
        /// </summary>
        [Fact]
        public void ReturnsSuccessfulIfOriginalIsSuccessful()
        {
            var some = Result.FromSuccess();

            var mapped = some.MapError((_, _) => new NotFoundError());

            Assert.True(mapped.IsSuccess);
        }

        /// <summary>
        /// Tests whether the method returns a successful result if the original result is successful.
        /// </summary>
        [Fact]
        public void ReturnsSuccessfulIfOriginalIsSuccessfulWithNewInner()
        {
            var some = Result.FromSuccess();

            var mapped = some.MapError((_, _) => (new NotFoundError(), Result.FromError(new InvalidOperationError())));

            Assert.True(mapped.IsSuccess);
        }

        /// <summary>
        /// Tests whether the method preserves the inner result if the original result has one.
        /// </summary>
        [Fact]
        public void PreservesInnerResult()
        {
            Result inner = new NotFoundError();
            Result err = Result.FromError(new InvalidOperationError(), inner);

            var mapped = err.MapError((_, _) => new NotFoundError());

            Assert.False(mapped.IsSuccess);
            Assert.IsType<NotFoundError>(mapped.Error);
            Assert.Equal(mapped.Inner, inner);
        }
    }

    /// <summary>
    /// Tests the <see cref="Result.FromSuccess"/> method.
    /// </summary>
    public class FromSuccess
    {
        /// <summary>
        /// Tests whether <see cref="Result.FromSuccess"/> creates a successful result.
        /// </summary>
        [Fact]
        public void CreatesASuccessfulResult()
        {
            var successful = Result.FromSuccess();
            Assert.True(successful.IsSuccess);
        }
    }

    /// <summary>
    /// Tests the <see cref="Result.FromError{TError}(TError)"/> method and its overloads.
    /// </summary>
    public class FromError
    {
        /// <summary>
        /// Tests whether <see cref="Result.FromSuccess"/> creates an unsuccessful result from a plain error
        /// instance.
        /// </summary>
        [Fact]
        public void CreatesAnUnsuccessfulResultFromAnErrorInstance()
        {
            var result = Result.FromError(new GenericError("Dummy error"));
            Assert.False(result.IsSuccess);
        }

        /// <summary>
        /// Tests whether <see cref="Result.FromSuccess"/> creates an unsuccessful result from a plain error
        /// instance and a wrapped result.
        /// </summary>
        [Fact]
        public void CreatesAnUnsuccessfulResultFromAnErrorInstanceAndAWrappedResult()
        {
            var wrapped = Result.FromError(new GenericError("Dummy error."));
            var result = Result.FromError(new GenericError("Dummy error"), wrapped);

            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Inner);
        }

        /// <summary>
        /// Tests whether <see cref="Result.FromSuccess"/> creates an unsuccessful result from another result type.
        /// </summary>
        [Fact]
        public void CreatesAnUnsuccessfulResultFromAnotherResult()
        {
            var wrapped = Result<int>.FromError(new GenericError("Dummy error."));
            var result = Result.FromError(wrapped);

            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Inner);
            Assert.IsType<GenericError>(result.Error);
        }
    }

    /// <summary>
    /// Tests the <see cref="Result.op_Implicit(ResultError)"/> operator.
    /// </summary>
    public class ResultErrorOperator
    {
        /// <summary>
        /// Tests whether <see cref="Result.op_Implicit(ResultError)"/> creates an unsuccessful result from a plain
        /// error instance.
        /// </summary>
        [Fact]
        public void CreatesAnUnsuccessfulResultFromAnErrorInstance()
        {
            Result result = new GenericError("Dummy error");
            Assert.False(result.IsSuccess);
        }
    }

    /// <summary>
    /// Tests the <see cref="Result.op_Implicit(Exception)"/> operator.
    /// </summary>
    public class ExceptionOperator
    {
        /// <summary>
        /// Tests whether <see cref="Result.op_Implicit(Exception)"/> creates an unsuccessful result from an
        /// exception instance.
        /// </summary>
        [Fact]
        public void CreatesAnUnsuccessfulResultFromAnExceptionInstance()
        {
            Result result = new Exception("Dummy error");

            Assert.False(result.IsSuccess);
            Assert.IsType<ExceptionError>(result.Error);
        }
    }
}
