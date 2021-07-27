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

// ROS includes
using RosSharp.RosBridgeClient;
using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;
using std_srvs = RosSharp.RosBridgeClient.MessageTypes.Std;
using rosapi = RosSharp.RosBridgeClient.MessageTypes.Rosapi;
using sensor_msgs = RosSharp.RosBridgeClient.MessageTypes.Sensor;
using tf2 = RosSharp.RosBridgeClient.MessageTypes.Tf2;
using lookupGoal = RosSharp.RosBridgeClient.MessageTypes.Tf2.LookupTransformGoal;
using geometry = RosSharp.RosBridgeClient.MessageTypes.Geometry;


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
        static readonly string uri = "ws://192.168.239.134:9090";
        static DateTime start_timer = new DateTime();
        static DateTime start_timer_ros_tf = new DateTime();
        static int tf_counter = 0;
        static double[] p1 = new double[] { 0, 0, 0, 0, 0, 0, 0 };
        static double[] p2 = new double[] { 0, 0, 0, 0, 0, 0, 0 };
        static double[] p3 = new double[] { 0, 0, 0, 0, 0, 0, 0 };
        static double[] p4 = new double[] { 0, 0, 0, 0, 0, 0, 0 };
        static double[] p5 = new double[] { 0, 0, 0, 0, 0, 0, 0 };
        static double[] p6 = new double[] { 0, 0, 0, 0, 0, 0, 0 };

        ///////                               ||    C1   ||    C2    ||     C3   ||    C4    ||
        static double[] arr0 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] arr1 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] arr2 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] arr3 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] arr4 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] arr5 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] arr6 = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        static double[] temp_arr = new double[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };


        static List<double[]> ros_transforms = new List<double[]>();

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
                //var document = application.GetActiveDocument<SolidEdgeAssembly.AssemblyDocument>(false);
                var document_by_name = application.GetActiveDocument<SolidEdgeAssembly.AssemblyDocument>(false);
                //Console.WriteLine(document.FullName);
                //string f = "C:\\Users\\benjaminw\\Documents\\UR3e_test.asm";
                string f = "C:\\Users\\benwa\\Documents\\1 Projects\\Part Library\\ur_robot\\Asm3.asm";
                //string f = "\\elsedge\\engineering\\Drawings\\Filling Hall 1\\BF03\\UET Cartoner Automation Ass'y\\UR3e_UET Cartoner_Vacuum_6_Pocket.asm";
                //var document_by_name = application.Documents.OpenInBackground<SolidEdgeAssembly.AssemblyDocument>(f);
                //Console.WriteLine("Opening by active: " + document.DisplayName);
                Console.WriteLine("Opening by filename: " + document_by_name.DisplayName);

                //if (document != null)
                //{
                    //documents.Add(document);
                //}
                //if (document_by_name != null)
                //{
                //    documents.Add(document_by_name);
                //}

                //for (int i = 1; i < application.Documents.Count + 1; i ++)
                //{
                //    Console.WriteLine(application.Documents.Item(i));
                //}

                //var d = document;
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

                    SolidEdgeAssembly.Occurrence link1 = null;
                    SolidEdgeAssembly.Occurrence link2 = null;
                    SolidEdgeAssembly.Occurrence link3 = null;
                    SolidEdgeAssembly.Occurrence link4 = null;
                    SolidEdgeAssembly.Occurrence link5 = null;
                    SolidEdgeAssembly.Occurrence link6 = null;
                    List<SolidEdgeAssembly.Occurrence> link_list = new List<SolidEdgeAssembly.Occurrence>();
                    link_list.Add(link1);
                    link_list.Add(link2);
                    link_list.Add(link3);
                    link_list.Add(link4);
                    link_list.Add(link5);
                    link_list.Add(link6);

                    List<SolidEdgeAssembly.AngularRelation3d> angularRelations = new List<SolidEdgeAssembly.AngularRelation3d>();
                    SolidEdgeAssembly.AngularRelation3d angle_1 = null;
                    SolidEdgeAssembly.AngularRelation3d angle_2 = null;
                    SolidEdgeAssembly.AngularRelation3d angle_3 = null;
                    SolidEdgeAssembly.AngularRelation3d angle_4 = null;
                    SolidEdgeAssembly.AngularRelation3d angle_5 = null;
                    SolidEdgeAssembly.AngularRelation3d angle_6 = null;
                    angularRelations.Add(angle_1);
                    angularRelations.Add(angle_2);
                    angularRelations.Add(angle_3);
                    angularRelations.Add(angle_4);
                    angularRelations.Add(angle_5);
                    angularRelations.Add(angle_6);
                    var occurrences_1 = d.Occurrences;

                    //// Create link references
                    foreach (var occurrence in occurrences_1.OfType<SolidEdgeAssembly.Occurrence>())
                    {
                        if (occurrence.Name.Contains("Link1") || occurrence.Name.Contains("Link3") || occurrence.Name.Contains("Link5"))
                        {
                            var relations3d = (SolidEdgeAssembly.Relations3d)occurrence.Relations3d;
                            var angleRelations = relations3d.OfType<SolidEdgeAssembly.AngularRelation3d>();
                            foreach (var an in angleRelations)
                            {
                                string name = an.Occurrence1.Name;
                                bool l1 = name.Contains(link1_str);
                                if (l1)
                                {
                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[0]);
                                    //an.Angle = pose_arr[2] + Math.PI / 2;
                                    //pose_buffer[0] = pose_arr[2] + Math.PI / 2;
                                    angularRelations[0] = an;
                                    continue;
                                }
                                bool l2 = name.Contains(link2_str);
                                if (l2)
                                {
                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[1]);
                                    angularRelations[1] = an;
                                    continue;
                                }
                                bool l3 = name.Contains(link3_str);
                                if (l3)
                                {
                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[2]);
                                    angularRelations[2] = an;
                                    continue;
                                }
                                bool l4 = name.Contains(link4_str);
                                if (l4)
                                {
                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[3]);
                                    angularRelations[3] = an;
                                    continue;
                                }
                                bool l5 = name.Contains(link5_str);
                                if (l5)
                                {
                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[4]);
                                    angularRelations[4] = an;
                                    continue;
                                }
                                bool l6 = name.Contains(link6_str);
                                if (l6)
                                {
                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[5]);
                                    angularRelations[5] = an;
                                    continue;
                                }
                            }
                        }
                    }

                    foreach (var occurrence in occurrences_1.OfType<SolidEdgeAssembly.Occurrence>())
                    {
                        string name = occurrence.Name;
                        bool l1 = name.Contains(link1_str);
                        if (l1)
                        {
                            //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[0]);
                            //an.Angle = pose_arr[2] + Math.PI / 2;
                            //pose_buffer[0] = pose_arr[2] + Math.PI / 2;
                            link_list[0] = occurrence;
                            continue;
                        }
                        bool l2 = name.Contains(link2_str);
                        if (l2)
                        {
                            //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[1]);
                            link_list[1] = occurrence;
                            continue;
                        }
                        bool l3 = name.Contains(link3_str);
                        if (l3)
                        {
                            //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[2]);
                            link_list[2] = occurrence;
                            continue;
                        }
                        bool l4 = name.Contains(link4_str);
                        if (l4)
                        {
                            //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[3]);
                            link_list[3] = occurrence;
                            continue;
                        }
                        bool l5 = name.Contains(link5_str);
                        if (l5)
                        {
                            //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[4]);
                            link_list[4] = occurrence;
                            continue;
                        }
                        bool l6 = name.Contains(link6_str);
                        if (l6)
                        {
                            //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[5]);
                            link_list[5] = occurrence;
                            continue;
                        }
                    }


                    // Wait for ROS
                    while (sequence == -1)
                    {
                        //Console.WriteLine("Waiting");
                        int a;
                    }

                    // Timer
                    DateTime start = new DateTime();
                    double time_sum = 0;
                    int time_counter = 0;

                    ////////////////////////////////////// Angle updates
                    // Continuously update joints
                    //while (true)
                    //{
                    //    start = DateTime.Now;
                    //    angularRelations[0].Angle = pose_arr[2] + Math.PI / 2;
                    //    angularRelations[1].Angle = pose_arr[1] + Math.PI;
                    //    angularRelations[2].Angle = -pose_arr[0] - Math.PI / 2;
                    //    angularRelations[3].Angle = pose_arr[3];
                    //    angularRelations[4].Angle = pose_arr[4] - Math.PI / 2;
                    //    angularRelations[5].Angle = pose_arr[5] + Math.PI;

                    //    time_sum += (DateTime.Now - start).TotalMilliseconds;
                    //    time_counter++;
                    //    double avg = 1000;
                    //    if (time_counter == 20)
                    //    {
                    //        avg = Math.Round(1000 / (time_sum / time_counter));
                    //        //Console.WriteLine("\nFPS: {0}\n", avg);
                    //        time_counter = 0;
                    //        time_sum = 0;
                    //    }
                    //    if (avg < 50)
                    //    {
                    //        Console.WriteLine("Refreshing");
                    //        application.StartCommand(SolidEdgeConstants.AssemblyCommandConstants.AssemblyViewRefreshWindow);
                    //    }
                    //}                 
                    double[] p = new double[] { 0, 0, 0, 0, 0, 0 };
                    while (true)
                    {
                        start = DateTime.Now;

                        //foreach (var occurance in link_list)
                        //{
                        //    occurance.PutMatrix(temp_arr, true);
                        //    //occurance.PutTransform(p[0], p[1], p[2], p[3], p[4], p[5]);
                        //    //occurance.GetTransform(out p[0], out p[1], out p[2], out p[3], out p[4], out p[5]);
                        //    //Console.WriteLine(p[0]);
                        //    for (int i = 0; i < p.Length; i++)
                        //    {
                        //        p[i] += 0.01;
                        //        temp_arr[12] += 0.01;
                        //        temp_arr[13] += 0.01;
                        //        temp_arr[14] += 0.01;
                        //    }
                        //}
                        //if (p[0] >= 2)
                        //{
                        //    for (int i = 0; i < p.Length; i++)
                        //    {
                        //        p[i] = 0;
                        //        temp_arr[12] = 0;
                        //        temp_arr[13] = 0;
                        //        temp_arr[14] = 0;
                        //    }
                        //}
                        //Console.WriteLine("Here");
                        //SolidEdgeAssembly
                        //Array arr2 = new double[] {};
                        //Array arr2 = new Array();
                        //link_list[0].GetMatrix(ref arr2);
                        //arr2.
                        //Console.WriteLine("rank: " + arr2.Rank);


                        //double[] arr3 = new double[16] {1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 }; 
                        //Array arr = new double[] { };
                        //arr.Initialize();
                        //Console.WriteLine(arr);
                        //arr.SetValue(base_to_shoulder.M11, 0);

                        //arr.SetValue(base_to_shoulder.M21, 1);
                        //arr.SetValue(base_to_shoulder.M21, 2);
                        //arr.SetValue(base_to_shoulder.M21, 3);
                        //arr.SetValue(base_to_shoulder.M21, 4);
                        //arr.SetValue(base_to_shoulder.M21, 5);
                        //arr.SetValue(base_to_shoulder.M21, 6);
                        //arr.SetValue(base_to_shoulder.M21, 7);
                        //arr.SetValue(base_to_shoulder.M21, 8);
                        //arr.SetValue(base_to_shoulder.M21, 9);
                        //arr.SetValue(base_to_shoulder.M21, 10);
                        //arr.SetValue(base_to_shoulder.M21, 11);
                        //arr.SetValue(base_to_shoulder.M21, 12);
                        //arr.SetValue(base_to_shoulder.M21, 13);
                        //arr.SetValue(base_to_shoulder.M21, 14);
                        //arr.SetValue(base_to_shoulder.M21, 15);

                        link_list[0].PutMatrix(arr0, true);
                        link_list[1].PutMatrix(arr1, true);
                        link_list[2].PutMatrix(arr2, true);
                        link_list[3].PutMatrix(arr3, true);
                        link_list[4].PutMatrix(arr4, true);
                        link_list[5].PutMatrix(arr5, true);

                        //Console.WriteLine(arr5[14]);



                        ////double[,] temp = new double[,] { { 1, 1, 1, 2, 3 } };
                        //double[,] t1 = new double[,] { { base_to_shoulder.M11, base_to_shoulder.M21, base_to_shoulder.M31, base_to_shoulder.M41 },
                        //    { base_to_shoulder.M12, base_to_shoulder.M22, base_to_shoulder.M32, base_to_shoulder.M42},
                        //    { base_to_shoulder.M13, base_to_shoulder.M23, base_to_shoulder.M33, base_to_shoulder.M43},
                        //    { base_to_shoulder.M14, base_to_shoulder.M24, base_to_shoulder.M34, base_to_shoulder.M44}
                        //};
                        //double[,] t2 = new double[,] { { base_to_upperArm.M11, base_to_upperArm.M21, base_to_upperArm.M31, base_to_upperArm.M41 },
                        //    { base_to_upperArm.M12, base_to_upperArm.M22, base_to_upperArm.M32, base_to_upperArm.M42},
                        //    { base_to_upperArm.M13, base_to_upperArm.M23, base_to_upperArm.M33, base_to_upperArm.M43},
                        //    { base_to_upperArm.M14, base_to_upperArm.M24, base_to_upperArm.M34, base_to_upperArm.M41}
                        //};
                        ////Console.WriteLine(t1);
                        //System.Array t11 = t1;
                        //System.Array t22 = t2;
                        //link_list[0].PutMatrix(ref t11, true);

                        ////link_list[1].PutMatrix(ref t22, true);
                        ////link_list[0].PutTransform(base_to_shoulder.Translation.X, base_to_shoulder.Translation.Y, base_to_shoulder.Translation.Z, );


                        time_sum += (DateTime.Now - start).TotalMilliseconds;
                        time_counter++;
                        double avg = 1000;
                        if (time_counter == 20)
                        {
                            avg = Math.Round(1000 / (time_sum / time_counter));
                            Console.WriteLine("\nFPS: {0}\n", avg);
                            time_counter = 0;
                            time_sum = 0;
                        }
                        if (avg > 50)
                        {
                            //Console.WriteLine("Refreshing");
                            //continue;
                            application.StartCommand(SolidEdgeConstants.AssemblyCommandConstants.AssemblyViewRefreshWindow);
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


        public static void Main(string[] args)
        {
            //RosSocket rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketSharpProtocol(uri));
            RosSocket rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketNetProtocol(uri));

            // Publication:
            std_msgs.String message = new std_msgs.String
            {
                data = "publication test masdasdessage data"
            };

            Console.WriteLine("Connected");

            ros_transforms.Add(p1);
            ros_transforms.Add(p2);
            ros_transforms.Add(p3);
            ros_transforms.Add(p4);
            ros_transforms.Add(p5);
            ros_transforms.Add(p6);

            ros_tfs.Add(m1);
            ros_tfs.Add(m2);
            ros_tfs.Add(m3);
            ros_tfs.Add(m4);
            ros_tfs.Add(m5);
            ros_tfs.Add(m6);

            // Subscribers
            string pose_array_id = rosSocket.Subscribe<sensor_msgs.JointState>("/joint_states", PoseArrayCallback);
            string tf_id = rosSocket.Subscribe<tf2.TFMessage>("/tf", tfCallback);
            //string geometry_id = rosSocket.Subscribe<geometry.Transform>("")

            // Service Call:
            rosSocket.CallService<rosapi.GetParamRequest, rosapi.GetParamResponse>("/rosapi/get_param", ServiceCallHandler, new rosapi.GetParamRequest("/rosdistro", "default"));

            // Service Response:
            //string service_id = rosSocket.AdvertiseService<std_srvs.TriggerRequest, std_srvs.TriggerResponse>("/service_response_test", ServiceResponseHandler);

            start_timer_ros_tf = DateTime.Now;

            // Connect to solid edge
            Thread thread = new Thread(new ThreadStart(Tester));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            Console.WriteLine("\nPress ENTER to exit the program...");
            ConsoleKeyInfo keyInfo = Console.ReadKey();
            while (keyInfo.Key != ConsoleKey.Enter)
            {
                keyInfo = Console.ReadKey();
            }

            // Disconnect ROS
            Console.WriteLine("Press any key to close...");
            Console.ReadKey(true);
            rosSocket.Unsubscribe(pose_array_id);
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
            if (!(msg.transforms[0].child_frame_id == "tool0_controller"))
            {
                //Console.WriteLine(msg.transforms[0].transform.rotation.w);
                var tfs = msg.transforms;
                int i = 0; // Set start point at Shoulder in Base_link
                int limit = 7; // Set end point after wrist 3 child

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
                    if (i >= limit)
                    {
                        break;
                    }
                    //Console.WriteLine(mat);
                }
            }

            // calculate transformations matrices
            //System.Numerics.Matrix4x4 base_to_shoulder = ros_tfs[2];
            //System.Numerics.Matrix4x4 base_to_upperArm = ros_tfs[1] * base_to_shoulder;
            //System.Numerics.Matrix4x4 base_to_forearm = ros_tfs[0] * base_to_upperArm;
            //System.Numerics.Matrix4x4 base_to_wrist1 = ros_tfs[3] * base_to_forearm;
            //System.Numerics.Matrix4x4 base_to_wrist2 = ros_tfs[4] * base_to_wrist1;
            //System.Numerics.Matrix4x4 base_to_wrist3 = ros_tfs[5] * base_to_wrist2;

            base_to_shoulder = ros_tfs[2];
            base_to_upperArm = ros_tfs[1] * base_to_shoulder;
            base_to_forearm = ros_tfs[0] * base_to_upperArm;
            base_to_wrist1 = ros_tfs[3] * base_to_forearm;
            base_to_wrist2 = ros_tfs[4] * base_to_wrist1;
            base_to_wrist3 = ros_tfs[5] * base_to_wrist2;

            //Console.WriteLine("base_to_forearm:");
            //PrintMatrix4x4(base_to_upperArm);
            //System.Numerics.Quaternion quatern = System.Numerics.Quaternion.CreateFromRotationMatrix(base_to_forearm);
            //Console.WriteLine(quatern);
            //Console.WriteLine(base_to_upperArm.Translation);
            //Console.WriteLine();

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

            ///////// 0 ///////
            arr0[0] = base_to_shoulder.M11;
            arr0[1] = base_to_shoulder.M12;
            arr0[2] = base_to_shoulder.M13;
               
            arr0[4] = base_to_shoulder.M21;
            arr0[5] = base_to_shoulder.M22;
            arr0[6] = base_to_shoulder.M23;
               
            arr0[8] = base_to_shoulder.M31;
            arr0[9] = base_to_shoulder.M32;
            arr0[10] = base_to_shoulder.M33;
               
            arr0[12] = base_to_shoulder.M41;
            arr0[13] = base_to_shoulder.M42;
            arr0[14] = base_to_shoulder.M43;
            ///////// 1 ///////
            arr1[0] = base_to_upperArm.M11;
            arr1[1] = base_to_upperArm.M12;
            arr1[2] = base_to_upperArm.M13;
               
            arr1[4] = base_to_upperArm.M21;
            arr1[5] = base_to_upperArm.M22;
            arr1[6] = base_to_upperArm.M23;
               
            arr1[8] = base_to_upperArm.M31;
            arr1[9] = base_to_upperArm.M32;
            arr1[10] = base_to_upperArm.M33;
               
            arr1[12] = base_to_upperArm.M41;
            arr1[13] = base_to_upperArm.M42;
            arr1[14] = base_to_upperArm.M43;
            ///////// 2 ///////
            arr2[0] = base_to_forearm.M11;
            arr2[1] = base_to_forearm.M12;
            arr2[2] = base_to_forearm.M13;
               
            arr2[4] = base_to_forearm.M21;
            arr2[5] = base_to_forearm.M22;
            arr2[6] = base_to_forearm.M23;
               
            arr2[8] = base_to_forearm.M31;
            arr2[9] = base_to_forearm.M32;
            arr2[10] = base_to_forearm.M33;
               
            arr2[12] = base_to_forearm.M41;
            arr2[13] = base_to_forearm.M42;
            arr2[14] = base_to_forearm.M43;
            ///////// 3 ///////
            arr3[0] = base_to_wrist1.M11;
            arr3[1] = base_to_wrist1.M12;
            arr3[2] = base_to_wrist1.M13;
                                   
            arr3[4] = base_to_wrist1.M21;
            arr3[5] = base_to_wrist1.M22;
            arr3[6] = base_to_wrist1.M23;
                                   
            arr3[8] = base_to_wrist1.M31;
            arr3[9] = base_to_wrist1.M32;
            arr3[10] = base_to_wrist1.M33;
                                    
            arr3[12] = base_to_wrist1.M41;
            arr3[13] = base_to_wrist1.M42;
            arr3[14] = base_to_wrist1.M43;
            ///////// 4 ///////
            arr4[0] = base_to_wrist2.M11;
            arr4[1] = base_to_wrist2.M12;
            arr4[2] = base_to_wrist2.M13;
                                   
            arr4[4] = base_to_wrist2.M21;
            arr4[5] = base_to_wrist2.M22;
            arr4[6] = base_to_wrist2.M23;
                                   
            arr4[8] = base_to_wrist2.M31;
            arr4[9] = base_to_wrist2.M32;
            arr4[10] = base_to_wrist2.M33;
                                    
            arr4[12] = base_to_wrist2.M41;
            arr4[13] = base_to_wrist2.M42;
            arr4[14] = base_to_wrist2.M43;
            ///////// 5 ///////
            arr5[0] = base_to_wrist3.M11;
            arr5[1] = base_to_wrist3.M12;
            arr5[2] = base_to_wrist3.M13;

            arr5[4] = base_to_wrist3.M21;
            arr5[5] = base_to_wrist3.M22;
            arr5[6] = base_to_wrist3.M23;

            arr5[8] = base_to_wrist3.M31;
            arr5[9] = base_to_wrist3.M32;
            arr5[10] = base_to_wrist3.M33;

            arr5[12] = base_to_wrist3.M41;
            arr5[13] = base_to_wrist3.M42;
            arr5[14] = base_to_wrist3.M43;



            //PrintMatrix4x4(base_to_wrist3);



            tf_counter++;
            double thresh = 1000;
            if (tf_counter == thresh)
            {
                //Console.WriteLine("Ros code: " + arr5[14]);
                double time = ((DateTime.Now - start_timer_ros_tf).TotalMilliseconds)/ thresh;
                time = time / 1000; // Convert to seconds
                Console.WriteLine("ROS TF FPS: " + Math.Round(1/time,0) );
                tf_counter = 0;
                start_timer_ros_tf = DateTime.Now;
            }

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

            //Console.WriteLine((msg).position[0]);
            //int thresh = 1000;
            //double time_taken = (DateTime.Now - start_timer).TotalMilliseconds;
            //if (counter == thresh)
            //{
            //    double avg = Math.Round(1000 / (time_taken / counter));
            //    Console.WriteLine("Ros messages per second: {0}", avg);
            //    counter = 0;
            //}
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






































///*
//© Siemens AG, 2017-2019
//Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//<http://www.apache.org/licenses/LICENSE-2.0>.
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
//*/
//// Solid edge

//using SolidEdgeCommunity.Extensions; // https://github.com/SolidEdgeCommunity/SolidEdge.Community/wiki/Using-Extension-Methods

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;

//// ROS includes
//using RosSharp.RosBridgeClient;
//using std_msgs = RosSharp.RosBridgeClient.MessageTypes.Std;
//using std_srvs = RosSharp.RosBridgeClient.MessageTypes.Std;
//using rosapi = RosSharp.RosBridgeClient.MessageTypes.Rosapi;
//using sensor_msgs = RosSharp.RosBridgeClient.MessageTypes.Sensor;
//using tf = RosSharp.RosBridgeClient.MessageTypes.Tf2;


//// commands on ROS system:
//// launch before starting:
//// roslaunch rosbridge_server rosbridge_websocket.launch
//// rostopic echo /publication_test
//// rostopic pub /subscription_test std_msgs/String "subscription test message data"

//// launch after starting:
//// rosservice call /service_response_test



//namespace RosSharp.RosBridgeClientTest
//{

//    public class RosSocketConsole
//    {
//        static double[] pose_arr = new double[6];
//        static int counter = 0;
//        static double sequence = -1;
//        static readonly string uri = "ws://192.168.239.132:9090";

//        public static void Tester()
//        {
//            Console.WriteLine("Trying");
//            SolidEdgeFramework.Application application = null;
//            Console.WriteLine("Setting app");
//            try
//            {
//                Console.WriteLine("Registering");

//                // Register with OLE to handle concurrency issues on the current thread.
//                SolidEdgeCommunity.OleMessageFilter.Register();
//                Console.WriteLine("Registered");

//                List<SolidEdgeAssembly.AssemblyDocument> documents = new List<SolidEdgeAssembly.AssemblyDocument>();

//                // Connect to or start Solid Edge.
//                application = SolidEdgeCommunity.SolidEdgeUtils.Connect(true, true);
//                Console.WriteLine("Opening program");
//                // Get a reference to the active assembly document.
//                var document = application.GetActiveDocument<SolidEdgeAssembly.AssemblyDocument>(false);
//                //string f = "C:\\Users\\benwa\\Documents\\1 Projects\\Part Library\\ur3-robotic-manipulator-1.snapshot.5---\\UR3 Assembly2.asm";
//                string f = "C:\\Users\\benwa\\Documents\\1 Projects\\Part Library\\ur_robot\\asm2.asm";
//                var document_by_name = application.Documents.OpenInBackground<SolidEdgeAssembly.AssemblyDocument>(f);
//                //Console.WriteLine("Opening by active: " + document.DisplayName);
//                Console.WriteLine("Opening by filename: " + document_by_name.DisplayName);

//                if (document != null)
//                {
//                    documents.Add(document);
//                }
//                //if (document_by_name != null)
//                //{
//                //    documents.Add(document_by_name);
//                //}

//                //for (int i = 1; i < application.Documents.Count + 1; i ++)
//                //{
//                //    Console.WriteLine(application.Documents.Item(i));
//                //}

//                //var d = document;
//                var d = document_by_name;
//                double[] p = new double[6];
//                for (int i = 0; i < p.Length; i++)
//                {
//                    p[i] = 0;
//                }

//                if (d != null)
//                {
//                    //Console.WriteLine("Hello");
//                    // Strings
//                    string link1_str = "Link1";
//                    string link2_str = "Link2";
//                    string link3_str = "Link3";
//                    string link4_str = "Link4";
//                    string link5_str = "Link5";
//                    string link6_str = "Link6";

//                    List<SolidEdgeAssembly.AngularRelation3d> angularRelations = new List<SolidEdgeAssembly.AngularRelation3d>();
//                    SolidEdgeAssembly.AngularRelation3d angle_1 = null;
//                    SolidEdgeAssembly.AngularRelation3d angle_2 = null;
//                    SolidEdgeAssembly.AngularRelation3d angle_3 = null;
//                    SolidEdgeAssembly.AngularRelation3d angle_4 = null;
//                    SolidEdgeAssembly.AngularRelation3d angle_5 = null;
//                    SolidEdgeAssembly.AngularRelation3d angle_6 = null;
//                    angularRelations.Add(angle_1);
//                    angularRelations.Add(angle_2);
//                    angularRelations.Add(angle_3);
//                    angularRelations.Add(angle_4);
//                    angularRelations.Add(angle_5);
//                    angularRelations.Add(angle_6);
//                    var occurrences_1 = d.Occurrences;
//                    Console.WriteLine("Hello");
//                    Console.WriteLine(occurrences_1.Item(1).Name);
//                    Console.WriteLine(occurrences_1.Item(1).Locatable);
//                    occurrences_1.Item(1).PutTransform(1, 1, 1, 1, 1, 1);
//                    //occurrences_1.Item(1).Move(1, 1, 1);


//                    // Timer
//                    DateTime start = new DateTime();
//                    double time_sum = 0;
//                    int time_counter = 0;
//                    double delay_time = 1;

//                    // Transform specification technique
//                    SolidEdgeAssembly.Occurrence link1_oc = null;
//                    SolidEdgeAssembly.Occurrence link2_oc = null;
//                    SolidEdgeAssembly.Occurrence link3_oc = null;
//                    SolidEdgeAssembly.Occurrence link4_oc = null;
//                    SolidEdgeAssembly.Occurrence link5_oc = null;
//                    SolidEdgeAssembly.Occurrence link6_oc = null;
//                    List<SolidEdgeAssembly.Occurrence> link_list = new List<SolidEdgeAssembly.Occurrence>();



//                    foreach (var occurrence in occurrences_1.OfType<SolidEdgeAssembly.Occurrence>())
//                    {
//                        string name = occurrence.Name;
//                        bool link1 = name.Contains(link1_str);
//                        if (link1)
//                        {
//                            link1_oc = occurrence;
//                            continue;
//                        }
//                        bool link2 = name.Contains(link2_str);
//                        if (link2)
//                        {
//                            link2_oc = occurrence;
//                            continue;
//                        }
//                        bool link3 = name.Contains(link3_str);
//                        if (link3)
//                        {
//                            link3_oc = occurrence;
//                            continue;
//                        }
//                        bool link4 = name.Contains(link4_str);
//                        if (link4)
//                        {
//                            link4_oc = occurrence;
//                            continue;
//                        }
//                        bool link5 = name.Contains(link5_str);
//                        if (link5)
//                        {
//                            link5_oc = occurrence;
//                            continue;
//                        }
//                        bool link6 = name.Contains(link6_str);
//                        if (link6)
//                        {
//                            link6_oc = occurrence;
//                            continue;
//                        }
//                    }

//                    link_list.Add(link1_oc);
//                    link_list.Add(link2_oc);
//                    link_list.Add(link3_oc);
//                    link_list.Add(link4_oc);
//                    link_list.Add(link5_oc);
//                    link_list.Add(link6_oc);

//                    while (true)
//                    {
//                        start = DateTime.Now;

//                        //link1_oc.PutTransform(p[0], p[1], p[2], p[3], p[4], p[5]);
//                        //for (int i = 0; i < p.Length; i++)
//                        //{
//                        //    p[i] += 0.01;
//                        //}
//                        //link2_oc.PutTransform(p[0], p[1], p[2], p[3], p[4], p[5]);
//                        //for (int i = 0; i < p.Length; i++)
//                        //{
//                        //    p[i] += 0.01;
//                        //}
//                        //link3_oc.PutTransform(p[0], p[1], p[2], p[3], p[4], p[5]);
//                        //for (int i = 0; i < p.Length; i++)
//                        //{
//                        //    p[i] += 0.01;
//                        //}
//                        //link4_oc.PutTransform(p[0], p[1], p[2], p[3], p[4], p[5]);
//                        //for (int i = 0; i < p.Length; i++)
//                        //{
//                        //    p[i] += 0.01;
//                        //}
//                        //link5_oc.PutTransform(p[0], p[1], p[2], p[3], p[4], p[5]);
//                        //for (int i = 0; i < p.Length; i++)
//                        //{
//                        //    p[i] += 0.01;
//                        //}
//                        //link6_oc.PutTransform(p[0], p[1], p[2], p[3], p[4], p[5]);
//                        //for (int i = 0; i < p.Length; i++)
//                        //{
//                        //    p[i] += 0.01;
//                        //}
//                        foreach (var occurrence in link_list)
//                        {
//                            //Console.WriteLine("move");
//                            //occurrence.Move(0.01, 0.01, 0.0);
//                            occurrence.PutTransform(p[0], p[1], p[2], p[3], p[4], p[5]);
//                            for (int i = 0; i < p.Length; i++)
//                            {
//                                p[i] += 0.01;
//                            }
//                        }
//                        if (p[0] >= 2)
//                        {
//                            for (int i = 0; i < p.Length; i++)
//                            {
//                                p[i] = 0;
//                            }
//                        }
//                        time_sum += (DateTime.Now - start).TotalMilliseconds + delay_time;
//                        time_counter++;
//                        //Console.WriteLine(time_sum);
//                        //Console.WriteLine(time_counter);
//                        if (time_counter == 10)
//                        {
//                            //Console.WriteLine(time_sum);
//                            double avg = time_sum / time_counter;
//                            Console.WriteLine("\nFPS: {0}\n", Math.Round(1000 / avg));
//                            time_counter = 0;
//                            time_sum = 0;
//                        }
//                        System.Threading.Thread.Sleep((int)delay_time);
//                    }

//                    // Create link references
//                    foreach (var occurrence in occurrences_1.OfType<SolidEdgeAssembly.Occurrence>())
//                    {
//                        if (occurrence.Name.Contains("Link1") || occurrence.Name.Contains("Link3") || occurrence.Name.Contains("Link5"))
//                        {
//                            var relations3d = (SolidEdgeAssembly.Relations3d)occurrence.Relations3d;
//                            var angleRelations = relations3d.OfType<SolidEdgeAssembly.AngularRelation3d>();
//                            foreach (var an in angleRelations)
//                            {
//                                string name = an.Occurrence1.Name;
//                                bool link1 = name.Contains(link1_str);
//                                if (link1)
//                                {
//                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[0]);
//                                    //an.Angle = pose_arr[2] + Math.PI / 2;
//                                    //pose_buffer[0] = pose_arr[2] + Math.PI / 2;
//                                    angularRelations[0] = an;
//                                    angle_1 = an;
//                                    continue;
//                                }
//                                bool link2 = name.Contains(link2_str);
//                                if (link2)
//                                {
//                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[1]);
//                                    angularRelations[1] = an;
//                                    angle_2 = an;
//                                    continue;
//                                }
//                                bool link3 = name.Contains(link3_str);
//                                if (link3)
//                                {
//                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[2]);
//                                    angularRelations[2] = an;
//                                    angle_3 = an;
//                                    continue;
//                                }
//                                bool link4 = name.Contains(link4_str);
//                                if (link4)
//                                {
//                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[3]);
//                                    angularRelations[3] = an;
//                                    angle_4 = an;
//                                    continue;
//                                }
//                                bool link5 = name.Contains(link5_str);
//                                if (link5)
//                                {
//                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[4]);
//                                    angularRelations[4] = an;
//                                    angle_5 = an;
//                                    continue;
//                                }
//                                bool link6 = name.Contains(link6_str);
//                                if (link6)
//                                {
//                                    //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[5]);
//                                    angularRelations[5] = an;
//                                    angle_6 = an;
//                                    continue;
//                                }
//                            }
//                        }
//                    }

//                    // Wait for ROS
//                    while (sequence == -1)
//                    {
//                        int a;
//                    }                    

//                    while (true)
//                    {
//                        start = DateTime.Now;
//                        //angularRelations[0].Angle = pose_arr[2] + Math.PI / 2;
//                        //angularRelations[1].Angle = pose_arr[1] + Math.PI;
//                        //angularRelations[2].Angle = pose_arr[0] + Math.PI / 2;
//                        //angularRelations[3].Angle = pose_arr[3];
//                        //angularRelations[4].Angle = pose_arr[4] - Math.PI / 2;
//                        //angularRelations[5].Angle = pose_arr[5] + Math.PI;
//                        angle_1.Angle += 0.01;
//                        angle_2.Angle += 0.01;
//                        angle_3.Angle += 0.01;
//                        angle_4.Angle += 0.01;
//                        angle_5.Angle = pose_arr[4] - Math.PI / 2;
//                        angle_6.Angle = pose_arr[5] + Math.PI;
//                        //angularRelations[0].Angle += 0.01;
//                        //angularRelations[1].Angle += 0.01;
//                        //angularRelations[2].Angle += 0.01;
//                        //angularRelations[3].Angle += 0.01;
//                        //angularRelations[4].Angle = pose_arr[4] - Math.PI / 2;
//                        //angularRelations[5].Angle = pose_arr[5] + Math.PI;
//                        //Console.WriteLine("Sleep");

//                        //System.Threading.Thread.Sleep(1000);
//                        //Console.WriteLine("wake");
//                        //System.Threading.Thread.Sleep(1000);

//                        time_sum += (DateTime.Now - start).TotalMilliseconds + delay_time;
//                        time_counter++;
//                        //Console.WriteLine(time_sum);
//                        //Console.WriteLine(time_counter);
//                        if (time_counter == 10)
//                        {
//                            double avg = time_sum / time_counter;
//                            Console.WriteLine("\nFPS: {0}\n", Math.Round(1000 / avg));
//                            time_counter = 0;
//                            time_sum = 0;
//                        }
//                        System.Threading.Thread.Sleep((int)delay_time);


//                    }


//                    // Get a reference to the occurrences collection.
//                    while (true)
//                    {
//                        start = DateTime.Now;
//                        //System.Threading.Thread.Sleep(10);
//                        var occurrences = d.Occurrences;
//                        //Console.WriteLine("--------------\n--------------");
//                        double[] pose_buffer = new double[6];

//                        foreach (var occurrence in occurrences.OfType<SolidEdgeAssembly.Occurrence>())
//                        {
//                            if (occurrence.Name.Contains("Link1") || occurrence.Name.Contains("Link3") || occurrence.Name.Contains("Link5"))
//                            {
//                                //Console.WriteLine("Processing occurrence {0}.", occurrence.Name);
//                                var relations3d = (SolidEdgeAssembly.Relations3d)occurrence.Relations3d;
//                                //var axialRelations = relations3d.OfType<SolidEdgeAssembly.AxialRelation3d>();
//                                var angleRelations = relations3d.OfType<SolidEdgeAssembly.AngularRelation3d>();

//                                foreach (var an in angleRelations)
//                                {
//                                    string name = an.Occurrence1.Name;
//                                    //Console.WriteLine(name);

//                                    bool link1 = name.Contains(link1_str);
//                                    if (link1)
//                                    {
//                                        //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[0]);
//                                        an.Angle = pose_arr[2] + Math.PI / 2;
//                                        //pose_buffer[0] = pose_arr[2] + Math.PI / 2;
//                                        continue;
//                                    }
//                                    bool link2 = name.Contains(link2_str);
//                                    if (link2)
//                                    {
//                                        //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[1]);
//                                        an.Angle = pose_arr[1] + Math.PI;
//                                        //pose_buffer[0] = pose_arr[1] + Math.PI;
//                                        continue;
//                                    }
//                                    bool link3 = name.Contains(link3_str);
//                                    if (link3)
//                                    {
//                                        //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[2]);
//                                        an.Angle = pose_arr[0] + Math.PI / 2;
//                                        //pose_buffer[0] = pose_arr[0] + Math.PI / 2;
//                                        continue;
//                                    }
//                                    bool link4 = name.Contains(link4_str);
//                                    if (link4)
//                                    {
//                                        //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[3]);
//                                        an.Angle = pose_arr[3];
//                                        //pose_buffer[0] = pose_arr[3];
//                                        continue;
//                                    }
//                                    bool link5 = name.Contains(link5_str);
//                                    if (link5)
//                                    {
//                                        //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[4]);
//                                        an.Angle = pose_arr[4] - Math.PI / 2;
//                                        //pose_buffer[0] = pose_arr[4] - Math.PI / 2;
//                                        continue;
//                                    }
//                                    bool link6 = name.Contains(link6_str);
//                                    if (link6)
//                                    {
//                                        //Console.WriteLine("Was {0}\nIs{1}", an.Angle, pose_arr[5]);
//                                        an.Angle = pose_arr[5] + Math.PI;
//                                        //pose_buffer[0] = pose_arr[5] + Math.PI;
//                                        continue;
//                                    }

//                                    //Console.WriteLine(link1);

//                                    //DateTime start = DateTime.Now;
//                                    //double t = ((TimeSpan)(DateTime.Now - start)).TotalMilliseconds;

//                                    //for (int i = 0; i < 180; i++)
//                                    //{
//                                    //an.Angle += Math.PI / 180 * i;
//                                    //System.Threading.Thread.Sleep(10);
//                                    //}
//                                }
//                            }

//                        }
//                        time_sum += (DateTime.Now - start).TotalMilliseconds;
//                        time_counter++;
//                        if (time_counter == 100)
//                        {
//                            double avg = time_sum / time_counter;
//                            Console.WriteLine("\nFPS: {0}\n", Math.Round(1000 / avg));
//                            time_counter = 0;
//                            time_sum = 0;
//                        }

//                    }
//                }
//                else
//                {
//                    throw new System.Exception("No active document.");
//                }

//                //while (true)
//                //{
//                //    Console.WriteLine(pose_arr[2]);
//                //}
//            }
//            catch (System.Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//            finally
//            {
//                SolidEdgeCommunity.OleMessageFilter.Unregister();
//            }

//        }


//        public static void Main(string[] args)
//        {
//            //RosSocket rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketSharpProtocol(uri));
//            RosSocket rosSocket = new RosSocket(new RosBridgeClient.Protocols.WebSocketNetProtocol(uri));

//            // Publication:
//            std_msgs.String message = new std_msgs.String
//            {
//                data = "publication test masdasdessage data"
//            };

//            Console.WriteLine("Connected");

//            // Subscribers
//            string pose_array_id = rosSocket.Subscribe<sensor_msgs.JointState>("/joint_states", PoseArrayCallback);

//            // Service Call:
//            rosSocket.CallService<rosapi.GetParamRequest, rosapi.GetParamResponse>("/rosapi/get_param", ServiceCallHandler, new rosapi.GetParamRequest("/rosdistro", "default"));


//            // Service Response:
//            //string service_id = rosSocket.AdvertiseService<std_srvs.TriggerRequest, std_srvs.TriggerResponse>("/service_response_test", ServiceResponseHandler);

//            // Connect to solid edge
//            Thread thread = new Thread(new ThreadStart(Tester));
//            thread.SetApartmentState(ApartmentState.STA);
//            thread.Start();

//            Console.WriteLine("\nPress ENTER to exit the program...");
//            ConsoleKeyInfo keyInfo = Console.ReadKey();
//            while (keyInfo.Key != ConsoleKey.Enter)
//            {
//                keyInfo = Console.ReadKey();
//            }

//            //while (true)
//            //{

//            //}

//            // Disconnect ROS
//            Console.WriteLine("Press any key to close...");
//            Console.ReadKey(true);
//            rosSocket.Unsubscribe(pose_array_id);
//            //rosSocket.UnadvertiseService(service_id);
//            rosSocket.Close();
//            thread.Abort();
//        }
//        private static void SubscriptionHandler(std_msgs.String message)
//        {
//            Console.WriteLine((message).data);
//        }

//        private static void PoseArrayCallback(sensor_msgs.JointState msg)
//        {
//            counter += 1;
//            //Console.WriteLine(counter);

//            if (counter >= 50)
//            {
//                sequence = msg.header.seq;
//                double[] p = msg.position;
//                for (int i = 0; i < p.Length; i++)
//                {
//                    double deg = p[i] * 180 / Math.PI;
//                    deg = Math.Round(deg, 3);
//                    pose_arr[i] = p[i];
//                    //Console.WriteLine("{0}:  {1}", msg.name[i], deg);
//                    //Console.WriteLine((msg).position[i]);
//                }
//                counter = 0;
//            }

//            //Console.WriteLine((msg).position[0]);
//        }
//        private static void ServiceCallHandler(rosapi.GetParamResponse message)
//        {
//            Console.WriteLine("ROS distro: " + message.value);
//        }

//        //private static bool ServiceResponseHandler(std_srvs.TriggerRequest arguments, out std_srvs.TriggerResponse result)
//        //{
//        //    result = new std_srvs.TriggerResponse(true, "service response message");
//        //    return true;
//        //}
//    }
//}
