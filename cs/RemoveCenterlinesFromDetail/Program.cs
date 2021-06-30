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
            SolidEdgeDraft.ModelLink link = null;
            SolidEdgeDraft.DrawingViews views = null;
            SolidEdgeDraft.ModelMembers modelMembers = null;

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

                    if (views.Count >= 1)
                    {
                        Console.WriteLine("Hiding hideen edges");
                        Console.WriteLine(views.Count);
                        //views.Item(1).RetrieveCenterLinesCenterMarks(true);
                        var a = views.Item(1);
                        views.Item(1).Defaults_ShowHiddenEdges = false;
                        views.Item(1).ShowEdgesHiddenByOtherParts = false;
                        views.Item(1).Defaults_ShowTubeCenterlines = false;
                        //views.Item(1).DisplayCroppedBoundary = true;
                        a.DisplayBorder = false;
                        Console.WriteLine(a.Name);
                        //views.Item(1).Update();


                        System.Threading.Thread.Sleep(3000);
                        Console.WriteLine("Showing hidden edges");
                        //views.Item(1).Defaults_ShowHiddenEdges = true;
                        //views.Item(1).ShowEdgesHiddenByOtherParts = true;
                    }
                    else
                    {
                        MessageBox.Show("No drawing views found");
                    }
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
    }
}


//namespace CustomProperties
//{
//    class Program
//    {
//        [STAThread]
//        static void Main(string[] args)
//        {
//            SolidEdgeFramework.Application application = null;
//            SolidEdgeDraft.DraftDocument draftDocument = null;
//            SolidEdgeDraft.Sheet sheet = null;
//            SolidEdgeDraft.ModelLink link = null;
//            SolidEdgeDraft.DrawingViews views = null;
//            SolidEdgeDraft.ModelMembers modelMembers = null;

//            try
//            {
//                // Register with OLE to handle concurrency issues on the current thread.
//                SolidEdgeCommunity.OleMessageFilter.Register();

//                // Connect to or start Solid Edge.
//                application = SolidEdgeCommunity.SolidEdgeUtils.Connect(true, true);

//                // Get a reference to the active assembly document.
//                draftDocument = application.GetActiveDocument<SolidEdgeDraft.DraftDocument>(false);
//                string title = null;
//                string doc_number = null;

//                if (draftDocument != null)
//                {
//                    sheet = draftDocument.ActiveSheet;
//                    views = sheet.DrawingViews;
//                    bool only_one_part_present = true;

//                    //foreach (var drawingView in views.OfType<SolidEdgeDraft.DrawingView>())
//                    //{                
//                    //    modelMembers = drawingView.ModelMembers;

//                    //    foreach (var modelMember in modelMembers.OfType<SolidEdgeDraft.ModelMember>())
//                    //    {
//                    //        //modelMember
//                    //        Console.WriteLine("Processing model member '{0}'.", modelMember.FileName);
//                    //        Console.WriteLine("ComponentType: '{0}'.", modelMember.ComponentType);
//                    //        Console.WriteLine("DisplayType: '{0}'.", modelMember.DisplayType);
//                    //        Console.WriteLine("Type: '{0}'.", modelMember.Type);
//                    //    }
//                    //}

//                    for(int i = 1; i < views.Count+1; i++)
//                    {
//                        //SolidEdgeDraft.ModelMembers members = views.Item(1).ModelMembers;
//                        //foreach (var modelMember in members.OfType<SolidEdgeDraft.ModelMembers>())
//                        //{

//                        //}
//                        SolidEdgeDraft.ModelLink temp_link = (SolidEdgeDraft.ModelLink)views.Item(i).ModelLink;
//                        if (i == 1)
//                        {
//                            link = (SolidEdgeDraft.ModelLink)views.Item(i).ModelLink;                            
//                            //Console.WriteLine(link.FileName);
//                            continue;
//                        }

//                        if(temp_link.FileName != link.FileName)
//                        {
//                            only_one_part_present = false;
//                            link = null;
//                            //Console.WriteLine("Blah");
//                            //System.Threading.Thread.Sleep(3000);
//                            break;
//                        }
//                    }

//                    if (link != null)
//                    {
//                        SolidEdgeFramework.PropertySets props = null;

