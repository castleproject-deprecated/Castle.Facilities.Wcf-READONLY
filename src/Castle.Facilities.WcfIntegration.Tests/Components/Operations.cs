// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Facilities.WcfIntegration.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	using Castle.Facilities.WcfIntegration.Tests.Behaviors;

	public class Operations : IOperationsEx
	{
		private readonly int number;

		public Operations(int number)
		{
			this.number = number;
		}

		public int GetValueFromConstructor()
		{
			return number;
		}

		public int GetValueFromConstructorAsRef(ref int refValue)
		{
			Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
			Thread.Sleep(2000);
			return (refValue = number);
		}

		public int GetValueFromConstructorAsRefAndOut(ref int refValue, out int outValue)
		{
			Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
			Thread.Sleep(2000);
			return (refValue = outValue = number);
		}

		public bool UnitOfWorkIsInitialized()
		{
			return UnitOfWork.initialized;
		}

		public void Backup(IDictionary<string, object> context)
		{
		}

		public void ThrowException()
		{
			throw new InvalidOperationException("Oh No!");
		}
	}

	internal delegate int GetValueFromConstructor();

	internal delegate int GetValueFromConstructorAsRef(ref int refValue);

	internal delegate int GetValueFromConstructorAsRefAndOut(ref int refValue, out int outValue);

	internal delegate bool UnitOfWorkIsInitialized();

	public class AsyncOperations : IAsyncOperations
	{
		private readonly Operations operations;
		private GetValueFromConstructor getValueCtor;
		private GetValueFromConstructorAsRef getValueCtorRef;
		private GetValueFromConstructorAsRefAndOut getValueCtorRefOut;
		private UnitOfWorkIsInitialized uow;

		public AsyncOperations(int number)
		{
			operations = new Operations(number);
			getValueCtor = operations.GetValueFromConstructor;
			getValueCtorRef = operations.GetValueFromConstructorAsRef;
			getValueCtorRefOut = operations.GetValueFromConstructorAsRefAndOut;
			uow = operations.UnitOfWorkIsInitialized;
		}

		public IAsyncResult BeginGetValueFromConstructor(AsyncCallback callback, object asyncState)
		{
			return getValueCtor.BeginInvoke(callback, asyncState);
		}

		public IAsyncResult BeginGetValueFromConstructorAsRef(
			ref int refValue, AsyncCallback callback, object asyncState)
		{
			return getValueCtorRef.BeginInvoke(ref refValue, callback, asyncState);
		}

		public IAsyncResult BeginGetValueFromConstructorAsRefAndOut(
			ref int refValue, AsyncCallback callback, object asyncState)
		{
			int outValue;
			return getValueCtorRefOut.BeginInvoke(ref refValue, out outValue, callback, asyncState);
		}

		public IAsyncResult BeginUnitOfWorkIsInitialized(AsyncCallback callback, object asyncState)
		{
			return uow.BeginInvoke(callback, asyncState);
		}

		public int EndGetValueFromConstructor(IAsyncResult result)
		{
			return getValueCtor.EndInvoke(result);
		}

		public int EndGetValueFromConstructorAsRef(ref int refValue, IAsyncResult result)
		{
			return getValueCtorRef.EndInvoke(ref refValue, result);
		}

		public int EndGetValueFromConstructorAsRefAndOut(ref int refValue, out int outValue, IAsyncResult result)
		{
			return getValueCtorRefOut.EndInvoke(ref refValue, out outValue, result);
		}

		public bool EndUnitOfWorkIsInitialized(IAsyncResult result)
		{
			return uow.EndInvoke(result);
		}
	}
}