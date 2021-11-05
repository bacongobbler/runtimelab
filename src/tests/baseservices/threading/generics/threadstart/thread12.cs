// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System;
using System.Threading;


interface IGen<T>
{
	void Target();
	T Dummy(T t);
}

class Gen<T> : IGen<T>
{
	public T Dummy(T t) { return t; }

	public virtual void Target()
	{		
		Interlocked.Increment(ref Test_thread12.Xcounter);
	}
	
	public static void ThreadPoolTest()
	{
		Thread[] threads = new Thread[Test_thread12.nThreads];
		IGen<T> obj = new Gen<T>();

		for (int i = 0; i < Test_thread12.nThreads; i++)
		{	
			threads[i]  = new Thread(new ThreadStart(obj.Target));
			threads[i].Start();
		}

		for (int i = 0; i < Test_thread12.nThreads; i++)
		{	
			threads[i].Join();
		}
		
		Test_thread12.Eval(Test_thread12.Xcounter==Test_thread12.nThreads);
		Test_thread12.Xcounter = 0;
	}
}

public class Test_thread12
{
	public static int nThreads =50;
	public static int counter = 0;
	public static int Xcounter = 0;
	public static bool result = true;
	public static void Eval(bool exp)
	{
		counter++;
		if (!exp)
		{
			result = exp;
			Console.WriteLine("Test Failed at location: " + counter);
		}
	
	}
	
	public static int Main()
	{
		Gen<int>.ThreadPoolTest();
		Gen<double>.ThreadPoolTest();
		Gen<string>.ThreadPoolTest();
		Gen<object>.ThreadPoolTest(); 
		Gen<Guid>.ThreadPoolTest(); 

		Gen<int[]>.ThreadPoolTest(); 
		Gen<double[,]>.ThreadPoolTest();
		Gen<string[][][]>.ThreadPoolTest(); 
		Gen<object[,,,]>.ThreadPoolTest();
		Gen<Guid[][,,,][]>.ThreadPoolTest();

		if (result)
		{
			Console.WriteLine("Test Passed");
			return 100;
		}
		else
		{
			Console.WriteLine("Test Failed");
			return 1;
		}
	}
}		


