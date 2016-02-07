Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports System.Text.Encoding
Imports Microsoft.VisualBasic.MMFProtocol

''' <summary>
''' Daemon module of the notification services.(这个是主进程)
''' </summary>
Module DaemonProcess

    Dim _continuesThread As Boolean = True
    Dim _notifyOsdController As Microsoft.VisualBasic.CommandLine.Interpreter =
        New CommandLine.Interpreter(GetType(DaemonProcess))
    Dim _osdNotifier As OsdNotifier = New OsdNotifier

    Dim WithEvents NotifyOsdService As Microsoft.VisualBasic.MMFProtocol.MMFSocket

    Public ReadOnly Property Manual As String
        Get
            Return DaemonProcess._notifyOsdController.SDKdocs
        End Get
    End Property

    Public Sub SendMessage(msg As Message)
        Call DaemonProcess._osdNotifier.SendMessage(msg)
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

        Dim Osd_Message As Message =
            New Message With {
                .Title = args("-title"),
                .Message = args("-message"),
                .IconURL = args("-icon")
        }
        Call DaemonProcess._osdNotifier.SendMessage(Osd_Message)

        Return 0
    End Function

    <ExportAPI("-about")>
    Public Function About() As Integer
        Return CLI.About
    End Function

    Private Sub __display(data As Byte())
        Call _notifyOsdController.Execute(Unicode.GetString(data))
    End Sub

    Public Sub Start(ServiceId As String)
        Dim Cycle As Integer = 0

        DaemonProcess.NotifyOsdService = New MMFSocket(ServiceId, AddressOf __display)

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

        Call DaemonProcess.NotifyOsdService.Free
    End Sub
End Module
