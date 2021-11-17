* UR Robot Simulator in SolidEdge

The SolidEdge simulation code is stored in the "RosBridgeClientTest" folder. 
This is should be the "startup project" when you open the RosSharp.sln in visual studio.

* Important
There are two text files in the /RosBridgeClientTest/bin/Release directory:

* IP ADDRESS.txt
This is where you input the ip address of the linux VM. You get this by running the command "ifconfig"
inside a terminal in the VM.

* UR ROBOT LOCATIONS.txt
This is where you specify the locations of the UR robots which appear as a selection at the beggining 
of the program.

* Adding a robot to the list
If you add a robot to the list there's a few things you need to consider:

A.	The part files are modified with very specific coordinate system offsets.
B.	The part file are renamed to contain keywords, e.g. "Base", "Link1", "Link2", ... , "Link6"
C.	If you want to have a TOOL move with the end effector of the robot, then in its name it MUST
	contain the following string "EE_tool". You will also need to adjust the base coordinate system
	of the TOOL assembly or part file, this will become obvious when you run the program.