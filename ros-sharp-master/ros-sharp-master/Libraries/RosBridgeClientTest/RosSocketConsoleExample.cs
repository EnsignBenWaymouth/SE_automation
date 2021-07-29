/*
© Siemens AG, 2017-2019
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
// Solid edge

using SolidEdgeCommunity.Extensions; // https://github.com/SolidEdgeCommunity/SolidEdge.Community/wiki/Using-Extension-Methods

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using Simplify.Windows;
using Simplify.Windows.Forms.Controls;
using Microsoft.WindowsAPICodePack.Controls;


// ROS includes
using RosSharp.RosBridgeClient;
using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;
using rosapi = RosSharp.RosBridgeClient.MessageTypes.Rosapi;
using sensor_msgs = RosSharp.RosBridgeClient.MessageTypes.Sensor;
using tf2 = RosSharp.RosBridgeClient.MessageTypes.Tf2;


// commands on ROS system:
// launch before starting:
// roslaunch rosbridge_server rosbridge_websocket.launch
// rostopic echo /publication_test
// rostopic pub /subscription_test std_msgs/String "subscription test message data"

// launch after starting:
// rosservice call /service_response_test



namespace RosSharp.RosBridgeClientTest
{

    public class RosSocketConsole
    {
        static double[] pose_arr = new double[6];
        static int counter = 0;
        static double sequence = -1;
        //static readonly string uri = "ws://192.168.239.134:9090";
        //static string uri = "ws://172.16.133.130:9090";s
        static string websocket_address = "";
        static readonly string IP_textfile_name = "IP ADDRESS.txt";
        static readonly string Locations_textfile_name = "UR ROBOT LOCATIONS.TXT";
        static string IP = "";
        static DateTime start_timer = new DateTime();
        static DateTime start_timer_ros_tf = new DateTime();
        static int tf_counter = 0;
        

        ///////                               ||    C1   ||    C2    ||     C3   ||    C4    ||
        static double[] arr0 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] arr1 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] arr2 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] arr3 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] arr4 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] arr5 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] temp_arr = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };

        static List<System.Numerics.Matrix4x4> ros_tfs = new List<System.Numerics.Matrix4x4>();
        static System.Numerics.Matrix4x4 m1 = new System.Numerics.Matrix4x4();
        static System.Numerics.Matrix4x4 m2 = new System.Numerics.Matrix4x4();
        static System.Numerics.Matrix4x4 m3 = new System.Numerics.Matrix4x4();
        static System.Numerics.Matrix4x4 m4 = new System.Numerics.Matrix4x4();
        static System.Numerics.Matrix4x4 m5 = new System.Numerics.Matrix4x4();
        static System.Numerics.Matrix4x4 m6 = new System.Numerics.Matrix4x4();

        static System.Numerics.Matrix4x4 base_to_shoulder = new System.Numerics.Matrix4x4();
        static System.Numerics.Matrix4x4 base_to_upperArm = new System.Numerics.Matrix4x4();
        static System.Numerics.Matrix4x4 base_to_forearm = new System.Numerics.Matrix4x4();
        static System.Numerics.Matrix4x4 base_to_wrist1 = new System.Numerics.Matrix4x4();
        static System.Numerics.Matrix4x4 base_to_wrist2 = new System.Numerics.Matrix4x4();
        static System.Numerics.Matrix4x4 base_to_wrist3 = new System.Numerics.Matrix4x4();


        public static void Tester()
        {
            Console.WriteLine("Trying");
            SolidEdgeFramework.Application application = null;
            Console.WriteLine("Setting app");
            try
            {
                Console.WriteLine("Registering");
                
                // Register with OLE to handle concurrency issues on the current thread.
                SolidEdgeCommunity.OleMessageFilter.Register();
                Console.WriteLine("Registered");

                List<SolidEdgeAssembly.AssemblyDocument> documents = new List<SolidEdgeAssembly.AssemblyDocument>();

                // Connect to or start Solid Edge.
                application = SolidEdgeCommunity.SolidEdgeUtils.Connect(true, true);
                Console.WriteLine("Opening program");


                // Get a reference to the active assembly document.
                string f = null;              
                bool ask_user_for_file_location = true;
                // Read IP
                if (ask_user_for_file_location)
                {
                    string current_path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
                    string textfile_path = current_path + "\\" + Locations_textfile_name;
                    Console.WriteLine("Reading IP from: " + textfile_path);

                    string[] all_file_lines = System.IO.File.ReadAllLines(textfile_path);
                    Console.WriteLine("Choose a UR robot file to open:");
                    int i = 0;
                    foreach (string line in all_file_lines)
                    {
                        Console.WriteLine(i++ + "  ----  " + line);
                    }

                                        
                    int n = -1;
                    while (n < 0)
                    {
                        Console.Write("\nType the corresponding number and press enter: ");
                        string user_input = Console.ReadLine();
                        try
                        {
                            n = Int32.Parse(user_input);
                        }
                        catch(FormatException e)
                        {
                            n = -1;
                            Console.WriteLine("You didn't enter a number...");                            
                        }
                        if((n > 0) && (n < all_file_lines.Length))
                        {
                            f = all_file_lines[n];
                        }
                        else
                        {
                            Console.WriteLine("'" + n + "' is out of range");
                        }
                    }
                    //Console.WriteLine("Number you entered: " + n);
                }
                else
                {
                    //f = "C:\\Users\\benjaminw\\Documents\\UR3e_test.asm";
                    f = "\\\\elsedge\\engineering\\Drawings\\Filling Hall 1\\BF03\\UET Cartoner Automation Ass'y\\UR3e, Vacuum, UET Automation.asm";
                    //f = "\\\\elsedge\\engineering\\Drawings\\UR3e\\Modified Orientation\\UR3e.asm";
                    //f = "\\\\elsedge\\engineering\\Drawings\\UR5e\\Modified Orientation\\UR5e.asm";
                    //f = "\\\\elsedge\\engineering\\Drawings\\UR10e\\Modified Orientation\\UR10e.asm";
                    //f = "\\\\elsedge\\engineering\\Drawings\\UR16e\\Modified Orientation\\UR16e.asm";
                    //f = "C:\\Users\\benwa\\Documents\\1 Projects\\Part Library\\ur_robot\\Asm3.asm";
                }

                //Console.WriteLine("Shown");
                bool ask_for_user_input = false;
                if (ask_for_user_input)
                {
                    string title = "text box";
                    string promptText = "File enter name";
                    string value = "*** Network location ***";

                    Form form = new Form();
                    Label label = new Label();
                    TextBox textBox = new TextBox();
                    Button buttonOk = new Button();
                    Button buttonCancel = new Button();

                    form.Text = title;
                    label.Text = promptText;
                    textBox.Text = value;

                    buttonOk.Text = "OK";
                    buttonCancel.Text = "Cancel";
                    buttonOk.DialogResult = DialogResult.OK;
                    buttonCancel.DialogResult = DialogResult.Cancel;

                    label.SetBounds(9, 20, 372, 13);
                    textBox.SetBounds(12, 36, 372, 20);
                    buttonOk.SetBounds(228, 72, 75, 23);
                    buttonCancel.SetBounds(309, 72, 75, 23);

                    label.AutoSize = true;
                    textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
                    buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
                    buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

                    form.ClientSize = new Size(396, 107);
                    form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
                    form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.MinimizeBox = false;
                    form.MaximizeBox = false;
                    form.AcceptButton = buttonOk;
                    form.CancelButton = buttonCancel;

                    DialogResult dialogResult = form.ShowDialog();
                    value = textBox.Text;

                    Console.WriteLine(value);
                    //// 
                    Console.WriteLine("Value: " + value);
                    f = value;
                }
                

                var document_by_name = application.Documents.OpenInBackground<SolidEdgeAssembly.AssemblyDocument>(f);
                //var document_by_name = application.GetActiveDocument<SolidEdgeAssembly.AssemblyDocument>(false);

                //Console.WriteLine("Opening by active: " + document.DisplayName);
                Console.WriteLine("Opening by filename: " + document_by_name.DisplayName);
                Console.WriteLine("Filename: " + document_by_name.FullName);

                var d = document_by_name;

                if (d != null)
                {
                    
                    // Strings
                    string link1_str = "Link1";
                    string link2_str = "Link2";
                    string link3_str = "Link3";
                    string link4_str = "Link4";
                    string link5_str = "Link5";
                    string link6_str = "Link6";
                    string link_EE_tool_str = "EE_tool";

                    SolidEdgeAssembly.Occurrence link1 = null;
                    SolidEdgeAssembly.Occurrence link2 = null;
                    SolidEdgeAssembly.Occurrence link3 = null;
                    SolidEdgeAssembly.Occurrence link4 = null;
                    SolidEdgeAssembly.Occurrence link5 = null;
                    SolidEdgeAssembly.Occurrence link6 = null;
                    SolidEdgeAssembly.Occurrence link_EE_tool = null;
                    List<SolidEdgeAssembly.Occurrence> link_list = new List<SolidEdgeAssembly.Occurrence>();
                    link_list.Add(link1);
                    link_list.Add(link2);
                    link_list.Add(link3);
                    link_list.Add(link4);
                    link_list.Add(link5);
                    link_list.Add(link6);
                    link_list.Add(link_EE_tool);

                    var occurrences_1 = d.Occurrences;

                    //// Create link references
                    foreach (var occurrence in occurrences_1.OfType<SolidEdgeAssembly.Occurrence>())
                    {
                        string name = occurrence.Name;
                        bool l1 = name.Contains(link1_str);
                        if (l1)
                        {
                            link_list[0] = occurrence;
                            continue;
                        }
                        bool l2 = name.Contains(link2_str);
                        if (l2)
                        {
                            link_list[1] = occurrence;
                            continue;
                        }
                        bool l3 = name.Contains(link3_str);
                        if (l3)
                        {
                            link_list[2] = occurrence;
                            continue;
                        }
                        bool l4 = name.Contains(link4_str);
                        if (l4)
                        {
                            link_list[3] = occurrence;
                            continue;
                        }
                        bool l5 = name.Contains(link5_str);
                        if (l5)
                        {
                            link_list[4] = occurrence;
                            continue;
                        }
                        bool l6 = name.Contains(link6_str);
                        if (l6)
                        {
                            link_list[5] = occurrence;
                            continue;
                        }
                        bool l7 = name.Contains(link_EE_tool_str);
                        if(l7)
                        {
                            link_list[6] = occurrence;
                            continue;
                        }
                    }

                    // Wait for ROS
                    while (sequence == -1)
                    {
                        //Console.WriteLine("Waiting");
                        continue;
                    }

                    // Timer
                    DateTime start = new DateTime();
                    double time_sum = 0;
                    int time_counter = 0;
                 
                    while (true)
                    {
                        start = DateTime.Now;

                        link_list[0].PutMatrix(arr0, true);
                        link_list[1].PutMatrix(arr1, true);
                        link_list[2].PutMatrix(arr2, true);
                        link_list[3].PutMatrix(arr3, true);
                        link_list[4].PutMatrix(arr4, true);
                        link_list[5].PutMatrix(arr5, true);
                        if(link_list[6] != null)
                        {
                            link_list[6].PutMatrix(arr5, true);  // EE_link base coordinate at Tool attachment
                        }
                        else
                        {
                            continue;
                            Console.WriteLine("No 'tool' found");
                        }

                        time_sum += (DateTime.Now - start).TotalMilliseconds;
                        time_counter++;
                        double avg = 1000;
                        if (time_counter == 20)
                        {
                            avg = Math.Round(1000 / (time_sum / time_counter));
                            Console.WriteLine("\nUR Update FPS: {0}\n", avg);
                            time_counter = 0;
                            time_sum = 0;
                        }
                        if (avg > 50)
                        {
                            //Console.WriteLine("Refreshing");
                            application.StartCommand(SolidEdgeConstants.AssemblyCommandConstants.AssemblyViewRefreshWindow);
                            //continue;
                        }
                    }
                }
                else
                {
                    throw new System.Exception("No active document.");
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                SolidEdgeCommunity.OleMessageFilter.Unregister();
            }
        }


        /// <summary>
        /// Main function with thread
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            // Read IP
            string current_path = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            string textfile_path = current_path + "\\" + IP_textfile_name;
            Console.WriteLine("Reading IP from: " + textfile_path);

            IP = System.IO.File.ReadAllText(textfile_path);
            Console.WriteLine("Read IP: " + IP);
            websocket_address = "ws://" + IP + ":9090";
            Console.WriteLine("Websocket Address: " + websocket_address);

            //RosSocket rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketSharpProtocol(uri));
            RosSocket rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketNetProtocol(websocket_address));

            // Publication:
            std_msgs.String message = new std_msgs.String
            {
                data = "publication test masdasdessage data"
            };

            Console.WriteLine("Connected");

            ros_tfs.Add(m1);
            ros_tfs.Add(m2);
            ros_tfs.Add(m3);
            ros_tfs.Add(m4);
            ros_tfs.Add(m5);
            ros_tfs.Add(m6);

            // Subscribers
            //string pose_array_id = rosSocket.Subscribe<sensor_msgs.JointState>("/joint_states", PoseArrayCallback);
            string tf_id = rosSocket.Subscribe<tf2.TFMessage>("/tf", tfCallback);
            //string geometry_id = rosSocket.Subscribe<geometry.Transform>("")

            // Service Call:
            //rosSocket.CallService<rosapi.GetParamRequest, rosapi.GetParamResponse>("/rosapi/get_param", ServiceCallHandler, new rosapi.GetParamRequest("/rosdistro", "default"));

            // Service Response:
            //string service_id = rosSocket.AdvertiseService<std_srvs.TriggerRequest, std_srvs.TriggerResponse>("/service_response_test", ServiceResponseHandler);

            start_timer_ros_tf = DateTime.Now;

            // Connect to solid edge
            Thread thread = new Thread(new ThreadStart(Tester));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            //Console.WriteLine("\nPress ENTER to exit the program...\n");
            //ConsoleKeyInfo keyInfo = Console.ReadKey();
            //while (keyInfo.Key != ConsoleKey.Enter)
            //{
                //keyInfo = Console.ReadKey();
            //}
            while(true)
            {
                continue;
            }

            // Disconnect ROS
            //Console.WriteLine("Press any key to close...");
            //Console.ReadKey(true);
            //rosSocket.Unsubscribe(pose_array_id);
            rosSocket.Unsubscribe(tf_id);
            //rosSocket.UnadvertiseService(service_id);
            rosSocket.Close();
            thread.Abort();
        }
        private static void SubscriptionHandler(std_msgs.String message)
        {
            Console.WriteLine((message).data);
        }

        private static void PrintMatrix4x4(System.Numerics.Matrix4x4 matrix)
        {
            Console.WriteLine(Math.Round(matrix.M11, 2) + " " + Math.Round(matrix.M21, 2) + " " + Math.Round(matrix.M31, 2) + " " + Math.Round(matrix.M41, 2));
            Console.WriteLine(Math.Round(matrix.M12, 2) + " " + Math.Round(matrix.M22, 2) + " " + Math.Round(matrix.M32, 2) + " " + Math.Round(matrix.M42, 2));
            Console.WriteLine(Math.Round(matrix.M13, 2) + " " + Math.Round(matrix.M23, 2) + " " + Math.Round(matrix.M33, 2) + " " + Math.Round(matrix.M43, 2));
            Console.WriteLine(Math.Round(matrix.M14, 2) + " " + Math.Round(matrix.M24, 2) + " " + Math.Round(matrix.M34, 2) + " " + Math.Round(matrix.M44, 2));
            Console.WriteLine();
        }

        private static void tfCallback(tf2.TFMessage msg)
        {
            sequence = msg.transforms[0].header.seq;
            if (!(msg.transforms[0].child_frame_id == "tool0_controller"))
            {
                //Console.WriteLine(msg.transforms[0].transform.rotation.w);
                var tfs = msg.transforms;
                int i = 0; // Set start point at Shoulder in Base_link

                foreach (var t in tfs)
                {
                    //Console.WriteLine(i + ":   " + "Showing: " + t.child_frame_id + "   In the " + t.header.frame_id + "  Frame");

                    System.Numerics.Vector3 trans = new System.Numerics.Vector3((float)t.transform.translation.x, (float)t.transform.translation.y, (float)t.transform.translation.z);
                    System.Numerics.Quaternion quat = new System.Numerics.Quaternion((float)t.transform.rotation.x, (float)t.transform.rotation.y, (float)t.transform.rotation.z, (float)t.transform.rotation.w);
                    System.Numerics.Matrix4x4 mat = System.Numerics.Matrix4x4.CreateFromQuaternion(quat);
                    mat.Translation = trans;
                    //PrintMatrix4x4(mat);
                    //Console.WriteLine();

                    ros_tfs[i] = mat;
                    i++;
                }
            }

            // calculate transformations matrices
            base_to_shoulder = ros_tfs[2];
            base_to_upperArm = ros_tfs[1] * base_to_shoulder;
            base_to_forearm = ros_tfs[0] * base_to_upperArm;
            base_to_wrist1 = ros_tfs[3] * base_to_forearm;
            base_to_wrist2 = ros_tfs[4] * base_to_wrist1;
            base_to_wrist3 = ros_tfs[5] * base_to_wrist2;

            ///////// 0 ///////
            /// C1
            arr0[0] = base_to_shoulder.M11;
            arr0[1] = base_to_shoulder.M12;
            arr0[2] = base_to_shoulder.M13;
            /// C2
            arr0[4] = base_to_shoulder.M21;
            arr0[5] = base_to_shoulder.M22;
            arr0[6] = base_to_shoulder.M23;
            /// C3
            arr0[8] = base_to_shoulder.M31;
            arr0[9] = base_to_shoulder.M32;
            arr0[10] = base_to_shoulder.M33;
            /// C4
            arr0[12] = base_to_shoulder.M41;
            arr0[13] = base_to_shoulder.M42;
            arr0[14] = base_to_shoulder.M43;
            ///////// 1 ///////
            /// C1
            arr1[0] = base_to_upperArm.M11;
            arr1[1] = base_to_upperArm.M12;
            arr1[2] = base_to_upperArm.M13;
            /// C2
            arr1[4] = base_to_upperArm.M21;
            arr1[5] = base_to_upperArm.M22;
            arr1[6] = base_to_upperArm.M23;
            /// C3
            arr1[8] = base_to_upperArm.M31;
            arr1[9] = base_to_upperArm.M32;
            arr1[10] = base_to_upperArm.M33;
            /// C4
            arr1[12] = base_to_upperArm.M41;
            arr1[13] = base_to_upperArm.M42;
            arr1[14] = base_to_upperArm.M43;
            ///////// 2 ///////
            /// C1
            arr2[0] = base_to_forearm.M11;
            arr2[1] = base_to_forearm.M12;
            arr2[2] = base_to_forearm.M13;
            /// C2
            arr2[4] = base_to_forearm.M21;
            arr2[5] = base_to_forearm.M22;
            arr2[6] = base_to_forearm.M23;
            /// C3
            arr2[8] = base_to_forearm.M31;
            arr2[9] = base_to_forearm.M32;
            arr2[10] = base_to_forearm.M33;
            /// C4
            arr2[12] = base_to_forearm.M41;
            arr2[13] = base_to_forearm.M42;
            arr2[14] = base_to_forearm.M43;
            ///////// 3 ///////
            /// C1
            arr3[0] = base_to_wrist1.M11;
            arr3[1] = base_to_wrist1.M12;
            arr3[2] = base_to_wrist1.M13;
            /// C2
            arr3[4] = base_to_wrist1.M21;
            arr3[5] = base_to_wrist1.M22;
            arr3[6] = base_to_wrist1.M23;
            /// C3
            arr3[8] = base_to_wrist1.M31;
            arr3[9] = base_to_wrist1.M32;
            arr3[10] = base_to_wrist1.M33;
            /// C4
            arr3[12] = base_to_wrist1.M41;
            arr3[13] = base_to_wrist1.M42;
            arr3[14] = base_to_wrist1.M43;
            ///////// 4 ///////
            /// C1
            arr4[0] = base_to_wrist2.M11;
            arr4[1] = base_to_wrist2.M12;
            arr4[2] = base_to_wrist2.M13;
            /// C2
            arr4[4] = base_to_wrist2.M21;
            arr4[5] = base_to_wrist2.M22;
            arr4[6] = base_to_wrist2.M23;
            /// C3
            arr4[8] = base_to_wrist2.M31;
            arr4[9] = base_to_wrist2.M32;
            arr4[10] = base_to_wrist2.M33;
            /// C4
            arr4[12] = base_to_wrist2.M41;
            arr4[13] = base_to_wrist2.M42;
            arr4[14] = base_to_wrist2.M43;
            ///////// 5 ///////
            /// C1
            arr5[0] = base_to_wrist3.M11;
            arr5[1] = base_to_wrist3.M12;
            arr5[2] = base_to_wrist3.M13;
            /// C2
            arr5[4] = base_to_wrist3.M21;
            arr5[5] = base_to_wrist3.M22;
            arr5[6] = base_to_wrist3.M23;
            /// C3
            arr5[8] = base_to_wrist3.M31;
            arr5[9] = base_to_wrist3.M32;
            arr5[10] = base_to_wrist3.M33;
            /// C4
            arr5[12] = base_to_wrist3.M41;
            arr5[13] = base_to_wrist3.M42;
            arr5[14] = base_to_wrist3.M43;


            //tf_counter++;
            //double thresh = 1000;
            //if (tf_counter == thresh)
            //{
            //    //Console.WriteLine("Ros code: " + arr5[14]);
            //    double time = ((DateTime.Now - start_timer_ros_tf).TotalMilliseconds)/ thresh;
            //    time = time / 1000; // Convert to seconds
            //    Console.WriteLine("ROS TF FPS: " + Math.Round(1/time,0) );
            //    tf_counter = 0;
            //    start_timer_ros_tf = DateTime.Now;
            //}

        }

        private static void PoseArrayCallback(sensor_msgs.JointState msg)
        {
            if (counter == 0)
            {
                start_timer = DateTime.Now;
            }
            counter++;

            sequence = msg.header.seq;
            double[] p = msg.position;
            for (int i = 0; i < p.Length; i++)
            {
                double deg = p[i] * 180 / Math.PI;
                deg = Math.Round(deg, 3);
                pose_arr[i] = p[i];
                //Console.WriteLine("{0}:  {1}", msg.name[i], deg);
                //Console.WriteLine((msg).position[i]);
            }
        }
        private static void ServiceCallHandler(rosapi.GetParamResponse message)
        {
            Console.WriteLine("ROS distro: " + message.value);
        }

        //private static bool ServiceResponseHandler(std_srvs.TriggerRequest arguments, out std_srvs.TriggerResponse result)
        //{
        //    result = new std_srvs.TriggerResponse(true, "service response message");
        //    return true;
        //}
    }
}

//arr0 = new double[16] { base_to_shoulder.M11, base_to_shoulder.M12, base_to_shoulder.M13, base_to_shoulder.M14,
//                        base_to_shoulder.M21, base_to_shoulder.M22, base_to_shoulder.M22, base_to_shoulder.M22,
//                        base_to_shoulder.M31, base_to_shoulder.M32, base_to_shoulder.M33, base_to_shoulder.M34,
//                        base_to_shoulder.M41, base_to_shoulder.M42, base_to_shoulder.M43, base_to_shoulder.M44
//};
//arr1 = new double[16] { base_to_upperArm.M11, base_to_upperArm.M12, base_to_upperArm.M13, base_to_upperArm.M14,
//                        base_to_upperArm.M21, base_to_upperArm.M22, base_to_upperArm.M22, base_to_upperArm.M22,
//                        base_to_upperArm.M31, base_to_upperArm.M32, base_to_upperArm.M33, base_to_upperArm.M34,
//                        base_to_upperArm.M41, base_to_upperArm.M42, base_to_upperArm.M43, base_to_upperArm.M44
//};
//arr2 = new double[16] { base_to_forearm.M11, base_to_forearm.M12, base_to_forearm.M13, base_to_forearm.M14,
//                        base_to_forearm.M21, base_to_forearm.M22, base_to_forearm.M22, base_to_forearm.M22,
//                        base_to_forearm.M31, base_to_forearm.M32, base_to_forearm.M33, base_to_forearm.M34,
//                        base_to_forearm.M41, base_to_forearm.M42, base_to_forearm.M43, base_to_forearm.M44
//};
//arr3 = new double[16] { base_to_wrist1.M11, base_to_wrist1.M12, base_to_wrist1.M13, base_to_wrist1.M14,
//                        base_to_wrist1.M21, base_to_wrist1.M22, base_to_wrist1.M22, base_to_wrist1.M22,
//                        base_to_wrist1.M31, base_to_wrist1.M32, base_to_wrist1.M33, base_to_wrist1.M34,
//                        base_to_wrist1.M41, base_to_wrist1.M42, base_to_wrist1.M43, base_to_wrist1.M44
//};
//arr4 = new double[16] { base_to_wrist2.M11, base_to_wrist2.M12, base_to_wrist2.M13, base_to_wrist2.M14,
//                        base_to_wrist2.M21, base_to_wrist2.M22, base_to_wrist2.M22, base_to_wrist2.M22,
//                        base_to_wrist2.M31, base_to_wrist2.M32, base_to_wrist2.M33, base_to_wrist2.M34,
//                        base_to_wrist2.M41, base_to_wrist2.M42, base_to_wrist2.M43, base_to_wrist2.M44
//};
//arr5 = new double[16] { base_to_wrist3.M11, base_to_wrist3.M12, base_to_wrist3.M13, base_to_wrist3.M14,
//                        base_to_wrist3.M21, base_to_wrist3.M22, base_to_wrist3.M22, base_to_wrist3.M22,
//                        base_to_wrist3.M31, base_to_wrist3.M32, base_to_wrist3.M33, base_to_wrist3.M34,
//                        base_to_wrist3.M41, base_to_wrist3.M42, base_to_wrist3.M43, base_to_wrist3.M44
//};