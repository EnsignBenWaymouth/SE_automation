﻿using SolidEdgeSpy.Extensions;
using SolidEdgeSpy.Forms;
using SolidEdgeSpy.InteropServices;
using SolidEdgeSpy.Properties;
using SolidEdgeCommunity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Squirrel;
using System.Threading.Tasks;
using System.Configuration;

namespace SolidEdgeSpy
{
    public partial class MainForm : Form, SolidEdgeFramework.ISEApplicationEvents
    {
        private SolidEdgeFramework.Application _application;
        private Dictionary<IConnectionPoint, int> _connectionPoints = new Dictionary<IConnectionPoint, int>();
        private ConcurrentQueue<SolidEdgeSpy.Forms.EventMonitorItem> _eventQueue = new ConcurrentQueue<Forms.EventMonitorItem>();
        private static AutoResetEvent _uiAutoResetEvent = new AutoResetEvent(false);
        private ConnectionPointController _connectionPointController;

        const int TabPageObjectBrowserIndex = 0;
        const int TabPageTypeBrowserIndex = 1;
        const int TabPageCommandBrowserIndex = 2;
        const int TabPageEventMonitorIndex = 3;
        const int TabPageGlobalParametersIndex = 4;
        const int TabPageProcessBrowserIndex = 5;

        public MainForm()
        {
            this.Font = SystemFonts.MessageBoxFont;
            InitializeComponent();
            _connectionPointController = new ConnectionPointController(this);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Register with OLE to handle concurrency issues on the current thread.
                OleMessageFilter.Register();
                
                PreloadTypeLibraries();

                ComTypeManager.Instance.ComTypeLibrarySelected += Instance_ComTypeLibrarySelected;
                ComTypeManager.Instance.ComTypeInfoSelected += Instance_ComTypeInfoSelected;
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }

            SetupToolStripManager();
        }

        private async void MainForm_Shown(object sender, EventArgs e)
        {
#if !DEBUG
            await UpdateApplication(false);
#endif
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                OleMessageFilter.Unregister();
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }
        }

        private void startupTimer_Tick(object sender, EventArgs e)
        {
            startupTimer.Enabled = false;

            if (ConnectToSolidEdge() == false)
            {
                startupTimer.Enabled = true;
            }
        }

        private void eventMonitorTimer_Tick(object sender, EventArgs e)
        {
            List<EventMonitorItem> items = new List<EventMonitorItem>();
            EventMonitorItem item = null;

            while (_eventQueue.TryDequeue(out item))
            {
                items.Add(item);
            }

            eventMonitor.LogEvents(items.ToArray());

            if (_application != null)
            {
                if (commandBrowser.ActiveEnvironment == null)
                {
                    commandBrowser.ActiveEnvironment = _application.GetActiveEnvironment();
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisconnectFromSolidEdge(false);
            Close();
        }

        private void projectWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Resources.ProjectUrl);
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }
        }

        private void solidEdgeCommunityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Resources.GitHubSolidEdgeCommunityUrl);
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }
        }

        private void githubSamplesForSolidEdgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Resources.GitHubSamples);
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }
        }

        private void nugetInteropSolidEdgeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Resources.NuGetInteropSolidEdge);
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }
        }

        private void nugetSolidEdgeCommunityToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Resources.NuGetSolidEdgeCommunity);
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }
        }

        private void nugetSolidEdgeCommunityReaderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Resources.NuGetSolidEdgeCommunityReader);
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }
        }

        private async void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await UpdateApplication(true);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (AboutForm form = new AboutForm())
                {
                    form.ShowDialog(this);
                }
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }
        }

        private void objectBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl.SelectedIndex = TabPageObjectBrowserIndex;
        }

        private void typeBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl.SelectedIndex = TabPageTypeBrowserIndex;
        }

        private void commandBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl.SelectedIndex = TabPageCommandBrowserIndex;
        }

        private void eventMonitorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl.SelectedIndex = TabPageEventMonitorIndex;
        }

        private void globalParametersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl.SelectedIndex = TabPageGlobalParametersIndex;
        }

        void Instance_ComTypeInfoSelected(object sender, ComTypeInfo comTypeInfo)
        {
            tabControl.SelectedIndex = TabPageTypeBrowserIndex;
        }

        void Instance_ComTypeLibrarySelected(object sender, ComTypeLibrary comTypeLibrary)
        {
            tabControl.SelectedIndex = TabPageTypeBrowserIndex;
        }

        private void processBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl.SelectedIndex = TabPageProcessBrowserIndex;
        }

        private void SetupToolStripManager()
        {
            try
            {
                ToolStripManager.RenderMode = ToolStripManagerRenderMode.Professional;

                ToolStripProfessionalRenderer renderer = ToolStripManager.Renderer as ToolStripProfessionalRenderer;
                if (renderer != null)
                {
                    renderer.RoundedEdges = false;
                }
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }
        }

        private bool ConnectToSolidEdge()
        {
            ComPtr pApplication = IntPtr.Zero;

            try
            {
                if (MarshalEx.Succeeded(MarshalEx.GetActiveObject("SolidEdge.Application", out pApplication)))
                {
                    _application = pApplication.TryGetUniqueRCW<SolidEdgeFramework.Application>();
                    _connectionPointController.AdviseSink<SolidEdgeFramework.ISEApplicationEvents>(_application);

                    commandBrowser.ActiveEnvironment = _application.GetActiveEnvironment();
                    globalParameterBrowser.RefreshGlobalParameters();

                    objectBrowser.Connect();

                    // Older versions of Solid Edge don't have the ProcessID property.
                    try
                    {
                        processBrowser.ProcessId = _application.ProcessID;
                    }
                    catch
                    {
                    }

                    return true;
                }
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }
            finally
            {
                pApplication.Dispose();
            }

            return false;
        }

        private void DisconnectFromSolidEdge(bool resetStartupTimer)
        {
            _connectionPointController.UnadviseAllSinks();

            HandleAutoResetEvent();

            globalParameterBrowser.SelectedObject = null;

            try
            {
                Marshal.FinalReleaseComObject(_application);
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }

            startupTimer.Enabled = resetStartupTimer;

            _application = null;
        }

        private void HandleAutoResetEvent()
        {
            objectBrowser.Disconnect();
            globalParameterBrowser.RefreshGlobalParameters();
            _uiAutoResetEvent.Set();
        }

        private void PreloadTypeLibraries()
        {
            try
            {
                Version version = new Version(1, 0);
                ComTypeManager.Instance.LoadRegTypeLib(TypeLibGuid.RevisionManager, version);
                ComTypeManager.Instance.LoadRegTypeLib(TypeLibGuid.SEInstallDataLib, version);
                ComTypeManager.Instance.LoadRegTypeLib(TypeLibGuid.SolidEdgeAssembly, version);
                ComTypeManager.Instance.LoadRegTypeLib(TypeLibGuid.SolidEdgeConstants, version);
                ComTypeManager.Instance.LoadRegTypeLib(TypeLibGuid.SolidEdgeDraft, version);
                ComTypeManager.Instance.LoadRegTypeLib(TypeLibGuid.SolidEdgeFileProperties, version);
                ComTypeManager.Instance.LoadRegTypeLib(TypeLibGuid.SolidEdgeFramework, version);
                ComTypeManager.Instance.LoadRegTypeLib(TypeLibGuid.SolidEdgeFrameworkSupport, version);
                ComTypeManager.Instance.LoadRegTypeLib(TypeLibGuid.SolidEdgeGeometry, version);
                ComTypeManager.Instance.LoadRegTypeLib(TypeLibGuid.SolidEdgePart, version);
                ComTypeManager.Instance.LoadRegTypeLib(TypeLibGuid.StructureEditor, version);
            }
            catch
            {
                GlobalExceptionHandler.HandleException();
            }
        }

        public SolidEdgeFramework.Application Application { get { return _application; } }

