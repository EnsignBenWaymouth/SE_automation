using SolidEdgeCommunity.Extensions; // https://github.com/SolidEdgeCommunity/SolidEdge.Community/wiki/Using-Extension-Methods
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace CustomProperties
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            SolidEdgeFramework.Application application = null;
            SolidEdgeDraft.DraftDocument draftDocument = null;
            SolidEdgeDraft.Sheet sheet = null;
            SolidEdgeDraft.DrawingViews views = null;
            SolidEdgeDraft.ModelMembers modelMembers = null;
            SolidEdgeDraft.GraphicMembers graphicMembers = null;

            bool assembly = false;
            DateTime start_time = DateTime.Now;

            try
            {
                // Register with OLE to handle concurrency issues on the current thread.
                SolidEdgeCommunity.OleMessageFilter.Register();

                // Connect to or start Solid Edge.
                application = SolidEdgeCommunity.SolidEdgeUtils.Connect(true, true);

                // Get a reference to the active assembly document.
                draftDocument = application.GetActiveDocument<SolidEdgeDraft.DraftDocument>(false);

                if (draftDocument != null)
                {
                    sheet = draftDocument.ActiveSheet;
                    views = sheet.DrawingViews;
                    //prefs = draftDocument
                    

                    foreach (var drawingView in views.OfType<SolidEdgeDraft.DrawingView>())
                    {
                        Console.WriteLine();
                        // Get a reference to the ModelMembers collection.
                        modelMembers = drawingView.ModelMembers;

                        foreach (var modelMember in modelMembers.OfType<SolidEdgeDraft.ModelMember>())
                        {
                            modelMember.ShowHiddenEdges = false;
                            var type = modelMember.Type;
                            if (type.ToString() == "seAssemblyMember")
                            {
                                assembly = true;
                            }

                            Console.WriteLine("Hiding hidden edges");
                        }

                        if (assembly)
                        {
                            //graphicMembers = drawingView.GraphicMembers;
                            //RemoveTubeCenterlines(graphicMembers);
                            //Console.WriteLine("Hiding tube Centerlines");
                            assembly = false;
                        }

                        drawingView.ShowEdgesHiddenByOtherParts = false;
                        drawingView.Update();
                    }

                    if (!(views.Count >= 1))
                    {
                        MessageBox.Show("No drawing views found");
                        //System.Threading.Thread.Sleep(5000);
                    }

                    //DateTime finish = DateTime.Now;                    
                    //Double elapsedMillisecs = Math.Round(((TimeSpan)(finish - start_time)).TotalMilliseconds/1000, 2);
                    //MessageBox.Show(elapsedMillisecs.ToString() + "  Seconds");

                    //System.Threading.Thread.Sleep(5000);
                }
                else
                {
                    throw new System.Exception("No active draft document.");
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




        static void RemoveTubeCenterlines(SolidEdgeDraft.GraphicMembers graphicMembers)
        {
            // This is a good example of how to deal with collections that contain different types.
            foreach (var graphicMember in graphicMembers.OfType<object>())
            {
                var graphicMemberType = SolidEdgeCommunity.Runtime.InteropServices.ComObject.GetType(graphicMember);

                if (graphicMemberType.Equals(typeof(SolidEdgeDraft.DVLine2d)))
                {
                    var line2d = graphicMember as SolidEdgeDraft.DVLine2d;
                    Console.WriteLine(line2d.Key);
                    //Console.WriteLine(line2d.ModelMember.ComponentName);
                    if (line2d.ModelMember.ComponentName == "Centerline")
                    {
                        line2d.ShowHideEdgeOverride = SolidEdgeDraft.DVShowHideEdgeOverrideType.DVShowHideEdgeOverrideHide;
                    }
                }
            }
            //Console.WriteLine("\nDone");
            //System.Threading.Thread.Sleep(1000);
            //while (true)
            //{
            //    int a;
            //}
        }
    }
}
