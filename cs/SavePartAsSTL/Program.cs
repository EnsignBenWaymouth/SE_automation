using SolidEdgeCommunity.Extensions; // https://github.com/SolidEdgeCommunity/SolidEdge.Community/wiki/Using-Extension-Methods
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace SavePartAsSTEP
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            SolidEdgeFramework.Application application = null;
            SolidEdgePart.PartDocument partDocument = null;

            try
            {
                SolidEdgeCommunity.OleMessageFilter.Register();
                application = SolidEdgeCommunity.SolidEdgeUtils.Connect(false);
                partDocument = application.GetActiveDocument<SolidEdgePart.PartDocument>(false);
                
                if (partDocument != null)
                {
                    string name_step = partDocument.Path + "\\" + partDocument.Name.Remove(partDocument.Name.Length - 4, 4) + ".stl";
                    Console.WriteLine(name_step);
                    partDocument.SaveCopyAs(name_step);
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
