using SolidEdgeCommunity.Extensions; // https://github.com/SolidEdgeCommunity/SolidEdge.Community/wiki/Using-Extension-Methods
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace SaveDraftAsSTL
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            SolidEdgeFramework.Application application = null;
            SolidEdgePart.PartDocument partDocument = null;
            SolidEdgeAssembly.AssemblyDocument assDocument = null;
            SolidEdgePart.SheetMetalDocument sheetDocument = null;
            
            try
            {                
                SolidEdgeCommunity.OleMessageFilter.Register();
                application = SolidEdgeCommunity.SolidEdgeUtils.Connect(false);
                partDocument = application.GetActiveDocument<SolidEdgePart.PartDocument>(false);
                assDocument = application.GetActiveDocument<SolidEdgeAssembly.AssemblyDocument>(false);
                sheetDocument = application.GetActiveDocument<SolidEdgePart.SheetMetalDocument>(false);

                if (partDocument != null)
                {
                    string name = partDocument.Path + "\\" + partDocument.Name.Remove(partDocument.Name.Length - 4, 4) + ".stl";
                    Console.WriteLine(name);
                    partDocument.SaveCopyAs(name);
                }
                else if(assDocument != null)
                {
                    string name = assDocument.Path + "\\" + assDocument.Name.Remove(assDocument.Name.Length - 4, 4) + ".stl";
                    Console.WriteLine(name);
                    assDocument.SaveCopyAs(name);
                }
                else if(sheetDocument != null)
                {
                    string name = sheetDocument.Path + "\\" + sheetDocument.Name.Remove(sheetDocument.Name.Length - 4, 4) + ".stl";
                    Console.WriteLine(name);
                    sheetDocument.SaveCopyAs(name);
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
    }
}