//                        SolidEdgePart.PartDocument part = null;
//                        SolidEdgeAssembly.AssemblyDocument asm = null;
//                        Console.WriteLine(link.FileName);
//                        //System.Threading.Thread.Sleep(3000);
//                        if (link.FileName.Contains(".par"))
//                        {
//                            Console.WriteLine("Part");
//                            part = application.Documents.OpenInBackground<SolidEdgePart.PartDocument>(link.FileName);
//                            //part.Close();
//                            //props = (SolidEdgeFramework.PropertySets)application.Documents.OpenInBackground<SolidEdgePart.PartDocument>(link.FileName).Properties;
//                            //application.Documents.OpenInBackground<SolidEdgePart.PartDocument>(link.FileName).Close();
//                            props = (SolidEdgeFramework.PropertySets)part.Properties;
//                            part.Close();
//                        }
//                        else if (link.FileName.Contains(".asm"))
//                        {
//                            Console.WriteLine("Assembly");
//                            asm = application.Documents.OpenInBackground<SolidEdgeAssembly.AssemblyDocument>(link.FileName);
//                            props = (SolidEdgeFramework.PropertySets)asm.Properties;
//                            asm.Close();
//                        }

//                        title = props.Item(1).Item(1).get_Value().ToString();
//                        doc_number = props.Item(5).Item(1).get_Value().ToString();
//                        //asm.Close();
//                        //part.Close();
//                        //for (int i = 1; i < props.Count + 1; i++)
//                        //{
//                        //    var c = props.Item(i).Count;
//                        //    if (c > 0)
//                        //    {
//                        //        for (int j = 1; j < c + 1; j++)
//                        //        {
//                        //            if (props.Item(i).Item(j).Name == "Title")
//                        //            {
//                        //                Console.WriteLine(i + ", " + j);
//                        //                title = props.Item(i).Item(j).get_Value().ToString();
//                        //            }
//                        //            if (props.Item(i).Item(j).Name == "Document Number")
//                        //            {
//                        //                Console.WriteLine(i + ", " + j);
//                        //                doc_number = props.Item(i).Item(j).get_Value().ToString();
//                        //            }
//                        //        }
//                        //    }
//                        //}
//                        Console.WriteLine("Title: {0}\nDocument Number: {1}", title, doc_number);

//                        // Find and assign new title and doc.No to draft
//                        var draft_props = (SolidEdgeFramework.PropertySets)draftDocument.Properties;
//                        draft_props.Item(1).Item(1).set_Value(title);
//                        draft_props.Item(4).Item(1).set_Value(doc_number);

//                        //for (int i = 1; i < draft_props.Count + 1; i++)
//                        //{
//                        //    var c = draft_props.Item(i).Count;
//                        //    if (c > 0)
//                        //    {
//                        //        for (int j = 1; j < c + 1; j++)
//                        //        {
//                        //            if (draft_props.Item(i).Item(j).Name == "Title")
//                        //            {
//                        //                Console.WriteLine(i + ", " + j);
//                        //                draft_props.Item(i).Item(j).set_Value(title);
//                        //            }
//                        //            if (draft_props.Item(i).Item(j).Name == "Document Number")
//                        //            {
//                        //                Console.WriteLine(i + ", " + j);
//                        //                draft_props.Item(i).Item(j).set_Value(doc_number);
//                        //            }
//                        //        }
//                        //    }
//                        //}
//                        //System.Threading.Thread.Sleep(3000);
//                        draftDocument.Save();
//                        //System.Threading.Thread.Sleep(1000);

//                    }
//                    else if (!only_one_part_present)
//                    {
//                        //var form = new Form();
//                        //var controls = new Control();
//                        //var comboBox1 = new ComboBox();
//                        //comboBox1.Location = new System.Drawing.Point(20, 60);
//                        //comboBox1.Name = "comboBox1";
//                        //comboBox1.Size = new System.Drawing.Size(245, 25);
//                        //comboBox1.BackColor = System.Drawing.Color.Orange;
//                        //comboBox1.ForeColor = System.Drawing.Color.Black;
//                        //string[] installs = new string[]{"a", "b", "c"};
//                        //comboBox1.Items.AddRange(installs);
//                        //form.Controls.Add(comboBox1);
//                        //form.Show();
//                        //MessageBox.Show(comboBox1.Text);
//                        //System.Threading.Thread.Sleep(3000);

//                        Console.WriteLine("Warning --- Multiple Parts present in draft");
//                        MessageBox.Show("Warning --- Multiple Parts present in draft");
//                        //System.Threading.Thread.Sleep(3000);
//                    }
//                    else
//                    {
//                        Console.WriteLine("Error --- Unknown");
//                        MessageBox.Show("Error --- Unknown");
//                        //System.Threading.Thread.Sleep(3000);
//                    }
//                }
//                else
//                {
//                    throw new System.Exception("No active draft document.");
//                }
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
//    }
//}
