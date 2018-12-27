/*//////////////////// Mother Builder////////////////////////
Source: Prof.Jim Fawcett
Author: Salim Zhulkhrni Shajahan                           //
Date Created: 31-Oct-2017  
Date Modified: 06-Dec-2017//
Version: 1.1                                               //
                                                           //
Demonstrating features of the Mother Builder               //
                                                           //
Package Functions:                                                 //
1)Sends message to repository to transfer the files in build//
storage(mother builder's) location                         //
2)Allocates test request to the proces from the available  //
pool of processes in the queue                             //
 3) commuiicates with repo & child builders
 4) sends files to child builders after parsing the test req

 Public Interface
 ----------------
 //1.void start_builder(int ch_proc)// starts mother builder & initiates child builders
 2.listen_builder_thread()// listener thread for mother builder
 3.void getFiles(string pattern, string id) //get files based on a pattern
 4.void generate_xml() // enqueue test req in the blocking queue
 5.build_req_thread ()//thread for build request and allocating request to the available thread

 Maintenance History: 
* -------------------- 
* ver 1.0 : 06 Dec 2017 
* - first release //
///////////////////////////////////////////////////////////
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using MessagePassingComm;
using System.Threading;

namespace Mother_Builder_Sp
{
    public class Mother_Builder
    {
        private SWTools.BlockingQueue<String> rd_q = new SWTools.BlockingQueue<String>();
        private SWTools.BlockingQueue<String> br_q = new SWTools.BlockingQueue<String>();
        Thread build_thread = null; // mother_builder thread 
        Thread br_thread = null; // br & rdQ thread
        Comm comm = null;

        public Mother_Builder()
        {
            comm = new Comm("http://localhost", 8091);

        }

        static bool createProcess(int i)
        {
            Process proc = new Process();
            string fileName = "..\\..\\..\\Build_Server\\bin\\debug\\Build_Server.exe";
            string absFileSpec = Path.GetFullPath(fileName);

            Console.Write("\n  attempting to start {0}", absFileSpec);
            string commandline = i.ToString();
            try
            {
                Process.Start(fileName, commandline);
                
            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }
        static bool create_proc_test_harness()
        {
            Process proc = new Process();
            string fileName = "..\\..\\..\\Test_Harness\\bin\\debug\\Test_Harness.exe";
            string absFileSpec = Path.GetFullPath(fileName);

            Console.Write("\n  attempting to start {0}", absFileSpec);
            int i = 1;
            string commandline = i.ToString();
            try
            {
                Process.Start(fileName, commandline);
                Console.WriteLine("TH:{0}",fileName);

            }
            catch (Exception ex)
            {
                Console.Write("\n  {0}", ex.Message);
                return false;
            }
            return true;
        }


        public void start_builder(int ch_proc)
        {
            generate_xml();
            Console.WriteLine("***********************************************************");
            Console.WriteLine("****************Mother Builder Started*********************");
            Console.WriteLine("***********************************************************");
            // start listening the br and rdQ thread //
            br_thread = new Thread(build_req_thread);
            br_thread.Start();
            ///start listening the mb thread //
            build_thread = new Thread(listen_builder_thread);
            build_thread.Start();
            //initiate test harness
            if (create_proc_test_harness())
            {
                Console.Write(" - succeeded");
            }
            else
            {
                Console.Write(" - failed");
            }
            //initiate child process//
            int count = ch_proc;
            for (int i = 1; i <= count; ++i)
            {
                if (createProcess(i))
                {
                    Console.Write(" - succeeded");
                }
                else
                {
                    Console.Write(" - failed");
                }

            }
            
            // send msg to repo for file request //
            CommMessage sndMsg_to_repo = new CommMessage(CommMessage.MessageType.request);
            sndMsg_to_repo.command = "Get_Files_From_Repo";
            sndMsg_to_repo.author = "Salim Zhulkhrni";
            sndMsg_to_repo.to = "http://localhost:8092/IPluggableComm";
            sndMsg_to_repo.from = "http://localhost:8091/IMessagePassingComm";
            sndMsg_to_repo.msg_body = "8091";
            sndMsg_to_repo.port_number = 8091;
            comm.postMessage(sndMsg_to_repo);
            Console.WriteLine();
            Console.WriteLine("***********************************************************");
            Console.WriteLine("********Inside Mother Builder: Request Files From Repository***");
            Console.WriteLine("***********************************************************");
            
            sndMsg_to_repo.show();
        }

        // < thread for builder > //
        void listen_builder_thread()
        {
            Console.WriteLine("***********************************************************");
            Console.WriteLine("************* Mother Builder Thread Running*****************");
            Console.WriteLine("***********************************************************");
            while (true)
            {
                CommMessage recv_msg = null;
                recv_msg = comm.getMessage();
                Console.WriteLine("***********************************************************");
                Console.WriteLine("********Inside Mother Builder Thread:Received The Message***");
                Console.WriteLine("***********************************************************");
                recv_msg.show();
                // send reply message to child process
                if (recv_msg.port_number != 0 && recv_msg.msg_body.Contains("child"))
                {
                    rd_q.enQ(recv_msg.port_number.ToString()); // enque the child's port number in the queue
                }
            }            
        }
        //<thread for build request and allocating request to the available thread>//
        void build_req_thread()
        {
            string build_req = "";
            string avail_proc = "";
            while (true)
            {
                if (br_q.size() != 0)
                {
                    if (rd_q.size() != 0)
                    {
                        build_req = build_req + br_q.deQ();
                        avail_proc = avail_proc + rd_q.deQ();
                        Console.WriteLine("***********************************************************");
                        Console.WriteLine("*************Allocating test request to child with port number:{0}****",avail_proc);
                        Console.WriteLine("***********************************************************");
                        CommMessage send_to_child = new CommMessage(CommMessage.MessageType.request);
                        send_to_child.command = "To Child With Port:" + avail_proc;
                        send_to_child.author = "Salim Zhulkhrni";
                        send_to_child.to = "http://localhost:" + avail_proc + "/IPluggableComm";
                        send_to_child.from = "http://localhost:8091/IMessagePassingComm";
                        send_to_child.msg_body = "to_child" + build_req;
                        send_to_child.port_number = 8091;
                        comm.postMessage(send_to_child);
                        
                        build_req = "";
                        avail_proc = "";
                    }
                    
                }
            }

        }
        //<generate xml string from xml file(test request) >//
        void generate_xml()
        {
            List<String> xml_string_list = new List<String>();
            string temp_str = "";
            String[] xml_files_list = Directory.GetFiles(@"../../../Repo_Files/", "*.xml");
            for (int i = 0; i < xml_files_list.Count(); i++)
            {
                temp_str = File.ReadAllText("" + xml_files_list[i]);
                xml_string_list.Add(temp_str);
                temp_str = "";
            }
            foreach (String str in xml_string_list)
                br_q.enQ(str); // enqueue all the test request            
        }

        static void Main(string[] args)
        {
            Console.Title = "SpawnProc";
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write("\n  Demo Parent Process");
            Console.Write("\n =====================");
            if (args.Count() == 0)
            {
                Console.Write("\n  please enter number of processes to create on command line");
                return;
            }
            else
            {
                int count = Int32.Parse(args[0]);
                for (int i = 1; i <= count; ++i)
                {
                    if (createProcess(i))
                    {
                        Console.Write(" - succeeded");
                    }
                    else
                    {
                        Console.Write(" - failed");
                    }
                }
            }
            Console.Write("\n  Press key to exit");
            Console.ReadKey();
            Console.Write("\n  ");
        } 


    }
}