#region SolidEdgeFramework.ISEApplicationEvents

        public void AfterActiveDocumentChange(object theDocument)
        {
            string eventString = Resources.AfterActiveDocumentChangeFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theDocument.SafeInvokeGetProperty("Name", "IUnknown"));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));

            this.BeginInvokeIfRequired(frm =>
            {
                frm.HandleAutoResetEvent();
            });

            _uiAutoResetEvent.WaitOne(2000);
            _uiAutoResetEvent.Reset();
        }

        public void AfterCommandRun(int theCommandID)
        {
            string eventString = Resources.AfterCommandRunFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, CommandHelper.ResolveCommandId(_application, theCommandID));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));
        }

        public void AfterDocumentOpen(object theDocument)
        {
            string eventString = Resources.AfterDocumentOpenFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theDocument.SafeInvokeGetProperty("Name", "IUnknown"));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));

            this.BeginInvokeIfRequired(frm =>
            {
                frm.HandleAutoResetEvent();
            });

            _uiAutoResetEvent.WaitOne(2000);
            _uiAutoResetEvent.Reset();
        }

        public void AfterDocumentPrint(object theDocument, int hDC, ref double ModelToDC, ref int Rect)
        {
            string eventString = Resources.AfterDocumentPrintFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theDocument.SafeInvokeGetProperty("Name", "IUnknown"), hDC, ModelToDC, Rect);
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));
        }

        public void AfterDocumentSave(object theDocument)
        {
            string eventString = Resources.AfterDocumentSaveFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theDocument.SafeInvokeGetProperty("Name", "IUnknown"));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));
        }

        public void AfterEnvironmentActivate(object theEnvironment)
        {
            string eventString = Resources.AfterEnvironmentActivateFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;
            SolidEdgeFramework.Environment environment = null;

            try
            {
                environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                commandBrowser.BeginInvokeIfRequired(ctl =>
                {
                    ctl.ActiveEnvironment = environment;
                });
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theEnvironment.SafeInvokeGetProperty("Name", "IUnknown"));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));

            //this.BeginInvokeIfRequired(frm =>
            //{
            //    frm.RefreshGlobalPropertiesPropertyGrid();
            //});
        }

        public void AfterNewDocumentOpen(object theDocument)
        {
            string eventString = Resources.AfterNewDocumentOpenFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theDocument.SafeInvokeGetProperty("Name", "IUnknown"));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));

            this.BeginInvokeIfRequired(frm =>
            {
                frm.HandleAutoResetEvent();
            });

            _uiAutoResetEvent.WaitOne(2000);
            _uiAutoResetEvent.Reset();
        }

        public void AfterNewWindow(object theWindow)
        {
            string eventString = Resources.AfterNewWindowFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theWindow.SafeInvokeGetProperty("Caption", "IUnknown"));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));
        }

        public void AfterWindowActivate(object theWindow)
        {
            string eventString = Resources.AfterWindowActivateFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theWindow.SafeInvokeGetProperty("Caption", "IUnknown"));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));
        }

        public void BeforeCommandRun(int theCommandID)
        {
            string eventString = Resources.BeforeCommandRunFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, CommandHelper.ResolveCommandId(_application, theCommandID));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));
        }

        public void BeforeDocumentClose(object theDocument)
        {
            string eventString = Resources.BeforeDocumentCloseFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theDocument.SafeInvokeGetProperty("Name", "IUnknown"));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));

            try
            {
                SolidEdgeFramework.SolidEdgeDocument document = theDocument as SolidEdgeFramework.SolidEdgeDocument;

                if ((document != null) && (document.IsTemporary() == false))
                {
                    this.BeginInvokeIfRequired(frm =>
                    {
                        frm.HandleAutoResetEvent();
                    });

                    _uiAutoResetEvent.WaitOne(2000);
                    _uiAutoResetEvent.Reset();
                }
            }
            catch
            {
            }
        }

        public void BeforeDocumentPrint(object theDocument, int hDC, ref double ModelToDC, ref int Rect)
        {
            string eventString = Resources.BeforeDocumentPrintFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theDocument.SafeInvokeGetProperty("Name", "IUnknown"), hDC, ModelToDC, Rect);
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));
        }

        public void BeforeDocumentSave(object theDocument)
        {
            string eventString = Resources.BeforeDocumentSaveFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theDocument.SafeInvokeGetProperty("Name", "IUnknown"));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));
        }

        public void BeforeEnvironmentDeactivate(object theEnvironment)
        {
            string eventString = Resources.BeforeEnvironmentDeactivateFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theEnvironment.SafeInvokeGetProperty("Name", "IUnknown"));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));
        }

        public void BeforeQuit()
        {
            string eventString = Resources.BeforeQuitFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));

            this.BeginInvokeIfRequired(frm =>
            {
                frm.DisconnectFromSolidEdge(true);
            });

            _uiAutoResetEvent.WaitOne(2000);
            _uiAutoResetEvent.Reset();
        }

        public void BeforeWindowDeactivate(object theWindow)
        {
            string eventString = Resources.BeforeWindowDeactivateFormat;
            string environmentName = String.Empty;
            string environmentCaption = String.Empty;
            string environmentCATID = String.Empty;

            try
            {
                SolidEdgeFramework.Environment environment = _application.GetActiveEnvironment();
                environment.GetInfo(out environmentName, out environmentCaption, out environmentCATID);
            }
            catch
            {
            }

            try
            {
                eventString = String.Format(eventString, theWindow.SafeInvokeGetProperty("Caption", "IUnknown"));
            }
            catch
            {
            }

            _eventQueue.Enqueue(new Forms.EventMonitorItem(eventString, environmentName, environmentCaption, environmentCATID));
        }

