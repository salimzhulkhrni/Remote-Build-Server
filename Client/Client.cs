/*////////////////////// Mock Client /////////////////////////
Author: Salim Zhulkhrni Shajahan                           //
Date: 06-Oct-2017                                          //
Version: 1.0                                               //
                                                           //
Demonstrating features of the client                       //
                                                           //
Functions:                                                 //
1)Creates a Test Request                                   //
2)Send the Test Request to Repository                      //
3)Sends all the files to the repository                    //
4) Test Stubs created to test each function                //
                                                           //
///////////////////////////////////////////////////////////
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Threading;
using Build_Server;
using Repository_Mock;
using MessagePassingComm;


namespace Client_Mock
{
    public class Client
    {
        public void call_client()
        {
            TestElement te1 = new TestElement();
            te1.testName = "test1";
            te1.addDriver("td1.cs");
            te1.addCode("tc1.cs");
            te1.addCode("tc2.cs");
            //te1.addCode("tc3.cs");


            TestElement te2 = new TestElement();
            te2.testName = "test2";
            te2.addDriver("td2.cs");
            te2.addCode("tc3.cs");
            te2.addCode("tc4.cs");


            TestElement te3 = new TestElement();
            te3.testName = "test3";
            te3.addDriver("td3.cs");
            te3.addCode("tc5.cs");
            te3.addCode("tc6.cs");
            //te1.addCode("tc3.cs");


            TestElement te4 = new TestElement();
            te4.testName = "test4";
            te4.addDriver("td4.cs");
            te4.addCode("tc7.cs");
            te4.addCode("tc8.cs");

            TestElement te5 = new TestElement();
            te5.testName = "test5";
            te5.addDriver("td5.cs");
            te5.addCode("tc9.cs");
            te5.addCode("tc10.cs"); 

            TestRequest tr = new TestRequest();
            tr.author = "Salim Zhulkhrni";
            tr.tests.Add(te1);
            tr.tests.Add(te2);
            tr.tests.Add(te3);
            tr.tests.Add(te4);
            tr.tests.Add(te5);

            // creating test request //
            string trXml = tr.ToXml();
            Console.WriteLine("************************************************");
            Console.WriteLine("-----------------Test Request------------------");
            Console.WriteLine("************************************************");
            Console.Write(trXml);
            Console.WriteLine();
            //sending files to the repository //
            RepoMock rm = new RepoMock();
            rm.send_files_to_mock_repo();
            Console.WriteLine();
            Console.WriteLine("*********************************************************************");
            Console.WriteLine("-----------------Sending Test Request to Repository------------------");
            Console.WriteLine("*********************************************************************");
            string repo_path = @"..\..\..\MockRepo\RepoStorage\Files\XMLDocument.xml";
            File.WriteAllText(repo_path,trXml);
            Build_Server.Build_Server bs = new Build_Server.Build_Server();            
            //bs.receive_string_from_client(trXml);
        }
        public void start_build()
        {
            int c = 3;
            Mother_Builder mb = new Mother_Builder();
            mb.start_builder(c);
            MockRepo rm = new MockRepo();
            Thread repo_thread_start = new Thread(rm.repo_main_thrd);
            repo_thread_start.Start();
        }
        
    }
// test_stub
#if (Client_Demo)
    class Client_Demo
    {
        static void Main()
        {
            //Console.WriteLine("********Demonstrating the functions of Mock Client*********");
            int s=3;
            Console.WriteLine("Starting Point");
            //Client cl = new Client();
            //cl.call_client();  // testing the function - call_client()
            Mother_Builder mb = new Mother_Builder();
            mb.start_builder(s);
            MockRepo rm = new MockRepo();
            Thread repo_thread_start = new Thread(rm.repo_main_thrd);
            repo_thread_start.Start();      
        }
    }
#endif
}
