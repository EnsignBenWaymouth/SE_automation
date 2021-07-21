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
        static readonly string uri = "ws://172.16.133.130:9090";
        static DateTime start_timer = new DateTime();
        static double[] p1 = new double[] { 0, 0, 0, 0, 0, 0 };
        static double[] p2 = new double[] { 0, 0, 0, 0, 0, 0 };
        static double[] p3 = new double[] { 0, 0, 0, 0, 0, 0 };
        static double[] p4 = new double[] { 0, 0, 0, 0, 0, 0 };
        static double[] p5 = new double[] { 0, 0, 0, 0, 0, 0 };
        static double[] p6 = new double[] { 0, 0, 0, 0, 0, 0 };
        static List<double[]> ros_transforms = new List<double[]>();

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
                var document = application.GetActiveDocument<SolidEdgeAssembly.AssemblyDocument>(false);
                Console.WriteLine(document.FullName);
                //string f = "C:\\Users\\benjaminw\\Documents\\UR3e_test.asm";
                string f = "C:\\Users\\benjaminw\\Documents\\UR3e_UET Cartoner_Vacuum_6_Pocket_ros.asm";
                //string f = "\\elsedge\\engineering\\Drawings\\Filling Hall 1\\BF03\\UET Cartoner Automation Ass'y\\UR3e_UET Cartoner_Vacuum_6_Pocket.asm";
                var document_by_name = application.Documents.OpenInBackground<SolidEdgeAssembly.AssemblyDocument>(f);
                //Console.WriteLine("Opening by active: " + document.DisplayName);
                Console.WriteLine("Opening by filename: " + document_by_name.DisplayName);

                if (document != null)
                {
                    documents.Add(document);
                }
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
                        int a;
                    }

                    // Timer
                    DateTime start = new DateTime();
                    double time_sum = 0;
                    int time_counter = 0;

                    // Continuously update joints
                    while (true)
                    {
                        start = DateTime.Now;
                        angularRelations[0].Angle = pose_arr[2] + Math.PI / 2;
                        angularRelations[1].Angle = pose_arr[1] + Math.PI;
                        angularRelations[2].Angle = -pose_arr[0] - Math.PI / 2;
                        angularRelations[3].Angle = pose_arr[3];
                        angularRelations[4].Angle = pose_arr[4] - Math.PI / 2;
                        angularRelations[5].Angle = pose_arr[5] + Math.PI;

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
                        if (avg < 50)
                        {
                            Console.WriteLine("Refreshing");
                            application.StartCommand(SolidEdgeConstants.AssemblyCommandConstants.AssemblyViewRefreshWindow);
                        }
                    }
                    double[] p = new double[] { 0,0,0,0,0,0};
                    while (true)
                    {
                        start = DateTime.Now;
                        
                        foreach (var occurance in link_list)
                        {
                            //occurance.PutTransform(p[0], p[1], p[2], p[3], p[4], p[5]);
                            for (int i = 0; i < p.Length; i++)
                            {
                                p[i] += 0.01;
                            }
                        }
                        if (p[0] >= 2)
                        {
                            for (int i = 0; i < p.Length; i++)
                            {
                                p[i] = 0;
                            }
                        }

                        time_sum += (DateTime.Now - start).TotalMilliseconds;
                        time_counter++;
                        double avg = 1000;
                        if (time_counter == 20)
                        {
                            avg = Math.Round(1000 / (time_sum / time_counter));
                            //Console.WriteLine("\nFPS: {0}\n", avg);
                            time_counter = 0;
                            time_sum = 0;
                        }
                        if(avg < 50)
                        {
                            //Console.WriteLine("Refreshing");
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

            // Subscribers
            string pose_array_id = rosSocket.Subscribe<sensor_msgs.JointState>("/joint_states", PoseArrayCallback);
            //string tf_id = rosSocket.Subscribe<tf2.TFMessage>("/tf", tfCallback);
            //string geometry_id = rosSocket.Subscribe<geometry.Transform>("")

            // Service Call:
            rosSocket.CallService<rosapi.GetParamRequest, rosapi.GetParamResponse>("/rosapi/get_param", ServiceCallHandler, new rosapi.GetParamRequest("/rosdistro", "default"));

            // Service Response:
            string service_id = rosSocket.AdvertiseService<std_srvs.TriggerRequest, std_srvs.TriggerResponse>("/service_response_test", ServiceResponseHandler);

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
            rosSocket.UnadvertiseService(service_id);
            rosSocket.Close();
            thread.Abort();
        }
        private static void SubscriptionHandler(std_msgs.String message)
        {
            Console.WriteLine((message).data);
        }

        private static void tfCallback(tf2.TFMessage msg)
        {
            Console.WriteLine(msg.transforms[0].transform.rotation.w);
            var tfs = msg.transforms;
            int i = 0;
            foreach (var t in tfs)
            {
                ros_transforms[i][0] = t.transform.translation.x;
                ros_transforms[i][1] = t.transform.translation.y;
                ros_transforms[i][2] = t.transform.translation.z;
                ros_transforms[i][3] = t.transform.rotation.x;
                ros_transforms[i][4] = t.transform.rotation.y;
                ros_transforms[i][5] = t.transform.rotation.z;
                i++;
            }
            
        }

        private static void PoseArrayCallback(sensor_msgs.JointState msg)
        {
            if(counter == 0)
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

        private static bool ServiceResponseHandler(std_srvs.TriggerRequest arguments, out std_srvs.TriggerResponse result)
        {
            result = new std_srvs.TriggerResponse(true, "service response message");
            return true;
        }
    }
}