#endregion

        private async Task UpdateApplication(bool showException)
        {
            bool shouldRestart = false;

            var updateUrl = Resources.UpdateUrl;

            try
            {
                updateUrl = ConfigurationManager.AppSettings["UpdateUrl"];
            }
            catch
            {
            }

            if (String.IsNullOrWhiteSpace(updateUrl))
            {
                updateUrl = Resources.UpdateUrl;
            }

            try
            {
                using (var updateManager = new UpdateManager(updateUrl))
                {
                    var updateInfo = await updateManager.CheckForUpdate();

                    if (updateInfo != null)
                    {
                        if (updateInfo.CurrentlyInstalledVersion.Version < updateInfo.FutureReleaseEntry.Version)
                        {
                            string message = $"Version {updateInfo.FutureReleaseEntry.Version} is available. Would you like to apply the update and restart?";

                            if (MessageBox.Show(message, "Update", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                shouldRestart = true;
                                Cursor.Current = Cursors.WaitCursor;
                                Enabled = false;

                                await updateManager.DownloadReleases(updateInfo.ReleasesToApply);
                                await updateManager.ApplyReleases(updateInfo);
                            }
                        }
                    }
                }

                if (shouldRestart)
                {
                    UpdateManager.RestartApp();
                }
            }
            catch (System.Exception ex)
            {
                if (showException)
                {
                    MessageBox.Show(ex.Message, "Updater", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            Cursor.Current = Cursors.Default;
            Enabled = true;
        }
    }
}
