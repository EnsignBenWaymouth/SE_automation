using SolidEdgeCommunity.Extensions; // https://github.com/SolidEdgeCommunity/SolidEdge.Community/wiki/Using-Extension-Methods
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Forms;

namespace SaveSheetMetalDraftAsPDFandDXF //SaveSheetAsEMF
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            SolidEdgeFramework.Application application = null;
            SolidEdgeDraft.DraftDocument draftDocument = null;
            SolidEdgeDraft.Sheet sheet = null;

            try
            {
                // Register with OLE to handle concurrency issues on the current thread.
                SolidEdgeCommunity.OleMessageFilter.Register();

                // Connect to or start Solid Edge.
                application = SolidEdgeCommunity.SolidEdgeUtils.Connect(false);

                // Get a reference to the active draft document.
                draftDocument = application.GetActiveDocument<SolidEdgeDraft.DraftDocument>(false);
                
                if (draftDocument != null)
                {
                    //draftDocument.EditProperties();
                    // Get a reference to the active sheet.
                    sheet = draftDocument.ActiveSheet;

                    SolidEdgeDraft.DrawingView FlatPatternOneToOne;
                    bool found1to1 = false;

                    var size = sheet.SheetSetup;
                    double sheet_height = size.SheetHeight;
                    double sheet_width = size.SheetWidth;
                    Console.WriteLine("sheet size (WxH): {0} x {1}", sheet_width, sheet_height);

                    foreach (SolidEdgeDraft.DrawingView dv in sheet.DrawingViews)
                    {
                        double x, y;
                        dv.GetOrigin(out x, out y);
                        //Console.WriteLine(dv.Name);
                        Console.WriteLine(dv.ViewType);
                        //Console.WriteLine("x: {0}      y:{1}\n", x, y);
                        if(y > sheet_height)
                        {
                            FlatPatternOneToOne = dv;
                            double scale = dv.ScaleFactor;

                            if (scale == 1)
                            {
                                found1to1 = true;                                
                                break;
                            }
                        }
                    }

                    DialogResult result = DialogResult.No;

                    if (!found1to1)
                    {
                        MessageBoxButtons buttons_yesNo = MessageBoxButtons.YesNo;
                        Console.WriteLine("WARNING --- COULD NOT FIND 1:1 SCALE OUTSIDE OF CANVAS");
                        result = MessageBox.Show("WARNING --- COULD NOT FIND 1:1 SCALE OUTSIDE OF CANVAS\nDo you still want to save?", "1:1 drawing not found", buttons_yesNo);
                    }
                    if(found1to1 || result == DialogResult.Yes)
                    {
                        Console.WriteLine("FOUND 1:1");
                        string name_pdf = draftDocument.Path + "\\" + draftDocument.Name.Remove(draftDocument.Name.Length - 4, 4) + ".pdf";
                        string name_dxf = draftDocument.Path + "\\" + draftDocument.Name.Remove(draftDocument.Name.Length - 4, 4) + ".dxf";
                        //Console.WriteLine(name_dxf);
                        //Console.WriteLine(name_pdf);
                        draftDocument.SaveCopyAs(name_pdf);
                        draftDocument.SaveCopyAs(name_dxf);                        
                    }

                    //System.Threading.Thread.Sleep(2000);
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
