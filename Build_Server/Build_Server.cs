/*////////////////////Build Server //////////////////////////
Author: Salim Zhulkhrni Shajahan,sshajaha@syr.edu          //
Original Date: 06-Oct-2017                                 //
Date Modified: 06-Dec-2017                                 //
Version: 1.1                                               //
                                                           //
Demonstrating features of the child builders
                                                           //
Functions:                                                 //
1)Parses the Test Request received from client             //
2)Receives the files from mother builder and stores in temp.storage//
3)Build and generates the .dll files                       //
4) Sends msg to the repository to store build logs          //
5)Sends Message to test harness to forward the .dll files   //
6) Thread for each child process that communicates with mother builder//
7) sends ready msg back to mother to indicate its avaiability
Public Interface:
-----------------
1.receive_string_from_client(String xml,string child_num,out string path) //parses the test request and generates .dll files
2.public bool load_builder_files(List<String> fbuild,String child_folder,List<String> test_names) // build the files
3.void listen_child_thread() // child thread for listening

Maintenance History: 
* -------------------- 
* ver 1.0 : 06 Dec 2017 
* - first release 
//
///////////////////////////////////////////////////////////
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Utilities;
using System.IO;
using Repository_Mock;
using Test_Harness;
using MessagePassingComm;
using System.Threading;

namespace Build_Server
{
    
   public  class Build_Server
    {
        private static Comm comm { get; set; } = null;
        static int str_port ;
        static string proc_num = "";
        
        public bool receive_string_from_client(String xml_str_client,string child_num,out string c_path_folder)
        {
            String child_path = "";
            child_path= @"../../../Build_Server/Build_Server_Local_Storage/child" + child_num;
            DirectoryInfo d = Directory.CreateDirectory(child_path);
            c_path_folder = "" + child_path;            
            Console.WriteLine();            
            TestRequest newRequest = xml_str_client.FromXml<TestRequest>();
            string typeName = newRequest.GetType().Name;
            Console.WriteLine("**********************************************************");
            Console.WriteLine("-----Deserializing xml string results in type: {0} -------------", typeName);
            Console.WriteLine("**********************************************************");
            Console.Write(newRequest);
            Console.WriteLine();
            Console.WriteLine();
            List<string> file_names = new List<string>(); // list of strings containing file names
            List<String> f_build = new List<String>();  // to generate dll files
            List<String> test_case_names = new List<String>();
            String temp_str = "";
            foreach (TestElement test_element in newRequest.tests)
            {
                if (test_element.testName !=null)
                {
                    test_case_names.Add(test_element.testName);
                    if (test_element.testDriver != null)
                    {
                        file_names.Add(test_element.testDriver);
                        temp_str = temp_str + test_element.testDriver + " ";
                    }
                    foreach (String test_codes in test_element.testCodes)
                    {
                        file_names.Add(test_codes);
                        temp_str = temp_str + test_codes + " ";
                    }
                    //Console.Write(temp_str);
                    f_build.Add(temp_str);
                    temp_str = "";
                }
            }
            string c_path = "";
            c_path=c_path+copy_to_child_storage(file_names,child_path);  //copy files to child local storage

            if (!c_path.Equals("File_Not_Found"))
            {
                bool flag = load_builder_files(f_build, c_path,test_case_names);    //load dll files

                if (flag == true)   // test harness will be called only after all dependency related files for all the test cases are placed
                {
                    Console.WriteLine();

                    Console.WriteLine("**************************************");
                    Console.WriteLine("-------------Build Success-------------");
                    Console.WriteLine("**************************************");
                    return true;
                    
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("**************************************");
                    Console.WriteLine("-------------Build Failed-------------");
                    Console.WriteLine("**************************************");
                    return false;
                }

            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("**************************************");
                Console.WriteLine("-------------Cannot Build Required Files Not Found-------------");
                Console.WriteLine("**************************************");
                return false;
            }
        }

        // copy files from repository to build server's local storage //
        public String copy_to_child_storage(List<String> files,String child_path)
        {
            
            try
            {
                string source_path = @"../../../Repo_Files";
                string dest_path = child_path;
                string source_file_name = "";
                string source_file = "";
                string destination_file = "";
                foreach (String file in files)
                {
                    source_file_name = "" + file;
                    source_file = System.IO.Path.Combine(source_path, source_file_name);
                    destination_file = System.IO.Path.Combine(dest_path, file);
                    System.IO.File.Copy(source_file, destination_file, true);
                }
                Console.WriteLine("******************************************************");
                Console.WriteLine("------------Required Files are stored in the path:{0}----------", System.IO.Path.GetFullPath(dest_path));
                Console.WriteLine("******************************************************");
                return dest_path;

            }
            catch (Exception e)
            {
                Console.WriteLine("File Not Found:{0}",e);
                return "File_Not_Found";
            }
           

        }
        // generates .dll files//
        public bool load_builder_files(List<String> fbuild,String child_folder,List<String> test_names)
        {
            Console.WriteLine();
            Console.WriteLine("****************************");
            Console.WriteLine("-------Build Started------ ");
            Console.WriteLine("****************************");
            Process pr = new Process();
            pr.StartInfo.FileName = "cmd.exe";
            pr.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            String errors="";
            String output="";
            String output_build_log = "";
            String file_build = "";
             foreach (String t_name in test_names)
            {
                
                foreach (String d_file in fbuild)
                {
                    
                    Console.WriteLine();
                    pr.StartInfo.Arguments = "/Ccsc /target:library /out:" +t_name+".dll " +d_file; // provide file to be built
                    pr.StartInfo.WorkingDirectory = child_folder;
                    pr.StartInfo.RedirectStandardError = true;
                    pr.StartInfo.RedirectStandardOutput = true;
                    pr.StartInfo.UseShellExecute = false;
                    pr.Start();
                    pr.WaitForExit();
                    errors = pr.StandardError.ReadToEnd();
                    output = pr.StandardOutput.ReadToEnd();
                    output_build_log = output_build_log + " " + output;
                    file_build = "" + "/Ccsc /target:library " + d_file;
                    Console.WriteLine("Build Command : {0} ",file_build);
                    file_build = "";
                }
                using (StreamWriter wr = new StreamWriter(@"../../../Repo_Files/Build_log_"+t_name+".txt"))
                {
                    wr.WriteLine(output_build_log);
                }
            }
            RepoMock rm = new RepoMock();
            String path = Path.GetFullPath(rm.storagePath);
            Console.WriteLine("*********************************************************************");
            Console.WriteLine("----------Build Logs stored in :\"{0}\"---------------",path);
            Console.WriteLine("**********************************************************************");
            if (output_build_log.Contains("error"))
                return false;
            else
            {
                Console.WriteLine("*********************************************************************");
                Console.WriteLine("----------------DLL Files Stored In:{0}---------------------------", System.IO.Path.GetFullPath(child_folder));
                Console.WriteLine("**********************************************************************");
                return true;
            }
        }

       
        static void Main(String[] args)
        {
            Console.Title = "Child:"+args[0].ToString();
            int ini_port = 8095;
            int port = ini_port + Int32.Parse(args[0]);
            proc_num = proc_num + Int32.Parse(args[0]);
            Console.WriteLine();
            Console.WriteLine("**************************************************************");
            Console.WriteLine("*********** Inside Child: {0} With Port Number :{1}***********",proc_num,port);
            Console.WriteLine("**************************************************************");
            // send msg from child process to mother builder for connection//
            comm = new Comm("http://localhost", port);
            Thread child_thread = null;
            child_thread = new Thread(listen_child_thread);
            child_thread.Start();
            str_port = port;
            CommMessage send_msg = new CommMessage(CommMessage.MessageType.request);
            send_msg.command = "from child:"+ Int32.Parse(args[0]);
            send_msg.author = "Salim Zhulkhrni";
            send_msg.to ="http://localhost:8091/IPluggableComm";
            send_msg.from ="http://localhost:"+port+"/IMessagePassingComm";
            send_msg.msg_body = "child";
            send_msg.port_number = port;
            comm.postMessage(send_msg);
            Console.WriteLine();
            Console.WriteLine("**************************************************************");
            Console.WriteLine(" Sending Message From Child:{0} To Mother Builder",proc_num);
            Console.WriteLine("**************************************************************");
            send_msg.show();
            Console.WriteLine();
            

        }
        //<child thread starts listening>//
         static void listen_child_thread()
        {
           
            Console.WriteLine();
            Console.WriteLine("**************************************************************");
            Console.WriteLine("************Child :{0} Listening*******************", proc_num);
            Console.WriteLine("**************************************************************");
            if (comm != null)
            {
                while (true)
                {
                    CommMessage rcv_msg = new CommMessage(CommMessage.MessageType.request);
                    rcv_msg = comm.getMessage();
                    if (rcv_msg.port_number !=0 && rcv_msg.port_number==8091 && rcv_msg.msg_body.Contains("to_child"))
                    {
                        Console.WriteLine();
                        Console.WriteLine("*******************************************************************************");
                        Console.WriteLine(" ********Child:{0} At Port Number: {1} Received Message From Mother Builder****", proc_num,str_port);
                        Console.WriteLine("*********************************************************************************");
                        rcv_msg.show();
                        Build_Server bs = new Build_Server();
                        bool flag=bs.receive_string_from_client(rcv_msg.msg_body.Replace("to_child",""),proc_num,out string child_folder_path); //build the files in each child process
                        CommMessage send_msg = new CommMessage(CommMessage.MessageType.request);
                        send_msg.command = "from child:" + proc_num;
                        send_msg.author = "Salim Zhulkhrni";
                        send_msg.to = "http://localhost:8091/IPluggableComm";
                        send_msg.from = "http://localhost:" + str_port + "/IMessagePassingComm";
                        send_msg.msg_body = "childreply"+System.IO.Path.GetFullPath(child_folder_path);
                        send_msg.port_number = str_port;
                        comm.postMessage(send_msg); // child process sends message back to mother builder to indicate its availablity
                        Console.WriteLine();
                        Console.WriteLine("*******************************************************************************");
                        Console.WriteLine("***************Child: {0} Is Ready For Next Request****************************",proc_num);
                        Console.WriteLine("*******************************************************************************");
                        // to_test_harness
                        if (flag == true)
                        {
                            CommMessage send_test_msg = new CommMessage(CommMessage.MessageType.request);
                            send_test_msg.command = "from child:" + proc_num;
                            send_test_msg.author = "Salim Zhulkhrni";
                            send_test_msg.to = "http://localhost:8010/IPluggableComm";
                            send_test_msg.from = "http://localhost:" + str_port + "/IMessagePassingComm";
                            send_test_msg.msg_body = "test" + System.IO.Path.GetFullPath(child_folder_path);
                            send_test_msg.port_number = str_port;
                            comm.postMessage(send_test_msg); // child process sends message to test harness with the folder path
                            Console.WriteLine();
                            Console.WriteLine("*******************************************************************************");
                            Console.WriteLine("***************Child: {0} Has Requested For Testing****************************", proc_num);
                            Console.WriteLine("*******************************************************************************");
                        }
                    }
                }
            }
             
        }


    }
    // test_stub//
    /*
#if (BS_Demo)
    class Build_Server_Demo
    {
        static void Main(String[] args )
        {
            Console.WriteLine();
            Console.WriteLine("*********** Demonstrating features of Build Server************");
            TestElement te = new TestElement();
            te.testName = "test1";
            te.addDriver("td1.cs");
            te.addCode("tc1.cs");
            te.addCode("tc2.cs");
            TestRequest tr = new TestRequest();
            tr.author = "Salim Zhulkhrni";
            tr.tests.Add(te);
            String xml = tr.ToXml();
            Build_Server b = new Build_Server();
            //b.receive_string_from_client(xml); // testing receive_String_from_client function
            List<String> temp_list = new List<String>();
            temp_list.Add("td1.cs");
            temp_list.Add("tc1.cs");
            temp_list.Add("tc2.cs");
            b.copy_to_child_storage(temp_list); // testing the function - copy_to_build_storage
            List<String> temp_files_to_load = new List<String>();
            String temp_string = "" + "td1.cs tc1.cs tc2.cs ";          
            //Console.Write(temp_str);
            temp_files_to_load.Add(temp_string);
            b.load_builder_files(temp_files_to_load);  // testing the function - to load the builder files
            b.call_test_harness(); // tesing the function - call test harness           
            

        }
    }
#endif
*/
}

