using SolidEdgeCommunity.Extensions; // https://github.com/SolidEdgeCommunity/SolidEdge.Community/wiki/Using-Extension-Methods
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreatePolygon
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            SolidEdgeFramework.Application application = null;
            SolidEdgeFramework.Documents documents = null;
            SolidEdgePart.PartDocument partDocument = null;
            SolidEdgePart.RefPlanes refPlanes = null;
            SolidEdgePart.RefPlane refPlane = null;
            SolidEdgePart.ProfileSets profileSets = null;
            SolidEdgePart.ProfileSet profileSet = null;
            SolidEdgePart.Profiles profiles = null;
            SolidEdgePart.Profile profile = null;
            SolidEdgeFrameworkSupport.Lines2d lines2d = null;
            SolidEdgeFrameworkSupport.Relations2d relations2d = null;
            SolidEdgeFramework.SelectSet selectSet = null;

            try
            {
                // Register with OLE to handle concurrency issues on the current thread.
                SolidEdgeCommunity.OleMessageFilter.Register();

                // Connect to or start Solid Edge.
                application = SolidEdgeCommunity.SolidEdgeUtils.Connect(true, true);

                // Get a reference to the documents collection.
                documents = application.Documents;

                // Create a new part document.
                partDocument = documents.AddPartDocument();

                // Always a good idea to give SE a chance to breathe.
                application.DoIdle();

                // Get a reference to the RefPlanes collection.
                refPlanes = partDocument.RefPlanes;

                // Get a reference to the top RefPlane using extension method.
                refPlane = refPlanes.GetTopPlane();

                // Get a reference to the ProfileSets collection.
                profileSets = partDocument.ProfileSets;

                // Add a new ProfileSet.
                profileSet = profileSets.Add();

                // Get a reference to the Profiles collection.
                profiles = profileSet.Profiles;

                // Add a new Profile.
                profile = profiles.Add(refPlane);

                // Get a reference to the Relations2d collection.
                relations2d = (SolidEdgeFrameworkSupport.Relations2d)profile.Relations2d;

                // Get a reference to the Lines2d collection.
                lines2d = profile.Lines2d;

                int sides = 8;
                double angle = 360 / sides;
                angle = (angle * Math.PI) / 180;

                double radius = .05;
                double lineLength = 2 * radius * (Math.Tan(angle) / 2);

                // x1, y1, x2, y2
                double[] points = { 0.0, 0.0, 0.0, 0.0 };

                double x = 0.0;
                double y = 0.0;

                points[2] = -((Math.Cos(angle / 2) * radius) - x);
                points[3] = -((lineLength / 2) - y);

                // Draw each line.
                for (int i = 0; i < sides; i++)
                {
                    points[0] = points[2];
                    points[1] = points[3];
                    points[2] = points[0] + (Math.Sin(angle * i) * lineLength);
                    points[3] = points[1] + (Math.Cos(angle * i) * lineLength);

                    lines2d.AddBy2Points(points[0], points[1], points[2], points[3]);
                }

                // Create endpoint relationships.
                for (int i = 1; i <= lines2d.Count; i++)
                {
                    if (i == lines2d.Count)
                    {
                        relations2d.AddKeypoint(lines2d.Item(i), (int)SolidEdgeConstants.KeypointIndexConstants.igLineEnd, lines2d.Item(1), (int)SolidEdgeConstants.KeypointIndexConstants.igLineStart);
                    }
                    else
                    {
                        relations2d.AddKeypoint(lines2d.Item(i), (int)SolidEdgeConstants.KeypointIndexConstants.igLineEnd, lines2d.Item(i + 1), (int)SolidEdgeConstants.KeypointIndexConstants.igLineStart);
                        relations2d.AddEqual(lines2d.Item(i), lines2d.Item(i + 1));
                    }
                }

                // Get a reference to the ActiveSelectSet.
                selectSet = application.ActiveSelectSet;

                // Empty ActiveSelectSet.
                selectSet.RemoveAll();

                // Add all lines to ActiveSelectSet.
                for (int i = 1; i <= lines2d.Count; i++)
                {
                    selectSet.Add(lines2d.Item(i));
                }

                // Switch to ISO view.
                application.StartCommand(SolidEdgeConstants.PartCommandConstants.PartViewISOView);
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
    }
}
