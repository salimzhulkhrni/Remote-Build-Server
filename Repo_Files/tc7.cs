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
            int pd = 0;
            pd = a * b;
            //Console.Write(sum);
            return pd;
    }
  }
}
