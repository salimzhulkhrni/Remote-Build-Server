///////////////////////////////////////////////////////////////////////////
// TestedLIb.cs - Simulates operation of a tested package                //
//                                                                       //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017       //
///////////////////////////////////////////////////////////////////////////

using System;


namespace DllLoaderDemo
{
  public class Tested : ITested
  {
    
    public int say(int a, int b)
    {
            //Console.Write("\n    Production code - TestedLib");
            //TestedLibDependency tld = new TestedLibDependency();
            //tld.sayHi();
            int sum = 0;
            sum = a + b;
            //Console.Write(sum);
            return sum+1;
    }
  }
}
