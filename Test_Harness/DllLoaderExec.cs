/*////////////////////// Test Harness /////////////////////////
Source: Prof. Jim Fawcett                                  //
Author: Salim Zhulkhrni Shajahan                           //
Date: 06-Oct-2017 
Date Modified : 06-DEc-2017//
Version: 1.0                                               //
                                                           //
Demonstrating features of the Test Harness                 //
                                                           //
Functions:                                                 //
1)Test the files by executing .dll files sent by the child builders//
2) Displays Test Results to the user  
3) sends test logs as msgs to repo                         // 
        
Public interface
------------------

   1.string loadAndExerciseTesters() //loads assemblies from testers location(local storage)
   2.static void listen_test_thread() // listener thread for test harness
   3.public static void call_harness(String folder_path) //starts testing the .dll files
 Maintenance History: 
* -------------------- 
* ver 1.0 : 06 Dec 2017 
* - first release ////
///////////////////////////////////////////////////////////
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using MessagePassingComm;
using System.Threading;
using System.Diagnostics;

namespace Test_Harness
{
    public class DllLoaderExec
    {
        private static Comm comm { get; set; } = null;
        List<String> tested_dll_files = new List<String>();
        static String test_result = "";
       
        public static string testersLocation { get; set; } = ".";

        /*----< library binding error event handler >------------------*/
        /*
         *  This function is an event handler for binding errors when
         *  loading libraries.  These occur when a loaded library has
         *  dependent libraries that are not located in the directory
         *  where the Executable is running.
         */
        static Assembly LoadFromComponentLibFolder(object sender, ResolveEventArgs args)
        {
            Console.Write("\n  called binding error event handler");
            string folderPath = testersLocation;
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (!File.Exists(assemblyPath)) return null;
            Assembly assembly = Assembly.LoadFrom(assemblyPath);
            return assembly;
        }
        //----< load assemblies from testersLocation and run their tests >-----

        string loadAndExerciseTesters()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(LoadFromComponentLibFolder);
            

            try
            {
                DllLoaderExec loader = new DllLoaderExec();

                // load each assembly found in testersLocation

                string[] files = Directory.GetFiles(testersLocation, "*.dll");
                if (files.Count() > 0)
                {
                    foreach (string file in files)
                    {
                        String output = "";
                        if (!tested_dll_files.Contains(file))
                        {
                            
                            Assembly asm = Assembly.LoadFile(file);
                            string fileName = Path.GetFileName(file);
                            
                            Console.Write("\n  Loaded the file : {0} ", fileName);
                            output = output + Path.GetFileNameWithoutExtension(fileName)+" : ";

                            // exercise each tester found in assembly

                            Type[] types = asm.GetTypes();
                            foreach (Type t in types)
                            {
                                // if type supports ITest interface then run test

                                if (t.GetInterface("DllLoaderDemo.ITest", true) != null)
                                    if (!loader.runSimulatedTest(t, asm,output))
                                        Console.Write("\n  test {0} failed to run", t.ToString());
                            }
                            tested_dll_files.Add(file);
                        }
                        else { }
                    }
                }
                else
                {
                    Console.Write("***********************************************************");
                    Console.WriteLine("-------------No files available to test---------------");
                    Console.Write("***********************************************************");
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Simulated Testing completed";
        }
        //
        //----< run tester t from assembly asm >-------------------------------

        bool runSimulatedTest(Type t, Assembly asm,String t_res)
        {
            try
            {
                //Console.Write(
                 // "\n  attempting to create instance of {0}", t.ToString()
                 // );
                object obj = asm.CreateInstance(t.ToString());

                // announce test

                MethodInfo method = t.GetMethod("say");
                if (method != null)
                    method.Invoke(obj, new object[0]);

                // run test

                bool status = false;
                method = t.GetMethod("test");
                if (method != null)
                    status = (bool)method.Invoke(obj, new object[0]);

                Func<bool, string> act = (bool pass) =>
                {
                    if (pass)                        
                        return "Passed";                   
                    
                        return "Failed";
                    
                };
                Console.Write("\n  Test {0}", act(status));
                t_res = t_res + act(status);
                test_result = test_result + t_res +"|";
                
            }
            catch (Exception ex)
            {
                Console.Write("\n  test failed with message \"{0}\"", ex.Message);
                return false;
            }

            ///////////////////////////////////////////////////////////////////
            //  You would think that the code below should work, but it fails
            //  with invalidcast exception, even though the types are correct.
            //
            //    DllLoaderDemo.ITest tester = (DllLoaderDemo.ITest)obj;
            //    tester.say();
            //    tester.test();
            //
            //  This is a design feature of the .Net loader.  If code is loaded 
            //  from two different sources, then it is considered incompatible
            //  and typecasts fail, even thought types are Liskov substitutable.
            //
            return true;
        }
        //
        //----< extract name of current directory without its parents ---------

        string GuessTestersParentDir()
        {
            string dir = Directory.GetCurrentDirectory();
            int pos = dir.LastIndexOf(Path.DirectorySeparatorChar);
            string name = dir.Remove(0, pos + 1).ToLower();
            if (name == "debug")
                return "../..";
            else
                return ".";
        }
        
        public static void call_harness(String folder_path)
        {
            Console.Write("***************************************");
            Console.Write("\n ------- Inside Test Harness----------");
            Console.Write("\n *************************************\n");

            DllLoaderExec loader = new DllLoaderExec();

            

            DllLoaderExec.testersLocation = folder_path;

            DllLoaderExec.testersLocation = Path.GetFullPath(DllLoaderExec.testersLocation);
            Console.Write("\n  Loading Test Modules from:\n    {0}\n", Path.GetFullPath(@"../../../Test_Harness/Local_Storage"));

            // run load and tests
            
            string result = loader.loadAndExerciseTesters();
            Console.Write("\n\n  {0}", result);
            Console.Write("\n\n");
        }
       
        
        static void Main(String[] args)
        {
            Console.Title="Test Harness";
            Console.WriteLine("*************************************************");
            Console.Write("\n --------------Test Harness Started----------------");
            Console.Write("\n ************************************************\n");
            comm = new Comm("http://localhost", 8010);
            Thread test_harness_thread = null;
            test_harness_thread = new Thread(listen_test_thread);
            test_harness_thread.Start();
            

        }
        static void listen_test_thread()
        {
            Console.WriteLine();
            Console.WriteLine("**************************************************************");
            Console.WriteLine("***************Test Harness Listening*************************");
            Console.WriteLine("**************************************************************");
            if (comm != null)
            {
                while (true)
                {
                    CommMessage recv_msg = new CommMessage(CommMessage.MessageType.request);
                    recv_msg = comm.getMessage();
                    if (recv_msg.port_number!=0 && recv_msg.msg_body.Contains("test"))
                    {
                        //recv_msg.show();
                        string[] files = Directory.GetFiles(recv_msg.msg_body.Replace("test", ""), "*.dll");
                        List<String> l_files = new List<String>();
                        foreach (String str in files)
                        {
                            l_files.Add(Path.GetFileName(str));
                        }
                        string source_path = recv_msg.msg_body.Replace("test", "");
                        string dest_path = @"../../../Test_Harness/Local_Storage";
                        string source_file_name = "";
                        string source_file = "";
                        string destination_file = "";
                        foreach (String file in l_files)
                        {
                            destination_file = "";
                            source_file_name = "" + file;
                            source_file = System.IO.Path.Combine(source_path, source_file_name);
                            destination_file = System.IO.Path.Combine(dest_path, source_file_name);
                            System.IO.File.Copy(source_file, destination_file, true);
                        }
                        Console.WriteLine(" * *****************************************************");
                        Console.WriteLine("------------Required .dll Files are stored in the Testers path:{0}----------", System.IO.Path.GetFullPath(dest_path));
                        Console.WriteLine("******************************************************");
                        call_harness(recv_msg.msg_body.Replace("test",""));                        
                        //send message to repo
                        CommMessage test_result_msg = new CommMessage(CommMessage.MessageType.request);
                        test_result_msg.command = "from_harness";
                        test_result_msg.author = "Salim Zhulkhrni";
                        test_result_msg.to = "http://localhost:8092/IPluggableComm";
                        test_result_msg.from = "http://localhost:8010/IMessagePassingComm";
                        test_result_msg.msg_body = test_result;
                        test_result_msg.port_number = 8010;
                        comm.postMessage(test_result_msg);
                        ///////////////////////
                        
                        String[] test_logs = test_result_msg.msg_body.Split('|');
                        test_logs = test_logs.Where(arr => !String.IsNullOrEmpty(arr)).ToArray();
                        foreach (String file in test_logs)
                        {
                            File.WriteAllText(@"../../../Repo_Files/Test_log_For_" + file.Split(':')[0] + ".txt",file.Split(':')[0]+":"+ file.Split(':')[1]);
                            
                        }
                        
                        ///////////////
                        test_result = "";
                        //send test notification to client


                    }
                }
            }

        }
        /*
        [STAThread]
        static void Main()
        {
            Console.Write("\n  Demonstrating Robust Test Loader");
            Console.Write("\n ==================================\n");

            DllLoaderExec loader = new DllLoaderExec();

           
            // convert testers relative path to absolute path

            DllLoaderExec.testersLocation = @"../../../Build_Server/Build_Server_Local_Storage";

            DllLoaderExec.testersLocation = Path.GetFullPath(DllLoaderExec.testersLocation);

            // run load and tests

            string result = loader.loadAndExerciseTesters();

            Console.Write("\n\n  {0}", result);
            Console.Write("\n\n");
        }
        */
    }
}



