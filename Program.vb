Imports System.Text.Encoding
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft
Imports Microsoft.VisualBasic.Parallel

''' <summary>
''' 假若需要进行编程处理的话，从本模块的<see cref="Run(String)"/>方法启动线程
''' </summary>
Public Module Program

    ''' <summary>
    ''' 从这里开始运行主进程
    ''' </summary>
    ''' <returns></returns>
    Public Function Main() As Integer
        Return GetType(Program).RunCLI(App.CommandLine)
    End Function

    ''' <summary>
    ''' 线程的内存映射文件名称
    ''' </summary>
    ''' <param name="uri"></param>
    Public Sub Run(uri As String)
        Call Microsoft.VisualBasic.Parallel.Run(Sub() Call Program.StartServices($"-start {uri}"))
    End Sub

    Private ProcessLock As Microsoft.VisualBasic.MMFProtocol.ProcessLock

    <ExportAPI("-start", Usage:="-start service_id_url")>
    Public Function StartServices(argvs As CommandLine.CommandLine) As Integer
        Dim ServiceName As String = argvs.Parameters?.First
        Program.ProcessLock = New MMFProtocol.ProcessLock(ServiceName & "-process_lock")
        If Not ProcessLock.Locked Then
            Call NotifyOsdDaemonProcess.Start(ServiceName)
        End If

        Return 0
    End Function

    <ExportAPI("-stop", Usage:="-stop service_id_url")>
    Public Function StopServices(argvs As CommandLine.CommandLine) As Integer
        Dim ServiceName As String = argvs.Parameters?.First
        Using Client As Microsoft.VisualBasic.MMFProtocol.MMFSocket = New MMFProtocol.MMFSocket(ServiceName)
            Call Client.SendMessage("-stop")
        End Using

        Return 0
    End Function

    <ExportAPI("-sendmessage", Usage:="-sendmessage -service <service_id_url> -message <messages>")>
    Public Function SendMessage(argvs As CommandLine.CommandLine) As Integer
        Dim ServiceName As String = argvs("-service")
        Dim s_Messages As String = argvs("-message")
        Using Client As Microsoft.VisualBasic.MMFProtocol.MMFSocket = New MMFProtocol.MMFSocket(ServiceName)
            Call Client.SendMessage(s_Messages)
        End Using

        Return 0
    End Function

    <ExportAPI("-about")>
    Public Function About() As Integer
        Dim AboutMsg As NotifyOsd.OsdNotifier.Message = New NotifyOsd.OsdNotifier.Message With {
            .Title = "Notify-osd service",
            .Message = $"Design by: xieguigang(xie.guigang@gmail.com)\nThis is a notify on screen dispalying service works like the 'notify-osd' on ubuntu linux.\n\n\n{NotifyOsdDaemonProcess.Manual}"
        }
        Call NotifyOsdDaemonProcess.SendMessage(AboutMsg)

        Return 0
    End Function
End Module

''' <summary>
''' 这个是主进程
''' </summary>
Module NotifyOsdDaemonProcess

    Dim _continuesThread As Boolean = True
    Dim _notifyOsdController As Microsoft.VisualBasic.CommandLine.Interpreter =
        New CommandLine.Interpreter(GetType(NotifyOsdDaemonProcess))
    Dim _osdNotifier As OsdNotifier = New OsdNotifier

    Dim WithEvents NotifyOsdService As Microsoft.VisualBasic.MMFProtocol.MMFSocket

    Public ReadOnly Property Manual As String
        Get
            Return NotifyOsdDaemonProcess._notifyOsdController.SDKdocs
        End Get
    End Property

    Public Sub SendMessage(msg As NotifyOsd.OsdNotifier.Message)
        Call NotifyOsdDaemonProcess._osdNotifier.SendMessage(msg)
    End Sub

    <ExportAPI("-Stop")>
    Public Function ExitThread() As Integer
        _continuesThread = False
        Return 0
    End Function

    <ExportAPI("-SendMessage", Usage:="-SendMessage -title <title> -message <message> -icon <icon_url>")>
    Public Function SendMessage(args As CommandLine.CommandLine) As Integer
        If args Is Nothing Then
            Return -1
        Else
            Call $"New message ""{args.CLICommandArgvs}""".__DEBUG_ECHO
        End If

        Dim Osd_Message As NotifyOsd.OsdNotifier.Message =
            New NotifyOsd.OsdNotifier.Message With {
                .Title = args("-title"),
                .Message = args("-message"),
                .IconURL = args("-icon")
        }
        Call NotifyOsdDaemonProcess._osdNotifier.SendMessage(Osd_Message)

        Return 0
    End Function

    <ExportAPI("-about")>
    Public Function About() As Integer
        Return Program.About
    End Function

    Public Sub Start(ServiceId As String)
        Dim Cycle As Integer = 0

        NotifyOsdDaemonProcess.NotifyOsdService =
            New Microsoft.VisualBasic.MMFProtocol.MMFSocket(
                ServiceId,
                Sub(data As Byte()) Call _notifyOsdController.Execute(Unicode.GetString(data)))

        Call $"Notify-OSD services start!".__DEBUG_ECHO

        Do While _continuesThread

            Call Threading.Thread.Sleep(100)

            If Cycle > 10 * 60 * 2 Then
                Cycle = 0
                Call FlushMemory()
            Else
                Cycle += 1
            End If
        Loop

        Call NotifyOsdDaemonProcess.NotifyOsdService.Free
    End Sub
End Module
