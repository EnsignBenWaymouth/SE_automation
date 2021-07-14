/* 
 * This message is auto generated by ROS#. Please DO NOT modify.
 * Note:
 * - Comments from the original code will be written in their own line 
 * - Variable sized arrays will be initialized to array of size 0 
 * Please report any issues at 
 * <https://github.com/siemens/ros-sharp> 
 */

using RosSharp.RosBridgeClient.MessageTypes.Std;
using RosSharp.RosBridgeClient.MessageTypes.Actionlib;

namespace RosSharp.RosBridgeClient.MessageTypes.ObjectRecognition
{
    public class ObjectRecognitionActionResult : ActionResult<ObjectRecognitionResult>
    {
        public const string RosMessageName = "object_recognition_msgs/ObjectRecognitionActionResult";

        public ObjectRecognitionActionResult() : base()
        {
            this.result = new ObjectRecognitionResult();
        }

        public ObjectRecognitionActionResult(Header header, GoalStatus status, ObjectRecognitionResult result) : base(header, status)
        {
            this.result = result;
        }
    }
}
